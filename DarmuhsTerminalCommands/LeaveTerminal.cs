using BepInEx;
using BepInEx.Bootstrap;
using FovAdjust;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static TerminalApi.TerminalApi;
using static TerminalStuff.AllMyTerminalPatches;
using static UnityEngine.EventSystems.EventTrigger;

namespace TerminalStuff
{
    public static class LeaveTerminal
    {
        public static string TotalValueFormat = "";
        public static string VideoErrorMessage = "";
        public static bool vitalsUpgradeEnabled = false;
        public static bool enemyScanUpgradeEnabled = false;
        private static Dictionary<string, PluginInfo> PluginsLoaded = new Dictionary<string, PluginInfo>();
        private static bool enabledSplitObjects = false;

        [HarmonyPatch(typeof(Terminal))]
        [HarmonyPatch("RunTerminalEvents")]
        public class Terminal_RunTerminalEvents_Patch : MonoBehaviour
        {
            private static VideoController videoController;

            public static void AddDuplicateRenderObjects() //used by overlay and proview
            {
                Plugin.instance.terminalCanvas = GameObject.Find("Environment/HangarShip/Terminal/Canvas").GetComponent<Canvas>();

                if (Plugin.instance.terminalCanvas != null)
                {
                    Plugin.Log.LogInfo("Canvas found");


                    RawImage originalRawImage = Plugin.instance.terminalCanvas.transform.Find("MainContainer/ImageContainer/Image (1)").GetComponent<RawImage>();


                    if (originalRawImage != null && Plugin.instance.splitViewCreated == false)
                    {
                        Plugin.Log.LogInfo("Original RawImage found");

                        // Duplicate the RawImage
                        GameObject rawImageGameObject2 = Instantiate(originalRawImage.gameObject, originalRawImage.transform.parent);
                        GameObject rawImageGameObject1 = Instantiate(originalRawImage.gameObject, originalRawImage.transform.parent);
                        rawImageGameObject1.name = "Terminal Cams (Clone)";
                        rawImageGameObject2.name = "Terminal Radar (Clone)";

                        // Get the RawImage components from the duplicated GameObjects
                        Plugin.instance.rawImage2 = rawImageGameObject2.GetComponent<RawImage>();
                        Plugin.instance.rawImage1 = rawImageGameObject1.GetComponent<RawImage>();

                        // Store the original dimensions and anchored positions
                        Plugin.instance.originalTopSize = Plugin.instance.rawImage1.rectTransform.sizeDelta;
                        Plugin.instance.originalTopPosition = Plugin.instance.rawImage1.rectTransform.anchoredPosition;

                        Plugin.instance.originalBottomSize = Plugin.instance.rawImage2.rectTransform.sizeDelta;
                        Plugin.instance.originalBottomPosition = Plugin.instance.rawImage2.rectTransform.anchoredPosition;

                        Plugin.instance.splitViewCreated = true;
                    }
                }
            }

            static IEnumerator PostfixCoroutine(Terminal __instance, TerminalNode node)
            {
                if (!string.IsNullOrWhiteSpace(node.terminalEvent))
                {
                    if (node.terminalEvent == "quit")
                    {

                        string text = $"{ConfigSettings.quitString.Value}\n";
                        node.displayText = text;

                        Debug.Log("Start of the coroutine");

                        // Delay for 1 second
                        yield return new WaitForSeconds(1);

                        Debug.Log("After 1 second");

                        // Now, call QuitTerminal on the original instance
                        __instance.QuitTerminal();
                    }

                    if (node.terminalEvent == "leverdo")
                    {
                        NetworkManager networkManager = __instance.NetworkManager;
                        if (!GameNetworkManager.Instance.gameHasStarted && ((object)networkManager != null && networkManager.IsHost))
                        {
                            node.displayText = $"{ConfigSettings.leverString.Value}\n";

                            // Assuming there is an existing instance of StartMatchLever in your scene
                            StartMatchLever leverInstance = FindObjectOfType<StartMatchLever>();

                            if (leverInstance != null)
                            {
                                leverInstance.LeverAnimation();
                                // Delay for 1 second
                                yield return new WaitForSeconds(1);
                                leverInstance.PullLever();
                                yield return new WaitForSeconds(1);
                                Plugin.Log.LogInfo("lever pulled");
                            }
                            else
                            {
                                Debug.LogError("StartMatchLever instance not found!");
                            }
                        }
                        else if (GameNetworkManager.Instance.gameHasStarted)
                        {
                            node.displayText = $"{ConfigSettings.leverString.Value}\n";

                            // Delay for 1 second
                            yield return new WaitForSeconds(1);

                            // Assuming there is an existing instance of StartMatchLever in your scene
                            StartMatchLever leverInstance = FindObjectOfType<StartMatchLever>();

                            if (leverInstance != null)
                            {
                                leverInstance.LeverAnimation();
                                // Delay for 1 second
                                yield return new WaitForSeconds(1);
                                leverInstance.PullLever();
                            }
                            else
                            {
                                Debug.LogError("StartMatchLever instance not found!");
                            }
                        }
                        else
                        {
                            node.displayText = "Cannot pull the lever at this time.\n\nIf game has not been started, only the host can do this.\n";
                        }
                    }
                        //end of lever event    

                    if (node.terminalEvent == "kickYes") //work dammit
                    {
                        Plugin.Log.LogInfo("We made it to the terminalEvent!!");
                        int playernum = Terminal_ParsePlayerSentence_Patch.playerObjIdForTerminal;
                        Plugin.Log.LogInfo("playerObjIdForTerminal = " + playernum.ToString());
                        node.displayText = $"{ConfigSettings.kickString.Value}\n";
                        // Delay for 1 second
                        yield return new WaitForSeconds(1);
                        Plugin.Log.LogInfo("Wait 1");
                        __instance.QuitTerminal();
                        yield return new WaitForSeconds(1);
                        Plugin.Log.LogInfo("Wait 2");
                        StartOfRound.Instance.KickPlayer(Terminal_ParsePlayerSentence_Patch.playerObjIdForTerminal);
                        Plugin.Log.LogInfo("kicked");
                    }
                    if (node.terminalEvent == "kickNo")
                        node.displayText = $"{ConfigSettings.kickNoString.Value}\n";
                    if (node.terminalEvent == "NotHost")
                        node.displayText = $"{ConfigSettings.kickNotHostString.Value}\n";
                    if (node.terminalEvent == "modlist")
                    {
                        PluginsLoaded = Chainloader.PluginInfos;
                        string concatenatedString = string.Join("\n",
                        PluginsLoaded.Select(kvp =>
                        $"{kvp.Value.Metadata.Name}, Version: {kvp.Value.Metadata.Version}"));
                        node.displayText = $"Mod List:\n\n{concatenatedString}\n";
                    }
                    if (node.terminalEvent == "lolevent")
                    {

                        if (videoController == null)
                        {
                            videoController = new VideoController();

                            // Get the shared render texture from the existing VideoPlayer
                            //RenderTexture sharedRenderTexture = __instance.videoPlayer.targetTexture;

                            // Call the Initialize method with the desired GameObject and shared render texture
                            GameObject terminalImage = GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer/ImageContainer/Image (1)");
                            videoController.Initialize(terminalImage);
                        }

                        // Check if the video is currently playing
                        if (VideoController.isVideoPlaying)
                        {
                            // Stop the video if it's playing
                            videoController.StopAdditionalVideo();
                            VideoController.isVideoPlaying = false;
                            node.clearPreviousText = true;
                            node.displayText = $"{ConfigSettings.lolStopString.Value}\n";
                        }
                        else
                        {
                            // Play the next video if not playing
                            videoController.PlayNextVideo();
                            VideoController.isVideoPlaying = true;
                            node.clearPreviousText = true;
                            node.displayText = $"{ConfigSettings.lolStartString.Value}\n";
                        }
                    }

                    if (node.terminalEvent == "loot")
                    {
                        Plugin.Log.LogInfo("attempting to run getloot");
                        shiploot.getLoot();
                        Plugin.Log.LogInfo("getloot ran");
                        node.displayText = $"{LeaveTerminal.TotalValueFormat}\n";
                        Plugin.Log.LogInfo("Display should read" + LeaveTerminal.TotalValueFormat);
                    }

                    if (node.terminalEvent == "door")
                    {

                        if (!StartOfRound.Instance.inShipPhase)
                        {
                            HangarShipDoor door = UnityEngine.Object.FindObjectOfType<HangarShipDoor>();
                            if (!StartOfRound.Instance.hangarDoorsClosed && door.overheated == false)
                            {
                                node.displayText = $"{ConfigSettings.doorCloseString.Value}\n";
                                ((UnityEvent<PlayerControllerB>)(object)door.triggerScript.onInteract).Invoke(GameNetworkManager.Instance.localPlayerController); //creds to NavarroTech, used their code for reference here
                            }
                            else
                            {
                                float pwrVal = door.doorPower;
                                door.doorPower = 0f;
                                node.displayText = $"{ConfigSettings.doorOpenString.Value}\n";
                                yield return 0.1;
                                door.doorPower = pwrVal;
                            }
                                
                        }
                        else
                        {
                            node.displayText = $"{ConfigSettings.doorSpaceString.Value}\n";
                        }
                    }

                    if (node.terminalEvent == "enemies")
                    {
                        if (RoundManager.Instance != null)
                        {
                            Plugin.Log.LogInfo("getting enemies count");
                            int scannedEnemies = RoundManager.Instance.SpawnedEnemies.Count;
                            //Plugin.Log.LogInfo("int scannedEnemies working");
                            int getCreds = __instance.groupCredits;
                            //Plugin.Log.LogInfo("getcreds");
                            int costCreds = ConfigSettings.enemyScanCost.Value; //need new config value
                            //Plugin.Log.LogInfo("cost config value");
                            string badNum = scannedEnemies.ToString();
                            //Plugin.Log.LogInfo("scanned enemies added to string");

                            if (enemyScanUpgradeEnabled == true && (getCreds >= costCreds))
                            {
                                int newCreds = getCreds - costCreds;
                                __instance.SyncGroupCreditsClientRpc(newCreds, __instance.numberOfItemsInDropship);  //localhost
                                __instance.SyncGroupCreditsServerRpc(newCreds, __instance.numberOfItemsInDropship);  //server

                                // Convert the array to a List for sorting
                                List<EnemyAI> enemiesList = RoundManager.Instance.SpawnedEnemies.ToList();
                                
                                // Filter the list based on the 'isEnemyDead' property
                                var livingEnemies = enemiesList.Where(enemy => !enemy.isEnemyDead);
                                
                                // Create a string representation of each living enemy
                                string livingEnemiesString = string.Join(Environment.NewLine, livingEnemies.Select(enemy => enemy.ToString()));
                                string pattern = @"\([^)]*\)";

                                // Apply the regular expression to remove text within parentheses
                                string filteredLivingEnemiesString = Regex.Replace(livingEnemiesString, pattern, string.Empty);
                                
                                node.displayText = $"Biomatter scanner charged {costCreds} credits and has detected [{badNum}] non-employee organic objects.\r\n\r\nYour new balance is ■{newCreds} Credits.\r\n\r\nDetailed scan has defined these objects as the following in the registry: \r\n{filteredLivingEnemiesString}\r\n";
                                Plugin.Log.LogInfo($"Living Enemies(filtered): {filteredLivingEnemiesString}");
                            }
                            else if (getCreds >= costCreds)
                            {
                                int newCreds = getCreds - costCreds;
                                __instance.SyncGroupCreditsClientRpc(newCreds, __instance.numberOfItemsInDropship);  //localhost
                                __instance.SyncGroupCreditsServerRpc(newCreds, __instance.numberOfItemsInDropship);  //server
                                node.displayText = $"Biomatter scanner charged {costCreds} credits and has detected [{badNum}] non-employee organic objects.\r\n\r\nYour new balance is ■{newCreds} Credits.\r\n";
                                Plugin.Log.LogInfo("v1 scanner utilized, only numbers shown");
                            }
                            else
                            {
                                node.displayText = $"Not enough credits to run Biomatter Scanner.\r\n";
                                Plugin.Log.LogInfo("brokeboy detected");
                            }
                        }
                        else
                            node.displayText = "Cannot scan for Biomatter at this time.\r\n";
                       
                    }

                    if (node.terminalEvent == "betterescan")
                    {
                        if (enemyScanUpgradeEnabled == false)
                        {
                            int getCreds = __instance.groupCredits;
                            int costCreds = ConfigSettings.bioScanUpgradeCost.Value;
                            if (getCreds >= costCreds)
                            {
                                int newCreds = getCreds - costCreds;
                                __instance.SyncGroupCreditsClientRpc(newCreds, __instance.numberOfItemsInDropship);  //localhost
                                __instance.SyncGroupCreditsServerRpc(newCreds, __instance.numberOfItemsInDropship);  //server
                                node.displayText = $"Biomatter Scanner software has been updated to the latest patch (2.0) and now provides more detailed information!\r\n\r\nYour new balance is ■{newCreds} Credits\r\n";
                                enemyScanUpgradeEnabled = true;
                            }
                            else
                            {
                                node.displayText = $"You cannot afford this upgrade.";
                            }

                        }
                        else
                            node.displayText = $"Upgrade already purchased.";
                    }

                    if (node.terminalEvent == "flashlight")
                    {
                        if (GameNetworkManager.Instance.localPlayerController.pocketedFlashlight != (UnityEngine.Object)null) //maybe get player holding items
                        {
                            FlashlightItem flashlight = GameNetworkManager.Instance.localPlayerController.pocketedFlashlight.gameObject.GetComponent<FlashlightItem>();
                            //string playerFlash = flashlight.playerHeldBy.playerUsername;
                            Color flashlightColor = Terminal_ParsePlayerSentence_Patch.FlashlightColor ?? Color.red; // Use red as a default color
                            flashlight.flashlightBulb.color = flashlightColor;
                            flashlight.flashlightBulbGlow.color = flashlightColor;
                            Plugin.Log.LogInfo($"Setting flashlight color to {Terminal_ParsePlayerSentence_Patch.FlashlightColor}");
                            //Color flashlightColor = Color.green;
                            GameNetworkManager.Instance.localPlayerController.helmetLight.color = flashlightColor;
                            node.displayText = "Flashlight color set.\n";
                        }
                        else
                        {
                            node.displayText = "No flashlight found to set color.\r\n\r\nMake sure you have already equipped a flashlight and turned it on before running this command.\r\n";
                        }                        
                    }
                    if (node.terminalEvent == "teleport")
                    {
                        ShipTeleporter[] objectsOfType = UnityEngine.Object.FindObjectsOfType<ShipTeleporter>();
                        ShipTeleporter tp = (ShipTeleporter)null;
                        foreach (ShipTeleporter normaltp in objectsOfType)
                        {
                            if (!normaltp.isInverseTeleporter)
                            {
                                tp = normaltp;
                                break;
                            }
                        }
                        if ((UnityEngine.Object)tp != (UnityEngine.Object)null)
                        {
                            if (tp.IsSpawned && tp.isActiveAndEnabled)
                            {
                                tp.PressTeleportButtonOnLocalClient();
                                node.displayText = $"{ConfigSettings.tpMessageString.Value}\n";
                            }
                            else node.displayText = "Can't teleport at this time.\n Is it on cooldown?\n";
                        }
                        else node.displayText = "Can't teleport at this time.\n Do you have a teleporter?\n";
                    }
                    if (node.terminalEvent == "inversetp")
                    {
                        ShipTeleporter[] objectsOfType = UnityEngine.Object.FindObjectsOfType<ShipTeleporter>();
                        ShipTeleporter tp = (ShipTeleporter)null;
                        foreach (ShipTeleporter inversetp in objectsOfType)
                        {
                            if (inversetp.isInverseTeleporter)
                            {
                                tp = inversetp;
                                break;
                            }
                        }
                        if ((UnityEngine.Object)tp != (UnityEngine.Object)null)
                        {
                            if (tp.IsSpawned && tp.isActiveAndEnabled && !(StartOfRound.Instance.inShipPhase) && tp.buttonTrigger.interactable)
                            {
                                tp.PressTeleportButtonOnLocalClient();
                                node.displayText = $"{ConfigSettings.itpMessageString.Value}\n";
                            }
                            else node.displayText = "Can't Inverse Teleport at this time.\n";
                        }
                        else node.displayText = "Can't Inverse Teleport at this time.\n Do you have an Inverse Teleporter?\n";
                    }
                    //end of teleport commands

                    if (node.terminalEvent == "vitalsUpgrade")
                    {
                        if (vitalsUpgradeEnabled == false)
                        {
                            int getCreds = __instance.groupCredits;
                            int vitalsUpgradeCost = ConfigSettings.vitalsUpgradeCost.Value;
                            if (getCreds >= vitalsUpgradeCost)
                            {
                                int newCreds = getCreds - vitalsUpgradeCost;
                                __instance.SyncGroupCreditsClientRpc(newCreds, __instance.numberOfItemsInDropship);  //localhost
                                __instance.SyncGroupCreditsServerRpc(newCreds, __instance.numberOfItemsInDropship);  //server
                                vitalsUpgradeEnabled = true;
                                node.displayText = $"Vitals Scanner software has been updated to the latest patch (2.0) and no longer requires credits to scan.\r\n\r\nYour new balance is ■{ newCreds } credits\r\n";
                            }
                            else
                            {
                                node.displayText = $"{ConfigSettings.vitalsUpgradePoor.Value}\n";
                            }
                        }
                        else
                            node.displayText = "Update already purchased.\n";
                    }

                    if (node.terminalEvent == "upgradevitalsAsk")
                    {
                        node.displayText = $"Purchase the Vitals Scanner 2.0 Patch?\nThis software update is available for {ConfigSettings.vitalsUpgradeCost.Value} Credits.\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
                    }

                    if (node.terminalEvent == "vitals")
                    {
                        PlayerControllerB getPlayerInfo = StartOfRound.Instance.mapScreen.targetedPlayer;

                        int getCreds = __instance.groupCredits;
                        int playerHealth = 0;
                        float playerWeight = 0;
                        int costCreds = ConfigSettings.vitalsCost.Value;
                        string playername = getPlayerInfo.playerUsername;
                        Plugin.Log.LogInfo("playername: " + playername);
                        if (getCreds >= costCreds || vitalsUpgradeEnabled) //checks if you can spend enough
                        {
                            if (!getPlayerInfo.isPlayerDead)
                            {
                                playerHealth = getPlayerInfo.health;
                                playerWeight = getPlayerInfo.carryWeight;
                                float playerSanity = getPlayerInfo.insanityLevel;
                                bool hasFlash = getPlayerInfo.ItemSlots.Any(item => item is FlashlightItem);
                                float realWeight = Mathf.RoundToInt(Mathf.Clamp(playerWeight - 1f, 0f, 100f) * 105f);
                                if (!vitalsUpgradeEnabled)
                                {
                                    int newCreds = getCreds - costCreds; //replace with config value after testing
                                    __instance.SyncGroupCreditsClientRpc(newCreds, __instance.numberOfItemsInDropship);  //localhost
                                    __instance.SyncGroupCreditsServerRpc(newCreds, __instance.numberOfItemsInDropship);  //server

                                    if (hasFlash)
                                    {
                                        Plugin.Log.LogInfo("flashlight found");
                                        float flashCharge = Mathf.RoundToInt((getPlayerInfo.pocketedFlashlight.insertedBattery.charge) * 100);
                                        node.displayText = "Charged ■" + costCreds + " Credits. \n" + playername + " Vitals:\n\n Health: " + playerHealth.ToString() + "\n Weight: " + realWeight.ToString() + "\n Sanity: " + playerSanity.ToString() + "\n Flashlight Battery Percentage: " + flashCharge.ToString() + $"%\r\n\r\nYour new balance is ■{newCreds} Credits.\r\n";
                                    }
                                    else //no flashlight
                                        node.displayText = "Charged ■" + costCreds + " Credits. \n" + playername + " Vitals:\n\n Health: " + playerHealth.ToString() + "\n Weight: " + realWeight.ToString() + "\n Sanity: " + playerSanity.ToString() + $"\r\n\r\nYour new balance is ■{newCreds} Credits.\r\n";
                                }
                                else
                                {
                                    if (hasFlash)
                                    {
                                        Plugin.Log.LogInfo("flashlight found");
                                        float flashCharge = Mathf.RoundToInt((getPlayerInfo.pocketedFlashlight.insertedBattery.charge) * 100);
                                        node.displayText = playername + " Vitals:\n\n Health: " + playerHealth.ToString() + "\n Weight: " + realWeight.ToString() + "\n Sanity: " + playerSanity.ToString() + "\n Flashlight Battery Percentage: " + flashCharge.ToString() + "%\n";
                                    }
                                    else //no flashlight
                                        node.displayText = playername + " Vitals:\n\n Health: " + playerHealth.ToString() + "\n Weight: " + realWeight.ToString() + "\n Sanity: " + playerSanity.ToString() + "\n";
                                }
                                
                            }
                            else if (!vitalsUpgradeEnabled && getPlayerInfo.isPlayerDead)
                            {
                                int newCreds = getCreds - costCreds; //replace with config value after testing
                                __instance.SyncGroupCreditsClientRpc(newCreds, __instance.numberOfItemsInDropship);  //localhost
                                __instance.SyncGroupCreditsServerRpc(newCreds, __instance.numberOfItemsInDropship);  //server
                                node.displayText = ($"Charged ■{costCreds} Credits. \n Unable to get +" + playername + $"vitals...\r\n\r\nYour new balance is ■{newCreds} Credits.\r\n");
                            }
                            else
                                node.displayText = ("Unable to get +" + playername + "vitals...\n");

                        }
                        else
                        {
                            node.displayText = $"{ConfigSettings.vitalsPoorString.Value}\n";
                        }
                    }
                    if (node.terminalEvent == "danger")
                    {
                        if (StartOfRound.Instance.shipHasLanded)
                        {
                            string dangerLevel = OpeningDoorsPatch.getDangerLevel;
                            node.displayText = ("Current Danger Level: " + dangerLevel + "\n");
                        }
                        else
                        {
                            node.displayText = ("Ship has not landed.\n");
                        }
                    }
                    if (node.terminalEvent == "gamble")
                    {
                        node.clearPreviousText = true;
                        // Example: Get the percentage from the ParsedValue
                        float percentage = Terminal_ParsePlayerSentence_Patch.ParsedValue;

                        // Check if the percentage is within the valid range (0-100)
                        if (!Terminal_ParsePlayerSentence_Patch.newParsedValue || (percentage < 0 || percentage > 100))
                        {
                            // Handle the case when percentage is outside the valid range
                            Debug.Log("Invalid percentage value. Telling user.");
                            node.displayText = ("Invalid gamble percentage, please input a value between 0 and 100.\n");
                        }
                        if (__instance.groupCredits <= ConfigSettings.gambleMinimum.Value)
                        {
                            // Handle the case when groupCredits is lower than minimum required
                            Debug.Log("Invalid percentage value. Telling user.");
                            node.displayText = $"{ConfigSettings.gamblePoorString.Value}\n";
                        }
                        else
                        {
                            // Make the gamble and get the result
                            var gambleResult = Gamble(__instance.groupCredits, percentage);


                            // Assign the result values to appropriate variables
                            __instance.groupCredits = gambleResult.newGroupCredits;
                            __instance.SyncGroupCreditsClientRpc(gambleResult.newGroupCredits, __instance.numberOfItemsInDropship);  //localhost
                            __instance.SyncGroupCreditsServerRpc(gambleResult.newGroupCredits, __instance.numberOfItemsInDropship);  //server
                            Terminal_ParsePlayerSentence_Patch.newParsedValue = false;
                            node.displayText = gambleResult.displayText;
                        }

                    }
                    if (node.terminalEvent == "healme")
                    {
                        //this code snippet is slightly modified from Octolar's Healing Mod, credit to them
                        if (GameNetworkManager.Instance.localPlayerController.health < 100 || !GameNetworkManager.Instance.localPlayerController.criticallyInjured)
                            node.displayText = $"{ConfigSettings.healIsFullString.Value}\n";
                        else
                        {
                            GameNetworkManager.Instance.localPlayerController.DamagePlayer(-100, false);
                            GameNetworkManager.Instance.localPlayerController.MakeCriticallyInjured(false);
                            node.displayText = $"{ConfigSettings.healString.Value}\nHealth: {GameNetworkManager.Instance.localPlayerController.health.ToString()}";
                        }
                    }
                    if (node.terminalEvent == "mapEvent")
                    {
                        enabledSplitObjects = false;
                        checkForSplitView("neither"); //disables split view if enabled
                        if (RoundManager.Instance != null && RoundManager.Instance.hasInitializedLevelRandomSeed)
                        {
                            if (Plugin.instance.isOnMap == false)
                            {
                                node.clearPreviousText = true;
                                Texture renderTexture = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[1].mainTexture;
                                node.displayTexture = renderTexture;
                                __instance.terminalImage.enabled = true;
                                Plugin.instance.isOnCamera = false;
                                Plugin.instance.isOnMap = true;
                                Plugin.Log.LogInfo("map radar enabled");
                                node.displayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nEnabling radar view\r\n";
                            }
                            else if (Plugin.instance.isOnMap == true)
                            {
                                Plugin.Log.LogInfo("disabling map & disabling cams"); //debug
                                node.displayTexture = null;
                                node.loadImageSlowly = false;
                                node.displayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nDisabling radar view\r\n";
                                Plugin.instance.isOnMap = false;
                                Plugin.instance.isOnCamera = false;
                                //endMapCommand = true;
                            }
                            else
                                Plugin.Log.LogError("Map command ERROR, isOnMap neither true nor false!!!");
                        }
                        else
                        {
                            node.displayTexture = null;
                            Plugin.Log.LogInfo("this should only trigger in orbit");
                            node.clearPreviousText = true;
                            node.loadImageSlowly = false;
                            node.displayText = "Radar view not available in orbit.\r\n";
                            Plugin.instance.isOnMap = false; //this one too
                            Plugin.instance.isOnCamera = false; //command will disable cams either way
                        }
                    }

                    if (node.terminalEvent == "cams")
                    {
                        enabledSplitObjects = false;
                        checkForSplitView("neither"); //disables split view if enabled
                        if (GameObject.Find("Environment/HangarShip/Cameras/ShipCamera") != null && Plugin.instance.isOnCamera == false)
                        {

                            node.clearPreviousText = true;
                            // Get the main texture from "Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001"
                            Texture renderTexture = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture;
                            node.displayTexture = renderTexture;
                            __instance.terminalImage.enabled = true;
                            Plugin.instance.isOnCamera = true;
                            Plugin.instance.isOnMap = false;
                            //isOnCamera = true;
                            Plugin.Log.LogInfo("cam added to terminal screen");
                            node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n{ConfigSettings.camString.Value}\n"; //the excessive n's are to get the text to display under the cams
                        }
                        else if (Plugin.instance.isOnCamera == true)
                        {
                            node.clearPreviousText = true;
                            node.displayTexture = null;
                            __instance.terminalImage.enabled = false;
                            Plugin.instance.isOnCamera = false;
                            Plugin.Log.LogInfo("cam removed");
                        }
                        else
                        {
                            Plugin.Log.LogInfo("Unable to run cameras event for some reason...");
                        }
                    }

                    if (node.terminalEvent == "overlay")
                    {
                        node.clearPreviousText = true;
                        Plugin.instance.isOnMap = false;
                        Plugin.instance.isOnCamera = false;
                        Texture texture1 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[1].mainTexture; // radar
                        Texture texture2 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture; // cams


                        if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnOverlay == false )
                        {
                            Plugin.instance.rawImage2.texture = texture2;
                            Plugin.instance.rawImage1.texture = texture1;
                            __instance.terminalImage.enabled = true;
                            Color currentColor = Plugin.instance.rawImage2.color;
                            Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.1f); //10% opacity
                            Plugin.instance.rawImage1.color = newColor;


                            // Set the visibility for the new RawImages

                            Plugin.instance.rawImage2.enabled = true;
                            Plugin.instance.rawImage1.enabled = true;

                            // Use Canvas's dimensions for positioning and scaling
                            RectTransform canvasRect = Plugin.instance.terminalCanvas.GetComponent<RectTransform>();
                            // Revert the changes to the original values
                            Plugin.instance.rawImage1.rectTransform.sizeDelta = Plugin.instance.originalTopSize;
                            Plugin.instance.rawImage1.rectTransform.anchoredPosition = Plugin.instance.originalTopPosition;

                            enabledSplitObjects = true;
                            checkForSplitView("overlay");
                            Plugin.instance.isOnOverlay = true;
                            node.displayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nOverlay View Enabled.\r\n";
                        }
                        else if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnOverlay == true)
                        {
                            enabledSplitObjects = false;
                            checkForSplitView("overlay"); //disables split view if enabled
                            node.displayText = "Overlay View disabled.\r\n";
                        }
                        else
                        {
                            Plugin.Log.LogError("Unexpected condition");
                        }
                    }

                    if (node.terminalEvent == "proview")
                    {
                        node.clearPreviousText = true;
                        Plugin.instance.isOnMap = false;
                        Plugin.instance.isOnCamera = false;
                        Plugin.instance.isOnOverlay = false;
                        Texture texture1 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[1].mainTexture; // radar
                        Texture texture2 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture; // cams

                        if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnProView == false )
                        {
                            Plugin.instance.rawImage2.texture = texture2;
                            Plugin.instance.rawImage1.texture = texture1;

                            __instance.terminalImage.enabled = true;
                            Color currentColor = Plugin.instance.rawImage2.color;
                            Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, 0.7f); //70% opacity
                            Plugin.instance.rawImage1.color = newColor;


                            // Set the visibility for the new RawImages

                            Plugin.instance.rawImage2.enabled = true;
                            Plugin.instance.rawImage1.enabled = true;

                            // Use Canvas's dimensions for positioning and scaling
                            RectTransform canvasRect = Plugin.instance.terminalCanvas.GetComponent<RectTransform>();

                             // Calculate the dimensions for radar image (rawImage1)
                             float topHeight = canvasRect.rect.height * 0.2f; // 20% of the canvas height
                             float topWidth = canvasRect.rect.width * 0.25f; //quarter of the width
                             Plugin.instance.rawImage1.rectTransform.sizeDelta = new Vector2(topWidth, topHeight);
                             Plugin.instance.rawImage1.rectTransform.anchoredPosition = new Vector2(130f, 103f);

                            enabledSplitObjects = true;
                            checkForSplitView("proview");
                            Plugin.instance.isOnProView = true;
                            node.displayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nPro View Enabled.\r\n";
                        }
                        else if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnProView == true)
                        {
                            enabledSplitObjects = false;
                            checkForSplitView("proview"); //disables split view if enabled
                            node.displayText = "Pro View disabled.\r\n";
                        }
                        else
                        {
                            Plugin.Log.LogError("Unexpected condition");
                        }
                    }



                    if (node.terminalEvent == "test")
                    {
                        node.displayText = "this shouldn't be enabled lol\n";
                        __instance.SyncGroupCreditsClientRpc(999999, __instance.numberOfItemsInDropship);

                    }
                    if (node.terminalEvent == "fov")
                    {
                        int num = Terminal_ParsePlayerSentence_Patch.ParsedValue;
                        float number = num;
                        if (number != 0 && number >= 66f && number <= 130f && Terminal_ParsePlayerSentence_Patch.newParsedValue)  // Or use an appropriate default value
                        {
                            node.clearPreviousText = true;
                            node.displayText = ("Setting FOV to - " + num.ToString() + "\n");
                            // Delay for 1 second
                            yield return new WaitForSeconds(1);

                            Debug.Log("After 1 second");

                            // Now, call QuitTerminal on the original instance
                            __instance.QuitTerminal();
                            number = Mathf.Clamp(number, 66f, 130f);
                            PlayerControllerBPatches.newTargetFovBase = number;
                            Terminal_ParsePlayerSentence_Patch.newParsedValue = false;
                        }
                        else
                        {
                            node.displayText = "Fov can only be set between 66 and 130\n"; //not sure why this isn't 66 to 130 lol
                        }
                    }
                }
                // Ensure all code paths return a value in a coroutine
                yield break;
            }

            private static (int newGroupCredits, string displayText) Gamble(int currentGroupCredits, float percentage)
            {
                // Ensure the percentage is within a valid range (0-100)
                percentage = Mathf.Clamp(percentage, 0, 100);

                // Calculate the gamble amount as a percentage of the total credits
                int gambleAmount = (int)(currentGroupCredits * (percentage / 100.0f));

                // Generate two separate random floats
                float randomValue1 = UnityEngine.Random.value;
                float randomValue2 = UnityEngine.Random.value;

                // Determine the outcome based on a fair comparison of the two random values
                bool isWinner = randomValue1 < randomValue2;

                if (isWinner)
                {
                    // Code for winning scenario
                    string displayText = $"Congratulations! You won ■{gambleAmount} credits!\r\n\r\nYour new balance is ■{currentGroupCredits + gambleAmount} Credits.\r\n";
                    return (currentGroupCredits + gambleAmount, displayText);
                }
                else
                {
                    // Code for losing scenario
                    int localResult = currentGroupCredits - gambleAmount;
                    if (ConfigSettings.gamblePityMode.Value && localResult == 0) //checks for pity mode and 0 credits
                    {
                        if(ConfigSettings.gamblePityCredits.Value <= 60) //capping pity credits to 60 to avoid abuses of this system.
                        {
                            string displayText = $"Sorry, you lost ■{gambleAmount} credits.\n\nHowever, you've received {ConfigSettings.gamblePityCredits.Value} Pity Credits.\r\n\r\nYour new balance is ■{ConfigSettings.gamblePityCredits.Value} Credits.\r\n";
                            return (ConfigSettings.gamblePityCredits.Value, displayText);
                        }
                        else
                        {
                            string displayText = $"Sorry, you lost ■{gambleAmount} credits.\n\nUnfortunately we're also fresh out of Pity Credits due to malicious actors.\r\n\r\nYour new balance is ■{localResult} Credits.\r\n";
                            return (currentGroupCredits - gambleAmount, displayText);
                        }
                        
                    }
                    else
                    {
                        string displayText = $"Sorry, you lost ■{gambleAmount} credits.\r\n\r\nYour new balance is ■{localResult} Credits.\r\n";
                        return (currentGroupCredits - gambleAmount, displayText);
                    }          
                }
            }

            static void Postfix(Terminal __instance, TerminalNode node)
            {
                // Start the coroutine
                __instance.StartCoroutine(PostfixCoroutine(__instance, node));
            }

        }

        private static void checkForSplitView( string whatisit )
        {
            if (enabledSplitObjects == false) //make sure to disable either splitview
            {
                // Assuming you have a reference to the ImageContainer GameObject
                GameObject imageContainer = GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer/ImageContainer");

                // Check if the imageContainer is not null
                if (imageContainer != null)
                {
                    // Disable specific children based on their names
                    foreach (Transform child in imageContainer.transform)
                    {
                        if (child.gameObject.name.Contains("(Clone)"))
                        {
                            child.gameObject.SetActive(false);
                        }
                    }
                }
                Plugin.Log.LogInfo("Arg specified:" + whatisit);

                // Update bools, safe to just disable both every time i think
                Plugin.instance.isOnOverlay = false;
                Plugin.instance.isOnProView = false;
            }
            else if (enabledSplitObjects == true)
            {
                // Assuming you have a reference to the ImageContainer GameObject
                GameObject imageContainer = GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer/ImageContainer");

                // Check if the imageContainer is not null
                if (imageContainer != null)
                {
                    // Disable specific children based on their names
                    foreach (Transform child in imageContainer.transform)
                    {
                        if (child.gameObject.name.Contains("(Clone)"))
                        {
                            child.gameObject.SetActive(true);
                        }
                    }
                }

                // Update bool
                enabledSplitObjects = false;
                // Update bools
                if(whatisit == "proview")
                {
                    Plugin.instance.isOnProView = true;
                    Plugin.instance.isOnOverlay = false;
                }
                else if(whatisit == "overlay")
                {
                    Plugin.instance.isOnOverlay = true;
                    Plugin.instance.isOnProView = false;
                }

            }
            else
                Plugin.Log.LogInfo("no matches for splitview objects");
        }


        class shiploot
        {
            public static void getLoot()
            {
                string totalvalue = string.Empty;
                Plugin.Log.LogInfo("calculating loot value next");
                float lootValue = shiploot.CalculateLootValue();
                totalvalue = string.Format("Total Value on Ship: ${0:F0}", (object)lootValue);
                LeaveTerminal.TotalValueFormat = totalvalue;
                Plugin.Log.LogInfo("loot calculated");
            }

            public static float CalculateLootValue()
            {
                List<GrabbableObject> list = ((IEnumerable<GrabbableObject>)GameObject.Find("/Environment/HangarShip").GetComponentsInChildren<GrabbableObject>())
                    .Where<GrabbableObject>(obj => obj.name != "ClipboardManual" && obj.name != "StickyNoteItem").ToList<GrabbableObject>();

                Plugin.Log.LogDebug((object)"Calculating total ship scrap value.");

                CollectionExtensions.Do<GrabbableObject>((IEnumerable<GrabbableObject>)list, (Action<GrabbableObject>)(scrap => Plugin.Log.LogDebug((object)string.Format("{0} - ${1}", (object)scrap.name, (object)scrap.scrapValue))));

                return (float)list.Sum<GrabbableObject>(scrap => scrap.scrapValue);
            }
        }

        public static void AddProView()
        {
            TerminalNode splitNode = CreateTerminalNode("testing split mode\n", true, "proview");
            //TerminalKeyword splitKeyword = CreateTerminalKeyword("pro", true, splitNode); //removed due to conflicts with shop
            TerminalKeyword Keyword2 = CreateTerminalKeyword("proview", true, splitNode);
            AddTerminalKeyword(Keyword2);
            Plugin.Log.LogInfo("added proview keywords");
        }

        public static void AddOverlayView()
        {
            TerminalNode splitNode = CreateTerminalNode("testing split mode\n", true, "overlay");
            TerminalKeyword splitKeyword = CreateTerminalKeyword("overlay", true, splitNode);
            AddTerminalKeyword(splitKeyword);
            Plugin.Log.LogInfo("added overlay keyword");
        }
        public static void AddDoor()
        {
            TerminalNode node = CreateTerminalNode("door terminalEvent", false, "door");
            TerminalKeyword doorKW = CreateTerminalKeyword("door", true, node);
            AddTerminalKeyword(doorKW);
            Plugin.Log.LogInfo($"Door keyword added");
        }

        public static void AddTest()
        {
            TerminalNode test = CreateTerminalNode("test\n", true, "door");
            TerminalKeyword testKeyword = CreateTerminalKeyword("test", true, test);
            AddTerminalKeyword(testKeyword);
            Plugin.Log.LogInfo("This should only be enabled for dev testing");
        }
        public static void AddModListKeywords()
        {
            TerminalNode modList = CreateTerminalNode("grabbing mods\n", true, "modlist");
            TerminalKeyword modlistKeyword = CreateTerminalKeyword("modlist", true, modList);
            TerminalKeyword modsKeyword = CreateTerminalKeyword("mods", true, modList);
            AddTerminalKeyword(modlistKeyword);
            AddTerminalKeyword(modsKeyword);
            Plugin.Log.LogInfo("Added Modlist keywords");
        }

        public static void AddTeleportKeywords()
        {
            TerminalNode tpNode = CreateTerminalNode("teleporter initiatied.\n", true, "teleport");
            TerminalKeyword teleportKeyword = CreateTerminalKeyword("teleport", true, tpNode);
            TerminalKeyword tpKeyword = CreateTerminalKeyword("tp", true, tpNode);
            AddTerminalKeyword(teleportKeyword);
            AddTerminalKeyword(tpKeyword);
            Plugin.Log.LogInfo("---------Teleport & TP Keywords added!---------");
        }

        public static void AddInverseTeleportKeywords()
        {
            TerminalNode tpNode = CreateTerminalNode("teleporter initiatied.\n", true, "inversetp");
            TerminalKeyword inverseteleportKeyword = CreateTerminalKeyword("inverse", true, tpNode);
            TerminalKeyword itpKeyword = CreateTerminalKeyword("itp", true, tpNode);
            AddTerminalKeyword(inverseteleportKeyword);
            AddTerminalKeyword(itpKeyword);
            Plugin.Log.LogInfo("---------Inverse & ITP Keywords added!---------");
        }

        public static void AddQuitKeywords()
        {
            TerminalNode quitNode = CreateTerminalNode("leaving.\n", true, "quit");
            TerminalKeyword exitKeyword = CreateTerminalKeyword("exit", true, quitNode);
            TerminalKeyword quitKeyword = CreateTerminalKeyword("quit", true, quitNode);
            AddTerminalKeyword(exitKeyword);
            AddTerminalKeyword(quitKeyword);
            Plugin.Log.LogInfo("---------Quit & Exit Keywords added!---------");
        }
        public static void hampterKeywords()
        {
            TerminalNode lolNode = CreateTerminalNode($"lol.\n", false, "lolevent");
            TerminalKeyword lolKeyword = CreateTerminalKeyword("lol", true, lolNode);
            TerminalKeyword hampterKeyword = CreateTerminalKeyword("hampter", true, lolNode);
            AddTerminalKeyword(hampterKeyword);
            AddTerminalKeyword(lolKeyword);
            Plugin.Log.LogInfo("lol");
        }
        public static void clearKeywords()
        {
            TerminalNode clearNode = CreateTerminalNode($"\n", true); //clear terminal event was not needed
            TerminalKeyword clearKeyword = CreateTerminalKeyword("clear", true, clearNode);
            AddTerminalKeyword(clearKeyword);
            Plugin.Log.LogInfo("Adding Clear keywords");
        }
        public static void dangerKeywords()
        {
            TerminalNode dangerNode = CreateTerminalNode($"\n", true, "danger");
            TerminalKeyword dangerKeyword = CreateTerminalKeyword("danger", true, dangerNode);
            AddTerminalKeyword(dangerKeyword);
            Plugin.Log.LogInfo("Adding danger keywords");
        }
        public static void vitalsKeywords()
        {
            TerminalNode vitalsNode = CreateTerminalNode($"\n", true, "vitals");
            TerminalKeyword vitalsKeyword = CreateTerminalKeyword("vitals", true, vitalsNode);
            AddTerminalKeyword(vitalsKeyword);
            Plugin.Log.LogInfo("Adding vitals keywords");
        }
        public static void healKeywords()
        {
            TerminalNode healNode = CreateTerminalNode($"\n", true, "healme");
            TerminalKeyword healKeyword = CreateTerminalKeyword("heal", true, healNode);
            TerminalKeyword healmeKeyword = CreateTerminalKeyword("healme", true, healNode);
            AddTerminalKeyword(healKeyword);
            AddTerminalKeyword(healmeKeyword);
            Plugin.Log.LogInfo("Added Heal Keywords");
        }
        public static void lootKeywords()
        {
            TerminalNode lootNode = CreateTerminalNode($"Attempting to grab total loot value on ship.\n", false, "loot");
            TerminalKeyword lootKeyword = CreateTerminalKeyword("loot", true, lootNode);
            TerminalKeyword shiplootKeyword = CreateTerminalKeyword("shiploot", true, lootNode);
            AddTerminalKeyword(lootKeyword);
            AddTerminalKeyword(shiplootKeyword);
            Plugin.Log.LogInfo("Loot commands added!");
        }

        public static void camsKeywords()
        {
            TerminalNode camsNode = CreateTerminalNode($"Toggling Cameras View.\n", true, "cams");
            TerminalKeyword camsKeyword = CreateTerminalKeyword("cams", true, camsNode);
            TerminalKeyword camerasKeyword = CreateTerminalKeyword("cameras", false, camsNode);
            AddCompatibleNoun("check", "cameras", camsNode);
            AddCompatibleNoun("check", "cams", camsNode);
            AddTerminalKeyword(camsKeyword);
            AddTerminalKeyword(camerasKeyword);
            Plugin.Log.LogInfo("Cameras commands added!");
            //can't believe this was easier than displaying a custom video
        }
        public static void mapKeywords()
        {
            TerminalNode mapNode = CreateTerminalNode($"Toggling radar view.\n", true, "mapEvent");
            TerminalKeyword mapKeyword = CreateTerminalKeyword("map", true, mapNode);
            AddTerminalKeyword(mapKeyword);
            Plugin.Log.LogInfo("Map command added!");
        }
    }
}


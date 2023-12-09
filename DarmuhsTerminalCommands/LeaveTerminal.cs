using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using BepInEx;
using HarmonyLib;
using UnityEngine;
using static TerminalApi.TerminalApi;
using static TerminalStuff.MyTerminalAwakePatch;
using GameNetcodeStuff;
using System.Collections;
using FovAdjust;
using Unity.Netcode;

namespace TerminalStuff
{
    public static class LeaveTerminal
    {
        public static string TotalValueFormat = "";
        public static string VideoErrorMessage = "";
        private static Dictionary<string, PluginInfo> PluginsLoaded = new Dictionary<string, PluginInfo>();


        public static TerminalNode GetNodeAfterConfirmation(this TerminalNode node)
        {
            var confirmationNoun = ((IEnumerable<CompatibleNoun>)node.terminalOptions)
                .FirstOrDefault(cn => cn.noun.name == "Confirm");

            return confirmationNoun?.result;
        }

        [HarmonyPatch(typeof(Terminal))]
        [HarmonyPatch("RunTerminalEvents")]
        public class Terminal_RunTerminalEvents_Patch : MonoBehaviour
        {
            private static VideoController videoController;
            static IEnumerator PostfixCoroutine(Terminal __instance, TerminalNode node)
            {
                if (!string.IsNullOrWhiteSpace(node.terminalEvent))
                {
                    if (node.terminalEvent == "quit")
                    {
                        
                        string text = "goodbye!\n";
                        node.displayText = text;

                        Debug.Log("Start of the coroutine");

                        // Delay for 1 second
                        yield return new WaitForSeconds(1);

                        Debug.Log("After 1 second");

                        // Now, call QuitTerminal on the original instance
                        __instance.QuitTerminal();
                    }
                    if (node.terminalEvent == "kickYes")
                        node.displayText = "Player has been kicked.\n";
                    if (node.terminalEvent == "kickNo")
                        node.displayText = "Unable to kick, player not found.\n";
                    if (node.terminalEvent == "NotHost")
                        node.displayText = "You do not have access to this command.\n";
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
                            node.displayText = "No more lol.\n";
                        }
                        else
                        {
                            // Play the next video if not playing
                            videoController.PlayNextVideo();
                            VideoController.isVideoPlaying = true;
                            node.clearPreviousText = true;
                            node.displayText = "lol.\n";
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
                    if (node.terminalEvent == "clear")
                    {
                        node.clearPreviousText = true;
                        Plugin.Log.LogInfo("text cleared");
                        //this function broke in an update
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
                                node.displayText = "Teleport Button pressed.\n";
                            }
                            else node.displayText = "Can't teleport at this time. Is it on cooldown?\n";
                        }
                        else node.displayText = "Can't teleport at this time. Do you have a teleporter?\n";
                    }
                    //end of teleport command
                            
                    if (node.terminalEvent == "vitals")
                    {
                        PlayerControllerB getPlayerInfo = StartOfRound.Instance.mapScreen.targetedPlayer;

                        int getCreds = __instance.groupCredits;
                        int playerHealth = 0;
                        float playerWeight = 0;
                        int costCreds = ConfigSettings.vitalsCost.Value;
                        string playername = getPlayerInfo.playerUsername;
                        Plugin.Log.LogInfo("playername: " + playername);
                        if (getCreds >= costCreds) //checks if you can spend enough
                        {
                            if (!getPlayerInfo.isPlayerDead)
                            {
                                playerHealth = getPlayerInfo.health;
                                playerWeight = getPlayerInfo.carryWeight;
                                float playerSanity = getPlayerInfo.insanityLevel;
                                bool hasFlash = getPlayerInfo.ItemSlots.Any(item => item is FlashlightItem);

                                float realWeight = Mathf.RoundToInt(Mathf.Clamp(playerWeight - 1f, 0f, 100f) * 105f);
                                int newCreds = getCreds - costCreds; //replace with config value after testing
                                __instance.SyncGroupCreditsClientRpc(newCreds, __instance.numberOfItemsInDropship);  //localhost
                                __instance.SyncGroupCreditsServerRpc(newCreds, __instance.numberOfItemsInDropship);  //server

                                if(hasFlash)
                                {
                                    Plugin.Log.LogInfo("flashlight found");
                                    float flashCharge = Mathf.RoundToInt((getPlayerInfo.pocketedFlashlight.insertedBattery.charge)*100);
                                    node.displayText = "Charged " + costCreds + " Credits. \n" + playername + " Vitals:\n\n Health: " + playerHealth.ToString() + "\n Weight: " + realWeight.ToString() + "\n Sanity: " + playerSanity.ToString() + "\n Flashlight Battery Percentage: " + flashCharge.ToString() + "%\n";
                                }
                                else //no flashlight
                                node.displayText = "Charged " + costCreds + " Credits. \n" + playername + " Vitals:\n\n Health: " + playerHealth.ToString() + "\n Weight: " + realWeight.ToString() + "\n Sanity: " + playerSanity.ToString() + "\n";
                            }
                            else
                            {
                                int newCreds = getCreds - 10; //replace with config value after testing
                                __instance.SyncGroupCreditsClientRpc(newCreds, __instance.numberOfItemsInDropship);  //localhost
                                __instance.SyncGroupCreditsServerRpc(newCreds, __instance.numberOfItemsInDropship);  //server
                                node.displayText = ("Charged 10 Credits. \n Unable to get +" + playername + "vitals...\n");
                            }

                        }
                        else
                        {
                            node.displayText = "You can't afford to run this command.\n";
                        }
                    }
                    if (node.terminalEvent == "danger")
                    {
                        if(StartOfRound.Instance.shipHasLanded)
                        {
                            string dangerLevel = RoundStartPatch.getDangerLevel;
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
                        float percentage = Terminal_ParseWord_Patch.ParsedValue;

                        // Check if the percentage is within the valid range (0-100)
                        if (!Terminal_ParseWord_Patch.newParsedValue || (percentage < 0 || percentage > 100))
                        {
                            // Handle the case when percentage is outside the valid range
                            Debug.Log("Invalid percentage value. Telling user.");
                            node.displayText = ("Invalid gamble percentage, please input a value between 0 and 100.\n");
                        }
                        else
                        {
                            // Make the gamble and get the result
                            var gambleResult = Gamble(__instance.groupCredits, percentage);
                            

                            // Assign the result values to appropriate variables
                            __instance.groupCredits = gambleResult.newGroupCredits;
                            __instance.SyncGroupCreditsClientRpc(gambleResult.newGroupCredits, __instance.numberOfItemsInDropship);  //localhost
                            __instance.SyncGroupCreditsServerRpc(gambleResult.newGroupCredits, __instance.numberOfItemsInDropship);  //server
                            Terminal_ParseWord_Patch.newParsedValue = false;
                            node.displayText = gambleResult.displayText;
                        }
                        
                    }
                    if (node.terminalEvent == "healme")
                    {
                        //this code snippet is slightly modified from Octolar's Healing Mod, credit to them
                        GameNetworkManager.Instance.localPlayerController.DamagePlayer(-100, false);
                        if (GameNetworkManager.Instance.localPlayerController.health < 10 || !GameNetworkManager.Instance.localPlayerController.criticallyInjured)
                            node.displayText = "You are full health!\n";
                        else
                        {
                            GameNetworkManager.Instance.localPlayerController.MakeCriticallyInjured(false);
                            node.displayText = "The terminal healed you?!?\n";
                        }
                    }
       /*             if (node.terminalEvent == "leverask") //planned feature for lever command
                    {
                        node.displayText = "Pull the Lever?\n\n\n\n\n\n\n\n";
                        node.displayText = "Please CONFIRM or DENY.\n";
                        TerminalNode nextNode = node.GetNodeAfterConfirmation();
                        nextNode.terminalEvent = "leverdo";
                    }
                    if (node.terminalEvent == "leverdont")
                    {
                        node.displayText = "NOT pulling the lever, smile.";
                    } */
                    if (node.terminalEvent == "leverdo")
                    {
                        NetworkManager networkManager = __instance.NetworkManager;
                        if (!GameNetworkManager.Instance.gameHasStarted && ((object)networkManager != null && networkManager.IsHost))
                            {
                            node.displayText = "PULLING THE LEVER!!!\n";

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
                        else if (GameNetworkManager.Instance.gameHasStarted)
                        {
                            node.displayText = "PULLING THE LEVER!!!\n";

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
                    //end of lever event    
                    }
                    if (node.terminalEvent == "cams")
                    {
                        bool isOnCamera = __instance.terminalImage.enabled;
                        if (GameObject.Find("Environment/HangarShip/Cameras/ShipCamera") != null && isOnCamera == false)
                        {
                            
                            node.clearPreviousText = true;
                            // Get the main texture from "Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001"
                            Texture renderTexture = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture;
                            node.displayTexture = renderTexture;
                            __instance.terminalImage.enabled = true;
                            //isOnCamera = true;
                            Plugin.Log.LogInfo("cam added to terminal screen");
                            node.displayText = ("\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n <Cameras Toggled>\n"); //the excessive n's are to get the text to display under the cams
                            
                        }
                        else if (isOnCamera == true)
                        {
                            node.displayTexture = null;
                            __instance.terminalImage.enabled = false;
                            //isOnCamera = false;
                            Plugin.Log.LogInfo("cam removed");
                        }
                        else
                        {
                            Plugin.Log.LogInfo("Unable to run cameras event for some reason...");
                        }
                    }

                    if (node.terminalEvent =="test")
                    {
                        node.displayText = "this shouldn't be enabled lol\n";
                        __instance.SyncGroupCreditsClientRpc(999999, __instance.numberOfItemsInDropship);

                    }
                    if (node.terminalEvent == "fov")
                    {
                        int num = Terminal_ParseWord_Patch.ParsedValue;
                        float number = num;
                        if (number != 0 && number >= 66f && number <= 130f && Terminal_ParseWord_Patch.newParsedValue)  // Or use an appropriate default value
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
                            Terminal_ParseWord_Patch.newParsedValue = false;
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
                    string displayText = $"Congratulations! You won {gambleAmount} credits!\n";
                    return (currentGroupCredits + gambleAmount, displayText);
                }
                else
                {
                    // Code for losing scenario
                    string displayText = $"Sorry, you lost {gambleAmount} credits.\n";
                    return (currentGroupCredits - gambleAmount, displayText);
                }
            }

            static void Postfix(Terminal __instance, TerminalNode node)
            {
                // Start the coroutine
                __instance.StartCoroutine(PostfixCoroutine(__instance, node));
            }

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

        /*
                class ErrorHandlingClass
                {
                    private string errorMessage;

                    public void HandleError(UnityEngine.Video.VideoPlayer source, string message)
                    {
                        // Your error handling code here
                        errorMessage = message;
                        errorMessage = VideoErrorMessage;
                        // Do something with the errorMessage
                    }

                    public string GetErrorMessage()
                    {
                        return errorMessage;
                    }
                }
        */

        //keywords

        public static void AddTest()
        {
            TerminalNode test = CreateTerminalNode("test\n", true, "test");
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
        public static void leverKeywords()
        {
            TerminalNode leverNode = CreateTerminalNode($"\n", true, "leverdo");
            TerminalKeyword leverKeyword = CreateTerminalKeyword("lever", true, leverNode);
            AddTerminalKeyword(leverKeyword);
            AddCompatibleNoun(leverKeyword, "confirm", "leverdo");
            AddCompatibleNoun(leverKeyword, "deny", "leverdont");
            Plugin.Log.LogInfo("Added Lever Keyword");
            
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
    }
}


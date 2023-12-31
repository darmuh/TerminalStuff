﻿using BepInEx;
using BepInEx.Bootstrap;
using FovAdjust;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using static TerminalApi.TerminalApi;
using static TerminalStuff.AllMyTerminalPatches;
using System.Security.Policy;
using UnityEngine.Video;
using Random = UnityEngine.Random;
using System.Diagnostics;
using Steamworks;

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
        public static bool isVideoPlaying = false;

        private static int lastPlayedIndex = -1;

        [HarmonyPatch(typeof(Terminal))]
        [HarmonyPatch("RunTerminalEvents")]
        public class Terminal_RunTerminalEvents_Patch : MonoBehaviour
        {

            private static int GetMyPlayerID()
            {
                List<PlayerControllerB> allPlayers = new List<PlayerControllerB>();
                string myName = GameNetworkManager.Instance.localPlayerController.playerUsername;
                int returnID = -1;
                allPlayers = StartOfRound.Instance.allPlayerScripts.ToList();
                allPlayers = allPlayers.OrderBy((PlayerControllerB player) => player.playerClientId).ToList();
                for (int i = 0; i < allPlayers.Count; i++)
                {
                    if (StartOfRound.Instance.allPlayerScripts[i].playerUsername == myName)
                    {
                        Plugin.Log.LogInfo("Found my playerID");
                        returnID = i;
                        break;
                    }
                }
                if (returnID == -1)
                    Plugin.Log.LogInfo("Failed to find ID");
                return returnID;
            }


            private static string getRandomSuit(out string displayText)
            {
                List<UnlockableSuit> allSuits = new List<UnlockableSuit>();
                List<UnlockableItem> Unlockables = new List<UnlockableItem>();

                //get allSuits
                allSuits = Resources.FindObjectsOfTypeAll<UnlockableSuit>().ToList();
                displayText = string.Empty;

                if (allSuits.Count > 1)
                {
                    // Order the list by syncedSuitID.Value
                    allSuits = allSuits.OrderBy((UnlockableSuit suit) => suit.suitID).ToList();

                    allSuits.RemoveAll(suit => suit.syncedSuitID.Value < 0); //simply remove bad suit IDs

                    Unlockables = StartOfRound.Instance.unlockablesList.unlockables;

                    
                    int playerID = GetMyPlayerID();


                    if (Unlockables != null)
                    {
                        for (int i = 0; i < Unlockables.Count; i++)
                        {
                            // Get a random index
                            int randomIndex = UnityEngine.Random.Range(0, allSuits.Count);
                            string SuitName;

                            // Get the UnlockableSuit at the random index
                            UnlockableSuit randomSuit = allSuits[randomIndex];
                            if (randomSuit != null && Unlockables[randomSuit.syncedSuitID.Value] != null)
                            {
                                SuitName = Unlockables[randomSuit.syncedSuitID.Value].unlockableName;
                                UnlockableSuit.SwitchSuitForPlayer(StartOfRound.Instance.allPlayerScripts[playerID], randomSuit.syncedSuitID.Value, true);
                                randomSuit.SwitchSuitServerRpc(playerID);
                                randomSuit.SwitchSuitClientRpc(playerID);
                                displayText = $"Changing suit to {SuitName}!\r\n";
                                return displayText;
                            }
                            else
                            {
                                displayText = "A suit could not be found.\r\n";
                                Plugin.Log.LogInfo($"Random suit ID was invalid or null");
                                return displayText;
                            }
                        }
                    }

                    displayText = "A suit could not be found.\r\n";
                    Plugin.Log.LogInfo($"Unlockables are null");
                    return displayText;
                }
                else
                {
                    displayText = "Not enough suits detected.\r\n";
                    Plugin.Log.LogInfo($"allsuits count too low");
                    return displayText;
                }

                
            }

            public static void AddDuplicateRenderObjects() //used by overlay and minimap
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
                        rawImageGameObject1.name = "Terminal Small Screen (Clone)";
                        rawImageGameObject2.name = "Terminal Full Screen (Clone)";

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
                    if (node.terminalEvent == "alwayson")
                    {
                        //toggle keeping display always on here
                        if (!Terminal_Awake_Patch.alwaysOnDisplay && ConfigSettings.networkedNodes.Value && ConfigSettings.ModNetworking.Value)
                        {
                            NetHandler.Instance.alwaysOnServerRpc(true);
                            node.displayText = $"Terminal Always-on Display [ENABLED]\r\n";
                            //Plugin.Log.LogInfo("set alwaysondisplay to true");
                        }
                        else if (Terminal_Awake_Patch.alwaysOnDisplay && ConfigSettings.networkedNodes.Value && ConfigSettings.ModNetworking.Value)
                        {
                            NetHandler.Instance.alwaysOnServerRpc(false);
                            node.displayText = $"Terminal Always-on Display [DISABLED]\r\n";
                            //Plugin.Log.LogInfo("set alwaysondisplay to false");
                        }
                        else if (!Terminal_Awake_Patch.alwaysOnDisplay && !ConfigSettings.networkedNodes.Value)
                        {
                            Terminal_Awake_Patch.alwaysOnDisplay = true;
                            node.displayText = $"Terminal Always-on Display [ENABLED]\r\n";
                        }
                        else if (Terminal_Awake_Patch.alwaysOnDisplay && !ConfigSettings.networkedNodes.Value)
                        {
                            Terminal_Awake_Patch.alwaysOnDisplay = false;
                            node.displayText = $"Terminal Always-on Display [DISABLED]\r\n";
                        }
                        else
                        {
                            Plugin.Log.LogInfo("report this as a bug with alwayson please");
                            node.displayText = "alwayson failed to initiate, report this as a bug please.";
                        }
                                

                    }
                    if (node.terminalEvent == "quit")
                    {

                        string text = $"{ConfigSettings.quitString.Value}\n";
                        node.displayText = text;

                        //Debug.Log("Start of the coroutine");

                        // Delay for 1 second
                        yield return new WaitForSeconds(0.5f);

                        //Debug.Log("After 1 second");

                        // Now, call QuitTerminal on the original instance
                        __instance.QuitTerminal();
                    }

                    if (node.terminalEvent == "randomsuit")
                    {
                        string suitString;
                        getRandomSuit(out suitString);
                        node.displayText = suitString;

                    }

                    if (node.terminalEvent == "leverdo")
                    {
                        
                        string getLevelName = StartOfRound.Instance.currentLevel.PlanetName;
                        if (!GameNetworkManager.Instance.gameHasStarted && !StartOfRound.Instance.travellingToNewLevel)
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
                                Plugin.Log.LogError("StartMatchLever instance not found!");
                            }
                        }
                        else if (StartOfRound.Instance.travellingToNewLevel)
                        {
                            node.displayText = $"We have not yet arrived to {getLevelName}, please wait.\r\n";
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
                                Plugin.Log.LogError("StartMatchLever instance not found!");
                            }
                        }
                        else
                        {
                            node.displayText = "Cannot pull the lever at this time.\r\n\r\nNOTE:If game has not been started, only the host can do this.\r\n\r\n";
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
                        fixVideoPatch.sanityCheckLOL = true;
                        Plugin.Log.LogInfo("start of lolevent");
                        checkForSplitView("neither"); //disables split view components if enabled

                        RawImage termRawImage = GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer/ImageContainer/Image (1)").GetComponent<RawImage>();
                        VideoPlayer termVP = GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer/ImageContainer/Image (1)").GetComponent <VideoPlayer>();

                        
                        if(!isVideoPlaying)
                        {
                            Plugin.Log.LogInfo("video not playing, running lolevents");
                            // Play the next video if not playing
                            if (VideoManager.Videos.Count > 0)
                            {
                                int randomIndex;
                                // Generate a random index that is not the same as the last played index
                                do
                                {
                                    randomIndex = Random.Range(0, VideoManager.Videos.Count);
                                } while (randomIndex == lastPlayedIndex);

                                lastPlayedIndex = randomIndex;


                                Plugin.Log.LogInfo($"Random Clip: {randomIndex} - {VideoManager.Videos[randomIndex]}");

                                // Set the URL for the random video
                                termVP.Stop(); //stop for setup
                                __instance.terminalAudio.Stop(); //fix audio


                                termVP.clip = null;
                                termVP.url = "file://" + VideoManager.Videos[randomIndex];
                                Plugin.Log.LogInfo("URL:" + termVP.url);



                                termVP.renderMode = VideoRenderMode.RenderTexture;
                                termVP.aspectRatio = VideoAspectRatio.Stretch;
                                //additionalVideoPlayer.transform.localScale = new Vector3(350f, 350f);
                                //additionalVideoPlayer.transform.localPosition = new Vector3(0f, 0f);
                                termVP.isLooping = false;
                                termVP.playOnAwake = false;



                                __instance.terminalImage.texture = __instance.videoTexture;

                                termVP.targetTexture = __instance.videoTexture;

                                termVP.audioOutputMode = VideoAudioOutputMode.AudioSource;
                                termVP.controlledAudioTrackCount = 1; //need this to get audio to work?

                                termVP.SetTargetAudioSource(0, __instance.terminalAudio);
                                //Plugin.Log.LogInfo($"Is AudioTrack 0 enabled? {termVP.IsAudioTrackEnabled(0)}");
                                /*if (!termVP.IsAudioTrackEnabled(0))
                                {
                                    Plugin.Log.LogInfo("attempting to set to true");
                                    termVP.EnableAudioTrack(0, true);
                                    Plugin.Log.LogInfo($"Is AudioTrack 0 enabled? {termVP.IsAudioTrackEnabled(0)}");
                                }*/


                                termVP.source = VideoSource.Url;
                            }
                            else
                            {
                                Plugin.Log.LogError("No videos found.");
                            }

                            node.clearPreviousText = true;
                            node.displayText = $"{ConfigSettings.lolStartString.Value}\n";
                            yield break;
                        }
                        else if (isVideoPlaying)
                        {
                            Plugin.Log.LogInfo("video detected playing, trying to stop it");
                            fixVideoPatch.OnVideoEnd(__instance.videoPlayer, __instance);
                            node.displayText = $"{ConfigSettings.lolStopString.Value}\n";
                            Plugin.Log.LogInfo("lol stop detected");
                            yield break;
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
                            //thanks chatgpt for the breakdown

                            // Determine the button name based on the hangar doors state
                            string buttonName = StartOfRound.Instance.hangarDoorsClosed ? "StartButton" : "StopButton";

                            // Find the corresponding button GameObject
                            GameObject buttonObject = GameObject.Find(buttonName);

                            // Get the InteractTrigger component from the button
                            InteractTrigger interactTrigger = buttonObject.GetComponentInChildren<InteractTrigger>();

                            // Determine the action based on the hangar doors state
                            string action = StartOfRound.Instance.hangarDoorsClosed ? "opened" : "closed";

                            // Log the door state
                            Plugin.Log.LogInfo($"Hangar doors are {action}.");

                            // Invoke the onInteract event if the button and event are found
                            if (interactTrigger != null)
                            {
                                UnityEvent<PlayerControllerB> onInteractEvent = interactTrigger.onInteract as UnityEvent<PlayerControllerB>;

                                if (onInteractEvent != null)
                                {
                                    onInteractEvent.Invoke(GameNetworkManager.Instance.localPlayerController);

                                    // Log individual messages for open and close events
                                    if (action == "opened")
                                    {
                                        node.displayText = $"{ConfigSettings.doorOpenString.Value}\n";
                                        Plugin.Log.LogInfo($"Hangar doors {action} successfully by interacting with button {buttonName}.");
                                    }
                                    else if (action == "closed")
                                    {
                                        node.displayText = $"{ConfigSettings.doorCloseString.Value}\n";
                                        Plugin.Log.LogInfo($"Hangar doors {action} successfully by interacting with button {buttonName}.");
                                    }
                                }
                                else
                                {
                                    // Log if onInteractEvent is null
                                    Plugin.Log.LogWarning($"Warning: onInteract event is null for button {buttonName}.");
                                }
                            }
                            else
                            {
                                // Log if interactTrigger is null
                                Plugin.Log.LogWarning($"Warning: InteractTrigger not found on button {buttonName}.");
                            }   
                        }
                        else
                        {
                            node.displayText = $"{ConfigSettings.doorSpaceString.Value}\n";
                        }
                    }

                    if (node.terminalEvent == "lights")
                    {
                        StartOfRound.Instance.shipRoomLights.ToggleShipLights();
                        if(StartOfRound.Instance.shipRoomLights.areLightsOn)
                            node.displayText = $"Ship Lights are [ON]\r\n\r\n";
                        else
                            node.displayText = $"Ship Lights are [OFF]\r\n\r\n";
                    }


                    if (node.terminalEvent == "shipLightsColor")
                    {
                        Color FrontColor = Color.red;
                        Color MiddleColor = Color.yellow;
                        Color BackColor = Color.green;
                        GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)").GetComponent<Light>().color = FrontColor;
                        GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)").GetComponent<Light>().color = MiddleColor;
                        GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)").GetComponent<Light>().color = BackColor;

                        Plugin.Log.LogInfo("Colors set");

                    }

                    if (node.terminalEvent == "enemies")
                    {
                        if (RoundManager.Instance != null)
                        {
                            //Plugin.Log.LogInfo("getting enemies count");
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

                    if (node.terminalEvent == "switchCamera")
                    {
                        Plugin.Log.LogInfo("Running darmuh's switch patch stuff");
                        Plugin.Log.LogInfo($"Map: {Plugin.instance.isOnMap} Cams: {Plugin.instance.isOnCamera} MiniMap: {Plugin.instance.isOnMiniMap} MiniCams: {Plugin.instance.isOnMiniCams} Overlay: {Plugin.instance.isOnOverlay}");
                        TerminalNode pvNode = CreateTerminalNode("go back to cams\n", true, "minimap");
                        TerminalNode mcNode = CreateTerminalNode("go back to minicams\n", true, "minicams");
                        TerminalNode ovNode = CreateTerminalNode("go back to cams\n", true, "overlay");
                        TerminalNode camsNode = CreateTerminalNode("go back to cams\n", true, "cams");
                        TerminalNode mapNode = CreateTerminalNode("go back to map\n", true, "mapEvent");
                        string playerName = "";
                        string displayText1 = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched to ";
                        string displayText2 = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched target ";
                        node.displayText = "";
                        if (Plugin.instance.switchTarget != string.Empty)
                        {
                            for (int i = 0; i < StartOfRound.Instance.mapScreen.radarTargets.Count; i++)
                            {
                                if(StartOfRound.Instance.mapScreen.radarTargets[i].name.Contains(Plugin.instance.switchTarget))
                                {
                                    Plugin.Log.LogInfo("name match found");
                                    playerName = StartOfRound.Instance.mapScreen.radarTargets[i].name;
                                    break;
                                }
                            }
                            
                        }
                        isVideoPlaying = false;

                        if (Plugin.instance.isOnCamera)
                        {
                            CamsMethods.HandleCams(__instance, camsNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handlecams");
                            yield break;
                        }
                        else if (Plugin.instance.isOnMap)
                        {
                            CamsMethods.HandleMap(__instance, mapNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handlemap");
                            yield break;
                        }
                        else if (Plugin.instance.isOnOverlay)
                        {
                            CamsMethods.HandleOverlay(__instance, ovNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handleover");
                            yield break;
                        }
                        else if (Plugin.instance.isOnMiniMap)
                        {
                            CamsMethods.HandleMiniMap(__instance, pvNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handleminimap");
                            yield break;
                        }
                        else if (Plugin.instance.isOnMiniCams)
                        {
                            CamsMethods.HandleMiniCams(__instance, mcNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handleminicams");
                            yield break;
                        }
                        else if (!Plugin.instance.isOnMap && !Plugin.instance.isOnCamera && !Plugin.instance.isOnMiniMap && !Plugin.instance.isOnMiniCams && !Plugin.instance.isOnOverlay)
                        {
                            CamsMethods.HandleNothingActive(__instance, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handledefault");
                            yield break;
                        }
                        else
                        {
                            Plugin.Log.LogInfo("somethin fucky goin on");
                        }
                    }

                    if (node.terminalEvent == "returnCams")
                    {
                        Plugin.Log.LogInfo($"Map: {Plugin.instance.isOnMap} Cams: {Plugin.instance.isOnCamera} MiniMap: {Plugin.instance.isOnMiniMap} MiniCams: {Plugin.instance.isOnMiniCams} Overlay: {Plugin.instance.isOnOverlay}");
                        TerminalNode pvNode = CreateTerminalNode("go back to minimap\n", true, "minimap");
                        TerminalNode mcNode = CreateTerminalNode("go back to minicams\n", true, "minicams");
                        TerminalNode ovNode = CreateTerminalNode("go back to overlay\n", true, "overlay");
                        TerminalNode camsNode = CreateTerminalNode("go back to cams\n", true, "cams");
                        TerminalNode mapNode = CreateTerminalNode("go back to map\n", true, "mapEvent");
                        string playerName = String.Empty;
                        string displayText1 = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: ";
                        string displayText2 = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring... ";

                        if (Plugin.instance.switchTarget != string.Empty)
                        {
                            for (int i = 0; i < StartOfRound.Instance.mapScreen.radarTargets.Count; i++)
                            {
                                if (StartOfRound.Instance.mapScreen.radarTargets[i].name.Contains(Plugin.instance.switchTarget))
                                {
                                    Plugin.Log.LogInfo("name match found");
                                    playerName = StartOfRound.Instance.mapScreen.radarTargets[i].name;
                                    break;
                                }
                            }

                        }
                        else
                        {
                            string playerNameText = StartOfRound.Instance.mapScreenPlayerName.text;
                            string removeText = "MONITORING: ";
                            playerName = playerNameText.Remove(0, removeText.Length);
                        }

                        
                        node.name = "ViewInsideShipCam 1";
                        isVideoPlaying = false;

                        if (Plugin.instance.isOnCamera)
                        {
                            CamsMethods.HandleCams(__instance, camsNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handlecams");
                            yield break;
                        }
                        else if (Plugin.instance.isOnMap)
                        {
                            CamsMethods.HandleMap(__instance, mapNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handlemap");
                            yield break;
                        }
                        else if (Plugin.instance.isOnOverlay)
                        {
                            CamsMethods.HandleOverlay(__instance, ovNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handleover");
                            yield break;
                        }
                        else if (Plugin.instance.isOnMiniMap)
                        {
                            CamsMethods.HandleMiniMap(__instance, pvNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handleminimap");
                            yield break;
                        }
                        else if (Plugin.instance.isOnMiniCams)
                        {
                            CamsMethods.HandleMiniCams(__instance, mcNode, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handleminicams");
                            yield break;
                        }
                        else if (!Plugin.instance.isOnMap && !Plugin.instance.isOnCamera && !Plugin.instance.isOnMiniMap && !Plugin.instance.isOnMiniCams && !Plugin.instance.isOnOverlay)
                        {
                            CamsMethods.HandleNothingActive(__instance, playerName, displayText1, displayText2);
                            Plugin.Log.LogInfo("handledefault");
                            yield break;
                        }
                        else
                        {
                            Plugin.Log.LogInfo("somethin fucky goin on");
                        }
                    }

                    if (node.terminalEvent == "flashlight")
                    {
                        string playerName = GameNetworkManager.Instance.localPlayerController.playerUsername;
                        ulong playerID = GameNetworkManager.Instance.localPlayerController.playerClientId;
                        string colorName = Terminal_ParsePlayerSentence_Patch.flashLightColor;

                        Plugin.Log.LogInfo($"{playerName} trying to set color {colorName} to flashlight");
                        Color flashlightColor = Terminal_ParsePlayerSentence_Patch.FlashlightColor ?? Color.white; // Use white as a default color
                        Plugin.Log.LogInfo($"got {colorName} - {flashlightColor}");

                        NetHandler.Instance.FlashColorServerRpc(flashlightColor, colorName, playerID, playerName);
                        if (Plugin.instance.fSuccess && Plugin.instance.hSuccess)
                        {
                            node.displayText = $"Flashlight Color set to {colorName}.\r\nHelmet Light Color set to {colorName}.\r\n";
                            Plugin.instance.fSuccess = false;
                            Plugin.instance.hSuccess = false;
                            yield break;
                        }
                        else if (Plugin.instance.fSuccess && !Plugin.instance.hSuccess)
                        {
                            node.displayText = $"Flashlight Color set to {colorName}.\r\nUnable to set Helmet Light Color.\r\n";
                            Plugin.instance.fSuccess = false;
                            Plugin.instance.hSuccess = false;
                            yield break;
                        }
                        else
                        {
                            node.displayText = "Cannot set flashlight color.\r\n\r\nEnsure you have equipped a flashlight before using this command.\r\n";
                        }
                    }


                    if (node.terminalEvent == "teleport")
                    {
                        ShipTeleporter[] objectsOfType = Object.FindObjectsOfType<ShipTeleporter>();
                        ShipTeleporter tp = (ShipTeleporter)null;
                        foreach (ShipTeleporter normaltp in objectsOfType)
                        {
                            if (!normaltp.isInverseTeleporter)
                            {
                                tp = normaltp;
                                break;
                            }
                        }
                        if ((Object)tp != (Object)null)
                        {
                            float cooldownTime = tp.cooldownTime;
                            if (Mathf.Round(cooldownTime) == 0 && tp.buttonTrigger.interactable)
                            {
                                tp.PressTeleportButtonOnLocalClient();
                                node.displayText = $"{ConfigSettings.tpMessageString.Value}\n";
                            }
                            else node.displayText = $"Teleporter has {Mathf.Round(cooldownTime)} seconds remaining on cooldown.\r\n";
                        }
                        else node.displayText = "Can't teleport at this time.\n Do you even have a teleporter?\n";
                    }
                    if (node.terminalEvent == "inversetp")
                    {
                        ShipTeleporter[] objectsOfType = Object.FindObjectsOfType<ShipTeleporter>();
                        ShipTeleporter tp = (ShipTeleporter)null;
                        foreach (ShipTeleporter inversetp in objectsOfType)
                        {
                            if (inversetp.isInverseTeleporter)
                            {
                                tp = inversetp;
                                break;
                            }
                        }
                        if ((Object)tp != (Object)null)
                        {
                            float cooldownTime = tp.cooldownTime;
                            if (!(StartOfRound.Instance.inShipPhase) && tp.buttonTrigger.interactable)
                            {
                                tp.PressTeleportButtonOnLocalClient();
                                node.displayText = $"{ConfigSettings.itpMessageString.Value}\n";
                            }
                            else if (Mathf.Round(cooldownTime) > 0)
                            {
                                node.displayText = $"Inverse Teleporter has {Mathf.Round(cooldownTime)} seconds remaining on cooldown.\r\n";
                            }
                            else
                            {
                                node.displayText = $"Can't Inverse Teleport from space...\r\n"; //test

                            }

                            
                        }
                        else node.displayText = "Can't Inverse Teleport at this time.\n Do you even have an Inverse Teleporter?\n";
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
                        if (getPlayerInfo != null && (getCreds >= costCreds || vitalsUpgradeEnabled)) //checks if you can spend enough
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
                            Plugin.Log.LogInfo("Invalid percentage value. Telling user.");
                            node.displayText = ("Invalid gamble percentage, please input a value between 0 and 100.\n");
                        }
                        if (__instance.groupCredits <= ConfigSettings.gambleMinimum.Value)
                        {
                            // Handle the case when groupCredits is lower than minimum required
                            Plugin.Log.LogInfo("Invalid percentage value. Telling user.");
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
                        int getPlayerHealth = GameNetworkManager.Instance.localPlayerController.health;
                        //this code snippet is slightly modified from Octolar's Healing Mod, credit to them
                        if (getPlayerHealth >= 100)
                        {
                            Plugin.Log.LogInfo($"Health = {getPlayerHealth}");
                            node.displayText = $"{ConfigSettings.healIsFullString.Value}\n";
                        }
                            
                        else
                        {
                            Plugin.Log.LogInfo($"Health before = {getPlayerHealth}");
                            GameNetworkManager.Instance.localPlayerController.DamagePlayer(-100, false, true);
                            GameNetworkManager.Instance.localPlayerController.MakeCriticallyInjured(false);
                            int getNewHealth = GameNetworkManager.Instance.localPlayerController.health;
                            node.displayText = $"{ConfigSettings.healString.Value}\nHealth: {GameNetworkManager.Instance.localPlayerController.health.ToString()}\r\n";
                            Plugin.Log.LogInfo($"Health now = {getNewHealth}");
                        }
                    }
                    if (node.terminalEvent == "mapEvent")
                    {
                        isVideoPlaying = false;
                        string playerNameText = StartOfRound.Instance.mapScreenPlayerName.text;
                        
                        string removeText = "MONITORING: ";
                        string playerName = playerNameText.Remove(0, removeText.Length);
                        node.name = "ViewInsideShipCam 1";
                        if (RoundManager.Instance != null && RoundManager.Instance.hasInitializedLevelRandomSeed)
                        {
                            if (Plugin.instance.isOnMap == false)
                            {
                                node.displayTexture = null; //hiding vanilla radar
                                node.clearPreviousText = true;

                                Plugin.instance.isOnOverlay = false;
                                Plugin.instance.isOnCamera = false;
                                Plugin.instance.isOnMiniCams = false;
                                Plugin.instance.isOnMiniMap = false;
                                __instance.terminalImage.enabled = true;
                                Texture renderTexture = StartOfRound.Instance.mapScreen.onScreenMat.mainTexture;
                                //Texture renderTexture = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[1].mainTexture;
                                Plugin.instance.rawImage2.texture = renderTexture; //replaced node.displayTexture
                                Plugin.instance.rawImage2.enabled = true;

                                enabledSplitObjects = true;
                                checkForSplitView("map");
                                Plugin.Log.LogInfo("map radar enabled");
                                if (playerName != String.Empty)
                                {
                                    node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} {ConfigSettings.mapString.Value}\n";
                                }
                                else
                                {
                                    node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring... {ConfigSettings.mapString.Value}\n";
                                }
                            }
                            else if (Plugin.instance.isOnMap == true)
                            {
                                node.clearPreviousText = true;
                                //Plugin.Log.LogInfo("disabling map"); //debug
                                enabledSplitObjects = false;
                                checkForSplitView("map"); //disables split view if enabled
                                node.displayText = $"{ConfigSettings.mapString2.Value}\r\n";
                                //endMapCommand = true;
                            }
                            else
                                Plugin.Log.LogError("Map command ERROR, isOnMap neither true nor false!!!");
                        }
                        else
                        {
                            node.displayTexture = null; //hiding vanilla radar
                            Plugin.instance.rawImage2.texture = null;
                            Plugin.instance.rawImage2.enabled = false;
                            Plugin.Log.LogInfo("this should only trigger in orbit");
                            node.clearPreviousText = true;
                            node.loadImageSlowly = false;
                            node.displayText = "Radar view not available in orbit.\r\n";
                            Plugin.instance.isOnMap = false;
                            Plugin.instance.isOnCamera = false;
                            Plugin.instance.isOnOverlay = false;
                            Plugin.instance.isOnMiniMap = false;
                        }
                    }

                    if (node.terminalEvent == "cams")
                    {
                        isVideoPlaying = false;
                        node.name = "ViewInsideShipCam 1";
                        string playerNameText = StartOfRound.Instance.mapScreenPlayerName.text;
                        string removeText = "MONITORING: ";
                        string playerName = playerNameText.Remove(0, removeText.Length);
                        if (GameObject.Find("Environment/HangarShip/Cameras/ShipCamera") != null && Plugin.instance.isOnCamera == false && Plugin.instance.splitViewCreated)
                        {
                            node.displayTexture = null; //hiding vanilla radar
                            node.clearPreviousText = true;

                            Plugin.instance.isOnOverlay = false;
                            Plugin.instance.isOnMap = false;
                            Plugin.instance.isOnMiniCams = false;
                            Plugin.instance.isOnMiniMap = false;
                            __instance.terminalImage.enabled = true;
                            // Get the main texture from "Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001"
                            Texture renderTexture = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture;
                            Plugin.instance.rawImage2.texture = renderTexture; //replaced node.displayTexture
                            Plugin.instance.rawImage2.enabled = true;

                            enabledSplitObjects = true;
                            checkForSplitView("cams");
                            //isOnCamera = true;
                            Plugin.Log.LogInfo("cam added to terminal screen");
                            if (playerName != String.Empty)
                            {
                                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} {ConfigSettings.camString.Value}\n";
                            }
                            else
                            {
                                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring... {ConfigSettings.camString.Value}\n";
                            }
                        }
                        else if (Plugin.instance.isOnCamera == true)
                        {
                            node.clearPreviousText = true;
                            //Plugin.Log.LogInfo("disabling map & disabling cams"); //debug
                            enabledSplitObjects = false;
                            checkForSplitView("cams"); //disables split view if enabled
                            node.displayText = $"{ConfigSettings.camString2.Value}\r\n";
                            Plugin.Log.LogInfo("cams removed");
                        }
                        else
                        {
                            Plugin.Log.LogInfo("Unable to run cameras event for some reason...");
                        }
                    }

                    if (node.terminalEvent == "overlay")
                    {
                        isVideoPlaying = false;
                        node.clearPreviousText = true;
                        node.name = "ViewInsideShipCam 1";
                        string playerNameText = StartOfRound.Instance.mapScreenPlayerName.text;
                        string removeText = "MONITORING: ";
                        string playerName = playerNameText.Remove(0, removeText.Length);
                        float opacityConfig = ConfigSettings.ovOpacity.Value / 100f;
                        Plugin.Log.LogInfo($"Overlay Opacity: {opacityConfig}");
                        Plugin.instance.isOnMap = false;
                        Plugin.instance.isOnCamera = false;
                        Plugin.instance.isOnMiniCams = false;
                        Plugin.instance.isOnMiniMap = false;
                        Texture texture1 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[1].mainTexture; // radar
                        Texture texture2 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture; // cams


                        if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnOverlay == false )
                        {
                            Plugin.instance.rawImage2.texture = texture2;
                            Plugin.instance.rawImage1.texture = texture1;
                            __instance.terminalImage.enabled = true;
                            Color currentColor = Plugin.instance.rawImage1.color;
                            Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, opacityConfig); 
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

                            if (playerName != String.Empty)
                            {
                                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} {ConfigSettings.ovString.Value}\r\n";
                            }
                            else
                            {
                                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring... {ConfigSettings.ovString.Value}\n";
                            }
                            
                        }
                        else if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnOverlay == true)
                        {
                            enabledSplitObjects = false;
                            checkForSplitView("overlay"); //disables split view if enabled
                            node.displayText = $"{ConfigSettings.ovString2.Value}\r\n";
                        }
                        else
                        {
                            Plugin.Log.LogError("Unexpected condition");
                        }
                    }

                    if (node.terminalEvent == "minimap")
                    {
                        isVideoPlaying = false;
                        node.name = "ViewInsideShipCam 1";
                        string playerNameText = StartOfRound.Instance.mapScreenPlayerName.text;
                        string removeText = "MONITORING: ";
                        string playerName = playerNameText.Remove(0, removeText.Length);
                        node.clearPreviousText = true;
                        Plugin.instance.isOnMiniCams = false;
                        Plugin.instance.isOnMap = false;
                        Plugin.instance.isOnCamera = false;
                        Plugin.instance.isOnOverlay = false;
                        Texture texture1 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[1].mainTexture; // radar
                        Texture texture2 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture; // cams

                        if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnMiniMap == false )
                        {
                            Plugin.instance.rawImage2.texture = texture2;
                            Plugin.instance.rawImage1.texture = texture1;

                            __instance.terminalImage.enabled = true;
                            Color currentColor = Plugin.instance.rawImage1.color;
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
                            checkForSplitView("minimap");
                            Plugin.instance.isOnMiniMap = true;
                            if (playerName != String.Empty)
                            {
                                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} {ConfigSettings.mmString.Value}\r\n";
                            }
                            else
                            {
                                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring... {ConfigSettings.mmString.Value}\n";
                            }
                        }
                        else if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnMiniMap == true)
                        {
                            enabledSplitObjects = false;
                            checkForSplitView("minimap"); //disables split view if enabled
                            node.displayText = $"{ConfigSettings.mmString2.Value}\r\n";
                        }
                        else
                        {
                            Plugin.Log.LogError("Unexpected condition");
                        }
                    }

                    if (node.terminalEvent == "minicams")
                    {
                        isVideoPlaying = false;
                        node.name = "ViewInsideShipCam 1";
                        node.clearPreviousText = true;
                        string playerNameText = StartOfRound.Instance.mapScreenPlayerName.text;
                        string removeText = "MONITORING: ";
                        string playerName = playerNameText.Remove(0, removeText.Length);
                        Plugin.instance.isOnMiniMap = false;
                        Plugin.instance.isOnMap = false;
                        Plugin.instance.isOnCamera = false;
                        Plugin.instance.isOnOverlay = false;
                        Texture texture2 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[1].mainTexture; // radar
                        Texture texture1 = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture; // cams

                        if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnMiniCams == false)
                        {
                            Plugin.instance.rawImage2.texture = texture2;
                            Plugin.instance.rawImage1.texture = texture1;

                            __instance.terminalImage.enabled = true;
                            Color currentColor = Plugin.instance.rawImage1.color;
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
                            checkForSplitView("minicams");
                            Plugin.instance.isOnMiniCams = true;
                            if (playerName != String.Empty)
                            {
                                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} {ConfigSettings.mcString.Value}\r\n";
                            }
                            else
                            {
                                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring... {ConfigSettings.mcString.Value}\n";
                            }
                            
                        }
                        else if (Plugin.instance.splitViewCreated == true && Plugin.instance.isOnMiniCams == true)
                        {
                            enabledSplitObjects = false;
                            checkForSplitView("minicams"); //disables split view if enabled
                            node.displayText = $"{ConfigSettings.mcString2.Value}\r\n";
                        }
                        else
                        {
                            Plugin.Log.LogError("Unexpected condition");
                        }
                    }

                    if (node.terminalEvent == "externalLink")
                    {
                        Application.OpenURL(ConfigSettings.customLink.Value);
                        yield return new WaitForSeconds(0.5f);
                        __instance.QuitTerminal();
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
                            yield return new WaitForSeconds(0.5f);

                            __instance.QuitTerminal();
                            number = Mathf.Clamp(number, 66f, 130f);
                            PlayerControllerBPatches.newTargetFovBase = number;
                            PlayerControllerBPatches.calculateVisorStuff();
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

        public static void checkForSplitView( string whatisit )
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
                Plugin.instance.isOnMiniMap = false;
                Plugin.instance.isOnMiniCams = false;
                Plugin.instance.isOnCamera = false;
                Plugin.instance.isOnMap = false;
            }
            else if (enabledSplitObjects == true && whatisit != "cams" && whatisit != "map")
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
                if(whatisit == "minimap")
                {
                    Plugin.instance.isOnMiniMap = true;
                    Plugin.instance.isOnMiniCams = false;
                    Plugin.instance.isOnOverlay = false;
                    Plugin.instance.isOnCamera = false;
                    Plugin.instance.isOnMap = false; ;
                }
                else if (whatisit == "minicams")
                {
                    Plugin.instance.isOnMiniMap = false;
                    Plugin.instance.isOnMiniCams = true;
                    Plugin.instance.isOnOverlay = false;
                    Plugin.instance.isOnCamera = false;
                    Plugin.instance.isOnMap = false;
                }
                else if(whatisit == "overlay")
                {
                    Plugin.instance.isOnOverlay = true;
                    Plugin.instance.isOnMiniCams = false;
                    Plugin.instance.isOnMiniMap = false;
                    Plugin.instance.isOnCamera = false;
                    Plugin.instance.isOnMap = false;
                }

            }
            else if (enabledSplitObjects == true && (whatisit == "cams" || whatisit == "map"))
            {
                //Terminal Full Screen (Clone)
                // Assuming you have a reference to the ImageContainer GameObject
                GameObject imageContainer = GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer/ImageContainer");

                // Check if the imageContainer is not null
                if (imageContainer != null)
                {
                    // Disable specific children based on their names
                    foreach (Transform child in imageContainer.transform)
                    {
                        if (child.gameObject.name.Equals("Terminal Full Screen (Clone)"))
                        {
                            child.gameObject.SetActive(true); //only set one active
                            Plugin.Log.LogInfo("enabled full screen");
                        }
                        if (child.gameObject.name.Equals("Terminal Small Screen (Clone)"))
                        {
                            child.gameObject.SetActive(false); //only set one active
                            Plugin.Log.LogInfo("disabled small screen");
                        }
                    }
                }
                // Update bool
                enabledSplitObjects = false;
                // Update bools
                if (whatisit == "cams")
                {
                    Plugin.instance.isOnCamera = true;
                    Plugin.instance.isOnMap = false;
                    Plugin.instance.isOnOverlay = false;
                    Plugin.instance.isOnMiniCams = false;
                    Plugin.instance.isOnMiniMap = false;
                }
                else if (whatisit == "map")
                {
                    Plugin.instance.isOnCamera = false;
                    Plugin.instance.isOnMap = true;
                    Plugin.instance.isOnOverlay = false;
                    Plugin.instance.isOnMiniCams = false;
                    Plugin.instance.isOnMiniMap = false;
                }
                else
                    Plugin.Log.LogInfo("whatisit is neither cams or map (this shouldn't happen lol)");
                
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

        public static void AddMiniMap()
        {
            TerminalNode splitNode = CreateTerminalNode("testing split mode\n", true, "minimap");
            TerminalKeyword Keyword2 = CreateTerminalKeyword(ConfigSettings.minimapKeyword.Value, true, splitNode);
            AddTerminalKeyword(Keyword2);
            splitNode.name = "ViewInsideShipCam 1";
            //Plugin.Log.LogInfo("added minimap keywords");
        }
        public static void AddMiniCams()
        {
            TerminalNode splitNode = CreateTerminalNode("testing split mode\n", true, "minicams");
            TerminalKeyword Keyword2 = CreateTerminalKeyword(ConfigSettings.minicamsKeyword.Value, true, splitNode);
            AddTerminalKeyword(Keyword2);
            //Plugin.Log.LogInfo("added minimap keywords");
            splitNode.name = "ViewInsideShipCam 1";
        }

        public static void AddOverlayView()
        {
            TerminalNode splitNode = CreateTerminalNode("testing split mode\n", true, "overlay");
            TerminalKeyword splitKeyword = CreateTerminalKeyword(ConfigSettings.overlayKeyword.Value, true, splitNode);
            AddTerminalKeyword(splitKeyword);
            //Plugin.Log.LogInfo("added overlay keyword");
            splitNode.name = "ViewInsideShipCam 1";
        }
        public static void AddDoor()
        {
            TerminalNode node = CreateTerminalNode("door terminalEvent", false, "door");
            TerminalKeyword doorKW = CreateTerminalKeyword(ConfigSettings.doorKeyword.Value, true, node);
            AddTerminalKeyword(doorKW);
            //Plugin.Log.LogInfo($"Door keyword added");
            node.name = "Toggle Doors";
        }

        public static void AddLights()
        {
            TerminalNode node = CreateTerminalNode("lights terminalEvent", false, "lights");
            TerminalKeyword lightsKW = CreateTerminalKeyword(ConfigSettings.lightsKeyword.Value, true, node);
            AddTerminalKeyword(lightsKW);
            //Plugin.Log.LogInfo($"Lights keyword added");
            node.name = "Toggle Lights";
        }

        public static void AddTest()
        {
            TerminalNode test = CreateTerminalNode("test\n", true, "randomsuit");
            TerminalKeyword testKeyword = CreateTerminalKeyword("test", true, test);
            AddTerminalKeyword(testKeyword);
            Plugin.Log.LogInfo("This should only be enabled for dev testing");
        }

        public static void AddRandomSuit()
        {
            TerminalNode node = CreateTerminalNode("randomsuit terminalEvent\n", true, "randomsuit");
            TerminalKeyword nodeKeyword = CreateTerminalKeyword(ConfigSettings.randomSuitKeyword.Value, true, node);
            AddTerminalKeyword(nodeKeyword);
            node.name = "RandomSuit";
        }

        public static void AddAlwaysOnKeywords()
        {
            TerminalNode aoNode = CreateTerminalNode("", true, "alwayson");
            TerminalKeyword aoKeyword = CreateTerminalKeyword(ConfigSettings.alwaysOnKeyword.Value, true, aoNode);
            AddTerminalKeyword(aoKeyword);
            //Plugin.Log.LogInfo("Added always on keyword");
            aoNode.name = "Always-On Display";
        }

        public static void AddModListKeywords()
        {
            TerminalNode modList = CreateTerminalNode("grabbing mods\n", true, "modlist");
            TerminalKeyword modlistKeyword = CreateTerminalKeyword(ConfigSettings.modsKeyword2.Value, true, modList);
            TerminalKeyword modsKeyword = CreateTerminalKeyword("mods", true, modList);
            AddTerminalKeyword(modlistKeyword);
            AddTerminalKeyword(modsKeyword);
            //Plugin.Log.LogInfo("Added Modlist keywords");
            modList.name = "ModList";
        }

        public static void AddTeleportKeywords()
        {
            TerminalNode tpNode = CreateTerminalNode("teleporter initiatied.\n", true, "teleport");
            TerminalKeyword teleportKeyword = CreateTerminalKeyword(ConfigSettings.tpKeyword2.Value, true, tpNode);
            TerminalKeyword tpKeyword = CreateTerminalKeyword("tp", true, tpNode);
            AddTerminalKeyword(teleportKeyword);
            AddTerminalKeyword(tpKeyword);
            //Plugin.Log.LogInfo("---------Teleport & TP Keywords added!---------");
            tpNode.name = "Use Teleporter";
        }

        public static void AddInverseTeleportKeywords()
        {
            TerminalNode tpNode = CreateTerminalNode("teleporter initiatied.\n", true, "inversetp");
            TerminalKeyword inverseteleportKeyword = CreateTerminalKeyword(ConfigSettings.itpKeyword2.Value, true, tpNode);
            TerminalKeyword itpKeyword = CreateTerminalKeyword("itp", true, tpNode);
            AddTerminalKeyword(inverseteleportKeyword);
            AddTerminalKeyword(itpKeyword);
            //Plugin.Log.LogInfo("---------Inverse & ITP Keywords added!---------");
            tpNode.name = "Use Inverse Teleporter";
        }

        public static void AddQuitKeywords()
        {
            TerminalNode quitNode = CreateTerminalNode("leaving.\n", true, "quit");
            TerminalKeyword exitKeyword = CreateTerminalKeyword(ConfigSettings.quitKeyword2.Value, true, quitNode);
            TerminalKeyword quitKeyword = CreateTerminalKeyword("quit", true, quitNode);
            AddTerminalKeyword(exitKeyword);
            AddTerminalKeyword(quitKeyword);
            //Plugin.Log.LogInfo("---------Quit & Exit Keywords added!---------");
            quitNode.name = "Quit Terminal";
        }
        public static void hampterKeywords()
        {
            TerminalNode lolNode = CreateTerminalNode($"lol.\n", false, "lolevent");
            TerminalKeyword lolKeyword = CreateTerminalKeyword(ConfigSettings.lolKeyword.Value, true, lolNode);
            //TerminalKeyword hampterKeyword = CreateTerminalKeyword("hampter", true, lolNode);
            //AddTerminalKeyword(hampterKeyword);
            AddTerminalKeyword(lolKeyword);
            //Plugin.Log.LogInfo("lol");
            lolNode.name = "darmuh's videoPlayer";
        }
        public static void clearKeywords()
        {
            TerminalNode clearNode = CreateTerminalNode($"\n", true);
            TerminalKeyword clearKeyword = CreateTerminalKeyword("clear", true, clearNode);
            TerminalKeyword clearKeyword2 = CreateTerminalKeyword(ConfigSettings.clearKeyword2.Value, true, clearNode);
            AddTerminalKeyword(clearKeyword);
            AddTerminalKeyword(clearKeyword2);
            //Plugin.Log.LogInfo("Adding Clear keywords");
            clearNode.name = "Clear Terminal Screen";
        }
        public static void dangerKeywords()
        {
            TerminalNode dangerNode = CreateTerminalNode($"\n", true, "danger");
            TerminalKeyword dangerKeyword = CreateTerminalKeyword(ConfigSettings.dangerKeyword.Value, true, dangerNode);
            AddTerminalKeyword(dangerKeyword);
            //Plugin.Log.LogInfo("Adding danger keywords");
            dangerNode.name = "Check Danger Level";
        }
        public static void vitalsKeywords()
        {
            TerminalNode vitalsNode = CreateTerminalNode($"\n", true, "vitals");
            TerminalKeyword vitalsKeyword = CreateTerminalKeyword("vitals", true, vitalsNode);
            AddTerminalKeyword(vitalsKeyword);
            //Plugin.Log.LogInfo("Adding vitals keywords");
            vitalsNode.name = "Check Vitals";
        }
        public static void healKeywords()
        {
            TerminalNode healNode = CreateTerminalNode($"\n", true, "healme");
            TerminalKeyword healKeyword = CreateTerminalKeyword("heal", true, healNode);
            TerminalKeyword healmeKeyword = CreateTerminalKeyword(ConfigSettings.healKeyword2.Value, true, healNode);
            AddTerminalKeyword(healKeyword);
            AddTerminalKeyword(healmeKeyword);
            //Plugin.Log.LogInfo("Added Heal Keywords");
            healNode.name = "HealFromTerminal";
        }
        public static void lootKeywords()
        {
            TerminalNode lootNode = CreateTerminalNode($"Attempting to grab total loot value on ship.\n", false, "loot");
            TerminalKeyword lootKeyword = CreateTerminalKeyword("loot", true, lootNode);
            TerminalKeyword shiplootKeyword = CreateTerminalKeyword(ConfigSettings.lootKeyword2.Value, true, lootNode);
            AddTerminalKeyword(lootKeyword);
            AddTerminalKeyword(shiplootKeyword);
            //Plugin.Log.LogInfo("Loot commands added!");
            lootNode.name = "Check Loot Value";
        }
        public static void camsKeywords()
        {
            TerminalNode camsNode = CreateTerminalNode($"Toggling Cameras View.\n", true, "cams");
            TerminalKeyword camsKeyword = CreateTerminalKeyword("cams", true, camsNode);
            TerminalKeyword camerasKeyword = CreateTerminalKeyword(ConfigSettings.camsKeyword2.Value, false, camsNode);
            AddTerminalKeyword(camsKeyword);
            AddTerminalKeyword(camerasKeyword);
            camsNode.name = "ViewInsideShipCam 1";
            //Plugin.Log.LogInfo("Cameras commands added!");
            //can't believe this was easier than displaying a custom video
        }
        public static void mapKeywords()
        {
            TerminalNode mapNode = CreateTerminalNode($"Toggling radar view.\n", true, "mapEvent");
            TerminalKeyword mapKeyword = CreateTerminalKeyword("map", true, mapNode);
            TerminalKeyword map2 = CreateTerminalKeyword(ConfigSettings.mapKeyword2.Value, true, mapNode);
            AddTerminalKeyword(mapKeyword);
            AddTerminalKeyword(map2);
            mapNode.name = "ViewInsideShipCam 1";
            //Plugin.Log.LogInfo("Map command added!");
        }
    }
}


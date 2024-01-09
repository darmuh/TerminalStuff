using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;
using static TerminalApi.TerminalApi;
using System.CodeDom;
using System.Collections;
using DunGen.Graph;
using static UnityEngine.GraphicsBuffer;
using Object = UnityEngine.Object;
using Steamworks;
using BepInEx.Configuration;
using AsmResolver.IO;

namespace TerminalStuff
{
    public class AllMyTerminalPatches
    {
        public static AllMyTerminalPatches Instance;

        //non-terminalAPI keywords
        public static string fColor;
        public static string Gamble;
        public static string Lever;
        public static string sColor;
        public static string Link;

        //LoadNewNode(TerminalNode node)
        [HarmonyPatch(typeof(Terminal), "LoadNewNode")]
        public class NodePatch : Terminal
        {
            static void Postfix(ref Terminal __instance, ref TerminalNode node)
            {
                
                if (Terminal_Awake_Patch.alwaysOnDisplay && NetHandler.netNodeSet == false && ConfigSettings.networkedNodes.Value)
                {
                    Plugin.Log.LogInfo("setting nodeset to true, syncing with other clients.");
                    NetHandler.netNodeSet = true;
                    NetHandler.Instance.nodeLoadServerRpc(node.name, node.terminalEvent);
                }

            }
        }

        [HarmonyPatch(typeof(Terminal), "QuitTerminal")]
        public class QuitPatch : Terminal
        {
            static void Postfix(ref Terminal __instance)
            {
                Terminal_Awake_Patch.isTermInUse = __instance.terminalInUse;
                //Plugin.Log.LogInfo($"terminuse set to {__instance.terminalInUse}");
                if(Terminal_Awake_Patch.alwaysOnDisplay)
                {
                    __instance.StartCoroutine(__instance.waitUntilFrameEndToSetActive(active: true));
                    Plugin.Log.LogInfo("Screen set to active");
                    if (LeaveTerminal.isVideoPlaying)
                    {
                        __instance.videoPlayer.Pause();
                        __instance.StartCoroutine(waitUntilFrameEndVideo(__instance));
                    }

                }
                    
            }

            private static IEnumerator waitUntilFrameEndVideo(Terminal instance)
            {
                yield return new WaitForEndOfFrame();
                if(LeaveTerminal.isVideoPlaying)
                    instance.videoPlayer.Play();
                Plugin.Log.LogInfo("attemtped to resume videoplayer");
            }

        }

        [HarmonyPatch(typeof(Terminal), "Start")]
        public class Terminal_Awake_Patch : Terminal
        {
            public static bool doesTPexist = false;
            public static bool doesITPexist = false;
            public static bool alwaysOnDisplay = false;
            public static bool isTermInUse = false;
            public static bool helpModified = false;
            //change vanilla terminal stuff here
            static void Postfix(ref Terminal __instance)
            {
                Plugin.Log.LogInfo("Upgrading terminal with my stuff, smile.");
                TerminalNode startNode = __instance.terminalNodes.specialNodes.ToArray()[1];
                TerminalNode helpNode = __instance.terminalNodes.specialNodes.ToArray()[13];
                if (!Plugin.instance.CompatibilityAC && !Plugin.instance.CompatibilityOther)
                    helpNode.displayText = ">MOONS\r\nList of moons the autopilot can route to.\r\n\r\n>STORE\r\nCompany store's selection of useful items.\r\n\r\n>BESTIARY\r\nTo see the list of wildlife on record.\r\n\r\n>STORAGE\r\nTo access objects placed into storage.\r\n\r\n>OTHER\r\nTo see the list of other commands\r\n\r\n>MORE\r\nTo see a list of commands added via darmuhsTerminalStuff\r\n\r\n[numberOfItemsOnRoute]\r\n"; //appended
                else if (Plugin.instance.CompatibilityAC)
                    alwaysOnDisplay = true;

                //no known compatibility issues with home screen
                startNode.displayText = $"{ConfigSettings.homeLine1.Value}\r\n{ConfigSettings.homeLine2.Value}\r\n\r\nType \"Help\" for a list of commands.\r\n\r\nType \"More\" for a list of darmuh's commands.\r\n\r\n     ._______.\r\n     | \\   / |\r\n  .--|.O.|.O.|______.\r\n__).-| = | = |/   \\ |\r\np__) (.'---`.)Q.|.Q.|--.\r\n      \\\\___// = | = |-.(__\r\n       `---'( .---. ) (__&lt;\r\n             \\\\.-.//\r\n              `---'\r\n\t\t\t  \r\n{ConfigSettings.homeLine3.Value}\r\n\r\n";
                doesTPexist = false;
                doesITPexist = false;
                isTermInUse = __instance.terminalInUse;
                StopPersistingKeywords();
                CreateSpecialNode(__instance);
               
                // Introduce a 3-second delay before calling checkForTPatStart
                Task.Run(() =>
                {
                    Thread.Sleep(3000);
                    checkForTPatStart();
                });
            }

            private static void CreateSpecialNode(Terminal instance)
            {
                TerminalNode camsNode = CreateTerminalNode("", true, "returnCams");
                camsNode.name = "ViewInsideShipCam 1";
                instance.terminalNodes.specialNodes.Add(camsNode);

            }


            private static void StopPersistingKeywords()
            {
                //deletes keywords at game start if they exist from previous plays

                DeleteKeyword("teleport");
                DeleteKeyword("tp");
                DeleteKeyword("itp");
                DeleteKeyword("inverse");
            }

            private static void checkForTPatStart()
            {
                //Add TP keywords ONLY if they have already been purchased and exist
                Plugin.Log.LogInfo("Checking for purchased Teleporter objects");
                ShipTeleporter[] objectsOfType = UnityEngine.Object.FindObjectsOfType<ShipTeleporter>();

                if (!doesTPexist && ConfigSettings.terminalTP.Value)
                {
                    ShipTeleporter tp = (ShipTeleporter)null;

                    foreach (ShipTeleporter tpobject in objectsOfType)
                    {
                        if (!tpobject.isInverseTeleporter)
                        {
                            tp = tpobject;
                            break;
                        }
                    }

                    if (tp != null)
                    {
                        doesTPexist = true;
                        LeaveTerminal.AddTeleportKeywords();
                    }
                    else
                    {
                        Plugin.Log.LogInfo("TP does not exist yet");
                    }
                }
                if (!doesITPexist && ConfigSettings.terminalITP.Value)
                {
                    ShipTeleporter itp = (ShipTeleporter)null;
                    foreach (ShipTeleporter tpobject in objectsOfType)
                    {
                        if (tpobject.isInverseTeleporter)
                        {
                            itp = tpobject;
                            break;
                        }
                    }

                    if (itp != null && ConfigSettings.terminalITP.Value)
                    {
                        doesITPexist = true;
                        LeaveTerminal.AddInverseTeleportKeywords();
                    }
                    else
                    {
                        Plugin.Log.LogInfo("ITP does not exist yet");
                    }
                }
            }
        }

        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        public class Terminal_Begin_Patch
        {

            static void Postfix(ref Terminal __instance)
            {
                Terminal_Awake_Patch.isTermInUse = __instance.terminalInUse;

                if (__instance.terminalInUse && Plugin.instance.CompatibilityAC && !Terminal_Awake_Patch.helpModified) //specifically for AdvancedCompany
                {
                    foreach (TerminalKeyword allKeyword in __instance.terminalNodes.allKeywords)
                    {
                        if (allKeyword.word == "other")
                        {
                            string originalString = allKeyword.specialKeywordResult.displayText;

                            StringBuilder newOther = new StringBuilder(originalString);

                            // Find the index of the last newline character
                            int lastNewlineIndex = newOther.ToString().LastIndexOf('\n');

                            // Remove the last line
                            if (lastNewlineIndex != -1)
                            {
                                newOther.Remove(lastNewlineIndex, newOther.Length - lastNewlineIndex);
                            }

                            // Append your own line
                            newOther.AppendLine(">MORE\r\nDisplay command categories added via darmuhsTerminalStuff\r\n");
                            allKeyword.specialKeywordResult.displayText = newOther.ToString();
                        }
                        //allKeyword.specialKeywordResult.displayText += ">MORE\r\nDisplay command categories added via darmuhsTerminalStuff\r\n";
                    }
                    Terminal_Awake_Patch.helpModified = true;
                    Plugin.Log.LogInfo("other modified"); //test

                }

                //Plugin.Log.LogInfo($"terminuse set to {__instance.terminalInUse}"); //for alwaysondisplay
                if (!Terminal_Awake_Patch.alwaysOnDisplay)
                {
                    LeaveTerminal.isVideoPlaying = false;
                    LeaveTerminal.checkForSplitView("neither");
                    Plugin.instance.isOnCamera = false;
                    Plugin.instance.isOnMap = false;
                    //patches in when terminal starts getting used

                    //Always load to start if alwayson disabled
                    TerminalNode startNode = __instance.terminalNodes.specialNodes.ToArray()[1];
                    __instance.LoadNewNode(startNode);
                }
                else if (__instance.usedTerminalThisSession && Terminal_Awake_Patch.alwaysOnDisplay)
                {
                    //TerminalNode foundNode = __instance.terminalNodes.terminalNodes.Find(x => x.name == "ViewInsideShipCam 1");
                    if (Plugin.instance.isOnCamera || Plugin.instance.isOnMap || Plugin.instance.isOnMiniCams|| Plugin.instance.isOnMiniMap || Plugin.instance.isOnOverlay)
                    {
                        if(__instance.terminalNodes.specialNodes[24].name != "ViewInsideShipCam 1")
                        {
                            Plugin.Log.LogInfo("Special node for returning to cams is not set properly or compatibility issues with other mods.");
                            return;
                        }
                        else
                        {
                            //__instance.currentNode.clearPreviousText = true;
                            __instance.LoadNewNode(__instance.terminalNodes.specialNodes[24]);
                            Plugin.Log.LogInfo($"returning to cams\nMap: {Plugin.instance.isOnMap} \nCams: {Plugin.instance.isOnCamera} \nMiniMap: {Plugin.instance.isOnMiniMap} \nMiniCams: {Plugin.instance.isOnMiniCams} \nOverlay: {Plugin.instance.isOnOverlay}");
                            return;
                        }
                        
                    }
                        
                }

            }
        }

        [HarmonyPatch(typeof(Terminal), "LoadTerminalImage")]
        public class fixVideoPatch : Terminal
        {
            public static bool sanityCheckLOL = false;
            static void Postfix(ref Terminal __instance, TerminalNode node)
            {

                Terminal instanceCopy = __instance;
                if (node.terminalEvent == "lolevent" && sanityCheckLOL)
                {
                    Plugin.Log.LogInfo("testing patch");
                    if(!LeaveTerminal.isVideoPlaying)
                    {
                        __instance.videoPlayer.enabled = true;
                        __instance.terminalImage.enabled = true;
                        __instance.videoPlayer.loopPointReached += vp => OnVideoEnd(vp, instanceCopy);

                        __instance.videoPlayer.Play();
                        LeaveTerminal.isVideoPlaying = true;
                        Plugin.Log.LogInfo("isVideoPlaying set to TRUE");
                        sanityCheckLOL = false;
                        return;
                    }
                }
            }

            public static void OnVideoEnd(VideoPlayer vp, Terminal instance)
            {
                // This method will be called when the video is done playing
                // Disable the video player and terminal image here
                if (LeaveTerminal.isVideoPlaying)
                {
                    instance.videoPlayer.enabled = false;
                    instance.terminalImage.enabled = false;
                    LeaveTerminal.isVideoPlaying = false;
                    sanityCheckLOL = false;
                    Plugin.Log.LogInfo("isVideoPlaying set to FALSE");
                    instance.videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
                    instance.videoPlayer.source = VideoSource.VideoClip;
                    instance.videoPlayer.aspectRatio = VideoAspectRatio.FitHorizontally;
                    instance.videoPlayer.isLooping = true;
                    instance.videoPlayer.playOnAwake = true;
                    
                } 
            }

        }

        [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
        public class Terminal_ParsePlayerSentence_Patch
        {

            public static int playerObjIdForTerminal; //needed for terminalEvent
            public static Color? FlashlightColor { get; private set; } // Public static variable to store the flashlight color
            public static string flashLightColor;

            // Define a public static property to hold the parsed values
            public static int ParsedValue { get; private set; }
            public static bool newParsedValue = false;
            public static bool isNextEnabled = false;
            public static StringBuilder extraLinesForInfoCommands = new StringBuilder("");

            // Public method to set the flashlight color
            public static void SetFlashlightColor(string colorKeyword)
            {
                switch (colorKeyword.ToLower())
                {
                    case "normal":
                        FlashlightColor = Color.white;
                        break;
                    case "default":
                        FlashlightColor = Color.white;
                        break;
                    case "red":
                        FlashlightColor = Color.red;
                        break;
                    case "blue":
                        FlashlightColor = Color.blue;
                        break;
                    case "yellow":
                        FlashlightColor = Color.yellow;
                        break;
                    case "cyan":
                        FlashlightColor = Color.cyan;
                        break;
                    case "magenta":
                        FlashlightColor = Color.magenta;
                        break;
                    case "green":
                        FlashlightColor = Color.green;
                        break;
                    case "purple":
                        FlashlightColor = new Color32(144, 100, 254, 1);
                        break;
                    case "lime":
                        FlashlightColor = new Color32(166, 254, 0, 1);
                        break;
                    case "pink":
                        FlashlightColor = new Color32(242, 0, 254, 1);
                        break;
                    case "maroon":
                        FlashlightColor = new Color32(114, 3, 3, 1); //new
                        break;
                    case "orange":
                        FlashlightColor = new Color32(255, 117, 24, 1); //new
                        break;
                    case "sasstro":
                        FlashlightColor = new Color32(212, 148, 180, 1);
                        break;
                    case "samstro":
                        FlashlightColor = new Color32(180, 203, 240, 1);
                        break;
                    default:
                        FlashlightColor = null; //this needs to be null for invalid results to return invalid
                        break;
                }
            }

            private static bool HandleConfirmation(Terminal __instance, string[] words, Action confirmCallback, Action denyCallback)
            {
                if (Plugin.instance.awaitingConfirmation)
                {
                    if (words.Length == 1 && (words[0].StartsWith("c")))
                    {
                        confirmCallback?.Invoke();
                        Plugin.instance.awaitingConfirmation = false;
                        return true; // Confirmation
                    }
                    if (words.Length == 1 && (words[0].StartsWith("d")))
                    {
                        denyCallback?.Invoke();
                        Plugin.instance.awaitingConfirmation = false;
                        return true; // Denial
                    }
                }
                return false; // No confirmation or denial
            }


            private static bool CheckForMYKeywords(string text, int textAdded, List<string> keywords)
            {
                string lowerText = text.Substring(text.Length - textAdded).ToLower();

                // Check if any of the keywords are present in the lowercase text
                foreach (string keyword in keywords)
                {
                    if (lowerText.Contains(keyword))
                    {
                        return true; // Return true if any keyword is found
                    }
                }

                return false; // Return false if no keyword is found
            }

            private static void getConfigKeywordsToUse()
            {
                if (ConfigSettings.fcolorKeyword.Value != null && GetKeyword(ConfigSettings.fcolorKeyword.Value) == null)
                    fColor = ConfigSettings.fcolorKeyword.Value.ToLower();
                else
                    fColor = "fcolor";
                if (ConfigSettings.gambleKeyword.Value != null && GetKeyword(ConfigSettings.gambleKeyword.Value) == null)
                    Gamble = ConfigSettings.gambleKeyword.Value.ToLower();
                else
                    Gamble = "gamble";

                if (ConfigSettings.leverKeyword.Value != null && GetKeyword(ConfigSettings.leverKeyword.Value) == null)
                    Lever = ConfigSettings.leverKeyword.Value.ToLower();
                else
                    Lever = "lever";

                if (ConfigSettings.scolorKeyword.Value != null && GetKeyword(ConfigSettings.scolorKeyword.Value) == null)
                    sColor = ConfigSettings.scolorKeyword.Value.ToLower();
                else
                    sColor = "scolor";

                if (ConfigSettings.linkKeyword.Value != null && GetKeyword(ConfigSettings.linkKeyword.Value) == null)
                    Link = ConfigSettings.linkKeyword.Value.ToLower();
                else
                    Link = "link";

            }

            private static void HelpCompatibility(Terminal termstance)
            {
                if (termstance.terminalInUse && Plugin.instance.CompatibilityOther && !Terminal_Awake_Patch.helpModified) //simple help append
                {
                    Task.Run(() =>
                    {
                        Thread.Sleep(400);
                        TerminalNode helpNode = termstance.terminalNodes.specialNodes.ToArray()[13];
                        helpNode.displayText += "\r\n\tdarmuhsTerminalStuff Additions:\r\n\r\n>MORE\r\nDisplay command categories added via darmuhsTerminalStuff\r\n\r\n";
                        Terminal_Awake_Patch.helpModified = true;
                        termstance.LoadNewNode(helpNode);
                        Plugin.Log.LogInfo("helpnode patched in compatibility mode");
                    }); 
                }
            }

            private static bool HideCams(Terminal __instance, TerminalNode __result)
            {
                return !ConfigSettings.camsNeverHide.Value
                    && __result.terminalEvent != "switchCamera"
                    && __result != __instance.terminalNodes.specialNodes[20]
                    && __result != __instance.terminalNodes.specialNodes[5]
                    && __result != __instance.terminalNodes.specialNodes[10]
                    && __result != __instance.terminalNodes.specialNodes[11]
                    && __result != __instance.terminalNodes.specialNodes[12]
                    && __result != __instance.terminalNodes.specialNodes[21]
                    && __result != __instance.terminalNodes.specialNodes[22]
                    && __result != __instance.terminalNodes.specialNodes[23];
            }

            static void Postfix(Terminal __instance, ref TerminalNode __result)
            {

                getConfigKeywordsToUse();

                // custom keywords not using TerminalApi to trigger a node result directly
                List<string> keywords = new List<string> { "lobby", "home", "more", "next", "comfort", "controls", "extras", "fun", "kick", fColor, "fov", Gamble, Lever, "vitalspatch", "bioscan", "bioscanpatch", sColor, Link }; // keyword catcher
                List<string> confirmationKeywords = new List<string> { "confirm", "c", "co", "con", "conf", "confi", "confir", "deny", "d", "de", "den" }; //confirm or deny catcher & shortened

                if (Plugin.instance.awaitingConfirmation && (!CheckForMYKeywords(__instance.screenText.text, __instance.textAdded, confirmationKeywords)))
                {
                    Plugin.instance.awaitingConfirmation = false;
                    Plugin.instance.confirmationNodeNum = 0;
                    Plugin.Log.LogInfo("disabled confirmation check, checked for confirmationKeywords and none found");
                }    //if player enters a different command than confirm or deny this should clear the waitingonconfirm state

                if (__result != null && (!CheckForMYKeywords(__instance.screenText.text, __instance.textAdded, confirmationKeywords))) //patching any input to disable confirmation check
                {
                    Plugin.instance.awaitingConfirmation = false;
                    Plugin.instance.confirmationNodeNum = 0;
                    
                    Plugin.Log.LogInfo("disabled confirmation check, checked __result at __result");    
                }

                if (__result != null && LeaveTerminal.isVideoPlaying && __result.terminalEvent != "lolevent")
                {
                    fixVideoPatch.OnVideoEnd(__instance.videoPlayer, __instance);
                    LeaveTerminal.isVideoPlaying = false;
                    //Plugin.Log.LogInfo("isVideoPlaying set to FALSE");
                }

                List<string> excludedNames = new List<string> //stuff that should not disable cams
                {
                    "ViewInsideShipCam 1",
                    "Toggle Doors",
                    "Toggle Lights",
                    "Always-On Display",
                    "Use Inverse Teleporter",
                    "Use Teleporter",
                    "Clear Terminal Screen",
                    "Check Danger Level",
                    "Check Vitals",
                    "HealFromTerminal",
                    "Check Loot Value",
                    "RandomSuit"
                };

                //20,21,22
                if (__result != null && !excludedNames.Contains(__result.name) && HideCams(__instance, __result))
                {
                    LeaveTerminal.checkForSplitView("neither");
                    Plugin.Log.LogInfo("disabling cams views");
                }

                if (__result == __instance.terminalNodes.specialNodes.ToArray()[13] && Plugin.instance.CompatibilityOther && !Terminal_Awake_Patch.helpModified) //simple help append
                {
                    HelpCompatibility(__instance);
                    Terminal_Awake_Patch.helpModified = true;
                }

                string cleanedText = GetCleanedScreenText(__instance);
                string[] words = cleanedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                TerminalNode dummyNode = CreateTerminalNode("replacing view monitor", true, "mapEvent");

                if (words.Length == 2 && words[0].ToLower() == "view" && words[1].ToLower() == "monitor" && __result != null)
                {
                    Plugin.Log.LogInfo("Found mapScreenNode");
                    __result = dummyNode;
                }

                if (__result != null && __result == __instance.terminalNodes.specialNodes[20])
                {
                    TerminalNode switchNode = CreateTerminalNode("", true, "returnCams");
                    Plugin.Log.LogInfo("attempting to run terminal events rather than patch node");
                    if (words.Length == 2 && words[0].ToLower() == "switch")
                    {
                        Plugin.instance.switchTarget = words[1].ToLower();
                    }
                    string playerName = "";
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
                    __instance.RunTerminalEvents(switchNode);
                    if (Plugin.instance.isOnCamera)
                    {
                        if (playerName != String.Empty)
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched to {playerName} (CAMS)\n";
                        }
                        else
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched target (CAMS)\n";
                        }
                    }
                    else if (Plugin.instance.isOnMap)
                    {
                        if (playerName != String.Empty)
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched to {playerName} (MAP)\n";
                        }
                        else
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched target (MAP)\n";
                        }
                    }
                    else if (Plugin.instance.isOnOverlay)
                    {
                        if (playerName != String.Empty)
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched to {playerName} (Overlay)\n";
                        }
                        else
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched target (Overlay)\n";
                        }
                    }
                    else if (Plugin.instance.isOnMiniMap)
                    {
                        if (playerName != String.Empty)
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched to {playerName} (MiniMap)\n";
                        }
                        else
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched target (MiniMap)\n";
                        }
                    }
                    else if (Plugin.instance.isOnMiniCams)
                    {
                        if (playerName != String.Empty)
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched to {playerName} (MiniCams)\n";
                        }
                        else
                        {
                            __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched target (MiniCams)\n";
                        }
                    }
                    else
                    {
                        __result.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched target.\n";
                    }
                    return;
                }


                //long ass if statement checking for both lists because I wanted them separate lol
                if (CheckForMYKeywords(__instance.screenText.text, __instance.textAdded, keywords) || CheckForMYKeywords(__instance.screenText.text, __instance.textAdded, confirmationKeywords))
                {
                    NetworkManager networkManager = __instance.NetworkManager;

                    //kick nodes
                    TerminalNode kickYes = CreateTerminalNode("Player has been kicked.\n", false, "kickYes");
                    TerminalNode kickNo = CreateTerminalNode("Unable to kick, player not found.\n", false, "kickNo");
                    TerminalNode notHost = CreateTerminalNode("You do not have access to this command.\n", false, "NotHost");
                    //fcolor nodes
                    TerminalNode flashReturn = CreateTerminalNode("changing flash color\n", false, "flashlight");
                    TerminalNode flashFail = CreateTerminalNode("Invalid color, flashlight color will not be changed.\n", false);
                    //gamble nodes
                    TerminalNode gambleDO = CreateTerminalNode("\n", false, "gamble");
                    TerminalNode gambleNO = CreateTerminalNode($"Gamble cancelled.\n", true);
                    //fov nodes
                    TerminalNode fovNode = CreateTerminalNode("\n", false, "fov");
                    //lever nodes
                    TerminalNode leverAsk = CreateTerminalNode("Pull the Lever?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n", true);
                    TerminalNode leverDO = CreateTerminalNode($"Lever pull confirmed, pulling now...\n", true, "leverdo");
                    TerminalNode leverDONT = CreateTerminalNode($"Lever pull cancelled...\n", true);

                    //link nodes
                    TerminalNode linkAsk = CreateTerminalNode($"Would you like to be taken to the following link?\n\n{ConfigSettings.customLink.Value}\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n", true);
                    TerminalNode linkDO = CreateTerminalNode($"Taking you to {ConfigSettings.customLink.Value} now...\n", true, "externalLink");
                    TerminalNode linkDONT = CreateTerminalNode($"You have cancelled visiting the site: {ConfigSettings.customLink.Value}\n", true);

                    //vitals nodes
                    TerminalNode VitalsUpgradeAsk = CreateTerminalNode($"", true, "upgradevitalsAsk");
                    TerminalNode VitalsDoUpgrade = CreateTerminalNode("\n", true, "vitalsUpgrade");
                    TerminalNode VitalsDontUpgrade = CreateTerminalNode("You have opted out of purchasing the Vitals Scanner Upgrade.\n", true);
                    TerminalNode VitalsAlreadyUpgraded = CreateTerminalNode("Vitals Scanner software has already been updated to the latest patch (2.0).\n", true);
                    //bioscan nodes
                    TerminalNode bioScanUpgradeAsk = CreateTerminalNode($"Purchase the BioScanner 2.0 Upgrade Patch?\nThis software update is available for {ConfigSettings.bioScanUpgradeCost.Value} Credits.\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n", true);
                    TerminalNode bioScanDoUpgrade = CreateTerminalNode("\n", true, "betterescan");
                    TerminalNode bioScanDontUpgrade = CreateTerminalNode("You have opted out of purchasing the BioScanner 2.0 Upgrade Patch.\n", true);
                    TerminalNode bioScanAlreadyUpgraded = CreateTerminalNode("BioScanner software has already been updated to the latest patch (2.0).\n", true);
                    TerminalNode localResult = __result;

                    // Check for confirmation or denial keywords
                    if (words.Length == 1 && CheckForMYKeywords(__instance.screenText.text, __instance.textAdded, confirmationKeywords) && Plugin.instance.awaitingConfirmation)
                    {
                        Plugin.Log.LogInfo("confirmation keywords detected, matching to event");

                        if( Plugin.instance.confirmationNodeNum == 1) //lever
                        HandleConfirmation(__instance, words,
                            confirmCallback: () =>
                            {
                                localResult = leverDO;
                                Plugin.Log.LogInfo("Confirmation received.");
                            },
                            denyCallback: () =>
                            {
                                localResult = leverDONT;
                                Plugin.Log.LogInfo("Denial received.");
                            });
                        if (Plugin.instance.confirmationNodeNum == 2) //gamble
                            HandleConfirmation(__instance, words,
                                confirmCallback: () =>
                                {
                                    localResult = gambleDO;
                                    Plugin.Log.LogInfo("Confirmation received.");
                                },
                                denyCallback: () =>
                                {
                                    localResult = gambleNO;
                                    Plugin.Log.LogInfo("Denial received.");
                                });
                        if (Plugin.instance.confirmationNodeNum == 3) //vitalsUpgrade
                            HandleConfirmation(__instance, words,
                                confirmCallback: () =>
                                {
                                    localResult = VitalsDoUpgrade;
                                    Plugin.Log.LogInfo("Confirmation received.");
                                },
                                denyCallback: () =>
                                {
                                    localResult = VitalsDontUpgrade;
                                    Plugin.Log.LogInfo("Denial received.");
                                });
                        if (Plugin.instance.confirmationNodeNum == 4) //bioScanUpgrade
                            HandleConfirmation(__instance, words,
                                confirmCallback: () =>
                                {
                                    localResult = bioScanDoUpgrade;
                                    Plugin.Log.LogInfo("Confirmation received.");
                                },
                                denyCallback: () =>
                                {
                                    localResult = bioScanDontUpgrade;
                                    Plugin.Log.LogInfo("Denial received.");
                                });

                        if (Plugin.instance.confirmationNodeNum == 5) //CustomLink
                            HandleConfirmation(__instance, words,
                                confirmCallback: () =>
                                {
                                    localResult = linkDO;
                                    Plugin.Log.LogInfo("Confirmation received.");
                                },
                                denyCallback: () =>
                                {
                                    localResult = linkDONT;
                                    Plugin.Log.LogInfo("Denial received.");
                                });

                        Plugin.instance.awaitingConfirmation = false; //remove confirm check
                        Plugin.instance.confirmationNodeNum = 0;
                        __result = localResult;
                        Plugin.Log.LogInfo("__result set (2)");
                        return;
                    }

                    if(words.Length <= 2 && words[0].ToLower() == "lobby")
                    {
                        if(GameNetworkManager.Instance.steamLobbyName != String.Empty)
                        {
                            string currentLobby = GameNetworkManager.Instance.steamLobbyName;

                            TerminalNode lobbyNode = CreateTerminalNode($"Lobby Name: {currentLobby}\n", true);
                            __result = lobbyNode;
                            return;
                        }
                        else
                        {
                            TerminalNode failNode = CreateTerminalNode($"Unable to determine Lobby Name");
                            __result = failNode;
                            return;
                        }
                        

                    }



                    if (words.Length == 1 && words[0].ToLower() == Lever && ConfigSettings.terminalLever.Value && ((object)networkManager != null && networkManager.IsHost))
                    {
                        Plugin.Log.LogInfo("command init: lever");
                        if (ConfigSettings.leverConfirmOverride.Value)
                        {
                            __result = leverDO;
                            Plugin.Log.LogInfo("Confirm Override is Enabled for command Lever.");
                            return;
                        }
                        else
                        {
                            __result = leverAsk; //Ask user to confirm or deny
                            Plugin.Log.LogInfo("__result set (1)");
                            Plugin.instance.awaitingConfirmation = true;
                            Plugin.instance.confirmationNodeNum = 1; //lever

                            // Awaiting Confirmation Logic
                            Plugin.Log.LogInfo("waiting for confirm for command, lever");
                            return;
                        }
                            
                        
                    }

                    if (words.Length == 1 && words[0].ToLower() == Link && ConfigSettings.terminalLink.Value)
                    {
                        Plugin.Log.LogInfo("command init: link");
                        __result = linkAsk; //Ask user to confirm or deny
                        Plugin.Log.LogInfo("__result set (1)");
                        Plugin.instance.awaitingConfirmation = true;
                        Plugin.instance.confirmationNodeNum = 5; //link

                        // Awaiting Confirmation Logic
                        Plugin.Log.LogInfo("waiting for confirm for command, link");
                        return;
                    }

                    if (words.Length == 1 && words[0].ToLower() == "vitalspatch" && ConfigSettings.terminalVitalsUpgrade.Value && ConfigSettings.ModNetworking.Value)
                    {
                        Plugin.Log.LogInfo("Asking user if they want to buy vitals upgrade");
                        if (LeaveTerminal.vitalsUpgradeEnabled == true)
                        {
                            __result = VitalsAlreadyUpgraded;
                            return;
                        }
                        else
                        {
                            __result = VitalsUpgradeAsk; //Ask user to confirm or deny
                            Plugin.instance.awaitingConfirmation = true;
                            Plugin.instance.confirmationNodeNum = 3; //VitalsUpgrade

                            // Awaiting Confirmation Logic
                            Plugin.Log.LogInfo("waiting for confirm");
                            return;
                        }
                    }
                    if (words.Length == 1 && words[0].ToLower() == "bioscan" && ConfigSettings.terminalBioScan.Value && ConfigSettings.ModNetworking.Value)
                    {
                        __result = CreateTerminalNode("", true, "enemies");
                        Plugin.Log.LogInfo("sending to enemies terminalEvent");
                        return;
                    }

                    if (words.Length == 1 && words[0].ToLower() == "bioscanpatch" && ConfigSettings.terminalBioScan.Value && ConfigSettings.ModNetworking.Value)
                    {
                        Plugin.Log.LogInfo("Asking user if they want to buy bioscan upgrade");
                        if (LeaveTerminal.enemyScanUpgradeEnabled == true)
                        {
                            __result = bioScanAlreadyUpgraded;
                            return;
                        }
                        else
                        {
                            __result = bioScanUpgradeAsk; //Ask user to confirm or deny
                            Plugin.instance.awaitingConfirmation = true;
                            Plugin.instance.confirmationNodeNum = 4; //bioscanUpgrade

                            // Awaiting Confirmation Logic
                            Plugin.Log.LogInfo("waiting for confirm");
                            return;
                        }   
                    }

                    if (words.Length >= 2 && words[0].ToLower() == Gamble && ConfigSettings.terminalGamble.Value && ConfigSettings.ModNetworking.Value)
                    {
                        Plugin.Log.LogInfo("gamble command init");
                        string digitsProvided = words[1];
                        if (Regex.IsMatch(digitsProvided, "\\d+"))
                        {
                            Terminal_ParsePlayerSentence_Patch.newParsedValue = true;
                            Plugin.Log.LogInfo("))))))))))))))))))Integer Established");
                            int parsedValue = int.Parse(digitsProvided);
                            ParsedValue = parsedValue;
                        }
                        else
                        {
                            Plugin.Log.LogWarning("there are no digits");
                            __result = __instance.terminalNodes.specialNodes[10];
                            return;
                        }
                        TerminalNode gambleAsk = CreateTerminalNode($"Gamble {ParsedValue}% of your credits?\n\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n", true);
                        __result = gambleAsk; //Ask user to confirm or deny
                        Plugin.Log.LogInfo("gamble ask set and asking for confirmation");
                        Plugin.instance.awaitingConfirmation = true;
                        Plugin.instance.confirmationNodeNum = 2; //gamble

                        // Awaiting Confirmation Logic
                        Plugin.Log.LogInfo("waiting for confirm");
                        return;
                    }

                    if (words.Length >= 2 && words[0].ToLower() == "fov" && ConfigSettings.terminalFov.Value)
                    {
                        Plugin.Log.LogWarning("word found: fov");
                        string digitsProvided = words[1];
                        if (Regex.IsMatch(digitsProvided, "\\d+"))
                        {
                            Terminal_ParsePlayerSentence_Patch.newParsedValue = true;
                            Plugin.Log.LogInfo("))))))))))))))))))Integer Established");
                            int parsedValue = int.Parse(digitsProvided);
                            ParsedValue = parsedValue;
                            __result = fovNode;
                            return;
                        }
                        else
                        {
                            Plugin.Log.LogWarning("there are no digits");
                            __result = __instance.terminalNodes.specialNodes[10];
                            return;
                        }


                    }
                    if(words.Length >= 2 && words[0].ToLower() == sColor && ConfigSettings.terminalScolor.Value && ConfigSettings.ModNetworking.Value)
                    {
                        string targetColor = "";
                        Plugin.Log.LogInfo("scolor command init");
                        if (words.Length == 3)
                        {
                            targetColor = words[2];
                            Plugin.Log.LogInfo("only setting color for 3 words");
                        }
                            

                        
                        if (words[1].ToLower() == "list")
                        {
                            plugin.Log.LogInfo("list detected");
                            TerminalNode sList = CreateTerminalNode($"========= Ship Lights Color Options List =========\r\nColor Name: \"command used\"\r\n\r\nDefault: \"{sColor} all normal\" or \"{sColor} all default\"\r\nRed: \"{sColor} back red\"\r\nGreen: \"{sColor} mid green\"\r\nBlue: \"{sColor} front blue\"\r\nYellow: \"{sColor} middle yellow\"\r\nCyan: \"{sColor} all cyan\"\r\nMagenta: \"{sColor} back magenta\"\r\nPurple: \"{sColor} mid purple\"\r\nLime: \"{sColor} all lime\"\r\nPink: \"{sColor} front pink\"\r\nMaroon: \"{sColor} middle maroon\"\r\nOrange: \"{sColor} back orange\"\r\nSasstro's Color: \"{sColor} all sasstro\"\r\nSamstro's Color: \"{sColor} all samstro\"\r\n\r\n", true);
                            __result = sList;
                            return;
                        }
                        else if (words[1].ToLower() == "all" && words.Length == 3)
                        {
                            Plugin.Log.LogInfo($" Attempting to set all ship light colors to {words[2]}");
                            SetFlashlightColor(targetColor); //get if color is valid
                            if (FlashlightColor.HasValue && targetColor != null)
                            {
                                Color newColor = FlashlightColor.Value;
                                NetHandler.Instance.ShipColorALLServerRpc(newColor, targetColor);
                                TerminalNode tempNode = CreateTerminalNode($"Color of all lights set to {targetColor}!\r\n");
                                __result = tempNode;
                                return;
                            }
                            else
                            {
                                TerminalNode tempNode = CreateTerminalNode($"Invalid color {targetColor}.\r\n");
                                __result = tempNode;
                                return;
                            }
                        }
                        else if(words[1].ToLower() == "front" && words.Length == 3)
                        {
                            Plugin.Log.LogInfo($" Attempting to set front ship light colors to {words[2]}");
                            SetFlashlightColor(targetColor); //get if color is valid
                            if (FlashlightColor.HasValue && targetColor!=null)
                            {
                                Color newColor = FlashlightColor.Value;
                                NetHandler.Instance.ShipColorFRONTServerRpc(newColor, targetColor);
                                TerminalNode tempNode = CreateTerminalNode($"Color of front ship lights set to {targetColor}!\r\n");
                                __result = tempNode;
                                return;
                            }
                            else
                            {
                                TerminalNode tempNode = CreateTerminalNode($"Invalid color {targetColor}.\r\n");
                                __result = tempNode;
                                return;
                            }
                        }
                        else if (words.Length == 3 && (words[1].ToLower() == "middle" || words[1].ToLower() == "mid"))
                        {
                            Plugin.Log.LogInfo($" Attempting to set middle ship light colors to {words[2]}");
                            SetFlashlightColor(targetColor); //get if color is valid
                            if (FlashlightColor.HasValue && targetColor != null)
                            {
                                Color newColor = FlashlightColor.Value;
                                NetHandler.Instance.ShipColorMIDServerRpc(newColor, targetColor);
                                TerminalNode tempNode = CreateTerminalNode($"Color of middle ship lights set to {targetColor}!\r\n");
                                __result = tempNode;
                                return;
                            }
                            else
                            {
                                TerminalNode tempNode = CreateTerminalNode($"Invalid color {targetColor}.\r\n");
                                __result = tempNode;
                                return;
                            }
                        }
                        else if (words.Length == 3 && words[1].ToLower() == "back")
                        {
                            Plugin.Log.LogInfo($" Attempting to set back ship light colors to {words[2]}");
                            SetFlashlightColor(targetColor); //get if color is valid
                            if (FlashlightColor.HasValue && targetColor != null)
                            {
                                Color newColor = FlashlightColor.Value;
                                NetHandler.Instance.ShipColorBACKServerRpc(newColor, targetColor);
                                TerminalNode tempNode = CreateTerminalNode($"Color of back ship lights set to {targetColor}!\r\n");
                                __result = tempNode;
                                return;
                            }
                            else
                            {
                                TerminalNode tempNode = CreateTerminalNode($"Invalid color {targetColor}.\r\n");
                                __result = tempNode;
                                return;
                            }
                        }
                        else
                        {
                            TerminalNode tempNode = CreateTerminalNode($"Invalid selection.\r\n\r\nPlease choose between all, front, middle, and back lights to set and ensure you have specified a color name.\r\n\r\nSee '{sColor} list' for a list of color names.\r\n");
                            __result = tempNode;
                            return;
                        }
                    }

                    if (words.Length >= 2 && words[0].ToLower() == fColor && words[1].ToLower() != "list" && ConfigSettings.terminalFcolor.Value && ConfigSettings.ModNetworking.Value)
                    {
                        Plugin.Log.LogInfo("fcolor command detected!");
                        string targetColor = words[1];

                        SetFlashlightColor(targetColor);
                        flashLightColor = targetColor;

                        if (FlashlightColor.HasValue)
                        {
                            Plugin.Log.LogInfo($"Using flashlight color: {targetColor}");
                            __result = flashReturn;
                            return;
                        }
                        else
                        {
                            Plugin.Log.LogInfo($"Invalid flashlight color keyword: {targetColor}");
                            __result = flashFail;
                            return;
                        }
                    }
                    if (words.Length >= 2 && words[0].ToLower() == fColor && words[1].ToLower() == "list" && ConfigSettings.terminalFcolor.Value == true) //get list of colors
                    {
                        TerminalNode fList = CreateTerminalNode("========= Flashlight Color Options List =========\r\nColor Name: \"command used\"\r\n\r\nDefault: \"fcolor normal\" or \"fcolor default\"\r\nRed: \"fcolor red\"\r\nGreen: \"fcolor green\"\r\nBlue: \"fcolor blue\"\r\nYellow: \"fcolor yellow\"\r\nCyan: \"fcolor cyan\"\r\nMagenta: \"fcolor magenta\"\r\nPurple: \"fcolor purple\"\r\nLime: \"fcolor lime\"\r\nPink: \"fcolor pink\"\r\nMaroon: \"fcolor maroon\"\r\nOrange: \"fcolor orange\"\r\nSasstro's Color: \"fcolor sasstro\"\r\nSamstro's Color: \"fcolor samstro\"\r\n\r\n", true);
                        __result = fList;
                        return;
                    }

                    if (words.Length >= 2 && words[0].ToLower() == "kick" && ConfigSettings.terminalKick.Value == true)
                    {
                        string targetPlayerName = words[1];

                        if (GameNetworkManager.Instance.localPlayerController.isHostPlayerObject)
                        {
                            if (targetPlayerName.Length >= 3)
                            {

                                // Find the matching player in allPlayerScripts
                                var matchingPlayer = StartOfRound.Instance.allPlayerScripts.FirstOrDefault(player =>
                                    player.playerUsername.IndexOf(targetPlayerName, StringComparison.OrdinalIgnoreCase) != -1);

                                if (matchingPlayer != null && matchingPlayer.isHostPlayerObject == false)
                                {
                                    int privatePlayerObjId = -1;
                                    // Get the player's ID
                                    for (int i = 0; i < StartOfRound.Instance.allPlayerScripts.Count(); i++)
                                    {
                                        if (StartOfRound.Instance.allPlayerScripts[i].playerUsername == matchingPlayer.playerUsername)
                                        {
                                            privatePlayerObjId = i;
                                            break;
                                        }
                                    }

                                    // Ensure playerID is valid
                                    if (privatePlayerObjId != -1)
                                    {
                                        // KickedClients list containing SteamIDs
                                        ulong getSteamID = matchingPlayer.playerSteamId;
                                        //if (!StartOfRound.Instance.KickedClientIds.Contains(getSteamID))
                                        //{
                                        //    StartOfRound.Instance.KickedClientIds.Add(getSteamID);
                                        //}

                                        playerObjIdForTerminal = privatePlayerObjId; //pass int for kick command in terminalEvent
                                        Plugin.Log.LogInfo($"Kick command detected for player: {matchingPlayer.playerUsername}, Steam ID: {getSteamID}");
                                        __result = kickYes;
                                        return;
                                    }
                                    else
                                    {
                                        Plugin.Log.LogInfo($"Player {targetPlayerName} not found in the lobby. Object failed.");
                                        __result = kickNo;
                                        return;
                                        // Invalid playerID or player not found
                                    }
                                }
                                else //matchingplayer returning null
                                {
                                    Plugin.Log.LogInfo($"Player {targetPlayerName} not found in the lobby. (null response)");
                                    __result = kickNo;
                                    return;
                                }
                            }
                            else //string (name) given not at least 3 characters
                            {
                                Plugin.Log.LogInfo($"Input must be at least 3 characters long.");
                                __result = kickNo;
                                return;
                                // Handle case where the input is too short
                            }
                        }
                        else // handles when the person entering the command is not the host
                        {
                            __result = notHost;
                            return;
                        }
                    }

                    if (words.Length == 1 && words[0].ToLower() == "more")
                    {
                        TerminalNode moreText = CreateTerminalNode("Welcome to darmuh's Terminal Upgrade!\r\n\tSee below Categories for new stuff :)\r\n\r\n[COMFORT]\r\nImproves the terminal user experience.\r\n\r\n[EXTRAS]\r\nAdds extra functionality to the ship terminal.\r\n\r\n[CONTROLS]\r\nGives terminal more control of the ship's systems.\r\n\r\n[FUN]ctionality\r\nType \"fun\" for a list of these FUNctional commands.\r\n\r\n", true);
                        moreText.name = "More Command by Darmuh";
                        __result = moreText;                        
                        return;
                    }
                    if (words.Length == 1 && words[0].ToLower() == "next" && isNextEnabled)
                    {
                        TerminalNode nextText = CreateTerminalNode($"{extraLinesForInfoCommands}", true);
                        __result = nextText;
                        isNextEnabled = false;
                        return;
                    }
                    if (words.Length == 1 && words[0].ToLower() == "comfort")
                    {

                        int maxLines = 17;
                        extraLinesForInfoCommands = new StringBuilder("=== Comfort (Quality of Life) Page 2 ===\r\n\r\n");
                        StringBuilder comfortString = new StringBuilder("=== Category 1: Comfort (Quality of Life) ===\r\n\r\n");

                        if (ConfigSettings.terminalClear.Value)
                            comfortString.AppendLine("> clear\r\nClear the terminal of any existing text.\r\n");

                        comfortString.AppendLine("> home\r\nReturn to start screen.\r\n");

                        if (ConfigSettings.terminalAlwaysOn.Value)
                            comfortString.AppendLine($"> {ConfigSettings.alwaysOnKeyword.Value}\r\nToggle the Always-On Terminal Screen mode.\r\n");
                        if (ConfigSettings.terminalFov.Value)
                            comfortString.AppendLine($"> fov <value>\r\nUpdate your in-game Field of View.\r\n");
                        if (ConfigSettings.terminalHeal.Value)
                            comfortString.AppendLine($"> heal, {ConfigSettings.healKeyword2.Value}\r\nHeal yourself from any damage.\r\n");
                        if (ConfigSettings.terminalKick.Value && ((object)networkManager != null && networkManager.IsHost))
                            comfortString.AppendLine($"> kick\r\nKick another employee from your group.\r\n");
                        if (ConfigSettings.terminalLobby.Value)
                            comfortString.AppendLine($"> lobby\r\nDisplay current lobby name.\r\n");
                        if (ConfigSettings.terminalMods.Value)
                            comfortString.AppendLine($"> mods, {ConfigSettings.modsKeyword2.Value}\r\nDisplay your currently loaded Mods.\r\n");
                        if (ConfigSettings.terminalQuit.Value)
                            comfortString.AppendLine($"> quit, {ConfigSettings.quitKeyword2.Value}\r\nLeave the terminal.\r\n");

                        int numberOfLines = comfortString.ToString().Split(new[] { ".\r\n" }, StringSplitOptions.None).Length;
                        (string remainingLines, StringBuilder shortenedStringBuilder) = LimitLinesInStringBuilder(ref comfortString, maxLines);
                        if (remainingLines == String.Empty)
                        {
                            TerminalNode comfortText = CreateTerminalNode($"{comfortString}", true);
                            Plugin.Log.LogInfo("Number of lines:" + numberOfLines.ToString());
                            Plugin.Log.LogInfo("no need for second page");
                            __result = comfortText;
                            return;
                        }
                        else
                        {
                            shortenedStringBuilder.AppendLine("[NEXT PAGE]\r\nType 'next' to see the next page of commands\r\n");
                            TerminalNode comfortText = CreateTerminalNode($"{shortenedStringBuilder}", true);
                            extraLinesForInfoCommands.AppendLine(remainingLines);
                            //Plugin.Log.LogInfo($"Added to StringBuilder: {remainingLines}");
                            isNextEnabled = true;
                            __result = comfortText;
                            return;
                        }
                    }
                    if (words.Length == 1 && words[0].ToLower() == "extras")
                    {
                        int maxLines = 17;
                        extraLinesForInfoCommands = new StringBuilder("=== Extras Page 2 ===\r\n\r\n");
                        StringBuilder extraString = new StringBuilder("=== Category 2: Extras ===\r\n\r\n");

                        if (ConfigSettings.terminalCams.Value)
                            extraString.AppendLine($"> cams, {ConfigSettings.camsKeyword2.Value}\r\nToggle displaying cameras in terminal.\r\n");
                        
                        if (ConfigSettings.terminalMap.Value)
                            extraString.AppendLine($"> map, {ConfigSettings.mapKeyword2.Value}\r\nShortcut to toggle radar map on terminal.\r\n");
                        
                        if (ConfigSettings.terminalMinimap.Value)
                            extraString.AppendLine($"> {ConfigSettings.minimapKeyword.Value}\r\nToggle cameras and radar map via MiniMap Mode.\r\n");
                        
                        if (ConfigSettings.terminalOverlay.Value)
                            extraString.AppendLine($"> {ConfigSettings.overlayKeyword.Value}\r\nToggle cameras and radar map via Overlay Mode.\r\n");

                        if (ConfigSettings.terminalLoot.Value)
                            extraString.AppendLine($"> loot, {ConfigSettings.lootKeyword2.Value}\r\nDisplay total value of all loot on-board.\r\n");
                        
                        if (ConfigSettings.terminalVitals.Value && ConfigSettings.ModNetworking.Value)
                            extraString.AppendLine($"> vitals\r\nDisplay vitals of employee being tracked on radar.\r\n");

                        if (ConfigSettings.terminalVitalsUpgrade.Value && ConfigSettings.ModNetworking.Value)
                            extraString.AppendLine($"> vitalspatch\r\nPurchase upgrade to Vitals Software Patch 2.0\r\n");

                        if (ConfigSettings.terminalBioScan.Value && ConfigSettings.ModNetworking.Value)
                            extraString.AppendLine($"> bioscan\r\n Use Ship BioScanner to search for non-employee lifeforms.\r\n");

                        if (ConfigSettings.terminalBioScan.Value && ConfigSettings.ModNetworking.Value)
                            extraString.AppendLine($"> bioscanpatch\r\n Purchase upgrade to BioScanner Software Patch 2.0\r\n");

                        if (ConfigSettings.terminalLink.Value)
                            extraString.AppendLine($"> {ConfigSettings.linkKeyword.Value}\r\n Go to a specific web page.\r\n");

                        int numberOfLines = extraString.ToString().Split(new[] { ".\r\n" }, StringSplitOptions.None).Length;
                        (string remainingLines, StringBuilder shortenedStringBuilder) = LimitLinesInStringBuilder(ref extraString, maxLines);
                        if (remainingLines == String.Empty)
                        {
                            TerminalNode extraNode = CreateTerminalNode($"{extraString}", true);
                            Plugin.Log.LogInfo("Number of lines:" + numberOfLines.ToString());
                            Plugin.Log.LogInfo("no need for second page");
                            __result = extraNode;
                            return;
                        }
                        else
                        {
                            shortenedStringBuilder.AppendLine("[NEXT PAGE]\r\nType 'next' to see the next page of commands\r\n");
                            TerminalNode extraNode = CreateTerminalNode($"{shortenedStringBuilder}", true);
                            extraLinesForInfoCommands.AppendLine(remainingLines);
                            //Plugin.Log.LogInfo($"Added to StringBuilder: {remainingLines}");
                            isNextEnabled = true;
                            __result = extraNode;
                            return;
                        }
                    }
                    if (words.Length == 1 && words[0].ToLower() == "controls")
                    {
                        int maxLines = 17;
                        extraLinesForInfoCommands = new StringBuilder("=== Controls Page 2 ===\r\n\r\n");
                        StringBuilder controlString = new StringBuilder("=== Category 3: Controls ===\r\n\r\n");

                        if (ConfigSettings.terminalDanger.Value)
                            controlString.AppendLine($"> {ConfigSettings.dangerKeyword.Value} \r\nDisplays the danger level once the ship has landed.\r\n");
                        if (ConfigSettings.terminalLever.Value)
                            controlString.AppendLine($"> {Lever}\r\nRemotely pull the ship lever.\r\n");
                        if (ConfigSettings.terminalDoor.Value)
                            controlString.AppendLine($"> {ConfigSettings.doorKeyword.Value}\r\nRemotely open/close the ship doors.\r\n");
                        if (ConfigSettings.terminalLights.Value)
                            controlString.AppendLine($"> {ConfigSettings.lightsKeyword.Value}\r\nRemotely toggle the ship lights.\r\n");
                        if (ConfigSettings.terminalTP.Value)
                            controlString.AppendLine($"> {ConfigSettings.tpKeyword2.Value}, tp\r\nRemotely push the Teleporter button.\r\n");
                        if (ConfigSettings.terminalITP.Value)
                            controlString.AppendLine($"> {ConfigSettings.itpKeyword2.Value}, itp\r\nRemotely push the Inverse Teleporter button.\r\n");

                        int numberOfLines = controlString.ToString().Split(new[] { ".\r\n" }, StringSplitOptions.None).Length;
                        (string remainingLines, StringBuilder shortenedStringBuilder) = LimitLinesInStringBuilder(ref controlString, maxLines);
                        if (remainingLines == String.Empty)
                        {
                            TerminalNode controlNode = CreateTerminalNode($"{controlString}", true);
                            Plugin.Log.LogInfo("Number of lines:" + numberOfLines.ToString());
                            Plugin.Log.LogInfo("no need for second page");
                            __result = controlNode;
                            return;
                        }
                        else
                        {
                            shortenedStringBuilder.AppendLine("[NEXT PAGE]\r\nType 'next' to see the next page of commands\r\n");
                            TerminalNode controlNode = CreateTerminalNode($"{shortenedStringBuilder}", true);
                            extraLinesForInfoCommands.AppendLine(remainingLines);
                            //Plugin.Log.LogInfo($"Added to StringBuilder: {remainingLines}");
                            isNextEnabled = true;
                            __result = controlNode;
                            return;
                        }
                    }
                    if (words.Length == 1 && words[0].ToLower() == "fun" || words[0].ToLower() == "functionality")
                    {
                        int maxLines = 17;
                        extraLinesForInfoCommands = new StringBuilder("=== Fun Stuff Page 2 ===\r\n\r\n");
                        StringBuilder funString = new StringBuilder("=== Category 4: Fun Stuff ===\r\n\r\n");

                        if (ConfigSettings.terminalFcolor.Value && ConfigSettings.ModNetworking.Value)
                        {
                            funString.AppendLine($"> {fColor} <color>\r\nUpgrade your flashlight with a new color.\r\n");
                            funString.AppendLine($"> {fColor} list\r\nView available colors for flashlight.\r\n");
                        }    
                        if (ConfigSettings.terminalScolor.Value && ConfigSettings.ModNetworking.Value)
                        {
                            funString.AppendLine($"> {sColor} <all,front,middle,back> <color>\r\nChange the color of the ship's lights.\r\n");
                            funString.AppendLine($"> {sColor} list\r\nView available colors to change ship lights.\r\n");
                        }

                        if(ConfigSettings.terminalRandomSuit.Value)
                            funString.AppendLine($"> {ConfigSettings.randomSuitKeyword.Value} \r\nPut on a random suit.\r\n");

                        if (ConfigSettings.terminalGamble.Value && ConfigSettings.ModNetworking.Value)
                            funString.AppendLine($"> {Gamble} <percentage>\r\nGamble a percentage of your credits.\r\n");
                        if (ConfigSettings.terminalLol.Value)
                            funString.AppendLine($"> {ConfigSettings.lolKeyword.Value}\r\nPlay a silly video on the terminal.\r\n");

                        int numberOfLines = funString.ToString().Split(new[] { ".\r\n" }, StringSplitOptions.None).Length;
                        (string remainingLines, StringBuilder shortenedStringBuilder) = LimitLinesInStringBuilder(ref funString, maxLines);
                        if (remainingLines == String.Empty)
                        {
                            TerminalNode funNode = CreateTerminalNode($"{funString}", true);
                            Plugin.Log.LogInfo("Number of lines:" + numberOfLines.ToString());
                            Plugin.Log.LogInfo("no need for second page");
                            __result = funNode;
                            return;
                        }
                        else
                        {
                            shortenedStringBuilder.AppendLine("[NEXT PAGE]\r\nType 'next' to see the next page of commands\r\n");
                            TerminalNode funNode = CreateTerminalNode($"{shortenedStringBuilder}", true);
                            extraLinesForInfoCommands.AppendLine(remainingLines);
                            //Plugin.Log.LogInfo($"Added to StringBuilder: {remainingLines}");
                            isNextEnabled = true;
                            __result = funNode;
                            return;
                        }
                    }
                    if (words.Length == 1 && words[0].ToLower() == "home")
                    {
                        __result = __instance.terminalNodes.specialNodes[1];
                    }

                    return;
                }

                //Add TP keywords AFTER they have been purchased and exist
                ShipTeleporter[] objectsOfType = UnityEngine.Object.FindObjectsOfType<ShipTeleporter>();

                if (!Terminal_Awake_Patch.doesTPexist && ConfigSettings.terminalTP.Value)
                {
                    ShipTeleporter tp = (ShipTeleporter)null;

                    foreach (ShipTeleporter tpobject in objectsOfType)
                    {
                        if (!tpobject.isInverseTeleporter)
                        {
                            tp = tpobject;
                            break;
                        }
                    }

                    if (tp != null)
                    {
                        Terminal_Awake_Patch.doesTPexist = true;
                        LeaveTerminal.AddTeleportKeywords();
                    }
                    else
                    {
                        Plugin.Log.LogInfo("TP does not exist yet");
                    }
                }
                if (!Terminal_Awake_Patch.doesITPexist && ConfigSettings.terminalITP.Value)
                {
                    ShipTeleporter itp = (ShipTeleporter)null;
                    foreach (ShipTeleporter tpobject in objectsOfType)
                    {
                        if (tpobject.isInverseTeleporter)
                        {
                            itp = tpobject;
                            break;
                        }
                    }

                    if (itp != null && ConfigSettings.terminalITP.Value)
                    {
                        Terminal_Awake_Patch.doesITPexist = true;
                        LeaveTerminal.AddInverseTeleportKeywords();
                    }
                }
            }

            private static (string remainingLines, StringBuilder shortenedStringBuilder) LimitLinesInStringBuilder(ref StringBuilder stringBuilder, int maxLines)
            {
                int numberOfLines = stringBuilder.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None).Length;

                if (numberOfLines > maxLines)
                {
                    // If the number of lines exceeds the maximum, move the excess to a new StringBuilder
                    string[] lines = stringBuilder.ToString().Split(new[] { "\r\n" }, StringSplitOptions.None);
                    StringBuilder remainingLines = new StringBuilder();
                    remainingLines.AppendLine(string.Join("\r\n", lines, maxLines, lines.Length - maxLines));

                    // Update the original StringBuilder to contain only the first maxLines lines
                    stringBuilder.Length = 0;
                    stringBuilder.AppendLine(string.Join("\r\n", lines, 0, maxLines));

                    // Return both remaining lines and the shortened version
                    return (remainingLines.ToString(), stringBuilder);
                }

                // If the number of lines is within the limit, return an empty string for remaining lines
                return (string.Empty, stringBuilder);
            }

            private static string GetCleanedScreenText(Terminal __instance)
            {
                string s = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
                return RemovePunctuation(s);
            }
        }

        private static string RemovePunctuation(string s) //copied from game files
        {
            StringBuilder stringBuilder = new StringBuilder();
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().ToLower();
        }

    }
}
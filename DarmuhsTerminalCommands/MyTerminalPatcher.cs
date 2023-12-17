using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Video;
using static TerminalApi.TerminalApi;

namespace TerminalStuff
{
    public class AllMyTerminalPatches
    {
        public static AllMyTerminalPatches Instance;

        [HarmonyPatch(typeof(Terminal), "Start")]
        public class Terminal_Awake_Patch
        {
            //change vanilla terminal stuff here
            static void Postfix(ref Terminal __instance)
            {
                Plugin.Log.LogInfo("Upgrading terminal with my stuff, smile.");
                TerminalNode startNode = __instance.terminalNodes.specialNodes.ToArray()[1];
                TerminalNode helpNode = __instance.terminalNodes.specialNodes.ToArray()[13];
                helpNode.displayText = ">MOONS\r\nList of moons the autopilot can route to.\r\n\r\n>STORE\r\nCompany store's selection of useful items.\r\n\r\n>BESTIARY\r\nTo see the list of wildlife on record.\r\n\r\n>STORAGE\r\nTo access objects placed into storage.\r\n\r\n>OTHER\r\nTo see the list of other commands\r\n\r\n>MORE\r\nTo see a list of commands added via darmuhsTerminalStuff\r\n\r\n[numberOfItemsOnRoute]\r\n";
                startNode.displayText = "Welcome to the FORTUNE-9 OS PLUS\r\n\tUpgraded by Employee: darmuh\r\n\r\nType \"Help\" for a list of commands.\r\n\r\nType \"More\" for a list of \"extra\" commands.\r\n\r\n     ._______.\r\n     | \\   / |\r\n  .--|.O.|.O.|______.\r\n__).-| = | = |/   \\ |\r\np__) (.'---`.)Q.|.Q.|--.\r\n      \\\\___// = | = |-.(__\r\n       `---'( .---. ) (__&lt;\r\n             \\\\.-.//\r\n              `---'\r\n\t\t\t  \r\nHave a wonderful [currentDay]!\r\n";
            }
        }

        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        public class Terminal_Begin_Patch
        {
            private static VideoController videoController;

            static void Postfix(ref Terminal __instance)
            {
                VideoController.isVideoPlaying = false;
                LeaveTerminal.checkForSplitView("neither");
                Plugin.instance.isOnCamera = false;
                Plugin.instance.isOnMap = false;
                //patches in when terminal starts getting used
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
                    case "sasstro":
                        FlashlightColor = new Color32(212, 148, 180, 1);
                        break;
                    case "samstro":
                        FlashlightColor = new Color32(180, 203, 240, 1);
                        break;
                    default:
                        FlashlightColor = Color.white;
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

            static void Postfix(Terminal __instance, ref TerminalNode __result)
            {
                // custom keywords not using TerminalApi to trigger a node result directly
                List<string> keywords = new List<string> { "home", "more", "next", "comfort", "controls", "extras", "fun", "kick", "fcolor", "fov", "gamble", "lever", "vitalspatch", "bioscan", "bioscanpatch" }; // keyword catcher
                List<string> confirmationKeywords = new List<string> { "confirm", "c", "co", "con", "conf", "confi", "confir", "deny", "d", "de", "den" }; //confirm or deny catcher & shortened

                if(Plugin.instance.awaitingConfirmation && (!CheckForMYKeywords(__instance.screenText.text, __instance.textAdded, confirmationKeywords)))
                {
                    Plugin.instance.awaitingConfirmation = false;
                    Plugin.instance.confirmationNodeNum = 0;
                    Plugin.Log.LogInfo("disabled confirmation check, checked for confirmationKeywords");
                }    //if player enters a different command than confirm or deny this should clear the waitingonconfirm state

                if (__result != null && !(__result == __instance.terminalNodes.specialNodes[5] || __result == __instance.terminalNodes.specialNodes[10] || __result == __instance.terminalNodes.specialNodes[11] | __result == __instance.terminalNodes.specialNodes[12])) //patching any command that is not an error
                {
                    //plugin.Log.LogInfo("patching any valid result");
                    Plugin.instance.awaitingConfirmation = false;
                    Plugin.instance.confirmationNodeNum = 0;
                    Plugin.Log.LogInfo("disabled confirmation check, checked __result at __result");

                    //if we want to keep consistency with actively displayed objects
                 /*   if ((__instance.terminalNodes.specialNodes.Contains(__result) && !(__result == __instance.terminalNodes.specialNodes[0] || __result == __instance.terminalNodes.specialNodes[1] || __result == __instance.terminalNodes.specialNodes[13])))//anything in here will not have the below applied (too much text)
                    {
                        Plugin.Log.LogInfo("patched into specialnodes that are not big long commands!");
                        if (Plugin.instance.isOnCamera)
                        {
                            Plugin.instance.isOnCamera = false;
                            Plugin.Log.LogInfo("was last on cams");
                            __result.terminalEvent = "cams";
                            return;
                        }
                        else if (Plugin.instance.isOnMap)
                        {
                            Plugin.instance.isOnMap = false;
                            Plugin.Log.LogInfo("was last on map");
                            __result.terminalEvent = "mapEvent";
                            return;
                        }
                        else if (Plugin.instance.isOnProView)
                        {
                            Plugin.instance.isOnProView = false;
                            Plugin.Log.LogInfo("was last on proview");
                            __result.terminalEvent = "proview";
                            return;
                        }
                        else if (Plugin.instance.isOnOverlay)
                        {
                            Plugin.instance.isOnOverlay = false;
                            Plugin.Log.LogInfo("was last on overlay");
                            __result.terminalEvent = "overlay";
                            return;
                        }
                        else
                            Plugin.Log.LogInfo("no active screen of any kind (cams, map, etc.)");
                        return;
                    } */
                        
                }

                string cleanedText = GetCleanedScreenText(__instance);
                string[] words = cleanedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                TerminalNode dummyNode = CreateTerminalNode("replacing view monitor", true, "mapEvent");

                if (words.Length == 2 && words[0].ToLower() == "view" && words[1].ToLower() == "monitor" && __result != null)
                {
                    Plugin.Log.LogInfo("Found mapScreenNode");
                    __result = dummyNode;
                }


                //long ass if statement checking for both lists because I wanted them separate lol
                if (CheckForMYKeywords(__instance.screenText.text, __instance.textAdded, keywords) || CheckForMYKeywords(__instance.screenText.text, __instance.textAdded, confirmationKeywords))
                {
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

                        Plugin.instance.awaitingConfirmation = false; //remove confirm check
                        Plugin.instance.confirmationNodeNum = 0;
                        __result = localResult;
                        Plugin.Log.LogInfo("__result set (2)");
                        return;
                    }

                    if (words.Length == 1 && words[0].ToLower() == "lever" && ConfigSettings.terminalLever.Value)
                    {
                        Plugin.Log.LogInfo("word found: lever");
                        NetworkManager networkManager = __instance.NetworkManager;
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

                    if (words.Length == 1 && words[0].ToLower() == "vitalspatch" && ConfigSettings.terminalVitalsUpgrade.Value)
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
                    if (words.Length == 1 && words[0].ToLower() == "bioscan" && ConfigSettings.terminalBioScan.Value)
                    {
                        __result = CreateTerminalNode("", true, "enemies");
                        Plugin.Log.LogInfo("sending to enemies terminalEvent");
                        return;
                    }

                    if (words.Length == 1 && words[0].ToLower() == "bioscanpatch" && ConfigSettings.terminalBioScan.Value)
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

                    if (words.Length >= 2 && words[0].ToLower() == "gamble" && ConfigSettings.terminalGamble.Value)
                    {
                        Plugin.Log.LogInfo("word found: gamble");
                        NetworkManager networkManager = __instance.NetworkManager;
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
                        Plugin.Log.LogInfo("__result set (1)");
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

                    if (words.Length >= 2 && words[0].ToLower() == "fcolor" && words[1].ToLower() != "list" && ConfigSettings.terminalFcolor.Value == true)
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
                    if (words.Length >= 2 && words[0].ToLower() == "fcolor" && words[1].ToLower() == "list" && ConfigSettings.terminalFcolor.Value == true) //get list of colors
                    {
                        TerminalNode fList = CreateTerminalNode("========= Flashlight Color Options List =========\r\nColor Name: \"command used\"\r\n\r\nDefault: \"fcolor normal\" or \"fcolor default\"\r\nRed: \"fcolor red\"\r\nGreen: \"fcolor green\"\r\nBlue: \"fcolor blue\"\r\nYellow: \"fcolor yellow\"\r\nCyan: \"fcolor cyan\"\r\nMagenta: \"fcolor magenta\"\r\nPurple: \"fcolor purple\"\r\nLime: \"fcolor lime\"\r\nPink: \"fcolor pink\"\r\nSasstro's Color: \"fcolor sasstro\"\r\nSamstro's Color: \"fcolor samstro\"\r\n\r\n", true);
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

                        if (ConfigSettings.terminalFov.Value)
                            comfortString.AppendLine("> fov <value>\r\nUpdate your in-game Field of View.\r\n");
                        if (ConfigSettings.terminalHeal.Value)
                            comfortString.AppendLine("> heal, healme\r\nHeal yourself from any damage.\r\n");
                        if (ConfigSettings.terminalKick.Value)
                            comfortString.AppendLine("> kick\r\nKick another employee (if you're the captain).\r\n");
                        if (ConfigSettings.terminalLobby.Value)
                            comfortString.AppendLine("> lobby name, lobby, name\r\nDisplay current lobby name.\r\n");
                        if (ConfigSettings.terminalMods.Value)
                            comfortString.AppendLine("> mods, modlist\r\nDisplay your currently loaded Mods.\r\n");
                        if (ConfigSettings.terminalQuit.Value)
                            comfortString.AppendLine("> quit/exit\r\nExit the terminal.\r\n");

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
                            extraString.AppendLine("> cams, cameras\r\nToggle displaying cameras in terminal.\r\n");
                        
                        if (ConfigSettings.terminalMap.Value)
                            extraString.AppendLine("> map\r\nShortcut to toggle radar map on terminal.\r\n");
                        
                        if (ConfigSettings.terminalProview.Value)
                            extraString.AppendLine("> proview\r\nToggle cameras and radar map via ProView Mode.\r\n");
                        
                        if (ConfigSettings.terminalOverlay.Value)
                            extraString.AppendLine("> overlay\r\nToggle cameras and radar map via Overlay Mode.\r\n");

                        if (ConfigSettings.terminalLoot.Value)
                            extraString.AppendLine("> loot, shiploot\r\nDisplay total value of all loot on-board.\r\n");
                        
                        if (ConfigSettings.terminalVitals.Value)
                            extraString.AppendLine("> vitals\r\nDisplay vitals of employee being tracked on radar.\r\n");

                        if (ConfigSettings.terminalVitalsUpgrade.Value)
                            extraString.AppendLine("> vitalspatch\r\nPurchase upgrade to Vitals Software Patch 2.0\r\n");

                        if (ConfigSettings.terminalBioScan.Value)
                            extraString.AppendLine("> bioscan\r\n Use Ship BioScanner to search for non-employee lifeforms.\r\n");

                        if (ConfigSettings.terminalBioScan.Value)
                            extraString.AppendLine("> bioscanpatch\r\n Purchase upgrade to BioScanner Software Patch 2.0\r\n");

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
                            controlString.AppendLine("> danger \r\nDisplays the danger level once the ship has landed.\r\n");
                        if (ConfigSettings.terminalLever.Value)
                            controlString.AppendLine("> lever\r\nRemotely pull the ship lever.\r\n");
                        if (ConfigSettings.terminalDoor.Value)
                            controlString.AppendLine("> door\r\nRemotely open/close the ship doors.\r\n");
                        if (ConfigSettings.terminalTP.Value)
                            controlString.AppendLine("> teleport, tp\r\nRemotely push the Teleporter button.\r\n");
                        if (ConfigSettings.terminalITP.Value)
                            controlString.AppendLine("> inverse, itp\r\nRemotely push the Inverse Teleporter button.\r\n");

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

                        if (ConfigSettings.terminalFcolor.Value)
                        {
                            funString.AppendLine("> fcolor <color>\r\nUpgrade your flashlight with a new color.\r\n");
                            funString.AppendLine("> fcolor list\r\nView available colors.\r\n");
                        }    
                            
                        if (ConfigSettings.terminalGamble.Value)
                            funString.AppendLine("> gamble <percentage>\r\nGamble a percentage of your credits.\r\n");
                        if (ConfigSettings.terminalLol.Value)
                            funString.AppendLine("> lol\r\nPlay a silly video.\r\n");

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


        internal static bool _isInGame()
        {
            try
            {
                return TerminalApi.TerminalApi.Terminal != null;
            }
            catch (NullReferenceException)
            {
                return false;
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

        public class VideoHandler
        {
            public void OnVideoErrorReceived(VideoPlayer source, string message)
            {
                // Handle the video error
                // Log the error message using your logger
                Plugin.Log.LogInfo($">>>>>>>>>>>>>>>>>>>VideoPlayer Error: {message}");

                // You may choose to handle the error in other ways as well
            }
        }
    }
}
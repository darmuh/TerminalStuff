using BepInEx;
using HarmonyLib;
using System.IO;
using TerminalApi;
using static TerminalApi.TerminalApi;
using static TerminalStuff.Plugin;
using UnityEngine;
using UnityEngine.Video;
using System;
using static TerminalStuff.Getlobby;
using System.Collections.Generic;
using static TerminalStuff.LeaveTerminal;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using System.Net;
using System.Reflection;
using System.Linq;
using System.Text;
using System.CodeDom;
using Unity.Netcode;
using BepInEx.Bootstrap;
using Steamworks;

namespace TerminalStuff
{
    [HarmonyPatch(typeof(Terminal), "Start")]
    public class MyTerminalAwakePatch
    {
        public static void Postfix(ref Terminal __instance) // Note the 'static' keyword
        {
            
            if (ConfigSettings.terminalGamble.Value)
            {
                AddGambleKeyword();
            }
            if (ConfigSettings.terminalFov.Value)
            {
                AddFovKeyword();
            }
            if (ConfigSettings.terminalLever.Value)
            {
                LeaveTerminal.leverKeywords();
            }

            //room for more

        }

        private int CheckForPlayerName(string firstWord, string secondWord)
        {

            if (secondWord.Length <= 2)
            {
                return -1;
            }

            Debug.Log("first word: " + firstWord + "; second word: " + secondWord);
            List<string> list = new List<string>();
            for (int i = 0; i < StartOfRound.Instance.mapScreen.radarTargets.Count; i++)
            {
                list.Add(StartOfRound.Instance.mapScreen.radarTargets[i].name);
                Debug.Log($"name {i}: {list[i]}");
            }

            secondWord = secondWord.ToLower();
            Debug.Log($"Target names length: {list.Count}");
            for (int j = 0; j < list.Count; j++)
            {
                Debug.Log("A");
                string text = list[j].ToLower();
                Debug.Log($"Word #{j}: {text}; length: {text.Length}");
                for (int num = secondWord.Length; num > 2; num--)
                {
                    Debug.Log($"c: {num}");
                    Debug.Log(secondWord.Substring(0, num));
                    if (text.StartsWith(secondWord.Substring(0, num)))
                    {
                        return j;
                    }
                }
            }

            return -1;
        }


        [HarmonyPatch(typeof(Terminal), "BeginUsingTerminal")]
        public class Terminal_Begin_Patch
        {
            private static VideoController videoController;

            static void Postfix(ref Terminal __instance)
            {
                VideoController.isVideoPlaying = false;
            }
        }

        [HarmonyPatch(typeof(Terminal), "ParseWord")]
        public class Terminal_ParseWord_Patch
        {
            // Define a public static property to hold the parsed value
            public static int ParsedValue { get; private set; }
            public static bool newParsedValue = false;

            static void Postfix(Terminal __instance, string playerWord, int specificityRequired, ref TerminalKeyword __result)
            {
                // Check if playerWord is a digit
                Plugin.Log.LogInfo("))))))))))))))))))Patched in");
                if (Regex.IsMatch(playerWord, "\\d+"))
                {
                    int parsedValue;
                    Terminal_ParseWord_Patch.newParsedValue = true;
                    Plugin.Log.LogInfo("))))))))))))))))))Integer Established");
                    string s = GetCleanedScreenText(__instance);
                    string[] array = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Plugin.Log.LogWarning("array created for parseword method");

                    if (!string.IsNullOrEmpty(s))
                    {
                        TerminalKeyword tempResult = null; // Declare a temporary variable

                        tempResult = ProcessWord(__instance, array, "gamble");
                        if (tempResult != null)
                        {
                            __result = tempResult;
                            return;
                        }

                        tempResult = ProcessWord(__instance, array, "fov");
                        if (tempResult != null)
                        {
                            __result = tempResult;
                            return;
                        }
                    }
                }
            }

            
            private static TerminalKeyword ProcessWord(Terminal __instance, string[] array, string targetString)
            {
                string firstWord = array.Length > 0 ? array[0] : null;
                Plugin.Log.LogWarning($"checking for {targetString}");

                if (!string.IsNullOrEmpty(firstWord) && firstWord.Equals(targetString, StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(array.ElementAtOrDefault(1), out int parsedValue))
                {
                    string valueString = parsedValue.ToString();
                    Plugin.Log.LogInfo("))))))))))))))))))Value Stored");
                    Plugin.Log.LogInfo("))))))))))))))))))Value:" + valueString);

                    TerminalNode triggerNode = CreateTerminalNode("\n", false, targetString);
                    TerminalKeyword terminalKeyword = CreateTerminalKeyword(targetString, true, triggerNode);
                    UpdateKeyword(terminalKeyword);

                    ParsedValue = parsedValue;
                    __instance.playerDefinedAmount = parsedValue;
                    Plugin.Log.LogInfo("))))))))))))))))))Value Usable");

                    return terminalKeyword;
                }

                return null;
            }

            private static string GetCleanedScreenText(Terminal __instance)
            {
                string s = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
                return RemovePunctuation(s);
            }
        }

        [HarmonyPatch(typeof(Terminal), "ParsePlayerSentence")]
        public class Terminal_ParsePlayerSentence_Patch
        {
            static void Postfix(Terminal __instance, ref TerminalNode __result)
            {
                // Access the array from ParsePlayerSentence method
                if (ConfigSettings.terminalKick.Value == true && (__instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded).ToLower().Contains("kick")))
                {
                    //add config check here I think
                    
                    string cleanedText = GetCleanedScreenText(__instance);
                    string[] words = cleanedText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    TerminalNode kickYes = CreateTerminalNode("Player has been kicked.\n", false, "kickyes");
                    TerminalNode kickNo = CreateTerminalNode("Cannot find player.\n", false, "kickno");
                    TerminalNode notHost = CreateTerminalNode("You are not the host and cannot kick.\n", false, "nothost");

                    if (words.Length >= 2 && words[0].ToLower() == "kick")
                    {
                        string targetPlayerName = words[1];

                        if (GameNetworkManager.Instance.localPlayerController.isHostPlayerObject)
                        {
                            if (targetPlayerName.Length >= 3)
                            {
                                // Check if the targetPlayerName starts with the input
                                var matchingPlayer = GameNetworkManager.Instance.currentLobby.Value.Members.FirstOrDefault(player =>
                                player.Name.IndexOf(targetPlayerName, StringComparison.OrdinalIgnoreCase) != -1);

                                if (!EqualityComparer<Friend>.Default.Equals(matchingPlayer, default(Friend)))
                                {
                                    // Get the player's ID
                                    ulong targetPlayerId = matchingPlayer.Id.Value;

                                    // Perform kick action
                                    if (!StartOfRound.Instance.KickedClientIds.Contains(targetPlayerId))
                                    {
                                        StartOfRound.Instance.KickedClientIds.Add(targetPlayerId);
                                    }

                                    NetworkManager.Singleton.DisconnectClient(targetPlayerId);
                                    Plugin.Log.LogInfo($"Kick command detected for player: {targetPlayerName}, ID: {targetPlayerId}");
                                    __result = kickYes;
                                }
                                else
                                {
                                    Plugin.Log.LogInfo($"Player {targetPlayerName} not found in the lobby.");
                                    __result = kickNo;
                                    // Handle case where the player is not found
                                }
                            }
                            else
                            {
                                Plugin.Log.LogInfo($"Input must be at least 3 characters long.");
                                __result = kickNo;
                                // Handle case where the input is too short
                            }
                        }
                        else __result = notHost; //handles when person entering command is not the host
                    }
                } 
                    // Check if you want to modify the result or perform additional actions
                if (__result != null && __result == __instance.terminalNodes.specialNodes[10])
                {
                    string s = GetCleanedScreenText(__instance);
                    string[] array = s.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Plugin.Log.LogWarning("got result 10");

                    if (!string.IsNullOrEmpty(s))
                    {
                        string firstWord = array.Length > 0 ? array[0] : null;
                        string gambleString = "gamble"; // Replace with the specific string you're checking for
                        string fovString = "fov";
                        Plugin.Log.LogWarning("checking for word");

                        if (!string.IsNullOrEmpty(firstWord))
                        {
                            TerminalNode gambleNode = CreateTerminalNode("\n", false, gambleString);
                            TerminalNode fovNode = CreateTerminalNode("\n", false, fovString);

                            if (firstWord.Equals(gambleString, StringComparison.OrdinalIgnoreCase) && ConfigSettings.terminalGamble.Value)
                            {
                                Plugin.Log.LogWarning("word found: " + firstWord);
                                __result = gambleNode;
                            }
                            else if (firstWord.Equals(fovString, StringComparison.OrdinalIgnoreCase) && ConfigSettings.terminalFov.Value)
                            {
                                Plugin.Log.LogWarning("word found: " + firstWord);
                                __result = fovNode;
                            }
                            else
                            {
                                Plugin.Log.LogFatal("couldn't find matching strings!");
                                __result = __instance.terminalNodes.specialNodes[10];
                            }
                        }
                    }
                    else
                    {
                        Plugin.Log.LogFatal("array was null?");
                        __result = __instance.terminalNodes.specialNodes[10];
                    }
                }
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

        public static void AddGambleKeyword()
        {
            TerminalNode gambleNode = CreateTerminalNode("\n", false, "gamble");
            TerminalKeyword gambleKeyword = CreateTerminalKeyword("gamble", true, gambleNode);
            AddTerminalKeyword(gambleKeyword);
            Plugin.Log.LogInfo("---------Gamble Keyword added!---------");  
        }
        public static void AddFovKeyword()
        {
            TerminalNode fovNode = CreateTerminalNode("\n", false, "fov"); //testing numbers grab
            TerminalKeyword fovKeyword = CreateTerminalKeyword("fov", true, fovNode);
            AddTerminalKeyword(fovKeyword);
            Plugin.Log.LogInfo("---------Fov Keyword added!---------");
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
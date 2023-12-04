using BepInEx;
using HarmonyLib;
using System.IO;
using TerminalApi;
using static TerminalApi.TerminalApi;
using static TerminalStuff.Plugin;
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

namespace TerminalStuff
{
    [HarmonyPatch(typeof(Terminal), "Start")]
    public class MyTerminalAwakePatch
    {
        // Shared variable to store the 'array'
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
            if (__instance.videoPlayer.source != VideoSource.Url || __instance.videoPlayer.url == "")
            {
                __instance.videoPlayer.enabled = true;

                // Create an instance of the videoHandler class
                var handler = new VideoHandler();

                // Subscribe to the errorReceived event using the instance method
                __instance.videoPlayer.errorReceived += handler.OnVideoErrorReceived;

                __instance.videoPlayer.clip = (VideoClip)null;
                string vfileName1 = $"file://{Paths.PluginPath.Substring(Path.GetPathRoot(Paths.PluginPath).Length)}".TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
                string vfileName2 = "/darmuh-TerminalStuff/lol.mp4";
                Plugin.Log.LogInfo("---------" + vfileName1 + vfileName2);
                __instance.videoPlayer.url = vfileName1 + vfileName2;
                __instance.videoPlayer.source = VideoSource.Url;
                __instance.videoPlayer.Prepare();
                __instance.videoPlayer.Stop();
                //Plugin.Log.LogInfo("this should break");
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
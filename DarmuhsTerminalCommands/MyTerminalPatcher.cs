using BepInEx;
using HarmonyLib;
using System.IO;
using TerminalApi;
using static TerminalApi.TerminalApi;
using static TerminalStuff.Plugin;
using UnityEngine.Video;
using System;
using static TerminalStuff.Getlobby;

namespace TerminalStuff
{
    [HarmonyPatch(typeof(Terminal), "Start")]
    public class MyTerminalAwakePatch
    {
        public static void Postfix(ref Terminal __instance) // Note the 'static' keyword
        {
            //AddNumbersToTest(0,10);
            if (__instance.videoPlayer.source != VideoSource.Url || __instance.videoPlayer.url == "")
            {
                __instance.videoPlayer.enabled = true;

                // Create an instance of the videoHandler class
                var handler = new VideoHandler();

                // Subscribe to the errorReceived event using the instance method
                __instance.videoPlayer.errorReceived += handler.OnVideoErrorReceived;

                __instance.videoPlayer.clip = (VideoClip)null;
                string vfileName1 = $"file://{Paths.PluginPath.Substring(Path.GetPathRoot(Paths.PluginPath).Length)}".TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Replace('\\', '/');
                string vfileName2 = "/darmuh-DarmuhsTerminalCommands/lol.mp4";
                Plugin.Log.LogInfo("---------" + vfileName1 + vfileName2);
                __instance.videoPlayer.url = vfileName1 + vfileName2;
                __instance.videoPlayer.source = VideoSource.Url;
                __instance.videoPlayer.Prepare();
                //__instance.videoPlayer.Stop();
                //Plugin.Log.LogInfo("this should break");
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
        
        public static void AddNumbersToTest( int start, int end )
        {
            TerminalNode testNode = CreateTerminalNode("leaving.\n", false, "test");
            TerminalKeyword testKeyword = CreateTerminalKeyword("test", true, testNode);
            AddTerminalKeyword(testKeyword);
            Plugin.Log.LogInfo("---------Test Keyword added!---------");
            
            if (start <= end && _isInGame())
            {
                //plugin.Log.LogInfo($"[Paramaters] myverbKeyword: {myverbKeyword}, start: {start}, end: {end}, mytriggerNode: {mytriggerNode}");
                for (int i = start; i <= end; i++)
                {
                    TerminalKeyword numbersTest = CreateTerminalKeyword(i.ToString(), false, null);
                    AddTerminalKeyword(numbersTest);
                    testKeyword.AddCompatibleNoun(i.ToString(), testNode);
                    UpdateKeyword(testKeyword);

                    //debug stuff
                    plugin.Log.LogInfo("adding number " + i.ToString() + " to test");
                    
                }

                //verbKeyword.compatibleNouns = newCompatibleNouns;
                Plugin.Log.LogInfo("---------Test Keywords updated!---------");
            }
            else
            {
                // Handle the case where the start is greater than the end
                plugin.Log.LogError("Invalid range: start should be less than or equal to end.");
            }
        }

        /*
        public static void AddCompatibleNumbersInRange(TerminalKeyword myverbKeyword, int start, int end, TerminalNode mytriggerNode) //thanks again chatgpt
        {
            if (start <= end && _isInGame())
            {
                plugin.Log.LogInfo($"[Paramaters] myverbKeyword: {myverbKeyword}, start: {start}, end: {end}, mytriggerNode: {mytriggerNode}");
                for (int i = start; i <= end; i++)
                {
                    CreateTerminalKeyword(i.ToString(), false, mytriggerNode);
                    //UpdateKeywordCompatibleNoun(myverbKeyword, i.ToString(), mytriggerNode);
                    myverbKeyword.AddCompatibleNoun(i.ToString(), mytriggerNode);

                    //debug stuff
                    string nameofTrigger = mytriggerNode.ToString();
                    string nameofVerb = myverbKeyword.ToString();
                    plugin.Log.LogInfo("adding number " + i.ToString() + " to verb " + nameofVerb + "for trigger" + nameofTrigger );
                        
                }

                //verbKeyword.compatibleNouns = newCompatibleNouns;
                UpdateKeyword(myverbKeyword);
            }
            else
            {
                // Handle the case where the start is greater than the end
                plugin.Log.LogError("Invalid range: start should be less than or equal to end.");
            }
        }
        */

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

using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static TerminalApi.TerminalApi;
using static TerminalStuff.MyTerminalAwakePatch;
using UnityEngine.Video;
using System.Collections;
using System.Runtime.CompilerServices;
using BepInEx;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TerminalStuff
{
    public static class LeaveTerminal
    {
        public static string TotalValueFormat = "";
        public static string VideoErrorMessage = "";
        [HarmonyPatch(typeof(Terminal))]
        [HarmonyPatch("RunTerminalEvents")]
        class Terminal_RunTerminalEvents_Patch
        {
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
                    if (node.terminalEvent == "lolevent") //trying to play custom media on the terminal
                    {
                        if (__instance.videoPlayer.clip != null)
                        {
                            __instance.videoPlayer.clip = (VideoClip)null;
                            Plugin.Log.LogInfo("videoPlayer.clip was not null");
                        }
                            __instance.terminalImage.enabled = true;
                            //__instance.videoPlayer.errorReceived += MyTerminalAwakePatch.OnVideoErrorReceived;
                            __instance.videoPlayer.enabled = true;
                            __instance.videoPlayer.Play();
                            Plugin.Log.LogInfo("" + __instance.videoPlayer.url + " <-- That should show the link");
                            Texture renderTexture = __instance.videoPlayer.texture;
                            node.displayTexture = renderTexture;
                            Plugin.Log.LogInfo("Target Texture:" + __instance.videoPlayer.targetTexture + " <-- That should show the texture it should be playing on");
                            Plugin.Log.LogInfo("Video Length:" + __instance.videoPlayer.length + ".");

                            node.displayText = "hi lol.\n";   

                        //Debug log commands
                        Plugin.Log.LogInfo("URL:" + __instance.videoPlayer.url);
                        Plugin.Log.LogInfo("Render Mode:" + __instance.videoPlayer.renderMode);
                        Plugin.Log.LogInfo("Prepared Value:" + __instance.videoPlayer.isPrepared);

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
                    }
                    if (node.terminalEvent == "healme")
                    {
                        //this code snippet is slightly modified from Octolar's Healing Mod, credit to them
                        GameNetworkManager.Instance.localPlayerController.DamagePlayer(-100, false);
                        if (GameNetworkManager.Instance.localPlayerController.health < 10 || !GameNetworkManager.Instance.localPlayerController.criticallyInjured)
                            node.displayText = "You are full health!";
                        else
                        {
                            GameNetworkManager.Instance.localPlayerController.MakeCriticallyInjured(false);
                            node.displayText = "The terminal healed you?!?";
                        }
                    }
                    if (node.terminalEvent == "cams")
                    {
                        bool isOnCamera = __instance.terminalImage.enabled;
                        if (GameObject.Find("Environment/HangarShip/Cameras/ShipCamera") != null && isOnCamera == false)
                        {
                            // ... (other code)

                            // Get the main texture from "Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001"
                            Texture renderTexture = GameObject.Find("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001").GetComponent<MeshRenderer>().materials[2].mainTexture;
                            node.displayTexture = renderTexture;
                            __instance.terminalImage.enabled = true;
                            //isOnCamera = true;
                            Plugin.Log.LogInfo("cam added to terminal screen");
                            // Now you can use 'yourTexture' in your own code
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
                        //int newGroupCredits = __instance.groupCredits;
                        //newGroupCredits
                        //node.displayText = ("Whatever you type here should be relayed back to you:\n");
                        __instance.screenText.Select();
                        string s = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);
                        string digitsOnly = Regex.Replace(s, @"\D", "");
                        node.displayText = ("Your input: " + digitsOnly + "\n");
                        //string numbers = new string(existingString.Where(char.IsDigit).ToArray());
                        // groupCredits = Mathf.Clamp(groupCredits - totalCostOfItems, 0, 10000000);

                    }
                }
                // Ensure all code paths return a value in a coroutine
                yield break;
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
            TerminalNode clearNode = CreateTerminalNode($"\n", true, "clear");
            TerminalKeyword clearKeyword = CreateTerminalKeyword("clear", true, clearNode);
            AddTerminalKeyword(clearKeyword);
            Plugin.Log.LogInfo("Adding Clear keywords");
        }
        public static void healKeywords()
        {
            TerminalNode healNode = CreateTerminalNode($"\n", true, "healme");
            TerminalKeyword healKeyword = CreateTerminalKeyword("heal", true, healNode);
            TerminalKeyword healmeKeyword = CreateTerminalKeyword("healme", true, healNode);
            AddTerminalKeyword(healKeyword);
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
            TerminalNode camsNode = CreateTerminalNode($"Attempting to grab active Cameras.\n", true, "cams");
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


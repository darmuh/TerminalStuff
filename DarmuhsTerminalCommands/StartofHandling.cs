using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using static TerminalApi.TerminalApi;
using static TerminalStuff.AllMyTerminalPatches;
using static TerminalStuff.DynamicCommands;
using static TerminalStuff.SpecialConfirmationLogic;

namespace TerminalStuff
{
    internal class StartofHandling
    {
        public static bool helpModified = false;
        internal static TerminalNode switchNode = CreateTerminalNode("switchNode", true);
        internal static Coroutine delayedUpdater;
        internal static List<string> dynamicKeywords = new List<string> { "kick", fColor, "fov", Gamble, Lever, "vitalspatch", "bioscanpatch", sColor, Link, Link2, Restart, "bind", "unbind" }; // keyword catcher
        internal static List<string> confirmationKeywords = new List<string> { "confirm", "c", "co", "con", "conf", "confi", "confir", "deny", "d", "de", "den" }; //confirm or deny catcher & shortened
        internal static TerminalNode HandleParsed(Terminal terminal, string[] words, out TerminalNode resultNode)
        {
            string firstWord = words[0].ToLower();

            InitCallbackNums();
            MenuBuild.CheckAndResetMenuVariables(firstWord);

            if (Plugin.instance.awaitingConfirmation)
            {
                CheckConfirm(firstWord, out bool handleConfirm);
                if (handleConfirm)
                {
                    ConfirmationNodes(terminal, words, out resultNode);
                    return resultNode;
                }
                else
                {
                    HandleAnyNode(terminal, words, firstWord, out resultNode);
                    return resultNode;
                }
            }
            else
            {
                HandleAnyNode(terminal, words, firstWord, out resultNode);
                return resultNode;
            }
        }

        internal static int FindViewNode(TerminalNode givenNode)
        {

            for(int i = 0; i < ViewCommands.termViewNodes.Count; i++)
            {
                if(givenNode == ViewCommands.termViewNodes[i])
                {
                    Plugin.MoreLogs("View node found, returning integer.");
                    return i;
                }       
            }
            return -1;
            
        }

        internal static void CheckNetNode(TerminalNode resultNode)
        {
            if (!ConfigSettings.networkedNodes.Value)
                return;

            Plugin.MoreLogs("Networked nodes enabled, sending result to server.");
            if (resultNode != null)
            {
                if (ViewCommands.termViewNodes.Contains(resultNode))
                {
                    int nodeNum = FindViewNode(resultNode);
                    NetHandler.NetNodeReset(true);
                    NetHandler.Instance.NodeLoadServerRpc(resultNode.name, resultNode.displayText, nodeNum);
                    Plugin.MoreLogs($"Valid node detected, nNS true & nodeNum: {nodeNum}");
                    return;
                }
                else
                {
                    NetHandler.NetNodeReset(true);
                    Plugin.MoreLogs("Valid node detected, nNS true");
                    NetHandler.Instance.NodeLoadServerRpc(resultNode.name, resultNode.displayText);
                    return;
                } 
            }
            else
            {
                Plugin.MoreLogs("Invalid node for sync");
                return;
            }
                
        }

        internal static TerminalNode HandleAnyNode(Terminal terminal, string[] words, string firstWord, out TerminalNode resultNode)
        {
            if (dynamicKeywords.Contains(firstWord))
            {
                Plugin.MoreLogs($"Dynamic Keyword Detected [{firstWord}]");
                DynamicCommands.SendToKeywordMethod(words, words.Length, out resultNode);
                return resultNode;
            }
            else if (MenuBuild.keywordList.Contains(firstWord))
            {

                MenuBuild.HandleMenus(firstWord, out resultNode);
                if (resultNode != null)
                {
                    Plugin.MoreLogs("Sending to darmuh's menus");
                    return resultNode;
                }
                else
                    return resultNode;
            }
            else if (firstWord == "next" && MenuBuild.isNextEnabled)
            {
                MenuBuild.HandleNext(out resultNode);
                Plugin.MoreLogs("Sending to darmuh's menus (next)");
                if (resultNode != null)
                {
                    Plugin.MoreLogs("Sending to next page");
                    return resultNode;
                }
                else
                {
                    Plugin.MoreLogs("node not found :(");
                    return resultNode;
                }
            }
            else if (firstWord == "home")
            {
                Plugin.MoreLogs("sending back to home");
                resultNode = terminal.terminalNodes.specialNodes[1];
                return resultNode;
            }
            else if (firstWord == "switch")
            {
                if(words.Length == 1)
                {
                    Plugin.MoreLogs("switch command detected");
                    resultNode = switchNode;

                    if (Plugin.instance.TwoRadarMapsMod)
                        TwoRadarMapsCompatibility.UpdateTerminalRadarTarget(terminal);
                    else
                        StartOfRound.Instance.mapScreen.SwitchRadarTargetForward(callRPC: true);

                    ViewCommands.DisplayTextUpdater(out string displayText);

                    resultNode.displayText = displayText;
                    return resultNode;
                }
                else
                {
                    Plugin.MoreLogs("switch to specific player command detected");
                    resultNode = terminal.terminalNodes.specialNodes[20];

                    if (Plugin.instance.TwoRadarMapsMod)
                    {
                        int playernum = TwoRadarMapsCompatibility.CheckForPlayerNameCommand(firstWord, words[1].ToLower());
                        if (playernum != -1)
                        {
                            TwoRadarMapsCompatibility.UpdateTerminalRadarTarget(terminal, playernum);
                            ViewCommands.InitializeTextures();
                            ViewCommands.DisplayTextUpdater(out string displayText);
                            resultNode.displayText = displayText;
                            return resultNode;
                        }
                        Plugin.MoreLogs("PlayerName returned invalid number");
                        resultNode = null;
                        return resultNode;
                    }
                    else
                    {
                        int playernum = terminal.CheckForPlayerNameCommand(firstWord, words[1].ToLower());
                        if (playernum != -1)
                        {
                            StartOfRound.Instance.mapScreen.SwitchRadarTargetAndSync(playernum);
                            ViewCommands.InitializeTextures();
                            ViewCommands.DisplayTextUpdater(out string displayText);
                            resultNode.displayText = displayText;
                            return resultNode;
                        }

                        Plugin.MoreLogs("PlayerName returned invalid number");
                        resultNode = null;
                        return resultNode;
                    }
                } 
            }
            else
            {
                resultNode = null;
                //Plugin.MoreLogs("returning null node");
                return resultNode;
            }
        }

        internal static void DelayedUpdateText(Terminal terminal)
        {
            if (delayedUpdater != null)
            {
                terminal.StopCoroutine(delayedUpdater);
            }

            delayedUpdater = terminal.StartCoroutine(DelayedUpdateTextRoutine(terminal));
        }

        internal static IEnumerator DelayedUpdateTextRoutine(Terminal terminal)
        {
            yield return new WaitForSeconds(0.045f);
            ViewCommands.DisplayTextUpdater(out string displayText);
            ViewCommands.InitializeTextures();
            switchNode.displayText = displayText;
            terminal.LoadNewNode(switchNode);

        }

        private static void NetFirstCheck(TerminalNode initialResult)
        {
            if(ConfigSettings.networkedNodes.Value)
            {
                if (initialResult != null && !NetHandler.netNodeSet)
                    StartofHandling.CheckNetNode(initialResult);
                else
                    Plugin.MoreLogs($"nNS: {NetHandler.netNodeSet}");
            }
        }

        internal static void FirstCheck(Terminal terminal, ref TerminalNode initialResult)
        {
            NetFirstCheck(initialResult);

            if (initialResult != null && ViewCommands.isVideoPlaying && initialResult.name != "darmuh's videoPlayer")
            {
                fixVideoPatch.OnVideoEnd(terminal.videoPlayer, terminal);
                ViewCommands.isVideoPlaying = false;
                //Plugin.Log.LogInfo("isVideoPlaying set to FALSE");
                Plugin.MoreLogs("disabling video");
            }

            List<string> excludedNames = new List<string> //stuff that should not disable cams
                {
                    "ViewInsideShipCam 1",
                    "Mirror",
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
                    "RandomSuit",
                    "Terminal Clock"
                };

            //20,21,22
            if (initialResult != null && !excludedNames.Contains(initialResult.name) && HideCams(terminal, initialResult))
            {
                SplitViewChecks.DisableSplitView("neither");
                Plugin.MoreLogs("disabling cams views");
            }

            /*
            if (initialResult == terminal.terminalNodes.specialNodes.ToArray()[13] && Plugin.instance.CompatibilityOther && !helpModified) //simple help append
            {
                HelpCompatibility(terminal);
                helpModified = true;
            } */

            return;
        }

        private static bool CheckConfirm(string keyword, out bool handle)
        {
            if (confirmationKeywords.Contains(keyword))
            {
                Plugin.MoreLogs("CheckConfirm verified confirmationKeywords");
                handle = true;
                return handle;
            }
            else
            {
                handle = false;
                Plugin.instance.awaitingConfirmation = false;
                Plugin.instance.confirmationNodeNum = 0;
                Plugin.MoreLogs("disabled confirmation check, checked for confirmationKeywords and none found");
                return handle;
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

        private static void HelpCompatibility(Terminal termstance)
        {
            if (termstance.terminalInUse && Plugin.instance.LateGameUpgrades && !helpModified) //simple help append
            {
                TerminalNode helpNode = termstance.terminalNodes.specialNodes.ToArray()[13];
                helpNode.displayText += "\r\n\tdarmuhsTerminalStuff Additions:\r\n\r\n>MORE\r\nDisplay command categories added via darmuhsTerminalStuff\r\n\r\n";
                helpModified = true;
                termstance.LoadNewNode(helpNode);
                Plugin.MoreLogs("helpnode patched in compatibility mode");
            }
        }

    }
}

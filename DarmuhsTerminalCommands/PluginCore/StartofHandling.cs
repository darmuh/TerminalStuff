using OpenLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TerminalStuff.Compatibility;
using TerminalStuff.EventSub;
using static OpenLib.CoreMethods.LogicHandling;
using static TerminalStuff.MoreCamStuff;

namespace TerminalStuff
{
    internal class StartofHandling
    {

        internal static void HandleShortcutFinal(string cleanedText)
        {
            SetTerminalInput(cleanedText);
            Plugin.Log.LogDebug($"Terminal Input set to:{cleanedText}");
            Plugin.instance.Terminal.OnSubmit();
        }

        internal static TerminalNode HandleParsed(TerminalNode currentNode, ref TerminalNode resultNode)
        {
            HandleAnyNode(currentNode, ref resultNode);
            return resultNode;
        }

        internal static int FindViewInt(TerminalNode givenNode)
        {
            foreach (KeyValuePair<TerminalNode, int> pairValue in ConfigSettings.TerminalStuffMain.specialListNum)
            {
                if (pairValue.Key == givenNode)
                {
                    int nodeNum = pairValue.Value;
                    return nodeNum;
                }
            }

            return -1;
        }

        internal static int FindViewIntByString()
        {
            if (!ViewCommands.AnyActiveMonitoring())
                return -1;

            string currentMode = GetViewMode(out int specialNum);

            Plugin.Spam($"Current Mode: {currentMode}");

            return specialNum;
        }

        private static string GetViewMode(out int specialNum)
        {
            string mode;

            if (Plugin.instance.isOnCamera)
            {
                mode = "cams";
                Plugin.MoreLogs("cams mode detected");
                specialNum = 1;
                return mode;
            }
            else if (Plugin.instance.isOnMap)
            {
                mode = "map";
                Plugin.MoreLogs("map mode detected");
                specialNum = 5;
                return mode;
            }
            else if (Plugin.instance.isOnOverlay)
            {
                mode = "overlay";
                Plugin.MoreLogs("overlay mode detected");
                specialNum = 2;
                return mode;
            }
            else if (Plugin.instance.isOnMiniMap)
            {
                mode = "minimap";
                Plugin.MoreLogs("minimap mode detected");
                specialNum = 3;
                return mode;
            }
            else if (Plugin.instance.isOnMiniCams)
            {
                mode = "minicams";
                Plugin.MoreLogs("minicams mode detected");
                specialNum = 4;
                return mode;
            }
            else if (Plugin.instance.isOnMirror)
            {
                mode = "mirror";
                Plugin.MoreLogs("Mirror mode detected");
                specialNum = 6;
                return mode;
            }
            else
            {
                Plugin.Log.LogError("Error with mode return, setting to default value");
                mode = "none";
                specialNum = -1;
                return mode;
            }
        }

        internal static TerminalNode FindViewNode(int givenInt)
        {
            if (givenInt < 0 || !ConfigSettings.TerminalStuffMain.specialListNum.ContainsValue(givenInt))
                return null;
            foreach (KeyValuePair<TerminalNode, int> pairValue in ConfigSettings.TerminalStuffMain.specialListNum)
            {
                if (pairValue.Value == givenInt)
                {
                    TerminalNode foundNode = pairValue.Key;
                    return foundNode;
                }
            }
            return null;
        }

        internal static void SyncTerminal(TerminalNode resultNode)
        {
            CheckNetNode(resultNode);
            NetHandler.Instance.SyncRadarZoomServerRpc(GameStuff.TerminalMapRenderer.cam.orthographicSize);
            if (Plugin.instance.TwoRadarMapsMod)
                NetHandler.Instance.SyncRadarMapServerRpc((int)StartOfRound.Instance.localPlayerController.playerClientId, GameStuff.TerminalMapRenderer.targetTransformIndex);    
        }

        internal static void CheckNetNode(TerminalNode resultNode)
        {
            if (!ConfigSettings.NetworkedNodes.Value || !ConfigSettings.ModNetworking.Value)
                return;

            Plugin.MoreLogs("Networked nodes enabled, sending result to server.");
            if (resultNode != null)
            {
                if (ConfigSettings.TerminalStuffMain.specialListNum.ContainsKey(resultNode)) //should be the listing that contains the viewnodes
                {
                    int nodeNum = FindViewInt(resultNode);
                    NetHandler.NetNodeReset(true);
                    NetHandler.Instance.NodeLoadServerRpc(Plugin.instance.Terminal.topRightText.text, resultNode.name, resultNode.displayText, nodeNum);
                    Plugin.MoreLogs($"Valid node detected, nNS true & nodeNum: {nodeNum}");
                    return;
                }
                else
                {
                    int nodeNum = FindViewIntByString();
                    NetHandler.NetNodeReset(true);
                    NetHandler.Instance.NodeLoadServerRpc(Plugin.instance.Terminal.topRightText.text, resultNode.name, resultNode.displayText, nodeNum);
                    Plugin.MoreLogs($"Valid node detected, nNS true & nodeNum is detected as: {nodeNum}");
                    return;
                }
            }
            else
            {
                Plugin.MoreLogs("Invalid node for sync");
                return;
            }

        }

        internal static TerminalNode HandleAnyNode(TerminalNode currentNode, ref TerminalNode resultNode)
        {
            if (GetNewDisplayText(ConfigSettings.TerminalStuffMain, ref resultNode))
                Plugin.MoreLogs("command found in TerminalStuffMain listing!");

            if (Plugin.instance.CruiserTerm)
            {
                if (CruiserTerm.Status())
                {
                    ParseCruiserTerm(ref currentNode);
                    resultNode = currentNode;
                    return currentNode;
                }
                
            }

            return resultNode;
        }

        internal static void ParseCruiserTerm(ref TerminalNode result)
        {
            if (!Plugin.instance.CruiserTerm)
                return;

            string query = CommonStringStuff.GetCleanedScreenText(Plugin.instance.Terminal);

            if (!CruiserTerm.CanParseWord(query))
            {
                Plugin.Spam("NO ACCESS\n\n\n\n\n\n\n\nCRUISERTERMINAL BLOCKED WORD");
                result = CruiserTerm.NoAccess;
                return;
            }
        }

        internal static void FirstCheck(TerminalNode initialResult)
        {
            string query = CommonStringStuff.GetCleanedScreenText(Plugin.instance.Terminal);
            if (ConfigSettings.TerminalHistory.Value && GameStuff.otherModWords.Any(w => w.ToLower() == query.ToLower()))
                TerminalHistory.AddToCommandHistory(query);

            if (initialResult == null)
                return;

            if (ConfigSettings.TerminalHistory.Value && !initialResult.name.Contains("ParserError") && !initialResult.name.Contains("GeneralError"))
                TerminalHistory.AddToCommandHistory(query);

            VideoPersist(initialResult.name);
            CamPersistance(initialResult.name, initialResult);

            return;
        }

    }
}

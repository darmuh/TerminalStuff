using OpenLib.Common;
using OpenLib.CoreMethods;
using System.Linq;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using TerminalStuff.Networking;
using TerminalStuff.SpecialStuff;
using TerminalStuff.Util;
using static OpenLib.CoreMethods.LogicHandling;
using static TerminalStuff.VisualElements.MoreCamStuff;

namespace TerminalStuff.CommandHandling;

internal class StartofHandling
{
    internal static void HandleShortcutFinal(string cleanedText)
    {
        SetTerminalInput(cleanedText);
        Loggers.LogDebug($"Terminal Input set to:{cleanedText}");
        Plugin.instance.Terminal.OnSubmit();
    }

    internal static TerminalNode HandleParsed(TerminalNode currentNode, ref TerminalNode resultNode)
    {
        HandleAnyNode(currentNode, ref resultNode);
        return resultNode;
    }

    internal static int FindViewInt(TerminalNode givenNode)
    {
        var viewNodes = Commands.GetSpecialCommands();
        foreach (var item in viewNodes)
        {
            if (item.terminalNode == givenNode)
            {
                int nodeNum = item.VerySpecialNum;
                return nodeNum;
            }
        }

        return -1;
    }

    internal static TerminalNode FindViewNode(int givenInt)
    {
        if (givenInt < 0)
            return null!;

        CommandManager result = Commands.GetSpecialCommands().FirstOrDefault(x => x.VerySpecialNum == givenInt);
        if (result == null)
            return null!;
        else
            return result.terminalNode;
    }

    internal static void SyncTerminal(TerminalNode resultNode)
    {
        CheckNetNode(resultNode);
        NetHandler.Instance.SyncRadarZoomRpc(GameStuff.TerminalMapRenderer.cam.orthographicSize);
        if (Plugin.instance.TwoRadarMapsMod)
            NetHandler.Instance.SyncRadarMapRpc(GameStuff.TerminalMapRenderer.targetTransformIndex);
    }

    internal static void CheckNetNode(TerminalNode resultNode)
    {
        if (!ConfigSettings.NetworkedNodes.Value || NetHandler.Instance == null)
            return;

        Loggers.LogInfo("Networked nodes enabled, sending result to server.");
        if (resultNode != null)
        {
            if (Commands.GetSpecialCommands().Any(x => x.terminalNode == resultNode))
            {
                int nodeNum = FindViewInt(resultNode);
                NetHandler.Instance.SyncNodesRpc(Plugin.instance.Terminal.topRightText.text, resultNode.name, resultNode.displayText, nodeNum);
                Loggers.LogInfo($"Valid node detected, nNS true & nodeNum: {nodeNum}");
                return;
            }
            else
            {
                int nodeNum = ((int)ViewCommands.CurrentView);
                NetHandler.Instance.SyncNodesRpc(Plugin.instance.Terminal.topRightText.text, resultNode.name, resultNode.displayText, nodeNum);
                Loggers.LogInfo($"Valid node detected, nNS true & nodeNum is detected as: {nodeNum}");
                return;
            }
        }
        else
        {
            Loggers.LogInfo("Invalid node for sync");
            return;
        }

    }

    internal static TerminalNode HandleAnyNode(TerminalNode currentNode, ref TerminalNode resultNode)
    {
        //if (GetDisplayTextFromCommand(ref resultNode))
        //Loggers.LogInfo("command found in TerminalStuffMain listing!");

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
            Loggers.LogDebug("NO ACCESS\n\n\n\n\n\n\n\nCRUISERTERMINAL BLOCKED WORD");
            result = CruiserTerm.NoAccess;
            return;
        }
    }

    internal static void FirstCheck(TerminalNode initialResult)
    {
        string query = CommonStringStuff.GetCleanedScreenText(Plugin.instance.Terminal);
        if (QoLConfig.TerminalHistory.Value && GameStuff.otherModWords.Any(w => OpenLib.Common.Misc.CompareStringsInvariant(w, query)))
            TerminalHistory.AddToCommandHistory(query);

        if (initialResult == null)
            return;

        if (QoLConfig.TerminalHistory.Value && !initialResult.name.Contains("ParserError") && !initialResult.name.Contains("GeneralError"))
            TerminalHistory.AddToCommandHistory(query);

        CheckVisualPersistance(initialResult.name);

        return;
    }

}

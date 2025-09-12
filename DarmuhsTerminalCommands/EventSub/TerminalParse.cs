using System.Linq;
using TerminalStuff.CommandHandling;
using TerminalStuff.Networking;
using TerminalStuff.StoreTweaks;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;

namespace TerminalStuff.EventSub;

public class TerminalParse
{
    internal static TerminalNode OnParseSent(ref TerminalNode node)
    {
        if (node == null) // handling cases where node is null for some reason
            return Plugin.instance.Terminal.currentNode;

        StartofHandling.FirstCheck(node);

        if (node.name.Equals("0_StoreHub") && StorePacksInfo.AllPacks.Count > 0)
            GetDynamicCost();

        StartofHandling.HandleParsed(node, ref node);

        if (node.displayText == null)
            return node;

        if (!node.displayText.EndsWith("\r\n"))
            node.displayText += "\r\n\r\n";

        NetSync(node);
        return node;

    }

    public static void NetSync(TerminalNode node)
    {
        if (!ConfigSettings.NetworkedNodes.Value || !ConfigSettings.ModNetworking.Value)
            return;

        Loggers.LogInfo("Networked nodes enabled, sending result to server.");
        if (node != null)
        {
            if (MoreCamStuff.IsViewNode(node)) //should be the listing that contains the viewnodes
            {
                int nodeNum = StartofHandling.FindViewInt(node);
                NetHandler.NetNodeReset(true);
                NetHandler.Instance.NodeLoadServerRpc(Plugin.instance.Terminal.topRightText.text, node.name, node.displayText, nodeNum);
                Loggers.LogInfo($"Valid node detected, nNS true & nodeNum: {nodeNum}");
                return;
            }
            else if (!Plugin.instance.splitViewCreated && (bool)node.persistentImage && node.name == "ViewInsideShipCam 1")
            {
                NetHandler.NetNodeReset(true);
                NetHandler.Instance.NodeLoadServerRpc(Plugin.instance.Terminal.topRightText.text, node.name, node.displayText, 100);
                Loggers.LogInfo($"Valid node detected, nNS true & nodeNum: 100 (vanilla view monitor)");
                return;
            }
            else
            {
                NetHandler.NetNodeReset(true);
                NetHandler.Instance.NodeLoadServerRpc(Plugin.instance.Terminal.topRightText.text, node.name, node.displayText);
                Loggers.LogInfo($"Valid node detected, nNS true, no nodeNum set");
                return;
            }
        }

        Loggers.LogDebug("attempting to sync node with other clients over the network");
    }

    internal static void GetDynamicCost()
    {
        foreach (var item in StorePacksInfo.AllPacks)
        {
            item.commandManager.terminalNode.itemCost = StorePacks.GetPriceFromNode(item.terminalNode);
            Loggers.LogDebug($"Updating price for {item.Name} to {item.commandManager.terminalNode.itemCost}");
        }
    }

    internal static TerminalNode OnNewDisplayText(ref TerminalNode node)
    {
        Loggers.LogDebug("newdisplaytext event!");

        if (Plugin.instance.CruiserTerm)
            StartofHandling.ParseCruiserTerm(ref node);

        TerminalNode query = node;
        StorePacks pack = StorePacksInfo.AllPacks.FirstOrDefault(x => x.terminalNode == query);

        if (pack != null)
        {
            node.itemCost = 0;
            Loggers.LogInfo("Updating currentPackList");
            TerminalNode refNode = node;
            StorePacksInfo.Selected = pack;
            if (node.creatureName != string.Empty)
                StorePacksInfo.CurrentPackName = node.creatureName;
        }

        return node;
    }
}

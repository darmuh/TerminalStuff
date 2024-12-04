using System.Collections.Generic;
using static OpenLib.Menus.MenuBuild;

namespace TerminalStuff.EventSub
{
    internal class TerminalParse
    {
        internal static TerminalNode OnParseSent(ref TerminalNode node)
        {
            if (node == null) // handling cases where node is null for some reason
                return Plugin.instance.Terminal.currentNode;

            StartofHandling.FirstCheck(node);

            if (node.name.Equals("0_StoreHub") && ConfigSettings.TerminalStuffMain.storePacks.Count > 0)
                GetDynamicCost();

            if (ConfigSettings.CreateMoreMenus.Value)
            {
                if (InMainMenu(node, MenuBuild.myMenu))
                    Plugin.Spam("got node from menus");
            }

            StartofHandling.HandleParsed(node, ref node);

            if (!node.displayText.EndsWith("\r\n"))
                node.displayText += "\r\n\r\n";

            NetSync(node);
            return node;

        }

        internal static void NetSync(TerminalNode node)
        {
            if (!ConfigSettings.NetworkedNodes.Value || !ConfigSettings.ModNetworking.Value)
                return;

            Plugin.MoreLogs("Networked nodes enabled, sending result to server.");
            if (node != null)
            {
                if (ConfigSettings.TerminalStuffMain.specialListNum.ContainsKey(node)) //should be the listing that contains the viewnodes
                {
                    int nodeNum = StartofHandling.FindViewInt(node);
                    NetHandler.NetNodeReset(true);
                    NetHandler.Instance.NodeLoadServerRpc(Plugin.instance.Terminal.topRightText.text, node.name, node.displayText, nodeNum);
                    Plugin.MoreLogs($"Valid node detected, nNS true & nodeNum: {nodeNum}");
                    return;
                }
                else if(!Plugin.instance.splitViewCreated && (bool)node.persistentImage && node.name == "ViewInsideShipCam 1")
                {
                    NetHandler.NetNodeReset(true);
                    NetHandler.Instance.NodeLoadServerRpc(Plugin.instance.Terminal.topRightText.text, node.name, node.displayText, 100);
                    Plugin.MoreLogs($"Valid node detected, nNS true & nodeNum: 100 (vanilla view monitor)");
                    return;
                }
                else
                {
                    NetHandler.NetNodeReset(true);
                    NetHandler.Instance.NodeLoadServerRpc(Plugin.instance.Terminal.topRightText.text, node.name, node.displayText);
                    Plugin.MoreLogs($"Valid node detected, nNS true, no nodeNum set");
                    return;
                }
            }

            Plugin.Spam("attempting to sync node with other clients over the network");
        }

        internal static void GetDynamicCost()
        {
            foreach (KeyValuePair<TerminalNode, string> item in ConfigSettings.TerminalStuffMain.storePacks)
            {
                if (!item.Key.name.Contains("_confirm"))
                {
                    int itemCost = CostCommands.GetItemListCost(item.Value);
                    item.Key.itemCost = itemCost;
                    Plugin.Spam($"Updating price for {item.Key.name} to {item.Key.itemCost}");
                }
            }
        }

        internal static TerminalNode OnNewDisplayText(ref TerminalNode node)
        {
            Plugin.Spam("newdisplaytext event!");

            if (Plugin.instance.CruiserTerm)
                StartofHandling.ParseCruiserTerm(ref node);

            if (ConfigSettings.TerminalStuffMain.storePacks.TryGetValue(node, out string value))
            {
                node.itemCost = 0;
                Plugin.MoreLogs("Updating currentPackList");
                CostCommands.currentPackList = value;
                if (node.creatureName != string.Empty)
                    CostCommands.currentPackName = node.creatureName;
            }

            return node;
        }
    }
}

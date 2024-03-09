using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static TerminalStuff.AllMyTerminalPatches;
using static TerminalStuff.TerminalEvents;
using static TerminalStuff.DynamicCommands;
using Object = UnityEngine.Object;
using System.Runtime.CompilerServices;

namespace TerminalStuff
{
    internal class MoreCommands
    {
        private static Dictionary<string, PluginInfo> PluginsLoaded = new Dictionary<string, PluginInfo>();
        internal static string currentLobbyName { get; private set; }
        internal static List<TerminalNode> infoOnlyNodes = new List<TerminalNode>();
        internal static List<TerminalNode> otherActionNodes = new List<TerminalNode>();
        internal static bool keepAlwaysOnDisabled = false;

        internal static void GetItemsOnShip(out string displayText)
        {

            LoadGrabbablesOnShip.LoadAllItems();

            StringBuilder sb = new StringBuilder();
            Dictionary<string, int> lineOccurrences = new Dictionary<string, int>();

            foreach (var grabbableItem in LoadGrabbablesOnShip.ItemsOnShip)
            {
                string itemName = grabbableItem.itemProperties.itemName;

                if (!grabbableItem.itemProperties.isScrap && !grabbableItem.isHeld)
                    lineOccurrences[itemName] = lineOccurrences.TryGetValue(itemName, out int count) ? count + 1 : 1;
                // Increment the occurrence count or add to the dictionary with an initial count of 1
            }

            foreach (var kvp in lineOccurrences)
            {
                if (kvp.Value > 1)
                {
                    sb.AppendLine($"{kvp.Key} x{kvp.Value}");
                }
                else
                    sb.AppendLine($"{kvp.Key}");
            }

            displayText = $"Items on ship:\n\n{sb}\n\n";
        }

        private static string ReturnLobbyName()
        {
            string displayText;
            if (GameNetworkManager.Instance.steamLobbyName != null && GameNetworkManager.Instance.steamLobbyName != string.Empty)
            {
                currentLobbyName = GameNetworkManager.Instance.steamLobbyName;

                displayText = $"Lobby Name: {currentLobbyName}\n";
                return displayText;
            }
            else
            {
                displayText = $"Unable to determine Lobby Name. \n";
                return displayText;
            }
        }

        internal static void GetLobbyName(out string displayText)
        {
            displayText = ReturnLobbyName();
            return;
        }

        internal static void ModListCommand(out string displayText)
        {
            
            //TerminalNode node = getTerm.currentNode;

            PluginsLoaded = Chainloader.PluginInfos;
            string concatenatedString = string.Join("\n",
            PluginsLoaded.Select(kvp =>
            $"{kvp.Value.Metadata.Name}, Version: {kvp.Value.Metadata.Version}"));
            displayText = $"Mod List:\n\n{concatenatedString}\n\n";
        }

        internal static void HealCommand(out string displayText)
        {
            
            //TerminalNode node = getTerm.currentNode;

            int getPlayerHealth = GameNetworkManager.Instance.localPlayerController.health;
            //this code snippet is slightly modified from Octolar's Healing Mod, credit to them
            if (getPlayerHealth >= 100)
            {
                Plugin.MoreLogs($"Health = {getPlayerHealth}");
                displayText = $"{ConfigSettings.healIsFullString.Value}\n";
            }

            else
            {
                Plugin.MoreLogs($"Health before = {getPlayerHealth}");
                GameNetworkManager.Instance.localPlayerController.DamagePlayer(-100, false, true);
                GameNetworkManager.Instance.localPlayerController.MakeCriticallyInjured(false);
                int getNewHealth = GameNetworkManager.Instance.localPlayerController.health;
                displayText = $"{ConfigSettings.healString.Value}\nHealth: {GameNetworkManager.Instance.localPlayerController.health}\r\n";
                Plugin.MoreLogs($"Health now = {getNewHealth}");
            }
        }

        internal static void DangerCommand(out string displayText)
        {
            
            //TerminalNode node = getTerm.currentNode;

            if (StartOfRound.Instance.shipDoorsEnabled)
            {
                string dangerLevel = StartOfRound.Instance.currentLevel.riskLevel;
                displayText = ("Current Danger Level: " + dangerLevel + "\n\n");
            }
            else
            {
                displayText = ("Still in orbit.\n\n");
            }
        }

        internal static void ExternalLink(out string displayText)
        {
            
            displayText = $"Taking you to {linktext} now...\n";
            Application.OpenURL(linktext);
            Plugin.Terminal.StartCoroutine(TerminalQuitter(Plugin.Terminal));
        }

        internal static void AlwaysOnDisplay(out string displayText)
        {

            //toggle keeping display always on here
            if (!TerminalStartPatch.alwaysOnDisplay && ConfigSettings.networkedNodes.Value && ConfigSettings.ModNetworking.Value)
            {
                keepAlwaysOnDisabled = false;
                NetHandler.Instance.AoDServerRpc(true);
                displayText = $"Terminal Always-on Display [ENABLED]\r\n";
                //Plugin.Log.LogInfo("set alwaysondisplay to true");
            }
            else if (TerminalStartPatch.alwaysOnDisplay && ConfigSettings.networkedNodes.Value && ConfigSettings.ModNetworking.Value)
            {
                keepAlwaysOnDisabled = true;
                NetHandler.Instance.AoDServerRpc(false);
                displayText = $"Terminal Always-on Display [DISABLED]\r\n";
                //Plugin.Log.LogInfo("set alwaysondisplay to false");
            }
            else if (!TerminalStartPatch.alwaysOnDisplay && !ConfigSettings.networkedNodes.Value)
            {
                keepAlwaysOnDisabled = false;
                TerminalStartPatch.alwaysOnDisplay = true;
                displayText = $"Terminal Always-on Display [ENABLED]\r\n";
            }
            else if (TerminalStartPatch.alwaysOnDisplay && !ConfigSettings.networkedNodes.Value)
            {
                keepAlwaysOnDisabled = true;
                TerminalStartPatch.alwaysOnDisplay = false;
                displayText = $"Terminal Always-on Display [DISABLED]\r\n";
            }
            else
            {
                Plugin.Log.LogError("report this as a bug with alwayson please");
                displayText = "alwayson failed to initiate, report this as a bug please.";
            }
        }
    }
}

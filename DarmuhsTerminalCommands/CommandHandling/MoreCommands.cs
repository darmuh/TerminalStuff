using BepInEx;
using BepInEx.Bootstrap;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalStuff.Patching;
using TerminalStuff.Util;
using UnityEngine;
using static TerminalStuff.DynamicCommands;
using static TerminalStuff.EventSub.TerminalStart;
using static TerminalStuff.TerminalEvents;

namespace TerminalStuff;

internal class MoreCommands
{
    private static Dictionary<string, PluginInfo> PluginsLoaded = [];
    internal static string? CurrentLobbyName { get; private set; }
    internal static bool keepAlwaysOnDisabled = false;

    internal static string GetItemsOnShip()
    {
        string displayText;
        LoadGrabbablesOnShip.LoadAllItems();

        StringBuilder sb = new();
        Dictionary<string, int> lineOccurrences = [];

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
        return displayText;
    }

    private static string ReturnLobbyName()
    {
        string displayText;
        if (GameNetworkManager.Instance.steamLobbyName != null && GameNetworkManager.Instance.steamLobbyName != string.Empty)
        {
            CurrentLobbyName = GameNetworkManager.Instance.steamLobbyName;

            displayText = $"Lobby Name: {CurrentLobbyName}\n";
            return displayText;
        }
        else
        {
            displayText = $"Unable to determine Lobby Name. \n";
            return displayText;
        }
    }

    internal static string GetLobbyName()
    {
        string displayText = ReturnLobbyName();
        return displayText;
    }

    internal static string ModListCommand()
    {
        string displayText;
        PluginsLoaded = Chainloader.PluginInfos;
        string concatenatedString = string.Join("\n",
        PluginsLoaded.Select(kvp =>
        $"{kvp.Value.Metadata.Name}, Version: {kvp.Value.Metadata.Version}"));
        displayText = $"Mod List:\n\n{concatenatedString}\n\n";
        return displayText;
    }

    internal static string HealCommand()
    {
        string displayText;
        int getPlayerHealth = GameNetworkManager.Instance.localPlayerController.health;
        //this code snippet is slightly modified from Octolar's Healing Mod, credit to them
        if (getPlayerHealth >= 100)
        {
            Loggers.LogInfo($"Health = {getPlayerHealth}");
            displayText = $"{ConfigSettings.HealIsFullString.Value}\n";
            return displayText;
        }

        else
        {
            Loggers.LogInfo($"Health before = {getPlayerHealth}");
            GameNetworkManager.Instance.localPlayerController.DamagePlayer(-100, false, true);
            GameNetworkManager.Instance.localPlayerController.MakeCriticallyInjured(false);
            int getNewHealth = GameNetworkManager.Instance.localPlayerController.health;
            displayText = $"{ConfigSettings.HealString.Value}\nHealth: {GameNetworkManager.Instance.localPlayerController.health}\r\n";
            Loggers.LogInfo($"Health now = {getNewHealth}");
            return displayText;
        }
    }

    internal static string DangerCommand()
    {
        string displayText;

        if (StartOfRound.Instance.shipDoorsEnabled)
        {
            string dangerLevel = StartOfRound.Instance.currentLevel.riskLevel;
            displayText = "Current Danger Level: " + dangerLevel + "\n\n";
            return displayText;
        }
        else
        {
            displayText = "Still in orbit.\n\n";
            return displayText;
        }
    }

    internal static string FirstLinkDo()
    {
        int linkID = 0;
        string displayText = ExternalLink(linkID);
        return displayText;
    }
    internal static string SecondLinkDo()
    {
        int linkID = 1;
        string displayText = ExternalLink(linkID);
        return displayText;
    }

    internal static string FirstLinkAsk()
    {
        int linkID = 0;
        string displayText = LinksAsk(linkID);
        return displayText;
    }
    internal static string SecondLinkAsk()
    {
        int linkID = 1;
        string displayText = LinksAsk(linkID);
        return displayText;
    }

    internal static string FirstLinkDeny()
    {
        int linkID = 0;
        string displayText = LinksDeny(linkID);
        return displayText;
    }
    internal static string SecondLinkDeny()
    {
        int linkID = 1;
        string displayText = LinksDeny(linkID);
        return displayText;
    }

    internal static void GetLinkText(int linkID)
    {
        if (linkID == 0)
            Linktext = ConfigSettings.CustomLink.Value;
        else
            Linktext = ConfigSettings.CustomLink2.Value;
    }

    internal static string ExternalLink(int linkID)
    {
        GetLinkText(linkID);
        string displayText = $"Taking you to {Linktext} now...\n";
        Application.OpenURL(Linktext);
        Plugin.instance.Terminal.StartCoroutine(TerminalQuitter(Plugin.instance.Terminal));
        return displayText;
    }

    internal static string LinksAsk(int linkID)
    {
        GetLinkText(linkID);
        string displayText = $"Would you like to be taken to the following link?\n\n\t{Linktext}\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n\n";
        return displayText;
    }

    internal static string LinksDeny(int linkID)
    {
        GetLinkText(linkID);
        string displayText = $"You have cancelled visiting the site:\n\n\t{Linktext}\n\n";
        return displayText;
    }

    internal static string AlwaysOnDisplay()
    {
        string displayText;
        if (!alwaysOnDisplay && ConfigSettings.NetworkedNodes.Value && ConfigSettings.ModNetworking.Value)
        {
            keepAlwaysOnDisabled = false;
            NetHandler.Instance.AoDServerRpc(true);
            displayText = $"Terminal Always-on Display [ENABLED]\r\n";
            return displayText;
            //Plugin.Log.LogInfo("set alwaysondisplay to true");
        }
        else if (alwaysOnDisplay && ConfigSettings.NetworkedNodes.Value && ConfigSettings.ModNetworking.Value)
        {
            keepAlwaysOnDisabled = true;
            NetHandler.Instance.AoDServerRpc(false);
            displayText = $"Terminal Always-on Display [DISABLED]\r\n";
            return displayText;
            //Plugin.Log.LogInfo("set alwaysondisplay to false");
        }
        else if (!alwaysOnDisplay && !ConfigSettings.NetworkedNodes.Value)
        {
            keepAlwaysOnDisabled = false;
            alwaysOnDisplay = true;
            displayText = $"Terminal Always-on Display [ENABLED]\r\n";
            return displayText;
        }
        else if (alwaysOnDisplay && !ConfigSettings.NetworkedNodes.Value)
        {
            keepAlwaysOnDisabled = true;
            alwaysOnDisplay = false;
            displayText = $"Terminal Always-on Display [DISABLED]\r\n";
            return displayText;
        }
        else
        {
            Plugin.Log.LogError("report this as a bug with alwayson please");
            displayText = "alwayson failed to initiate, report this as a bug please.";
            return displayText;
        }
    }
}

using GameNetcodeStuff;
using OpenLib.CoreMethods;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalStuff.Configs;
using TerminalStuff.SpecialStuff;
using UnityEngine;
using static TerminalStuff.VisualElements.MoreCamStuff;
using static TerminalStuff.VisualElements.TerminalCustomizer;
using static TerminalStuff.Util.StringStuff;
using TerminalStuff.Util;
using TerminalStuff.StoreTweaks;

#pragma warning disable IDE0130 // OpenLib depdendent class name
namespace TerminalStuff;
#pragma warning restore IDE0130

public static class TerminalEvents
{
    internal static string lastText = "";
    internal static string VideoErrorMessage = "";
    public static bool clockDisabledByCommand = false;
#pragma warning disable IDE1006
    public static TerminalSettings terminalSettings { get; internal set; } = new(); //used in OpenLib compat
#pragma warning restore IDE1006
    internal static bool quitTerminalEnum = false;
    internal static List<CommandManager> PurchasePacks { get; set; } = [];

    internal static Color transparent = new(0, 0, 0, 0);

    internal static void CreateStorePacks()
    {
        if (!Commands.TerminalPurchasePacks.Value)
            return;

        if (ConfigSettings.PurchasePackCommands.Value == "")
            return;

        Dictionary<string, string> keywordAndItems = GetKeywordAndItemNames(ConfigSettings.PurchasePackCommands.Value);

        if (keywordAndItems.Count == 0)
            return;

        foreach (KeyValuePair<string, string> item in keywordAndItems)
        {
            Loggers.LogDebug($"setting {item.Key} keyword to purchase pack with items: {item.Value}");

            if (PurchasePacks.Count != 0)
            {
                CommandManager match = PurchasePacks.FirstOrDefault(p => p.KeywordList.Contains(item.Key));
                if (match != null)
                {
                    match.RegisterCommand();
                    continue;
                }
            }

            
            CommandManager packCmd = Commands.AddLocalCommmandManualWords($"{item.Key}_PP", Commands.TerminalPurchasePacks, [item.Key], StorePacks.AskPurchasePack, "Comfort", true, false);
            Commands.AddStoreCommand(packCmd, $"{item.Key}", 0, StorePacks.CompletePurchasePack, () => $"You have cancelled the purchase of Purchase Pack [{item.Key}].\r\n\r\n", 0, true);
            packCmd.RegisterCommand();
            PurchasePacks.Add(packCmd);

            StorePlus.ExcludedNodesFromAutoGen.Add(packCmd.terminalNode);

            StorePacks pack = StorePacksInfo.AllPacks.FirstOrDefault(s => s.Name == item.Key);

            if (pack != null)
                pack.UpdateExisting(item.Value, packCmd.terminalNode);
            else
                pack = new(item.Key, item.Value, packCmd.terminalNode);
        }
    }

    internal static TerminalNode GetNodeFromList(string query, Dictionary<string, TerminalNode> nodeListing)
    {
        foreach (KeyValuePair<string, TerminalNode> pairValue in nodeListing)
        {
            if (pairValue.Key == query)
            {
                return pairValue.Value;
            }
        }
        return null!; // No matching command found for the given query
    }

    internal static string RandomSuit()
    {
        SuitCommands.GetRandomSuit(out string suitString);
        return suitString;
    }

    internal static string QuitTerminalCommand()
    {
        string text = $"{ConfigSettings.QuitString.Value}";

        Plugin.instance.Terminal.StartCoroutine(TerminalQuitter(Plugin.instance.Terminal));
        return text;
    }

    internal static IEnumerator TerminalQuitter(Terminal terminal)
    {
        if (quitTerminalEnum)
            yield break;

        if (Plugin.instance.CruiserTerm)
        {
            if (Compatibility.CruiserTerm.Status())
            {
                quitTerminalEnum = true;
                yield return new WaitForSeconds(0.5f);
                Compatibility.CruiserTerm.Quit();
                quitTerminalEnum = false;
                yield break;
            }
        }

        quitTerminalEnum = true;
        yield return new WaitForSeconds(0.5f);
        terminal.QuitTerminal();
        quitTerminalEnum = false;
    }

    internal static string ClockToggle()
    {
        if (TerminalClockStuff.textComponent == null)
            return "Unable to find Terminal Clock component!\r\n\r\n";

        if (StartOfRound.Instance.inShipPhase)
            return "Unable to determine time zone while in Orbit!\r\n\r\n";

        if (!clockDisabledByCommand && TerminalClockStuff.IsClockVisible())
        {
            Loggers.LogDebug("Disabling clock via command!");
            clockDisabledByCommand = true;
            TerminalClockStuff.SetClockVisible(false);
            return "Terminal Clock [DISABLED].\r\n";
        }
        else
        {
            Loggers.LogDebug("Enabling clock via command!");
            clockDisabledByCommand = false;
            TerminalClockStuff.SetClockVisible(true);
            return "Terminal Clock [ENABLED].\r\n";
        }
    }

    internal static string GetCleanedScreenText(Terminal __instance)
    {
        string s = __instance.screenText.text[^__instance.textAdded..];

        return RemovePunctuation(s);
    }

    private static string RemovePunctuation(string s) //copied from game files
    {
        StringBuilder stringBuilder = new();
        foreach (char c in s)
        {
            if (!char.IsPunctuation(c))
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().ToLower();
    }

    internal static void ShouldLockPlayerCamera(bool value, PlayerControllerB localPlayer)
    {
        if (!QoLConfig.LockCameraInTerminal.Value)
            return;

        if (localPlayer != null)
        {
            localPlayer.disableLookInput = !value;
            Loggers.LogInfo($"ShouldLockPlayerCamera set to: {!value}");
        }
    }

    internal static void ShouldDisableTerminalLight(bool value, string setting)
    {
        if (setting == "nochange")
            return;

        if (Plugin.instance.Terminal.terminalLight.enabled == value)
        {
            Plugin.instance.Terminal.terminalLight.enabled = !value;
            Loggers.LogInfo($"terminalLight set to {!value} for setting: {setting}");
        }

    }

    internal static string RefreshCustomizationCommand()
    {
        string text = $"Refreshing TerminalCustomization from config.\n\n";
        TerminalCustomization();
        return text;
    }

    internal static int PlayerNameToTargetInt(string query, List<TransformAndName> radarTargets)
    {
        query = query.TrimStart();

        if (query.Length <= 2) //too short to compare names
            return -1;

        Dictionary<int, int> nameToScore = [];

        for (int i = 0; i < radarTargets.Count; i++) //iterate through all targets
        {
            if (TargetIsValid(radarTargets[i])) //verify target is valid
            {
                if (OpenLib.Common.Misc.StringStartsWithInvariant(radarTargets[i].name, query[..2]))
                {
                    int score = Levenshtein.Distance(query, radarTargets[i].name); //get score at current target
                    Loggers.LogDebug($"TargetNum {i} has score {score}");
                    nameToScore.Add(i, score); //map score to current target
                }
                else
                    Loggers.LogDebug($"name [ {radarTargets[i].name} ] does not match start of query [ {query[..2]} ]");
            }
        }

        if (nameToScore.Count == 0)
            return -1;

        return nameToScore.OrderBy(x => x.Value).First().Key; //order by score values and return targetnum with highest score
    }

    internal static string PlayerNameToTargetString(string query, List<TransformAndName> radarTargets)
    {
        query = query.TrimStart();

        if (query.Length <= 2) //too short to compare names
            return "";

        Dictionary<string, int> nameToScore = [];

        for (int i = 0; i < radarTargets.Count; i++) //iterate through all targets
        {
            if (TargetIsValid(radarTargets[i])) //verify target is valid
            {
                if (OpenLib.Common.Misc.StringStartsWithInvariant(radarTargets[i].name, query[..2]))
                {
                    int score = Levenshtein.Distance(query, radarTargets[i].name); //get score at current target
                    Loggers.LogDebug($"TargetNum {i} has score {score}");
                    nameToScore.Add(radarTargets[i].name, score); //map score to current target
                }
                else
                    Loggers.LogDebug($"name [ {radarTargets[i].name} ] does not match start of query [ {query[..2]} ]");
            }
        }

        if (nameToScore.Count == 0)
            return "";

        return nameToScore.OrderBy(x => x.Value).First().Key; //order by score values and return targetnum with highest score
    }

    internal static void LoadAndSync(TerminalNode node)
    {
        if (node == null)
            return;

        Plugin.instance.Terminal.LoadNewNode(node);
        Loggers.LogDebug($"Loading node!");

        if (!ConfigSettings.NetworkedNodes.Value || !ConfigSettings.ModNetworking.Value)
            return;

        Loggers.LogDebug($"Syncing with TerminalStuff!");
        EventSub.TerminalParse.NetSync(node);
    }
}

public class TerminalSettings
{
    public TerminalNode startPage = null!;
#pragma warning disable IDE1006
    public string startPageValue { get; internal set; } = string.Empty; //used by OpenLib
#pragma warning restore IDE1006
    internal void StartPage(string entry)
    {
        startPageValue = entry;
        startPage = null!;
        if (OpenLib.Common.Misc.StringStartsWithInvariant(startPageValue, "none") || startPageValue.Length < 2)
            return;

        if (DynamicBools.TryGetKeyword(startPageValue, out TerminalKeyword keyword))
        {
            startPage = keyword.specialKeywordResult;
        }
        else
            Loggers.WARNING($"Unable to find matching keyword for start page - {entry}");

        //"None", "Home", "Moons", "Store", "Help", "Other", "Bestiary", "Storage", "Sigurd", "Video"
    }
}


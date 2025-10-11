using System.Collections;
using System.Collections.Generic;
using TerminalStuff.CommandHandling;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TerminalStuff.SpecialStuff;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;
using TerminalStuff.StoreTweaks;
using TerminalStuff.MoonsTweaks;
using UnityEngine;
using static OpenLib.CoreMethods.LogicHandling;
using static TerminalStuff.TerminalEvents;
using TerminalStuff.Networking;

namespace TerminalStuff.EventSub;

public class TerminalStart
{
    internal static bool AlwaysOnDisplay = false;

    //Ignore Naming warnings since used in other mods, including OpenLib
#pragma warning disable IDE1006
    public static TerminalNode startNode { get; internal set; } = null!;
    public static TerminalNode helpNode { get; internal set; } = null!;
#pragma warning restore IDE1006
    internal static List<TerminalNode> vanillaNodes = [];
    internal static TerminalNode viewMonitorVanilla = null!;
    internal static TerminalNode switchNodeVanilla = null!;
    internal static TerminalKeyword RouteKeyword = null!;
    internal static bool delayStartEnum = false;

    internal static void OnTerminalStart()
    {
        TerminalStartGroup();
        TerminalStartGroupDelay();
    }

    internal static void TerminalStartGroup()
    {
        Loggers.LogInfo("Upgrading terminal with my stuff, smile.");

        OtherModWords();
        OverWriteTextNodes();
        VanillaNodesCache();
        TerminalClockStuff.MakeClock();
        ShortcutBindings.InitSavedShortcuts();
        TerminalCustomizer.TerminalCustomization();
        SaveManager.InitUnlocks(); // sync upgrades status for this save
        StorePlus.GetStoreItems(); // StorePlus/StorePacks init
    }

    private static void OtherModWords()
    {
        if (Plugin.instance.ITAPI)
            InteractiveAPI.GetITAPIWords();
    }


    private static void OverWriteTextNodes()
    {
        Loggers.LogInfo("updating displaytext for help");
        helpNode = Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[13];
        Plugin.instance.Terminal.terminalNodes.specialNodes[4].displayText = Plugin.instance.Terminal.terminalNodes.specialNodes[4].displayText.Replace("12", $"[GetMaxPossibleItems]");

        if (!GameStuff.OneTimeOnly)
        {
            string original = helpNode.displayText;
            //Loggers.LogDebug(original);
            string replacement = original.Replace("To see the list of moons the autopilot can route to.", "List of moons the autopilot can route to.").Replace("To see the company store's selection of useful items.", "Company store's selection of useful items.");
            //Loggers.LogDebug($"{replacement}");

            Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[13].displayText = replacement;
            Loggers.LogDebug("~~~~~~~~~~~~~~~~~~~~~~~~~~~~ HELP MODIFIED ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~");
            GameStuff.OneTimeOnly = true;
        }

        OpenLib.CoreMethods.AddingThings.AddKeywordToExistingNode("home", Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[1], true); //startNode

    }

    internal static void VanillaNodesCache()
    {
        vanillaNodes.Clear();

        if (OpenLib.CoreMethods.DynamicBools.TryGetKeyword("store", out TerminalKeyword storeWord))
        {
            TerminalNode storeNode = storeWord.specialKeywordResult;
            vanillaNodes.Add(storeNode);
            Loggers.LogDebug("storeNode cached");
        }

        if (OpenLib.CoreMethods.DynamicBools.TryGetKeyword("moons", out TerminalKeyword moonsWord))
        {
            TerminalNode moonsNode = moonsWord.specialKeywordResult;
            vanillaNodes.Add(moonsNode);
            Loggers.LogDebug("moonsNode cached");
        }

        if (OpenLib.CoreMethods.DynamicBools.TryGetKeyword("bestiary", out TerminalKeyword bestiaryWord))
        {
            TerminalNode bestiaryNode = bestiaryWord.specialKeywordResult;
            vanillaNodes.Add(bestiaryNode);
            Loggers.LogDebug("bestiaryNode cached");
        }

        if (OpenLib.CoreMethods.DynamicBools.TryGetKeyword("route", out RouteKeyword))
            Loggers.LogDebug("routeKeyword grabbed!");

        if (!Plugin.instance.splitViewCreated)
        {
            Loggers.LogDebug("Trying to cache vanilla view monitor");
            if (TryGetFromAllNodes("ViewInsideShipCam 1", out viewMonitorVanilla))
            {
                vanillaNodes.Add(viewMonitorVanilla);
                Loggers.LogDebug("cached vanilla view monitor!");
            }
        }

    }

    internal static void TerminalStartGroupDelay()
    {
        Loggers.LogInfo("Starting TerminalDelayStartEnumerator");
        Plugin.instance.Terminal.StartCoroutine(TerminalDelayStartEnumerator());
    }

    internal static IEnumerator TerminalDelayStartEnumerator()
    {
        if (delayStartEnum)
            yield break;

        delayStartEnum = true;
        yield return new WaitForSeconds(1);
        MenuBuild.CreateDarmuhsTerminalStuffMenus();
        Loggers.LogInfo("1 Second delay methods starting.");
        SplitViewChecks.CheckForSplitView("neither");
        Loggers.LogInfo("disabling cams views");
        ViewCommands.isVideoPlaying = false;
        //TerminalClockStuff.StartClockCoroutine();
        AlwaysOnStart(Plugin.instance.Terminal, startNode);
        MoonsPlus.MoonsPlusSetup();
        yield return new WaitForSeconds(0.1f);
        Plugin.instance.Terminal.topRightText.text = $"${Plugin.instance.Terminal.groupCredits}"; //fix creds display for alwayson
        StartCheck(Plugin.instance.Terminal, startNode);
        DebugShowInfo();
        delayStartEnum = false;
    }

    public static void ToggleScreen(bool status)
    {
        OpenLib.Common.CommonTerminal.ToggleScreen(status);
    }

    internal static void InitiateTerminalStuff()
    {
        if (Plugin.instance.Terminal == null)
            return;

        terminalSettings.StartPage(QoLConfig.TerminalStartPage.Value);
    }

    private static void AlwaysOnStart(Terminal thisterm, TerminalNode startNode)
    {

        if (AlwaysOnStuff.screenSettings.AlwaysOn && !AlwaysOnStuff.screenSettings.inUse)
        {
            Loggers.LogDebug("Setting AlwaysOn Display.");
            if (ConfigSettings.NetworkedNodes.Value && ConfigSettings.ModNetworking.Value)
            {
                Loggers.LogDebug("network nodes enabled, syncing alwayson status");
                NetHandler.Instance.AlwaysOnDisplaySyncRpc(true);
            }
            else
            {
                AlwaysOnDisplay = true;
                ToggleScreen(true);
                thisterm.LoadNewNode(startNode);
            }

            if (QoLConfig.TerminalLightBehaviour.Value == "alwayson")
                ShouldDisableTerminalLight(false, "alwayson");

        }
    }

    private static void StartCheck(Terminal thisterm, TerminalNode startNode)
    {
        if (startNode == null)
            startNode = Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[13];

        if (!ConfigSettings.ModNetworking.Value || !ConfigSettings.NetworkedNodes.Value)
        {
            Loggers.LogDebug("Networking disabled, returning...");
            thisterm.LoadNewNode(startNode);
            return;
        }

        if (GameNetworkManager.Instance.localPlayerController.IsHost)
        {
            GameStuff.TerminalMapRenderer.SwitchRadarTargetAndSync(0); //fix vanilla bug where you need to switch map target at start
            NetHandler.Instance.SyncRadarZoomRpc(QoLConfig.TerminalRadarDefaultZoom.Value); //host only at load-in
            thisterm.LoadNewNode(startNode);
            StartofHandling.CheckNetNode(startNode);
            return;
        }
        else
        {
            Plugin.instance.Terminal.StartCoroutine(ClientStuffDelayed());
            return;
        }
    }

    private static IEnumerator ClientStuffDelayed()
    {
        yield return new WaitForSeconds(1);
        Loggers.LogDebug("------------ CLIENT JUST LOADED --------------");
        Loggers.LogDebug("grabbing node from host");
        Loggers.LogDebug("------------ CLIENT JUST LOADED --------------");

        NetHandler.Instance.GetHostTerminalRpc();
    }

    private static void DebugShowInfo()
    {
        Loggers.LogDebug($"Terminal Keywords Count: {Plugin.instance.Terminal.terminalNodes.allKeywords.Length}");
        Loggers.LogDebug($"Plugin.Allnodes: {Plugin.Allnodes.Count}");
        Loggers.LogDebug($"Commands.AllCommands: {Commands.AllCommands.Count}");
        Loggers.LogDebug("------------------------ end of darmuh's debug info ------------------------");

    }
}

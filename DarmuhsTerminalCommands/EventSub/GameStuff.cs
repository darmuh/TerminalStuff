using HarmonyLib;
using System;
using System.Collections.Generic;
using TerminalStuff.CommandHandling;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TerminalStuff.MoonsTweaks;
using TerminalStuff.SpecialStuff;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;
using static OpenLib.Common.StartGame;
using static TerminalStuff.VisualElements.AlwaysOnStuff;

namespace TerminalStuff.EventSub;

internal class GameStuff
{
    //cachedstuff
    private static ManualCameraRenderer? maprenderer;
    internal static ManualCameraRenderer TerminalMapRenderer
    {
        get
        {
            maprenderer = GetMapRenderer();
            return maprenderer;
        }

        set => maprenderer = value;
    }

    internal static List<string> otherModWords = [];

    internal static bool OneTimeOnly { get; set; } = false;
    internal static void OnGameStart()
    {
        CompatibilityCheck();
        OneTimeOnly = false;
        MenuBuild.MoreInit();
    }

    internal static void OnChangeLevel()
    {
        Loggers.LogDebug("OnChangeLevel!");
        if (Commands.TerminalMoonsPlus.Value)
            MoonsPlus.MoonListing.Do(x => x.UpdateInfo());
    }

    internal static void OnShipReset()
    {
        if (Commands.TerminalMoonsPlus.Value)
            MoonsPlus.ShipReset();

        SaveManager.ResetUnlocks();
    }

    internal static ManualCameraRenderer GetMapRenderer()
    {
        if (Plugin.instance.TwoRadarMapsMod)
            return TwoRadarMapsCompatibility.GetTerminalMap();
        else
            return StartOfRound.Instance.mapScreen;
    }

    internal static void ResetClockStatus()
    {
        if (!QoLConfig.TerminalClock.Value)
            return;

        TerminalClockStuff.SetClockVisible(false);
    }

    internal static void OnNextDay()
    {
        ResetClockStatus();
    }

    internal static void OnStartOfRoundStart()
    {
        Plugin.instance.splitViewCreated = false;
        SplitViewChecks.InitSplitViewObjects(); //addSplitViewObjects
        Bools.ResetEnumBools(); // resets all enum bools
        TerminalClockStuff.SetClockVisible(false); // disable clock on game restart
        ViewCommands.CurrentView = ViewCommands.ViewMode.None;
    }

    internal static void OnPlayerSpawn()
    {

        if (screenSettings == null)
            return;

        screenSettings.Update(QoLConfig.TerminalScreen.Value);
        if (!screenSettings.inUse && (screenSettings.AlwaysOn || screenSettings.Dynamic))
        {
            Loggers.LogDebug("Enabling screen!");
            if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf && StartOfRound.Instance.localPlayerController.isInHangarShipRoom)
                Plugin.instance.Terminal.terminalUIScreen.gameObject.SetActive(true);
        }
        else
            Loggers.LogDebug($"Screen setting set to inUse - {screenSettings.inUse} ");

    }

    private static void CompatibilityCheck()
    {
        if (SoftCompatibility("BMX.LobbyCompatibility", ref Plugin.instance.LobbyCompat))
        {
            Loggers.LogInfo("LobbyCompatibility detected, setting appropriate Lobby Compatibility Level depending on networking status");
            BMX_LobbyCompat.SetCompat(ConfigSettings.ModNetworking.Value);
        }

        if (SoftCompatibility("com.malco.lethalcompany.moreshipupgrades", ref Plugin.instance.LateGameUpgrades))
        {
            Loggers.LogDebug("Lategame Upgrades detected!");
            //manual list of commands added by LGU that do not show up in ITAPI registered commands dictionary
            otherModWords.AddRange(["demon", "lookup", "bruteforce", "initattack", "atk", "cd", "cooldown", "lategame", "lgc", "forcecredits", "load", "quantum", "intern", "interns"]);
        }

        if (SoftCompatibility("mborsh.CruiserTerminal", ref Plugin.instance.CruiserTerm))
        {
            Loggers.LogDebug("CruiserTerminal by mborsh detected!");
            Version minVersion = new("1.1.0");
            if (OpenLib.Common.Misc.GetPluginVersion("mborsh.CruiserTerminal") < minVersion)
            {
                Plugin.instance.CruiserTerm = false;
                Loggers.WARNING("Older CruiserTerminal Mod detected! Compatibility functions are disabled!");
            }
        }

        if (OpenLib.Plugin.instance.LethalConfig)
            OpenLib.Compat.LethalConfigSoft.AddButton("Terminal Customization", "Refresh Customizations", "Press this button to refresh all terminal customizations", "Refresh", TerminalCustomizer.TerminalCustomization);
    }
}

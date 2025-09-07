using HarmonyLib;
using System;
using System.Collections.Generic;
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
        MoreCamStuff.ResetPluginInstanceBools(); //reset view command bools
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

        if (SoftCompatibility("Rozebud.FovAdjust", ref Plugin.instance.FovAdjust))
            Loggers.LogDebug("Rozebud's FovAdjust detected!");

        if (SoftCompatibility("RickArg.lethalcompany.helmetcameras", ref Plugin.instance.HelmetCamsMod))
            Loggers.LogDebug("Helmet Cameras by Rick Arg detected!");

        if (SoftCompatibility("SolosBodycams", ref Plugin.instance.SolosBodyCamsMod))
            Loggers.LogDebug("SolosBodyCams by CapyCat (Solo) detected!");

        if (SoftCompatibility("Zaggy1024.OpenBodyCams", ref Plugin.instance.OpenBodyCamsMod))
            Loggers.LogDebug("OpenBodyCams by Zaggy1024 detected!");

        if (SoftCompatibility("Zaggy1024.TwoRadarMaps", ref Plugin.instance.TwoRadarMapsMod))
            Loggers.LogDebug("TwoRadarMaps by Zaggy1024 detected!");

        if (SoftCompatibility("com.malco.lethalcompany.moreshipupgrades", ref Plugin.instance.LateGameUpgrades))
        {
            Loggers.LogDebug("Lategame Upgrades detected!");
            //manual list of commands added by LGU that do not show up in ITAPI registered commands dictionary
            otherModWords.AddRange(["demon", "lookup", "bruteforce", "initattack", "atk", "cd", "cooldown", "lategame", "lgc", "forcecredits", "load", "quantum", "intern", "interns"]);
        }


        if (SoftCompatibility("darmuh.suitsTerminal", ref Plugin.instance.suitsTerminal))
            Loggers.LogDebug("suitsTerminal detected!");

        if (SoftCompatibility("TerminalFormatter", ref Plugin.instance.TerminalFormatter))
            Loggers.LogDebug("Terminal Formatter by mrov detected!");

        if (SoftCompatibility("com.github.darmuh.LethalConstellations", ref Plugin.instance.Constellations))
            Loggers.LogDebug("LethalConstellations detected ^.^");

        if (SoftCompatibility("ShipInventory", ref Plugin.instance.ShipInventory))
            Loggers.LogDebug("ShipInventory compatibility enabled!");

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

        if (SoftCompatibility("WhiteSpike.InteractiveTerminalAPI", ref Plugin.instance.ITAPI))
            Loggers.LogDebug("InteractiveTerminalAPI detected!");

        if (SoftCompatibility("imabatby.lethallevelloader", ref Plugin.instance.LethalLevelLoader))
            Loggers.LogDebug("LethalLevelLoader by IAmBatby detected!");
        if (SoftCompatibility("WeatherTweaks", ref Plugin.instance.WeatherTweaks))
            Loggers.LogDebug("WeatherTweaks by mrov detected!");

        if (SoftCompatibility("ShaosilGaming.GeneralImprovements", ref Plugin.instance.GenImprovements))
            Loggers.LogDebug("Adding compatibility for General Improvements by Shaosil!");

        if (OpenLib.Plugin.instance.LethalConfig)
            OpenLib.Compat.LethalConfigSoft.AddButton("Terminal Customization", "Refresh Customizations", "Press this button to refresh all terminal customizations", "Refresh", TerminalCustomizer.TerminalCustomization);
    }
}

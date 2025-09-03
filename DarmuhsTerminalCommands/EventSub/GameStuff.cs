using HarmonyLib;
using System;
using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.PluginCore;
using TerminalStuff.SpecialStuff;
using static OpenLib.Common.StartGame;
using static TerminalStuff.AlwaysOnStuff;

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
        set
        {
            maprenderer = value;
        }
    }

    internal static List<string> otherModWords = [];

    internal static bool oneTimeOnly = false;
    internal static void OnGameStart()
    {
        CompatibilityCheck();
        oneTimeOnly = false;
    }

    internal static void OnChangeLevel()
    {
        Plugin.Spam("OnChangeLevel!");
        if(Commands.TerminalMoonsPlus.Value)
            MoonsPlus.MoonListing.Do(x => x.UpdateInfo());
    }

    internal static void OnShipReset()
    {
        if(Commands.TerminalMoonsPlus.Value)
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
        BoolStuff.ResetEnumBools(); // resets all enum bools
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
            Plugin.Spam("Enabling screen!");
            if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf && StartOfRound.Instance.localPlayerController.isInHangarShipRoom)
                Plugin.instance.Terminal.terminalUIScreen.gameObject.SetActive(true);
        }
        else
            Plugin.Spam($"Screen setting set to inUse - {screenSettings.inUse} ");

    }

    private static void CompatibilityCheck()
    {
        if (SoftCompatibility("BMX.LobbyCompatibility", ref Plugin.instance.LobbyCompat))
        {
            Plugin.MoreLogs("LobbyCompatibility detected, setting appropriate Lobby Compatibility Level depending on networking status");
            BMX_LobbyCompat.SetCompat(ConfigSettings.ModNetworking.Value);
        }
        
        if (SoftCompatibility("Rozebud.FovAdjust", ref Plugin.instance.FovAdjust))
            Plugin.Spam("Rozebud's FovAdjust detected!");
        
        if (SoftCompatibility("RickArg.lethalcompany.helmetcameras", ref Plugin.instance.HelmetCamsMod))
            Plugin.Spam("Helmet Cameras by Rick Arg detected!");
        
        if (SoftCompatibility("SolosBodycams", ref Plugin.instance.SolosBodyCamsMod))
            Plugin.Spam("SolosBodyCams by CapyCat (Solo) detected!");
        
        if (SoftCompatibility("Zaggy1024.OpenBodyCams", ref Plugin.instance.OpenBodyCamsMod))
            Plugin.Spam("OpenBodyCams by Zaggy1024 detected!");
        
        if (SoftCompatibility("Zaggy1024.TwoRadarMaps", ref Plugin.instance.TwoRadarMapsMod))
            Plugin.Spam("TwoRadarMaps by Zaggy1024 detected!");
        
        if (SoftCompatibility("com.malco.lethalcompany.moreshipupgrades", ref Plugin.instance.LateGameUpgrades))
        {
            Plugin.Spam("Lategame Upgrades detected!");
            //manual list of commands added by LGU that do not show up in ITAPI registered commands dictionary
            otherModWords.AddRange(["demon", "lookup", "bruteforce", "initattack", "atk", "cd", "cooldown", "lategame", "lgc", "forcecredits", "load", "quantum", "intern", "interns"]);
        }
            
        
        if (SoftCompatibility("darmuh.suitsTerminal", ref Plugin.instance.suitsTerminal))
            Plugin.Spam("suitsTerminal detected!");
        
        if (SoftCompatibility("TerminalFormatter", ref Plugin.instance.TerminalFormatter))
            Plugin.Spam("Terminal Formatter by mrov detected!");
        
        if (SoftCompatibility("com.github.darmuh.LethalConstellations", ref Plugin.instance.Constellations))
            Plugin.Spam("LethalConstellations detected ^.^");
        
        if (SoftCompatibility("ShipInventory", ref Plugin.instance.ShipInventory))
            Plugin.Spam("ShipInventory compatibility enabled!");
        
        if (SoftCompatibility("mborsh.CruiserTerminal", ref Plugin.instance.CruiserTerm))
        {
            Plugin.Spam("CruiserTerminal by mborsh detected!");
            Version minVersion = new("1.1.0");
            if(OpenLib.Common.Misc.GetPluginVersion("mborsh.CruiserTerminal") < minVersion)
            {
                Plugin.instance.CruiserTerm = false;
                Plugin.WARNING("Older CruiserTerminal Mod detected! Compatibility functions are disabled!");
            }
        }     

        if (SoftCompatibility("WhiteSpike.InteractiveTerminalAPI", ref Plugin.instance.ITAPI))
            Plugin.Spam("InteractiveTerminalAPI detected!");

        if (SoftCompatibility("imabatby.lethallevelloader", ref Plugin.instance.LethalLevelLoader))
            Plugin.Spam("LethalLevelLoader by IAmBatby detected!");
        if (SoftCompatibility("WeatherTweaks", ref Plugin.instance.WeatherTweaks))
            Plugin.Spam("WeatherTweaks by mrov detected!");

        if (SoftCompatibility("ShaosilGaming.GeneralImprovements", ref Plugin.instance.GenImprovements))
            Plugin.Spam("Adding compatibility for General Improvements by Shaosil!");

        if (OpenLib.Plugin.instance.LethalConfig)
            OpenLib.Compat.LethalConfigSoft.AddButton("Terminal Customization", "Refresh Customizations", "Press this button to refresh all terminal customizations", "Refresh", TerminalCustomizer.TerminalCustomization);
    }
}

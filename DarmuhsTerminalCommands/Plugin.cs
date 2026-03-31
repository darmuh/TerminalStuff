using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using TerminalStuff.CommandHandling;
using TerminalStuff.EventSub;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;
using OpenLib.Common;
using TerminalStuff.Networking;
using TerminalStart = TerminalStuff.EventSub.TerminalStart;
using TerminalStuff.SpecialStuff;


namespace TerminalStuff;

[BepInAutoPlugin("darmuh.TerminalStuff")]
[BepInDependency("darmuh.OpenLib", "0.4.1")] //OpenLib requires latest version!
public partial class Plugin : BaseUnityPlugin
{
    public static Plugin instance = null!;

    //Networking
    internal NetworkClass<NetHandler> Networker = null!;

    internal static ManualLogSource Log = null!;

    //Compatibility
    public bool LobbyCompat = false;
    public bool LateGameUpgrades = false;
    public bool FovAdjust => Chainloader.PluginInfos.ContainsKey("Rozebud.FovAdjust");
    public bool HelmetCamsMod => Chainloader.PluginInfos.ContainsKey("RickArg.lethalcompany.helmetcameras");
    public bool SolosBodyCamsMod => Chainloader.PluginInfos.ContainsKey("SolosBodycams");
    public bool OpenBodyCamsMod => Chainloader.PluginInfos.ContainsKey("Zaggy1024.OpenBodyCams");
    public bool TwoRadarMapsMod => Chainloader.PluginInfos.ContainsKey("Zaggy1024.TwoRadarMaps");
    public bool suitsTerminal => Chainloader.PluginInfos.ContainsKey("darmuh.suitsTerminal");
    public bool TerminalFormatter => Chainloader.PluginInfos.ContainsKey("TerminalFormatter");
    public bool Constellations => Chainloader.PluginInfos.ContainsKey("com.github.darmuh.LethalConstellations");
    public bool ShipInventory => Chainloader.PluginInfos.ContainsKey("ShipInventory");
    public bool CruiserTerm = false;
    public bool ITAPI => Chainloader.PluginInfos.ContainsKey("WhiteSpike.InteractiveTerminalAPI");
    public bool LethalLevelLoader => Chainloader.PluginInfos.ContainsKey("imabatby.lethallevelloader");
    public bool WeatherTweaks => Chainloader.PluginInfos.ContainsKey("WeatherTweaks");
    public bool GenImprovements => Chainloader.PluginInfos.ContainsKey("ShaosilGaming.GeneralImprovements");
    public bool DawnLibPresent => Chainloader.PluginInfos.ContainsKey("com.github.teamxiaolan.dawnlib");
    public bool DawnLLLCombo => DawnLibPresent && LethalLevelLoader;
    public bool NoLevelLoader => !DawnLibPresent && !LethalLevelLoader;


    //public stuff for instance
    public bool splitViewCreated = false; 

    //AutoComplete
    internal bool removeTab = false;

    internal Terminal Terminal = null!;
    internal static bool refreshNodes = false;
    private static List<TerminalNode> _allnodescached = [];
    internal static List<TerminalNode> Allnodes
    {
        get
        {
            if (_allnodescached.Count == 0)
                _allnodescached = OpenLib.CoreMethods.LogicHandling.GetAllNodes();

            if (refreshNodes)
            {
                _allnodescached = OpenLib.CoreMethods.LogicHandling.RefreshAllNodes();
                refreshNodes = false;
            }   

            return _allnodescached;
        }
        set
        {
            return; //read only
        }
    }

    private void Awake()
    {
        instance = this;
        Log = base.Logger;
        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is loaded with version {MyPluginInfo.PLUGIN_VERSION}!\nUpgrading your terminal!!!");
        ConfigSettings.BindConfigSettings();
        //Addkeywords used to be here
        VideoManager.Load();
        Subscribers.Subscribe();
        Config.ConfigReloaded += OnConfigReloaded;
        Config.SettingChanged += OnSettingChanged;
        //FontStuff.TestingFonts();

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());

        Networker = new NetworkClass<NetHandler>("darmuhsTerminalStuff NetHandler", ConfigSettings.ModNetworking);
    }

    internal void OnSettingChanged(object sender, SettingChangedEventArgs settingChangedArg)
    {
        Loggers.LogDebug("CONFIG SETTING CHANGE EVENT");
        StuffForLibrary.ConfigSettingChange();
        TerminalStart.InitiateTerminalStuff();
        if (settingChangedArg.ChangedSetting.Definition.Section == "StorePlus")
            StorePlusConfig.UpdateLists();

        if (settingChangedArg.ChangedSetting == null)
            return;
    }

    internal void OnConfigReloaded(object sender, EventArgs e)
    {
        Loggers.LogDebug("Config has been reloaded!");
    }

    //Keeping this here since transpilers can run before LogLevel is set
    internal static void PatchLog(string message)
    {
        Log.LogInfo(message);
    }
}
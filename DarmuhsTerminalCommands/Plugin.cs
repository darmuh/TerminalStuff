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


namespace TerminalStuff;

[BepInAutoPlugin("darmuh.TerminalStuff")]
[BepInDependency("darmuh.OpenLib", OpenLib.MyPluginInfo.PLUGIN_VERSION)] //OpenLib requires latest version!
public partial class Plugin : BaseUnityPlugin
{
    public static Plugin instance = null!;

    //Networking
    internal NetworkClass<NetHandler> Networker = null!;

    internal static ManualLogSource Log = null!;

    //Compatibility
    public bool LobbyCompat = false;
    public bool LateGameUpgrades = false;
    public bool FovAdjust = false;
    public bool HelmetCamsMod = false;
    public bool SolosBodyCamsMod = false;
    public bool OpenBodyCamsMod = false;
    public bool TwoRadarMapsMod = false;
    public bool suitsTerminal = false;
    public bool TerminalFormatter = false;
    public bool Constellations = false;
    public bool ShipInventory = false;
    public bool CruiserTerm = false;
    public bool ITAPI = false;
    public bool LethalLevelLoader => Chainloader.PluginInfos.ContainsKey("imabatby.lethallevelloader");
    public bool WeatherTweaks = false;
    public bool GenImprovements = false;

    //public stuff for instance
    public bool isOnMirror = false;
    public bool isOnCamera = false;
    public bool isOnMap = false;
    public bool isOnOverlay = false;
    public bool isOnMiniMap = false;
    public bool isOnMiniCams = false;
    public bool activeCam = false;
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
            if (_allnodescached.Count == 0 || refreshNodes == true)
                _allnodescached = OpenLib.CoreMethods.LogicHandling.GetAllNodes();

            if (refreshNodes == true)
                refreshNodes = false;

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
        Log.LogInfo($"{MyPluginInfo.PLUGIN_NAME} is loaded with version {MyPluginInfo.PLUGIN_VERSION}!\nThis mod has been compiled for v73 of LethalCompany!");
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
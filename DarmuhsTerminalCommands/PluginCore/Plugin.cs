using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using OpenLib.ConfigManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;
using UnityEngine;
using static OpenLib.ConfigManager.ConfigSetup;


namespace TerminalStuff;

[BepInPlugin("darmuh.TerminalStuff", "darmuhsTerminalStuff", PluginInfo.PLUGIN_VERSION)]
[BepInDependency("darmuh.OpenLib", "0.4.0")] //hard dependency for my library


public class Plugin : BaseUnityPlugin
{
    public static Plugin instance = null!;
    public static class PluginInfo
    {
        public const string PLUGIN_GUID = "darmuh.TerminalStuff";
        public const string PLUGIN_NAME = "darmuhsTerminalStuff";
        public const string PLUGIN_VERSION = "3.9.0";
    }

    internal static ManualLogSource Log = null!;
    internal static bool gamePatched = false;

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
    public bool LethalLevelLoader = false;
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
    private static List<TerminalNode> allnodescached = [];
    internal static List<TerminalNode> Allnodes
    {
        get
        {
            if (allnodescached.Count == 0 || refreshNodes == true)
                allnodescached = OpenLib.CoreMethods.LogicHandling.GetAllNodes();

            if (refreshNodes == true)
                refreshNodes = false;

            return allnodescached;
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
        Log.LogInfo($"{PluginInfo.PLUGIN_NAME} is loaded with version {PluginInfo.PLUGIN_VERSION}!");
        StuffForLibrary.Init();
        ConfigSettings.BindConfigSettings();
        //Addkeywords used to be here
        VideoManager.Load();
        Subscribers.Subscribe();
        Config.ConfigReloaded += OnConfigReloaded;
        Config.SettingChanged += OnSettingChanged;
        //FontStuff.TestingFonts();

        //start of networking stuff

        var types = AccessTools.GetTypesFromAssembly(Assembly.GetExecutingAssembly());
        var methods = types.SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static));
        foreach (MethodInfo method in methods)
        {
            var atts = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
            if (atts.Length > 0)
                method.Invoke(null, null);
        }

        //end of networking stuff

        Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
        gamePatched = true;
    }

    internal void OnSettingChanged(object sender, SettingChangedEventArgs settingChangedArg)
    {
        Loggers.LogDebug("CONFIG SETTING CHANGE EVENT");
        StuffForLibrary.ManualManagedBools();
        TerminalStart.InitiateTerminalStuff();

        if (settingChangedArg.ChangedSetting == null)
            return;

        if (ConfigMisc.CheckChangedConfigSetting(defaultManaged, settingChangedArg.ChangedSetting) || ConfigMisc.CheckChangedConfigSetting(Configs.Commands.TerminalStuffBools, settingChangedArg.ChangedSetting))
        {
            Loggers.LogDebug("managed bools have been modified!!");
        }
    }

    internal void OnConfigReloaded(object sender, EventArgs e)
    {
        Loggers.LogDebug("Config has been reloaded!");
        NetworkingCheck(ConfigSettings.ModNetworking.Value, instance.Config, defaultManaged);
        ReadConfigAndAssignValues(instance.Config, defaultManaged);
        ReadConfigAndAssignValues(instance.Config, Configs.Commands.TerminalStuffBools);
    }

    //Keeping this here since transpilers can run before LogLevel is set
    internal static void PatchLog(string message)
    {
        if (gamePatched)
            return;

        Log.LogInfo(message);
    }
}
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;


namespace TerminalStuff
{
    [BepInPlugin("darmuh.TerminalStuff", "darmuhsTerminalStuff", "3.0.0")]
    [BepInDependency("atomic.terminalapi")]
    //[BepInDependency("Rozebud.FovAdjust")]

    public class Plugin : BaseUnityPlugin
    {
        public static Plugin instance;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "darmuh.lethalcompany.darmuhsTerminalStuff";
            public const string PLUGIN_NAME = "darmuhsTerminalStuff";
            public const string PLUGIN_VERSION = "3.0.0";
        }

        internal static new ManualLogSource Log;

        //Compatibility
        public bool CompatibilityAC = false;
        public bool LateGameUpgrades = false;
        public bool FovAdjust = false;
        public bool HelmetCamsMod = false;
        public bool SolosBodyCamsMod = false;
        public bool OpenBodyCamsMod = false;
        public bool TwoRadarMapsMod = false;

        //public stuff for instance
        public bool awaitingConfirmation = false;
        public bool isOnMirror = false;
        public bool isOnCamera = false;
        public bool isOnMap = false;
        public bool isOnOverlay = false;
        public bool isOnMiniMap = false;
        public bool isOnMiniCams = false;
        public bool activeCam = false;
        public bool radarNonPlayer = false;
        public bool splitViewCreated = false;
        public int customSpecialNodes = 0;
        public int confirmationNodeNum { get; internal set; }
        public string switchTarget = "";

        //flashlight stuff
        public bool fSuccess = false;
        public bool hSuccess = false;

        public static Terminal Terminal;


        public RawImage rawImage1;
        public RawImage rawImage2;
        public RenderTexture renderTexturePub;
        public Canvas terminalCanvas;
        public Vector2 originalTopSize;
        public Vector2 originalTopPosition;
        public Vector2 originalBottomSize;
        public Vector2 originalBottomPosition;
        public GameObject myNetworkPrefab;

        private void Awake()
        {
            Plugin.instance = this;
            Plugin.Log = base.Logger;
            Plugin.Log.LogInfo((object)"Plugin darmuhsTerminalCommands is loaded with version 3.0.0!");
            Plugin.Log.LogInfo((object)"--------[Completely reworked for Optimal Compatibility]---------");
            ConfigSettings.BindConfigSettings();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            //LeaveTerminal.AddTest(); //this command is only for devtesting
            //Addkeywords used to be here
            VideoManager.Load();

            //start of networking stuff

            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }


            //end of networking stuff
        }

        public static void MoreLogs(string message)
        {
            if (ConfigSettings.extensiveLogging.Value)
                Plugin.Log.LogInfo(message);
            else
                return;
        }

        public static void AddKeywords()
        {
            MenuBuild.CreateMoreCommand();
            AddKeywordIfEnabled(ConfigSettings.terminalQuit.Value, TerminalEvents.AddQuitKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalLol.Value, TerminalEvents.VideoKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalLoot.Value, TerminalEvents.lootKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalCams.Value, TerminalEvents.camsKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalClear.Value, TerminalEvents.ClearKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalHeal.Value, TerminalEvents.healKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalDanger.Value, TerminalEvents.dangerKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalVitals.Value, TerminalEvents.vitalsKeywords, ConfigSettings.ModNetworking.Value);
            AddKeywordIfEnabled(ConfigSettings.terminalMods.Value, TerminalEvents.AddModListKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalOverlay.Value, TerminalEvents.AddOverlayView);
            AddKeywordIfEnabled(ConfigSettings.terminalMinimap.Value, TerminalEvents.AddMiniMap);
            AddKeywordIfEnabled(ConfigSettings.terminalMinicams.Value, TerminalEvents.AddMiniCams);
            AddKeywordIfEnabled(ConfigSettings.terminalMap.Value, TerminalEvents.mapKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalDoor.Value, TerminalEvents.AddDoor);
            AddKeywordIfEnabled(ConfigSettings.terminalAlwaysOn.Value, TerminalEvents.AddAlwaysOnKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalLights.Value, TerminalEvents.AddLights);
            AddKeywordIfEnabled(ConfigSettings.terminalRandomSuit.Value, TerminalEvents.AddRandomSuit);
            AddKeywordIfEnabled(ConfigSettings.terminalClockCommand.Value, TerminalEvents.AddClockKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalLobby.Value, TerminalEvents.AddCommandAction("lobby command\n", false, MoreCommands.otherActionNodes, "lobby", true, "Lobby Name", "lobby name", "", "", MoreCommands.GetLobbyName));
            AddKeywordIfEnabled(ConfigSettings.terminalListItems.Value, TerminalEvents.AddCommandAction("List Items TermEvent\n", true, MoreCommands.otherActionNodes, ConfigSettings.ListItemsKeyword.Value, true, "List Items on Ship", "itemlist", "", "", MoreCommands.GetItemsOnShip));
            AddKeywordIfEnabled(ConfigSettings.terminalListItems.Value, TerminalEvents.AddCommandAction("Detailed Loot TermEvent\n", true, MoreCommands.otherActionNodes, ConfigSettings.ListScrapKeyword.Value, true, "List Scrap on Ship", "lootlist", "", "", AllTheLootStuff.DetailedLootCommand));
            AddKeywordIfEnabled(ConfigSettings.terminalBioScan.Value, TerminalEvents.AddCommandAction("bioscan terminal event\n", true, MoreCommands.infoOnlyNodes, "bioscan", true, "BioScan", "", "", CostCommands.BioscanCommand), ConfigSettings.ModNetworking.Value);
            AddKeywordIfEnabled(ConfigSettings.terminalMirror.Value, TerminalEvents.AddCommandAction("mirror terminal event\n", true, ViewCommands.termViewNodes, "mirror", true, "Mirror", "", "", ViewCommands.MirrorEvent));
            AddKeywordIfEnabled(ConfigSettings.terminalRefund.Value, TerminalEvents.AddCommandAction("refund terminal event\n", true, MoreCommands.infoOnlyNodes, "refund", true, "Refund", "", "", CostCommands.GetRefund), ConfigSettings.ModNetworking.Value); //unable to sync between clients without netpatch
            AddKeywordIfEnabled(ConfigSettings.terminalPrevious.Value, TerminalEvents.AddCommandAction("switch back\n", true, ViewCommands.termViewNodes, "previous", true, "Switch to Previous", "", "", ViewCommands.HandlePreviousSwitchEvent));
        }

        private static void AddKeywordIfEnabled(bool isEnabled, Action keywordAction)
        {
            if (isEnabled)
            {
                keywordAction();
            }
        }

        private static void AddKeywordIfEnabled(bool isEnabled, Action keywordAction, bool checkNetwork)
        {
            if (checkNetwork)
            {
                if (isEnabled)
                {
                    keywordAction();
                }
            }

        }
    }

}
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine.UI;
using UnityEngine;
using BepInEx.Bootstrap;


namespace TerminalStuff
{
    [BepInPlugin("darmuh.TerminalStuff", "darmuhsTerminalStuff", "2.2.2")]
    [BepInDependency("atomic.terminalapi")]
    [BepInDependency("Rozebud.FovAdjust")]

    public class Plugin : BaseUnityPlugin 
    {
        public static Plugin instance;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "darmuh.lethalcompany.darmuhsTerminalStuff";
            public const string PLUGIN_NAME = "darmuhsTerminalStuff";
            public const string PLUGIN_VERSION = "2.2.2";
        }

        internal static new ManualLogSource Log;

        //Compatibility
        public bool CompatibilityAC = false;
        public bool CompatibilityOther = false;


        //public stuff for instance
        public bool awaitingConfirmation = false;
        public bool isOnCamera = false;
        public bool isOnMap = false;
        public bool isOnOverlay = false;
        public bool isOnMiniMap = false;
        public bool isOnMiniCams = false;
        public bool splitViewCreated = false;
        public int customSpecialNodes = 0;
        public int confirmationNodeNum = 0;
        public string switchTarget = "";

        //network bools
        public bool syncedNodes = false;

        //flashlight stuff
        public bool fSuccess = false;
        public bool hSuccess = false;
        
        

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
            Plugin.Log.LogInfo((object)"Plugin darmuhsTerminalCommands is loaded with version 2.2.2!");
            Plugin.Log.LogInfo((object)"--------Who woulda thought this would be another cams hotfix! :).---------");
            ConfigSettings.BindConfigSettings();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            //LeaveTerminal.AddTest(); //this command is only for devtesting
            //Addkeywords used to be here
            VideoManager.Load();

            if (Chainloader.PluginInfos.ContainsKey("com.potatoepet.AdvancedCompany"))
            {
                Plugin.Log.LogInfo("Advanced Company detected, setting Advanced Company Compatibility options");
                CompatibilityAC = true;
            }
            if (Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades")) //other mods that simply append to the help command
            {
                Plugin.Log.LogInfo("Known mod detected that requires compatibility");
                CompatibilityOther = true;
            }

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



        public static void AddKeywords()
        {
            AddKeywordIfEnabled(ConfigSettings.terminalQuit.Value, LeaveTerminal.AddQuitKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalLol.Value, LeaveTerminal.hampterKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalLoot.Value, LeaveTerminal.lootKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalCams.Value, LeaveTerminal.camsKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalClear.Value, LeaveTerminal.clearKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalHeal.Value, LeaveTerminal.healKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalDanger.Value, LeaveTerminal.dangerKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalVitals.Value, LeaveTerminal.vitalsKeywords, ConfigSettings.ModNetworking.Value); //not allowing on clientside
            AddKeywordIfEnabled(ConfigSettings.terminalMods.Value, LeaveTerminal.AddModListKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalOverlay.Value, LeaveTerminal.AddOverlayView);
            AddKeywordIfEnabled(ConfigSettings.terminalMinimap.Value, LeaveTerminal.AddMiniMap);
            AddKeywordIfEnabled(ConfigSettings.terminalMinicams.Value, LeaveTerminal.AddMiniCams);
            AddKeywordIfEnabled(ConfigSettings.terminalMap.Value, LeaveTerminal.mapKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalDoor.Value, LeaveTerminal.AddDoor);
            AddKeywordIfEnabled(ConfigSettings.terminalAlwaysOn.Value, LeaveTerminal.AddAlwaysOnKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalLights.Value, LeaveTerminal.AddLights);
            AddKeywordIfEnabled(ConfigSettings.terminalRandomSuit.Value, LeaveTerminal.AddRandomSuit);
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
            if(checkNetwork)
            {
                if (isEnabled)
                {
                    keywordAction();
                }
            }
            
        }

    }

}
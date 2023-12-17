using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Reflection;
using TerminalApi;
using UnityEngine.UI;
using UnityEngine;
using static TerminalApi.TerminalApi;
using static TerminalStuff.Getlobby;
using Unity.Netcode;

namespace TerminalStuff
{
    [BepInPlugin("darmuh.TerminalStuff", "darmuhsTerminalStuff", "2.0.3")]
    [BepInDependency("atomic.terminalapi")]
    [BepInDependency("Rozebud.FovAdjust")]

    public class Plugin : BaseUnityPlugin 
    {
        public static Plugin instance;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "darmuh.lethalcompany.darmuhsTerminalStuff";
            public const string PLUGIN_NAME = "darmuhsTerminalStuff";
            public const string PLUGIN_VERSION = "2.0.3";
        }

        internal static new ManualLogSource Log;
        

        //public stuff for instance
        public bool awaitingConfirmation = false;
        public bool isOnCamera = false;
        public bool isOnMap = false;
        public bool isOnOverlay = false;
        public bool isOnProView = false;
        public bool splitViewCreated = false;
        public int customSpecialNodes = 0;
        public int confirmationNodeNum = 0;

        public RawImage rawImage1;
        public RawImage rawImage2;
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
            Plugin.Log.LogInfo((object)"Plugin darmuhsTerminalCommands is loaded with version 2.0.3!");
            Plugin.Log.LogInfo((object)"--------ChatGPT goes craaaaaazy.---------");
            ConfigSettings.BindConfigSettings();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            //LeaveTerminal.AddTest(); //this command is only for devtesting
            AddKeywords();
            VideoManager.Load();

            //start of networking stuff
            /*

            //var MainAssetBundle = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("darmuhsTerminalStuff.darmuhngo"));
            var MainAssetBundle = AssetBundle.LoadFromMemory(TerminalStuff.Properties.Resources.darmuhngo);
            myNetworkPrefab = MainAssetBundle.LoadAsset<GameObject>("darmuhNGO");


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

            */
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
            AddKeywordIfEnabled(ConfigSettings.terminalVitals.Value, LeaveTerminal.vitalsKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalMods.Value, LeaveTerminal.AddModListKeywords); //this should have been in 1.2.1
            AddKeywordIfEnabled(ConfigSettings.terminalTP.Value, LeaveTerminal.AddTeleportKeywords); //1.2.1 last command added
            AddKeywordIfEnabled(ConfigSettings.terminalITP.Value, LeaveTerminal.AddInverseTeleportKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalOverlay.Value, LeaveTerminal.AddOverlayView);
            AddKeywordIfEnabled(ConfigSettings.terminalProview.Value, LeaveTerminal.AddProView);
            AddKeywordIfEnabled(ConfigSettings.terminalMap.Value, LeaveTerminal.mapKeywords);
            AddKeywordIfEnabled(ConfigSettings.terminalDoor.Value, LeaveTerminal.AddDoor); //2.0.0 last command added
        }

        private static void AddKeywordIfEnabled(bool isEnabled, Action keywordAction)
        {
            if (isEnabled)
            {
                keywordAction();
            }
        }

        public static void AddLobbyKeywords()
        {
            TerminalNode triggerNode = CreateTerminalNode($" No lobby name found.\n", true);
            TerminalKeyword verbKeyword = CreateTerminalKeyword("lobby", true);
            TerminalKeyword nounKeyword = CreateTerminalKeyword("name");
            if (Getlobby.LastSteamLobbyName != null)
            {
                TerminalKeyword lobbyCheck = verbKeyword.AddCompatibleNoun(nounKeyword, CreateTerminalNode($"{Getlobby.LastSteamLobbyName} is the name of the lobby.\n", true));
                nounKeyword.defaultVerb = verbKeyword;
            }
            else if (HostSettingsPatch.LastLobbyName != null)
            {
                TerminalKeyword lobbyCheck = verbKeyword.AddCompatibleNoun(nounKeyword, CreateTerminalNode($"{HostSettingsPatch.LastLobbyName} is the name of the lobby.\n", true));
                nounKeyword.defaultVerb = verbKeyword;
            }
            else
            {
                Plugin.Log.LogInfo("Unable to find lobby name.");
            }

            AddTerminalKeyword(verbKeyword);
            AddTerminalKeyword(nounKeyword);
        }

    }

}
using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using HarmonyLib;
using TerminalApi;
using static TerminalApi.TerminalApi;
using System.Diagnostics;
using TerminalStuff;
using static TerminalStuff.Getlobby;

namespace TerminalStuff
{
    [BepInPlugin("darmuh.TerminalStuff", "darmuhsTerminalStuff", "1.2.0")]
    [BepInDependency("atomic.terminalapi")]
    [BepInDependency("Rozebud.FovAdjust")]

    public class Plugin : BaseUnityPlugin //renamed project
    {
        public static Plugin instance;
        public static class PluginInfo
        {
            public const string PLUGIN_GUID = "darmuh.lethalcompany.DarmuhsTerminalCommands";
            public const string PLUGIN_NAME = "darmuhsTerminalCommands";
            public const string PLUGIN_VERSION = "1.2.0";
        }

        internal static new ManualLogSource Log;
        private void Awake()
        {
            Plugin.instance = this;
            Plugin.Log = base.Logger;
            Plugin.Log.LogInfo((object)"Plugin darmuhsTerminalCommands is loaded with version 1.2.0!");
            Plugin.Log.LogInfo((object)"--------ChatGPT goes craaaaaazy.---------");
            ConfigSettings.BindConfigSettings();
            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly());
            //LeaveTerminal.AddTest(); this command is only for devtesting costly items
            LeaveTerminal.AddModListKeywords();
            AddKeywords();
            VideoManager.Load();
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
            AddKeywordIfEnabled(ConfigSettings.terminalTP.Value, LeaveTerminal.AddTeleportKeywords);
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
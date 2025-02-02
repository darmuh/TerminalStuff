using static OpenLib.CoreMethods.AddingThings;
using static OpenLib.Common.CommonStringStuff;
using static OpenLib.ConfigManager.ConfigSetup;
using static OpenLib.CoreMethods.CommandRegistry;
using static TerminalStuff.EventSub.TerminalStart;
using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.SpecialStuff.Keywords;

namespace TerminalStuff.PluginCore
{
    internal class StuffForLibrary
    {
        internal static void Init()
        {
            Commands.TerminalStuffBools = [];
            ConfigSettings.TerminalStuffMain = new();

            InitListing(ref ConfigSettings.TerminalStuffMain);
            Plugin.Log.LogInfo("TerminalStuffMain listing initialized");
        }

        internal static void AddCommands()
        {
            Plugin.Log.LogInfo("AddCommands called for TerminalStuffMain listing");
            GetCommandsToAdd(Commands.TerminalStuffBools, ConfigSettings.TerminalStuffMain);
            TerminalEvents.CreateStorePacks();
            SwitchCommand();
        }

        internal static void SwitchCommand()
        {
            if (Plugin.instance.Terminal == null)
                return;

            //switch command
            if (!KeywordConfigs.SwitchKeywords.Value.Contains("switch"))
            {
                KeywordConfigs.SwitchKeywords.Value += ", switch";
                Plugin.WARNING("SwitchKeywords MUST contain \"switch\"");
            }

            if (!OpenLib.CoreMethods.DynamicBools.TryGetKeyword("switch", out TerminalKeyword switchKeyword))
                Plugin.WARNING("Unable to get original switch keyword!!!");

            TerminalNode switchNode = AddNodeManual("SwitchedCam", KeywordConfigs.SwitchKeywords, ViewCommands.SwitchCommandHandler, true, 0, ConfigSettings.TerminalStuffMain, defaultManaged, "EXTRAS", "Switch Camera/Radar Views. Type a crewmate's name after the command to target them");
            switchKeyword.specialKeywordResult = switchNode;
            switchNodeVanilla = Plugin.instance.Terminal.terminalNodes.specialNodes[20];
            Plugin.instance.Terminal.terminalNodes.specialNodes[20] = switchNode;



            List<string> keywords = GetKeywordsPerConfigItem(KeywordConfigs.SwitchKeywords.Value);


            foreach (string keyword in keywords)
                AddSpecialListString(ref defaultListing, switchNode, keyword);


        }

        internal static void ManualManagedBools() //for any commands that can be added before awake that are not managed by one config item per command
        {
            if (!QoLConfig.TerminalShortcuts.Value && Commands.TerminalShortcutCommands.Value)
            {
                Commands.TerminalShortcutCommands.Value = false;
                Plugin.WARNING("TerminalShortcutCommands was enabled while feature, TerminalShortcuts, was disabled. Setting to FALSE");
                Plugin.instance.Config.Save();
            }

            Plugin.Spam($"TerminalShortcutCommands Value: {Commands.TerminalShortcutCommands.Value}");

            NewManagedBool(ref defaultManaged, "bindCommand", Commands.TerminalShortcutCommands.Value, "Use this command to bind new shortcuts", false, "COMFORT", GetKeywordsPerConfigItem("bind"), DynamicCommands.BindKeyToCommand, 0, true, null, null, "", "", "bind");
            NewManagedBool(ref defaultManaged, "unbindCommand", Commands.TerminalShortcutCommands.Value, "Use this command to unbind a terminal shortcut from a key", false, "COMFORT", GetKeywordsPerConfigItem("unbind"), DynamicCommands.UnBindKeyToCommand, 0, true, null, null, "", "", "unbind");

            if (QoLConfig.TerminalRunDelay.Value)
            {
                NewManagedBool(ref defaultManaged, "delayStart", QoLConfig.TerminalRunDelay.Value, "Use this command to run another command on a delay of up to 900 seconds!", false, "COMFORT", GetKeywordsPerConfigItem(KeywordConfigs.DelayKWs.Value), HandleDelayRun.HandleCommandDelay, 0, true, null, null, "", "", "delayStart");
                NewManagedBool(ref defaultManaged, "stopDelay", QoLConfig.TerminalRunDelay.Value, "Use this command to stop any delayed commands!", false, "COMFORT", GetKeywordsPerConfigItem(KeywordConfigs.StopDelayKWs.Value), HandleDelayRun.StopCommandDelay, 0, true, null, null, "", "", "stopDelay");
            }
        }
    }
}

using static OpenLib.CoreMethods.AddingThings;
using static OpenLib.Common.CommonStringStuff;
using static OpenLib.ConfigManager.ConfigSetup;
using static OpenLib.CoreMethods.CommandRegistry;
using static TerminalStuff.EventSub.TerminalStart;
using System.Collections.Generic;

namespace TerminalStuff.PluginCore
{
    internal class StuffForLibrary
    {
        internal static void Init()
        {
            ConfigSettings.TerminalStuffBools = [];
            ConfigSettings.TerminalStuffMain = new();

            InitListing(ref ConfigSettings.TerminalStuffMain);
            Plugin.Log.LogInfo("TerminalStuffMain listing initialized");
        }

        internal static void AddCommands()
        {
            Plugin.Log.LogInfo("AddCommands called for TerminalStuffMain listing");
            GetCommandsToAdd(ConfigSettings.TerminalStuffBools, ConfigSettings.TerminalStuffMain);
            TerminalEvents.StorePacks();
            SwitchCommand();

        }

        internal static void SwitchCommand()
        {
            if (Plugin.instance.Terminal == null)
                return;

            //switch command
            if (!ConfigSettings.SwitchKeywords.Value.Contains("switch"))
            {
                ConfigSettings.SwitchKeywords.Value += ", switch";
                Plugin.WARNING("SwitchKeywords MUST contain \"switch\"");
            }

            if (!OpenLib.CoreMethods.DynamicBools.TryGetKeyword("switch", out TerminalKeyword switchKeyword))
                Plugin.WARNING("Unable to get original switch keyword!!!");

            TerminalNode switchNode = AddNodeManual("SwitchedCam", ConfigSettings.SwitchKeywords, ViewCommands.SwitchCommandHandler, true, 0, ConfigSettings.TerminalStuffMain, defaultManaged, "EXTRAS", "Switch Camera/Radar Views. Type a crewmate's name after the command to target them");
            switchKeyword.specialKeywordResult = switchNode;
            switchNodeVanilla = Plugin.instance.Terminal.terminalNodes.specialNodes[20];
            Plugin.instance.Terminal.terminalNodes.specialNodes[20] = switchNode;



            List<string> keywords = GetKeywordsPerConfigItem(ConfigSettings.SwitchKeywords.Value);


            foreach (string keyword in keywords)
                AddSpecialListString(ref defaultListing, switchNode, keyword);


        }

        internal static void ManualManagedBools() //for any commands that can be added before awake that are not managed by one config item per command
        {
            if (!ConfigSettings.TerminalShortcuts.Value && ConfigSettings.TerminalShortcutCommands.Value)
            {
                ConfigSettings.TerminalShortcutCommands.Value = false;
                Plugin.WARNING("TerminalShortcutCommands was enabled while feature, TerminalShortcuts, was disabled. Setting to FALSE");
                Plugin.instance.Config.Save();
            }

            Plugin.Spam($"TerminalShortcutCommands Value: {ConfigSettings.TerminalShortcutCommands.Value}");

            NewManagedBool(ref defaultManaged, "bindCommand", ConfigSettings.TerminalShortcutCommands.Value, "Use this command to bind new shortcuts", false, "COMFORT", GetKeywordsPerConfigItem("bind"), DynamicCommands.BindKeyToCommand, 0, true, null, null, "", "", "bind");
            NewManagedBool(ref defaultManaged, "unbindCommand", ConfigSettings.TerminalShortcutCommands.Value, "Use this command to unbind a terminal shortcut from a key", false, "COMFORT", GetKeywordsPerConfigItem("unbind"), DynamicCommands.UnBindKeyToCommand, 0, true, null, null, "", "", "unbind");
        }
    }
}

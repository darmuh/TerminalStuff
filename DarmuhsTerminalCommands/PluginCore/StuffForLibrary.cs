using OpenLib.CoreMethods;
using TerminalStuff.Configs;
using static TerminalStuff.EventSub.TerminalStart;

namespace TerminalStuff.PluginCore;

internal class StuffForLibrary
{
    internal static CommandManager Switch = null!;
    internal static CommandManager Bind = null!;
    internal static CommandManager Unbind = null!;
    internal static void Init()
    {
        //Commands.TerminalStuffBools = [];
        //ConfigSettings.TerminalStuffMain = new();

        //InitListing(ref ConfigSettings.TerminalStuffMain);
        Plugin.Log.LogInfo("TerminalStuffMain listing initialized");
    }

    internal static void AddCommands()
    {
        Plugin.Log.LogInfo("AddCommands called for TerminalStuffMain listing");
        //GetCommandsToAdd(Commands.TerminalStuffBools, ConfigSettings.TerminalStuffMain);
        SwitchCommand();
    }

    internal static void SwitchCommand()
    {
        if (Plugin.instance.Terminal == null)
            return;

        //switch command
        if (!Switch.KeywordsConfig.Value.Contains("switch"))
        {
            Switch.KeywordsConfig.Value += ", switch";
            Loggers.WARNING("SwitchKeywords MUST contain \"switch\"");
        }

        if (!DynamicBools.TryGetKeyword("switch", out TerminalKeyword switchKeyword))
            Loggers.WARNING("Unable to get original switch keyword!!!");

        Switch.RegisterCommand(false);
        switchKeyword.specialKeywordResult = Switch.terminalNode;
        switchNodeVanilla = Plugin.instance.Terminal.terminalNodes.specialNodes[20];
        Plugin.instance.Terminal.terminalNodes.specialNodes[20] = Switch.terminalNode;
    }

    internal static void BindCommands()
    {
        if (!QoLConfig.TerminalShortcuts.Value && Commands.TerminalShortcutCommands.Value)
        {
            Commands.TerminalShortcutCommands.Value = false;
            Loggers.WARNING("TerminalShortcutCommands was enabled while feature, TerminalShortcuts, was disabled. Setting to FALSE");
            Plugin.instance.Config.Save();
        }

        Loggers.LogDebug($"TerminalShortcutCommands Value: {Commands.TerminalShortcutCommands.Value}");
    }

    internal static void ManualManagedBools() //for any commands that can be added before awake that are not managed by one config item per command
    {
        if (!QoLConfig.TerminalShortcuts.Value && Commands.TerminalShortcutCommands.Value)
        {
            Commands.TerminalShortcutCommands.Value = false;
            Loggers.WARNING("TerminalShortcutCommands was enabled while feature, TerminalShortcuts, was disabled. Setting to FALSE");
            Plugin.instance.Config.Save();
        }

        Loggers.LogDebug($"TerminalShortcutCommands Value: {Commands.TerminalShortcutCommands.Value}");

        //moved to Commands.cs

        //NewManagedBool(ref defaultManaged, "bindCommand", Commands.TerminalShortcutCommands.Value, "Use this command to bind new shortcuts", false, "COMFORT", GetKeywordsPerConfigItem("bind"), DynamicCommands.BindKeyToCommand, 0, true, null, null, "", "", "bind");
        //NewManagedBool(ref defaultManaged, "unbindCommand", Commands.TerminalShortcutCommands.Value, "Use this command to unbind a terminal shortcut from a key", false, "COMFORT", GetKeywordsPerConfigItem("unbind"), DynamicCommands.UnBindKeyToCommand, 0, true, null, null, "", "", "unbind");


        //if (QoLConfig.TerminalRunDelay.Value)
        //{
        //NewManagedBool(ref defaultManaged, "delayStart", QoLConfig.TerminalRunDelay.Value, "Use this command to run another command on a delay of up to 900 seconds!", false, "COMFORT", GetKeywordsPerConfigItem(KeywordConfigs.DelayKWs.Value), HandleDelayRun.HandleCommandDelay, 0, true, null, null, "", "", "delayStart");
        //NewManagedBool(ref defaultManaged, "stopDelay", QoLConfig.TerminalRunDelay.Value, "Use this command to stop any delayed commands!", false, "COMFORT", GetKeywordsPerConfigItem(KeywordConfigs.StopDelayKWs.Value), HandleDelayRun.StopCommandDelay, 0, true, null, null, "", "", "stopDelay");
        //}
    }
}

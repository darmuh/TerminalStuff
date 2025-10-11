using OpenLib.CoreMethods;
using TerminalStuff.Configs;
using TerminalStuff.Util;
using static TerminalStuff.EventSub.TerminalStart;

namespace TerminalStuff.CommandHandling;

internal class StuffForLibrary
{
    internal static CommandManager Switch = null!;
    internal static CommandManager Bind = null!;
    internal static CommandManager Unbind = null!;

    internal static void AddCommands()
    {
        Plugin.Log.LogInfo("AddCommands called for TerminalStuffMain listing");
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

    internal static void ConfigSettingChange() //for any commands that can be added before awake that are not managed by one config item per command
    {
        if (!QoLConfig.TerminalShortcuts.Value && Commands.TerminalShortcutCommands.Value)
        {
            Commands.TerminalShortcutCommands.Value = false;
            Loggers.WARNING("TerminalShortcutCommands was enabled while feature, TerminalShortcuts, was disabled. Setting to FALSE");
            Plugin.instance.Config.Save();
        }

        Loggers.LogDebug($"TerminalShortcutCommands Value: {Commands.TerminalShortcutCommands.Value}");
    }
}

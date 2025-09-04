using OpenLib.CoreMethods;
using TerminalStuff.Configs;
using TerminalStuff.PluginCore;

namespace TerminalStuff.EventSub;

internal class Teleporters
{
    internal static CommandManager Inverse = null!;
    internal static CommandManager Regular = null!;

    internal static void OnInverseAwake()
    {
        if (!Commands.TerminalITP.Value)
            return;

        Loggers.LogInfo("InverseTP instance detected, adding keyword");

        Inverse.RegisterCommand();
    }

    internal static void OnNormalAwake()
    {
        if (!Commands.TerminalTP.Value)
            return;

        Loggers.LogInfo("NormalTP instance detected, adding keyword");

        Regular.RegisterCommand();
    }
}

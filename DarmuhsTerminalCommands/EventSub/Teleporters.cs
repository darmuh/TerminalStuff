using OpenLib.CoreMethods;
using TerminalStuff.Configs;

namespace TerminalStuff.EventSub
{
    internal class Teleporters
    {
        internal static CommandManager Inverse;
        internal static CommandManager Regular;

        internal static void OnInverseAwake()
        {
            if (!Commands.TerminalITP.Value)
                return;

            Plugin.MoreLogs("InverseTP instance detected, adding keyword");

            Inverse.RegisterCommand();
        }

        internal static void OnNormalAwake()
        {
            if (!Commands.TerminalTP.Value)
                return;

            Plugin.MoreLogs("NormalTP instance detected, adding keyword");

            Regular.RegisterCommand();
        }
    }
}

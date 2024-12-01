using System.Runtime.CompilerServices;
using static TwoRadarMaps.Plugin;

namespace TerminalStuff
{
    internal class TwoRadarMapsCompatibility
    {
        internal static ManualCameraRenderer GetTerminalMap()
        {
            return TerminalMapRenderer;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static string TeleportCompatibility()
        {
            TeleportTarget(TerminalMapRenderer.targetTransformIndex);
            Plugin.MoreLogs("Valid player attached to tworadarmaps, teleporting");
            string displayText = $"{ConfigSettings.TpMessageString.Value} (Targeted Player: {TerminalMapRenderer.radarTargets[TerminalMapRenderer.targetTransformIndex].name})";
            return displayText;
        }
    }
}

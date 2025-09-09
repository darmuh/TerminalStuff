using System.Runtime.CompilerServices;
using TerminalStuff.Util;
using static TwoRadarMaps.Plugin;

namespace TerminalStuff.Compatibility;

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
        Loggers.LogInfo("Valid player attached to tworadarmaps, teleporting");
        string displayText = $"{ConfigSettings.TpMessageString.Value} (Targeted Player: {TerminalMapRenderer.radarTargets[TerminalMapRenderer.targetTransformIndex].name})";
        return displayText;
    }
}

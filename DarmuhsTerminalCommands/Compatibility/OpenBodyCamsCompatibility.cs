using System.Runtime.CompilerServices;

namespace TerminalStuff.Compatibility;

internal class OpenBodyCamsCompatibility
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool IsCreatingCommands()
    {
        if (!Plugin.instance.OpenBodyCamsMod)
            return false;

        if (OpenBodyCams.Plugin.TerminalPiPBodyCamEnabled.Value)
            return true;

        return false;

    }
}

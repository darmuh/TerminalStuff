using static GeneralImprovements.Plugin;

namespace TerminalStuff.Compatibility;

internal class GenImproves
{
    internal static int GetMaxItems()
    {
        if (!Plugin.instance.GenImprovements)
            return 18;

        return DropShipItemLimit.Value;

    }
}

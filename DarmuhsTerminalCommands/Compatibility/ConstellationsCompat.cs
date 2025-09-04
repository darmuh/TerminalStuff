using LethalConstellations.PluginCore;

namespace TerminalStuff.Compatibility;

internal class ConstellationsCompat
{
    internal static bool IsLevelInConstellation(SelectableLevel level)
    {
        return ClassMapper.IsLevelInConstellation(level);
    }
}

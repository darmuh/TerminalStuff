using System.Linq;
using TerminalStuff.EventSub;
using TerminalStuff.MoonsTweaks;
using TerminalStuff.Util;

namespace TerminalStuff.Compatibility;

internal class LLLCompat
{
    internal static void UpdateLLLFontSize(float fontSize)
    {
        if (!Plugin.instance.LethalLevelLoader)
            return;

        LethalLevelLoader.TerminalManager.defaultTerminalFontSize = fontSize;
        Plugin.instance.Terminal.screenText.textComponent.fontSize = LethalLevelLoader.TerminalManager.defaultTerminalFontSize;
        Loggers.LogDebug($"TerminalManager.defaultTerminalFontSize set to {fontSize}!");
    }

    internal static int GetPrice(SelectableLevel level)
    {
        if (!Plugin.instance.LethalLevelLoader)
            return 0;

        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out LethalLevelLoader.ExtendedLevel extendedLevel))
        {
            return extendedLevel.RoutePrice;
        }
        return 0;
    }

    internal static bool IsLocked(SelectableLevel level)
    {
        if (!Plugin.instance.LethalLevelLoader)
            return false;

        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out LethalLevelLoader.ExtendedLevel extendedLevel))
            return extendedLevel.IsRouteLocked;

        return false;
    }

    internal static bool IsHidden(SelectableLevel level)
    {
        if (!Plugin.instance.LethalLevelLoader)
            return false;

        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out LethalLevelLoader.ExtendedLevel extendedLevel))
            return extendedLevel.IsRouteHidden;

        return false;
    }

    internal static bool IsDisabled(SelectableLevel level)
    {
        if (!Plugin.instance.LethalLevelLoader)
            return false;

        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out LethalLevelLoader.ExtendedLevel extendedLevel))
            return MoonInfo.IsRouteEnabled(extendedLevel.RouteNode);

        return false;
    }

    // Unused, would probably need a method to relock/hide on lobby reset
    internal static void UnlockUnhide(SelectableLevel level)
    {
        if (!Plugin.instance.LethalLevelLoader)
            return;
        
        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out LethalLevelLoader.ExtendedLevel extendedLevel))
        {
            extendedLevel.IsRouteHidden = false;
            extendedLevel.IsRouteLocked = false;
            Loggers.LogDebug($"Unlocked/Unhidden - {extendedLevel.NumberlessPlanetName}");
        } 
    }

    internal static void ChangeHiddenStatus(SelectableLevel level, bool shouldHide)
    {
        if (!Plugin.instance.LethalLevelLoader)
            return;
        
        if (LethalLevelLoader.LevelManager.TryGetExtendedLevel(level, out LethalLevelLoader.ExtendedLevel extendedLevel))
        {
            extendedLevel.IsRouteHidden = shouldHide;
            Loggers.LogDebug($"IsHidden {shouldHide} - {extendedLevel.NumberlessPlanetName}");
        }
    }
}

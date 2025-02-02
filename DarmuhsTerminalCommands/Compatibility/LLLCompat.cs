using LethalLevelLoader;
using System.Linq;

namespace TerminalStuff.Compatibility
{
    internal class LLLCompat
    {
        internal static void UpdateLLLFontSize(float fontSize)
        {
            if (!Plugin.instance.LethalLevelLoader)
                return;

            TerminalManager.defaultTerminalFontSize = fontSize;
            Plugin.instance.Terminal.screenText.textComponent.fontSize = TerminalManager.defaultTerminalFontSize;
            Plugin.Spam($"TerminalManager.defaultTerminalFontSize set to {fontSize}!");
        }

        internal static int GetPrice(SelectableLevel level)
        {
            if (!Plugin.instance.LethalLevelLoader)
                return 0;

            if (LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
            {
                return extendedLevel.RoutePrice;
            }
            return 0;
        }

        internal static bool IsLocked(SelectableLevel level)
        {
            if (!Plugin.instance.LethalLevelLoader)
                return false;

            if (LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
                return extendedLevel.IsRouteLocked;
            
            return false;
        }

        internal static bool IsHidden(SelectableLevel level)
        {
            if (!Plugin.instance.LethalLevelLoader)
                return false;

            if (LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
                return extendedLevel.IsRouteHidden;

            return false;
        }

        internal static bool IsDisabled(SelectableLevel level)
        {
            if (!Plugin.instance.LethalLevelLoader)
                return false;

            if (LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
            {
                return !TerminalManager.routeKeyword.compatibleNouns.Any(x => x.result == extendedLevel.RouteNode);
            }

            return false;
        }

        internal static void UnlockUnhide(SelectableLevel level)
        {
            if (!Plugin.instance.LethalLevelLoader)
                return;

            if (LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
            {
                extendedLevel.IsRouteHidden = false;
                extendedLevel.IsRouteLocked = false;
                Plugin.Spam($"Unlocked/Unhidden - {extendedLevel.NumberlessPlanetName}");
            }
        }

        internal static void ChangeHiddenStatus(SelectableLevel level, bool shouldHide)
        {
            if (!Plugin.instance.LethalLevelLoader)
                return;

            if (LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
            {
                extendedLevel.IsRouteHidden = shouldHide;
                Plugin.Spam($"IsHidden {shouldHide} - {extendedLevel.NumberlessPlanetName}");
            }
        }
    }
}

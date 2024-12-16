using LethalLevelLoader;

namespace TerminalStuff.Compatibility
{
    internal class LLLCompat
    {
        internal static void UpdateLLLFontSize(float fontSize)
        {
            TerminalManager.defaultTerminalFontSize = fontSize;
            Plugin.instance.Terminal.screenText.textComponent.fontSize = TerminalManager.defaultTerminalFontSize;
            Plugin.Spam($"TerminalManager.defaultTerminalFontSize set to {fontSize}!");
        }

        internal static bool IsLocked(SelectableLevel level)
        {
            if(LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
            {
                if (extendedLevel.IsRouteLocked)
                    return true;
                else
                    return false;
            }
            return false;
        }

        internal static bool IsHidden(SelectableLevel level)
        {
            if (LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
            {
                if (extendedLevel.IsRouteHidden)
                    return true;
                else
                    return false;
            }
            return false;
        }

        internal static void UnlockUnhide(SelectableLevel level)
        {
            if (LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
            {
                extendedLevel.IsRouteHidden = false;
                extendedLevel.IsRouteLocked = false;
                Plugin.Spam($"Unlocked/Unhidden - {extendedLevel.NumberlessPlanetName}");
            }
        }

        internal static void ChangeHiddenStatus(SelectableLevel level, bool shouldHide)
        {
            if (LevelManager.TryGetExtendedLevel(level, out ExtendedLevel extendedLevel))
            {
                extendedLevel.IsRouteHidden = shouldHide;
                Plugin.Spam($"IsHidden {shouldHide} - {extendedLevel.NumberlessPlanetName}");
            }
        }
    }
}

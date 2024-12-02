using UnityEngine;
using static suitsTerminal.AdvancedMenu;

namespace TerminalStuff
{
    internal class SuitsTerminalCompatibility
    {
        internal static bool CheckForSuitsMenu()
        {
            if (!Plugin.instance.suitsTerminal)
                return false;

            if (specialMenusActive)
            {
                Plugin.MoreLogs("In suitsTerminal menu");
                return true;
            }
            else
            {
                return false;
            }
        }

        internal static void SetCaretColor(Color color)
        {
            CaretOriginal = color;
        }
    }
}

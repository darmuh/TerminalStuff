using TerminalStuff.Util;
using UnityEngine;
using static suitsTerminal.OfTerminal.Menu;

namespace TerminalStuff.Compatibility;

internal class SuitsTerminalCompatibility
{
    internal static bool CheckForSuitsMenu()
    {
        if (!Plugin.instance.suitsTerminal)
            return false;

        if (specialMenusActive)
        {
            Loggers.LogInfo("In suitsTerminal menu");
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

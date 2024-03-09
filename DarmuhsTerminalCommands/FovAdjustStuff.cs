using FovAdjust;
using UnityEngine;

namespace TerminalStuff
{
    internal class FovAdjustStuff
    {
        internal static void FovAdjustFunc(TerminalNode node, Terminal instance, float number)
        {
            instance.QuitTerminal();
            number = Mathf.Clamp(number, 66f, 130f);
            PlayerControllerBPatches.newTargetFovBase = number;
            PlayerControllerBPatches.calculateVisorStuff();
            DynamicCommands.newParsedValue = false;
        }
    }
}

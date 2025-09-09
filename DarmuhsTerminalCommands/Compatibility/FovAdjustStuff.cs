using FovAdjust;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace TerminalStuff.Compatibility;

internal class FovAdjustStuff
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void FovAdjustFunc(Terminal instance, float number)
    {
        instance.QuitTerminal();
        number = Mathf.Clamp(number, 66f, 130f);
        PlayerControllerBPatches.newTargetFovBase = number;
        PlayerControllerBPatches.calculateVisorStuff();
        DynamicCommands.newParsedValue = false;
    }
}

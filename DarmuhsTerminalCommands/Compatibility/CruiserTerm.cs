using CruiserTerminal.Patches;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenLib.Common;

namespace TerminalStuff.Compatibility;

internal class CruiserTerm
{
    internal static List<string> WordList = [];
    internal static bool isDeny = false;
    internal static TerminalNode NoAccess = null!;

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static void Quit()
    {
        if (Status())
            CTPatches.cterminal.QuitCruiserTerminal();
    }

    internal static void CreateDenyKeyword()
    {
        NoAccess = CreateNoAccess();
        if (OpenLib.Common.Misc.CompareStringsInvariant(ConfigSettings.CruiserTerminalFilterType.Value, "deny"))
            isDeny = true;
        else
            isDeny = false;

        WordList = CommonStringStuff.GetKeywordsPerConfigItem(ConfigSettings.CruiserKeywordList.Value, ',');
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    internal static bool Status()
    {
        if (CTPatches.cterminal != null)
        {
            if (CTPatches.cterminal.cruiserTerminalInUse)
                return true;
            else
                return false;
        }
        else
            return false;
    }

    internal static bool CanParseWord(string query)
    {
        if (Status())
        {
            if (isDeny)
            {
                if (WordList.Any(d => OpenLib.Common.Misc.CompareStringsInvariant(d, query)))
                    return false;
                else
                    return true;
            }
            else
            {
                if (WordList.Any(d => OpenLib.Common.Misc.CompareStringsInvariant(d, query)))
                    return true;
                else
                    return false;
            }
        }

        return true;
    }

    internal static TerminalNode CreateNoAccess()
    {
        TerminalNode node = OpenLib.CoreMethods.BasicTerminal.CreateNewTerminalNode();
        node.displayText = "\n\nThis page cannot be accessed via the Cruiser Terminal!\n\nPlease return to the ship to use this command.\n\n\n";
        node.clearPreviousText = true;

        return node;
    }

}

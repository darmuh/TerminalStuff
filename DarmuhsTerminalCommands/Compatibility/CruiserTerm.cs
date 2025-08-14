using CruiserTerminal.Patches;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace TerminalStuff.Compatibility
{
    internal class CruiserTerm
    {
        //private static CruiserTerminalScript cruiserTerminal;
        internal static List<string> WordList = [];
        internal static bool isDeny = false;
        internal static TerminalNode NoAccess;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Quit()
        {
            if (Status())
                CTPatches.cterminal.QuitCruiserTerminal();
        }

        internal static void CreateDenyKeyword()
        {
            NoAccess = CreateNoAccess();
            if(ConfigSettings.CruiserTerminalFilterType.Value.ToLower() == "deny" )
                isDeny = true;
            else
                isDeny = false;

            WordList = OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(ConfigSettings.CruiserKeywordList.Value, ',');
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
                    if (WordList.Any(d => d.ToLower() == query.ToLower()))
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (WordList.Any(d => d.ToLower() == query.ToLower()))
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
            node.displayText = "\r\n\r\nThis page cannot be accessed via the Cruiser Terminal!\r\n\r\nPlease return to the ship to use this command.\r\n\r\n\r\n";
            node.clearPreviousText = true;

            return node;
        }

    }
}

/*

extern alias old;
using System.Collections.Generic;
using System.Linq;

namespace TerminalStuff.Compatibility
{
    internal class CruiserTermOld
    {
        internal static old::CruiserTerminal.CruiserTerminal cruiserTerminal;
        internal static List<string> WordList = [];
        internal static bool isDeny = false;
        internal static TerminalNode NoAccess;

        internal static bool GetInstance()
        {
            if (cruiserTerminal == null)
            {
                //Plugin.Spam("Attempting to find cruiserTerminal instance");
                cruiserTerminal = UnityEngine.Object.FindObjectOfType<old::CruiserTerminal.CruiserTerminal>();
                //Plugin.Spam($"Instance found [ {cruiserTerminal != null} ]");
            }

            return cruiserTerminal != null;
        }

        internal static void Quit()
        {
            if (!GetInstance())
                return;

            cruiserTerminal.QuitCruiserTerminal();
        }

        internal static void CreateDenyKeyword()
        {
            NoAccess = CreateNoAccess();
            if (ConfigSettings.CruiserTerminalFilterType.Value.ToLower() == "deny")
                isDeny = true;
            else
                isDeny = false;

            WordList = OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(ConfigSettings.CruiserKeywordList.Value, ',');
        }

        internal static bool Status()
        {
            if (GetInstance())
            {
                if (cruiserTerminal.cruiserTerminalInUse)
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
 */
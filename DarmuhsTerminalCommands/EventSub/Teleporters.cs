using System.Collections.Generic;
using TerminalStuff.Configs;
using static OpenLib.ConfigManager.ConfigSetup;
using static OpenLib.CoreMethods.AddingThings;

namespace TerminalStuff.EventSub
{
    internal class Teleporters
    {
        internal static void OnInverseAwake()
        {
            if (!Commands.TerminalITP.Value)
                return;

            Plugin.MoreLogs("InverseTP instance detected, adding keyword");

            AddNodeManual("Use Inverse Teleporter", KeywordConfigs.ItpKeywords, ShipControls.InverseTeleporterCommand, true, 0, defaultListing, defaultManaged, "CONTROLS", "Active the Inverse Teleporter");
            if (MenuBuild.myMenuItems.Count > 0)
                MenuBuild.RefreshMyMenu(); //refresh menu items
        }

        internal static void OnNormalAwake()
        {
            if (!Commands.TerminalTP.Value)
                return;

            Plugin.MoreLogs("NormalTP instance detected, adding keyword");

            TerminalNode tpNode = AddNodeManual("Use Teleporter", KeywordConfigs.TpKeywords, ShipControls.RegularTeleporterCommand, true, 0, defaultListing, defaultManaged, "CONTROLS", "Activate the Teleporter. Type a crewmate's name after the command to target them");

            List<string> keywords = OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(KeywordConfigs.TpKeywords.Value);

            foreach (string keyword in keywords)
                OpenLib.CoreMethods.CommandRegistry.AddSpecialListString(ref defaultListing, tpNode, keyword);

            if (MenuBuild.myMenuItems.Count > 0)
                MenuBuild.RefreshMyMenu(); //refresh menu items
        }
    }
}

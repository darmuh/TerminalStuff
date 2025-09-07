using OpenLib.Common;
using OpenLib.CoreMethods;
using OpenLib.Menus;
using TerminalStuff.Configs;
using static OpenLib.Menus.CommandsMenu;

namespace TerminalStuff.Util;


internal class MenuBuild
{
    //NEW
    internal static CommandsMenuBase MoreMenu = null!;
    internal static CommandMenuItem<CommandsMenuBase> MainMenuItem = null!;
    internal static CommandManager MoreMenuCommand = null!;

    internal static void MoreInit()
    {
        MoreMenuCommand = Commands.AddLocalCommmandManualWords("More Menus", QoLConfig.CreateMoreMenus, ["more"], EnterCommandMenu);
        MoreMenu = new("More Menu")
        {
            MainMenu = MainMenuItem,
            PageSize = 10,
            AdjustScrollInMenu = false
        };

        MainMenuItem = CreateMainMenu(MoreMenu, "More Commands Menu", () => "=== More Commands Menu ===\n\n");
    }

    internal static string EnterCommandMenu()
    {
        if (MainMenuItem == null)
            return "This menu has not been created correctly";

        MoreMenu.EnterAtPage(MainMenuItem);
        return "";
    }

    internal static void CreateDarmuhsTerminalStuffMenus()
    {
        Loggers.LogDebug("START CreateDarmuhsTerminalStuffMenus");
        EventSub.TerminalStart.InitiateTerminalStuff();
        if (!QoLConfig.CreateMoreMenus.Value)
        {
            if (!DynamicBools.TryGetKeyword("other", out TerminalKeyword otherWord))
                return;

            foreach (CommandManager item in Commands.GetEnabledCommands())
            {
                if (item.KeywordList.Count == 0 || item.IsEnabled == null)
                    continue;

                AddingThings.AddToExistingNodeText($"\n>{CommonStringStuff.GetKeywordsForMenuItem(item.KeywordList).ToUpper()}\n{item.IsEnabled.ConfigItem.Description.Description}", ref otherWord.specialKeywordResult);
                Loggers.LogDebug($"{item.Name} keywords added to other menu");
            }
            return;
        }

        var active = OpenLib.Plugin.GetActiveCommands();
        UpdateMenuListing(MoreMenu, MainMenuItem, active);

        MoreMenu.MenuNode = MoreMenuCommand.terminalNode;
        CreateAndSetControlsFooter(MoreMenu, MoreMenu.GetMenuItemsOfType<CommandMenuItem<CommandsMenuBase>>());
        Loggers.LogDebug("END CreateDarmuhsTerminalStuffMenus");

    }
}

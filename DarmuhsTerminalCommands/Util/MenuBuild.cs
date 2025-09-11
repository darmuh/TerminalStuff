using OpenLib.Common;
using OpenLib.CoreMethods;
using OpenLib.Menus;
using TerminalStuff.Configs;
using static TerminalStuff.Configs.CustomizeConfig;
using static OpenLib.Menus.CommandsMenu;

namespace TerminalStuff.Util;


internal class MenuBuild
{
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
            AdjustScrollInMenu = false,
            CategoryHeader = MoreMenuSectionHeader.Value,
            CommandHeader = MoreMenuCommandHeader.Value,
            KeywordsHeader = MoreMenuKeywordsHeader.Value,
            InfoHeader = MoreMenuInfoHeader.Value,
            AddInfoMenu = MoreIncludeCommandInfo.Value,
            AddKeywordsMenu = MoreIncludeCommandKeywords.Value,
            AddRunCommand = MoreIncludeCommandRun.Value
        };

        MainMenuItem = CreateMainMenu(MoreMenu, "More Commands Menu", () => MoreMenuMainHeader.Value);
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

        AddingThings.AddToHelpCommand(MoreHintText.Value);
        if (LogicHandling.TryGetFromAllNodes("OtherCommands", out TerminalNode otherNode))
            AddingThings.AddToExistingNodeText($"\n{MoreHintText.Value}", ref otherNode);


        var active = OpenLib.Plugin.GetActiveCommands();
        UpdateMenuListing(MoreMenu, MainMenuItem, active);

        MoreMenu.MenuNode = MoreMenuCommand.terminalNode;
        CreateAndSetControlsFooter(MoreMenu, MoreMenu.GetMenuItemsOfType<CommandMenuItem<CommandsMenuBase>>());
        Loggers.LogDebug("END CreateDarmuhsTerminalStuffMenus");

    }
}

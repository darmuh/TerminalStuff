using OpenLib.Common;
using OpenLib.ConfigManager;
using OpenLib.CoreMethods;
using OpenLib.InteractiveMenus;
using OpenLib.Menus;
using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.PluginCore;
using static OpenLib.Menus.CommandsMenu;
using static OpenLib.Menus.MenuBuild;

namespace TerminalStuff;


internal class MenuBuild
{
    //NEW
    internal static CommandsMenuBase MoreMenu = null!;
    internal static CommandMenuItem<CommandsMenuBase> MainMenuItem = null!;
    internal static CommandManager MoreMenuCommand = null!;

    //OLD
    internal static List<TerminalMenuCategory> myMenuCategories = [];
    internal static List<TerminalMenuItem> myMenuItems = [];
    internal static TerminalMenu myMenu = null!;
    
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

            foreach (TerminalMenuItem item in myMenuItems)
            {
                if (item.itemKeywords.Count == 0)
                {
                    Loggers.WARNING($"{item.ItemName} has no keywords!!");
                    continue;
                }

                if (item == null)
                {
                    Loggers.WARNING($"NULL ITEM IN myMenuItems!!!");
                    continue;
                }

                AddingThings.AddToExistingNodeText($"\n>{CommonStringStuff.GetKeywordsForMenuItem(item.itemKeywords).ToUpper()}\n{item.itemDescription}", ref otherWord.specialKeywordResult);
                Loggers.LogDebug($"{item.ItemName} keywords added to other menu");
            }
            return;
        }



        var active = OpenLib.Plugin.GetActiveCommands();
        UpdateMenuListing(MoreMenu, MainMenuItem, active);

        MoreMenu.MenuNode = MoreMenuCommand.terminalNode;
        CreateAndSetControlsFooter(MoreMenu, MoreMenu.GetMenuItemsOfType<CommandMenuItem<CommandsMenuBase>>());
        Loggers.LogDebug("END CreateDarmuhsTerminalStuffMenus");

    }

    internal static void AddMenuItems(List<ManagedConfig> managedItems, TerminalMenu myMenu)
    {
        if (myMenu.menuItems.Count == 0)
            return;

        foreach (ManagedConfig item in managedItems)
        {
            if (item.menuItem == null)
                continue;

            if (!myMenu.menuItems.Contains(item.menuItem))
                myMenu.menuItems.Add(item.menuItem);
        }
    }

    internal static void AddMenuItems(List<ManagedConfig> managedItems, List<TerminalMenuItem> myMenuItems)
    {
        if (myMenuItems.Count == 0)
            return;

        foreach (ManagedConfig item in managedItems)
        {
            if (item.menuItem == null)
                continue;

            if (!myMenuItems.Contains(item.menuItem))
                myMenuItems.Add(item.menuItem);
        }
    }

    internal static void RefreshMyMenu()
    {
        if (myMenu == null)
            return;

        myMenu.menuItems.Clear();
        myMenuItems.Clear();
        //myMenuItems = TerminalMenuItems(defaultManaged);
        myMenu.menuItems = myMenuItems;
        //AddMenuItems(Commands.TerminalStuffBools, myMenu);
        UpdateCategories(myMenu);
    }

    internal static void ClearMyMenustuff()
    {
        myMenu?.Delete();
        myMenuItems.Clear();
        myMenuCategories.Clear();
    }
}

using OpenLib.Common;
using OpenLib.ConfigManager;
using OpenLib.CoreMethods;
using OpenLib.Menus;
using System.Collections.Generic;
using static OpenLib.ConfigManager.ConfigSetup;
using static OpenLib.Menus.MenuBuild;

namespace TerminalStuff
{

    internal class MenuBuild
    {
        internal static List<TerminalMenuCategory> myMenuCategories = [];
        internal static List<TerminalMenuItem> myMenuItems = [];
        internal static TerminalMenu myMenu;
        internal static void CategoryList()
        {
            Dictionary<string, string> myCategories = [];
            myMenuItems.Clear();
            myMenuItems = TerminalMenuItems(defaultManaged);
            AddMenuItems(ConfigSettings.TerminalStuffBools, myMenuItems);
            if (ShouldAddCategoryNameToMainMenu(myMenuItems, "COMFORT"))
                myCategories.Add("COMFORT", "Improves the terminal user experience.");
            if (ShouldAddCategoryNameToMainMenu(myMenuItems, "EXTRAS"))
                myCategories.Add("EXTRAS", "Adds extra functionality to the ship terminal.");
            if (ShouldAddCategoryNameToMainMenu(myMenuItems, "CONTROLS"))
                myCategories.Add("CONTROLS", "Gives terminal more control of the ship's systems.");
            if (ShouldAddCategoryNameToMainMenu(myMenuItems, "FUN"))
                myCategories.Add("FUN", "Type \"fun\" for a list of these [FUN]ctional commands.");

            if (myCategories.Count == 0)
            {
                Plugin.WARNING("No enabled commands? ending menu creation");
            }
            myMenuCategories = InitCategories(myCategories);

            //CatName = item.Key,
            //CatDescription = item.Value
            CreateDarmuhsTerminalStuffMenus();
        }

        internal static void CreateDarmuhsTerminalStuffMenus()
        {
            Plugin.Spam("START CreateDarmuhsTerminalStuffMenus");
            EventSub.TerminalStart.InitiateTerminalStuff();
            if (!ConfigSettings.CreateMoreMenus.Value)
            {
                if (!DynamicBools.TryGetKeyword("other", out TerminalKeyword otherWord))
                    return;

                foreach (TerminalMenuItem item in myMenuItems)
                {
                    if (item.itemKeywords.Count == 0)
                    {
                        Plugin.WARNING($"{item.ItemName} has no keywords!!");
                        continue;
                    }

                    if (item == null)
                    {
                        Plugin.WARNING($"NULL ITEM IN myMenuItems!!!");
                        continue;
                    }

                    AddingThings.AddToExistingNodeText($"\n>{CommonStringStuff.GetKeywordsForMenuItem(item.itemKeywords).ToUpper()}\n{item.itemDescription}", ref otherWord.specialKeywordResult);
                    Plugin.Spam($"{item.ItemName} keywords added to other menu");
                }
                return;
            }
            myMenu = AssembleMainMenu("darmuhsTerminalStuff", "more", ConfigSettings.MoreMenuText.Value, myMenuCategories, myMenuItems);
            AddingThings.AddToHelpCommand(ConfigSettings.MoreHintText.Value);
            if (LogicHandling.TryGetFromAllNodes("OtherCommands", out TerminalNode otherNode))
                AddingThings.AddToExistingNodeText($"\n{ConfigSettings.MoreHintText.Value}", ref otherNode);

            Plugin.Spam($"myMenu info:\nMenuName: {myMenu.MenuName}\nmyMenu.Categories.Count: {myMenu.Categories.Count}\n");


            if (ConfigSettings.FauxMoreMenu.Value)
                CreateCategoryFauxCommands(myMenu, defaultListing);
            else
                CreateCategoryCommands(myMenu, ConfigSettings.TerminalStuffMain);

            Plugin.Spam("END CreateDarmuhsTerminalStuffMenus");

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
            myMenuItems = TerminalMenuItems(defaultManaged);
            myMenu.menuItems = myMenuItems;
            AddMenuItems(ConfigSettings.TerminalStuffBools, myMenu);
            UpdateCategories(myMenu);
        }

        internal static void ClearMyMenustuff()
        {
            myMenu?.Delete();
            myMenuItems.Clear();
            myMenuCategories.Clear();
        }
    }
}

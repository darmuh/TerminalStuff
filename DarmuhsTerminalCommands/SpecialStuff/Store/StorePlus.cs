using OpenLib.CoreMethods;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static TerminalStuff.EventSub.TerminalStart;
using static OpenLib.CoreMethods.AddingThings;
using static OpenLib.ConfigManager.ConfigSetup;
using static TerminalStuff.TerminalEvents;
using System.Text;
using TerminalStuff.PluginCore;
using HarmonyLib;
using TerminalStuff.Configs;
using static TerminalStuff.AllMyTerminalPatches;
using BepInEx;
using TerminalStuff.Compatibility;
using TerminalStuff.SpecialStuff.Store;

namespace TerminalStuff.SpecialStuff
{
    internal class StorePlus
    {
        internal static InteractiveMenu StorePlusMenu = new("storeMenu", LoadPage, SelectInMenu, ExitInTerminal);
        internal static TerminalNode OriginalStorePage = null!;

        internal static TerminalNode StoreMenu = null!;

        internal static int MenuLevel = 0; //to navigate to nested menu levels
        internal static int SubTotal = 0; //subtotal before purchase, resets to 0 at launch of menu

        internal static List<StoreInfo> AllStoreItems = [];
        internal static List<StoreInfo> DisplayItems = [];
        internal static StoreMenuItem CurrentMenu = null!;
        internal static List<StoreMenuItem> storeMenuMain = [];
        internal static List<StoreMenuItem> storeMenuItemsDisplay = [];
        internal static List<StoreInfo> storeSelection = [];
        internal static List<int> excludedNodesFromAutoGen = [];

        internal static List<string> ManualUpgradeNames = [];

        //menuitems
        internal static StoreMenuItem AllBuyables = new ("Items", 1, false);
        internal static StoreMenuItem AllUpgrades = new ("Upgrades & Furniture", 1, false);
        internal static StoreMenuItem AllVehicles = new ("Vehicles", 1, false);
        internal static StoreMenuItem AllBuyableSuits = new ("Suits", 1, false);
        internal static StoreMenuItem AllPurchasePacks = new("Purchase Packs", 1, false);
        internal static StoreMenuItem AllExternalModMenus = new("Other", 10, false);

        internal static void SetToVanilla()
        {
            StorePlusMenu.isMenuEnabled = false;

            if (OriginalStorePage == null)
                return;

            if (DynamicBools.TryGetKeyword("store", out TerminalKeyword Store))
            {
                Store.specialKeywordResult = OriginalStorePage;
                Plugin.Spam("Moons keyword set back to original");
            }
        }

        internal static void LoadPage()
        {
            if (StorePlusMenu.acceptAnything)
                return;

            Plugin.instance.Terminal.StartCoroutine(DelayUpdateText());
        }

        internal static void SelectInMenu()
        {
            if(MenuLevel == 0)
            {
                UpdateMenuSelection();
                return;
            }
            
            StoreMenuItem current = storeMenuMain.First(x => x.menuLevel == MenuLevel && x.active);
            if(current == null)
            {
                Plugin.WARNING("Unable to select current item!!");
                return;
            }

            if(current.nestedMenuItems.Count > 0)
            {
                Plugin.Spam($"Selecting Nested Menu Item!");
                StoreMenuItem selected = current.nestedMenuItems[StorePlusMenu.activeSelection];

                selected.MenuSpecialAction?.Invoke();

                if (selected.externalCommand)
                {
                    Plugin.Spam("Taking player to external command!");
                    StorePlusMenu.inMenu = false;
                    StorePlusMenu.acceptAnything = false;
                    Plugin.instance.Terminal.screenText.caretColor = TerminalCustomizer.SetColorFor(CustomizeConfig.TerminalCaretColor.Value, CustomTerminalStuff.TextCaret);
                    Plugin.instance.Terminal.screenText.ActivateInputField();
                    Plugin.instance.Terminal.screenText.interactable = true;
                    StartofHandling.HandleShortcutFinal(selected.Keyword);
                    return;
                }

                if (selected.nestedMenuItems.Count > 0)
                {
                    Plugin.Spam("Setting to nested menu item!");
                    StorePlusMenu.currentPage = 0;
                    CurrentMenu.active = false;
                    selected.active = true;
                    MenuLevel = selected.menuLevel;
                    CurrentMenu = selected;
                    StorePlusMenu.activeSelection = 0;
                }

                StoreMenu.displayText = GetStorePage(StorePlusMenu.activeSelection, 6, ref StorePlusMenu.currentPage, ref MenuLevel);
                LoadAndSync(StoreMenu);
                return;
            }

            if(current.nestedMenuItems.Count > 0)
            {
                
            }

            StoreInfo selection = DisplayItems[StorePlusMenu.activeSelection];

            if (!selection.selected && selection.price > Plugin.instance.Terminal.groupCredits - SubTotal - StoreSettings.savings)
                return;

            selection.selected = !selection.selected;

            if (selection.selected)
                storeSelection.Add(selection);
            else
                storeSelection.Remove(selection);

            UpdateSubtotal();
            LoadPage();
        }

        internal static void IncreaseItemCount()
        {
            Plugin.Spam("IncreaseItemCount");

            if (MenuLevel == 0)
                return;

            StoreInfo selection = DisplayItems[StorePlusMenu.activeSelection];

            int selectedCount = DisplayItems.FindAll(x=>x.selected).Sum(s=>s.selectionCount) + Plugin.instance.Terminal.orderedItemsFromTerminal.Count;

            if (!selection.selected)
                return;

            if (selection.maxAllowed != 0 && selection.maxAllowed <= selection.selectionCount)
                return;

            if (selection.isVehicle)
                return;

            if (SubTotal + selection.price > Plugin.instance.Terminal.groupCredits - StoreSettings.savings)
                return;

            if (selection.selectionCount >= ConfigGetters.GetMaxItems() || selectedCount >= ConfigGetters.GetMaxItems())
                return;

            if (selection.isPurchasePack)
            {
                if (StorePacks.GetBuyablesCountFromNode(selection.terminalNode) >= ConfigGetters.GetMaxItems())
                    return;
            }
                    

            int newNum = selection.selectionCount+1;
            SetCount(ref selection, newNum);
        }

        internal static void DecreaseItemCount()
        {
            Plugin.Spam("DecreaseItemCount");

            if (MenuLevel == 0)
                return;

            StoreInfo selection = DisplayItems[StorePlusMenu.activeSelection];

            if (!selection.selected)
                return;

            int newNum = selection.selectionCount-1;
            SetCount(ref selection, newNum);
        }

        internal static void SetCount(ref StoreInfo selection, int newCount)
        {
            Plugin.Spam($"Original count: {selection.selectionCount} vs newCount - {newCount}");
            selection.selectionCount = Mathf.Clamp(newCount, 1, 99);
            UpdateSubtotal();
            LoadPage();
        }

        internal static void ExitInTerminal()
        {
            if (MenuLevel == 0)
                ExitMenu(true);
            else if(CurrentMenu.ParentMenu != null)
            {
                Plugin.Spam("Setting to ParentMenu!");
                StorePlusMenu.currentPage = 0;
                CurrentMenu.active = false;
                CurrentMenu.ParentMenu.active = true;
                MenuLevel = CurrentMenu.ParentMenu.menuLevel;
                CurrentMenu = CurrentMenu.ParentMenu;
                StoreMenu.displayText = GetStorePage(0, 6, ref StorePlusMenu.currentPage, ref MenuLevel);
                LoadAndSync(StoreMenu);
            }
            else
            {
                Plugin.Spam($"Setting MenuLevel to 0");
                MenuLevel = 0;
                StorePlusMenu.currentPage = 0;
                UpdateMenuSelection(true);
                StoreMenu.displayText = GetStorePage(0, 6, ref StorePlusMenu.currentPage, ref MenuLevel);
                LoadAndSync(StoreMenu);
            }
        }

        internal static IEnumerator MenuClose(bool enableInput)
        {
            yield return new WaitForEndOfFrame();
            StorePlusMenu.inMenu = false;
            StorePlusMenu.acceptAnything = false;
            yield return new WaitForEndOfFrame();

            TerminalNode nextNode = startNode;

            if (terminalSettings.startPage != null)
                nextNode = terminalSettings.startPage;

            LoadAndSync(nextNode);

            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.caretColor = TerminalCustomizer.SetColorFor(CustomizeConfig.TerminalCaretColor.Value, CustomTerminalStuff.TextCaret);

            if (enableInput)
            {
                Plugin.instance.Terminal.screenText.ActivateInputField();
                Plugin.instance.Terminal.screenText.interactable = true;
            }

            yield break;
        }

        internal static void ExitMenu(bool enableInput)
        {
            Plugin.instance.Terminal.StartCoroutine(MenuClose(enableInput));
        }

        internal static IEnumerator DelayUpdateText()
        {
            yield return new WaitForEndOfFrame();
            StoreMenu.displayText = GetStorePage(StorePlusMenu.activeSelection, 6, ref StorePlusMenu.currentPage, ref MenuLevel);


            yield return new WaitForEndOfFrame();
            LoadAndSync(StoreMenu);
            yield return new WaitForEndOfFrame();

        }

        internal static IEnumerator PurchaseResultPage()
        {
            yield return new WaitForEndOfFrame();
            StoreMenu.displayText = GetResultPage();
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);

            yield return new WaitForEndOfFrame();
            LoadAndSync(StoreMenu);
            yield return new WaitForEndOfFrame();
        }

        internal static int GetSalesPrice(int originalPrice, int itemID, bool isVehicle = false)
        {

            if (itemID == -1)
                return originalPrice;

            if (isVehicle)
                itemID = Plugin.instance.Terminal.buyableItemsList.Length + itemID;

            if (Plugin.instance.Terminal.itemSalesPercentages.Length < itemID)
                return originalPrice;

            float newPrice = (float)originalPrice * (float)(Plugin.instance.Terminal.itemSalesPercentages[itemID] / 100f);

            return (int)newPrice;
        }

        internal static int GetSalesPercentage(StoreInfo item)
        {
            if (item.terminalNode.buyItemIndex != -1)
                return 100 - Plugin.instance.Terminal.itemSalesPercentages[item.terminalNode.buyItemIndex];
            

            if (item.terminalNode.buyVehicleIndex != -1)
                return 100 - Plugin.instance.Terminal.itemSalesPercentages[Plugin.instance.Terminal.buyableItemsList.Length + item.terminalNode.buyVehicleIndex];

            return 0;
                
        }

        internal static string GetResultPage()
        {
            StorePlusMenu.acceptAnything = true;
            StringBuilder message = new();

            message.Append($"\r\n");

            foreach(StoreInfo item in storeSelection)
            {
                if(item.isPurchasePack)
                {
                    List<string> packList = StorePacks.GetItemListFromNode(item.terminalNode);
                    message.Append($"${item.price} [{item.name}] x {item.selectionCount}\r\n");
                    foreach(string packItem in packList)
                        message.Append($"    {packItem}\n");
                    continue;
                }

                message.Append($"${item.price} {item.name} x {item.selectionCount}\n");
            }

            message.Append($"\r\n\r\nTotal Value of Purchase: <color=#e6b800>{SubTotal}</color>");
            message.Append($"\r\nNew Credits Ammount: {Plugin.instance.Terminal.groupCredits}");

            message.Append("\r\n\r\nPress any key to continue...");
            return message.ToString();
        }

        internal static string GetStorePage(int activeIndex, int pageSize, ref int currentPage, ref int menuLevel)
        {
            StringBuilder message = new();

            string menuItem;

            if (menuLevel == 0)
            {
                message.Append($"================= Store Plus  =================\r\n\r\n");
                Plugin.Spam($"MenuLevel is 0, showing main menuitems");
                storeMenuItemsDisplay = storeMenuMain.FindAll(x => x.storeItems.FindAll(y => y.InRotation()).Count > 0 || x.nestedMenuItems.Count > 0 || x.externalCommand);

                currentPage = Mathf.Clamp(currentPage, 1, Mathf.CeilToInt((float)storeMenuItemsDisplay.Count / pageSize));
                int startIndex = (currentPage - 1) * pageSize;
                int endIndex = Mathf.Min(startIndex + pageSize, storeMenuItemsDisplay.Count);
                activeIndex = Mathf.Clamp(activeIndex, startIndex, endIndex-1);
                Plugin.Spam($"activeSelection: {StorePlusMenu.activeSelection} activeIndex: {activeIndex}");
                StorePlusMenu.activeSelection = activeIndex;

                for (int i = startIndex; i < endIndex; i++)
                {
                    menuItem = (i == activeIndex)
                   ? $"> "
                   : $"";

                    menuItem += storeMenuItemsDisplay[i].MenuName;
                    message.Append(menuItem + "\n");
                }

                int emptySpace = (endIndex - startIndex - pageSize);

                if (emptySpace < 0)
                {
                    for (int i = emptySpace; i < 0; i++)
                        message.Append("\n");
                }

                message.Append($"\r\n\r\nCurrent Selection Total: [ <color=#e6b800>${SubTotal}</color> ]\r\n\r\n");
                message.Append($"Page [LeftArrow] < {currentPage}/{Mathf.CeilToInt((float)storeMenuItemsDisplay.Count / pageSize)} > [RightArrow]\r\n");
                message.Append($"Select Store Category: [Enter]\r\n");
                message.Append($"Leave Menu: [BackSpace]\r\n\r\n");
            }
            else if(menuLevel >= 10) //nested menu before main menu
            {
                Plugin.Spam("displaying nested menu");
                int level = menuLevel;
                StoreMenuItem match = storeMenuMain.First(x => x.menuLevel == level && x.active);
                StoreMenuItem.UpdateDisplayMenu(match);
                message.Append($"=== Store Plus [ {match.MenuName} ]  ===\r\n\r\n");
                currentPage = Mathf.Clamp(currentPage, 1, Mathf.CeilToInt((float)storeMenuItemsDisplay.Count / pageSize));
                int startIndex = (currentPage - 1) * pageSize;
                int endIndex = Mathf.Min(startIndex + pageSize, storeMenuItemsDisplay.Count);
                activeIndex = Mathf.Clamp(activeIndex, startIndex, endIndex - 1);
                Plugin.Spam($"activeSelection: {StorePlusMenu.activeSelection} activeIndex: {activeIndex}");
                StorePlusMenu.activeSelection = activeIndex;

                for (int i = startIndex; i < endIndex; i++)
                {
                    menuItem = (i == activeIndex)
                   ? $"> "
                   : $"";

                    menuItem += storeMenuItemsDisplay[i].MenuName;
                    message.Append(menuItem + "\n");
                }

                int emptySpace = (endIndex - startIndex - pageSize);

                if (emptySpace < 0)
                {
                    for (int i = emptySpace; i < 0; i++)
                        message.Append("\n");
                }

                message.Append($"\r\n\r\nCurrent Selection Total: [ <color=#e6b800>${SubTotal}</color> ]\r\n\r\n");
                if (match.bottomTextAdd.Length > 0)
                    message.Append(match.bottomTextAdd);
                message.Append($"Page [LeftArrow] < {currentPage}/{Mathf.CeilToInt((float)storeMenuItemsDisplay.Count / pageSize)} > [RightArrow]\r\n");
                message.Append($"Make Selection: [Enter]\r\n");
                message.Append($"Leave Menu: [BackSpace]\r\n\r\n");
            }
            else
            {
                Plugin.Spam($"MenuLevel is {menuLevel}, checking matching values (store items)");
                int level = menuLevel;
                StoreMenuItem match = storeMenuMain.First(x => x.menuLevel == level && x.active);

                if (match == null)
                {
                    message.Append($"Unable to resolve current menu! (please report this)\r\n");
                    menuLevel = 0;
                    return message.ToString();
                }

                message.Append($"=== Store Plus [ {match.MenuName} ]  ===\r\n\r\n");
                DisplayItems = match.storeItems.FindAll(x => x.InRotation());
                
                StoreSettings.UpdateSort(ref DisplayItems);
                currentPage = Mathf.Clamp(currentPage, 1, Mathf.CeilToInt((float)DisplayItems.Count / pageSize));

                int startIndex = (currentPage - 1) * pageSize;
                int endIndex = Mathf.Min(startIndex + pageSize, DisplayItems.Count);
                activeIndex = Mathf.Clamp(activeIndex, startIndex, endIndex-1);
                Plugin.Spam($"activeSelection: {StorePlusMenu.activeSelection} activeIndex: {activeIndex}");
                StorePlusMenu.activeSelection = activeIndex;

                if(DisplayItems.Count != 0)
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        StoreInfo item = DisplayItems[i];

                        item.PriceChecks();

                        menuItem = (i == activeIndex)
                       ? $"> "
                       : $"";

                        if (item.isVehicle && Plugin.instance.Terminal.hasWarrantyTicket)
                            menuItem += $"${item.price} (warranty) ";
                        else
                            menuItem += $"${item.price} ";

                        menuItem += item.name;

                        if (item.selectionCount > 1)
                            menuItem += $" x {item.selectionCount}";

                        if (item.selected)
                            menuItem += " *";

                        if (item.onSale)
                            menuItem += $"   ({GetSalesPercentage(item)}% OFF!)";

                        if (item.price <= GetProjectedCredits() && StorePlusConfig.AffordableColor.Value.Length > 0)
                        {
                            menuItem = menuItem.Insert(0, $"<color={StorePlusConfig.AffordableColor.Value}>");
                            menuItem += "</color>";
                        }

                        if (item.price > GetProjectedCredits() && StorePlusConfig.NotEnoughCredsColor.Value.Length > 0 && !item.selected)
                        {
                            menuItem = menuItem.Insert(0, $"<color={StorePlusConfig.NotEnoughCredsColor.Value}>");
                            menuItem += "</color>";
                        }

                        message.Append($"{menuItem}\n");
                    }
                }

                

                int emptySpace = (endIndex - startIndex - pageSize);

                if(emptySpace < 0)
                {
                    for (int i = emptySpace; i < 0; i++)
                        message.Append("\n");
                }

                message.Append($"\r\n\r\nCurrent Selection Total: [ <color=#e6b800>${SubTotal}</color> ]\r\n\r\n");
                message.Append($"Page [LeftArrow] < {currentPage}/{Mathf.CeilToInt((float)DisplayItems.Count / pageSize)} > [RightArrow]\r\n");
                message.Append($"Decrease Count [Z] / Increase Count [X]\r\n");
                message.Append($"Add/Remove Item: [Enter]  / Complete Purchase: [P]\r\n");
                message.Append($"Leave Menu: [BackSpace]\r\n\r\n");
            }

            return message.ToString();
        }

        internal static int GetProjectedCredits()
        {
            return Plugin.instance.Terminal.groupCredits - SubTotal - StoreSettings.savings;
        }

        internal static void UpdateSubtotal()
        {
            int sub = 0;

            foreach(StoreInfo item in storeSelection)
            {
                sub += item.price * item.selectionCount;
            }

            SubTotal = sub;
        }

        internal static void GetStoreItems()
        {
            if (!Commands.TerminalStorePlus.Value)
            {
                SetToVanilla();
                return;
            }

            if (Commands.TerminalRefund.Value)
                StoreInfo.MakeMainMenuItem($"{OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(KeywordConfigs.RefundKeywords.Value)[0]}", "Refund");

            if (Plugin.instance.ITAPI)
                InteractiveAPI.AddToStorePlus();

            if (OpenLib.Plugin.instance.TooManyEmotes)
                StoreInfo.MakeStoreInfo("emote", "TooManyEmotes Store");

            ManualUpgradeNames.AddRange(["Inverse Teleporter", "Teleporter", "Signal Translator", "Loud Horn"]);

            List<TerminalNode> nodes = LogicHandling.GetAllNodes();
            List<TerminalNode> unlockables = nodes.FindAll(x => x.shipUnlockableID > -1);
            List<TerminalNode> buyables = nodes.FindAll(n => StoreInfo.IsValidItem(n) && StoreInfo.IsItemEnabled(n.buyItemIndex));
            List<TerminalNode> terminalVehicles = nodes.FindAll(n => n.buyVehicleIndex > -1 && n.creatureFileID == -1 && n.buyItemIndex == -1 && n.buyRerouteToMoon == -1);

            foreach (TerminalNode node in unlockables)
            {
                if (node.itemCost < 0)
                    continue;

                if (excludedNodesFromAutoGen.Contains(node.shipUnlockableID))
                    continue;

                if (node.creatureName.IsNullOrWhiteSpace())
                    continue;

                StoreInfo info = AllStoreItems.FirstOrDefault(x => x.name == node.creatureName);
                if (info != null)
                    info.UpdateNode(node);
                else
                {
                    info = new(node);
                    AllStoreItems.Add(info);
                }
            }

            foreach(TerminalNode node in terminalVehicles)
            {
                StoreInfo item = AllStoreItems.FirstOrDefault(v => v.terminalNode.buyVehicleIndex == node.buyVehicleIndex);

                if (item != null)
                    item.UpdateNode(node);
                else
                {
                    item = new(node);
                    AllStoreItems.Add(item);
                }

                continue;
            }

            foreach(TerminalNode buyNode in buyables)
            {
                StoreInfo item = AllStoreItems.FirstOrDefault(y => y.terminalNode.buyItemIndex == buyNode.buyItemIndex);

                if (item != null)
                    item.UpdateNode(buyNode);
                else
                {
                    item = new(buyNode);
                    AllStoreItems.Add(item);
                }

                continue;
            } 

            Plugin.Spam($"AllStoreItems count: {AllStoreItems.Count}");
            SortStoreItems();
            KeyActions();

            if (!StorePlusConfig.StorePlusKeywords.Value.Split(';').Any(x => x.Trim().ToLower() == "store"))
            {
                StoreMenu = AddNodeManual("StorePlus", StorePlusConfig.StorePlusKeywords, EnterStoreMenu, true, 0, ConfigSettings.TerminalStuffMain, defaultManaged, "EXTRAS", "Open the Store Plus Page");
                return;
            }

            if (DynamicBools.TryGetKeyword("store", out TerminalKeyword Store))
            {
                StoreMenu = AddNodeManual("StorePlus", StorePlusConfig.StorePlusKeywords, EnterStoreMenu, true, 0, ConfigSettings.TerminalStuffMain, defaultManaged, "EXTRAS", "Open the Store Plus Page");

                OriginalStorePage = Store.specialKeywordResult;
                StoreMenu.displayText = OriginalStorePage.displayText;
                Store.specialKeywordResult = StoreMenu;
                Plugin.Log.LogMessage("Store page replaced with StorePlus page!");
            }
            else
                Plugin.ERROR("UNABLE TO GET STORE KEYWORD FOR MENU!\nUNABLE TO GET STORE KEYWORD FOR MENU!\nUNABLE TO GET STORE KEYWORD FOR MENU!");
        }

        internal static void ResetVars()
        {
            foreach(StoreMenuItem item in storeMenuItemsDisplay)
                item.storeItems.DoIf(x => x.selected || x.selectionCount > 1, ResetItem);

            StorePlusMenu.acceptAnything = false;
            StorePlusMenu.currentPage = 1;
            MenuLevel = 0;
            SubTotal = 0;
            storeSelection = [];
        }

        internal static void ReturnToStore()
        {
            ResetVars();
            StoreMenu.displayText = GetStorePage(0, 6, ref StorePlusMenu.currentPage, ref MenuLevel);
            LoadAndSync(StoreMenu);
        }

        internal static string EnterStoreMenu()
        {
            ResetVars();
            Plugin.instance.Terminal.StartCoroutine(MenuStart());
            return GetStorePage(0, 6, ref StorePlusMenu.currentPage, ref MenuLevel);
        }

        internal static IEnumerator MenuStart()
        {
            if (StorePlusMenu.inMenu)
                yield break;

            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.caretColor = transparent;
            StorePlusMenu.inMenu = true;
            yield return new WaitForEndOfFrame();
            Plugin.instance.Terminal.screenText.DeactivateInputField();
            Plugin.instance.Terminal.screenText.interactable = false;
            yield return new WaitForEndOfFrame();
            LoadAndSync(StoreMenu);
            yield break;
        }

        internal static void KeyActions()
        {
            StorePlusMenu.AddToOtherActions(UnityEngine.InputSystem.Key.P, CompletePurchase);
            StorePlusMenu.AddToOtherActions(UnityEngine.InputSystem.Key.Z, DecreaseItemCount);
            StorePlusMenu.AddToOtherActions(UnityEngine.InputSystem.Key.X, IncreaseItemCount);
            StorePlusMenu.AcceptAnyKeyEvent.AddListener(ReturnToStore);
            StorePlusMenu.isMenuEnabled = true;
        }

        internal static void AddNestedMenuItem(StoreMenuItem item, string menuName)
        {
            Plugin.Spam($"AddNestedMenuItem! {item.MenuName} for menu [ {menuName} ] with level [ {item.menuLevel} ]");
            StoreMenuItem menuItem = storeMenuMain.FirstOrDefault(x => x.MenuName == menuName);

            if (menuItem == null)
            {
                Plugin.WARNING($"AddNestedMenuItem Failed! menuName [ {menuName} ] could not be found!");
                return;
            }

            if (!menuItem.nestedMenuItems.Contains(item))
                menuItem.nestedMenuItems.Add(item);
        }

        internal static void AddToStorePlus(StoreInfo item, string menuName = "")
        {
            Plugin.Spam($"AddToStorePlus! {item.name} for menu [ {menuName} ]");
            StoreMenuItem menuItem;
            if (menuName.Length > 0)
                menuItem = storeMenuMain.FirstOrDefault(x => x.MenuName == menuName);
            else
                menuItem = null!;

            if(menuItem != null)
            {
                if(!menuItem.storeItems.Contains(item))
                    menuItem.storeItems.Add(item);
                
                return;
            }
            else
            {
                SortStoreItem(item);
            }
        }

        internal static void SortStoreItems()
        {
            if (AllStoreItems.Count < 1)
                return;

            foreach (StoreInfo item in AllStoreItems)
                SortStoreItem(item);

        }

        private static void SortStoreItem(StoreInfo item)
        {
            StoreSettings.Init();

            if (item.isPurchasePack)
            {
                Plugin.Spam($"Purchase Pack detected @ SortStoreItem: {item.name}");

                if (!AllPurchasePacks.storeItems.Contains(item))
                    AllPurchasePacks.storeItems.Add(item);
                return;
            }

            if (item.isSuit && item.name.Length > 1)
            {
                Plugin.Spam($"Buyable Suit detected @ SortStoreItem: {item.name}");

                if (!AllBuyableSuits.storeItems.Contains(item))
                    AllBuyableSuits.storeItems.Add(item);
                return;
            }

            if (item.isVehicle)
            {
                Plugin.Spam($"Buyable Vehicle detected @ SortStoreItem: {item.name}");

                if (!AllVehicles.storeItems.Contains(item))
                    AllVehicles.storeItems.Add(item);
                return;
            }

            if (!item.waitForDelivery)
            {
                Plugin.Spam($"Upgrade detected @ SortStoreItem: {item.name}");
                if (!AllUpgrades.storeItems.Contains(item))
                    AllUpgrades.storeItems.Add(item);

                return;
            }

            Plugin.Spam($"Buyable Item detected @ SortStoreItem: {item.name}");

            if (!AllBuyables.storeItems.Contains(item))
                AllBuyables.storeItems.Add(item);
                
        }

        internal static void UpdateMenuSelection(bool selectNone = false)
        {
            if (MenuLevel != 0)
                return;

            foreach (StoreMenuItem item in storeMenuItemsDisplay)
            {
                if (selectNone)
                {
                    CurrentMenu = null!;
                    item.active = false;
                    continue;
                }

                if(item == storeMenuItemsDisplay[StorePlusMenu.activeSelection])
                {
                    CurrentMenu = item;
                    item.active = true;
                    MenuLevel = item.menuLevel;
                    if (item.externalCommand)
                    {
                        Plugin.Spam("Taking player to external command!");
                        StorePlusMenu.inMenu = false;
                        StorePlusMenu.acceptAnything = false;
                        Plugin.instance.Terminal.screenText.caretColor = TerminalCustomizer.SetColorFor(CustomizeConfig.TerminalCaretColor.Value, CustomTerminalStuff.TextCaret);
                        Plugin.instance.Terminal.screenText.ActivateInputField();
                        Plugin.instance.Terminal.screenText.interactable = true;
                        StartofHandling.HandleShortcutFinal(item.Keyword);
                        return;
                    }
                        
                }
                else
                    item.active = false;         
            }

            StorePlusMenu.activeSelection = 0;
            StorePlusMenu.currentPage = 1;
            LoadPage();
        }

        internal static void ResetItem(StoreInfo storeInfo)
        {
            storeInfo.Reset();
        }

        internal static void CompletePurchase()
        {
            if (StorePlusMenu.acceptAnything)
                return;

            if (MenuLevel == 0) //selecting a menu
                return;

            if (storeSelection.Count == 0)
                return;

            if (Plugin.instance.Terminal.groupCredits - StoreSettings.savings < SubTotal)
                return;

            List<StoreInfo> dropshipItems = [];

            foreach(StoreInfo item in storeSelection)
            {
                Plugin.Spam($"storeSelection contains: {item.name}");

                if (item.isPurchasePack)
                    StorePacks.CompletePurchaseV2(item.terminalNode, item.selectionCount);
                else if (item.isVehicle)
                    VehiclePurchase(item);
                else if (item.waitForDelivery && !item.isPurchasePack) //deliverable items and vehicles
                    dropshipItems.Add(item);
                else
                {
                    //unlockables
                    if (item.terminalNode.shipUnlockableID < 0 || item.terminalNode.shipUnlockableID > StartOfRound.Instance.unlockablesList.unlockables.Count)
                        continue;
                    
                    if(item.isPurchasePack)
                        continue;

                    StartOfRound.Instance.BuyShipUnlockableServerRpc(item.terminalNode.shipUnlockableID, Plugin.instance.Terminal.groupCredits - item.price);
                }
            }

            if(dropshipItems.Count > 0)
            {
                storeSelection.RemoveAll(x => dropshipItems.Contains(x));

                DropShipPurchase(ref dropshipItems);

                storeSelection.AddRange(dropshipItems);
            }

            Plugin.instance.Terminal.StartCoroutine(PurchaseResultPage());
        }

        internal static void VehiclePurchase(StoreInfo item)
        {
            if(Plugin.instance.Terminal.hasWarrantyTicket)
                Plugin.instance.Terminal.BuyVehicleServerRpc(item.terminalNode.buyVehicleIndex, Plugin.instance.Terminal.groupCredits, true);
            else
                Plugin.instance.Terminal.BuyVehicleServerRpc(item.terminalNode.buyVehicleIndex, Plugin.instance.Terminal.groupCredits - item.price);
        }

        internal static void DropShipPurchase(ref List<StoreInfo> items)
        {
            List<int> add = [];
            int cost = 0;
            int fullItemsCount = items.Sum(x => x.selectionCount);
            int skip = 0;

            if (fullItemsCount + add.Count > ConfigGetters.GetMaxItems())
                skip = fullItemsCount + add.Count - ConfigGetters.GetMaxItems();

            Plugin.Spam($"Skipping {skip} items from DropShipPurchase!");

            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (items[i].isPurchasePack)
                {
                    Plugin.WARNING($"PURCHASE PACK {items[i].name} DETECTED IN REGULAR PURCHASE!!");
                    continue;
                }

                if (skip > 0 && items.Count -1 - skip <= i)
                {
                    if (items[i].selectionCount > 1 && items[i].selectionCount - skip > 0)
                    {
                        items[i].selectionCount = items[i].selectionCount - skip;
                        skip = 0;
                    }      
                    else
                    {
                        skip = skip - items[i].selectionCount;
                        items.Remove(items[i]);
                        continue;
                    }       
                }

                cost += items[i].price * items[i].selectionCount;

                if (items[i].selectionCount > 1)
                {
                    for (int s = 0; s < items[i].selectionCount; s++)
                        add.Add(items[i].terminalNode.buyItemIndex);
                }
                else
                    add.Add(items[i].terminalNode.buyItemIndex);
            }

            Plugin.Spam($"Full dropship purchase Count: {add.Count}");
            int[] fullpurchase = [.. add];

            Plugin.instance.Terminal.BuyItemsServerRpc(fullpurchase, Plugin.instance.Terminal.groupCredits - cost, fullpurchase.Length + Plugin.instance.Terminal.orderedItemsFromTerminal.Count);
        }

    }
}

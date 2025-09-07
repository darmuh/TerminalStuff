using BepInEx;
using HarmonyLib;
using OpenLib.CoreMethods;
using OpenLib.InteractiveMenus;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TerminalStuff.Util;
using UnityEngine;
using static TerminalStuff.Patching.AllMyTerminalPatches;
using static TerminalStuff.TerminalEvents;

namespace TerminalStuff.StoreTweaks;

internal class StorePlus
{
    //BetterMenus
    internal static BetterMenu<StoreMenuItem> StorePlusMenu = new("StorePlus");
    internal static CommandManager StoreCommand = null!;
    private static bool InitOnce = false;

    //BetterMenuItems
    internal static StoreMenuItem TheMainMenu = new("StorePlus");
    internal static StoreMenuItem Buyables = new("Items");
    internal static StoreMenuItem Upgrades = new("Upgrades & Furniture");
    internal static StoreMenuItem Vehicles = new("Vehicles");
    internal static StoreMenuItem Suits = new("Suits");
    internal static StoreMenuItem Packs = new("Purchase Packs");
    internal static StoreMenuItem ExternalMods = new("Other");

    //BetterMenuItem that changes
    internal static StoreMenuItem Current = null!;

    //TerminalNodes
    internal static TerminalNode OriginalStorePage = null!;

    //Misc
    //internal static string LoadExtKey = ""; // 
    internal static int SubTotal = 0; //subtotal before purchase, resets to 0 at launch of menu
    internal static List<StoreInfo> AllStoreItems = [];
    internal static List<StoreInfo> storeSelection = [];
    internal static List<int> excludedNodesFromAutoGen = [];
    internal static List<string> ManualUpgradeNames = [];

    internal static void SetToVanilla()
    {
        StorePlusMenu.IsMenuEnabled = false;

        if (OriginalStorePage == null)
            return;

        if (DynamicBools.TryGetKeyword("store", out TerminalKeyword Store))
        {
            Store.specialKeywordResult = OriginalStorePage;
            Loggers.LogDebug("Moons keyword set back to original");
        }
    }

    private static void SetKeys()
    {
        StorePlusMenu.AddToOtherActions(UnityEngine.InputSystem.Key.P, CompletePurchase);
        StorePlusMenu.AddToOtherActions(UnityEngine.InputSystem.Key.Z, DecreaseItemCount);
        StorePlusMenu.AddToOtherActions(UnityEngine.InputSystem.Key.X, IncreaseItemCount);
    }

    internal static void InitBetterMenu()
    {
        if (InitOnce)
            return;

        StorePlusMenu.PageSize = 6; // add config item
        StorePlusMenu.MainMenu = TheMainMenu;
        StorePlusMenu.OnExit.AddListener(OnExit);

        //Global Events
        StorePlusMenu.OnLoad.AddListener(OnAnyPageLoad);
        StorePlusMenu.AcceptAnyKeyEvent.AddListener(ReturnToStore);

        //Main Menu
        TheMainMenu.Header = () => "================= Store Plus =================\r\n\r\n";
        TheMainMenu.Footer = GetMainFooter;

        //Items
        Buyables.Header = () => "================= Items =================\r\n\r\n";
        Buyables.Footer = GetStoreFooter;
        Buyables.SetParentMenu(TheMainMenu);

        //Upgrades & Furniture
        Upgrades.Header = () => "================= Upgrades =================\r\n\r\n";
        Upgrades.Footer = GetStoreFooter;
        Upgrades.SetParentMenu(TheMainMenu);

        //Vehicles
        Vehicles.Header = () => "================= Vehicles =================\r\n\r\n";
        Vehicles.Footer = GetStoreFooter;
        Vehicles.SetParentMenu(TheMainMenu);

        //Suits
        Suits.Header = () => "============= Buyable Suits =============\r\n\r\n";
        Suits.Footer = GetStoreFooter;
        Suits.SetParentMenu(TheMainMenu);

        //Purchase Packs
        Packs.Header = () => "============= Purchase Packs =============\r\n\r\n";
        Packs.Footer = GetStoreFooter;
        Packs.SetParentMenu(TheMainMenu);

        //Other Mod Menus
        ExternalMods.Header = () => "============= Other Menus =============\r\n\r\n";
        ExternalMods.Footer = GetOtherFooter;
        ExternalMods.SetParentMenu(TheMainMenu);

        InitOnce = true;
    }

    internal static string EnterStoreMenu()
    {
        StorePlusMenu.ExitAction = null!;
        StorePlusMenu.EnterAtPage(StoreMenuItem.GetStartMenu());
        return "";
    }

    internal static void OnAnyPageLoad()
    {
        UpdateSubtotal();
        Current = StorePlusMenu.AllMenuItemsOfType.Cast<StoreMenuItem>().FirstOrDefault(m => m.IsActive);
    }

    internal static void IncreaseItemCount()
    {
        Loggers.LogDebug("IncreaseItemCount");

        StoreMenuItem menuItem = StorePlusMenu.DisplayMenuItemsOfType.Cast<StoreMenuItem>().ElementAtOrDefault(StorePlusMenu.ActiveSelection);

        if (menuItem == null)
            return;

        StoreInfo selection = menuItem.storeItem;


        int selectedCount = StorePlusMenu.DisplayMenuItemsOfType.OfType<StoreMenuItem>().Where(x => x.storeItem.selected).Sum(s => s.storeItem.selectionCount) + Plugin.instance.Terminal.orderedItemsFromTerminal.Count;

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


        int newNum = selection.selectionCount + 1;
        SetCount(ref selection, newNum);
    }

    internal static void DecreaseItemCount()
    {
        Loggers.LogDebug("DecreaseItemCount");

        StoreMenuItem menuItem = StorePlusMenu.DisplayMenuItemsOfType.Cast<StoreMenuItem>().ElementAtOrDefault(StorePlusMenu.ActiveSelection);

        if (menuItem == null)
            return;

        StoreInfo selection = menuItem.storeItem;

        if (!selection.selected)
            return;

        int newNum = selection.selectionCount - 1;
        SetCount(ref selection, newNum);
    }

    internal static void SetCount(ref StoreInfo selection, int newCount)
    {
        Loggers.LogDebug($"Original count: {selection.selectionCount} vs newCount - {newCount}");
        selection.selectionCount = Mathf.Clamp(newCount, 1, 99);
        UpdateSubtotal();
        StorePlusMenu.Load();
    }

    internal static void OnExit()
    {
        ResetVars();
    }

    internal static IEnumerator PurchaseResultPage()
    {
        yield return new WaitForEndOfFrame();
        StorePlusMenu.MenuNode.displayText = GetResultPage();
        Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);

        yield return new WaitForEndOfFrame();
        LoadAndSync(StorePlusMenu.MenuNode);
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

        float newPrice = originalPrice * (float)(Plugin.instance.Terminal.itemSalesPercentages[itemID] / 100f);

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
        StorePlusMenu.AcceptAnything = true;
        StringBuilder message = new();

        message.Append($"\r\n");

        foreach (StoreInfo item in storeSelection)
        {
            if (item.isPurchasePack)
            {
                List<string> packList = StorePacks.GetItemListFromNode(item.terminalNode);
                message.Append($"${item.price} [{item.name}] x {item.selectionCount}\r\n");
                foreach (string packItem in packList)
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

    internal static string GetMainFooter()
    {
        string bottomText = string.Empty;

        if (Current != null)
            bottomText = Current.AdditionalBottomText;

        StringBuilder message = new();
        message.Append($"\r\n\r\nCurrent Selection Total: [ <color=#e6b800>${SubTotal}</color> ]\r\n\r\n");
        if (bottomText.Length > 0)
            message.Append(bottomText);
        message.Append($"Page [LeftArrow] < {StorePlusMenu.CurrentPage}/{Mathf.CeilToInt((float)StorePlusMenu.DisplayMenuItemsOfType.Count / StorePlusMenu.PageSize)} > [RightArrow]\r\n");
        message.Append($"Select Store Category: [Enter]\r\n");
        message.Append($"Leave Menu: [BackSpace]\r\n\r\n");
        return message.ToString();
    }

    internal static string GetOtherFooter()
    {
        string bottomText = string.Empty;

        if (Current != null)
            bottomText = Current.AdditionalBottomText;

        StringBuilder message = new();
        message.Append($"\r\n\r\nCurrent Selection Total: [ <color=#e6b800>${SubTotal}</color> ]\r\n\r\n");
        if (bottomText.Length > 0)
            message.Append(bottomText);
        message.Append($"Page [LeftArrow] < {StorePlusMenu.CurrentPage}/{Mathf.CeilToInt((float)StorePlusMenu.DisplayMenuItemsOfType.Count / StorePlusMenu.PageSize)} > [RightArrow]\r\n");
        message.Append($"Make Selection: [Enter]\r\n");
        message.Append($"Back Menu: [BackSpace]\r\n\r\n");
        return message.ToString();
    }

    internal static string GetStoreFooter()
    {
        string bottomText = string.Empty;

        if (Current != null)
            bottomText = Current.AdditionalBottomText;

        StringBuilder message = new();
        message.Append($"\r\n\r\nCurrent Selection Total: [ <color=#e6b800>${SubTotal}</color> ]\r\n\r\n");
        if (bottomText.Length > 0)
            message.Append(bottomText);
        message.Append($"Page [LeftArrow] < {StorePlusMenu.CurrentPage}/{Mathf.CeilToInt((float)StorePlusMenu.DisplayMenuItemsOfType.Count / StorePlusMenu.PageSize)} > [RightArrow]\r\n");
        message.Append($"Decrease Count [Z] / Increase Count [X]\r\n");
        message.Append($"Add/Remove Item: [Enter]  / Complete Purchase: [P]\r\n");
        message.Append($"Back Menu: [BackSpace]\r\n\r\n");
        return message.ToString();
    }

    internal static int GetProjectedCredits()
    {
        return Plugin.instance.Terminal.groupCredits - SubTotal - StoreSettings.savings;
    }

    internal static void UpdateSubtotal()
    {
        Loggers.LogDebug("UpdateSubtotal!");
        int sub = 0;

        foreach (StoreInfo item in storeSelection)
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
            CreateStorePacks();
            return;
        }

        InitBetterMenu();
        StorePlusMenu.IsMenuEnabled = true;
        StorePlusMenu.ActiveSelection = 0;
        StorePlusMenu.CurrentPage = 1;
        SetKeys();

        if (Commands.TerminalRefund.Value)
        {
            _ = new StoreMenuItem($"Refund", $"{OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(KeywordConfigs.RefundKeywords.Value)[0]}", TheMainMenu);
        }

        if (Plugin.instance.ITAPI)
            InteractiveAPI.AddToStorePlus();

        if (OpenLib.Plugin.instance.TooManyEmotes)
        {
            StoreMenuItem emote = new("TooManyEmotes Store", "emote", ExternalMods);
            Loggers.LogDebug("Added TooManyEmotes menu item!");
        }


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

        foreach (TerminalNode node in terminalVehicles)
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

        foreach (TerminalNode buyNode in buyables)
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

        Loggers.LogDebug($"AllStoreItems count: {AllStoreItems.Count}");
        SortStoreItems();
        CreateStorePacks();

        if (!StoreCommand.KeywordList.Any(x => OpenLib.Common.Misc.CompareStringsInvariant(x, "store")))
        {
            StoreCommand.RegisterCommand();
            StorePlusMenu.MenuNode = StoreCommand.terminalNode;
            Plugin.Log.LogMessage("StorePlus added without replacing vanilla store!");
            return;
        }

        if (DynamicBools.TryGetKeyword("store", out TerminalKeyword Store))
        {
            OriginalStorePage = Store.specialKeywordResult;
            StoreCommand.RegisterCommand(false);
            StorePlusMenu.MenuNode = StoreCommand.terminalNode;
            StorePlusMenu.MenuNode.displayText = OriginalStorePage.displayText;
            Store.specialKeywordResult = StorePlusMenu.MenuNode;
            Plugin.Log.LogMessage("Store page replaced with StorePlus page!");
        }
        else
            Loggers.ERROR("UNABLE TO GET STORE KEYWORD FOR MENU!\nUNABLE TO GET STORE KEYWORD FOR MENU!\nUNABLE TO GET STORE KEYWORD FOR MENU!");
    }

    internal static void ResetVars()
    {
        AllStoreItems.DoIf(x => x.selected || x.selectionCount > 1, x => x.Reset());
        StorePlusMenu.AcceptAnything = false;
        StorePlusMenu.CurrentPage = 1;
        SubTotal = 0;
        storeSelection = [];
    }

    internal static void ReturnToStore()
    {
        ResetVars();
        StorePlusMenu.Load();
    }

    internal static void SortStoreItems()
    {
        if (AllStoreItems.Count < 1)
            return;

        foreach (StoreInfo item in AllStoreItems)
            SortStoreItem(item);

        StoreSettings.Init();
    }

    private static void SortStoreItem(StoreInfo item)
    {
        if (item.isPurchasePack)
        {
            Loggers.LogDebug($"Purchase Pack detected @ SortStoreItem: {item.name}");
            item.menuItem.SetParentMenu(Packs);
            return;
        }

        if (item.isSuit && item.name.Length > 1)
        {
            Loggers.LogDebug($"Buyable Suit detected @ SortStoreItem: {item.name}");
            item.menuItem.SetParentMenu(Suits);
            return;
        }

        if (item.isVehicle)
        {
            Loggers.LogDebug($"Buyable Vehicle detected @ SortStoreItem: {item.name}");
            item.menuItem.SetParentMenu(Vehicles);
            return;
        }

        if (!item.waitForDelivery)
        {
            Loggers.LogDebug($"Upgrade detected @ SortStoreItem: {item.name}");
            item.menuItem.SetParentMenu(Upgrades);
            return;
        }

        Loggers.LogDebug($"Buyable Item detected @ SortStoreItem: {item.name}");

        item.menuItem.SetParentMenu(Buyables);

    }

    internal static void CompletePurchase()
    {
        if (StorePlusMenu.AcceptAnything)
            return;

        if (storeSelection.Count == 0)
        {
            Plugin.Log.LogMessage("No items selected to purchase!");
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
            return;
        }

        //this should never happen but just in case lol
        if (Plugin.instance.Terminal.groupCredits - StoreSettings.savings < SubTotal)
        {
            Plugin.Log.LogMessage("Not enough credits!");
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
            return;
        }


        List<StoreInfo> dropshipItems = [];

        foreach (StoreInfo item in storeSelection)
        {
            Loggers.LogDebug($"storeSelection contains: {item.name}");

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

                if (item.isPurchasePack)
                    continue;

                StartOfRound.Instance.BuyShipUnlockableServerRpc(item.terminalNode.shipUnlockableID, Plugin.instance.Terminal.groupCredits - item.price);
            }
        }

        if (dropshipItems.Count > 0)
        {
            storeSelection.RemoveAll(x => dropshipItems.Contains(x));

            DropShipPurchase(ref dropshipItems);

            storeSelection.AddRange(dropshipItems);
        }

        Plugin.instance.Terminal.StartCoroutine(PurchaseResultPage());
    }

    internal static void VehiclePurchase(StoreInfo item)
    {
        if (Plugin.instance.Terminal.hasWarrantyTicket)
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

        Loggers.LogDebug($"Skipping {skip} items from DropShipPurchase!");

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].isPurchasePack)
            {
                Loggers.WARNING($"PURCHASE PACK {items[i].name} DETECTED IN REGULAR PURCHASE!!");
                continue;
            }

            if (skip > 0 && items.Count - 1 - skip <= i)
            {
                if (items[i].selectionCount > 1 && items[i].selectionCount - skip > 0)
                {
                    items[i].selectionCount = items[i].selectionCount - skip;
                    skip = 0;
                }
                else
                {
                    skip -= items[i].selectionCount;
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

        Loggers.LogDebug($"Full dropship purchase Count: {add.Count}");
        int[] fullpurchase = [.. add];

        Plugin.instance.Terminal.BuyItemsServerRpc(fullpurchase, Plugin.instance.Terminal.groupCredits - cost, fullpurchase.Length + Plugin.instance.Terminal.orderedItemsFromTerminal.Count);
    }

}

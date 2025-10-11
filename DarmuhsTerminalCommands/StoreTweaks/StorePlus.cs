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
using TerminalStuff.SpecialStuff;
using TerminalStuff.Util;
using UnityEngine;
using static TerminalStuff.Patching.AllMyTerminalPatches;
using static TerminalStuff.TerminalEvents;

namespace TerminalStuff.StoreTweaks;

public class StorePlus
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
    internal static StoreMenuItem Current { get; set; } = null!;

    //TerminalNodes
    internal static TerminalNode OriginalStorePage { get; set; } = null!;

    //Misc
    //internal static string LoadExtKey = ""; // 
    internal static int SubTotal = 0; //subtotal before purchase, resets to 0 at launch of menu
    internal static List<StoreInfo> AllStoreItems { get; set; } = [];
    internal static List<StoreInfo> StoreSelection { get; set; } = [];
    internal static List<TerminalNode> ExcludedNodesFromAutoGen { get; set; } = [];
    internal static List<string> ManualUpgradeNames = [];

    public enum StartingPage
    {
        MainMenu,
        Items,
        Upgrades,
        Vehicles,
        Suits,
        Packs,
        Settings
    }

    internal static MenuItem GetStartMenu()
    {
        switch (StorePlusConfig.MenuStartPage.Value)
        {
            case StartingPage.MainMenu:
                return TheMainMenu;
            case StartingPage.Items:
                return Buyables;
            case StartingPage.Upgrades:
                return Upgrades;
            case StartingPage.Vehicles:
                return Vehicles;
            case StartingPage.Suits:
                return Suits;
            case StartingPage.Packs:
                return Packs;
            case StartingPage.Settings:
                return StoreSettings.Settings;
            default:
                break;
        }

        return TheMainMenu;
    }

    internal static void SetToVanilla()
    {
        StorePlusMenu.IsMenuEnabled = false;

        if (OriginalStorePage == null)
            return;

        if (DynamicBools.TryGetKeyword("store", out TerminalKeyword Store))
        {
            Store.specialKeywordResult = OriginalStorePage;
            Loggers.LogDebug("Store keyword set back to original");
        }
        else
            Loggers.ERROR("Store keyword has been deleted!!!");
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
        TheMainMenu.Header = () => "================= Store Plus =================\n\n";
        TheMainMenu.Footer = GetMainFooter;

        //Items
        Buyables.Header = () => "================= Items =================\n\n";
        Buyables.Footer = GetStoreFooter;
        Buyables.SetParentMenu(TheMainMenu);
        Buyables.AdjustNestedMenuList.AddListener(StoreSettings.SortMenu);

        //Upgrades & Furniture
        Upgrades.Header = () => "================= Upgrades =================\n\n";
        Upgrades.Footer = GetStoreFooter;
        Upgrades.SetParentMenu(TheMainMenu);
        Upgrades.AdjustNestedMenuList.AddListener(StoreSettings.SortMenu);

        //Vehicles
        Vehicles.Header = () => "================= Vehicles =================\n\n";
        Vehicles.Footer = GetStoreFooter;
        Vehicles.SetParentMenu(TheMainMenu);
        Vehicles.AdjustNestedMenuList.AddListener(StoreSettings.SortMenu);

        //Suits
        Suits.Header = () => "============= Buyable Suits =============\n\n";
        Suits.Footer = GetStoreFooter;
        Suits.SetParentMenu(TheMainMenu);
        Suits.AdjustNestedMenuList.AddListener(StoreSettings.SortMenu);

        //Purchase Packs
        Packs.Header = () => "============= Purchase Packs =============\n\n";
        Packs.Footer = GetStoreFooter;
        Packs.SetParentMenu(TheMainMenu);
        Packs.AdjustNestedMenuList.AddListener(StoreSettings.SortMenu);

        //Other Mod Menus
        ExternalMods.Header = () => "============= Other Menus =============\n\n";
        ExternalMods.Footer = GetOtherFooter;
        ExternalMods.SetParentMenu(TheMainMenu);

        InitOnce = true;
    }

    internal static string EnterStoreMenu()
    {
        StorePlusMenu.ExitAction = null!;
        StorePlusMenu.EnterAtPage(GetStartMenu());
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

        StoreInfo selection = menuItem.StoreItem;


        int selectedCount = StorePlusMenu.DisplayMenuItemsOfType.OfType<StoreMenuItem>().Where(x => x.StoreItem.Selected).Sum(s => s.StoreItem.SelectionCount) + Plugin.instance.Terminal.orderedItemsFromTerminal.Count;

        if (!selection.Selected)
            return;

        if (selection.maxAllowed != 0 && selection.maxAllowed <= selection.SelectionCount)
            return;

        if (selection.IsVehicle())
            return;

        if (SubTotal + selection.price > Plugin.instance.Terminal.groupCredits - StoreSettings.Savings)
            return;

        if (selection.SelectionCount >= ConfigGetters.GetMaxItems() || selectedCount >= ConfigGetters.GetMaxItems())
            return;

        if (selection.IsPurchasePack)
        {
            if (StorePacks.GetBuyablesCountFromNode(selection.terminalNode) >= ConfigGetters.GetMaxItems())
                return;
        }


        int newNum = selection.SelectionCount + 1;
        SetCount(ref selection, newNum);
    }

    internal static void DecreaseItemCount()
    {
        Loggers.LogDebug("DecreaseItemCount");

        StoreMenuItem menuItem = StorePlusMenu.DisplayMenuItemsOfType.Cast<StoreMenuItem>().ElementAtOrDefault(StorePlusMenu.ActiveSelection);

        if (menuItem == null)
            return;

        StoreInfo selection = menuItem.StoreItem;

        if (!selection.Selected)
            return;

        int newNum = selection.SelectionCount - 1;
        SetCount(ref selection, newNum);
    }

    internal static void SetCount(ref StoreInfo selection, int newCount)
    {
        Loggers.LogDebug($"Original count: {selection.SelectionCount} vs newCount - {newCount}");
        selection.SelectionCount = Mathf.Clamp(newCount, 1, 99);
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

        message.Append($"\n");

        foreach (StoreInfo item in StoreSelection)
        {
            if (item.IsPurchasePack)
            {
                List<string> packList = StorePacks.GetItemListFromNode(item.terminalNode);
                message.Append($"${item.price} [{item.name}] x {item.SelectionCount}\n");
                foreach (string packItem in packList)
                    message.Append($"    {packItem}\n");
                continue;
            }

            message.Append($"${item.price} {item.name} x {item.SelectionCount}\n");
        }

        message.Append($"\n\nTotal Value of Purchase: <color=#e6b800>{SubTotal}</color>");
        message.Append($"\nNew Credits Ammount: {Plugin.instance.Terminal.groupCredits}");

        message.Append("\n\nPress any key to continue...");
        return message.ToString();
    }

    internal static string GetMainFooter()
    {
        string bottomText = StoreSettings.CurrentSortText;

        StringBuilder message = new();
        message.Append($"\n\nCurrent Selection Total: [ <color=#e6b800>${SubTotal}</color> ]\n\n");
        if (bottomText.Length > 0)
            message.Append(bottomText);
        message.Append($"Page [LeftArrow] < {StorePlusMenu.CurrentPage}/{Mathf.CeilToInt((float)StorePlusMenu.DisplayMenuItemsOfType.Count / StorePlusMenu.PageSize)} > [RightArrow]\n");
        message.Append($"Select Store Category: [Enter]\n");
        message.Append($"Leave Menu: [BackSpace]\n\n");
        return message.ToString();
    }

    internal static string GetOtherFooter()
    {
        StringBuilder message = new();
        message.Append($"\n\nCurrent Selection Total: [ <color=#e6b800>${SubTotal}</color> ]\n\n");
        message.Append($"Page [LeftArrow] < {StorePlusMenu.CurrentPage}/{Mathf.CeilToInt((float)StorePlusMenu.DisplayMenuItemsOfType.Count / StorePlusMenu.PageSize)} > [RightArrow]\n");
        message.Append($"Make Selection: [Enter]\n");
        message.Append($"Back Menu: [BackSpace]\n\n");
        return message.ToString();
    }

    internal static string GetStoreFooter()
    {
        string bottomText = StoreSettings.CurrentSortText;

        StringBuilder message = new();
        message.Append($"\n\nCurrent Selection Total: [ <color=#e6b800>${SubTotal}</color> ]\n\n");
        if (bottomText.Length > 0)
            message.Append(bottomText);
        message.Append($"Page [LeftArrow] < {StorePlusMenu.CurrentPage}/{Mathf.CeilToInt((float)StorePlusMenu.DisplayMenuItemsOfType.Count / StorePlusMenu.PageSize)} > [RightArrow]\n");
        message.Append($"Decrease Count [Z] / Increase Count [X]\n");
        message.Append($"Add/Remove Item: [Enter]  / Complete Purchase: [P]\n");
        message.Append($"Back Menu: [BackSpace]\n\n");
        return message.ToString();
    }

    internal static int GetProjectedCredits()
    {
        return Plugin.instance.Terminal.groupCredits - SubTotal - StoreSettings.Savings;
    }

    internal static void UpdateSubtotal()
    {
        Loggers.LogDebug("UpdateSubtotal!");
        int sub = 0;

        foreach (StoreInfo item in StoreSelection)
        {
            sub += item.price * item.SelectionCount;
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

            if (ExcludedNodesFromAutoGen.Contains(node))
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

        if (!StorePlusConfig.StorePlusKeywords.Value.Contains("store", System.StringComparison.InvariantCultureIgnoreCase))
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
        AllStoreItems.DoIf(x => x.Selected || x.SelectionCount > 1, x => x.Reset());
        StorePlusMenu.AcceptAnything = false;
        StorePlusMenu.CurrentPage = 1;
        SubTotal = 0;
        StoreSelection = [];
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
        if (item.IsPurchasePack)
        {
            Loggers.LogDebug($"Purchase Pack detected @ SortStoreItem: {item.name}");
            item.ThisMenuItem.SetParentMenu(Packs);
            return;
        }

        if (item.IsSuit() && item.name.Length > 1)
        {
            Loggers.LogDebug($"Buyable Suit detected @ SortStoreItem: {item.name}");
            item.ThisMenuItem.SetParentMenu(Suits);
            return;
        }

        if (item.IsVehicle())
        {
            Loggers.LogDebug($"Buyable Vehicle detected @ SortStoreItem: {item.name}");
            item.ThisMenuItem.SetParentMenu(Vehicles);
            return;
        }

        if (!item.WaitForDelivery)
        {
            Loggers.LogDebug($"Upgrade detected @ SortStoreItem: {item.name}");
            item.ThisMenuItem.SetParentMenu(Upgrades);
            return;
        }

        Loggers.LogDebug($"Buyable Item detected @ SortStoreItem: {item.name}");

        item.ThisMenuItem.SetParentMenu(Buyables);

    }

    internal static void CompletePurchase()
    {
        if (StorePlusMenu.AcceptAnything)
            return;

        if (StoreSelection.Count == 0)
        {
            Plugin.Log.LogMessage("No items selected to purchase!");
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
            return;
        }

        //this should never happen but just in case lol
        if (Plugin.instance.Terminal.groupCredits - StoreSettings.Savings < SubTotal)
        {
            Plugin.Log.LogMessage("Not enough credits!");
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
            return;
        }


        List<StoreInfo> dropshipItems = [];

        foreach (StoreInfo item in StoreSelection)
        {
            Loggers.LogDebug($"storeSelection contains: {item.name}");

            if (item.IsPurchasePack)
                StorePacks.CompletePurchaseV2(item.terminalNode, item.SelectionCount);
            else if (item.IsVehicle())
                VehiclePurchase(item);
            else if (item.WaitForDelivery && !item.IsPurchasePack) //deliverable items and vehicles
                dropshipItems.Add(item);
            else
            {
                //unlockables
                if (item.terminalNode.shipUnlockableID < 0 || item.terminalNode.shipUnlockableID > StartOfRound.Instance.unlockablesList.unlockables.Count)
                    continue;

                if (item.IsPurchasePack)
                    continue;

                StartOfRound.Instance.BuyShipUnlockableServerRpc(item.terminalNode.shipUnlockableID, Plugin.instance.Terminal.groupCredits - item.price);
            }
        }

        if (dropshipItems.Count > 0)
        {
            StoreSelection.RemoveAll(x => dropshipItems.Contains(x));

            DropShipPurchase(ref dropshipItems);

            StoreSelection.AddRange(dropshipItems);
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
        int fullItemsCount = items.Sum(x => x.SelectionCount);
        int skip = 0;

        if (fullItemsCount + add.Count > ConfigGetters.GetMaxItems())
            skip = fullItemsCount + add.Count - ConfigGetters.GetMaxItems();

        Loggers.LogDebug($"Skipping {skip} items from DropShipPurchase!");

        for (int i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].IsPurchasePack)
            {
                Loggers.WARNING($"PURCHASE PACK {items[i].name} DETECTED IN REGULAR PURCHASE!!");
                continue;
            }

            if (skip > 0 && items.Count - 1 - skip <= i)
            {
                if (items[i].SelectionCount > 1 && items[i].SelectionCount - skip > 0)
                {
                    items[i].SelectionCount = items[i].SelectionCount - skip;
                    skip = 0;
                }
                else
                {
                    skip -= items[i].SelectionCount;
                    items.Remove(items[i]);
                    continue;
                }
            }

            cost += items[i].price * items[i].SelectionCount;

            if (items[i].SelectionCount > 1)
            {
                for (int s = 0; s < items[i].SelectionCount; s++)
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

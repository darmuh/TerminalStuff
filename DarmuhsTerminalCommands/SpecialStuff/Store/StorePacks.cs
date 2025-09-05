using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;
using static TerminalStuff.AllMyTerminalPatches;
using static TerminalStuff.SpecialStuff.StorePacksInfo;


namespace TerminalStuff.SpecialStuff;

internal class StorePacks
{
    internal string Name = "";
    internal string configValue = "";
    internal List<string> ContentsList = [];
    internal TerminalNode terminalNode;

    internal string Contents = "";

    //used in funcs
    internal List<TerminalNode> UpgradeItems = [];
    internal Dictionary<Item, int> ItemsToPurchase = [];

    public override string ToString()
    {
        return Name;
    }

    internal StorePacks(string name, string value, TerminalNode node)
    {
        Name = name;
        configValue = value;
        terminalNode = node;
        terminalNode.creatureName = Name;
        GetPurchasePackContents();
        Loggers.LogDebug($"PurchasePack {Name} created with {ItemsToPurchase.Count} unique items and {UpgradeItems.Count} Upgrades/Furniture");
        AllPacks.Add(this);

        if (Commands.TerminalStorePlus.Value)
            AddToStorePlus();
    }

    internal void UpdateExisting(string value, TerminalNode node)
    {
        configValue = value;
        terminalNode = node;
        GetPurchasePackContents();
        Loggers.LogDebug($"PurchasePack {Name} updated with {ItemsToPurchase.Count} unique items and {UpgradeItems.Count} Upgrades/Furniture");

        if (Commands.TerminalStorePlus.Value)
            AddToStorePlus();
    }
    internal void AddToStorePlus()
    {
        terminalNode.creatureName = Name;
        if (!StorePlus.ManualUpgradeNames.Contains(Name))
            StorePlus.ManualUpgradeNames.Add(Name);
        StoreInfo item = null!;

        if (StorePlus.Packs.NestedMenus.Count > 0)
        {
            StoreMenuItem existing = StorePlus.Packs.NestedMenus.Cast<StoreMenuItem>().FirstOrDefault(x => x.Name == Name && x.storeItem.isPurchasePack);
            if (existing != null)
                item = existing.storeItem;
        }

        if (item != null)
        {
            Loggers.LogDebug("Updating purchasepack StorePlus item!");
            item.UpdateNode(terminalNode);
        }
        else
        {
            item = new(terminalNode)
            {
                isPurchasePack = true,
                name = Name
            };
            item.menuItem.SetParentMenu(StorePlus.Packs);
        }

    }

    internal static string AskPurchasePack()
    {
        if (Selected == null)
            return "Purchase pack has FAILED at AskPurchasePack due to a null selection!\r\n";

        Selected.GetContents();
        StringBuilder packAsk = new();
        packAsk.AppendLine($"Would you like to purchase the [{CurrentPackName}] PurchasePack?\r\n\r\n\tContents:\r\n");
        packAsk.Append(Selected.Contents);
        int totalCost = Selected.GetTotalCost(out int itemCount); //items

        if (totalCost <= Plugin.instance.Terminal.groupCredits)
        {
            packAsk.AppendLine($"\r\n\tTotal Cost: ■{totalCost}({itemCount} items)\r\n\r\nPlease CONFIRM or DENY.\n");
            return packAsk.ToString();
        }
        else
        {
            Loggers.LogInfo("not enough credits to purchase, sending to cannot afford display");
            TerminalGeneral.CancelConfirmation = true;
            return $"You cannot afford the {CurrentPackName} PurchasePack ({itemCount} items).\r\n\r\n\tTotal Cost: ■<color=#BD3131>{totalCost}</color>\r\n\r\n";
        }
    }

    internal static int GetPriceFromNode(TerminalNode node)
    {
        int price = 0;
        StorePacks thisPack = AllPacks.FirstOrDefault(x => x.terminalNode == node);

        if (thisPack == null)
            return price;

        price = thisPack.GetTotalCost(out int itemCount);

        Loggers.LogDebug($"Got price for {thisPack.Name} of ${price} from {itemCount} items");
        return price;
    }

    internal static int GetBuyablesCountFromNode(TerminalNode node)
    {
        int count = 0;
        StorePacks thisPack = AllPacks.FirstOrDefault(x => x.terminalNode == node);

        if (thisPack == null)
            return count;

        count = thisPack.ItemsToPurchase.Sum(x => x.Value);
        Loggers.LogDebug($"Got buyables count for {thisPack.Name} of {count} from terminalnode");
        return count;
    }

    internal static string CompletePurchasePack()
    {
        if (Selected == null)
            return "Purchase pack has FAILED at AskPurchasePack due to a null selection!\r\n";

        Selected.GetContents();

        StringBuilder packBuy = new();
        packBuy.AppendLine($"You have purchased the {CurrentPackName} PurchasePack!\r\n\r\n\tContents:\r\n");
        packBuy.Append(Selected.Contents);
        int totalCost = Selected.GetTotalCost(out int itemCount);

        if (totalCost > Plugin.instance.Terminal.groupCredits)
        {
            Loggers.LogInfo("not enough credits to purchase, sending to error message");
            return Plugin.instance.Terminal.terminalNodes.specialNodes[5].displayText;
        }

        BuyAll(totalCost, itemCount);

        packBuy.AppendLine($"\r\n\r\nYour new balance is ■{Plugin.instance.Terminal.groupCredits} credits\r\n\r\n\tEnjoy!\r\n");
        Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
        return packBuy.ToString();
    }

    private static void BuyAll(int totalCost, int itemCount)
    {
        Loggers.LogInfo($"BuyAll: totalCost - {totalCost} itemCount - {itemCount}");

        int maxItems = ConfigGetters.GetMaxItems();

        int[] fullItemList = Selected.BuyItems();
        if (fullItemList.Length + Plugin.instance.Terminal.orderedItemsFromTerminal.Count > maxItems)
            fullItemList = [.. fullItemList.Skip(fullItemList.Length + Plugin.instance.Terminal.orderedItemsFromTerminal.Count - maxItems)];

        Plugin.instance.Terminal.BuyItemsServerRpc(fullItemList, Plugin.instance.Terminal.groupCredits - totalCost, fullItemList.Length);

        Loggers.LogDebug($"orderedItems update: {Plugin.instance.Terminal.orderedItemsFromTerminal.Count}");

        if (Selected.UpgradeItems.Count > 0)
        {
            foreach (TerminalNode item in Selected.UpgradeItems)
            {
                StartOfRound.Instance.BuyShipUnlockableServerRpc(item.shipUnlockableID, Plugin.instance.Terminal.groupCredits);
                Loggers.LogInfo($"Unlocking {item.creatureName}");
            }

        }
    }

    internal static List<string> GetItemListFromNode(TerminalNode node)
    {
        Selected = AllPacks.FirstOrDefault(x => x.terminalNode == node);

        if (Selected == null)
            return [];

        return Selected.ContentsList;
    }

    internal static void CompletePurchaseV2(TerminalNode node, int count)
    {
        Selected = AllPacks.FirstOrDefault(x => x.terminalNode == node);
        if (Selected == null)
        {
            Loggers.WARNING("CompletePurchaseV2 failed to parse given node for purchase!");
            return;
        }

        Selected.GetContents();
        int totalCost = Selected.GetTotalCost(out int itemCount);

        if (totalCost > Plugin.instance.Terminal.groupCredits)
        {
            Loggers.WARNING("Not enough credits for totalCost of purchase pack during CompletePurchaseV2 (this should have been checked for already)");
            return;
        }

        for (int i = 0; i < count; i++)
            BuyAll(totalCost, itemCount);
    }

    private int[] BuyItems()
    {
        List<int> thisOrder = [];
        List<Item> buyables = [.. Plugin.instance.Terminal.buyableItemsList];


        foreach (KeyValuePair<Item, int> pair in ItemsToPurchase)
        {
            int count = pair.Value;
            int index = buyables.IndexOf(pair.Key);
            for (int i = 0; i < count; i++)
            {
                thisOrder.Add(index);
                Loggers.LogDebug($"Adding {pair.Key.itemName} to order list ({i})");
            }
        }

        return [.. thisOrder];
    }

    private int GetTotalCost(out int itemCount)
    {
        int totalCost = 0;
        itemCount = 0;

        //buyableItem = Plugin.instance.Terminal.buyableItemsList[storeNode.buyItemIndex];

        if (ItemsToPurchase.Count > 0)
        {
            foreach (KeyValuePair<Item, int> item in ItemsToPurchase)
            {
                int itemID = Plugin.instance.Terminal.buyableItemsList.ToList().IndexOf(item.Key);
                int purchaseCount = ItemsToPurchase[item.Key];
                int itemCost = StorePlus.GetSalesPrice(item.Key.creditsWorth, itemID);
                itemCost *= purchaseCount;
                totalCost += itemCost;
                itemCount += purchaseCount;
                Loggers.LogDebug($"Added {itemCost} to total: {totalCost} (total item count: {itemCount})");
            }
        }

        if (UpgradeItems.Count > 0)
        {
            UpdateUnlockStatus();
            int upgradesCost = UpgradeItems.Sum(upgrade => upgrade.itemCost); //upgrades
            Loggers.LogDebug($"Adding {upgradesCost} to {totalCost}");
            totalCost += upgradesCost;
        }

        Loggers.LogInfo($"Total Cost of {Name}: [{totalCost}]");
        return totalCost;
    }

    private void UpdateUnlockStatus()
    {
        if (UpgradeItems.Count == 0)
            return;

        Loggers.LogDebug($"{Name} UpdateUnlockStatus: original ContentsList count = {ContentsList.Count}");
        List<TerminalNode> Unlocked = UpgradeItems.FindAll(x => !IsUnlockableBuyable(x.shipUnlockableID));
        ContentsList.RemoveAll(x => Unlocked.Any(c => OpenLib.Common.Misc.CompareStringsInvariant(c.creatureName, x)));
        UpgradeItems.RemoveAll(x => Unlocked.Count != 0);
        Loggers.LogDebug($"{Name} Removed all unlocked upgrades, new ContentsList count = {ContentsList.Count}");
    }

    private void GetPurchasePackContents()
    {
        List<string> itemList = OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(configValue, ',');
        List<string> itemNames = [];

        foreach (string item in itemList)
        {
            if (TryGetItemToBuy(item, out Item itemValue))
            {
                Loggers.LogDebug($"{item} is a valid item, adding to pack purchase");
                int count = GetNameCount(itemList, item);
                if (count > 1 && !itemNames.Contains(itemValue.itemName))
                    ContentsList.Add($"{itemValue.itemName} x {count}");
                else if (count <= 1)
                    ContentsList.Add($"{itemValue.itemName}");

                if (!itemNames.Contains(itemValue.itemName))
                    itemNames.Add(itemValue.itemName);
                ItemsToPurchase.TryAdd(itemValue, count);
            }
            else if (TryGetUpgrade(item, out TerminalNode upgradeItem))
            {
                if (IsUnlockableBuyable(upgradeItem.shipUnlockableID))
                {
                    if (!ContentsList.Contains($"{upgradeItem.creatureName}"))
                        ContentsList.Add($"{upgradeItem.creatureName}");
                    if (!UpgradeItems.Contains(upgradeItem))
                        UpgradeItems.Add(upgradeItem);
                    Loggers.LogDebug($"Added {upgradeItem.creatureName} to ContentList & UpgradeItems Lists");
                }
            }
        }
    }

    private string GetContents()
    {
        UpdateUnlockStatus();

        StringBuilder result = new();
        foreach (string line in Selected.ContentsList)
            result.AppendLine(line);

        return result.ToString();
    }

    private static bool TryGetItemToBuy(string itemName, out Item itemValue)
    {
        List<Item> buyables = [.. Plugin.instance.Terminal.buyableItemsList];
        itemValue = buyables.FirstOrDefault(x => OpenLib.Common.Misc.StringContainsInvariant(x.itemName, itemName));

        if (itemValue == null)
            return false;

        int itemID = Plugin.instance.Terminal.buyableItemsList.ToList().IndexOf(itemValue);

        Loggers.LogDebug($"{itemValue.itemName} found matching to {itemName} at index: [{itemID}]");
        return true;
    }

    private static bool TryGetUpgrade(string upgradeName, out TerminalNode UpgradeNode)
    {
        Loggers.LogDebug($"TryGetUpgrade from {upgradeName}");
        Loggers.LogDebug("Getting all nodes");
        Plugin.refreshNodes = true;
        List<TerminalNode> allNodes = Plugin.Allnodes;
        allNodes.RemoveAll(x => x.creatureName == null);
        Loggers.LogDebug($"iterating through allNodes {allNodes.Count}");

        UpgradeNode = allNodes.FirstOrDefault(node => OpenLib.Common.Misc.StringStartsWithInvariant(node.creatureName, upgradeName));

        if (UpgradeNode == null)
        {
            Loggers.WARNING($"TryGetUpgrade by name {upgradeName} could not be found!");
            return false;
        }

        if (UpgradeNode.shipUnlockableID < 0)
        {
            Loggers.WARNING($"TryGetUpgrade name {upgradeName} has invalid shipUnlockableID!");
            return false;
        }

        if (!IsUnlockableBuyable(UpgradeNode.shipUnlockableID))
        {
            Loggers.LogDebug($"TryGetUpgrade detected {upgradeName} as already unlocked!");
            return false;
        }


        Loggers.LogDebug($"unlockableID: {UpgradeNode.shipUnlockableID}");
        Loggers.LogDebug($"creatureName: {UpgradeNode.creatureName} matching {upgradeName}");
        return true;
    }

    private static int GetNameCount(List<string> itemList, string itemName)
    {
        int count = itemList.FindAll(i => i == itemName).Count;
        Loggers.LogInfo($"Final Count: {itemName} - {count}");
        return count;
    }

    internal static bool IsUnlockableBuyable(int itemID)
    {
        if (itemID >= StartOfRound.Instance.unlockablesList.unlockables.Count)
            return false;

        UnlockableItem item = StartOfRound.Instance.unlockablesList.unlockables[itemID];
        if (!item.alreadyUnlocked && !item.hasBeenUnlockedByPlayer)
        {
            Loggers.LogDebug($"Upgrade ID: {itemID} has not been unlocked. Setting variable to true");
            return true;
        }

        return false;
    }
}

internal static class StorePacksInfo
{
    internal static string CurrentPackName = "";
    internal static StorePacks Selected = null!;
    //internal static string PackContents = "";
    internal static List<StorePacks> AllPacks = [];

    internal static void CancelConfirmation()
    {
        CurrentPackName = "";
        Selected = null!;
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using static TerminalStuff.AllMyTerminalPatches;
using static TerminalStuff.SpecialStuff.StorePacksInfo;


namespace TerminalStuff.SpecialStuff
{
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
            Plugin.Spam($"PurchasePack {Name} created with {ItemsToPurchase.Count} unique items and {UpgradeItems.Count} Upgrades/Furniture");
            AllPacks.Add(this);

            if (Commands.TerminalStorePlus.Value)
                AddToStorePlus();
        }

        internal void UpdateExisting(string value, TerminalNode node)
        {
            configValue = value;
            terminalNode = node;
            GetPurchasePackContents();
            Plugin.Spam($"PurchasePack {Name} updated with {ItemsToPurchase.Count} unique items and {UpgradeItems.Count} Upgrades/Furniture");

            if (Commands.TerminalStorePlus.Value)
                AddToStorePlus();
        }
        internal void AddToStorePlus()
        {
            terminalNode.creatureName = Name;
            if(!StorePlus.ManualUpgradeNames.Contains(Name))
                StorePlus.ManualUpgradeNames.Add(Name);
            StoreInfo item = null!;
            
            if(StorePlus.Packs.NestedMenus.Count > 0)
            {
                StoreMenuItem existing = StorePlus.Packs.NestedMenus.Cast<StoreMenuItem>().FirstOrDefault(x => x.Name == Name && x.storeItem.isPurchasePack);
                if (existing != null)
                    item = existing.storeItem;
            }

            if (item != null)
            {
                Plugin.Spam("Updating purchasepack StorePlus item!");
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
                Plugin.MoreLogs("not enough credits to purchase, sending to cannot afford display");
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

            Plugin.Spam($"Got price for {thisPack.Name} of ${price} from {itemCount} items");
            return price;
        }

        internal static int GetBuyablesCountFromNode(TerminalNode node)
        {
            int count = 0;
            StorePacks thisPack = AllPacks.FirstOrDefault(x => x.terminalNode == node);

            if (thisPack == null)
                return count;

            count = thisPack.ItemsToPurchase.Sum(x => x.Value);
            Plugin.Spam($"Got buyables count for {thisPack.Name} of {count} from terminalnode");
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
                Plugin.MoreLogs("not enough credits to purchase, sending to error message");
                return Plugin.instance.Terminal.terminalNodes.specialNodes[5].displayText;
            }

            Selected.BuyAll(totalCost, itemCount);

            packBuy.AppendLine($"\r\n\r\nYour new balance is ■{Plugin.instance.Terminal.groupCredits} credits\r\n\r\n\tEnjoy!\r\n");
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
            return packBuy.ToString();
        }

        private void BuyAll(int totalCost, int itemCount)
        {
            Plugin.MoreLogs($"BuyAll: totalCost - {totalCost} itemCount - {itemCount}");

            int maxItems = ConfigGetters.GetMaxItems();

            int[] fullItemList = Selected.BuyItems();
            if (fullItemList.Length + Plugin.instance.Terminal.orderedItemsFromTerminal.Count > maxItems)
                fullItemList = [.. fullItemList.Skip(fullItemList.Length + Plugin.instance.Terminal.orderedItemsFromTerminal.Count - maxItems)];

            Plugin.instance.Terminal.BuyItemsServerRpc(fullItemList, Plugin.instance.Terminal.groupCredits - totalCost, fullItemList.Length);

            Plugin.Spam($"orderedItems update: {Plugin.instance.Terminal.orderedItemsFromTerminal.Count}");

            if (Selected.UpgradeItems.Count > 0)
            {
                foreach (TerminalNode item in Selected.UpgradeItems)
                {
                    StartOfRound.Instance.BuyShipUnlockableServerRpc(item.shipUnlockableID, Plugin.instance.Terminal.groupCredits);
                    Plugin.MoreLogs($"Unlocking {item.creatureName}");
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
                Plugin.WARNING("CompletePurchaseV2 failed to parse given node for purchase!");
                return;
            }

            Selected.GetContents();
            int totalCost = Selected.GetTotalCost(out int itemCount);

            if (totalCost > Plugin.instance.Terminal.groupCredits)
            {
                Plugin.WARNING("Not enough credits for totalCost of purchase pack during CompletePurchaseV2 (this should have been checked for already)");
                return;
            }

            for(int i =0; i < count; i++)
                Selected.BuyAll(totalCost, itemCount);
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
                    Plugin.Spam($"Adding {pair.Key.itemName} to order list ({i})");
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
                    Plugin.Spam($"Added {itemCost} to total: {totalCost} (total item count: {itemCount})");
                }
            }

            if (UpgradeItems.Count > 0)
            {
                UpdateUnlockStatus();
                int upgradesCost = UpgradeItems.Sum(upgrade => upgrade.itemCost); //upgrades
                Plugin.Spam($"Adding {upgradesCost} to {totalCost}");
                totalCost += upgradesCost;
            }

            Plugin.MoreLogs($"Total Cost of {Name}: [{totalCost}]");
            return totalCost;
        }

        private void UpdateUnlockStatus()
        {
            if (UpgradeItems.Count == 0)
                return;

            Plugin.Spam($"{Name} UpdateUnlockStatus: original ContentsList count = {ContentsList.Count}");
            List<TerminalNode> Unlocked = UpgradeItems.FindAll(x => !IsUnlockableBuyable(x.shipUnlockableID));
            ContentsList.RemoveAll(x => Unlocked.Any(c => c.creatureName.ToLower() == x.ToLower()));
            UpgradeItems.RemoveAll(x => Unlocked.Any());
            Plugin.Spam($"{Name} Removed all unlocked upgrades, new ContentsList count = {ContentsList.Count}");
        }

        private void GetPurchasePackContents()
        {
            List<string> itemList = OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(configValue, ',');
            List<string> itemNames = [];

            foreach (string item in itemList)
            {
                if (TryGetItemToBuy(item, out Item itemValue))
                {
                    Plugin.Spam($"{item} is a valid item, adding to pack purchase");
                    int count = GetNameCount(itemList, item);
                    if (count > 1 && !itemNames.Contains(itemValue.itemName))
                        ContentsList.Add($"{itemValue.itemName} x {count}");
                    else if (count <= 1)
                        ContentsList.Add($"{itemValue.itemName}");

                    if (!itemNames.Contains(itemValue.itemName))
                        itemNames.Add(itemValue.itemName);
                    if (!ItemsToPurchase.ContainsKey(itemValue))
                        ItemsToPurchase.Add(itemValue, count);
                }
                else if (TryGetUpgrade(item, out TerminalNode upgradeItem))
                {
                    if (IsUnlockableBuyable(upgradeItem.shipUnlockableID))
                    {
                        if(!ContentsList.Contains($"{upgradeItem.creatureName}"))
                            ContentsList.Add($"{upgradeItem.creatureName}");
                        if(!UpgradeItems.Contains(upgradeItem))
                            UpgradeItems.Add(upgradeItem);
                        Plugin.Spam($"Added {upgradeItem.creatureName} to ContentList & UpgradeItems Lists");
                    }
                }
            }
        }

        private string GetContents()
        {
            UpdateUnlockStatus();

            StringBuilder result = new();
            foreach(string line in Selected.ContentsList)
                result.AppendLine(line);

            return result.ToString();
        }

        private bool TryGetItemToBuy(string itemName, out Item itemValue)
        {
            List<Item> buyables = [.. Plugin.instance.Terminal.buyableItemsList];
            itemValue = buyables.FirstOrDefault(x => x.itemName.ToLower().Contains(itemName));

            if (itemValue == null)
                return false;

            int itemID = Plugin.instance.Terminal.buyableItemsList.ToList().IndexOf(itemValue);

            Plugin.Spam($"{itemValue.itemName} found matching to {itemName} at index: [{itemID}]");
            return true;
        }

        private bool TryGetUpgrade(string upgradeName, out TerminalNode UpgradeNode)
        {
            Plugin.Spam($"TryGetUpgrade from {upgradeName}");
            Plugin.Spam("Getting all nodes");
            Plugin.refreshNodes = true;
            List<TerminalNode> allNodes = Plugin.Allnodes;
            allNodes.RemoveAll(x => x.creatureName == null);
            Plugin.Spam($"iterating through allNodes {allNodes.Count}");

            UpgradeNode = allNodes.FirstOrDefault(node => node.creatureName.ToLower().StartsWith(upgradeName.ToLower()));

            if(UpgradeNode == null)
            {
                Plugin.WARNING($"TryGetUpgrade by name {upgradeName} could not be found!");
                return false;
            }    

            if(UpgradeNode.shipUnlockableID < 0)
            {
                Plugin.WARNING($"TryGetUpgrade name {upgradeName} has invalid shipUnlockableID!");
                return false;
            }

            if(!IsUnlockableBuyable(UpgradeNode.shipUnlockableID))
            {
                Plugin.Spam($"TryGetUpgrade detected {upgradeName} as already unlocked!");
                return false;
            }
                

            Plugin.Spam($"unlockableID: {UpgradeNode.shipUnlockableID}");
            Plugin.Spam($"creatureName: {UpgradeNode.creatureName} matching {upgradeName}");
            return true;
        }

        private int GetNameCount(List<string> itemList, string itemName)
        {
            int count = itemList.FindAll(i => i == itemName).Count;
            Plugin.MoreLogs($"Final Count: {itemName} - {count}");
            return count;
        }

        internal static bool IsUnlockableBuyable(int itemID)
        {
            if (itemID >= StartOfRound.Instance.unlockablesList.unlockables.Count)
                return false;

            UnlockableItem item = StartOfRound.Instance.unlockablesList.unlockables[itemID];
            if (!item.alreadyUnlocked && !item.hasBeenUnlockedByPlayer)
            {
                Plugin.Spam($"Upgrade ID: {itemID} has not been unlocked. Setting variable to true");
                return true;
            }

            return false;
        }
    }

    internal static class StorePacksInfo
    {
        internal static string CurrentPackName = "";
        internal static StorePacks Selected = null;
        //internal static string PackContents = "";
        internal static List<StorePacks> AllPacks = [];

        internal static void CancelConfirmation()
        {
            CurrentPackName = "";
            Selected = null!;
        }
    }
}

using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TerminalStuff.EventSub;
using TerminalStuff.PluginCore;
using UnityEngine;

namespace TerminalStuff
{
    internal class CostCommands
    {
        internal static bool vitalsUpgradeEnabled = false;
        internal static bool enemyScanUpgradeEnabled = false;
        //List<int> items 
        internal static List<int> storeCart = [];
        internal static string currentPackList;
        internal static string currentPackName;
        internal static string buyPackName;
        internal static Dictionary<Item, int> itemsIndexed = [];

        internal static bool CheckUnlockableStatus(string itemName)
        {
            foreach (UnlockableItem item in StartOfRound.Instance.unlockablesList.unlockables)
            {
                //Plugin.MoreLogs($"Checking {itemName}");
                if (item.unlockableName == itemName)
                {
                    if (item.alreadyUnlocked || item.hasBeenUnlockedByPlayer)
                    {
                        Plugin.Spam($"Upgrade: {itemName} already unlocked. Setting variable to true");
                        return true;
                    }
                }
            }

            Plugin.Spam($"Upgrade: {itemName} is NOT unlocked already");
            return false;
        }

        internal static bool CheckUnlockableStatus(int itemID)
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

        internal static void UpdateUnlockStatus()
        {
            enemyScanUpgradeEnabled = false;
            vitalsUpgradeEnabled = false;

            foreach(string name in SaveManager.AllUpgradesUnlocked)
            {
                Plugin.Spam($"Updating upgrade status for {name}");

                if (name == "BioscanPatch")
                    enemyScanUpgradeEnabled = true;
                else if (name == "VitalsPatch")
                    vitalsUpgradeEnabled = true;
                else
                    Plugin.WARNING($"Unexpected upgrade unlock name [ {name} ]");
            }

        }

        internal static string BioscanCommand()
        {
            string displayText;

            if (RoundManager.Instance != null)
            {
                int scannedEnemies = RoundManager.Instance.SpawnedEnemies.Count;
                int getCreds = Plugin.instance.Terminal.groupCredits;
                int costCreds = ConfigSettings.BioScanCost.Value;

                if (ShouldRunBioscan2(getCreds, costCreds)) //upgraded bioscan
                {
                    int newCreds = CalculateNewCredits(getCreds, costCreds, Plugin.instance.Terminal);

                    List<EnemyAI> livingEnemies = GetLivingEnemiesList();
                    string filteredLivingEnemiesString = FilterLivingEnemies(livingEnemies);

                    string bioscanResult = GetBioscanResult(scannedEnemies, costCreds, newCreds, filteredLivingEnemiesString);
                    displayText = bioscanResult;
                    Plugin.MoreLogs($"Living Enemies(filtered): {filteredLivingEnemiesString}");
                    return displayText;
                }
                else if (getCreds >= costCreds) //nonupgraded
                {
                    int newCreds = CalculateNewCredits(getCreds, costCreds, Plugin.instance.Terminal);
                    string bioscanResult = GetBasicBioscanResult(scannedEnemies, costCreds, newCreds);
                    displayText = bioscanResult;
                    Plugin.MoreLogs("v1 scanner utilized, only numbers shown");
                    return displayText;
                }
                else
                {
                    displayText = "Not enough credits to run Biomatter Scanner.\r\n";
                    Plugin.MoreLogs("brokeboy detected");
                    return displayText;
                }
            }
            else
            {
                displayText = "Cannot scan for Biomatter at this time.\r\n";
                return displayText;
            }
        }

        private static bool ShouldRunBioscan2(int getCreds, int costCreds)
        {
            return enemyScanUpgradeEnabled && getCreds >= costCreds;
        }

        private static string GetBasicBioscanResult(int scannedEnemies, int costCreds, int newCreds)
        {
            return $"Biomatter scanner charged {costCreds} credits and has detected [{scannedEnemies}] non-employee organic objects.\r\n\r\nYour new balance is ■{newCreds} Credits.\r\n";
        }

        private static List<EnemyAI> GetLivingEnemiesList()
        {
            return RoundManager.Instance.SpawnedEnemies.Where(enemy => !enemy.isEnemyDead).ToList();
        }

        private static string FilterLivingEnemies(List<EnemyAI> livingEnemies)
        {
            string livingEnemiesString = string.Join(Environment.NewLine, livingEnemies.Select(enemy => enemy.ToString()));
            string pattern = @"\([^)]*\)";
            return Regex.Replace(livingEnemiesString, pattern, string.Empty);
        }
        private static string GetBioscanResult(int scannedEnemies, int costCreds, int newCreds, string filteredLivingEnemiesString)
        {
            string bioscanResult = $"Biomatter scanner charged {costCreds} credits and has detected [{scannedEnemies}] non-employee organic objects.\r\n\r\n";

            if (!string.IsNullOrEmpty(filteredLivingEnemiesString))
            {
                bioscanResult += $"Your new balance is ■{newCreds} Credits.\r\n\r\nDetailed scan has defined these objects as the following in the registry: \r\n{filteredLivingEnemiesString}\r\n";
            }
            else
            {
                bioscanResult += $"Your new balance is ■{newCreds} Credits.\r\n";
                Plugin.MoreLogs("v1 scanner utilized, only numbers shown");
            }

            return bioscanResult;
        }
        internal static string VitalsCommand()
        {
            string displayText;
            PlayerControllerB getPlayerInfo = GameStuff.TerminalMapRenderer.targetedPlayer;

            if (getPlayerInfo == null)
            {
                displayText = $"Vitals command malfunctioning...\n\n";
                return displayText;
            }

            int getCreds = Plugin.instance.Terminal.groupCredits;
            int costCreds = GetCostCreds(vitalsUpgradeEnabled);

            string playername = getPlayerInfo.playerUsername;

            Plugin.MoreLogs("playername: " + playername);

            if (ShouldDisplayVitals(getPlayerInfo, getCreds, costCreds))
            {
                int newCreds = CalculateNewCredits(getCreds, costCreds, Plugin.instance.Terminal);

                string vitalsInfo = GetVitalsInfo(getPlayerInfo);
                string creditsInfo = GetCreditsInfo(newCreds);

                if (!vitalsUpgradeEnabled)
                {
                    displayText = $"Charged ■{costCreds} Credits. \n{vitalsInfo}\n{creditsInfo}";
                    return displayText;
                }
                else
                {
                    displayText = $"{vitalsInfo}\n{creditsInfo}";
                    return displayText;
                }
            }
            else
            {
                displayText = $"{ConfigSettings.VitalsPoorString.Value}\n";
                return displayText;
            }
        }
        internal static int GetCostCreds(bool upgradeStatus)
        {
            if (!upgradeStatus)
            {
                return ConfigSettings.VitalsCost.Value;
            }
            else
            {
                return 0;
            }
        }
        internal static string AskBioscanUpgrade()
        {
            if (enemyScanUpgradeEnabled == false)
            {
                string patchASK = $"Purchase the BioScanner 2.0 Upgrade Patch?\nThis software update is available for {ConfigSettings.BioScanUpgradeCost.Value} Credits.\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
                return patchASK;
            }
            else
            {
                TerminalGeneral.CancelConfirmation = true;
                string displayText = $"BioScanner software has already been updated to the latest patch (2.0).\r\n\r\n";
                return displayText;
            }
        }
        internal static string AskVitalsUpgrade()
        {
            if (vitalsUpgradeEnabled == false)
            {
                string patchASK = $"Purchase the Vitals Scanner 2.0 Patch?\nThis software update is available for {ConfigSettings.VitalsUpgradeCost.Value} Credits.\n\n\n\n\n\n\n\n\n\n\nPlease CONFIRM or DENY.\n";
                return patchASK;
            }
            else
            {
                TerminalGeneral.CancelConfirmation = true;
                string displayText = $"Vitals Scanner software has already been updated to the latest patch (2.0).\r\n\r\n";
                return displayText;
            }
        }

        private static int GetIndexNum(Item givenItem)
        {
            int index = 0;
            foreach (Item item in Plugin.instance.Terminal.buyableItemsList)
            {
                if (givenItem == item)
                    return index;
                else
                    index++;
            }

            return -1;
        }

        private static bool TryGetItemToBuy(string itemName, out Item itemValue)
        {
            int indexNum = 0;
            foreach (Item item in Plugin.instance.Terminal.buyableItemsList)
            {
                if (item.itemName.ToLower().Contains(itemName))
                {
                    Plugin.Spam($"{item.itemName} found matching to {itemName} at index: [{indexNum}]");
                    if (itemsIndexed.ContainsKey(item))
                        Plugin.Spam($"{itemName} already indexed");
                    else
                    {
                        itemsIndexed.Add(item, indexNum);
                        Plugin.Spam($"indexed item: {itemName}");
                    }

                    itemValue = item;
                    return true;
                }
                Plugin.Spam($"{item.itemName} at index [{indexNum}] does not match config item {itemName}");
                indexNum++;
            }

            itemValue = null;
            return false;
        }

        private static int GetTotalCost(Dictionary<Item, int> itemsToPurchase, Dictionary<Item, int> itemsIndexed, out int itemCount)
        {
            int totalCost = 0;
            itemCount = 0;
            if (itemsIndexed.Count == 0 || itemsToPurchase.Count == 0)
                return totalCost;

            foreach (KeyValuePair<Item, int> item in itemsToPurchase)
            {
                int purchaseCount = itemsToPurchase[item.Key];
                int itemCost = item.Key.creditsWorth * (Plugin.instance.Terminal.itemSalesPercentages[itemsIndexed[item.Key]] / 100);
                itemCost *= purchaseCount;
                totalCost += itemCost;
                itemCount += purchaseCount;
                Plugin.Spam($"Added {itemCost} to total: {totalCost} (total item count: {itemCount})");

            }

            Plugin.MoreLogs($"Total Cost: [{totalCost}]");
            return totalCost;
        }

        private static int GetUpgradesTotalCost(List<TerminalNode> upgradeList, out int itemCount)
        {
            int totalCost = 0;
            itemCount = 0;
            foreach (TerminalNode item in upgradeList)
            {
                totalCost += item.itemCost;
                itemCount++;
                Plugin.Spam($"Adding {item.itemCost} to total: {totalCost}");
            }

            Plugin.Spam($"totalCost: {totalCost}");
            return totalCost;
        }

        internal static string AskPurchasePack()
        {
            itemsIndexed.Clear();
            List<string> itemList = StringStuff.GetItemList(currentPackList);
            Dictionary<Item, int> itemsToPurchase = [];
            StringBuilder packAsk = new();
            packAsk.AppendLine($"Would you like to purchase the [{currentPackName}] PurchasePack?\r\n\r\n\tContents:\r\n");

            PurchasePackContents(itemList, ref itemsToPurchase, ref packAsk, out List<TerminalNode> upgradeItems);

            int totalCost = GetTotalCost(itemsToPurchase, itemsIndexed, out int itemCount);

            if (upgradeItems.Count > 0)
            {
                int upgradesCost = GetUpgradesTotalCost(upgradeItems, out int upgradeCount);
                Plugin.Spam($"Adding {upgradesCost} to {totalCost}");
                totalCost += upgradesCost;
                //itemCount += upgradeCount;
            }

            if (totalCost <= Plugin.instance.Terminal.groupCredits)
            {
                packAsk.AppendLine($"\r\n\tTotal Cost: ■{totalCost}({itemCount} items)\r\n\r\nPlease CONFIRM or DENY.\n");
                buyPackName = currentPackName;
                return packAsk.ToString();
            }
            else
            {
                Plugin.MoreLogs("not enough credits to purchase, sending to cannot afford display");
                TerminalGeneral.CancelConfirmation = true;
                return $"You cannot afford the {currentPackName} PurchasePack ({itemCount} items).\r\n\r\n\tTotal Cost: ■<color=#BD3131>{totalCost}</color>\r\n\r\n";
            }
        }

        private static int GetNameCount(List<string> itemList, string itemName)
        {
            int count = 0;
            foreach (string name in itemList)
            {
                if (name == itemName)
                {
                    count++;
                    Plugin.Spam($"Count for {name} - {count}");
                }
            }
            Plugin.MoreLogs($"Final Count: {itemName} - {count}");
            return count;
        }

        internal static int GetCostFromList(List<string> itemList)
        {
            int total = 0;
            foreach (string itemName in itemList)
            {
                if (TryGetItem(itemName, out Item itemObject))
                {
                    Plugin.Spam($"{itemName} is a valid item, getting cost");
                    int indexNum = GetIndexNum(itemObject);
                    int price = itemObject.creditsWorth * (Plugin.instance.Terminal.itemSalesPercentages[indexNum] / 100);
                    Plugin.Spam($"Adding {price} to {total}");
                    total += price;
                }
                else if (TryGetUpgrade(itemName, out TerminalNode upgradeItem))
                {
                    Plugin.Spam($"Upgrade detected");
                    if (upgradeItem == null)
                        Plugin.Spam("returned upgradeItem terminalNode is null for some reason");
                    else if (CheckUnlockableStatus(upgradeItem.shipUnlockableID))
                    {
                        Plugin.Spam($"list contains ship upgrade {upgradeItem.creatureName}, adding value {upgradeItem.itemCost} to {total}");
                        total += upgradeItem.itemCost;
                    }
                }
            }
            Plugin.MoreLogs($"total cost of list: {total}");
            return total;
        }

        private static bool TryGetItem(string itemName, out Item itemValue)
        {
            foreach (Item item in Plugin.instance.Terminal.buyableItemsList)
            {
                if (item.itemName.ToLower().Contains(itemName))
                {
                    itemValue = item;
                    return true;
                }
                //Plugin.MoreLogs($"{item.itemName} at index [{indexNum}] does not match config item {itemName}");
            }

            itemValue = null;
            return false;
        }

        private static bool TryGetUpgrade(string upgradeName, out TerminalNode UpgradeNode)
        {
            UpgradeNode = null;
            Plugin.Spam($"TryGetUpgrade from {upgradeName}");

            Plugin.Spam($"iterating through Plugin.Allnodes");

            foreach (TerminalNode node in Plugin.Allnodes)
            {
                if (node != null)
                {
                    //Plugin.Spam($"checking node: {node.name}");
                    if (node.creatureName != null)
                    {
                        if (node.creatureName.ToLower().StartsWith(upgradeName.ToLower()) && node.shipUnlockableID > 0 && CheckUnlockableStatus(node.shipUnlockableID))
                        {
                            Plugin.Spam($"unlockableID: {node.shipUnlockableID}");
                            Plugin.Spam($"creatureName: {node.creatureName} matching {upgradeName}");
                            UpgradeNode = node;
                            return true;
                        }
                    }
                }
                //Plugin.Spam("node is null");
            }
            return false;
        }


        private static void PurchasePackContents(List<string> itemList, ref Dictionary<Item, int> itemsToPurchase, ref StringBuilder packAsk, out List<TerminalNode> upgradeItems)
        {
            List<string> itemNames = [];
            upgradeItems = [];
            foreach (string item in itemList)
            {
                if (TryGetItemToBuy(item, out Item itemValue))
                {
                    Plugin.Spam($"{item} is a valid item, adding to pack purchase");
                    int count = GetNameCount(itemList, item);
                    if (count > 1 && !itemNames.Contains(itemValue.itemName))
                        packAsk.AppendLine($"{itemValue.itemName} x{count}");
                    else if (count <= 1)
                        packAsk.AppendLine($"{itemValue.itemName}");

                    if (!itemNames.Contains(itemValue.itemName))
                        itemNames.Add(itemValue.itemName);
                    if (!itemsToPurchase.ContainsKey(itemValue))
                        itemsToPurchase.Add(itemValue, count);
                }
                else if (TryGetUpgrade(item, out TerminalNode upgradeItem))
                {
                    if (!upgradeItems.Contains(upgradeItem) && (CheckUnlockableStatus(upgradeItem.shipUnlockableID)))
                    {
                        packAsk.AppendLine($"{upgradeItem.creatureName}");
                        upgradeItems.Add(upgradeItem);
                        Plugin.Spam($"list contains ship upgrades, adding to custom list and returning containsUpgrades true");
                    }
                }
            }
        }

        internal static int GetItemListCost(string rawList)
        {
            List<string> itemList = StringStuff.GetItemList(rawList);
            int totalCost = GetCostFromList(itemList);
            return totalCost;
        }

        internal static string CompletePurchasePack()
        {
            itemsIndexed.Clear();
            bool costDeducted = false;
            List<string> itemList = StringStuff.GetItemList(currentPackList);
            Dictionary<Item, int> itemsToPurchase = [];
            StringBuilder packBuy = new();
            packBuy.AppendLine($"You have purchased the {buyPackName} PurchasePack!\r\n\r\n\tContents:\r\n");
            PurchasePackContents(itemList, ref itemsToPurchase, ref packBuy, out List<TerminalNode> upgradeItems);
            int totalCost = GetTotalCost(itemsToPurchase, itemsIndexed, out int itemCount);

            if (upgradeItems.Count > 0)
            {
                int upgradesCost = GetUpgradesTotalCost(upgradeItems, out int upgradeCount);
                Plugin.Spam($"Adding {upgradesCost} to {totalCost}");
                totalCost += upgradesCost;
                //itemCount += upgradeCount;
            }

            if (totalCost > Plugin.instance.Terminal.groupCredits)
            {
                Plugin.MoreLogs("not enough credits to purchase, sending to error message");
                return Plugin.instance.Terminal.terminalNodes.specialNodes[5].displayText;
            }
            int[] fullItemList = BuyItems(itemsIndexed, itemsToPurchase);
            if (fullItemList.Length > 9)
                MegaPurchase(fullItemList, totalCost, out costDeducted);
            else
            {
                Plugin.instance.Terminal.BuyItemsServerRpc([.. fullItemList], Plugin.instance.Terminal.groupCredits - totalCost, Plugin.instance.Terminal.numberOfItemsInDropship + itemCount);
                costDeducted = true;
            }

            if (upgradeItems.Count > 0)
            {
                foreach (TerminalNode item in upgradeItems)
                {
                    StartOfRound.Instance.BuyShipUnlockableServerRpc(item.shipUnlockableID, Plugin.instance.Terminal.groupCredits);
                    Plugin.MoreLogs($"Unlocking {item.creatureName}");
                }

                if (!costDeducted)
                {
                    int[] boughtItems = [];
                    Plugin.instance.Terminal.BuyItemsServerRpc([.. boughtItems], Plugin.instance.Terminal.groupCredits - totalCost, Plugin.instance.Terminal.numberOfItemsInDropship);
                }

            }

            packBuy.AppendLine($"\r\n\r\nYour new balance is ■{Plugin.instance.Terminal.groupCredits} credits\r\n\r\n\tEnjoy!\r\n");
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
            return packBuy.ToString();
        }

        private static void MegaPurchase(int[] fullItemList, int totalCost, out bool costDeducted)
        {
            int count = 0;
            List<int> splitArray = [];
            foreach (int item in fullItemList)
            {
                if (count < 9)
                {
                    splitArray.Add(item);
                    count++;
                    Plugin.Spam($"count: {count}");
                }
                else
                {
                    Plugin.instance.Terminal.BuyItemsServerRpc([.. splitArray], Plugin.instance.Terminal.groupCredits, Plugin.instance.Terminal.numberOfItemsInDropship + 9);
                    Plugin.Spam($"purchased {count} items");
                    count = 0;
                    splitArray.Clear();
                    splitArray.Add(item);
                    count++;
                }
            }
            if (splitArray.Count > 0)
            {
                Plugin.instance.Terminal.BuyItemsServerRpc([.. splitArray], Plugin.instance.Terminal.groupCredits, Plugin.instance.Terminal.numberOfItemsInDropship + splitArray.Count);
                Plugin.Spam($"purchased another {splitArray.Count} items");
            }

            int[] boughtItems = [];
            Plugin.instance.Terminal.BuyItemsServerRpc([.. boughtItems], Plugin.instance.Terminal.groupCredits - totalCost, Plugin.instance.Terminal.numberOfItemsInDropship);
            costDeducted = true;
            Plugin.Spam("end of megapurchase");
        }

        private static int[] BuyItems(Dictionary<Item, int> itemsIndexed, Dictionary<Item, int> itemCounts)
        {
            List<int> thisOrder = [];
            foreach (KeyValuePair<Item, int> item in itemsIndexed)
            {
                int count = itemCounts[item.Key];
                for (int i = 0; i < count; i++)
                {
                    thisOrder.Add(item.Value);
                    Plugin.Spam($"Adding {item.Key} to order list ({i})");
                }
            }
            return [.. thisOrder];
        }

        internal static string PerformBioscanUpgrade()
        {
            if (enemyScanUpgradeEnabled == false)
            {
                int newCreds = Plugin.instance.Terminal.groupCredits - ConfigSettings.BioScanUpgradeCost.Value;
                string displayText = $"Biomatter Scanner software has been updated to the latest patch (2.0) and now provides more detailed information!\r\n\r\nYour new balance is ■{newCreds} Credits\r\n";
                SaveManager.NewUnlock("BioscanPatch");
                Plugin.instance.Terminal.SyncGroupCreditsServerRpc(newCreds, Plugin.instance.Terminal.numberOfItemsInDropship);
                Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
                return displayText;
            }
            else
            {
                string displayText = $"BioScanner software has already been updated to the latest patch (2.0).\r\n\r\n";
                return displayText;
            }
        }

        internal static string GetRefund()
        {
            string displayText;
            int deliverables = Plugin.instance.Terminal.numberOfItemsInDropship;
            Item[] buyables = Plugin.instance.Terminal.buyableItemsList;
            //List<int> items = Plugin.instance.Terminal.orderedItemsFromTerminal; //not using this cursed list anymore
            List<string> returnlist = [];
            int refund = 0;

            Plugin.MoreLogs($"buyables: {buyables.Length}, deliverables: {deliverables}, items: {storeCart.Count}");

            if (deliverables > 0)
            {
                foreach (int num in storeCart)
                {
                    if (num <= buyables.Length)
                    {
                        int index = GetIndexNum(buyables[num]);
                        int itemCost = buyables[num].creditsWorth * (Plugin.instance.Terminal.itemSalesPercentages[index] / 100);
                        refund += itemCost;
                        string itemname = buyables[num].itemName;
                        returnlist.Add(itemname + "\n");
                        Plugin.Spam($"Adding {itemname} ${itemCost} to refund list");
                    }
                    else
                    {
                        Plugin.Spam($"Unable to add item to refund list");
                    }

                }
                Plugin.Spam($"old creds: {Plugin.instance.Terminal.groupCredits}");
                int newCreds = Plugin.instance.Terminal.groupCredits + refund;
                Plugin.instance.Terminal.groupCredits = newCreds;
                Plugin.Spam($"new creds: {newCreds}");
                Plugin.instance.Terminal.orderedItemsFromTerminal.Clear();
                storeCart.Clear();
                NetHandler.Instance.SyncDropShipServerRpc(true);

                NetHandler.Instance.SyncCreditsServerRpc(newCreds, 0);

                string allitems = ListToStringBuild(returnlist);

                Plugin.MoreLogs($"Refund total: ${refund}");
                displayText = $"Cancelling order for:\n{allitems}\nYou have been refunded ■{refund} Credits!\r\n";
                Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
                return displayText;
            }
            else
                displayText = "No ordered items detected on the dropship.\n\n";

            return displayText;
        }

        private static string ListToStringBuild(List<string> list)
        {
            StringBuilder sb = new();

            for (int i = 0; i < list.Count; i++)
            {
                sb.Append(list[i]);
            }

            return sb.ToString();
        }

        internal static string PerformVitalsUpgrade()
        {
            if (vitalsUpgradeEnabled == false)
            {
                int newCreds = Plugin.instance.Terminal.groupCredits - ConfigSettings.VitalsUpgradeCost.Value;
                SaveManager.NewUnlock("VitalsPatch");
                string displayText = $"Vitals Scanner software has been updated to the latest patch (2.0) and no longer requires credits to scan.\r\n\r\nYour new balance is ■{newCreds} credits\r\n";
                Plugin.instance.Terminal.SyncGroupCreditsServerRpc(newCreds, Plugin.instance.Terminal.numberOfItemsInDropship);
                Plugin.instance.Terminal.PlayTerminalAudioServerRpc(0);
                return displayText;
            }
            else
            {
                string displayText = "Update already purchased.\n";
                return displayText;
            }
        }

        private static bool ShouldDisplayVitals(PlayerControllerB playerInfo, int getCreds, int costCreds)
        {
            return !playerInfo.isPlayerDead && (getCreds >= costCreds || vitalsUpgradeEnabled);
        }

        internal static int CalculateNewCredits(int getCreds, int costCreds, Terminal frompatch)
        {
            int newCreds = getCreds - costCreds;
            frompatch.groupCredits = newCreds;

            NetHandler.Instance.SyncCreditsServerRpc(newCreds, frompatch.numberOfItemsInDropship);
            return newCreds;
        }

        private static string GetVitalsInfo(PlayerControllerB playerInfo)
        {
            int playerHealth = playerInfo.health;
            float playerWeight = playerInfo.carryWeight;
            float playerSanity = playerInfo.insanityLevel;
            bool hasFlash = playerInfo.ItemSlots.Any(item => item is FlashlightItem);
            float realWeight = Mathf.RoundToInt(Mathf.Clamp(playerWeight - 1f, 0f, 100f) * 105f);

            string vitalsInfo = $"{playerInfo.playerUsername} Vitals:\n\n Health: {playerHealth}\n Weight: {realWeight}\n Sanity: {playerSanity}";

            if (hasFlash)
            {
                float flashCharge = Mathf.RoundToInt(playerInfo.pocketedFlashlight.insertedBattery.charge * 100);
                vitalsInfo += $"\n Flashlight Battery Percentage: {flashCharge}%";
            }

            return vitalsInfo;
        }

        private static string GetCreditsInfo(int newCreds)
        {
            return $"Your new balance is ■{newCreds} Credits.\r\n";
        }

    }
}

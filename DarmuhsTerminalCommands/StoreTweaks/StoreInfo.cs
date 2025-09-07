using System;
using System.Linq;
using TerminalStuff.SpecialStuff;
using static TerminalStuff.StoreTweaks.StorePlus;

namespace TerminalStuff.StoreTweaks;

public class StoreInfo
{
    internal int price = 0;
    public string name = "";
    public bool selected = false;
    internal UnlockableItem unlockable = null!;
    internal Item buyableItem = null!;
    internal BuyableVehicle vehicle = null!;
    internal bool isVehicle = false;
    internal bool isSuit = false;
    internal bool isPurchasePack = false;
    internal bool waitForDelivery = true;
    internal bool isUnlocked = false;
    public bool onSale = false;
    public int selectionCount = 1;
    internal int maxAllowed = 0;
    internal TerminalNode terminalNode = null!;

    public StoreMenuItem menuItem;

    public override string ToString()
    {
        return name;
    }

    internal StoreInfo(TerminalNode storeNode, bool purchasePack = false)
    {
        terminalNode = storeNode;
        price = storeNode.itemCost;
        isPurchasePack = purchasePack;
        GetStoreInfo(terminalNode);
        menuItem = new(name, true)
        {
            OnPageLoad = PageLoad,
            storeItem = this
        };
        menuItem.SelectionEvent.AddListener(OnSelect);
    }

    internal void UpdateNode(TerminalNode storeNode)
    {
        //name already exists
        if (terminalNode == null) //replace node if it's null
            terminalNode = storeNode;
        else if (terminalNode.itemCost < storeNode.itemCost)
            terminalNode = storeNode; //replace node that has a lower cost than the current one

        if (buyableItem == null) //buyable items do NOT use node cost
        {
            price = storeNode.itemCost;

            if (terminalNode.itemCost != storeNode.itemCost) //fix cases where one terminal node has the price and one doesnt
                price = Math.Max(terminalNode.itemCost, storeNode.itemCost);
        }

        GetStoreInfo(storeNode);
        menuItem.storeItem = this;
    }

    internal void OnSelect()
    {
        selected = !selected;
        selectionCount = 1;
        Plugin.Log.LogMessage($"{name} selected [{selected}]");
        UpdateSelection();
    }

    internal void UpdateSelection()
    {
        if (selected && !storeSelection.Contains(this))
            storeSelection.Add(this);

        if (!selected && storeSelection.Contains(this))
            storeSelection.Remove(this);
    }

    internal void PageLoad()
    {
        PriceChecks();

        menuItem.Prefix = "";
        menuItem.Suffix = "";

        menuItem.Prefix += $"${price} ";

        if (isVehicle && Plugin.instance.Terminal.hasWarrantyTicket)
            menuItem.Prefix += "(warranty) ";

        if (selectionCount > 1)
            menuItem.Suffix += $" x {selectionCount}";

        if (selected)
            menuItem.Suffix += " *";

        if (onSale)
            menuItem.Suffix += $"   ({GetSalesPercentage(this)}% OFF!)";

        if (price <= GetProjectedCredits() && StorePlusConfig.AffordableColor.Value.Length > 0)
        {
            menuItem.Prefix = menuItem.Prefix.Insert(0, $"<color={StorePlusConfig.AffordableColor.Value}>");
            menuItem.Suffix += "</color>";
        }

        if (price > GetProjectedCredits() && StorePlusConfig.NotEnoughCredsColor.Value.Length > 0 && !selected)
        {
            menuItem.Prefix = menuItem.Prefix.Insert(0, $"<color={StorePlusConfig.NotEnoughCredsColor.Value}>");
            menuItem.Suffix += "</color>";
        }
    }

    internal void GetStoreInfo(TerminalNode storeNode)
    {
        if (storeNode.buyItemIndex != -1 && Plugin.instance.Terminal.buyableItemsList.Length > storeNode.buyItemIndex)
        {
            buyableItem = Plugin.instance.Terminal.buyableItemsList[storeNode.buyItemIndex];
            if (buyableItem != null)
            {
                name = buyableItem.itemName;
                price = buyableItem.creditsWorth; //buyableitems use this value
            }

        }

        if (storeNode.buyVehicleIndex != -1)
        {
            vehicle = Plugin.instance.Terminal.buyableVehicles[storeNode.buyVehicleIndex];
            isVehicle = true;

            if (vehicle != null)
                name = vehicle.vehicleDisplayName;

            name ??= $"Company Cruiser ({storeNode.buyVehicleIndex})";
        }

        if (storeNode.shipUnlockableID != -1 && StartOfRound.Instance.unlockablesList.unlockables.Count > storeNode.shipUnlockableID)
        {
            unlockable = StartOfRound.Instance.unlockablesList.unlockables[storeNode.shipUnlockableID];

            if (unlockable == null)
                return;

            name = terminalNode.creatureName;
            waitForDelivery = false;

            if (unlockable.unlockableType == 0 || unlockable.suitMaterial != null)
                isSuit = true;

            if (unlockable.alreadyUnlocked || unlockable.hasBeenUnlockedByPlayer)
                isUnlocked = true;

            if (unlockable.maxNumber > 0)
                maxAllowed = unlockable.maxNumber;

        }

        if (isPurchasePack)
            name = unlockable.unlockableName;

        name ??= $"Unknown Item";
        name = name.Trim();
    }

    internal void PriceChecks()
    {
        //CommandManager thisItem = Configs.Commands.GetEnabledCommands().FirstOrDefault(x => x.CommandType == 2 && x.terminalNode == terminalNode);
        //if (thisItem != null)
        //{
        //   price = thisItem.StoreBase.ActualPrice;
        //   return;
        //}

        if (buyableItem == null)
            price = terminalNode.itemCost;
        else
            price = buyableItem.creditsWorth;

        if (isPurchasePack)
        {
            price = StorePacks.GetPriceFromNode(terminalNode);
            return;
        }

        if (waitForDelivery)
        {
            int original = price;

            if (!isVehicle)
                price = GetSalesPrice(price, terminalNode.buyItemIndex);
            else
                price = GetSalesPrice(price, terminalNode.buyVehicleIndex, true);

            onSale = price != original;
        }

        if (isVehicle && Plugin.instance.Terminal.hasWarrantyTicket)
            price = 0;

    }

    internal bool InRotation()
    {
        if (isVehicle)
            return true;

        if (waitForDelivery)
            return true;

        if (isPurchasePack)
            return true;

        if (ManualUpgradeNames.Any(c => OpenLib.Common.Misc.CompareStringsInvariant(c, terminalNode.creatureName)))
            return true;

        if (!StorePacks.IsUnlockableBuyable(terminalNode.shipUnlockableID))
            return false;

        if (!StorePlusConfig.RespectStoreRotation.Value)
            return true;

        if (Plugin.instance.Terminal.ShipDecorSelection.Contains(terminalNode))
            return true;

        return false;
    }

    internal void Reset()
    {
        selected = false;
        selectionCount = 1;
    }

    internal static bool IsItemEnabled(int indexNum)
    {
        if (indexNum < 0)
            return false;

        if (OpenLib.CoreMethods.DynamicBools.TryGetKeyword("buy", out TerminalKeyword buy))
        {
            return buy.compatibleNouns.Any(c => c.result.buyItemIndex == indexNum);
        }

        return false;
    }

    internal static bool IsValidItem(TerminalNode node)
    {
        if (node.terminalOptions == null)
            return false;

        if (node.terminalOptions.Length < 1)
            return false;

        if (node.buyItemIndex < 0)
            return false;

        return true;
    }

}

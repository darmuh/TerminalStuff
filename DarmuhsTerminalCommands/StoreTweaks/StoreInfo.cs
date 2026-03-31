using System;
using System.Linq;
using TerminalStuff.SpecialStuff;
using static TerminalStuff.StoreTweaks.StorePlus;

namespace TerminalStuff.StoreTweaks;

public class StoreInfo
{
    internal int price = 0;
    public string name = "";
    public bool Selected { get; private set; } = false;
    internal UnlockableItem Unlockable = null!;
    internal Item BuyableItem = null!;
    internal BuyableVehicle Vehicle = null!;
    internal bool IsVehicle()
    {
        return Vehicle != null;
    }
    internal bool IsSuit()
    {
        if (Unlockable == null)
            return false;

        return (Unlockable.unlockableType == 0 || Unlockable.suitMaterial != null);
    }
    internal bool IsPurchasePack { get; set; } = false;
    internal bool WaitForDelivery { get; set; } = true;
    internal bool IsUnlocked
    {
        get
        {
            if (Unlockable == null)
                return false;

            return (Unlockable.alreadyUnlocked || Unlockable.hasBeenUnlockedByPlayer);
        }
    }
    public bool OnSale { get; set; } = false;
    public int SelectionCount = 1;
    internal int maxAllowed = 0;
    internal TerminalNode terminalNode = null!;

    internal bool ShowItem = true;

    public StoreMenuItem ThisMenuItem;

    public override string ToString()
    {
        return name;
    }

    public bool ShouldShowInStore()
    {
        if (IsUnlocked)
            return false;

        if (terminalNode.buyItemIndex != -1 && BuyableItem == null)
            return false;

        //hide items defined by user
        if (OpenLib.Common.Misc.DoesListHaveInvariant(StorePlusConfig.HideItemListing, name))
            return false;

        return true;
    }

    internal StoreInfo(TerminalNode storeNode, bool purchasePack = false)
    {
        terminalNode = storeNode;
        price = storeNode.itemCost;
        IsPurchasePack = purchasePack;
        GetStoreInfo(terminalNode);
        ThisMenuItem = new(name, true)
        {
            OnPageLoad = PageLoad,
            StoreItem = this
        };
        ThisMenuItem.SelectionEvent.AddListener(OnSelect);
    }

    internal void UpdateNode(TerminalNode storeNode)
    {
        //name already exists
        if (terminalNode == null) //replace node if it's null
            terminalNode = storeNode;
        else if (terminalNode.itemCost < storeNode.itemCost)
            terminalNode = storeNode; //replace node that has a lower cost than the current one

        if (BuyableItem == null) //buyable items do NOT use node cost
        {
            price = storeNode.itemCost;

            if (terminalNode.itemCost != storeNode.itemCost) //fix cases where one terminal node has the price and one doesnt
                price = Math.Max(terminalNode.itemCost, storeNode.itemCost);
        }

        GetStoreInfo(storeNode);
        ThisMenuItem.StoreItem = this;
    }

    internal void OnSelect()
    {
        Selected = !Selected;
        SelectionCount = 1;
        Plugin.Log.LogMessage($"{name} selected [{Selected}]");
        UpdateSelection();
    }

    internal void UpdateSelection()
    {
        if (Selected && !StoreSelection.Contains(this))
            StoreSelection.Add(this);

        if (!Selected && StoreSelection.Contains(this))
            StoreSelection.Remove(this);
    }

    internal void PageLoad()
    {
        PriceChecks();

        ThisMenuItem.Prefix = "";
        ThisMenuItem.Suffix = "";

        ThisMenuItem.Prefix += $"${price} ";

        if (IsVehicle() && Plugin.instance.Terminal.hasWarrantyTicket)
            ThisMenuItem.Prefix += "(warranty) ";

        if (SelectionCount > 1)
            ThisMenuItem.Suffix += $" x {SelectionCount}";

        if (Selected)
            ThisMenuItem.Suffix += " *";

        if (OnSale)
            ThisMenuItem.Suffix += $"   ({GetSalesPercentage(this)}% OFF!)";

        if (price <= GetProjectedCredits() && StorePlusConfig.AffordableColor.Value.Length > 0)
        {
            ThisMenuItem.Prefix = ThisMenuItem.Prefix.Insert(0, $"<color={StorePlusConfig.AffordableColor.Value}>");
            ThisMenuItem.Suffix += "</color>";
        }

        if (price > GetProjectedCredits() && StorePlusConfig.NotEnoughCredsColor.Value.Length > 0 && !Selected)
        {
            ThisMenuItem.Prefix = ThisMenuItem.Prefix.Insert(0, $"<color={StorePlusConfig.NotEnoughCredsColor.Value}>");
            ThisMenuItem.Suffix += "</color>";
        }
    }

    internal void GetStoreInfo(TerminalNode storeNode)
    {
        if (storeNode.buyItemIndex != -1 && Plugin.instance.Terminal.buyableItemsList.Length > storeNode.buyItemIndex)
        {
            BuyableItem = Plugin.instance.Terminal.buyableItemsList[storeNode.buyItemIndex];
            if (BuyableItem != null)
            {
                name = BuyableItem.itemName;
                price = BuyableItem.creditsWorth; //buyableitems use this value
            }

        }

        if (storeNode.buyVehicleIndex != -1)
        {
            Vehicle = Plugin.instance.Terminal.buyableVehicles[storeNode.buyVehicleIndex];

            if (Vehicle != null)
                name = Vehicle.vehicleDisplayName;

            name ??= $"Company Cruiser ({storeNode.buyVehicleIndex})";
        }

        if (storeNode.shipUnlockableID != -1 && StartOfRound.Instance.unlockablesList.unlockables.Count > storeNode.shipUnlockableID)
        {
            Unlockable = StartOfRound.Instance.unlockablesList.unlockables[storeNode.shipUnlockableID];

            if (Unlockable == null)
                return;

            name = terminalNode.creatureName;
            WaitForDelivery = false;

            if (Unlockable.maxNumber > 0)
                maxAllowed = Unlockable.maxNumber;

        }

        if (IsPurchasePack)
            name = Unlockable.unlockableName;

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

        if (BuyableItem == null)
            price = terminalNode.itemCost;
        else
            price = BuyableItem.creditsWorth;

        if (IsPurchasePack)
        {
            price = StorePacks.GetPriceFromNode(terminalNode);
            return;
        }

        if (WaitForDelivery)
        {
            int original = price;

            if (!IsVehicle())
                price = GetSalesPrice(price, terminalNode.buyItemIndex);
            else
                price = GetSalesPrice(price, terminalNode.buyVehicleIndex, true);

            OnSale = price != original;
        }

        if (IsVehicle() && Plugin.instance.Terminal.hasWarrantyTicket)
            price = 0;

    }

    internal bool InRotation()
    {
        if (IsVehicle())
            return true;

        if (WaitForDelivery)
            return true;

        if (IsPurchasePack)
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
        Selected = false;
        SelectionCount = 1;
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

using OpenLib.ConfigManager;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TerminalStuff.SpecialStuff
{
    internal class StoreInfo
    {
        internal int price = 0;
        internal string name = "";
        internal bool selected = false;
        internal UnlockableItem unlockable = null!;
        internal Item buyableItem = null!;
        internal BuyableVehicle vehicle = null!;
        internal bool isVehicle = false;
        internal bool isSuit = false;
        internal bool isPurchasePack = false;
        internal bool waitForDelivery = true;
        internal bool isUnlocked = false;
        internal bool onSale = false;
        internal int selectionCount = 1;
        internal int maxAllowed = 0;
        internal TerminalNode terminalNode = null!;

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
        }

        internal void UpdateNode(TerminalNode storeNode)
        {
            //name already exists
            if(terminalNode == null) //only replace node if it's null
                terminalNode = storeNode;

            price = storeNode.itemCost;

            if (terminalNode.itemCost != storeNode.itemCost) //fix cases where one terminal node has the price and one doesnt
                price = Math.Max(terminalNode.itemCost, storeNode.itemCost);

            GetStoreInfo(storeNode);
        }

        internal void GetStoreInfo(TerminalNode storeNode)
        {
            if (storeNode.buyItemIndex != -1 && Plugin.instance.Terminal.buyableItemsList.Length > storeNode.buyItemIndex)
            {
                buyableItem = Plugin.instance.Terminal.buyableItemsList[storeNode.buyItemIndex];
                if(buyableItem != null)
                    name = buyableItem.itemName;
            }

            if (storeNode.buyVehicleIndex != -1)
            {
                vehicle = Plugin.instance.Terminal.buyableVehicles[storeNode.buyVehicleIndex];
                isVehicle = true;

                if(vehicle != null)
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

                if(unlockable.maxNumber > 0)
                    maxAllowed = unlockable.maxNumber;

            }

            if (isPurchasePack)
                name = unlockable.unlockableName;

            name ??= $"Unknown Item";
            name.Trim();
        }

        internal void PriceChecks()
        {
            ManagedConfig thisItem = ConfigSetup.defaultManaged.FirstOrDefault(x => x.CommandType == 2 && x.TerminalNode == terminalNode);
            if (thisItem != null)
            {
                price = thisItem.price;
                return;
            }

            price = terminalNode.itemCost;

            if (isPurchasePack)
            {
                price = StorePacks.GetPriceFromNode(terminalNode);
                return;
            }    

            if (waitForDelivery)
            {
                if(!isVehicle)
                    price = StorePlus.GetSalesPrice(terminalNode.itemCost, terminalNode.buyItemIndex);
                else
                    price = StorePlus.GetSalesPrice(terminalNode.itemCost, terminalNode.buyVehicleIndex, true);

                onSale = price != terminalNode.itemCost;
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

            if (StorePlus.ManualUpgradeNames.Any(c => c.ToLower() == terminalNode.creatureName.ToLower()))
                return true;

            if (!StorePacks.IsUnlockableBuyable(terminalNode.shipUnlockableID))
                return false;

            if(!StorePlusConfig.RespectStoreRotation.Value)
                return true;

            if(Plugin.instance.Terminal.ShipDecorSelection.Contains(terminalNode))
                return true;

            return false;
        }

        internal void Reset()
        {
            selected = false;
            selectionCount = 1;
        }

        internal static void MakeStoreInfo(string kw, string displayName = "")
        {
            StoreMenuItem item;
            string display;
            if (displayName.Length > 0)
                display = displayName;
            else
                display = kw;

            item = StorePlus.AllExternalModMenus.nestedMenuItems.FirstOrDefault(x => x.MenuName == display);

            if (item != null)
                item.UpdateExternal(kw, 11, display);
            else
            {
                item = new(kw, 11, true, display);
                StorePlus.AddNestedMenuItem(item, "Other");
            }

        }

        internal static void MakeMainMenuItem(string kw, string displayName = "")
        {
            StoreMenuItem item;
            string display;
            if (displayName.Length > 0)
                display = displayName;
            else
                display = kw;

            item = StorePlus.storeMenuMain.FirstOrDefault(x => x.MenuName == display);

            if (item != null)
                item.UpdateExternal(kw, 1, display);
            else
            {
                item = new(display, 1, false)
                {
                    Keyword = kw,
                    externalCommand = true
                };
            }

        }

    }

    internal class StoreMenuItem
    {
        internal string MenuName = "";
        internal string Keyword = "";
        internal StoreMenuItem ParentMenu = null!;
        internal List<StoreInfo> storeItems = [];
        internal string bottomTextAdd = "";
        internal List<StoreMenuItem> nestedMenuItems = [];
        internal Action MenuSpecialAction = null!;
        internal int menuLevel = 0;
        internal bool active = false;

        //for linking to external page outside this menu
        internal bool externalCommand = false;

        public override string ToString()
        {
            return MenuName;
        }

        internal StoreMenuItem(string menuName, int level, bool isNested = false)
        {
            Plugin.Spam("Registering modded menu keyword for loading");
            MenuName = menuName;
            menuLevel = level;

            if(!isNested)
                StorePlus.storeMenuMain.Add(this);

        }

        internal StoreMenuItem(string menuKW, int level, bool external = true, string displayName = "") //nested menu
        {
            Plugin.Spam("Registering external keyword for loading");
            Keyword = menuKW;
            if (displayName.Length > 0)
                MenuName = displayName;
            else
                MenuName = menuKW;

            menuLevel = level;
            externalCommand = external;
        }

        internal void SetParentMenu(StoreMenuItem parent)
        {
            ParentMenu = parent;
            if(!parent.nestedMenuItems.Contains(this))
                parent.nestedMenuItems.Add(this);
        }

        internal void UpdateExternal(string menuKW, int level, string displayName = "")
        {
            Plugin.Spam("Updating registered keyword for loading");
            Keyword = menuKW;
            if (displayName.Length > 0)
                MenuName = displayName;
            else
                MenuName = menuKW;

            menuLevel = level;
            externalCommand = true;
        }
    }
}

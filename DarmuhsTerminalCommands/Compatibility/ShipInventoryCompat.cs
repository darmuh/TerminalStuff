using ShipInventoryUpdated.Objects;
using ShipInventoryUpdated.Scripts;
using System.Collections.Generic;

namespace TerminalStuff.Compatibility;

internal class ShipInventoryCompat
{
    //Only call this method after checking bool is true
    internal static int GetInventoryValue()
    {
        int value = 0;

        foreach(var item in Inventory.Items)
        {
            value += item.SCRAP_VALUE;
        }

        return value;
    }

    internal static void GetInventoryItems(out List<Item> itemsInventory)
    {
        itemsInventory = [];
        if (!Plugin.instance.ShipInventory)
            return;

        List<ItemData> allItems = [.. Inventory.Items];

        foreach (ItemData item in allItems)
        {
            Item? thisItem = item.GetItem();
            if (thisItem != null)
                itemsInventory.Add(thisItem);
        }
    }
}

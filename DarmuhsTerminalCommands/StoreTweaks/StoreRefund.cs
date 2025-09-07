using System.Collections.Generic;

namespace TerminalStuff.StoreTweaks;

internal class StoreRefundList
{
    internal static List<StoreRefundItem> storeRefundItems = [];
}
internal class StoreRefundItem
{
    internal Item item;
    internal int count;
    internal int value;

    internal StoreRefundItem(Item thing, int number)
    {
        item = thing;
        count = number;
    }

    internal void GetValue(Item[] buyables, int num)
    {
        value = StorePlus.GetSalesPrice(buyables[num].creditsWorth, num);
    }
}

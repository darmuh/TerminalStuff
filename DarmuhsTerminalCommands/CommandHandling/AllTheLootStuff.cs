using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalStuff.Patching;
using TerminalStuff.Util;
using UnityEngine;

namespace TerminalStuff;

internal class AllTheLootStuff
{
    internal static string GetLootSimple()
    {
        string displayText;
        Loggers.LogDebug("calculating loot value next");
        float lootValue = CalculateLootValue();
        string totalvalue;
        if (Plugin.instance.ShipInventory)
        {
            int inventoryVal = Compatibility.ShipInventoryCompat.GetInventoryValue();
            totalvalue = $"Total Value on Ship: ${lootValue}\n\nTotal Value in Ship Inventory: ${inventoryVal}";
        }
        else
        {
            totalvalue = $"Total Value on Ship: ${lootValue}";
        }

        Loggers.LogDebug("loot calculated");
        displayText = $"{totalvalue}\n\n";
        return displayText;
    }

    internal static string DetailedLootCommand()
    {
        LoadGrabbablesOnShip.LoadAllItems();
        string displayText;
        StringBuilder sb = new();
        Dictionary<string, int> lineOccurrences = [];
        int totalCredsWorth = 0;

        foreach (var grabbableItem in LoadGrabbablesOnShip.ItemsOnShip)
        {
            string itemName = grabbableItem.itemProperties.itemName;
            int scrapWorth = grabbableItem.scrapValue;


            if (grabbableItem.itemProperties.isScrap)
            {
                // Concatenate the itemName and scrapWorth to form the line
                string line = $"{itemName} ({scrapWorth} credits)";
                Loggers.LogDebug(line + "added to output");
                totalCredsWorth += scrapWorth;

                lineOccurrences[line] = lineOccurrences.TryGetValue(line, out int count) ? count + 1 : 1;
            }
        }

        foreach (var kvp in lineOccurrences)
        {
            if (kvp.Value > 1)
            {
                sb.AppendLine($"{kvp.Key} [x{kvp.Value}]");
            }
            else
                sb.AppendLine($"{kvp.Key}");
        }

        if (Plugin.instance.ShipInventory)
        {
            Compatibility.ShipInventoryCompat.GetInventoryItems(out List<Item> itemsInv);
            StringBuilder inv = ShipInventoryItems(itemsInv, ref totalCredsWorth);
            displayText = $"Scrap on ship (not stored):\n\n{sb}Scrap stored in Ship Inventory:\n\n{inv}\n\n\tTotal Value: {totalCredsWorth}\n\n";

        }
        else
            displayText = $"Scrap on ship:\n\n{sb}\n\n\tTotal Value: {totalCredsWorth}\n\n";

        return displayText;
    }

    private static StringBuilder ShipInventoryItems(List<Item> inventory, ref int totalCredsWorth)
    {
        StringBuilder builder = new();
        Dictionary<string, int> lineOccurrences = [];

        foreach (Item item in inventory)
        {
            if (!item.isScrap)
                continue;

            string itemName = item.itemName;
            int scrapWorth = item.creditsWorth;

            // Concatenate the itemName and scrapWorth to form the line
            string line = $"{itemName} ({scrapWorth} credits)";
            Loggers.LogDebug(line + "added to output");
            totalCredsWorth += scrapWorth;

            lineOccurrences[line] = lineOccurrences.TryGetValue(line, out int count) ? count + 1 : 1;
        }

        foreach (var kvp in lineOccurrences)
        {
            if (kvp.Value > 1)
            {
                builder.AppendLine($"{kvp.Key} [x{kvp.Value}]");
            }
            else
                builder.AppendLine($"{kvp.Key}");
        }

        return builder;
    }

    private static float CalculateLootValue()
    {
        List<GrabbableObject> list = [.. GameObject.Find("/Environment/HangarShip").GetComponentsInChildren<GrabbableObject>().Where<GrabbableObject>(obj => obj.name != "ClipboardManual" && obj.name != "StickyNoteItem" && obj.name != "Key(Clone)")]; //!obj.name.Contains("Key") or Key(Clone)

        Plugin.Log.LogDebug("Calculating total ship scrap value.");

        HarmonyLib.CollectionExtensions.Do<GrabbableObject>(list, scrap => Plugin.Log.LogDebug(string.Format("{0} - ${1}", scrap.name, scrap.scrapValue)));

        return list.Sum<GrabbableObject>(scrap => scrap.scrapValue);
    }


}

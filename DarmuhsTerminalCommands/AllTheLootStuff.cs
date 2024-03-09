using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace TerminalStuff
{
    internal class AllTheLootStuff
    {
        internal static void GetLootSimple(out string displayText)
        {
            Plugin.MoreLogs("calculating loot value next");
            float lootValue = CalculateLootValue();
            string totalvalue = string.Format("Total Value on Ship: ${0:F0}", (object)lootValue);
            TerminalEvents.TotalValueFormat = totalvalue;
            Plugin.MoreLogs("loot calculated");
            displayText = $"{TerminalEvents.TotalValueFormat}\n\n";
        }

        internal static void DetailedLootCommand(out string displayText)
        {
            LoadGrabbablesOnShip.LoadAllItems();

            StringBuilder sb = new StringBuilder();
            Dictionary<string, int> lineOccurrences = new Dictionary<string, int>();
            int totalCredsWorth = 0;

            foreach (var grabbableItem in LoadGrabbablesOnShip.ItemsOnShip)
            {
                string itemName = grabbableItem.itemProperties.itemName;
                int scrapWorth = grabbableItem.scrapValue;


                if (grabbableItem.itemProperties.isScrap)
                {
                    // Concatenate the itemName and scrapWorth to form the line
                    string line = $"{itemName} ({scrapWorth} credits)";
                    Plugin.MoreLogs(line + "added to output");
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

            displayText = $"Scrap on ship:\n\n{sb}\n\n\tTotal Value: {totalCredsWorth}\n\n";
            return;
        }

        private static float CalculateLootValue()
        {
            List<GrabbableObject> list = ((IEnumerable<GrabbableObject>)GameObject.Find("/Environment/HangarShip").GetComponentsInChildren<GrabbableObject>())
                .Where<GrabbableObject>(obj => obj.name != "ClipboardManual" && obj.name != "StickyNoteItem" && obj.name != "Key(Clone)").ToList<GrabbableObject>(); //!obj.name.Contains("Key") or Key(Clone)

            Plugin.Log.LogDebug((object)"Calculating total ship scrap value.");

            CollectionExtensions.Do<GrabbableObject>((IEnumerable<GrabbableObject>)list, (Action<GrabbableObject>)(scrap => Plugin.Log.LogDebug((object)string.Format("{0} - ${1}", (object)scrap.name, (object)scrap.scrapValue))));

            return (float)list.Sum<GrabbableObject>(scrap => scrap.scrapValue);
        }


    }
}

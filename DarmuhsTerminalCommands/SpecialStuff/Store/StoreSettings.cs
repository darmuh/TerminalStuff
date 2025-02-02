using System.Collections.Generic;
using System.Linq;

namespace TerminalStuff.SpecialStuff.Store
{
    internal class StoreSettings
    {
        internal static int savings = 0;
        internal static StoreMenuItem settings = new("Settings", 10, false);
        internal static StoreMenuItem sortAbc = new("Sort Alphabetically", 11, true);
        internal static StoreMenuItem sort123 = new("Sort By Price", 11, true);
        internal static StoreMenuItem sortNone = new("Remove Sorting", 11, true);
        internal static StoreMenuItem keepCreds = new($"Increase Savings: <color=#1cde2b>${savings}</color> (5%)", 11, true);
        internal static StoreMenuItem clearSavings = new($"Clear Savings", 11, true);
        internal static SortingStyle SortStyle = new();

        internal enum SortingStyle
        {
            None = 0,
            AlphabeticalDown = 1,
            AlphabeticalUp = 2,
            PriceDown = 3,
            PriceUp = 4
        }

        internal static void Init()
        {
            sortAbc.SetParentMenu(settings);
            sortAbc.MenuSpecialAction = SetSortAbc;
            sort123.SetParentMenu(settings);
            sort123.MenuSpecialAction = SetSort123;
            sortNone.SetParentMenu(settings);
            sortNone.MenuSpecialAction = SortNone;
            keepCreds.SetParentMenu(settings);
            keepCreds.MenuSpecialAction = ToggleSavings;
            clearSavings.SetParentMenu(settings);
            clearSavings.MenuSpecialAction = ClearSavings;
            SortStyle = SortingStyle.None;
            settings.bottomTextAdd = $"Current Sort: [{SortStyle}]\r\n\r\n";
        }

        private static void SortNone()
        {
            SortStyle = SortingStyle.None;
            settings.bottomTextAdd = $"Current Sort: [{SortStyle}]\r\n\r\n";
        }

        private static void SetSortAbc()
        {
            if (SortStyle == SortingStyle.AlphabeticalDown)
                SortStyle = SortingStyle.AlphabeticalUp;
            else if (SortStyle == SortingStyle.AlphabeticalUp)
                SortStyle = SortingStyle.AlphabeticalDown;
            else
                SortStyle = SortingStyle.AlphabeticalDown; 

            settings.bottomTextAdd = $"Current Sort: [{SortStyle}]\r\n\r\n";
        }

        private static void SetSort123()
        {
            if (SortStyle == SortingStyle.PriceDown)
                SortStyle = SortingStyle.PriceUp;
            else if (SortStyle == SortingStyle.PriceUp)
                SortStyle = SortingStyle.PriceDown;
            else
                SortStyle = SortingStyle.PriceDown;

            settings.bottomTextAdd = $"Current Sort: [{SortStyle}]\r\n\r\n";
        }

        private static void ToggleSavings()
        {
            int fullCreds = Plugin.instance.Terminal.groupCredits - StorePlus.SubTotal;

            float mathresult = (float)Plugin.instance.Terminal.groupCredits * 0.05f;

            if (savings + (int)mathresult <= fullCreds)
                savings += (int)mathresult;
            else 
                return;

            keepCreds.MenuName = $"Increase Savings: <color=#1cde2b>${savings}</color> (5%)";
        }

        private static void ClearSavings()
        {
            savings = 0;
            keepCreds.MenuName = $"Increase Savings: <color=#1cde2b>${savings}</color> (5%)";
        }

        internal static void UpdateSort(ref List<StoreInfo> items)
        {
            Plugin.Spam($"Updating items sort style! Using: {SortStyle}");
            if (SortStyle == SortingStyle.AlphabeticalDown)
                items = [.. items.OrderByDescending(x => x.name)];
            else if (SortStyle == SortingStyle.AlphabeticalUp)
                items = [.. items.OrderBy(x => x.name)];
            else if (SortStyle == SortingStyle.PriceDown)
                items = [.. items.OrderByDescending(x => x.price)];
            else if (SortStyle == SortingStyle.PriceUp)
                items = [.. items.OrderBy(x => x.price)];
            else
                Plugin.Spam("No ordering necessary!");
        }
    }
}

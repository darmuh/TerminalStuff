using OpenLib.InteractiveMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalStuff.MoonsTweaks;
using TerminalStuff.SpecialStuff;

namespace TerminalStuff.StoreTweaks;

public class StoreSettings
{
    public static int Savings
    {
        get
        {
            if (StorePlusConfig.DefaultSavings != null)
                return StorePlusConfig.DefaultSavings.Value;
            else
                return 0;
        }
        set
        {
            StorePlusConfig.DefaultSavings.Value = value;
        }
    }
    public static StoreMenuItem Settings = new("Settings", true);
    public static StoreMenuItem SortAbc = new("Sort Alphabetically", true);
    public static StoreMenuItem Sort123 = new("Sort By Price", true);
    public static StoreMenuItem SortNoneMenu = new("Remove Sorting", true);
    public static StoreMenuItem KeepCredsMenu = new($"Increase Savings: <color=#1cde2b>${Savings}</color> (5%)", true);
    public static StoreMenuItem ClearSavingsMenu = new($"Clear Savings", true);
    internal static SortingStyle CurrentSort { get; set; } = SortingStyle.None;
    internal static string CurrentSortText = string.Empty;


    public enum SortingStyle
    {
        None = 0,
        AlphabeticalDown = 1,
        AlphabeticalUp = 2,
        PriceDown = 3,
        PriceUp = 4
    }

    internal static void Init()
    {
        Settings.Header = () => "===== StorePlus Settings =====\n\n";
        Settings.SetParentMenu(StorePlus.TheMainMenu);
        Settings.Footer = () =>
        {
            StringBuilder message = new();
            message.Append($"Currently Sorting by: {CurrentSort}\n");
            message.Append($"Back Menu: [{StorePlus.StorePlusMenu.leaveMenu}]    Toggle Setting: [{StorePlus.StorePlusMenu.selectMenu}]\n\n");
            return message.ToString();
        };
        SortAbc.SetParentMenu(Settings);
        SortAbc.SelectionEvent.AddListener(SetSortAbc);
        Sort123.SetParentMenu(Settings);
        Sort123.SelectionEvent.AddListener(SetSort123);
        SortNoneMenu.SetParentMenu(Settings);
        SortNoneMenu.SelectionEvent.AddListener(SortNone);
        KeepCredsMenu.SetParentMenu(Settings);
        KeepCredsMenu.SelectionEvent.AddListener(ToggleSavings);
        ClearSavingsMenu.SetParentMenu(Settings);
        ClearSavingsMenu.SelectionEvent.AddListener(ClearSavings);
        CurrentSort = StorePlusConfig.DefaultSortStyle.Value;
        CurrentSortText = $"Current Sort: {CurrentSort}\n";
    }

    private static void SortNone()
    {
        CurrentSort = SortingStyle.None;
        CurrentSortText = $"Current Sort: {CurrentSort}\n";
    }

    private static void SetSortAbc()
    {
        if (CurrentSort == SortingStyle.AlphabeticalDown)
            CurrentSort = SortingStyle.AlphabeticalUp;
        else if (CurrentSort == SortingStyle.AlphabeticalUp)
            CurrentSort = SortingStyle.AlphabeticalDown;
        else
            CurrentSort = SortingStyle.AlphabeticalDown;

        CurrentSortText = $"Current Sort: {CurrentSort}\n";
    }

    private static void SetSort123()
    {
        if (CurrentSort == SortingStyle.PriceDown)
            CurrentSort = SortingStyle.PriceUp;
        else if (CurrentSort == SortingStyle.PriceUp)
            CurrentSort = SortingStyle.PriceDown;
        else
            CurrentSort = SortingStyle.PriceDown;

        CurrentSortText = $"Current Sort: [{CurrentSort}]\n\n";
    }

    private static void ToggleSavings()
    {
        int fullCreds = Plugin.instance.Terminal.groupCredits - StorePlus.SubTotal;

        int mathresult = Convert.ToInt32(Plugin.instance.Terminal.groupCredits * 0.05f);

        if (Savings + mathresult <= fullCreds)
            Savings += mathresult;
        else
            return;

        KeepCredsMenu.Name = $"Increase Savings: <color=#1cde2b>${Savings}</color> (5%)";
    }

    private static void ClearSavings()
    {
        Savings = 0;
        KeepCredsMenu.Name = $"Increase Savings: <color=#1cde2b>${Savings}</color> (5%)";
    }

    internal static void SortMenu(ref List<MenuItem> menuItems)
    {
        UpdateSorting(CurrentSort, ref menuItems);
    }

    internal static void UpdateSorting(SortingStyle style, ref List<MenuItem> menuItems)
    {
        CurrentSort = style;
        List<StoreMenuItem> list = [];

        switch (style)
        {
            case SortingStyle.AlphabeticalUp:
                list = [.. menuItems.OfType<StoreMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderBy(x => x.Name)];
                break;
            case SortingStyle.AlphabeticalDown:
                list = [.. menuItems.OfType<StoreMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderByDescending(x => x.Name)];
                break;
            case SortingStyle.PriceUp:
                list = [.. menuItems.OfType<StoreMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderBy(x => x.StoreItem.price)];
                break;
            case SortingStyle.PriceDown:
                list = [.. menuItems.OfType<StoreMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderByDescending(x => x.StoreItem.price)];
                break;
            case SortingStyle.None:
                break;
        }
    }
}

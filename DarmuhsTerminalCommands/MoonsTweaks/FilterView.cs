using OpenLib.InteractiveMenus;
using System;
using System.Collections.Generic;
using System.Linq;
using TerminalStuff.Configs;
using TerminalStuff.Util;
using static TerminalStuff.MoonsTweaks.MoonsPlus;

namespace TerminalStuff.MoonsTweaks;

public class FilterView
{
    public MoonMenuItem MenuItem = null!;
    public static SortingStyle Sorting
    {
        get
        {
            if (MoonsPlusConfig.DefaultSorting == null)
                return SortingStyle.ID_Up;
            else
                return MoonsPlusConfig.DefaultSorting.Value;
        }
        set
        {
            MoonsPlusConfig.DefaultSorting.Value = value;
        }
    }
    public static DisplayStyle Styling = DisplayStyle.None; //default

    [Flags]
    public enum DisplayStyle
    {
        None = 0,
        Weather = 1,
        Price = 2,
        Difficulty = 4
    }

    public enum SortingStyle
    {
        ID_Up,
        ID_Down,
        AlphabeticalUp,
        AlphabeticalDown,
        PriceUp,
        PriceDown,
        WeatherUp,
        WeatherDown,
        DifficultyUp,
        DifficultyDown
    }

    internal static void DebugFlags()
    {
        Loggers.LogDebug($"HasFlag(Weather) {Styling.HasFlag(DisplayStyle.Weather)}");
        Loggers.LogDebug($"HasFlag(Difficulty) {Styling.HasFlag(DisplayStyle.Difficulty)}");
        Loggers.LogDebug($"HasFlag(Price) {Styling.HasFlag(DisplayStyle.Price)}");
    }

    internal static void SetDefaults(string config)
    {
        if (config.Length < 1)
            return;

        List<string> entries = [.. config.Split(',')];
        foreach (string entry in entries)
        {

            if (entry.Trim().Equals("weather", StringComparison.InvariantCultureIgnoreCase))
                Styling |= DisplayStyle.Weather;

            if (entry.Trim().Equals("price", StringComparison.InvariantCultureIgnoreCase))
                Styling |= DisplayStyle.Price;

            if (entry.Trim().Equals("difficulty", StringComparison.InvariantCultureIgnoreCase))
                Styling |= DisplayStyle.Difficulty;
        }

        Loggers.LogMessage($"MoonsPlus Default Styling: {Styling}");
        DebugFlags();
    }

    internal static void MoonOnTopCheck(ref List<MenuItem> menuItems)
    {
        if (MoonsPlusConfig.ThisAlwaysOnTop.Value.Length < 1)
            return;

        MenuItem expectedTop = menuItems.FirstOrDefault(x => OpenLib.Common.Misc.StringContainsInvariant(x.Name, MoonsPlusConfig.ThisAlwaysOnTop.Value));

        if (expectedTop == null)
        {
            Plugin.Log.LogMessage($"Could not find {MoonsPlusConfig.ThisAlwaysOnTop.Value} on this page");
            return;
        }

        int thisIndex = menuItems.IndexOf(expectedTop);
        Loggers.LogDebug($"{MoonsPlusConfig.ThisAlwaysOnTop.Value} = {thisIndex}");
        if (thisIndex > 0)
        {
            menuItems.RemoveAt(thisIndex);
            menuItems.Insert(0, expectedTop);

            Loggers.LogDebug($"{MoonsPlusConfig.ThisAlwaysOnTop.Value} is now first in list!");
        }
    }

    internal static void OrderMenu(ref List<MenuItem> menuItems)
    {
        if(menuItems.Count == 0) return;

        UpdateSorting(Sorting, ref menuItems);
        MoonOnTopCheck(ref menuItems);
    }

    internal static void SortByLevelID()
    {
        if(Sorting == SortingStyle.ID_Up)
            Sorting = SortingStyle.ID_Down;
        else
            Sorting = SortingStyle.ID_Up;
    }

    internal static void SortByName()
    {
        if (Sorting == SortingStyle.AlphabeticalUp)
            Sorting = SortingStyle.AlphabeticalDown;
        else
            Sorting = SortingStyle.AlphabeticalUp;
    }

    internal static void SortByPrice()
    {
        if (Sorting == SortingStyle.PriceUp)
            Sorting = SortingStyle.PriceDown;
        else
            Sorting = SortingStyle.PriceUp;
    }

    internal static void SortByWeather()
    {
        if (Sorting == SortingStyle.WeatherUp)
            Sorting = SortingStyle.WeatherDown;
        else
            Sorting = SortingStyle.WeatherUp;
    }

    internal static void SortByRisk()
    {
        if (Sorting == SortingStyle.DifficultyUp)
            Sorting = SortingStyle.DifficultyDown;
        else
            Sorting = SortingStyle.DifficultyUp;
    }

    internal static void UpdateSorting(SortingStyle style, ref List<MenuItem> menuItems)
    {
        Sorting = style;
        List<MoonMenuItem> list = [];

        if (MoonListing.Count == 0)
            return;

        switch (style)
        {
            case SortingStyle.ID_Up:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if(list.Count != 0)
                    menuItems = [.. list.OrderBy(x => x.moonInfo.LevelID)];
                break;
            case SortingStyle.ID_Down:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderByDescending(x => x.moonInfo.LevelID)];
                break;
            case SortingStyle.AlphabeticalUp:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderBy(x => x.moonInfo.LevelName)];
                break;
            case SortingStyle.AlphabeticalDown:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderByDescending(x => x.moonInfo.LevelName)];
                break;
            case SortingStyle.WeatherUp:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderBy(x => GetWeatherName(x.moonInfo.Level))];
                break;
            case SortingStyle.WeatherDown:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderByDescending(x => GetWeatherName(x.moonInfo.Level))];
                break;
            case SortingStyle.DifficultyUp:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderBy(x => x.moonInfo.Level.riskLevel)];
                break;
            case SortingStyle.DifficultyDown:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderByDescending(x => x.moonInfo.Level.riskLevel)];
                break;
            case SortingStyle.PriceUp:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderBy(x => x.moonInfo.DisplayPrice)];
                break;
            case SortingStyle.PriceDown:
                list = [.. menuItems.OfType<MoonMenuItem>()];
                if (list.Count != 0)
                    menuItems = [.. list.OrderByDescending(x => x.moonInfo.DisplayPrice)];
                break;
        }
    }

    internal static void ToggleGeneric(DisplayStyle flag, out bool isEnabled)
    {
        isEnabled = !Styling.HasFlag(flag);
        if (isEnabled)
            Styling |= flag;
        else
            Styling &= ~flag;
    }

    internal static void ToggleWeatherDisplay()
    {
        ToggleGeneric(DisplayStyle.Weather, out bool isEnabled);
        ToggleWeather.Suffix = $" {FilterMenuBools(isEnabled)}";
    }

    internal static void TogglePriceDisplay()
    {
        ToggleGeneric(DisplayStyle.Price, out bool isEnabled);
        TogglePrice.Suffix = $" {FilterMenuBools(isEnabled)}";
    }

    internal static void ToggleRiskDisplay()
    {
        ToggleGeneric(DisplayStyle.Difficulty, out bool isEnabled);
        ToggleRisk.Suffix = $" {FilterMenuBools(isEnabled)}";
    }
}

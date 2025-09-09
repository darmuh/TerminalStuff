using OpenLib.InteractiveMenus;
using System.Collections.Generic;
using System.Linq;
using TerminalStuff.Configs;
using TerminalStuff.Util;
using static TerminalStuff.MoonsTweaks.MoonsPlus;

namespace TerminalStuff.MoonsTweaks;

public class FilterView
{
    internal bool Weather = true;
    internal bool Price = true;
    internal bool Difficulty = false;
    internal bool RemoveBadWeather = false;
    internal bool RemoveTooExpensive = false;
    public MoonMenuItem MenuItem = null!;

    public string Sorting = "LevelID";

    internal void SetDefaults(string config)
    {
        Weather = false;
        Price = false;
        Difficulty = false;

        if (config.Length < 1)
            return;

        List<string> entries = [.. config.Split(',')];
        foreach (string entry in entries)
        {

            if (entry.Trim() == "weather")
                Weather = true;

            if (entry.Trim() == "price")
                Price = true;

            if (entry.Trim() == "difficulty")
                Difficulty = true;
        }
    }

    internal static void MoonOnTopCheck()
    {
        if (MoonsPlusConfig.ThisAlwaysOnTop.Value.Length < 1)
            return;

        MenuItem thisMoon = ShowMoons.NestedMenus.FirstOrDefault(x => OpenLib.Common.Misc.StringContainsInvariant(x.Name, MoonsPlusConfig.ThisAlwaysOnTop.Value));

        if (thisMoon == null)
        {
            Plugin.Log.LogMessage($"Could not find {MoonsPlusConfig.ThisAlwaysOnTop.Value} in nested menus");
            return;
        }

        int thisIndex = ShowMoons.NestedMenus.IndexOf(thisMoon);
        Loggers.LogDebug($"{MoonsPlusConfig.ThisAlwaysOnTop.Value} = {thisIndex}");
        if (thisIndex > 0)
        {
            ShowMoons.NestedMenus.Remove(thisMoon);
            ShowMoons.NestedMenus.Insert(0, thisMoon);

            Loggers.LogDebug($"{MoonsPlusConfig.ThisAlwaysOnTop.Value} is now first in list!");
        }
    }

    internal static void AssignSorting(string config)
    {
        if (config == "id")
            SortByLevelID();
        else if (config == "alphabetical")
            SortByName();
        else if (config == "price")
            SortByPrice();
        else if (config == "weather")
            SortByWeather();
        else if (config == "difficulty")
            SortByRisk();
        else
            Loggers.WARNING("Failed to assign DefaultSorting value from config!");
    }

    internal static void SortByLevelID()
    {
        MoonsFilter.Sorting = "LevelID";

        if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
            return;

        MoonsPlusMenu.DisplayMenuItemsOfType = [.. MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().OrderBy(x => x.moonInfo.LevelID)];
    }

    internal static void ToggleHideByAffordable()
    {
        MoonsFilter.RemoveTooExpensive = !MoonsFilter.RemoveTooExpensive;
    }

    internal static void ToggleHideByWeather()
    {
        MoonsFilter.RemoveBadWeather = !MoonsFilter.RemoveBadWeather;
    }

    internal static void SortByName()
    {
        if (MoonsFilter.Sorting == "Name (A-Z)")
        {
            MoonsFilter.Sorting = "Name (Z-A)";

            if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
                return;
            MoonsPlusMenu.DisplayMenuItemsOfType = [.. MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().OrderByDescending(x => x.moonInfo.LevelName)];
        }
        else
        {
            if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
                return;
            MoonsFilter.Sorting = "Name (A-Z)";
            MoonsPlusMenu.DisplayMenuItemsOfType = [.. MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().OrderBy(x => x.moonInfo.LevelName)];
        }

    }

    internal static void SortByWeather()
    {

        if (MoonsFilter.Sorting == "Weather (A-Z)")
        {
            MoonsFilter.Sorting = "Weather (Z-A)";
            if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
                return;
            MoonsPlusMenu.DisplayMenuItemsOfType = [.. MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().OrderByDescending(x => GetWeatherName(x.moonInfo.Level))];
        }
        else
        {
            MoonsFilter.Sorting = "Weather (A-Z)";
            if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
                return;
            MoonsPlusMenu.DisplayMenuItemsOfType = [.. MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().OrderBy(x => GetWeatherName(x.moonInfo.Level))];
        }

    }

    internal static void SortByRisk()
    {
        if (MoonsFilter.Sorting == "Risk (A-Z)")
        {
            MoonsFilter.Sorting = "Risk (Z-A)";
            if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
                return;
            MoonsPlusMenu.DisplayMenuItemsOfType = [.. MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().OrderByDescending(x => x.moonInfo.Level.riskLevel)];
        }
        else
        {
            MoonsFilter.Sorting = "Risk (A-Z)";
            if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
                return;
            MoonsPlusMenu.DisplayMenuItemsOfType = [.. MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().OrderBy(x => x.moonInfo.Level.riskLevel)];
        }

    }

    internal static void SortByPrice()
    {
        if (MoonsFilter.Sorting == "Price (Up)")
        {
            MoonsFilter.Sorting = "Price (Down)";
            if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
                return;
            MoonsPlusMenu.DisplayMenuItemsOfType = [.. MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().OrderByDescending(x => x.moonInfo.DisplayPrice)];
        }
        else
        {
            MoonsFilter.Sorting = "Price (Up)";
            if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
                return;
            MoonsPlusMenu.DisplayMenuItemsOfType = [.. MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().OrderBy(x => x.moonInfo.DisplayPrice)];
        }
    }

    internal static void ToggleWeatherDisplay()
    {
        MoonsFilter.Weather = !MoonsFilter.Weather;
        ToggleWeather.Suffix = $" {FilterMenuBools(MoonsFilter.Weather)}";
    }

    internal static void TogglePriceDisplay()
    {
        MoonsFilter.Price = !MoonsFilter.Price;
        TogglePrice.Suffix = $" {FilterMenuBools(MoonsFilter.Price)}";
    }

    internal static void ToggleRiskDisplay()
    {
        MoonsFilter.Difficulty = !MoonsFilter.Difficulty;
        ToggleRisk.Suffix = $" {FilterMenuBools(MoonsFilter.Difficulty)}";
    }

}

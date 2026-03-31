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

    internal static void OrderMenu(ref List<MenuItem> menuItems)
    {
        if(menuItems.Count == 0) return;

        UpdateSorting(Sorting, ref menuItems);
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

        list = [.. menuItems.OfType<MoonMenuItem>()];
        HoveredMoon = null;

        if (list.Count == 0)
            return;

        // null unless assigned successfully
        MoonMenuItem? expectedTop = null;

        // assign expected first level from config value
        if (!string.IsNullOrEmpty(MoonsPlusConfig.ThisAlwaysOnTop.Value))
            expectedTop = list.FirstOrDefault(x => OpenLib.Common.Misc.StringContainsInvariant(x.Name, MoonsPlusConfig.ThisAlwaysOnTop.Value));

        // if not null, remove from list to be added later during sorting
        if (expectedTop != null)
            list.Remove(expectedTop);

        switch (style)
        {
            case SortingStyle.ID_Up:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderBy(x => x.moonInfo.LevelID)] : [.. list.OrderBy(x => x.moonInfo.LevelID)];
                break;
            case SortingStyle.ID_Down:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderByDescending(x => x.moonInfo.LevelID)] : [.. list.OrderByDescending(x => x.moonInfo.LevelID)];
                break;
            case SortingStyle.AlphabeticalUp:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderBy(x => x.moonInfo.LevelName)] : [.. list.OrderBy(x => x.moonInfo.LevelName)];
                break;
            case SortingStyle.AlphabeticalDown:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderBy(x => x.moonInfo.LevelName)] : [.. list.OrderByDescending(x => x.moonInfo.LevelName)];
                break;
            case SortingStyle.WeatherUp:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderBy(x => GetWeatherName(x.moonInfo.Level))] : [.. list.OrderBy(x => GetWeatherName(x.moonInfo.Level))];
                break;
            case SortingStyle.WeatherDown:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderByDescending(x => GetWeatherName(x.moonInfo.Level))] : [.. list.OrderByDescending(x => GetWeatherName(x.moonInfo.Level))];
                break;
            case SortingStyle.DifficultyUp:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderBy(x => x.moonInfo.Level.riskLevel)] : [.. list.OrderBy(x => x.moonInfo.Level.riskLevel)];
                break;
            case SortingStyle.DifficultyDown:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderByDescending(x => x.moonInfo.Level.riskLevel)] : [.. list.OrderByDescending(x => x.moonInfo.Level.riskLevel)];
                break;
            case SortingStyle.PriceUp:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderBy(x => x.moonInfo.DisplayPrice)] : [.. list.OrderBy(x => x.moonInfo.DisplayPrice)];
                break;
            case SortingStyle.PriceDown:
                menuItems = expectedTop != null ? [expectedTop, .. list.OrderByDescending(x => x.moonInfo.DisplayPrice)] : [.. list.OrderByDescending(x => x.moonInfo.DisplayPrice)];
                break;
        }

        if (MoonsPlusMenu.ActiveSelection > 0 && MoonsPlusMenu.ActiveSelection < menuItems.Count)
        {
            if (menuItems[MoonsPlusMenu.ActiveSelection] is MoonMenuItem moonMenu)
                HoveredMoon = moonMenu?.moonInfo;
        }

        MoonsPlusMenu.MenuNode.displayVideo = null;

        if (HoveredMoon != null)
        {
            if (HoveredMoon.Level.videoReel == null)
                return;

            if (HoveredMoon.IsHidden && MoonsPlusConfig.ObscureHiddenInfo.Value)
            {
                if (HiddenClip != null)
                {
                    MoonsPlusMenu.MenuNode.displayVideo = HiddenClip;
                }

                return;
            }

            Loggers.LogDebug($"Hovered Moon video reel - {HoveredMoon.LevelName}");
            MoonsPlusMenu.MenuNode.displayVideo = HoveredMoon.Level.videoReel;
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

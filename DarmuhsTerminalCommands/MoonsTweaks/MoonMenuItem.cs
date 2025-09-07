using HarmonyLib;
using OpenLib.InteractiveMenus;
using System.Collections.Generic;
using System.Linq;
using TerminalStuff.SpecialStuff;
using static OpenLib.Events.Events;
using static TerminalStuff.MoonsTweaks.MoonsPlus;

namespace TerminalStuff.MoonsTweaks;

public class MoonMenuItem(string name) : MenuItem(MoonsPlusMenu)
{
    private string _name = name;
    public override string Name
    {
        get => _name;
        set => _name = value;
    }

    private bool _showEmpty = true;
    public override bool ShowIfEmptyNest
    {
        get => _showEmpty;
        set => _showEmpty = value;
    }

    private CustomEvent _selection = new();
    public override CustomEvent SelectionEvent
    {
        get => _selection;
        set => _selection = value;
    }

    private List<MenuItem> _nested = [];
    public override List<MenuItem> NestedMenus
    {
        get => _nested;
        set => _nested = value;
    }

    public MoonInfo moonInfo = null!;

    public void AddNestedList(List<MoonMenuItem> menuList)
    {

        if (NestedMenus.Count != 0)
        {
            menuList.DoIf(m => !NestedMenus.Any(x => x == m), m => m.SetParentMenu(this));
        }
        else
        {
            menuList.Do(m => m.SetParentMenu(this));
        }
    }

    internal static void CreateFilterMenus() //CHECK THIS
    {
        ToggleWeather = new("Show Weather for Moons:");
        ToggleWeather.SelectionEvent.AddListener(FilterView.ToggleWeatherDisplay);
        ToggleWeather.Suffix = $" {FilterMenuBools(MoonsFilter.Weather)}";
        TogglePrice = new("Show Price for Moons:");
        TogglePrice.SelectionEvent.AddListener(FilterView.TogglePriceDisplay);
        TogglePrice.Suffix = $" {FilterMenuBools(MoonsFilter.Price)}";
        ToggleRisk = new("Show Difficulty for Moons:");
        ToggleRisk.SelectionEvent.AddListener(FilterView.ToggleRiskDisplay);
        ToggleRisk.Suffix = $" {FilterMenuBools(MoonsFilter.Difficulty)}";
        SortLevelID = new("Sort Moons by LevelID");
        SortLevelID.SelectionEvent.AddListener(FilterView.SortByLevelID);
        SortName = new("Sort Moons Alphabetically");
        SortName.SelectionEvent.AddListener(FilterView.SortByName);
        SortPrice = new("Sort Moons by Price:");
        SortPrice.SelectionEvent.AddListener(FilterView.SortByPrice);
        SortWeather = new("Sort Moons by Weather:");
        SortWeather.SelectionEvent.AddListener(FilterView.SortByWeather);
        SortRisk = new("Sort Moons by Difficulty:");
        SortRisk.SelectionEvent.AddListener(FilterView.SortByRisk);
        FilterUnaffordable = new("Hide Unaffordable Moons:");
        FilterUnaffordable.SelectionEvent.AddListener(FilterView.ToggleHideByAffordable);
        FilterWeather = new("Hide Unfavorable Weather");
        FilterWeather.SelectionEvent.AddListener(FilterView.SortByLevelID);
        FilterMain.AddNestedList([ToggleWeather, TogglePrice, ToggleRisk, SortLevelID, SortName, SortPrice, SortWeather, SortRisk, FilterUnaffordable, FilterWeather]);
    }

    internal static MenuItem GetStartMenu()
    {
        //"main", "favs", "change", "help"
        if (MoonsPlusConfig.MenuStartPage.Value == "get moons")
            return ShowMoons;
        else if (MoonsPlusConfig.MenuStartPage.Value == "filter")
            return FilterMain;
        else
            return MoonsMainMenu;
    }
}

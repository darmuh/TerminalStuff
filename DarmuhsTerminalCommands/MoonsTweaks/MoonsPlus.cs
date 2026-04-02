using HarmonyLib;
using OpenLib.CoreMethods;
using OpenLib.InteractiveMenus;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TerminalStuff.CommandHandling;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;
using UnityEngine;
using UnityEngine.Video;

namespace TerminalStuff.MoonsTweaks;

public class MoonsPlus
{
    public static OpenLib.Events.Events.CustomEvent<List<MoonInfo>> UpdateMoonsDisplayed = new();
    internal static BetterMenu<MoonMenuItem> MoonsPlusMenu = new("MoonsPlus");
    internal static CommandManager MoonsCommand = null!;

    internal static TerminalNode OriginalMoonsPage = null!;

    internal static List<MoonInfo> MoonListing { get; set; } = [];
    internal static List<string> AcceptableWeathers = [];
    internal static List<string> MoonsVisited = [];
    internal static List<string> MoonsPurchased = [];

    internal static MoonMenuItem MoonsMainMenu = new("MoonsPlus Menu");
    internal static MoonMenuItem AllMoons = new("All Moons");
    internal static MoonMenuItem GoodWeatherMoons = new("Favorable Weather Moons");
    internal static MoonMenuItem AffordableMoons = new("Affordable Moons");
    internal static MoonMenuItem FilterMain = new("Settings");

    //filter menus
    internal static MoonMenuItem ToggleWeather { get; set; } = null!;
    internal static MoonMenuItem ToggleRisk { get; set; } = null!;
    internal static MoonMenuItem TogglePrice { get; set; } = null!;
    internal static MoonMenuItem SortLevelID { get; set; } = null!;
    internal static MoonMenuItem SortName { get; set; } = null!;
    internal static MoonMenuItem SortWeather { get; set; } = null!;
    internal static MoonMenuItem SortPrice { get; set; } = null!;
    internal static MoonMenuItem SortRisk { get; set; } = null!;

    //Assets
    internal static AssetBundle HiddenAsset = null!;
    internal static VideoClip HiddenClip = null!;
    internal static MoonInfo? CurrentMoon;
    internal static MoonInfo? HoveredMoon;

    //Misc
    internal static bool RunOnce = false; //will only run once per game launch
    public static bool ShowingReel { get; set; } = false;
    public enum StartPage
    {
        Main,
        AllMoons,
        GoodWeatherMoons,
        AffordableMoons,
        Settings
    }

    internal static void SetToVanilla()
    {
        MoonsPlusMenu.IsMenuEnabled = false;
        UnloadAssets();

        if (OriginalMoonsPage == null)
            return;

        if (DynamicBools.TryGetKeyword("moons", out TerminalKeyword Moons))
        {
            Moons.specialKeywordResult = OriginalMoonsPage;
            Loggers.LogDebug("Moons keyword set back to original");
        }
    }

    internal static void LoadAssets()
    {
        string fileName = Assembly.GetExecutingAssembly().GetManifestResourceNames().Single(str => str.EndsWith("hidden169"));

        if (string.IsNullOrEmpty(fileName))
        {
            Loggers.WARNING("Unable to get embedded resource, hidden169!");
            return;
        }
        else
            Loggers.LogDebug($"Found hidden169 @ {fileName}");

        if (HiddenAsset == null)
            HiddenAsset = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream(fileName));

        if (HiddenClip == null)
            HiddenClip = (VideoClip)HiddenAsset.LoadAsset("hidden169.mp4");

    }

    internal static void UnloadAssets()
    {
        if (HiddenAsset == null)
            return;

        HiddenClip = null!;
        HiddenAsset.Unload(true);
    }

    internal static bool IsNodeMoonsPlus(TerminalNode node)
    {
        if(node == null) return false;
        if(MoonsPlusMenu.MenuNode == null) return false;

        return MoonsPlusMenu.MenuNode == node;
    }

    internal static void SetupBetterMenu()
    {
        if (RunOnce)
            return;

        MoonsPlusMenu.MainMenu = MoonsMainMenu;
        MoonsPlusMenu.PageSize = MoonsPlusConfig.MenuPageSize.Value;
        MoonsMainMenu.Header = () => "============= MoonsPlus Main  =============\n\n";
        MoonsMainMenu.OnPageLoad = DisableReel;
        AllMoons.SetParentMenu(MoonsMainMenu);
        AllMoons.Header = () => "============= Select a Moon  =============\n\n";
        AllMoons.Footer = GetMoonsFooter;
        AllMoons.OnPageLoad = AllMoonsOnPageLoad;
        AllMoons.AdjustNestedMenuList.AddListener(FilterView.OrderMenu);
        AffordableMoons.SetParentMenu(MoonsMainMenu);
        AffordableMoons.Header = () => "============= Select a Moon  =============\n\n";
        AffordableMoons.Footer = GetMoonsFooter;
        AffordableMoons.OnPageLoad = AffordableMoonsOnPageLoad;
        AffordableMoons.AdjustNestedMenuList.AddListener(FilterView.OrderMenu);
        GoodWeatherMoons.SetParentMenu(MoonsMainMenu);
        GoodWeatherMoons.Header = () => "============= Select a Moon  =============\n\n";
        GoodWeatherMoons.Footer = GetMoonsFooter;
        GoodWeatherMoons.OnPageLoad = GoodWeatherMoonsOnPageLoad;
        GoodWeatherMoons.AdjustNestedMenuList.AddListener(FilterView.OrderMenu);
        FilterMain.SetParentMenu(MoonsMainMenu);
        FilterMain.Header = () => "===== Settings =====\n\n";
        FilterMain.Footer = GetFilterFooter;
        MoonMenuItem.CreateFilterMenus();
        MoonsPlusMenu.OnExit.AddListener(DisableReel);

        RunOnce = true;
    }

    private static void DisableReel()
    {
        MoonsPlusMenu.MenuNode.displayVideo = null;
        TerminalReelSetDimensions(false);
    }

    private static void CreateMoonInfos()
    {
        List<TerminalNode> allNodes = LogicHandling.GetAllNodes();

        for (int i = 0; i < StartOfRound.Instance.levels.Length; i++)
        {
            Loggers.LogDebug($"MoonsPlusSetup - {StartOfRound.Instance.levels[i].name}");
            if (StartOfRound.Instance.levels[i].name == "LiquidationLevel")
                continue;

            MoonInfo moon = MoonListing.FirstOrDefault(m => m.LevelID == StartOfRound.Instance.levels[i].levelID);

            if (moon != null)
                moon.Reload(StartOfRound.Instance.levels[i], allNodes);
            else
            {
                moon = new(StartOfRound.Instance.levels[i], allNodes);
                MoonListing.Add(moon);
            }

        }

        SaveManager.InitMoonPlusSave();
    }

    internal static void MoonsPlusSetup()
    {
        if (!Commands.TerminalMoonsPlus.Value)
        {
            SetToVanilla();
            return;
        }

        if (StartOfRound.Instance.levels.Length == 0)
            return;

        LoadAssets();
        SetupBetterMenu();

        MoonsPlusMenu.IsMenuEnabled = true;
        MoonsPlusMenu.ActiveSelection = 0;
        MoonsPlusMenu.CurrentPage = 1;

        CreateMoonInfos();

        if (!OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(MoonsPlusConfig.MoonsPlusKeywords.Value).Any(x => x.Equals("moons", System.StringComparison.InvariantCultureIgnoreCase)))
        {
            MoonsCommand.RegisterCommand();
            MoonsPlusMenu.MenuNode = MoonsCommand.terminalNode;
            Plugin.Log.LogMessage("MoonsPlus added without replacing vanilla moons page!");
            return;
        }

        if (DynamicBools.TryGetKeyword("moons", out TerminalKeyword Moons))
        {
            OriginalMoonsPage = Moons.specialKeywordResult;
            MoonsCommand.RegisterCommand(false);
            MoonsPlusMenu.MenuNode = MoonsCommand.terminalNode;
            MoonsPlusMenu.MenuNode.displayText = OriginalMoonsPage.displayText;
            Moons.specialKeywordResult = MoonsPlusMenu.MenuNode;
            Plugin.Log.LogMessage("Moons page replaced with MoonsPlus page!");
        }
        else
            Loggers.ERROR("UNABLE TO GET MOONS KEYWORD FOR MENU!\nUNABLE TO GET MOONS KEYWORD FOR MENU!\nUNABLE TO GET MOONS KEYWORD FOR MENU!");
    }

    internal static string GetFilterFooter()
    {
        TerminalReelSetDimensions(false);
        StringBuilder message = new();
        message.Append($"Currently Sorting by: {FilterView.Sorting}\n");
        message.Append($"Back Menu: [{MoonsPlusMenu.leaveMenu}]    Toggle Setting: [{MoonsPlusMenu.selectMenu}]\n\n");
        return message.ToString();
    }

    internal static string GetMoonsFooter()
    {
        string currentLevel;
        CurrentMoon = MoonListing.FirstOrDefault(x => x.Level == StartOfRound.Instance.currentLevel);

        if (CurrentMoon != null)
        {
            if (CurrentMoon.IsHidden && !MoonsPlusConfig.RevealHiddenOnRoute.Value)
                currentLevel = "?????";
            else if (CurrentMoon.IsCompany)
                currentLevel = "71 Gordion (Company)";
            else
                currentLevel = CurrentMoon.Level.PlanetName;
        }
        else
            currentLevel = StartOfRound.Instance.currentLevel.PlanetName;

        StringBuilder message = new();
        message.Append($"\n\nCurrently Orbiting: {currentLevel}\n\n");
        message.Append($"Page [LeftArrow] < {MoonsPlusMenu.CurrentPage}/{Mathf.CeilToInt((float)MoonsPlusMenu.DisplayMenuItemsOfType.Count / MoonsPlusMenu.PageSize)} > [RightArrow]\n");
        message.Append($"Back Menu: [BackSpace]    Select Moon: [Enter]\n\n");
        return message.ToString();
    }

    internal static string GetWeatherName(SelectableLevel level)
    {
        string levelWeather;
        if (Plugin.instance.WeatherTweaks)
            levelWeather = WeatherTweaksCompat.GetWeather(level);
        else
            levelWeather = $"{level.currentWeather}";

        if (OpenLib.Common.Misc.StringStartsWithInvariant(levelWeather, "none"))
            levelWeather = "";

        Loggers.LogDebug($"{level.PlanetName} weather - {levelWeather}");
        return levelWeather;
    }

    internal static string FilterMenuBools(bool isEnabled)
    {
        if (isEnabled)
            return "<color=#00ab66>Active</color>";
        else
            return "<color=#b22222>Disabled</color>";
    }

    //command return
    internal static string EnterMoonsMenu()
    {
        MoonsPlusMenu.ExitAction = null!;
        MoonsPlusMenu.EnterAtPage(GetStartMenu());
        return "";
    }

    internal static MenuItem GetStartMenu()
    {
        switch (MoonsPlusConfig.MenuStartPage.Value)
        {
            case StartPage.Main:
                return MoonsMainMenu;
            case StartPage.AllMoons:
                return AllMoons;
            case StartPage.GoodWeatherMoons:
                return GoodWeatherMoons;
            case StartPage.AffordableMoons:
                return AffordableMoons;
            case StartPage.Settings:
                return FilterMain;
            default:
                break;
        }

        return MoonsMainMenu;
    }

    private static void UpdateMoonListing(ref MoonMenuItem Parent, bool price = false, bool weather = false)
    {
        Parent.NestedMenus = [];
        HoveredMoon = null!;
        foreach (MoonInfo moon in MoonListing)
        {
            moon.MenuItem.ShowIfEmptyNest = moon.ShowInListing(price, weather);
            Parent.AddNestedItem(moon.MenuItem);
        }

        List<MoonInfo> currentList = [];
        MoonListing.DoIf(x => x.MenuItem.moonInfo != null && x.MenuItem.ShowIfEmptyNest, x => currentList.Add(x));

        UpdateMoonsDisplayed.Invoke(currentList);
    }

    internal static void AllMoonsOnPageLoad()
    {
        UpdateMoonListing(ref AllMoons);
    }

    internal static void AffordableMoonsOnPageLoad()
    {
        UpdateMoonListing(ref AffordableMoons, true);
    }

    internal static void GoodWeatherMoonsOnPageLoad()
    {
        UpdateMoonListing(ref GoodWeatherMoons, false, true);
    }

    internal static bool HijackTerminalImage()
    {
        if (!MoonsPlusConfig.ShowVideoReels.Value)
        {
            Loggers.LogDebug($"Video Reels Disabled! (ShowVideoReels is {MoonsPlusConfig.ShowVideoReels.Value})");
            return false;
        }

        if (MoonsPlusMenu.CurrentMenuItem == MoonsMainMenu || MoonsPlusMenu.CurrentMenuItem == FilterMain || MoonsPlusMenu.CurrentMenuItem == null)
            return false;

        if (MoonsPlusMenu.MenuNode.displayVideo == null)
            return false;

        return true;  
    }

    internal static void TerminalReelSetDimensions(bool activeReel)
    {
        Loggers.LogDebug($"TerminalShowingReel {activeReel}");
        if (activeReel)
        {
            Plugin.instance.Terminal.terminalImage.rectTransform.sizeDelta = new Vector2(200, 150);
            Plugin.instance.Terminal.terminalImage.rectTransform.anchoredPosition = new Vector2(80, 0);
        }
        else
        {
            CamEvents.SetRawImageDimensions(Plugin.instance.Terminal.terminalImage.rectTransform, isFullScreen: true);
        }

        ShowingReel = activeReel;
    }

    internal static void HideLevelFromMapScreen()
    {
        if (!MoonsPlusConfig.ObscureHiddenInfo.Value)
            return;

        MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.Level == StartOfRound.Instance.currentLevel);
        if (currentMoon == null)
            return;

        if (!currentMoon.IsHidden)
            return;

        StartOfRound.Instance.screenLevelVideoReel.enabled = false;
        StartOfRound.Instance.screenLevelVideoReel.gameObject.SetActive(value: false);
        StartOfRound.Instance.screenLevelDescription.text = "\t????????";
    }

    internal static void CheckNodePurchase(TerminalNode node)
    {
        MoonInfo moon = MoonListing.FirstOrDefault(x => x.ResultNode == node);
        if (moon != null)
        {
            moon.OTP = true;
            moon.ResultNode.itemCost = 0;
            moon.PurchaseNode.itemCost = 0;

            moon.UpdateInfo();
        }
    }

    internal static void LobbyClose()
    {
        ClearMoonsVisited();
        MoonListing.Do(x => x.Reset());
        SetToVanilla();
    }

    internal static void ShipReset()
    {
        ClearMoonsVisited();
        MoonListing.Do(x => x.Reset());
    }

    internal static void UpdateMoonTravelHistory(string levelName)
    {
        if (TryGetMoon(levelName, out MoonInfo moon))
        {
            if (MoonsPlusConfig.OneTimePurchase.Value)
                moon.OTP = true;
            moon.UpdateHistory();
        }
    }

    internal static List<string> GetTravelHistory()
    {
        Loggers.LogDebug("GetTravelHistory");
        MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.IsCurrent);

        if (currentMoon != null)
            return [currentMoon.LevelName];
        else
            return [];
    }

    internal static void ClearMoonsVisited()
    {
        MoonsVisited.Clear();
    }

    //public stuff
    public static bool TryGetMoon(string levelName, out MoonInfo moon)
    {
        moon = null!;
        if (MoonListing.Count == 0)
            return false;

        moon = MoonListing.FirstOrDefault(x => x.LevelName.Equals(levelName, System.StringComparison.InvariantCultureIgnoreCase));
        if (moon == null)
            return false;

        return true;
    }

    //Get selectable level's MoonInfo
    public static bool TryGetMoon(SelectableLevel level, out MoonInfo moon)
    {
        moon = null!;
        if (MoonListing.Count == 0)
            return false;

        moon = MoonListing.FirstOrDefault(x => x.Level == level);
        if (moon == null)
            return false;

        return true;
    }

    //Try to get a selectable level from the currently displayed moons
    public static bool TryGetDisplayMoon(SelectableLevel level, out MoonInfo moon)
    {
        moon = null!;
        if (MoonsPlusMenu.DisplayMenuItemsOfType.Count == 0)
            return false;

        moon = MoonsPlusMenu.DisplayMenuItemsOfType.OfType<MoonMenuItem>().FirstOrDefault(x => x.moonInfo.Level == level).moonInfo;

        if (moon == null)
            return false;

        return true;
    }
}

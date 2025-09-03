using HarmonyLib;
using OpenLib.CoreMethods;
using OpenLib.InteractiveMenus;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TerminalStuff.PluginCore;
using TerminalStuff.VisualCore;
using UnityEngine;
using UnityEngine.Video;

namespace TerminalStuff.SpecialStuff;

public class MoonsPlus
{
    public static OpenLib.Events.Events.CustomEvent<List<MoonInfo>> UpdateMoonsDisplayed = new();
    internal static BetterMenu<MoonMenuItem> MoonsPlusMenu = new("MoonsPlus");
    internal static CommandManager MoonsCommand = null!;

    internal static TerminalNode OriginalMoonsPage = null!;

    internal static List<MoonInfo> MoonListing = [];
    internal static List<string> AcceptableWeathers = [];
    internal static List<string> MoonsVisited = [];
    internal static List<string> MoonsPurchased = [];

    internal static MoonMenuItem MoonsMainMenu = new("MoonsPlus Menu");
    internal static MoonMenuItem ShowMoons = new("Moons");
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
    internal static MoonMenuItem FilterUnaffordable { get; set; } = null!;
    internal static MoonMenuItem FilterWeather { get; set; } = null!;

    //menu stuff
    internal static FilterView MoonsFilter = new();

    //Assets
    internal static AssetBundle hiddenAsset = null!;
    internal static VideoClip HiddenClip = null!;

    //Misc
    internal static bool RunOnce = false; //will only run once per game launch
    public static bool ShowingReel = false;

    internal static void SetToVanilla()
    {
        MoonsPlusMenu.IsMenuEnabled = false;
        UnloadAssets();

        if (OriginalMoonsPage == null)
            return;

        if (DynamicBools.TryGetKeyword("moons", out TerminalKeyword Moons))
        {
            Moons.specialKeywordResult = OriginalMoonsPage;
            Plugin.Spam("Moons keyword set back to original");
        }
    }

    internal static void LoadAssets()
    {
        if(hiddenAsset == null)
            hiddenAsset = AssetBundle.LoadFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("TerminalStuff.Assets.hidden169"));
        
        if(HiddenClip == null)
            HiddenClip = (VideoClip)hiddenAsset.LoadAsset("hidden169.mp4");

    }

    internal static void UnloadAssets()
    {
        if (hiddenAsset == null)
            return;

        HiddenClip = null!;
        hiddenAsset.Unload(true);
    }

    internal static void SetupBetterMenu()
    {
        if (RunOnce)
            return;

        MoonsPlusMenu.MainMenu = MoonsMainMenu;
        MoonsPlusMenu.PageSize = 10;
        MoonsMainMenu.Header = () => "============= MoonsPlus Main  =============\r\n\r\n";
        ShowMoons.SetParentMenu(MoonsMainMenu);
        ShowMoons.Header = () => "============= Select a Moon  =============\r\n\r\n";
        ShowMoons.Footer = GetMoonsFooter;
        ShowMoons.OnPageLoad = MoonsOnPageLoad;
        FilterMain.SetParentMenu(MoonsMainMenu);
        FilterMain.Header = () => "===== Settings =====\r\n\r\n";
        FilterMain.Footer = GetFilterFooter;
        MoonMenuItem.CreateFilterMenus();

        RunOnce = true;
    }

    private static void CreateMoonInfos()
    {
        List<TerminalNode> allNodes = LogicHandling.GetAllNodes();

        for (int i = 0; i < StartOfRound.Instance.levels.Length; i++)
        {
            Plugin.Spam($"MoonsPlusSetup - {StartOfRound.Instance.levels[i].name}");
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

        if (!MoonsCommand.KeywordList.Any(x => OpenLib.Common.Misc.CompareStringsInvariant(x, "moons")))
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
            Plugin.ERROR("UNABLE TO GET MOONS KEYWORD FOR MENU!\nUNABLE TO GET MOONS KEYWORD FOR MENU!\nUNABLE TO GET MOONS KEYWORD FOR MENU!");
    }

    internal static string GetFilterFooter()
    {
        StringBuilder message = new();
        message.Append($"Currently Sorting by: {MoonsFilter.Sorting}\r\n");
        message.Append($"Back Menu: [BackSpace]    Toggle Filter: [Enter]\r\n\r\n");
        return message.ToString();
    }

    internal static string GetMainFooter()
    {
        string currentLevel;
        MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.Level == StartOfRound.Instance.currentLevel);

        if (currentMoon != null)
        {
            if (currentMoon.IsHidden && !MoonsPlusConfig.RevealHiddenOnRoute.Value)
                currentLevel = "?????";
            else if (currentMoon.IsCompany)
                currentLevel = "71 Gordion (Company)";
            else
                currentLevel = currentMoon.Level.PlanetName;
        }
        else
            currentLevel = StartOfRound.Instance.currentLevel.PlanetName;

        StringBuilder message = new();
        message.Append($"\r\n\r\nCurrently Orbiting: {currentLevel}\r\n\r\n");
        message.Append($"Page [LeftArrow] < {MoonsPlusMenu.CurrentPage}/{Mathf.CeilToInt((float)MoonsPlusMenu.DisplayMenuItemsOfType.Count / MoonsPlusMenu.PageSize)} > [RightArrow]\r\n");
        message.Append($"Leave Menu: [BackSpace]    Select: [Enter]\r\n\r\n");
        return message.ToString();
    }

    internal static string GetMoonsFooter()
    {
        string currentLevel;
        MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.Level == StartOfRound.Instance.currentLevel);

        if (currentMoon != null)
        {
            if (currentMoon.IsHidden && !MoonsPlusConfig.RevealHiddenOnRoute.Value)
                currentLevel = "?????";
            else if (currentMoon.IsCompany)
                currentLevel = "71 Gordion (Company)";
            else
                currentLevel = currentMoon.Level.PlanetName;
        }
        else
            currentLevel = StartOfRound.Instance.currentLevel.PlanetName;

        StringBuilder message = new();
        message.Append($"\r\n\r\nCurrently Orbiting: {currentLevel}\r\n\r\n");
        message.Append($"Page [LeftArrow] < {MoonsPlusMenu.CurrentPage}/{Mathf.CeilToInt((float)MoonsPlusMenu.DisplayMenuItemsOfType.Count / MoonsPlusMenu.PageSize)} > [RightArrow]\r\n");
        message.Append($"Back Menu: [BackSpace]    Select Moon: [Enter]\r\n\r\n");
        return message.ToString();
    }

    internal static string GetWeatherName(SelectableLevel level)
    {
        string levelWeather;
        if (Plugin.instance.WeatherTweaks)
            levelWeather = WeatherTweaksCompat.GetWeather(level);
        else
            levelWeather = level.currentWeather.ToString();

        if (OpenLib.Common.Misc.StringStartsWithInvariant(levelWeather, "none"))
            levelWeather = "";

        return levelWeather;
    }

    internal static string FilterMenuBools(bool enabled)
    {
        if (enabled)
            return "<color=#00ab66>Active</color>";
        else
            return "<color=#b22222>Disabled</color>";
    }

    //command return
    internal static string EnterMoonsMenu()
    {
        MoonsPlusMenu.EnterAtPage(MoonMenuItem.GetStartMenu());
        return "";
    }

    //only used for moons listing
    internal static void MoonsOnPageLoad()
    {
        // Determine what menu items to show
        FilterView.MoonOnTopCheck();
        MoonListing.Do(x => x.menuItem.ShowIfEmptyNest = x.ShowInListing());

        // Video Reel Section
        VideoReelStuff();
        List<MoonInfo> currentList = [];
        ShowMoons.NestedMenus.FindAll(m => m.ShowIfEmptyNest).OfType<MoonMenuItem>().DoIf(x => x.moonInfo != null, x => currentList.Add(x.moonInfo));

        UpdateMoonsDisplayed.Invoke(currentList);

    }

    internal static void VideoReelStuff()
    {
        MenuItem activeMenu = MoonsPlusMenu.AllMenuItemsOfType.FirstOrDefault(x => x.IsActive);
        if (activeMenu == null)
        {
            Plugin.ERROR("Unable to get activeMenu!");
            return;
        }

        if(activeMenu != ShowMoons)
        {
            ShowReel(false);
            return;
        }

        if (MoonsPlusMenu.ActiveSelection >= MoonsPlusMenu.DisplayMenuItemsOfType.Count || MoonsPlusMenu.ActiveSelection < 0)
            return;

        MoonInfo current = MoonListing.FirstOrDefault(x => x.menuItem == MoonsPlusMenu.DisplayMenuItemsOfType[MoonsPlusMenu.ActiveSelection]);

        if (current == null)
        {
            Plugin.WARNING($"Could not get current moon from active index [{MoonsPlusMenu.ActiveSelection}]");
            return;
        }

        if (current.Level.videoReel != null)
            ShowReel(true);
        else
            ShowReel(false);
    }

    internal static void ShowReel(bool show)
    {
        if (show == ShowingReel)
            return;

        if (!MoonsPlusConfig.ShowVideoReels.Value)
        {
            Plugin.Spam($"Video Reels Disabled! (ShowVideoReels is {MoonsPlusConfig.ShowVideoReels.Value})");
            HideReel();
            return;
        }

        if(MoonsPlusMenu.ActiveSelection >= MoonsPlusMenu.DisplayMenuItemsOfType.Count)
        {
            Plugin.Spam($"Video Reel Disabled! ActiveSelection is greater than or equal to the display items count!");
            HideReel();
            return;
        }

        ShowingReel = show;

        if (show)
        {
            MoonInfo currentMoon = MoonListing.FirstOrDefault(x => x.menuItem == MoonsPlusMenu.DisplayMenuItemsOfType[MoonsPlusMenu.ActiveSelection]);

            if (currentMoon != null)
            {
                if(currentMoon.IsHidden && MoonsPlusConfig.ObscureHiddenInfo.Value)
                {
                    if (HiddenClip != null)
                    {
                        Plugin.instance.Terminal.terminalImage.rectTransform.sizeDelta = new Vector2(200, 150);
                        Plugin.instance.Terminal.terminalImage.rectTransform.anchoredPosition = new Vector2(80, 0);
                        MoonsPlusMenu.MenuNode.displayVideo = HiddenClip;
                        return;
                    }

                    HideReel();
                    return;
                }

                Plugin.instance.Terminal.terminalImage.rectTransform.sizeDelta = new Vector2(200, 150);
                Plugin.instance.Terminal.terminalImage.rectTransform.anchoredPosition = new Vector2(80, 0);
                MoonsPlusMenu.MenuNode.displayVideo = currentMoon.Level.videoReel;
            }
        }
        else
            HideReel();
    }

    internal static void HideReel()
    {
        MoonsPlusMenu.MenuNode.displayVideo = null!;
        CamEvents.SetRawImageDimensions(Plugin.instance.Terminal.terminalImage.rectTransform, isFullScreen: true);
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
        MoonInfo moon = MoonListing.FirstOrDefault(x => x.resultNode == node);
        if (moon != null)
        {
            moon.OTP = true;
            moon.resultNode.itemCost = 0;
            moon.purchaseNode.itemCost = 0;

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
        if(TryGetMoon(levelName, out MoonInfo moon))
        {
            if (MoonsPlusConfig.OneTimePurchase.Value)
                moon.OTP = true;
            moon.UpdateHistory();
        }
    }

    internal static List<string> GetTravelHistory()
    {
        Plugin.Spam("GetTravelHistory");
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
        if(MoonListing.Count == 0)
            return false;

        moon = MoonListing.FirstOrDefault(x => x.Level == level);
        if(moon == null) 
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

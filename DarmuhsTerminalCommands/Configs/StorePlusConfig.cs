using System.Collections.Generic;
using BepInEx.Configuration;
using TerminalStuff.StoreTweaks;
using TerminalStuff.Util;
using static OpenLib.ConfigManager.ConfigSetup;
using static TerminalStuff.StoreTweaks.StorePlus;

namespace TerminalStuff.SpecialStuff;

public class StorePlusConfig
{
    public static ConfigEntry<string> StorePlusKeywords { get; internal set; } = null!;
    public static ConfigEntry<string> AffordableColor { get; internal set; } = null!;
    public static ConfigEntry<string> NotEnoughCredsColor { get; internal set; } = null!;
    public static ConfigEntry<bool> RespectStoreRotation { get; internal set; } = null!;
    public static ConfigEntry<string> DontAddToOther { get; internal set; } = null!;
    public static ConfigEntry<StartingPage> MenuStartPage { get; set; } = null!;
    public static ConfigEntry<int> MenuPageSize { get; internal set; } = null!;
    public static ConfigEntry<string> HideFromStorePlus { get; internal set; } = null!;

    //NEW
    public static ConfigEntry<StoreSettings.SortingStyle> DefaultSortStyle { get; internal set; } = null!;
    public static ConfigEntry<int> DefaultSavings { get; internal set; } = null!;

    internal static List<string> DontAddToOtherListing = [];
    internal static List<string> HideItemListing = [];

    internal static void Init()
    {
        StorePlusKeywords = MakeGeneric(Plugin.instance.Config, "StorePlus", "StorePlusKeywords", "store; shop", "This semi-colon separated list is all keywords that can be used in terminal to return <storeplus> command");
        AffordableColor = MakeGeneric(Plugin.instance.Config, "StorePlus", "AffordableColor", "#00ab66", "The color of store items that you can afford");
        NotEnoughCredsColor = MakeGeneric(Plugin.instance.Config, "StorePlus", "NotEnoughCredsColor", "#b22222", "The color of store items that you CANNOT afford");
        RespectStoreRotation = MakeGeneric(Plugin.instance.Config, "StorePlus", "RespectStoreRotation", false, "Enable this if you want to respect the vanilla store rotation and not allow for purchasing all furniture/suits at all times");
        DontAddToOther = MakeGeneric(Plugin.instance.Config, "StorePlus", "DontAddToOther", "othermenu,constellations", "Comma-separated list of external mod menu keywords that should NOT be added to the other list.\nThe keyword is the word that you would type to enter the other mod's menu");
        MenuStartPage = MakeGeneric(Plugin.instance.Config, "StorePlus", "MenuStartPage", StartingPage.MainMenu, "The menu that will open when first running the moons command.");
        MenuPageSize = MakeGeneric(Plugin.instance.Config, "StorePlus", "MenuPageSize", 10, "The amount of menu items to display per page of this menu.\nNote: Anything over 10 will enable automated scrolling (beta)", 3, 30);
        DefaultSortStyle = MakeGeneric(Plugin.instance.Config, "StorePlus", "DefaultSortingStyle", StoreSettings.SortingStyle.AlphabeticalDown, "Set the default StorePlus setting for sorting store menu items");
        DefaultSavings = MakeGeneric(Plugin.instance.Config, "StorePlus", "DefaultSavings", 0, "Set your default savings value here.");
        HideFromStorePlus = MakeGeneric(Plugin.instance.Config, "StorePlus", "HideFromStorePlus", "", "Comma-separated list of shop items that will be hidden from the StorePlus menu.\nThe names listed here are compared to the terminal node's \"creature name\"\nNOTE: These items will NOT be disabled completely");

        Loggers.LogDebug("StorePlus configs done!");
        UpdateLists();
    }

    internal static void UpdateLists()
    {
        DontAddToOtherListing = OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(DontAddToOther.Value, ',');
        HideItemListing = ["skippedFirstEntryLol", .. OpenLib.Common.CommonStringStuff.GetKeywordsPerConfigItem(HideFromStorePlus.Value, ',')];
    }

}

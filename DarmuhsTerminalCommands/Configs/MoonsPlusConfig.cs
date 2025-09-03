using BepInEx.Configuration;
using static OpenLib.ConfigManager.ConfigSetup;

namespace TerminalStuff.SpecialStuff
{
    public class MoonsPlusConfig
    {
        public static ConfigEntry<string> MoonsPlusKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> DefaultDisplayStyle { get; internal set; } = null!;
        public static ConfigEntry<string> DefaultSorting { get; internal set; } = null!;
        public static ConfigEntry<bool> IncludeHidden { get; internal set; } = null!;
        public static ConfigEntry<bool> IncludeLocked { get; internal set; } = null!;
        public static ConfigEntry<bool> ObscureHiddenInfo { get; internal set; } = null!;
        public static ConfigEntry<bool> RevealHiddenOnRoute { get; internal set; } = null!;
        public static ConfigEntry<string> WeathersToKeep { get; internal set; } = null!;
        public static ConfigEntry<string> ThisAlwaysOnTop { get; internal set; } = null!;
        public static ConfigEntry<bool> ShowVideoReels { get; internal set; } = null!;
        public static ConfigEntry<string> AlwaysHideList { get; internal set; } = null!;
        public static ConfigEntry<bool> UseVanillaPurchaseNodes { get; internal set; } = null!;
        public static ConfigEntry<bool> OneTimePurchase {  get; internal set; } = null!;
        public static ConfigEntry<string> AffordableColor { get; internal set; } = null!;
        public static ConfigEntry<string> NotEnoughCredsColor { get; internal set; } = null!;
        public static ConfigEntry<string> MenuStartPage { get; internal set; } = null!;
        public static ConfigEntry<int> MenuPageSize { get; internal set; } = null!;


        internal static void Init()
        {
            DefaultDisplayStyle = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "DefaultDisplayStyle", "weather,price", "Comma-separated list of what to show by default alongside each moon.\nValid names include: \"weather\", \"price\", and \"difficulty\"");
            DefaultSorting = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "DefaultSorting", "id", "Set the default sorting style, vanilla is \"id\"", new AcceptableValueList<string>("id", "alphabetical", "price", "weather", "difficulty"));
            IncludeHidden = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "IncludeHidden", true, "Decide whether hidden moons should be displayed in the MoonsPlus listing.\nIf enabled, will display hidden moons that have not been traveled to as \"[ ??? ]\" with their associated price/difficulty");
            IncludeLocked = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "IncludeLocked", true, "Decide whether locked moons should be displayed in the MoonsPlus listing.\nIf enabled, will display locked moons as \"[ROUTE LOCKED]\" with no further information.");
            ObscureHiddenInfo = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "ObscureHiddenInfo", false, "When hidden moons are included in the listing, try to obscure info relating to the moon (level reel, ship monitors, etc.)\nRequires Networking to hide for ALL players");
            MoonsPlusKeywords = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "MoonsPlusKeywords", "moons; moonsplus", "This semi-colon separated list is all keywords that can be used in terminal to return <moonsplus> command");
            RevealHiddenOnRoute = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "RevealHiddenOnRoute", false, "Decide whether hidden moons should be unhidden after routing to them.\nIf enabled, will unhide moons that you have been to previously.\nRequires networking");
            WeathersToKeep = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "WeathersToKeep", "fog, rainy", "Comma-separated list of what moons to display when filtering the listing by weather.\nThis will try to match your entry to an existing weather type, (modded weathers are untested).\nIf list is empty, will leave the moons listing unfiltered.\nNOTE: \"none\" is clear weather");
            ThisAlwaysOnTop = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "ThisAlwaysOnTop", "Company", "The moon matching this name will always be listed on top of the listing. Leave blank to not specify a moon to list on top");
            AlwaysHideList = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "AlwaysHideList", "Liquidation, FakeMoonName123", "Comma-separated list of moon names to hide from the list at all time.\n Please use the numberless moon name for this list, if this is not known enable ExtensiveLogging and look for AlwaysHideList Resolution in your logs\nWill also resolve \"Company\" as a valid name to hide");
            ShowVideoReels = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "ShowVideoReels", true, "Decide whether video reels should be displayed in the MoonsPlus menu when a moon has an associated video reel");
            UseVanillaPurchaseNodes = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "UseVanillaPurchaseNodes", false, "This will load the terminalnode for routing to a moon after selection.\nEnable this for better compatibility with mods like LethalMoonUnlocks (or if you just like the vanilla route pages)");
            OneTimePurchase = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "OneTimePurchase", false, "When enabled, will make the Price to route to a moon free after your first purchase\nRequires networking");
            AffordableColor = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "AffordableColor", "#00ab66", "The color of moon routes that you CAN afford, leave blank to not change the color");
            NotEnoughCredsColor = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "NotEnoughCredsColor", "#b22222", "The color of moon routes that you CANNOT afford, leave blank to not change the color");
            MenuStartPage = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "MenuStartPage", "main", "The menu that will open when first running the moons command.", new AcceptableValueList<string>("main", "filter", "get moons"));
            MenuPageSize = MakeGeneric(Plugin.instance.Config, "MoonsPlus", "MenuPageSize", 10, "The amount of menu items to display per page of this menu.\nNote: Anything over 10 will enable automated scrolling (beta)", 3, 30);


            MoonsPlus.MoonsFilter.AssignSorting(DefaultSorting.Value);
            MoonsPlus.MoonsFilter.SetDefaults(DefaultDisplayStyle.Value);
            MoonsPlus.AcceptableWeathers = [.. WeathersToKeep.Value.Split(',')];
            MoonsPlus.AcceptableWeathers = [.. MoonsPlus.AcceptableWeathers.ConvertAll(x => x.Trim().ToLowerInvariant())];

            Plugin.Spam("MoonsPlus config section done");
        }
    }
}

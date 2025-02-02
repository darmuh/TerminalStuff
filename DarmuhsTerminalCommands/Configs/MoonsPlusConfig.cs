using static OpenLib.ConfigManager.ConfigSetup;
using BepInEx.Configuration;

namespace TerminalStuff.SpecialStuff
{
    public class MoonsPlusConfig
    {
        public static ConfigEntry<string> MoonsPlusKeywords { get; internal set; }
        public static ConfigEntry<string> DefaultDisplayStyle { get; internal set; }
        public static ConfigEntry<string> DefaultSorting { get; internal set; }
        public static ConfigEntry<bool> IncludeHidden { get; internal set; }
        public static ConfigEntry<bool> IncludeLocked { get; internal set; }
        public static ConfigEntry<bool> ObscureHiddenInfo { get; internal set; }
        public static ConfigEntry<bool> RevealHiddenOnRoute { get; internal set; }
        public static ConfigEntry<string> WeathersToKeep { get; internal set; }
        public static ConfigEntry<string> ThisAlwaysOnTop { get; internal set; }
        public static ConfigEntry<bool> ShowVideoReels { get; internal set; }
        public static ConfigEntry<string> AlwaysHideList { get; internal set; }
        public static ConfigEntry<bool> UseVanillaPurchaseNodes { get; internal set; }
        public static ConfigEntry<bool> OneTimePurchase {  get; internal set; }
        public static ConfigEntry<string> AffordableColor { get; internal set; }
        public static ConfigEntry<string> NotEnoughCredsColor { get; internal set; }


        internal static void Init()
        {

            DefaultDisplayStyle = MakeString(Plugin.instance.Config, "MoonsPlus", "DefaultDisplayStyle", "weather,price", "Comma-separated list of what to show by default alongside each moon.\nValid names include: \"weather\", \"price\", and \"difficulty\"");
            DefaultSorting = MakeClampedString(Plugin.instance.Config, "MoonsPlus", "DefaultSorting", "id", "Set the default sorting style, vanilla is \"id\"", new AcceptableValueList<string>("id", "alphabetical", "price", "weather", "difficulty"));
            IncludeHidden = MakeBool(Plugin.instance.Config, "MoonsPlus", "IncludeHidden", true, "Decide whether hidden moons should be displayed in the MoonsPlus listing.\nIf enabled, will display hidden moons that have not been traveled to as \"[ ??? ]\" with their associated price/difficulty");
            IncludeLocked = MakeBool(Plugin.instance.Config, "MoonsPlus", "IncludeLocked", true, "Decide whether locked moons should be displayed in the MoonsPlus listing.\nIf enabled, will display locked moons as \"[ROUTE LOCKED]\" with no further information.");
            ObscureHiddenInfo = MakeBool(Plugin.instance.Config, "MoonsPlus", "ObscureHiddenInfo", true, "When hidden moons are included in the listing, try to obscure info relating to the moon (level reel, ship monitors, etc.)");
            MoonsPlusKeywords = MakeString(Plugin.instance.Config, "MoonsPlus", "MoonsPlusKeywords", "moons; moonsplus", "This semi-colon separated list is all keywords that can be used in terminal to return <moonsplus> command");
            RevealHiddenOnRoute = MakeBool(Plugin.instance.Config, "MoonsPlus", "RevealHiddenOnRoute", true, "Decide whether hidden moons should be unhidden after routing to them.\nIf enabled, will unhide moons that you have been to previously.\nNOTE: This currently does not work as intended between saves.");
            WeathersToKeep = MakeString(Plugin.instance.Config, "MoonsPlus", "WeathersToKeep", "fog, rainy", "Comma-separated list of what moons to display when filtering the listing by weather.\nThis will try to match your entry to an existing weather type, (modded weathers are untested).\nIf list is empty, will leave the moons listing unfiltered.\nNOTE: \"none\" is clear weather");
            ThisAlwaysOnTop = MakeString(Plugin.instance.Config, "MoonsPlus", "ThisAlwaysOnTop", "Company", "The moon matching this name will always be listed on top of the listing. Leave blank to not specify a moon to list on top");
            AlwaysHideList = MakeString(Plugin.instance.Config, "MoonsPlus", "AlwaysHideList", "Liquidation, FakeMoonName123", "Comma-separated list of moon names to hide from the list at all time.\n Please use the numberless moon name for this list, if this is not known enable ExtensiveLogging and look for AlwaysHideList Resolution in your logs\nWill also resolve \"Company\" as a valid name to hide");
            ShowVideoReels = MakeBool(Plugin.instance.Config, "MoonsPlus", "ShowVideoReels", true, "Decide whether video reels should be displayed in the MoonsPlus menu when a moon has an associated video reel");
            UseVanillaPurchaseNodes = MakeBool(Plugin.instance.Config, "MoonsPlus", "UseVanillaPurchaseNodes", false, "This will load the terminalnode for routing to a moon after selection.\nEnable this for better compatibility with mods like LethalMoonUnlocks (or if you just like the vanilla route pages)");
            OneTimePurchase = MakeBool(Plugin.instance.Config, "MoonsPlus", "OneTimePurchase", false, "When enabled, will make the Price to route to a moon free after your first purchase");
            AffordableColor = MakeString(Plugin.instance.Config, "MoonsPlus", "AffordableColor", "#00ab66", "The color of moon routes that you CAN afford, leave blank to not change the color");
            NotEnoughCredsColor = MakeString(Plugin.instance.Config, "MoonsPlus", "NotEnoughCredsColor", "#b22222", "The color of moon routes that you CANNOT afford, leave blank to not change the color");


            MoonsPlus.MoonsFilter.AssignSorting(DefaultSorting.Value);
            MoonsPlus.MoonsFilter.SetDefaults(DefaultDisplayStyle.Value);
            MoonsPlus.AcceptableWeathers = [.. WeathersToKeep.Value.Split(',')];
            MoonsPlus.AcceptableWeathers = [.. MoonsPlus.AcceptableWeathers.ConvertAll(x => x.Trim().ToLowerInvariant())];

            Plugin.Spam("MoonsPlus config section done");
        }
    }
}

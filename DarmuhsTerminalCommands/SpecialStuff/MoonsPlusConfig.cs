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
        public static ConfigEntry<bool> RevealHiddenOnRoute { get; internal set; }
        public static ConfigEntry<string> WeathersToKeep { get; internal set; }


        internal static void Init()
        {

            DefaultDisplayStyle = MakeString(Plugin.instance.Config, "MoonsPlus", "DefaultDisplayStyle", "weather,price", "Comma-separated list of what to show by default alongside each moon.\nValid names include: \"weather\", \"price\", and \"difficulty\"");
            DefaultSorting = MakeClampedString(Plugin.instance.Config, "MoonsPlus", "DefaultSorting", "id", "Set the default sorting style, vanilla is \"id\"", new AcceptableValueList<string>("id", "alphabetical", "price", "weather", "difficulty"));
            IncludeHidden = MakeBool(Plugin.instance.Config, "MoonsPlus", "IncludeHidden", true, "Decide whether hidden moons should be displayed in the MoonsPlus listing.\nIf enabled, will display hidden moons that have not been traveled to as \"[ ??? ]\" with their associated price/difficulty");
            IncludeLocked = MakeBool(Plugin.instance.Config, "MoonsPlus", "IncludeLocked", true, "Decide whether locked moons should be displayed in the MoonsPlus listing.\nIf enabled, will display locked moons as \"[ROUTE LOCKED]\" with no further information.");
            MoonsPlusKeywords = MakeString(Plugin.instance.Config, "MoonsPlus", "MoonsPlusKeywords", "moons; moonsplus", "This semi-colon separated list is all keywords that can be used in terminal to return <moonsplus> command");
            RevealHiddenOnRoute = MakeBool(Plugin.instance.Config, "MoonsPlus", "RevealHiddenOnRoute", true, "Decide whether hidden moons should be unhidden after routing to them.\nIf enabled, will unhide moons that you have been to previously.\nNOTE: This currently does not work as intended between saves.");
            WeathersToKeep = MakeString(Plugin.instance.Config, "MoonsPlus", "WeathersToKeep", "fog, rainy", "Comma-separated list of what moons to display when filtering the listing by weather.\nThis will try to match your entry to an existing weather type, (modded weathers are untested).\nIf list is empty, will leave the moons listing unfiltered.\nNOTE: \"none\" is clear weather");

            MoonsPlus.MoonsFilter.AssignSorting(DefaultSorting.Value);
            MoonsPlus.MoonsFilter.SetDefaults(DefaultDisplayStyle.Value);
            MoonsPlus.AcceptableWeathers = [.. WeathersToKeep.Value.Split(',')];
            MoonsPlus.AcceptableWeathers = [.. MoonsPlus.AcceptableWeathers.ConvertAll(x => x.Trim().ToLowerInvariant())];
        }
    }
}

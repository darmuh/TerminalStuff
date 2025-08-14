using BepInEx.Configuration;
using static OpenLib.ConfigManager.ConfigSetup;

namespace TerminalStuff.SpecialStuff
{
    public class StorePlusConfig
    {
        public static ConfigEntry<string> StorePlusKeywords { get; internal set; }
        public static ConfigEntry<string> AffordableColor {  get; internal set; }
        public static ConfigEntry<string> NotEnoughCredsColor { get; internal set; }
        public static ConfigEntry<bool> RespectStoreRotation { get; internal set; }
        public static ConfigEntry<string> DontAddToOtherList { get; internal set; }
        public static ConfigEntry<string> MenuStartPage { get; internal set; }
        public static ConfigEntry<int> MenuPageSize { get; internal set; }

        internal static void Init()
        {
            StorePlusKeywords = MakeString(Plugin.instance.Config, "StorePlus", "StorePlusKeywords", "store; shop", "This semi-colon separated list is all keywords that can be used in terminal to return <storeplus> command");
            AffordableColor = MakeString(Plugin.instance.Config, "StorePlus", "AffordableColor", "#00ab66", "The color of store items that you can afford");
            NotEnoughCredsColor = MakeString(Plugin.instance.Config, "StorePlus", "NotEnoughCredsColor", "#b22222", "The color of store items that you CANNOT afford");
            RespectStoreRotation = MakeBool(Plugin.instance.Config, "StorePlus", "RespectStoreRotation", false, "Enable this if you want to respect the vanilla store rotation and not allow for purchasing all furniture/suits at all times");
            DontAddToOtherList = MakeString(Plugin.instance.Config, "StorePlus", "DontAddToOtherList", "othermenu,constellations", "Comma-separated list of external mod menu keywords that should NOT be added to the other list.\nThe keyword is the word that you would type to enter the other mod's menu");
            MenuStartPage = MakeClampedString(Plugin.instance.Config, "StorePlus", "MenuStartPage", "main", "The menu that will open when first running the moons command.", new AcceptableValueList<string>("main", "upgrades", "items", "vehicles", "packs", "suits", "other", "settings"));
            MenuPageSize = MakeClampedInt(Plugin.instance.Config, "StorePlus", "MenuPageSize", 10, "The amount of menu items to display per page of this menu.\nNote: Anything over 10 will enable automated scrolling (beta)", 3, 30);


            Plugin.Spam("StorePlus configs done!");
        }

    }
}

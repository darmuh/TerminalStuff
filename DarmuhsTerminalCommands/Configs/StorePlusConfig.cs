using static OpenLib.ConfigManager.ConfigSetup;
using BepInEx.Configuration;

namespace TerminalStuff.SpecialStuff
{
    public class StorePlusConfig
    {
        public static ConfigEntry<string> StorePlusKeywords { get; internal set; }
        public static ConfigEntry<string> AffordableColor {  get; internal set; }
        public static ConfigEntry<string> NotEnoughCredsColor { get; internal set; }
        public static ConfigEntry<bool> RespectStoreRotation { get; internal set; }

        internal static void Init()
        {
            StorePlusKeywords = MakeString(Plugin.instance.Config, "StorePlus", "StorePlusKeywords", "store; shop", "This semi-colon separated list is all keywords that can be used in terminal to return <storeplus> command");
            AffordableColor = MakeString(Plugin.instance.Config, "StorePlus", "AffordableColor", "#00ab66", "The color of store items that you can afford");
            NotEnoughCredsColor = MakeString(Plugin.instance.Config, "StorePlus", "NotEnoughCredsColor", "#b22222", "The color of store items that you CANNOT afford");
            RespectStoreRotation = MakeBool(Plugin.instance.Config, "StorePlus", "RespectStoreRotation", false, "Enable this if you want to respect the vanilla store rotation and not allow for purchasing all furniture/suits at all times");

            Plugin.Spam("StorePlus configs done!");
        }

    }
}

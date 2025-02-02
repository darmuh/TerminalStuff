using BepInEx.Configuration;
using static OpenLib.ConfigManager.ConfigSetup;

namespace TerminalStuff.Configs
{
    public class CustomizeConfig
    {
        //page text
        public static ConfigEntry<string> HomeLine1 { get; internal set; }
        public static ConfigEntry<string> HomeLine2 { get; internal set; }
        public static ConfigEntry<string> HomeLine3 { get; internal set; }
        public static ConfigEntry<string> HomeHelpLines { get; internal set; }
        public static ConfigEntry<string> HomeTextArt { get; internal set; }
        public static ConfigEntry<string> MoreMenuText { get; internal set; }
        public static ConfigEntry<string> MoreHintText { get; internal set; }

        //colors
        public static ConfigEntry<bool> AutoResizeMoneyBG { get; internal set; }
        public static ConfigEntry<bool> TerminalCustomization { get; internal set; }
        public static ConfigEntry<string> TerminalColor { get; internal set; }
        public static ConfigEntry<string> TerminalButtonsColor { get; internal set; }
        public static ConfigEntry<string> TerminalKeyboardColor { get; internal set; }
        public static ConfigEntry<string> TerminalTextColor { get; internal set; }
        public static ConfigEntry<string> TerminalMoneyColor { get; internal set; }
        public static ConfigEntry<string> TerminalMoneyBGColor { get; internal set; }
        public static ConfigEntry<float> TerminalMoneyBGAlpha { get; internal set; }
        public static ConfigEntry<string> TerminalCaretColor { get; internal set; }
        public static ConfigEntry<string> TerminalScrollbarColor { get; internal set; }
        public static ConfigEntry<string> TerminalScrollBGColor { get; internal set; }
        public static ConfigEntry<string> TerminalClockColor { get; internal set; }
        public static ConfigEntry<string> TerminalLightColor { get; internal set; }
        public static ConfigEntry<bool> TerminalCustomBG { get; internal set; }
        public static ConfigEntry<string> TerminalCustomBGColor { get; internal set; }
        public static ConfigEntry<float> TerminalCustomBGAlpha { get; internal set; }

        //font stuff
        public static ConfigEntry<string> CustomFontPath { get; internal set; }
        public static ConfigEntry<string> CustomFontName { get; internal set; }
        public static ConfigEntry<int> CustomFontSizeMain { get; internal set; }
        public static ConfigEntry<int> CustomFontSizeMoney { get; internal set; }
        public static ConfigEntry<int> CustomFontSizeClock { get; internal set; }

        internal static void Init()
        {
            //homescreen lines
            HomeLine1 = MakeString(Plugin.instance.Config, "Terminal Customization", "homeline1", $"Welcome to the FORTUNE-9{Plugin.PluginInfo.PLUGIN_VERSION} OS PLUS", "First line of the home command (startup screen)");
            HomeLine2 = MakeString(Plugin.instance.Config, "Terminal Customization", "homeline2", "\tUpgraded by Employee: <color=#e6b800>darmuh</color>", "Second line of the home command (startup screen)");
            HomeLine3 = MakeString(Plugin.instance.Config, "Terminal Customization", "homeline3", "Have a wonderful [currentDay]!", "Last line of the home command (startup screen)");
            HomeHelpLines = MakeString(Plugin.instance.Config, "Terminal Customization", "HomeHelpLines", ">>Type \"Help\" for a list of commands.\r\n>>Type <color=#b300b3>\"More\"</color> for a menu of darmuh's commands.\r\n", "these two lines should generally be used to point to menus of other usable commands. Can also be expanded to more than two lines by using \"\\r\\n\" to indicate a new line");

            HomeTextArt = MakeString(Plugin.instance.Config, "Terminal Customization", "HomeTextArt", "[leadingSpacex4][leadingSpace]<color=#e6b800>^^      .-=-=-=-.  ^^\r\n ^^        (`-=-=-=-=-`)         ^^\r\n         (`-=-=-=-=-=-=-`)  ^^         ^^\r\n   ^^   (`-=-=-=-=-=-=-=-`)   ^^          \r\n       ( `-=-=-=-(@)-=-=-` )      ^^\r\n       (`-=-=-=-=-=-=-=-=-`)  ^^          \r\n       (`-=-=-=-=-=-=-=-=-`)  ^^\r\n        (`-=-=-=-=-=-=-=-`)          ^^\r\n         (`-=-=-=-=-=-=-`)  ^^            \r\n           (`-=-=-=-=-`)\r\n            `-=-=-=-=-`</color>", "ASCII Art goes here");

            MoreMenuText = MakeString(Plugin.instance.Config, "Terminal Customization", "MoreMenuText", "Welcome to darmuh's Terminal Upgrade!\r\n\tSee below Categories for new stuff :)", "This is the header of the more command menu\nRefreshing this text requires a lobby restart.");
            MoreHintText = MakeString(Plugin.instance.Config, "Terminal Customization", "MoreHintText", "<color=#b300b3>>MORE</color>\nTo open a menu of darmuh's commands.\nRefreshing this text requires a lobby restart.", "Text displayed for hints to the more command menu");

            //Terminal Customization
            TerminalCustomization = MakeBool(Plugin.instance.Config, "Terminal Customization", "TerminalCustomization", true, "Enable or Disable terminal color customizations");
            AutoResizeMoneyBG = MakeBool(Plugin.instance.Config, "Terminal Customization", "AutoResizeMoneyBG", true, "Enable or disable auto-resize for the money background.\nRequires TerminalCustomization set to \"true\" and will not work with GeneralImprovements' \"FitCreditsInBackgroundImage\" setting enabled.");
            TerminalColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalColor", "#CFCFCF", "This changes the color of the physical terminal");
            TerminalButtonsColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalButtonsColor", "#B1D2FF", "This changes the color of the physical buttons on the terminal");
            TerminalKeyboardColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalKeyboardColor", "#878787", "This changes the color of the keyboard on the terminal");
            TerminalTextColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalTextColor", "#03E715", "This changes the color of the main text in the terminal");
            TerminalMoneyColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalMoneyColor", "#03E715", "This changes the color of the current credits text in the top left of the terminal");
            TerminalMoneyBGColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalMoneyBGColor", "#064F00", "This changes the color of the current credits text in the top left of the terminal");
            TerminalMoneyBGAlpha = MakeClampedFloat(Plugin.instance.Config, "Terminal Customization", "TerminalMoneyBGAlpha", 0.1f, "This changes the transparency of the money background color.", 0f, 1f);
            TerminalCaretColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalCaretColor", "#00BC0F", "This changes the color of the text caret in the terminal");
            TerminalScrollbarColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalScrollbarColor", "#075200", "This changes the color of the scrollbar in the terminal");
            TerminalScrollBGColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalScrollBGColor", "#075200", "This changes the color of the background box of the scrollbar in the terminal");
            TerminalClockColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalClockColor", "#03E715", "This changes the color of the clock element that is added to the terminal");
            TerminalLightColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalLightColor", "#DBFFBA", "This changes the color of the light that shines from the terminal");
            TerminalCustomBG = MakeBool(Plugin.instance.Config, "Terminal Customization", "TerminalCustomBG", false, "Enable or Disable custom background for the terminal screen");
            TerminalCustomBGColor = MakeString(Plugin.instance.Config, "Terminal Customization", "TerminalCustomBGColor", "#FFFFFF", "This changes the color of the custom background for the terminal screen");
            TerminalCustomBGAlpha = MakeClampedFloat(Plugin.instance.Config, "Terminal Customization", "TerminalCustomBGAlpha", 0.08f, "This changes the transparency of the custom background for the terminal screen", 0f, 1f);

            //Font Stuff
            CustomFontPath = MakeString(Plugin.instance.Config, "Terminal Customization", "CustomFontPath", "fonts", "If you want to share a profile code that includes the font file, put it in a folder with this name in the config folder. The default example would be \"BepInEx\\config\\fonts\"");
            CustomFontName = MakeString(Plugin.instance.Config, "Terminal Customization", "CustomFontName", "", "Name of the custom font you'd like to use in the terminal, leave blank or set to \"default\" to use the normal terminal font");
            CustomFontSizeMain = MakeClampedInt(Plugin.instance.Config, "Terminal Customization", "CustomFontSizeMain", -1, "Set a custom size for your custom font (main text), leave at -1 if you wish not to change it", -1, 72);
            CustomFontSizeMoney = MakeClampedInt(Plugin.instance.Config, "Terminal Customization", "CustomFontSizeMoney", -1, "Set a custom size for your font (credits at the top left), leave at -1 if you wish not to change it\nNOTE: Will not change font size when AutoResizeMoneyBG is enabled", -1, 72);
            CustomFontSizeClock = MakeClampedInt(Plugin.instance.Config, "Terminal Customization", "CustomFontSizeClock", -1, "Set a custom size for your font (TerminalClock), leave at -1 if you wish not to change it", -1, 72);

            Plugin.Spam("Terminal Customization configs section done");
        }
    }
}

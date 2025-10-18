using BepInEx.Configuration;
using TerminalStuff.Util;
using static OpenLib.ConfigManager.ConfigSetup;

namespace TerminalStuff.Configs;

public class CustomizeConfig
{
    //page text
    public static ConfigEntry<string> HomeLine1 { get; internal set; } = null!;
    public static ConfigEntry<string> HomeLine2 { get; internal set; } = null!;
    public static ConfigEntry<string> HomeLine3 { get; internal set; } = null!;
    public static ConfigEntry<string> HomeHelpLines { get; internal set; } = null!;
    public static ConfigEntry<string> HomeTextArt { get; internal set; } = null!;
    
    //More Menu
    public static ConfigEntry<string> MoreMenuMainHeader { get; internal set; } = null!;
    public static ConfigEntry<string> MoreMenuSectionHeader { get; internal set; } = null!;
    public static ConfigEntry<string> MoreMenuCommandHeader { get; internal set; } = null!;
    public static ConfigEntry<string> MoreMenuKeywordsHeader { get; internal set; } = null!;
    public static ConfigEntry<string> MoreMenuInfoHeader { get; internal set; } = null!;
    public static ConfigEntry<string> MoreHintText { get; internal set; } = null!;
    public static ConfigEntry<bool> MoreIncludeCommandInfo { get; internal set; } = null!;
    public static ConfigEntry<bool> MoreIncludeCommandKeywords { get; internal set; } = null!;
    public static ConfigEntry<bool> MoreIncludeCommandRun { get; internal set; } = null!;


    //colors
    public static ConfigEntry<bool> AutoResizeMoneyBG { get; internal set; } = null!;
    public static ConfigEntry<bool> TerminalCustomization
    {
        get => ConfigSettings.TerminalCustomization;
        set => ConfigSettings.TerminalCustomization = value;
    }
    public static ConfigEntry<string> TerminalColor { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalButtonsColor { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalKeyboardColor { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalTextColor { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalMoneyColor { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalMoneyBGColor { get; internal set; } = null!;
    public static ConfigEntry<float> TerminalMoneyBGAlpha { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalCaretColor { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalScrollbarColor { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalScrollBGColor { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalClockColor { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalLightColor { get; internal set; } = null!;
    public static ConfigEntry<bool> TerminalCustomBG { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalCustomBGColor { get; internal set; } = null!;
    public static ConfigEntry<float> TerminalCustomBGAlpha { get; internal set; } = null!;

    //font stuff
    public static ConfigEntry<string> CustomFontPath { get; internal set; } = null!;
    public static ConfigEntry<string> CustomFontName { get; internal set; } = null!;
    public static ConfigEntry<int> CustomFontSizeMain { get; internal set; } = null!;
    public static ConfigEntry<int> CustomFontSizeMoney { get; internal set; } = null!;
    public static ConfigEntry<int> CustomFontSizeClock { get; internal set; } = null!;

    internal static void Init()
    {
        //homescreen lines
        HomeLine1 = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "homeline1", $"Welcome to the FORTUNE-9{MyPluginInfo.PLUGIN_VERSION} OS PLUS", "First line of the home command (startup screen)");
        HomeLine2 = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "homeline2", "\tUpgraded by Employee: <color=#e6b800>darmuh</color>\n", "Second line of the home command (startup screen)");
        HomeLine3 = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "homeline3", "Have a wonderful [currentDay]!", "Last line of the home command (startup screen)");
        HomeHelpLines = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "HomeHelpLines", ">>Type \"Help\" for a list of commands.\n>>Type <color=#b300b3>\"More\"</color> for a menu of darmuh's commands.\n", "these two lines should generally be used to point to menus of other usable commands. Can also be expanded to more than two lines by using \"\\\n\" to indicate a new line");

        HomeTextArt = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "HomeTextArt", "[leadingSpacex4][leadingSpace]<color=#e6b800>^^      .-=-=-=-.  ^^\n ^^        (`-=-=-=-=-`)         ^^\n         (`-=-=-=-=-=-=-`)  ^^         ^^\n   ^^   (`-=-=-=-=-=-=-=-`)   ^^          \n       ( `-=-=-=-(@)-=-=-` )      ^^\n       (`-=-=-=-=-=-=-=-=-`)  ^^          \n       (`-=-=-=-=-=-=-=-=-`)  ^^\n        (`-=-=-=-=-=-=-=-`)          ^^\n         (`-=-=-=-=-=-=-`)  ^^            \n           (`-=-=-=-=-`)\n            `-=-=-=-=-`</color>\n", "ASCII Art goes here");

        MoreHintText = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "MoreHintText", "<color=#b300b3>>MORE</color>\nTo open a menu of darmuh's commands.\nRefreshing this text requires a lobby restart.", "Text displayed for hints to the more command menu");
        MoreMenuMainHeader = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "MoreMenu Main Header", "====== More Commands Menu ======\n\n", "Customize the header of the main menu of the More menu system");
        MoreMenuSectionHeader = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "MoreMenu Section Header", "======== {catName>>} {commands>>} ========\n\n", "Customize the header of the main menu of the More menu system");
        MoreMenuCommandHeader = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "MoreMenu Command Header", "======== {commandName} ========\n\n", "Customize the header for commands in the More menu system");
        MoreMenuKeywordsHeader = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "MoreMenu Keywords Header", "======== {commandName} Keywords ========\n\n", "Customize the header for the keywords listing of a command in the More menu system");
        MoreMenuInfoHeader = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "MoreMenu Info Header", "====== {commandName} Information ======\n\n", "Customize the header for the information of a command in the More menu system");
        MoreIncludeCommandInfo = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "MoreMenu Include Command Info", true, "This determines if more menu generation will try to generate info menu items for each command");
        MoreIncludeCommandKeywords = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "MoreMenu Include Command Keywords", true, "This determines if more menu generation will try to generate a command's keywords as menu items");
        MoreIncludeCommandRun = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "MoreMenu Include Command Run", true, "This determines if more menu generation will try to generate menu items to exit the menu and run the specific command");

        //Terminal Customization
        TerminalCustomization = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalCustomization", true, "Enable or Disable terminal color customizations");
        AutoResizeMoneyBG = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "AutoResizeMoneyBG", true, "Enable or disable auto-resize for the money background.\nRequires TerminalCustomization set to \"true\" and will not work with GeneralImprovements' \"FitCreditsInBackgroundImage\" setting enabled.");
        TerminalColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalColor", "#CFCFCF", "This changes the color of the physical terminal");
        TerminalButtonsColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalButtonsColor", "#B1D2FF", "This changes the color of the physical buttons on the terminal");
        TerminalKeyboardColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalKeyboardColor", "#878787", "This changes the color of the keyboard on the terminal");
        TerminalTextColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalTextColor", "#03E715", "This changes the color of the main text in the terminal");
        TerminalMoneyColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalMoneyColor", "#03E715", "This changes the color of the current credits text in the top left of the terminal");
        TerminalMoneyBGColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalMoneyBGColor", "#064F00", "This changes the color of the current credits text in the top left of the terminal");
        TerminalMoneyBGAlpha = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalMoneyBGAlpha", 0.1f, "This changes the transparency of the money background color.", 0f, 1f);
        TerminalCaretColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalCaretColor", "#00BC0F", "This changes the color of the text caret in the terminal");
        TerminalScrollbarColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalScrollbarColor", "#075200", "This changes the color of the scrollbar in the terminal");
        TerminalScrollBGColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalScrollBGColor", "#075200", "This changes the color of the background box of the scrollbar in the terminal");
        TerminalClockColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalClockColor", "#03E715", "This changes the color of the clock element that is added to the terminal");
        TerminalLightColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalLightColor", "#DBFFBA", "This changes the color of the light that shines from the terminal");
        TerminalCustomBG = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalCustomBG", false, "Enable or Disable custom background for the terminal screen");
        TerminalCustomBGColor = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalCustomBGColor", "#FFFFFF", "This changes the color of the custom background for the terminal screen");
        TerminalCustomBGAlpha = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "TerminalCustomBGAlpha", 0.08f, "This changes the transparency of the custom background for the terminal screen", 0f, 1f);

        //Font Stuff
        CustomFontPath = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "CustomFontPath", "fonts", "If you want to share a profile code that includes the font file, put it in a folder with this name in the config folder. The default example would be \"BepInEx\\config\\fonts\"");
        CustomFontName = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "CustomFontName", "", "Name of the custom font you'd like to use in the terminal, leave blank or set to \"default\" to use the normal terminal font");
        CustomFontSizeMain = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "CustomFontSizeMain", -1, "Set a custom size for your custom font (main text), leave at -1 if you wish not to change it", -1, 72);
        CustomFontSizeMoney = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "CustomFontSizeMoney", -1, "Set a custom size for your font (credits at the top left), leave at -1 if you wish not to change it\nNOTE: Will not change font size when AutoResizeMoneyBG is enabled", -1, 72);
        CustomFontSizeClock = MakeGeneric(Plugin.instance.Config, "Terminal Customization", "CustomFontSizeClock", -1, "Set a custom size for your font (TerminalClock), leave at -1 if you wish not to change it", -1, 72);

        Loggers.LogDebug("Terminal Customization configs section done");
    }
}

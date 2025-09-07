using BepInEx.Configuration;
using TerminalStuff.Util;
using static OpenLib.ConfigManager.ConfigSetup;

namespace TerminalStuff.Configs;

internal class QoLConfig
{
    //Quality of Life features
    public static ConfigEntry<bool> TerminalShortcuts { get; internal set; } = null!; //adds the bind/unbind keywords and checks for shortcuts on keypress
    public static ConfigEntry<bool> TerminalRunDelay { get; internal set; } = null!; //adds both delay related keywords and has coroutine to run the command on a delay
    public static ConfigEntry<bool> LockCameraInTerminal { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalLightBehaviour { get; internal set; } = null!;
    public static ConfigEntry<bool> TerminalAutoComplete { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalAutoCompleteKey { get; internal set; } = null!;
    public static ConfigEntry<int> TerminalAutoCompleteMaxCount { get; internal set; } = null!;
    public static ConfigEntry<bool> TerminalHistory { get; internal set; } = null!;
    public static ConfigEntry<int> TerminalHistoryMaxCount { get; internal set; } = null!;
    public static ConfigEntry<bool> TerminalConflictResolution { get; internal set; } = null!;
    public static ConfigEntry<float> TerminalRadarDefaultZoom { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalFillEmptyText { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalStartPage { get; internal set; } = null!;
    public static ConfigEntry<bool> SaveLastInput { get; internal set; } = null!;
    public static ConfigEntry<int> TerminalInputMaxChars { get; internal set; } = null!;
    public static ConfigEntry<int> TerminalMaxOrderedItems { get; internal set; } = null!;
    public static ConfigEntry<string> TerminalScreen { get; internal set; } = null!;
    public static ConfigEntry<int> StartingCreds { get; internal set; } = null!;
    public static ConfigEntry<bool> ScreenOnWhileDead { get; internal set; } = null!;
    public static ConfigEntry<int> ScreenOffDelay { get; internal set; } = null!;
    public static ConfigEntry<bool> TerminalClock { get; internal set; } = null!; //Clock object itself
    public static ConfigEntry<bool> WalkieTerm { get; internal set; } = null!; //Use walkie at terminal function
    public static ConfigEntry<string> WalkieTermKey { get; internal set; } = null!;
    public static ConfigEntry<string> WalkieTermMB { get; internal set; } = null!;
    public static ConfigEntry<string> KeyActionsConfig { get; internal set; } = null!;
    public static ConfigEntry<bool> CreateMoreMenus { get; internal set; } = null!;
    public static ConfigEntry<bool> FauxMoreMenu { get; internal set; } = null!;

    internal static void Init()
    {
        TerminalClock = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalClock", false, "Enable or Disable the TerminalClock feature in it's entirety");
        WalkieTerm = MakeGeneric(Plugin.instance.Config, "Quality of Life", "WalkieTerm", false, "Enable or Disable the ability to use a walkie from your inventory at the terminal (vanilla method will work regardless)");
        TerminalShortcuts = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalShortcuts", false, "Enable this for the ability to bind commands to any valid key (also enables the \"bind\" & \"unbind\" keywords.");
        TerminalRunDelay = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalRunDelay", false, "Enable this for the ability to run commands on a delay, Adds 2 customizable keywords (DelayKWs/StopDelayKWs)");
        KeyActionsConfig = MakeGeneric(Plugin.instance.Config, "Quality of Life", "KeyActionsConfig", "", "Stored keybinds, don't modify this unless you know what you're doing!");
        CreateMoreMenus = MakeGeneric(Plugin.instance.Config, "Quality of Life", "CreateMoreMenus", true, "Set this to false to remove the More commands menu.\nIf disabled, any command added by this mod will be added to the 'Other' command listing");
        FauxMoreMenu = MakeGeneric(Plugin.instance.Config, "Quality of Life", "FauxMoreMenu", false, "Set this to true to use Faux Keywords for more menu category & next commands.\nCan be used in the case of certain category commands not being created due to existing keywords (ie. fun moon filter from LLL)");

        LockCameraInTerminal = MakeGeneric(Plugin.instance.Config, "Quality of Life", "LockCameraInTerminal", false, "Enable this to lock the player camera to the terminal when it is in use.");
        TerminalLightBehaviour = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalLightBehaviour", "alwayson", "Use this config item to change how the terminal light behaves. Options are 'nochange' which keeps vanilla behaviour, 'disable' which disables this light whenever you use it, and 'alwayson' which will keep the light on as long as the screen is on", new AcceptableValueList<string>("nochange", "disable", "alwayson"));
        TerminalScreen = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalScreen", "inship", "Use this config item to change how the terminal screen behaves. Options are 'nochange' which keeps vanilla behaviour, 'alwayson' which keeps the screen on at all times, 'inship' which will keep the screen on whenever you are in the ship, and 'inuse' which will keep the screen on whenever anyone is using the terminal.", new AcceptableValueList<string>("nochange", "alwayson", "inship", "inuse"));
        TerminalHistory = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalHistory", false, "(Requires TerminalShortcuts feature to function) With this feature enabled, uparrow and downarrow will cycle through a list of previously used commands.");
        TerminalHistoryMaxCount = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalHistoryMaxCount", 9, "Max amount of previous commands to save in TerminalHistory list.", 3, 50);
        TerminalAutoComplete = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalAutoComplete", false, "(Requires TerminalShortcuts feature to function) With this feature enabled, tab key will cycle through a list of matching commands to the current input.");
        TerminalAutoCompleteKey = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalAutoCompleteKey", "Tab", "Key used to activate TerminalAutoComplete feature https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Key.html");
        TerminalAutoCompleteMaxCount = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalAutoCompleteMaxCount", 5, "Max amount of matching commands to store before disabling autocomplete.", 3, 50);
        TerminalConflictResolution = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalConflictResolution", false, "With this feature enabled, terminal command input will be weighted for conflict resolution using the Levenshtein algorithm.");
        TerminalRadarDefaultZoom = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalRadarDefaultZoom", 30f, "The default level zoom for the radar. The lower the number the more zoomed in you'll be.\nDefault is 30", 5f, 60f);
        TerminalFillEmptyText = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalFillEmptyText", "nochange", "AutoFill any node with empty space depending on your desired formatting", new AcceptableValueList<string>("nochange", "fillbottom", "textmiddle", "textbottom"));
        TerminalStartPage = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalStartPage", "Help", "Enter a keyword to load when a player begins using the terminal.\nSet to \"None\" to not load any page!");
        SaveLastInput = MakeGeneric(Plugin.instance.Config, "Quality of Life", "SaveLastInput", true, "Will save the input of the person last using the terminal and add it back when you start using it again");
        StartingCreds = MakeGeneric(Plugin.instance.Config, "Quality of Life", "StartingCreds", -1, "Change this Quality of Life feature from -1 to set your desired starting credits amount\n-1 will leave starting credits unchanged by this mod.\nCapped at 20K, you shouldn't need more than this", -1, 20000);
        TerminalInputMaxChars = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalInputMaxChars", -1, "Change this Quality of Life feature from -1 to set your desired Max Input Length to adjust node's max input that accepts at least 20 characters of input. Setting this to any number below 20 will do nothing.", -1, 999);
        TerminalMaxOrderedItems = MakeGeneric(Plugin.instance.Config, "Quality of Life", "TerminalMaxOrderedItems", 12, "Set your desired Max Number of Items that you can order at one time.(capped at 150 for sanity)\nThe vanilla default is 12 and will remain the minimum value for this config item.\nCAUTION: Do not lower this value while loaded into a lobby, may cause crashes/errors", 12, 150);
        ScreenOnWhileDead = MakeGeneric(Plugin.instance.Config, "Quality of Life", "ScreenOnWhileDead", false, "Set this to true if you wish to keep the screen on after death when TerminalScreen is set to any mode that keeps the screen on.");
        ScreenOffDelay = MakeGeneric(Plugin.instance.Config, "Quality of Life", "ScreenOffDelay", -1, "Set this to delay turning the terminal screen off by this many seconds after leaving the ship.", -1, 30);

        //keybinds
        WalkieTermKey = MakeGeneric(Plugin.instance.Config, "Quality of Life", "WalkieTermKey", "LeftAlt", "Key used to activate your walkie while at the terminal, see here for valid key names https://docs.unity3d.com/Packages/com.unity.inputsystem@1.0/api/UnityEngine.InputSystem.Key.html");
        WalkieTermMB = MakeGeneric(Plugin.instance.Config, "Quality of Life", "WalkieTermMB", "Left", "Mousebutton used to activate your walkie while at the terminal, see here for valid button names https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/api/UnityEngine.InputSystem.LowLevel.MouseButton.html");

        Loggers.LogDebug("Quality of Life configs section done");
    }
}

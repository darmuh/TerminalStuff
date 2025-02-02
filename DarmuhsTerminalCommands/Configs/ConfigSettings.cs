using BepInEx.Configuration;
using OpenLib.ConfigManager;
using OpenLib.CoreMethods;
using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.SpecialStuff;
using static OpenLib.ConfigManager.ConfigSetup;

namespace TerminalStuff
{
    public static class ConfigSettings
    {
        public static MainListing TerminalStuffMain;
        
        public static ConfigEntry<bool> TerminalCustomization
        {
            get
            {
                return CustomizeConfig.TerminalCustomization;
            }
            set
            {
                return;
            }
        }

        //cams special
        public static ConfigEntry<bool> CamsUseDetectedMods { get; internal set; }
        public static ConfigEntry<bool> ObcRequireUpgrade { get; internal set; }
        public static ConfigEntry<string> ObcResolutionMirror { get; internal set; }
        public static ConfigEntry<string> ObcResolutionBodyCam { get; internal set; }
        public static ConfigEntry<float> MirrorZoom { get; internal set; }
        public static ConfigEntry<bool> Mirror2DStyle { get; internal set; }
        public static ConfigEntry<bool> ModNetworking { get; internal set; }
        public static ConfigEntry<bool> NetworkedNodes { get; internal set; } //enable or disable networked terminal nodes (beta)
        
        public static ConfigEntry<bool> ExtensiveLogging { get; internal set; }
        public static ConfigEntry<bool> DeveloperLogging { get; internal set; }

        //Strings for display messages
        public static ConfigEntry<bool> CanOpenDoorInSpace { get; internal set; } //bool to allow for opening door in space
        public static ConfigEntry<string> DoorOpenString { get; internal set; } //Door String
        public static ConfigEntry<string> DoorCloseString { get; internal set; } //Door String
        public static ConfigEntry<string> DoorSpaceString { get; internal set; } //Door String
        public static ConfigEntry<string> QuitString { get; internal set; } //Quit String
        public static ConfigEntry<string> LeverString { get; internal set; } //Lever String
        public static ConfigEntry<string> VideoStartString { get; internal set; } //lol, start video string
        public static ConfigEntry<string> VideoStopString { get; internal set; } //lol, stop video string
        public static ConfigEntry<string> TpMessageString { get; internal set; } //TP Message String
        public static ConfigEntry<string> ItpMessageString { get; internal set; } //TP Message String
        public static ConfigEntry<string> VitalsPoorString { get; internal set; } //Vitals can't afford string
        public static ConfigEntry<string> VitalsUpgradePoor { get; internal set; } //Vitals Upgrade can't afford string
        public static ConfigEntry<string> HealIsFullString { get; internal set; } //full health string
        public static ConfigEntry<string> HealString { get; internal set; } //healing player string
        public static ConfigEntry<string> CamOnString { get; internal set; } //Cameras on string
        public static ConfigEntry<string> CamOffString { get; internal set; } //Cameras off string
        public static ConfigEntry<string> MapOnString { get; internal set; } //map on string
        public static ConfigEntry<string> MapOffString { get; internal set; } //map off string
        public static ConfigEntry<string> OverlayOnString { get; internal set; } //overlay on string
        public static ConfigEntry<string> OverlayOffString { get; internal set; } //overlay off string
        public static ConfigEntry<string> MiniMapOnString { get; internal set; } //minimap on string
        public static ConfigEntry<string> MiniMapOffString { get; internal set; } //minimap off string
        public static ConfigEntry<string> MiniCamsOnString { get; internal set; } //minicam on string
        public static ConfigEntry<string> MiniCamsOffString { get; internal set; } //minicam off string

        //Other config items
        public static ConfigEntry<int> GambleMinimum { get; internal set; } //Minimum amount of credits needed to gamble
        public static ConfigEntry<bool> GamblePityMode { get; internal set; } //enable or disable pity for gamblers
        public static ConfigEntry<int> GamblePityCredits { get; internal set; } //Pity Credits for losers
        public static ConfigEntry<string> GamblePoorString { get; internal set; } //gamble credits too low string
        public static ConfigEntry<string> VideoFolderPath { get; internal set; } //Specify a different folder with videos
        public static ConfigEntry<bool> VideoSync { get; internal set; } //Should videos be synced between players (good for AOD)
        public static ConfigEntry<bool> AlwaysUniqueVideo { get; internal set; }

        public static ConfigEntry<bool> MonitoringNeverHide { get; internal set; }
        public static ConfigEntry<string> MonitoringDefaultView { get; internal set; }
        public static ConfigEntry<int> OverlayOpacity { get; internal set; } //Opacity Percentage for Overlay Cams View
        public static ConfigEntry<string> CustomLink { get; internal set; }
        public static ConfigEntry<string> CustomLink2 { get; internal set; }
        public static ConfigEntry<string> CustomLinkHint { get; internal set; }
        public static ConfigEntry<string> CustomLink2Hint { get; internal set; }
        public static ConfigEntry<string> RouteRandomBannedWeather { get; internal set; }
        public static ConfigEntry<int> RouteRandomCost { get; internal set; }
        public static ConfigEntry<bool> RouteOnlyInCurrentConstellation { get; internal set; }
        public static ConfigEntry<string> PurchasePackCommands { get; internal set; }

        //Cruiser Terminal
        public static ConfigEntry<string> CruiserTerminalFilterType { get; internal set; }
        public static ConfigEntry<string> CruiserKeywordList { get; internal set; }
        //public static ConfigEntry<bool> AddMapObjectsForControlCommands { get; internal set; }

        public static void BindConfigSettings()
        {
            //Network Configs
            ModNetworking = MakeBool(Plugin.instance.Config, "Networking", "ModNetworking", true, "Disable this if you want to disable networking and use this mod as a Client-sided mod");
            NetworkedNodes = MakeBool(Plugin.instance.Config, "Networking", "NetworkedNodes", true, "Enable networked Always-On Display & displaying synced terminal nodes");

            AddManagedBool(NetworkedNodes, defaultManaged, true, "", "");


            ExtensiveLogging = MakeBool(Plugin.instance.Config, "Debug", "ExtensiveLogging", false, "Enable or Disable extensive logging for this mod.");
            DeveloperLogging = MakeBool(Plugin.instance.Config, "Debug", "DeveloperLogging", false, "Enable or Disable developer logging for this mod. (this will fill your log file FAST)");

            //AddMapObjectsForControlCommands = MakeBool(Plugin.instance.Config, "Controls Configuration", "AddMapObjectsForControlCommands", false, "Enable this to create your controls commands with map objects. They will behave in the same way as door/turret/mine codes");
            PurchasePackCommands = MakeString(Plugin.instance.Config, "Comfort Configuration", "PurchasePackCommands", "Essentials:pro,shov,walkie;PortalPack:teleporter,inverse", "List of purchase pack commands to create. Format is command:item1,item2,etc.;next command:item1,item2");

            Plugin.Spam("network configs section done");

            

            Plugin.Spam("keybind configs section done");

            //Cams Mod Config
            CamsUseDetectedMods = MakeBool(Plugin.instance.Config, "Extras Configuration", "CamsUseDetectedMods", true, "With this enabled, this mod will detect if another mod that adds player cams is enabled and use the mod's camera for all cams commands. Currently detects the following: Helmet Cameras by Rick Arg, Body Cameras by Solo, OpenBodyCams by Zaggy1024");

            ObcRequireUpgrade = MakeBool(Plugin.instance.Config, "Extras Configuration", "ObcRequireUpgrade", true, "With this enabled (and CamsUseDetectedMods), cams views will not be available until the bodycam upgrade from OpenBodyCams has been unlocked.");

            RouteRandomBannedWeather = MakeString(Plugin.instance.Config, "Fun Configuration", "RouteRandomBannedWeather", "Eclipsed;Flooded;Foggy", "This semi-colon separated list will be used to exclude moons from the route random command");
            RouteRandomCost = MakeClampedInt(Plugin.instance.Config, "Fun Configuration", "RouteRandomCost", 100, "Flat rate for running the route random command to get a random moon...", 0, 99999);
            RouteOnlyInCurrentConstellation = MakeBool(Plugin.instance.Config, "Fun Configuration", "RouteOnlyInCurrentConstellation", true, "When LethalConstellations mod is present, setting this to true will only choose a random moon within the current constellation");



            Plugin.Spam("cost configs section done");
            KeywordConfigs.Init();

            //String Configs
            DoorOpenString = MakeString(Plugin.instance.Config, "Controls Configuration", "DoorOpenString", "Opening door.", "Message returned on door (open) command.");
            DoorCloseString = MakeString(Plugin.instance.Config, "Controls Configuration", "DoorCloseString", "Closing door.", "Message returned on door (close) command.");
            DoorSpaceString = MakeString(Plugin.instance.Config, "Controls Configuration", "DoorSpaceString", "Can't open doors in space.", "Message returned on door (inSpace) command.");
            CanOpenDoorInSpace = MakeBool(Plugin.instance.Config, "Controls Configuration", "CanOpenDoorInSpace", true, "Allow/Disallow for using the terminal to press the button to open the door in space.\nDoes not change whether the door can actually be opened.");
            QuitString = MakeString(Plugin.instance.Config, "Comfort Configuration", "QuitString", "goodbye!", "Message returned on quit command.");
            LeverString = MakeString(Plugin.instance.Config, "Controls Configuration", "LeverString", "PULLING THE LEVER!!!", "Message returned on lever pull command.");
            VideoStartString = MakeString(Plugin.instance.Config, "Fun Configuration", "VideoStartString", "lol.", "Message displayed when first playing a video.");
            VideoStopString = MakeString(Plugin.instance.Config, "Fun Configuration", "VideoStopString", "No more lol.", "Message displayed if you want to end video playback early.");
            TpMessageString = MakeString(Plugin.instance.Config, "Controls Configuration", "TpMessageString", "Teleport Button pressed.", "Message returned when TP command is run.");
            ItpMessageString = MakeString(Plugin.instance.Config, "Controls Configuration", "ItpMessageString", "Inverse Teleport Button pressed.", "Message returned when ITP command is run.");
            VitalsPoorString = MakeString(Plugin.instance.Config, "Upgrades", "VitalsPoorString", "You can't afford to run this command.", "Message returned when you don't have enough credits to run the <Vitals> command.");
            VitalsUpgradePoor = MakeString(Plugin.instance.Config, "Upgrades", "VitalsUpgradePoor", "You can't afford to upgrade the Vitals Scanner.", "Message returned when you don't have enough credits to unlock the vitals scanner upgrade.");
            HealIsFullString = MakeString(Plugin.instance.Config, "Comfort Configuration", "HealIsFullString", "You are full health!", "Message returned when heal command is run and player is already full health.");
            HealString = MakeString(Plugin.instance.Config, "Comfort Configuration", "HealString", "The terminal healed you?!?", "Message returned when heal command is run and player is healed.");
            CamOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "CamOnString", "(CAMS)", "Message returned when enabling Cams command (cams).");
            CamOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "CamOffString", "Cameras disabled.", "Message returned when disabling Cams command (cams).");
            MapOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "MapOnString", "(MAP)", "Message returned when enabling map command (map).");
            MapOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "MapOffString", "Map View disabled.", "Message returned when disabling map command (map).");
            OverlayOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "OverlayOnString", "(Overlay)", "Message returned when enabling Overlay command (overlay).");
            OverlayOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "OverlayOffString", "Overlay disabled.", "Message returned when disabling Overlay command (overlay).");
            MiniMapOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "MiniMapOnString", "(MiniMap)", "Message returned when enabling minimap command (minimap).");
            MiniMapOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "MiniMapOffString", "MiniMap disabled.", "Message returned when disabling minimap command (minimap).");
            MiniCamsOnString = MakeString(Plugin.instance.Config, "Extras Configuration", "MiniCamsOnString", "(MiniCams)", "Message returned when enabling minicams command (minicams).");
            MiniCamsOffString = MakeString(Plugin.instance.Config, "Extras Configuration", "MiniCamsOffString", "MiniCams disabled.", "Message returned when disabling minicams command (minicams).");

            CustomLink = MakeString(Plugin.instance.Config, "Extras Configuration", "CustomLink", "https://thunderstore.io/c/lethal-company/p/darmuh/darmuhsTerminalStuff/", "URL to send players to when using the \"link\" command.");
            CustomLinkHint = MakeString(Plugin.instance.Config, "Extras Configuration", "CustomLinkHint", "Go to the thunderstore listing for this mod.", "Hint given to players in extras menu for \"link\" command.");
            CustomLink2 = MakeString(Plugin.instance.Config, "Extras Configuration", "CustomLink2", "https://github.com/darmuh/TerminalStuff", "URL to send players to when using the second \"link\" command.");
            CustomLink2Hint = MakeString(Plugin.instance.Config, "Extras Configuration", "CustomLink2Hint", "Go to the github for this mod.", "Hint given to players in extras menu for \"link\" command.");

            //Other configs
            GambleMinimum = MakeInt(Plugin.instance.Config, "Fun Configuration", "GambleMinimum", 0, "Credits needed to start gambling, 0 means you can gamble everything.");
            GamblePityMode = MakeBool(Plugin.instance.Config, "Fun Configuration", "GamblePityMode", false, "Enable Gamble Pity Mode, which gives credits back to those who lose everything.");
            GamblePityCredits = MakeClampedInt(Plugin.instance.Config, "Fun Configuration", "GamblePityCredits", 10, "If Gamble Pity Mode is enabled, specify how much Pity Credits are given to losers. (Max: 60)", 0, 60);
            GamblePoorString = MakeString(Plugin.instance.Config, "Fun Configuration", "GamblePoorString", "You don't meet the minimum credits requirement to gamble.", "Message returned when your credits is less than the GambleMinimum set.");
            VideoFolderPath = MakeString(Plugin.instance.Config, "Fun Configuration", "VideoFolderPath", "darmuh-darmuhsTerminalVideos", "Folder name where videos will be pulled from, needs to be in BepInEx/plugins");
            VideoSync = MakeBool(Plugin.instance.Config, "Fun Configuration", "VideoSync", true, "When networking is enabled, this setting will sync videos being played on the terminal for all players whose terminal screen is on.");
            AlwaysUniqueVideo = MakeBool(Plugin.instance.Config, "Fun Configuration", "AlwaysUniqueVideo", true, "When enabled, this setting will shuffle all of the videos into a list. Each time a video command is run it will play a video from the list until it reaches the end of the list. Then a new re-shuffled list will be created.");
            ObcResolutionMirror = MakeString(Plugin.instance.Config, "Extras Configuration", "ObcResolutionMirror", "1000; 700", "Set the resolution of the Mirror Camera created with OpenBodyCams for darmuhsTerminalStuff");
            ObcResolutionBodyCam = MakeString(Plugin.instance.Config, "Extras Configuration", "ObcResolutionBodyCam", "1000; 700", "Set the resolution of the Body Camera created with OpenBodyCams for darmuhsTerminalStuff");
            MirrorZoom = MakeClampedFloat(Plugin.instance.Config, "Extras Configuration", "MirrorZoom", 3.4f, "Set the mirror zoom level, the higher the value the more zoomed out the mirror will be.\nThis requires [Mirror2DStyle] to be enabled", 0.2f, 9f);
            Mirror2DStyle = MakeBool(Plugin.instance.Config, "Extras Configuration", "Mirror2DStyle", false, "Change whether the mirror will use Orthographic (2D) Styling.\n Old versions of this mod had this enabled by default.");
            MonitoringNeverHide = MakeBool(Plugin.instance.Config, "Extras Configuration", "MonitoringNeverHide", false, "Setting this to true will make it so no command will ever auto-hide any monitoring view.");
            MonitoringDefaultView = MakeClampedString(Plugin.instance.Config, "Extras Configuration", "MonitoringDefaultView", "Map", "Set the default monitoring view to use when using the switch/previous commands and there is no active monitoring view.\nSet to \"None\" to not automatically enable any views when switch/previous commands are used.", new AcceptableValueList<string>("None", "Map", "Cams", "Minicams", "Minimap", "Overlay"));
            OverlayOpacity = MakeClampedInt(Plugin.instance.Config, "Extras Configuration", "OverlayOpacity", 10, "Opacity percentage for Overlay View.", 0, 100);


            CruiserTerminalConfigs();
            QoLConfig.Init();
            CustomizeConfig.Init();
            MoonsPlusConfig.Init();
            StorePlusConfig.Init();
            

            PluginCore.StuffForLibrary.ManualManagedBools(); //add more managedbools that dont come from a specific config item

            Plugin.MoreLogs("end of config setup");

            RemoveOrphanedEntries(Plugin.instance.Config);
            NetworkingCheck(ModNetworking.Value, Plugin.instance.Config, defaultManaged);
            Plugin.Spam("Main config load COMPLETE");
        }

        public static void CruiserTerminalConfigs()
        {
            CruiserTerminalFilterType = MakeClampedString(Plugin.instance.Config, "CruiserTerminal", "Cruiser Terminal Filter Type", "Deny", "Use this to set whether the Cruiser Keyword List is a list of keywords to permit or deny", new AcceptableValueList<string>("Deny", "Permit"));
            CruiserKeywordList = MakeString(Plugin.instance.Config, "CruiserTerminal", "Cruiser Terminal Keyword List", "mirror, reflection, show mirror, restart, tp, use teleporter, teleport, itp, use inverse, inverse, door, lights, toggle lights, lol, play video, route random, random moon, refresh colors, paintme, customize, scolor, fcolor, lever, link, link2, fov, restart, reset", "Comma-separated listing of keywords to permit OR deny depending on Cruiser Terminal Filter Type");
        }
    }
}
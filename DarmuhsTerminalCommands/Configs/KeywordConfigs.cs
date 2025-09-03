using BepInEx.Configuration;
using static OpenLib.ConfigManager.ConfigSetup;

namespace TerminalStuff.Configs
{
    public class KeywordConfigs
    {
        //keywords
        public static ConfigEntry<string> AlwaysOnKeywords { get; internal set; } = null!; //string to match keyword
        public static ConfigEntry<string> MinimapKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> MinicamsKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> OverlayKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> DoorKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> LightsKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> ModsKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> TpKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> ItpKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> SwitchKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> QuitKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> VideoKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> ClearKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> DangerKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> HealKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> LootKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> CamsKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> MapKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> MirrorKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> RandomSuitKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> ClockKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> ListItemsKeywords { get; internal set; } = null!; //List Items Command
        public static ConfigEntry<string> ListScrapKeywords { get; internal set; } = null!; //List Scrap Command
        public static ConfigEntry<string> RandomRouteKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> LobbyKeywords { get; internal set; } = null!; //show lobby name keywords
        public static ConfigEntry<string> RefreshcustomizationKWs { get; internal set; } = null!;
        public static ConfigEntry<string> RadarZoomKWs { get; internal set; } = null!;
        public static ConfigEntry<string> FovKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> KickKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> FcolorKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> GambleKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> LeverKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> ScolorKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> LinkKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> Link2Keywords { get; internal set; } = null!;
        public static ConfigEntry<string> RefundKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> PreviousKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> RestartKeywords { get; internal set; } = null!;
        public static ConfigEntry<string> DelayKWs { get; internal set; } = null!;
        public static ConfigEntry<string> StopDelayKWs { get; internal set; } = null!;

        public static void Init()
        {
            //Keyword configs (multiple per config item)
            AlwaysOnKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "AlwaysOnKeywords", "alwayson;always on", "This semi-colon separated list is all keywords that can be used in terminal to return <alwayson> command");
            CamsKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "CamsKeywords", "cameras; show cams; cams", "This semi-colon separated list is all keywords that can be used in terminal to return <cams> command");
            MapKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "MapKeywords", "show map; map", "Additional This semi-colon separated list is all keywords that can be used in terminal to return <map> command");
            MinimapKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "MinimapKeywords", "minimap; show minimap", "This semi-colon separated list is all keywords that can be used in terminal to return <minimap> command.");
            MinicamsKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "MinicamsKeywords", "minicams; show minicams", "This semi-colon separated list is all keywords that can be used in terminal to return <minicams> command");
            OverlayKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "OverlayKeywords", "overlay; show overlay", "This semi-colon separated list is all keywords that can be used in terminal to return <overlay> command");
            MirrorKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "MirrorKeywords", "mirror; reflection; show mirror", "This semi-colon separated list is all keywords that can be used in terminal to return <cams> command");
            DoorKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "DoorKeywords", "door; toggle door", "This semi-colon separated list is all keywords that can be used in terminal to return <door> command");
            LightsKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "LightsKeywords", "lights; toggle lights", "This semi-colon separated list is all keywords that can be used in terminal to return <lights> command");
            ModsKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "ModsKeywords", "modlist; show mods", "This semi-colon separated list is all keywords that can be used in terminal to return <mods> command");
            TpKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "TpKeywords", "tp; use teleporter; teleport", "This semi-colon separated list is all keywords that can be used in terminal to return <tp> command");
            ItpKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "ItpKeywords", "itp; use inverse; inverse", "This semi-colon separated list is all keywords that can be used in terminal to return <itp> command");
            SwitchKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "SwitchKeywords", "switch; view next", "This semi-colon separated list is all keywords that can be used in terminal to return <switch> command");
            QuitKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "QuitKeywords", "quit;exit;leave", "This semi-colon separated list is all keywords that can be used in terminal to return <quit> command");
            VideoKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "VideoKeywords", "lol; play video", "This semi-colon separated list is all keywords that can be used in terminal to return <video> command");
            ClearKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "ClearKeywords", "clear;wipe", "This semi-colon separated list is all keywords that can be used in terminal to return <clear> command");
            DangerKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "DangerKeywords", "danger;hazard;show danger; show hazard", "This semi-colon separated list is all keywords that can be used in terminal to return <danger> command");
            HealKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "HealKeywords", "heal me; heal", "This semi-colon separated list is all keywords that can be used in terminal to return <heal> command");
            LootKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "LootKeywords", "loot; shiploot", "This semi-colon separated list is all keywords that can be used in terminal to return <loot> command");
            RandomSuitKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "RandomSuitKeywords", "randomsuit; random suit", "This semi-colon separated list is all keywords that can be used in terminal to return <randomsuit> command");
            ClockKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "ClockKeywords", "clock; show clock; time", "This semi-colon separated list is all keywords that can be used in terminal to toggle Terminal Clock display");
            ListItemsKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "ListItemsKeywords", "show items; get items; list items", "This semi-colon separated list is all keywords that can be used in terminal to return <itemlist> command");
            ListScrapKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "ListScrapKeywords", "loot detail; loot list", "This semi-colon separated list is all keywords that can be used in terminal to return <lootlist> command");
            RandomRouteKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "RandomRouteKeywords", "route random; random moon", "This semi-colon separated list is all keywords that can be used in terminal to return <randomRoute> command");
            LobbyKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "LobbyKeywords", "show lobby; lobby name", "This semi-colon separated list is all keywords that can be used in terminal to return <lobby> command");
            RefreshcustomizationKWs = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "RefreshcustomizationKWs", "refresh colors; paintme; customize", "This semi-colon separated list is all keywords that can be used to run the TerminalRefreshCustomization command");
            RadarZoomKWs = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "RadarZoomKWs", "zoom; enhance; radar zoom", "This semi-colon separated list is all keywords that can be used in terminal to return <lootlist> command");
            FcolorKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "FcolorKeywords", "fcolor", "This semi-colon separated list is all keywords that can be used in terminal to return <fcolor> command");
            GambleKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "GambleKeywords", "gamble", "This semi-colon separated list is all keywords that can be used in terminal to return <gamble> command");
            LeverKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "LeverKeywords", "lever", "This semi-colon separated list is all keywords that can be used in terminal to return <lever> command");
            ScolorKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "ScolorKeywords", "scolor", "This semi-colon separated list is all keywords that can be used in terminal to return <scolor> command");
            LinkKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "LinkKeywords", "link", "This semi-colon separated list is all keywords that can be used in terminal to return <link> command");
            Link2Keywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "Link2Keywords", "link2", "This semi-colon separated list is all keywords that can be used in terminal to return <link2> command");
            KickKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "KickKeywords", "kick; italy", "This semi-colon separated list is all keywords that can be used in terminal to return <kick> command");
            FovKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "FovKeywords", "fov", "This semi-colon separated list is all keywords that can be used in terminal to return <fov> command");
            RefundKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "RefundKeywords", "refund; cancel", "This semi-colon separated list is all keywords that can be used in terminal to return <refund> command");
            PreviousKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "PreviousKeywords", "previous; goback", "This semi-colon separated list is all keywords that can be used in terminal to return <previous> command");
            RestartKeywords = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "RestartKeywords", "restart; reset", "This semi-colon separated list is all keywords that can be used in terminal to return <restart> command");
            DelayKWs = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "DelayKWs", "delay; runat", "This semi-colon separated list is all keywords that can be used in terminal to return delayed commands");
            StopDelayKWs = MakeGeneric(Plugin.instance.Config, "Custom Keywords", "StopDelayKWs", "stopdelay; stoprun", "This semi-colon separated list is all keywords that can be used in terminal to return delayed commands");

            Plugin.Spam("keyword configs section done");
            Commands.Init();
        }
    }
}

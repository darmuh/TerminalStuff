using BepInEx.Configuration;
using static OpenLib.ConfigManager.ConfigSetup;

namespace TerminalStuff.Configs
{
    public class KeywordConfigs
    {
        //keywords
        public static ConfigEntry<string> AlwaysOnKeywords { get; internal set; } //string to match keyword
        public static ConfigEntry<string> MinimapKeywords { get; internal set; }
        public static ConfigEntry<string> MinicamsKeywords { get; internal set; }
        public static ConfigEntry<string> OverlayKeywords { get; internal set; }
        public static ConfigEntry<string> DoorKeywords { get; internal set; }
        public static ConfigEntry<string> LightsKeywords { get; internal set; }
        public static ConfigEntry<string> ModsKeywords { get; internal set; }
        public static ConfigEntry<string> TpKeywords { get; internal set; }
        public static ConfigEntry<string> ItpKeywords { get; internal set; }
        public static ConfigEntry<string> SwitchKeywords { get; internal set; }
        public static ConfigEntry<string> QuitKeywords { get; internal set; }
        public static ConfigEntry<string> VideoKeywords { get; internal set; }
        public static ConfigEntry<string> ClearKeywords { get; internal set; }
        public static ConfigEntry<string> DangerKeywords { get; internal set; }
        public static ConfigEntry<string> HealKeywords { get; internal set; }
        public static ConfigEntry<string> LootKeywords { get; internal set; }
        public static ConfigEntry<string> CamsKeywords { get; internal set; }
        public static ConfigEntry<string> MapKeywords { get; internal set; }
        public static ConfigEntry<string> MirrorKeywords { get; internal set; }
        public static ConfigEntry<string> RandomSuitKeywords { get; internal set; }
        public static ConfigEntry<string> ClockKeywords { get; internal set; }
        public static ConfigEntry<string> ListItemsKeywords { get; internal set; } //List Items Command
        public static ConfigEntry<string> ListScrapKeywords { get; internal set; } //List Scrap Command
        public static ConfigEntry<string> RandomRouteKeywords { get; internal set; }
        public static ConfigEntry<string> LobbyKeywords { get; internal set; } //show lobby name keywords
        public static ConfigEntry<string> RefreshcustomizationKWs { get; internal set; }
        public static ConfigEntry<string> RadarZoomKWs { get; internal set; }
        public static ConfigEntry<string> FovKeywords { get; internal set; }
        public static ConfigEntry<string> KickKeywords { get; internal set; }
        public static ConfigEntry<string> FcolorKeywords { get; internal set; }
        public static ConfigEntry<string> GambleKeywords { get; internal set; }
        public static ConfigEntry<string> LeverKeywords { get; internal set; }
        public static ConfigEntry<string> ScolorKeywords { get; internal set; }
        public static ConfigEntry<string> LinkKeywords { get; internal set; }
        public static ConfigEntry<string> Link2Keywords { get; internal set; }
        public static ConfigEntry<string> RefundKeywords { get; internal set; }
        public static ConfigEntry<string> PreviousKeywords { get; internal set; }
        public static ConfigEntry<string> RestartKeywords { get; internal set; }
        public static ConfigEntry<string> DelayKWs { get; internal set; }
        public static ConfigEntry<string> StopDelayKWs { get; internal set; }

        public static void Init()
        {
            //Keyword configs (multiple per config item)
            AlwaysOnKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "AlwaysOnKeywords", "alwayson;always on", "This semi-colon separated list is all keywords that can be used in terminal to return <alwayson> command");
            CamsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "CamsKeywords", "cameras; show cams; cams", "This semi-colon separated list is all keywords that can be used in terminal to return <cams> command");
            MapKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "MapKeywords", "show map; map", "Additional This semi-colon separated list is all keywords that can be used in terminal to return <map> command");
            MinimapKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "MinimapKeywords", "minimap; show minimap", "This semi-colon separated list is all keywords that can be used in terminal to return <minimap> command.");
            MinicamsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "MinicamsKeywords", "minicams; show minicams", "This semi-colon separated list is all keywords that can be used in terminal to return <minicams> command");
            OverlayKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "OverlayKeywords", "overlay; show overlay", "This semi-colon separated list is all keywords that can be used in terminal to return <overlay> command");
            MirrorKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "MirrorKeywords", "mirror; reflection; show mirror", "This semi-colon separated list is all keywords that can be used in terminal to return <cams> command");
            DoorKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "DoorKeywords", "door; toggle door", "This semi-colon separated list is all keywords that can be used in terminal to return <door> command");
            LightsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LightsKeywords", "lights; toggle lights", "This semi-colon separated list is all keywords that can be used in terminal to return <lights> command");
            ModsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ModsKeywords", "modlist; show mods", "This semi-colon separated list is all keywords that can be used in terminal to return <mods> command");
            TpKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "TpKeywords", "tp; use teleporter; teleport", "This semi-colon separated list is all keywords that can be used in terminal to return <tp> command");
            ItpKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ItpKeywords", "itp; use inverse; inverse", "This semi-colon separated list is all keywords that can be used in terminal to return <itp> command");
            SwitchKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "SwitchKeywords", "switch; view next", "This semi-colon separated list is all keywords that can be used in terminal to return <switch> command");
            QuitKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "QuitKeywords", "quit;exit;leave", "This semi-colon separated list is all keywords that can be used in terminal to return <quit> command");
            VideoKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "VideoKeywords", "lol; play video", "This semi-colon separated list is all keywords that can be used in terminal to return <video> command");
            ClearKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ClearKeywords", "clear;wipe", "This semi-colon separated list is all keywords that can be used in terminal to return <clear> command");
            DangerKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "DangerKeywords", "danger;hazard;show danger; show hazard", "This semi-colon separated list is all keywords that can be used in terminal to return <danger> command");
            HealKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "HealKeywords", "heal me; heal", "This semi-colon separated list is all keywords that can be used in terminal to return <heal> command");
            LootKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LootKeywords", "loot; shiploot", "This semi-colon separated list is all keywords that can be used in terminal to return <loot> command");
            RandomSuitKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "RandomSuitKeywords", "randomsuit; random suit", "This semi-colon separated list is all keywords that can be used in terminal to return <randomsuit> command");
            ClockKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ClockKeywords", "clock; show clock; time", "This semi-colon separated list is all keywords that can be used in terminal to toggle Terminal Clock display");
            ListItemsKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ListItemsKeywords", "show items; get items; list items", "This semi-colon separated list is all keywords that can be used in terminal to return <itemlist> command");
            ListScrapKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ListScrapKeywords", "loot detail; loot list", "This semi-colon separated list is all keywords that can be used in terminal to return <lootlist> command");
            RandomRouteKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "RandomRouteKeywords", "route random; random moon", "This semi-colon separated list is all keywords that can be used in terminal to return <randomRoute> command");
            LobbyKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LobbyKeywords", "show lobby; lobby name", "This semi-colon separated list is all keywords that can be used in terminal to return <lobby> command");
            RefreshcustomizationKWs = MakeString(Plugin.instance.Config, "Custom Keywords", "RefreshcustomizationKWs", "refresh colors; paintme; customize", "This semi-colon separated list is all keywords that can be used to run the TerminalRefreshCustomization command");
            RadarZoomKWs = MakeString(Plugin.instance.Config, "Custom Keywords", "RadarZoomKWs", "zoom; enhance; radar zoom", "This semi-colon separated list is all keywords that can be used in terminal to return <lootlist> command");
            FcolorKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "FcolorKeywords", "fcolor", "This semi-colon separated list is all keywords that can be used in terminal to return <fcolor> command");
            GambleKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "GambleKeywords", "gamble", "This semi-colon separated list is all keywords that can be used in terminal to return <gamble> command");
            LeverKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LeverKeywords", "lever", "This semi-colon separated list is all keywords that can be used in terminal to return <lever> command");
            ScolorKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "ScolorKeywords", "scolor", "This semi-colon separated list is all keywords that can be used in terminal to return <scolor> command");
            LinkKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "LinkKeywords", "link", "This semi-colon separated list is all keywords that can be used in terminal to return <link> command");
            Link2Keywords = MakeString(Plugin.instance.Config, "Custom Keywords", "Link2Keywords", "link2", "This semi-colon separated list is all keywords that can be used in terminal to return <link2> command");
            KickKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "KickKeywords", "kick; italy", "This semi-colon separated list is all keywords that can be used in terminal to return <kick> command");
            FovKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "FovKeywords", "fov", "This semi-colon separated list is all keywords that can be used in terminal to return <fov> command");
            RefundKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "RefundKeywords", "refund; cancel", "This semi-colon separated list is all keywords that can be used in terminal to return <refund> command");
            PreviousKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "PreviousKeywords", "previous; goback", "This semi-colon separated list is all keywords that can be used in terminal to return <previous> command");
            RestartKeywords = MakeString(Plugin.instance.Config, "Custom Keywords", "RestartKeywords", "restart; reset", "This semi-colon separated list is all keywords that can be used in terminal to return <restart> command");
            DelayKWs = MakeString(Plugin.instance.Config, "Custom Keywords", "DelayKWs", "delay; runat", "This semi-colon separated list is all keywords that can be used in terminal to return delayed commands");
            StopDelayKWs = MakeString(Plugin.instance.Config, "Custom Keywords", "StopDelayKWs", "stopdelay; stoprun", "This semi-colon separated list is all keywords that can be used in terminal to return delayed commands");

            Plugin.Spam("keyword configs section done");
            Commands.Init();
        }
    }
}

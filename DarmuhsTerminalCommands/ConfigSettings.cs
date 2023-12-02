using BepInEx.Configuration;
using FovAdjust;

namespace TerminalStuff
{
    public static class ConfigSettings
    {
        //establish commands that can be turned on or off here
        public static ConfigEntry<bool> terminalLobby; //lobby name command
        public static ConfigEntry<bool> terminalCams; //cams command
        public static ConfigEntry<bool> terminalQuit; //quit command
        public static ConfigEntry<bool> terminalClear; //clear command
        public static ConfigEntry<bool> terminalLoot; //loot command
        public static ConfigEntry<bool> terminalLol; //lol command
        public static ConfigEntry<bool> terminalHeal; //heal command




        public static void BindConfigSettings()
        {

            Plugin.Log.LogInfo("Binding configuration settings");
            ConfigSettings.terminalLobby = Plugin.instance.Config.Bind<bool>("Commands", "terminalLobby", true, "Enable or Disable command: <Lobby Name>");
            ConfigSettings.terminalQuit = Plugin.instance.Config.Bind<bool>("Commands", "terminalQuit", true, "Enable or Disable command: <Quit>");
            ConfigSettings.terminalClear = Plugin.instance.Config.Bind<bool>("Commands", "terminalClear", true, "Enable or Disable command: <Clear>");
            ConfigSettings.terminalLoot = Plugin.instance.Config.Bind<bool>("Commands", "terminalLoot", true, "Enable or Disable command: <Ship Loot>");
            ConfigSettings.terminalCams = Plugin.instance.Config.Bind<bool>("Commands", "terminalCams", true, "Enable or Disable command: <Cameras>");
            ConfigSettings.terminalLol = Plugin.instance.Config.Bind<bool>("Commands", "terminalLol", false, "Enable or Disable command: <lol>");
            ConfigSettings.terminalHeal = Plugin.instance.Config.Bind<bool>("Commands", "terminalHeal", false, "Enable or Disable command: <Heal>");
        }
    }
}
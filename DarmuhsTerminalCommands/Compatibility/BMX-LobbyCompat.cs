using LobbyCompatibility.Features;
using System;
using System.Reflection;

namespace TerminalStuff.Compatibility;

internal class BMX_LobbyCompat
{
    public static void SetCompat(bool isNetworked)
    {
        Version version = Assembly.GetExecutingAssembly().GetName().Version;

        if (isNetworked)
        {
            PluginHelper.RegisterPlugin(MyPluginInfo.PLUGIN_GUID, version, LobbyCompatibility.Enums.CompatibilityLevel.Everyone, LobbyCompatibility.Enums.VersionStrictness.Patch);
        }
        else
        {
            PluginHelper.RegisterPlugin(MyPluginInfo.PLUGIN_GUID, version, LobbyCompatibility.Enums.CompatibilityLevel.ClientOnly, LobbyCompatibility.Enums.VersionStrictness.Patch);
        }
    }
}


namespace TerminalStuff.PluginCore;
public class Loggers
{
    private static void Log(BepInEx.Logging.LogLevel bepLevel, object data)
    {
        int settingVal = (int)ConfigSettings.LogLevel.Value;
        int messageLevel = (int)bepLevel;

        if (messageLevel > settingVal)
            return;

        Plugin.Log.Log(bepLevel, data);

    }

    internal static void LogDebug(object data)
    {
        Log(BepInEx.Logging.LogLevel.Debug, data);
    }

    internal static void LogInfo(object data)
    {
        Log(BepInEx.Logging.LogLevel.Info, data);
    }

    internal static void LogMessage(object data)
    {
        Log(BepInEx.Logging.LogLevel.Message, data);
    }

    internal static void WARNING(object data)
    {
        Log(BepInEx.Logging.LogLevel.Warning, data);
    }

    internal static void ERROR(object data)
    {
        Log(BepInEx.Logging.LogLevel.Error, data);
    }

    internal static void FATAL(object data)
    {
        Log(BepInEx.Logging.LogLevel.Fatal, data);
    }


}

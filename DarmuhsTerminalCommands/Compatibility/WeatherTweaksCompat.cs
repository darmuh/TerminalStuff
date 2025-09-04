

namespace TerminalStuff.Compatibility;

internal class WeatherTweaksCompat
{
    internal static string GetWeather(SelectableLevel level)
    {
        return WeatherTweaks.Variables.GetPlanetCurrentWeather(level);
    }
}

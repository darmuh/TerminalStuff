using LethalLevelLoader;

namespace TerminalStuff.Compatibility
{
    internal class LLLCompat
    {
        internal static void UpdateLLLFontSize(float fontSize)
        {
            TerminalManager.defaultTerminalFontSize = fontSize;
            Plugin.instance.Terminal.screenText.textComponent.fontSize = TerminalManager.defaultTerminalFontSize;
            Plugin.Spam($"TerminalManager.defaultTerminalFontSize set to {fontSize}!");
        }
    }
}

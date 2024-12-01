using TerminalStuff.EventSub;

namespace TerminalStuff.Compatibility
{
    internal class InteractiveAPI
    {
        internal static void GetITAPIWords()
        {
            Plugin.Spam($"Adding InteractiveTerminalAPI Keywords to otherModWords ({GameStuff.otherModWords.Count}) list!");
            GameStuff.otherModWords.AddRange(InteractiveTerminalAPI.UI.InteractiveTerminalManager.registeredApplications.Keys);
            Plugin.Spam($"New count for otherModWords - {GameStuff.otherModWords.Count}");
        }
        
    }
}

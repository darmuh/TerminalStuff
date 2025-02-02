using TerminalStuff.EventSub;
using static TerminalStuff.SpecialStuff.StoreInfo;

namespace TerminalStuff.Compatibility
{
    internal class InteractiveAPI
    {
        internal static void GetITAPIWords()
        {
            if (!Plugin.instance.ITAPI)
                return;

            Plugin.Spam($"Adding InteractiveTerminalAPI Keywords to otherModWords ({GameStuff.otherModWords.Count}) list!");
            GameStuff.otherModWords.AddRange(InteractiveTerminalAPI.UI.InteractiveTerminalManager.registeredApplications.Keys);
            Plugin.Spam($"New count for otherModWords - {GameStuff.otherModWords.Count}");
        }

        internal static void AddToStorePlus()
        {
            if (!Plugin.instance.ITAPI)
                return;

            Plugin.Spam("Adding InteractiveTerminalAPI menus to storeplus");
            foreach(var pair in InteractiveTerminalAPI.UI.InteractiveTerminalManager.registeredApplications)
            {
                if (pair.Key.ToLower() == "lgu")
                {
                    MakeStoreInfo(pair.Key, "Lategame Upgrades");
                }
                else if (pair.Key.ToLower() == "ship")
                {
                    MakeStoreInfo(pair.Key, "Ship Inventory");
                }
                else if (pair.Key.ToLower() == "contracts")
                {
                    MakeStoreInfo(pair.Key, "Lategame Contracts");
                }
                else if (pair.Key.ToLower() == "contract" || pair.Key.ToLower() == "lategame store")
                    continue;
                else
                    MakeStoreInfo(pair.Key);
            }
        }

        
        
    }
}

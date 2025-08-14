using TerminalStuff.EventSub;
using TerminalStuff.SpecialStuff;

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
                if (pair.Key.ToLower() == "lgu" && !StoreMenuItem.DoesMenuItemExist(pair.Key))
                {
                    _ = new StoreMenuItem("Lategame Upgrades", pair.Key, StorePlus.ExternalMods);
                }
                else if (pair.Key.ToLower() == "ship" && !StoreMenuItem.DoesMenuItemExist(pair.Key))
                {
                    _ = new StoreMenuItem("Ship Inventory", pair.Key, StorePlus.ExternalMods);
                }
                else if (pair.Key.ToLower() == "contracts" && !StoreMenuItem.DoesMenuItemExist(pair.Key))
                {
                    _ = new StoreMenuItem("Lategame Contracts", pair.Key, StorePlus.ExternalMods);
                }
                else if (pair.Key.ToLower() == "contract" || pair.Key.ToLower() == "lategame store")
                    continue;
                else
                {
                    if (!StoreMenuItem.DoesMenuItemExist(pair.Key))
                    {
                        _ = new StoreMenuItem(pair.Key, pair.Key, StorePlus.ExternalMods);
                    }
                }
                    
            }
        }

        
        
    }
}

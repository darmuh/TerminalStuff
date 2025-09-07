using TerminalStuff.EventSub;
using TerminalStuff.StoreTweaks;
using TerminalStuff.Util;

namespace TerminalStuff.Compatibility;

internal class InteractiveAPI
{
    internal static void GetITAPIWords()
    {
        if (!Plugin.instance.ITAPI)
            return;

        Loggers.LogDebug($"Adding InteractiveTerminalAPI Keywords to otherModWords ({GameStuff.otherModWords.Count}) list!");
        GameStuff.otherModWords.AddRange(InteractiveTerminalAPI.UI.InteractiveTerminalManager.registeredApplications.Keys);
        Loggers.LogDebug($"New count for otherModWords - {GameStuff.otherModWords.Count}");
    }

    internal static void AddToStorePlus()
    {
        if (!Plugin.instance.ITAPI)
            return;

        Loggers.LogDebug("Adding InteractiveTerminalAPI menus to storeplus");
        foreach (var pair in InteractiveTerminalAPI.UI.InteractiveTerminalManager.registeredApplications)
        {
            if (OpenLib.Common.Misc.CompareStringsInvariant(pair.Key, "lgu") && !StoreMenuItem.DoesMenuItemExist(pair.Key))
            {
                _ = new StoreMenuItem("Lategame Upgrades", pair.Key, StorePlus.ExternalMods);
            }
            else if (OpenLib.Common.Misc.CompareStringsInvariant(pair.Key, "ship") && !StoreMenuItem.DoesMenuItemExist(pair.Key))
            {
                _ = new StoreMenuItem("Ship Inventory", pair.Key, StorePlus.ExternalMods);
            }
            else if (OpenLib.Common.Misc.CompareStringsInvariant(pair.Key, "contracts") && !StoreMenuItem.DoesMenuItemExist(pair.Key))
            {
                _ = new StoreMenuItem("Lategame Contracts", pair.Key, StorePlus.ExternalMods);
            }
            else if (OpenLib.Common.Misc.CompareStringsInvariant(pair.Key, "contract") || OpenLib.Common.Misc.CompareStringsInvariant(pair.Key, "lategame store"))
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

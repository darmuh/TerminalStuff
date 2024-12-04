using GameNetcodeStuff;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalStuff.SpecialStuff;
using UnityEngine;
using static OpenLib.CoreMethods.AddingThings;
using static TerminalStuff.PluginCore.TerminalCustomizer;
using static TerminalStuff.StringStuff;
using static TerminalStuff.MoreCamStuff;

namespace TerminalStuff
{

    public static class TerminalEvents
    {
        internal static string lastText = "";
        internal static string VideoErrorMessage = "";
        public static bool clockDisabledByCommand = false;
        internal static TerminalSettings terminalSettings = new();
        internal static bool quitTerminalEnum = false;

        internal static void StorePacks()
        {
            if (!ConfigSettings.TerminalPurchasePacks.Value)
                return;

            if (ConfigSettings.PurchasePackCommands.Value == "")
                return;

            Dictionary<string, string> keywordAndItems = GetKeywordAndItemNames(ConfigSettings.PurchasePackCommands.Value);

            if (keywordAndItems.Count == 0)
                return;

            foreach (KeyValuePair<string, string> item in keywordAndItems)
            {
                Plugin.Spam($"setting {item.Key} keyword to purchase pack with items: {item.Value}");
                AddNodeManual($"{item.Key}_PP", item.Key, CostCommands.AskPurchasePack, true, 2, ConfigSettings.TerminalStuffMain, 0, CostCommands.CompletePurchasePack, null, "", $"You have cancelled the purchase of Purchase Pack [{item.Key}].\r\n\r\n", true, 0, item.Key, true, item.Value);
            }
        }

        internal static TerminalNode GetNodeFromList(string query, Dictionary<string, TerminalNode> nodeListing)
        {
            foreach (KeyValuePair<string, TerminalNode> pairValue in nodeListing)
            {
                if (pairValue.Key == query)
                {
                    return pairValue.Value;
                }
            }
            return null; // No matching command found for the given query
        }
        internal static string RandomSuit()
        {
            SuitCommands.GetRandomSuit(out string suitString);
            return suitString;
        }
        internal static string QuitTerminalCommand()
        {
            string text = $"{ConfigSettings.QuitString.Value}";        

            Plugin.instance.Terminal.StartCoroutine(TerminalQuitter(Plugin.instance.Terminal));
            return text;
        }
        internal static IEnumerator TerminalQuitter(Terminal terminal)
        {
            if (quitTerminalEnum)
                yield break;

            if (Plugin.instance.CruiserTerm)
            {
                if (Compatibility.CruiserTerm.Status())
                {
                    quitTerminalEnum = true;
                    yield return new WaitForSeconds(0.5f);
                    Compatibility.CruiserTerm.Quit();
                    quitTerminalEnum = false;
                    yield break;
                }
            }

            quitTerminalEnum = true;
            yield return new WaitForSeconds(0.5f);
            terminal.QuitTerminal();
            quitTerminalEnum = false;
        }

        internal static string ClockToggle()
        {
            if (TerminalClockStuff.textComponent == null)
                return "Unable to find Terminal Clock component!\r\n\r\n";

            if (StartOfRound.Instance.inShipPhase)
                return "Unable to determine time zone while in Orbit!\r\n\r\n";

            if (!clockDisabledByCommand && TerminalClockStuff.IsClockVisible())
            {
                Plugin.Spam("Disabling clock via command!");
                clockDisabledByCommand = true;
                TerminalClockStuff.SetClockVisible(false);
                return "Terminal Clock [DISABLED].\r\n";
            }
            else
            {
                Plugin.Spam("Enabling clock via command!");
                clockDisabledByCommand = false;
                TerminalClockStuff.SetClockVisible(true);
                return "Terminal Clock [ENABLED].\r\n";
            }
        }

        internal static string GetCleanedScreenText(Terminal __instance)
        {
            string s = __instance.screenText.text.Substring(__instance.screenText.text.Length - __instance.textAdded);

            return RemovePunctuation(s);
        }

        private static string RemovePunctuation(string s) //copied from game files
        {
            StringBuilder stringBuilder = new();
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                {
                    stringBuilder.Append(c);
                }
            }

            return stringBuilder.ToString().ToLower();
        }

        internal static void ShouldLockPlayerCamera(bool value, PlayerControllerB localPlayer)
        {
            if (!ConfigSettings.LockCameraInTerminal.Value)
                return;

            if (localPlayer != null)
            {
                localPlayer.disableLookInput = !value;
                Plugin.MoreLogs($"ShouldLockPlayerCamera set to: {!value}");
            }
        }

        internal static void ShouldDisableTerminalLight(bool value, string setting)
        {
            if (setting == "nochange")
                return;

            if (Plugin.instance.Terminal.terminalLight.enabled == value)
            {
                Plugin.instance.Terminal.terminalLight.enabled = !value;
                Plugin.MoreLogs($"terminalLight set to {!value} for setting: {setting}");
            }

        }

        internal static string RefreshCustomizationCommand()
        {
            string text = $"Refreshing TerminalCustomization from config.\n\n";
            TerminalCustomization();
            return text;
        }

        internal static int PlayerNameToTarget(string query, List<TransformAndName> radarTargets)
        {
            query = query.TrimStart();

            if (query.Length <= 2) //too short to compare names
                return -1;

            Dictionary<int, int> nameToScore = [];

            for (int i = 0; i < radarTargets.Count; i++) //iterate through all targets
            {
                if (TargetIsValid(radarTargets[i]?.transform)) //verify target is valid
                {
                    if (radarTargets[i].name.ToLower().StartsWith(query.ToLower().Substring(0, 2))) //still need to test this
                    {
                        int score = Levenshtein.Distance(query, radarTargets[i].name); //get score at current target
                        Plugin.Spam($"TargetNum {i} has score {score}");
                        nameToScore.Add(i, score); //map score to current target
                    }
                    else
                        Plugin.Spam($"name [ {radarTargets[i].name.ToLower()} ] does not match start of query [ {query.ToLower().Substring(0, 2)} ]");
                }
            }

            if (nameToScore.Count == 0)
                return -1;

            return nameToScore.OrderBy(x => x.Value).First().Key; //order by score values and return targetnum with highest score
        }

        internal static string QueryToPlayerName(string query, List<TransformAndName> radarTargets)
        {
            query = query.TrimStart();

            if (query.Length <= 2) //too short to compare names
                return "";

            Dictionary<string, int> nameToScore = [];

            for (int i = 0; i < radarTargets.Count; i++) //iterate through all targets
            {
                if (TargetIsValid(radarTargets[i]?.transform)) //verify target is valid
                {
                    if (radarTargets[i].name.ToLower().StartsWith(query.ToLower().Substring(0, 2))) //still need to test this
                    {
                        int score = Levenshtein.Distance(query, radarTargets[i].name); //get score at current target
                        Plugin.Spam($"TargetNum {i} has score {score}");
                        nameToScore.Add(radarTargets[i].name, score); //map score to current target
                    }
                    else
                        Plugin.Spam($"name [ {radarTargets[i].name.ToLower()} ] does not match start of query [ {query.ToLower().Substring(0, 2)} ]");
                }
            }

            if (nameToScore.Count == 0)
                return "";

            return nameToScore.OrderBy(x => x.Value).First().Key; //order by score values and return targetnum with highest score
        }
    }

    internal class TerminalSettings
    {
        internal TerminalNode startPage;
        internal string startPageValue;

        internal void StartPage(string entry)
        {
            startPageValue = entry;
            startPage = null;
            if (startPageValue.ToLower() == "none" || startPageValue.Length < 2)
                return;

            if (OpenLib.CoreMethods.DynamicBools.TryGetKeyword(startPageValue, out TerminalKeyword keyword))
            {
                startPage = keyword.specialKeywordResult;
            }
            else
                Plugin.WARNING($"Unable to find matching keyword for start page - {entry}");

            //"None", "Home", "Moons", "Store", "Help", "Other", "Bestiary", "Storage", "Sigurd", "Video"
        }
    }
}


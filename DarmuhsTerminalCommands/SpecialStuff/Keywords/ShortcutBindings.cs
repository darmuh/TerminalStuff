using OpenLib.CoreMethods;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TerminalStuff.Configs;
using TerminalStuff.PluginCore;
using Key = UnityEngine.InputSystem.Key;

namespace TerminalStuff;

public class ShortcutBindings
{
    // Define a dictionary to map keys to actions
    internal static Dictionary<Key, string> keyActions = [];
    internal static List<Key> invalidKeys = [];
    internal static Key keyBeingPressed;
    public static bool stopForAnyReason = false;

    internal static void InitSavedShortcuts()
    {
        if (!QoLConfig.TerminalShortcuts.Value)
            return;

        Loggers.LogInfo("Loading shortcuts from config");
        invalidKeys = [
        Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J,
        Key.K, Key.L, Key.M, Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T,
        Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z, Key.Space
        ];
        DeserializeKeyActions(QoLConfig.KeyActionsConfig.Value);
        InitTerminalHistoryBinds();
        InitTerminalAutoCompleteBinds();

    }

    private static void InitTerminalHistoryBinds()
    {
        if (!QoLConfig.TerminalHistory.Value)
            return;

        if (keyActions.ContainsKey(Key.UpArrow))
            keyActions.Remove(Key.UpArrow);

        if (keyActions.ContainsKey(Key.DownArrow))
            keyActions.Remove(Key.DownArrow);

        keyActions.Add(Key.UpArrow, "[historyPrevious]");
        Loggers.LogDebug("historyPrevious bind set to Key: UpArrow");
        keyActions.Add(Key.DownArrow, "[historyNext]");
        Loggers.LogDebug("historyNext bind set to Key: UpArrow");

    }

    private static void InitTerminalAutoCompleteBinds()
    {
        if (!QoLConfig.TerminalAutoComplete.Value)
            return;

        keyActions.Add(GetAutoCompleteKey(), "[autocomplete]");
    }

    private static Key GetAutoCompleteKey()
    {
        if (Enum.TryParse(QoLConfig.TerminalAutoCompleteKey.Value, out Key keyFromString))
        {
            if (keyFromString.Equals(Key.Tab))
                Plugin.instance.removeTab = true;
            else
                Plugin.instance.removeTab = false;

            if (keyActions.ContainsKey(keyFromString))
            {
                keyActions.Remove(keyFromString);
                Loggers.WARNING($"Key: {QoLConfig.TerminalAutoCompleteKey.Value} had an active shortcut bind that has now been removed!");
            }


            return keyFromString;
        }
        else
        {
            Plugin.instance.removeTab = true;
            return Key.Tab;
        }
    }

    private static void DeserializeKeyActions(string serializedData)
    {
        // Clear existing keyActions dictionary
        keyActions.Clear();

        // Deserialize the serialized data into dictionary
        var pairs = serializedData.Split(';');
        foreach (var pair in pairs)
        {
            var keyValue = pair.Split('=');
            if (keyValue.Length == 2 && Enum.TryParse(keyValue[0], out Key key))
            {
                keyActions[key] = keyValue[1];
                Loggers.LogInfo($"Adding shortcut: Key({keyValue[0]}) Command({keyValue[1]})");
            }
        }
    }

    private static string SerializeKeyActions()
    {
        // Serialize the dictionary into a delimited string
        return string.Join(";", keyActions.Select(kv => $"{kv.Key}={kv.Value}"));
    }

    private static void SaveShortcutsToConfig()
    {
        // Serialize the dictionary into a format that can be stored in the configuration
        QoLConfig.KeyActionsConfig.Value = SerializeKeyActions();

        Loggers.LogInfo("Shortcuts saved to config");
    }

    internal static void UnbindKey(string[] words, int wordCount, out string displayText)
    {
        string invalidInput = "Unable to unbind key.\n\nUsage: unbind <key> \r\nexample: unbind f1 \r\n";

        if (wordCount < 2 || wordCount > 2)
        {
            Loggers.LogInfo("Invalid amount of words!");
            displayText = invalidInput;
            return;
        }

        Loggers.LogInfo("Unbind command detected!");
        string givenKey = words[1].ToLower();
        string bindNotFound = $"Unable to find keybinding for key <{givenKey}>";

        if (!IsValidKey(givenKey, invalidKeys))
        {
            Loggers.LogInfo("Invalid key detected!");
            displayText = bindNotFound;
            return;
        }
        else
        {
            Enum.TryParse(givenKey, ignoreCase: true, out Key keyFromString);
            keyActions.Remove(keyFromString);
            SaveShortcutsToConfig();
            displayText = $"Keybind removed! Key: {givenKey} has been removed from any command mappings.\r\n";
            Loggers.LogInfo($"Unbound shortcut tied to {givenKey}");
            return;
        }
    }

    internal static void BindToCommand(string[] words, int wordCount, out string displayText)
    {
        string invalidBind = "Unable to bind key to command.\n\nUsage: bind <key> <keyword>\nexample: bind f1 switch\r\n";
        if (wordCount < 3)
        {
            Loggers.LogInfo("Not enough words detected!");
            displayText = invalidBind;
            return;
        }
        Loggers.LogInfo("Bind command detected!");
        string givenKey = words[1].ToLower();

        if (wordCount > 3)
        {
            if (!IsValidKey(givenKey, invalidKeys))
            {
                Loggers.LogInfo("Invalid key detected!");
                displayText = invalidBind;
                return;
            }
            else
            {
                StringBuilder command = new();
                for (int i = 2; i < words.Length; i++)
                {
                    command.Append($"{words[i]} ");
                }

                Loggers.LogDebug($"BindToCommand detected Command: {command}");
                Enum.TryParse(givenKey, ignoreCase: true, out Key keyFromString);
                keyActions.Add(keyFromString, command.ToString());
                SaveShortcutsToConfig();
                displayText = $"Keybind created! Key: {givenKey} has been mapped to the following multi-word input: {command}\r\n";
                Loggers.LogInfo($"Keybind created mapping {givenKey} to [{command}]");
                return;
            }
        }

        string givenWord = words[2].ToLower();
        Loggers.LogDebug($"BindToCommand detected givenWord - {givenWord}");

        if (!MatchToKeyword(givenWord))
        {
            Loggers.LogInfo($"Invalid word detected! {givenWord}");
            displayText = invalidBind;
            return;
        }
        else if (!IsValidKey(givenKey, invalidKeys))
        {
            Loggers.LogInfo($"Invalid key detected! {givenKey}");
            displayText = invalidBind;
            return;
        }
        else
        {
            Enum.TryParse(givenKey, ignoreCase: true, out Key keyFromString);
            keyActions.Add(keyFromString, givenWord);
            SaveShortcutsToConfig();
            displayText = $"Keybind created! Key: {givenKey} has been mapped to the command: {givenWord}\r\n";
            Loggers.LogInfo($"BindToCommand: Keybind created mapping {givenKey} to {givenWord}");
        }
    }

    private static bool IsValidKey(string key, List<Key> invalidKeys)
    {
        if (Enum.TryParse(key, ignoreCase: true, out Key keyFromString))
        {
            if (invalidKeys.Contains(keyFromString))
            {
                Loggers.LogInfo("Alphabetical Key detected, rejecting bind.");
                return false;
            }
            else if (keyActions.ContainsKey(keyFromString))
            {
                keyActions.Remove(keyFromString);
                SaveShortcutsToConfig();
                Loggers.LogInfo("Key was already bound, removing bind and returning true");
                return true;
            }
            else
            {
                //keyBeingPressed = keyFromString;
                Loggers.LogInfo("Valid Key Detected and being assigned to bind");
                return true;
            }
        }
        else
            return false;
    }

    internal static bool MatchToKeyword(string input)
    {
        TerminalKeyword[] allKeywords = Plugin.instance.Terminal.terminalNodes.allKeywords;

        foreach (TerminalKeyword keyword in allKeywords)
        {
            if (OpenLib.Common.Misc.CompareStringsInvariant(keyword.word, input))
            {
                return true;
            }
        }

        Loggers.LogInfo("Unable to find keyword");
        return false;
    }



    internal static void MatchToBind(string input)
    {
        //List<string> skipAllKeywords = [];

        if (BannedWords(input))
        {
            Loggers.LogInfo("Banned word detected.");
            return;
        }

        TerminalKeyword[] allKeywords = Plugin.instance.Terminal.terminalNodes.allKeywords;
        foreach (TerminalKeyword keyword in allKeywords)
        {
            if (keyword.word == input)
            {
                Loggers.LogInfo("Loading node from Terminal Keywords");
                if (LogicHandling.TryGetFuncFromTerminalNode(ref keyword.specialKeywordResult, out Func<string> supplier))
                {
                    string displayText = supplier();
                    Loggers.LogInfo("related function has started for terminal start page!");
                    keyword.specialKeywordResult.displayText = displayText;
                }

                MoreCamStuff.CamPersistance(keyword.specialKeywordResult.name, keyword.specialKeywordResult);
                MoreCamStuff.VideoPersist(keyword.specialKeywordResult.name);
                Plugin.instance.Terminal.LoadNewNode(keyword.specialKeywordResult);
                return;
            }
        }

        StartofHandling.HandleShortcutFinal(input);

        //"kick", fColor, "fov", Gamble, Lever, "vitalspatch", "bioscanpatch", sColor, Link, Link2, Restart }; // keyword catcher
        //banned words - (word == Gamble || word == "fov" || word == "kick" || word == sColor || word == fColor)
        //remaining words - Lever, "vitalspatch", "bioscanpatch", Link, Link2, Restart
    }

    private static bool BannedWords(string word)
    {
        List<string> bannedWords = ["bind", "unbind"];

        if (bannedWords.Contains(word))
            return true;
        else
            return false;
    }

    // Method to handle key presses
    internal static void HandleKeyPress(Key key)
    {
        // Check if the key exists in the dictionary
        if (keyActions.ContainsKey(key))
        {
            // Get the keyword associated to the key
            keyActions.TryGetValue(key, out string value);

            if (value == string.Empty)
                return;

            if (value.Contains(' '))
            {
                LogicHandling.SetTerminalInput(value);
                Plugin.Log.LogDebug($"Terminal Input set to:{value}");
                Plugin.instance.Terminal.OnSubmit();
                return;
            }
            else if (value.Equals("[historyPrevious]") && QoLConfig.TerminalHistory.Value)
            {
                Loggers.LogDebug("terminalhistory [previous] bind detected and feature is enabled");
                TerminalHistory.historyIndex = TerminalHistory.PreviousIndex();
                LogicHandling.SetTerminalInput(TerminalHistory.GetFromCommandHistory(ref TerminalHistory.historyIndex));
                Plugin.Log.LogDebug($"Terminal Input set to:{TerminalHistory.GetFromCommandHistory(ref TerminalHistory.historyIndex)}");
                return;

            }
            else if (value.Equals("[historyNext]") && QoLConfig.TerminalHistory.Value)
            {
                Loggers.LogDebug("terminalhistory [next] bind detected and feature is enabled");
                TerminalHistory.historyIndex = TerminalHistory.NextIndex();
                LogicHandling.SetTerminalInput(TerminalHistory.GetFromCommandHistory(ref TerminalHistory.historyIndex));
                Plugin.Log.LogDebug($"Terminal Input set to:{TerminalHistory.GetFromCommandHistory(ref TerminalHistory.historyIndex)}");
                return;
            }
            else if (value.Equals("[autocomplete]") && QoLConfig.TerminalAutoComplete.Value)
            {
                Loggers.LogDebug("autocomplete key detected");

                if (AutoComplete.CheckCurrentInput(AutoComplete.AutoCompleteResults, TerminalEvents.GetCleanedScreenText(Plugin.instance.Terminal)))
                {
                    LogicHandling.SetTerminalInput(AutoComplete.ShowMatchingKeywords(AutoComplete.AutoCompleteResults, ref AutoComplete.AutoCompleteIndex));
                    Plugin.Log.LogDebug($"Terminal Input set to:{AutoComplete.ShowMatchingKeywords(AutoComplete.AutoCompleteResults, ref AutoComplete.AutoCompleteIndex)}");
                }

                else
                {
                    AutoComplete.AutoCompleteResults = AutoComplete.GetMatchingKeywords(TerminalEvents.GetCleanedScreenText(Plugin.instance.Terminal));
                    Loggers.LogDebug("setting AutoCompleteResults");
                    LogicHandling.SetTerminalInput(AutoComplete.ShowMatchingKeywords(AutoComplete.AutoCompleteResults, ref AutoComplete.AutoCompleteIndex));
                    Plugin.Log.LogDebug($"Terminal Input set to:{AutoComplete.ShowMatchingKeywords(AutoComplete.AutoCompleteResults, ref AutoComplete.AutoCompleteIndex)}");
                }
                return;
            }
            // Execute the action corresponding to the key
            Loggers.LogInfo($"Attempting to match given word to keyword: {value}");
            MatchToBind(value);
            return;
        }
        else
            Plugin.Log.LogError("Shortcut KeyActions list not updating properly");
    }
}

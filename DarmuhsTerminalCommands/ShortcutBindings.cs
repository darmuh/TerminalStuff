using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using static TerminalApi.TerminalApi;
using static TerminalStuff.DynamicCommands;
using Key = UnityEngine.InputSystem.Key;
using System.Collections;
using UnityEngine;
using System.Linq;

namespace TerminalStuff
{
    internal class ShortcutBindings
    {
        // Define a dictionary to map keys to actions
        internal static Dictionary<Key, string> keyActions = new Dictionary<Key, string>();
        internal static Key keyBeingPressed;

        internal static void InitSavedShortcuts()
        {
            Plugin.MoreLogs("Loading shortcuts from config");
            DeserializeKeyActions(ConfigSettings.keyActionsConfig.Value);
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
                    Plugin.MoreLogs($"Adding shortcut: Key({keyValue[0]}) Command({keyValue[1]})");
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
            ConfigSettings.keyActionsConfig.Value = SerializeKeyActions();

            Plugin.MoreLogs("Shortcuts saved to config");
        }

        internal static TerminalNode UnbindKey(string[] words, int wordCount, out TerminalNode outNode)
        {
            TerminalNode unbindNode = CreateTerminalNode("", true);
            
            string invalidInput = "Unable to unbind key.\n\nUsage: unbind <key> \\nexample: unbind f1 \r\n";

            if(wordCount < 2 || wordCount > 2)
            {
                Plugin.MoreLogs("Invalid amount of words!");
                unbindNode.displayText = invalidInput;
                outNode = unbindNode;
                return outNode;
            }

            Plugin.MoreLogs("Unbind command detected!");
            string givenKey = words[1].ToLower();
            string bindNotFound = $"Unable to find keybinding for key <{givenKey}>";

            if (!IsValidKey(givenKey))
            {
                Plugin.MoreLogs("Invalid key detected!");
                unbindNode.displayText = bindNotFound;
                outNode = unbindNode;
                return outNode;
            }
            else
            {
                Enum.TryParse(givenKey, ignoreCase: true, out Key keyFromString);
                keyActions.Remove(keyFromString);
                SaveShortcutsToConfig();
                unbindNode.displayText = $"Keybind removed! Key: {givenKey} has been removed from any command mappings.\r\n";
                outNode = unbindNode;
                Plugin.MoreLogs($"Unbound shortcut tied to {givenKey}");
                return outNode;
            }
        }

        internal static TerminalNode BindToCommand(string[] words, int wordCount, out TerminalNode outNode)
        {
            TerminalNode shortcutNode = CreateTerminalNode("", true);
            string invalidBind = "Unable to bind key to command.\n\nUsage: bind <key> <keyword>\nexample: bind f1 switch\r\n";
            if (wordCount < 3)
            {
                Plugin.MoreLogs("Not enough words detected!");
                shortcutNode.displayText = invalidBind;
                outNode = shortcutNode;
                return outNode;
            }
            Plugin.MoreLogs("Bind command detected!");
            string givenKey = words[1].ToLower();
            string givenWord = words[2].ToLower();

            if(!MatchToKeyword(givenWord))
            {
                Plugin.MoreLogs("Invalid word detected!");
                shortcutNode.displayText = invalidBind;
                outNode = shortcutNode;
                return outNode;
            }
            else if (!IsValidKey(givenKey))
            {
                Plugin.MoreLogs("Invalid key detected!");
                shortcutNode.displayText = invalidBind;
                outNode = shortcutNode;
                return outNode;
            }
            else
            {
                Enum.TryParse(givenKey, ignoreCase: true, out Key keyFromString);
                keyActions.Add(keyFromString, givenWord);
                SaveShortcutsToConfig();
                shortcutNode.displayText = $"Keybind created! Key: {givenKey} has been mapped to the command: {givenWord}\r\n";
                outNode = shortcutNode;
                Plugin.MoreLogs($"Keybind created mapping {givenKey} to {givenWord}");
                return outNode;
            }
        }

        internal static bool MatchToKeyword(string input)
        {
            TerminalKeyword[] allKeywords = Plugin.Terminal.terminalNodes.allKeywords;

            foreach(TerminalKeyword keyword in allKeywords)
            {
                if(keyword.word.ToLower() == input)
                {
                    return true;
                }
            }

            foreach(string word in StartofHandling.dynamicKeywords)
            {
                if(word.ToLower() == input)
                {
                    return true;
                }
            }

            Plugin.MoreLogs("Unable to find keyword");
            return false;
        }

        private static bool IsValidKey(string key)
        {
            List<Key> invalidKeys = new List<Key>() {
            Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G, Key.H, Key.I, Key.J,
            Key.K, Key.L, Key.M, Key.N, Key.O, Key.P, Key.Q, Key.R, Key.S, Key.T,
            Key.U, Key.V, Key.W, Key.X, Key.Y, Key.Z, Key.Space
            };
            if (Enum.TryParse(key, ignoreCase: true, out Key keyFromString))
            {
                if(invalidKeys.Contains(keyFromString))
                {
                    Plugin.MoreLogs("Alphabetical Key detected, rejecting bind.");
                    return false;
                }
                else if(keyActions.ContainsKey(keyFromString))
                {
                    keyActions.Remove(keyFromString);
                    SaveShortcutsToConfig();
                    Plugin.MoreLogs("Key was already bound, removing bind and returning true");
                    return true;
                }
                else
                {
                    //keyBeingPressed = keyFromString;
                    Plugin.MoreLogs("Valid Key Detected and being assigned to bind");
                    return true;
                }  
            }
            else
                return false;
        }

        internal static void MatchToBind(string input)
        {
            List<string> skipAllKeywords = new List<string>() { "switch" };

            if (BannedWords(input))
            {
                Plugin.MoreLogs("Banned word detected.");
                return;
            }

            TerminalKeyword[] allKeywords = Plugin.Terminal.terminalNodes.allKeywords;
            foreach (TerminalKeyword keyword in allKeywords)
            {
                if (keyword.word == input && !skipAllKeywords.Contains(input))
                {
                    Plugin.MoreLogs("Loading node from Terminal Keywords");
                    Func<string> displayTextSupplier = TerminalEvents.GetCommandDisplayTextSupplier(keyword.specialKeywordResult);

                    if (displayTextSupplier != null)
                    {
                        string displayText = displayTextSupplier();
                        Plugin.MoreLogs("running function related to displaytext supplier");
                        keyword.specialKeywordResult.displayText = displayText;
                    }

                    Plugin.Terminal.LoadNewNode(keyword.specialKeywordResult);
                    return;
                }
            }

            string[] words = new string[1];
            words[0] = input;

            StartofHandling.HandleParsed(Plugin.Terminal, words, out TerminalNode resultNode);

            if (resultNode != null)
            {
                Plugin.MoreLogs($"handling parsed node");
                Plugin.Terminal.LoadNewNode(resultNode);
                return;
            }

            //"kick", fColor, "fov", Gamble, Lever, "vitalspatch", "bioscanpatch", sColor, Link, Link2, Restart }; // keyword catcher
            //banned words - (word == Gamble || word == "fov" || word == "kick" || word == sColor || word == fColor)
            //remaining words - Lever, "vitalspatch", "bioscanpatch", Link, Link2, Restart
        }

        private static bool BannedWords(string word)
        {
            List<string> bannedWords = new List<string>() { Gamble, "fov", "kick", sColor, fColor, "bind", "unbind" };
            if (bannedWords.Contains(word))
                return true;
            else
                return false;
        }

        // Method to check if any key in the dictionary is pressed
        public static bool AnyKeyIsPressed()
        {
            foreach (var keyAction in keyActions)
            {
                if (Keyboard.current[keyAction.Key].isPressed)
                {
                    keyBeingPressed = keyAction.Key;
                    Plugin.MoreLogs($"Key detected in use: {keyAction.Key}");
                    return true;
                }
            }
            return false;
        }

        // Method to handle key presses
        private static void HandleKeyPress(Key key)
        {
            // Check if the key exists in the dictionary
            if (keyActions.ContainsKey(key))
            {
                // Get the keyword associated to the key
                keyActions.TryGetValue(key, out string value);

                if (value == string.Empty)
                    return;

                // Execute the action corresponding to the key
                Plugin.MoreLogs($"Attempting to match given word to keyword: {value}");
                MatchToBind(value);
            }
            else
                Plugin.Log.LogError("Shortcut KeyActions list not updating properly");
        }


        public static IEnumerator TerminalShortCuts()
        {
            Plugin.MoreLogs("Listening for shortcuts");
            while (Plugin.Terminal.terminalInUse && ConfigSettings.terminalShortcuts.Value)
            {
                if (AnyKeyIsPressed())
                {
                    HandleKeyPress(keyBeingPressed);
                    yield return new WaitForSeconds(0.1f);
                }
                else
                    yield return new WaitForSeconds(0.1f);
            }

            if (!Plugin.Terminal.terminalInUse)
                Plugin.MoreLogs("No longer monitoring for shortcuts");
            yield break;
        }
    }
}

using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using static TerminalApi.TerminalApi;
using static TerminalStuff.ColorCommands;
using static TerminalStuff.SpecialConfirmationLogic;

namespace TerminalStuff
{
    internal class DynamicCommands //Non-terminalAPI commands
    {
        //stuff
        public static int ParsedValue { get; internal set; }
        public static bool newParsedValue = false;

        //non-terminalAPI keywords
        public static string fColor;
        public static string Gamble;
        public static string Lever;
        public static string sColor;
        public static string Link;
        public static string Link2;
        public static string Restart;
        public static string linktext { get; internal set; } //public static string
        public static TerminalNode currentNode = null;

        internal static TerminalNode fovNode = CreateTerminalNode("replace this", true);
        internal static TerminalNode flashReturn = CreateTerminalNode("changing flash color\n", false);
        internal static TerminalNode flashFail = CreateTerminalNode("Invalid color, flashlight color will not be changed.\n", false);

        internal static void GetConfigKeywordsToUse()
        {
            if (ConfigSettings.fcolorKeyword.Value != null && GetKeyword(ConfigSettings.fcolorKeyword.Value) == null)
                fColor = ConfigSettings.fcolorKeyword.Value.ToLower();
            else
                fColor = "fcolor";
            if (ConfigSettings.gambleKeyword.Value != null && GetKeyword(ConfigSettings.gambleKeyword.Value) == null)
                Gamble = ConfigSettings.gambleKeyword.Value.ToLower();
            else
                Gamble = "gamble";

            if (ConfigSettings.leverKeyword.Value != null && GetKeyword(ConfigSettings.leverKeyword.Value) == null)
                Lever = ConfigSettings.leverKeyword.Value.ToLower();
            else
                Lever = "lever";

            if (ConfigSettings.scolorKeyword.Value != null && GetKeyword(ConfigSettings.scolorKeyword.Value) == null)
                sColor = ConfigSettings.scolorKeyword.Value.ToLower();
            else
                sColor = "scolor";

            if (ConfigSettings.linkKeyword.Value != null && GetKeyword(ConfigSettings.linkKeyword.Value) == null)
                Link = ConfigSettings.linkKeyword.Value.ToLower();
            else
                Link = "link";

            if (ConfigSettings.link2Keyword.Value != null && GetKeyword(ConfigSettings.link2Keyword.Value) == null)
                Link2 = ConfigSettings.link2Keyword.Value.ToLower();
            else
                Link2 = "link2";

            Restart = "restart";

            SpecialConfirmationLogic.InitKeywords();
        }

        internal static TerminalNode SendToKeywordMethod(string[] words, int wordCount, out TerminalNode outNode)
        {
            string keyWord = words[0];
            outNode = null;
            if (confKeywords.Contains(keyWord))
            {
                Plugin.MoreLogs($"confKeyword detected: {keyWord}");
                HandleQuestion(keyWord, out outNode);
                if (outNode != null && outNode == ask2gamble)
                {
                    string digitsProvided = string.Empty;
                    if (wordCount >= 2)
                    {
                        digitsProvided = words[1];
                    }

                    Plugin.MoreLogs("gamble with input detected.");
                    GambaCommands.AskToGamble(digitsProvided, out TerminalNode newNode);
                    Plugin.instance.awaitingConfirmation = true;
                    Plugin.instance.confirmationNodeNum = 2; //gamble
                    outNode = newNode;
                    return outNode;
                }
                else if (outNode != null && outNode.name.Contains("do"))
                {
                    Plugin.MoreLogs("result node detected early, must have skipped confirmation check.");
                    return outNode;
                }
                else if (outNode != null)
                {
                    ResolveCallbackToInt(keyWord, out int getNodeConfNum);
                    Plugin.instance.awaitingConfirmation = true;
                    Plugin.instance.confirmationNodeNum = getNodeConfNum;
                    Plugin.MoreLogs($"ConfNodeNum ({Plugin.instance.confirmationNodeNum}) set to {getNodeConfNum}");
                    return outNode;
                }
                else
                {
                    Plugin.MoreLogs("null outNode from SendToKeywordMethod");
                    return outNode;
                }
            }
            if (keyWord == "fov" && wordCount >= 2 && ConfigSettings.terminalFov.Value)
            {
                string digitsProvided = words[1];
                outNode = FovCommand(digitsProvided, out outNode);
                return outNode;
            }
            else if (keyWord == sColor && wordCount >= 2 && ConfigSettings.terminalScolor.Value && ConfigSettings.ModNetworking.Value)
            {
                ShipLightsColorCommand(words, wordCount, out outNode);
                return outNode;
            }
            else if (keyWord == fColor && wordCount == 2 && ConfigSettings.terminalFcolor.Value && ConfigSettings.ModNetworking.Value)
            {
                FlashLightColorCommands(words, out outNode);
                return outNode;
            }
            else if (keyWord == "kick" && wordCount >= 2 && ConfigSettings.terminalKick.Value)
            {
                AdminCommands.KickPlayerCommand(words, out outNode);
                return outNode;
            }
            else if (keyWord == "bind" && ConfigSettings.terminalShortcuts.Value)
            {
                Plugin.MoreLogs("Bind command detected");
                ShortcutBindings.BindToCommand(words, wordCount, out outNode);
                return outNode;
            }
            else if (keyWord == "unbind" && ConfigSettings.terminalShortcuts.Value)
            {
                Plugin.MoreLogs("Unbind command detected");
                ShortcutBindings.UnbindKey(words, wordCount, out outNode);
                return outNode;
            }
            else
                Plugin.MoreLogs("No matches in SendToKeywordMethod");

            return outNode;
        }

        internal static TerminalNode FlashLightColorCommands(string[] words, out TerminalNode outNode)
        {
            Plugin.MoreLogs("fcolor command detected!");
            usingHexCode = false;
            string targetColor = words[1].ToLower();

            if (targetColor == "rainbow")
            {
                NetHandler.Instance.CycleThroughRainbowFlash();
                Plugin.MoreLogs("Rainbowflash detected!");
                TerminalNode tempNode = CreateTerminalNode($"Flashlight color set to Rainbow Mode! (performance may vary)\r\n");
                outNode = tempNode;
                return outNode;
            }
            else if (targetColor == "list")
            {
                TerminalNode fList = CreateTerminalNode($"========= Flashlight Color Options List =========\r\nColor Name: \"command used\"\r\n\r\nDefault: \"{fColor} normal\" or \"{fColor} default\"\r\nRed: \"{fColor} red\"\r\nGreen: \"{fColor} green\"\r\nBlue: \"{fColor} blue\"\r\nYellow: \"{fColor} yellow\"\r\nCyan: \"{fColor} cyan\"\r\nMagenta: \"{fColor} magenta\"\r\nPurple: \"{fColor} purple\"\r\nLime: \"{fColor} lime\"\r\nPink: \"{fColor} pink\"\r\nMaroon: \"{fColor} maroon\"\r\nOrange: \"{fColor} orange\"\r\nSasstro's Color: \"{fColor} sasstro\"\r\nSamstro's Color: \"{fColor} samstro\"\r\n\r\nRainbow Color (animated): \"{fColor} rainbow\"\r\nANY HEXCODE: \"{fColor} FF00FF\"\r\n\r\n", true);
                outNode = fList;
                return outNode;
            }
            else
            {
                SetCustomColor(targetColor);
                flashLightColor = targetColor;

                if (CustomColor.HasValue)
                {
                    Plugin.MoreLogs($"Using flashlight color: {targetColor}");
                    NetHandler.Instance.endFlashRainbow = true;
                    FlashLightCommandAction(out string displayText);
                    flashReturn.displayText = displayText;
                    outNode = flashReturn;
                    return outNode;
                }
                else
                {
                    Plugin.MoreLogs($"Invalid flashlight color keyword: {targetColor}");
                    outNode = flashFail;
                    return outNode;
                }
            }
        }

        internal static TerminalNode ShipLightsColorCommand(string[] words, int wordCount, out TerminalNode outNode)
        {
            Plugin.MoreLogs("Scolor command init");
            usingHexCode = false;

            if (words[1].ToLower() == "list")
            {
                Plugin.MoreLogs("List detected");
                string listContent = $"========= Ship Lights Color Options List =========\r\nColor Name: \"command used\"\r\n\r\nDefault: \"{sColor} all normal\" or \"{sColor} all default\"\r\nRed: \"{sColor} back red\"\r\nGreen: \"{sColor} mid green\"\r\nBlue: \"{sColor} front blue\"\r\nYellow: \"{sColor} middle yellow\"\r\nCyan: \"{sColor} all cyan\"\r\nMagenta: \"{sColor} back magenta\"\r\nPurple: \"{sColor} mid purple\"\r\nLime: \"{sColor} all lime\"\r\nPink: \"{sColor} front pink\"\r\nMaroon: \"{sColor} middle maroon\"\r\nOrange: \"{sColor} back orange\"\r\nSasstro's Color: \"{sColor} all sasstro\"\r\nSamstro's Color: \"{sColor} all samstro\"\r\nANY HEXCODE: \"{sColor} all FF00FF\"\r\n\r\n\r\n";
                outNode = CreateTerminalNode(listContent, true);
                return outNode;
            }

            if (wordCount == 3)
            {
                string targetColor = words[2];
                Plugin.MoreLogs($"Attempting to set {words[1]} ship light colors to {targetColor}");
                ColorCommands.SetAndSendShipColor(words[1].ToLower(), targetColor, out outNode);
                return outNode;
            }
            else
            {
                TerminalNode tempNode = CreateTerminalNode($"Invalid selection.\r\n\r\nPlease choose between all, front, middle, and back lights to set and ensure you have specified a color name.\r\n\r\nSee '{sColor} list' for a list of color names.\r\n");
                outNode = tempNode;
                return outNode;
            }
        }

        internal static TerminalNode FovCommand(string digitsProvided, out TerminalNode outNode)
        {
            outNode = null;
            
            if (Regex.IsMatch(digitsProvided, "\\d+"))
            {
                newParsedValue = true;
                Plugin.MoreLogs("FOV: Integer Established");
                int parsedValue = int.Parse(digitsProvided);
                ParsedValue = parsedValue;
                FovNodeText(out string displayText);
                fovNode.displayText = displayText;
                outNode = fovNode;
                return outNode;
            }
            else
            {
                Plugin.Log.LogWarning("there are no digits");
                fovNode.displayText = "Fov can only be set between 66 and 130\n\n";
                outNode = fovNode;
                return outNode;
            }
        }

        private static void FovNodeText(out string displayText)
        {
            
            TerminalNode node = Plugin.Terminal.currentNode;

            if (!Plugin.instance.FovAdjust)
            {
                displayText = "FovAdjust mod is not installed, command can not be run.\r\n";
            }
            else
            {
                int num = ParsedValue;
                float number = num;
                if (number != 0 && number >= 66f && number <= 130f && newParsedValue)  // Or use an appropriate default value
                {
                    node.clearPreviousText = true;
                    displayText = ("Setting FOV to - " + num.ToString() + "\n\n");
                    Plugin.Terminal.StartCoroutine(FovEnum(node, Plugin.Terminal, number));
                }
                else
                {
                    displayText = "Fov can only be set between 66 and 130\n"; //not sure why this isn't 66 to 130 lol
                }

            }
        }

        private static IEnumerator FovEnum(TerminalNode node, Terminal term, float number)
        {
            yield return new WaitForSeconds(0.5f);
            FovAdjustStuff.FovAdjustFunc(node, term, number);
        }

    }
}

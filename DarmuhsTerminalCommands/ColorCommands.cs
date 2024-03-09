using System;
using System.Text.RegularExpressions;
using UnityEngine;
using static TerminalApi.TerminalApi;

namespace TerminalStuff
{
    internal class ColorCommands
    {
        //fcolor nodes
        internal static TerminalNode flashReturn = CreateTerminalNode("changing flash color\n", false, "flashlight");
        internal static TerminalNode flashFail = CreateTerminalNode("Invalid color, flashlight color will not be changed.\n", false);
        public static Color? CustomColor { get; private set; } // Public static variable to store the flashlight color
        public static string flashLightColor;
        internal static bool usingHexCode = false;

        internal static void FlashLightCommandAction(out string displayText)
        {
            string playerName = GameNetworkManager.Instance.localPlayerController.playerUsername;
            ulong playerID = GameNetworkManager.Instance.localPlayerController.playerClientId;
            string colorName = flashLightColor;

            Plugin.MoreLogs($"{playerName} trying to set color {colorName} to flashlight");
            Color flashlightColor = CustomColor ?? Color.white; // Use white as a default color
            Plugin.MoreLogs($"got {colorName} - {flashlightColor}");

            NetHandler.Instance.FlashColorServerRpc(flashlightColor, colorName, playerID, playerName);
            if (Plugin.instance.fSuccess && Plugin.instance.hSuccess)
            {
                displayText = $"Flashlight Color set to {colorName}.\r\nHelmet Light Color set to {colorName}.\r\n";
                Plugin.instance.fSuccess = false;
                Plugin.instance.hSuccess = false;
                return;
            }
            else if (Plugin.instance.fSuccess && !Plugin.instance.hSuccess)
            {
                displayText = $"Flashlight Color set to {colorName}.\r\nUnable to set Helmet Light Color.\r\n";
                Plugin.instance.fSuccess = false;
                Plugin.instance.hSuccess = false;
                return;
            }
            else
            {
                displayText = "Cannot set flashlight color.\r\n\r\nEnsure you have equipped a flashlight before using this command.\r\n";
                return;
            }
        }

        public static void SetCustomColor(string colorKeyword)
        {
            if (IsHexColorCode(colorKeyword))
            {
                // If it's a valid hex code, convert it to a Color
                usingHexCode = true;
                CustomColor = HexToColor("#" + colorKeyword);
                return;
            }
            else
            {
                switch (colorKeyword.ToLower())
                {
                    case "normal":
                        CustomColor = Color.white;
                        break;
                    case "default":
                        CustomColor = Color.white;
                        break;
                    case "red":
                        CustomColor = Color.red;
                        break;
                    case "blue":
                        CustomColor = Color.blue;
                        break;
                    case "yellow":
                        CustomColor = Color.yellow;
                        break;
                    case "cyan":
                        CustomColor = Color.cyan;
                        break;
                    case "magenta":
                        CustomColor = Color.magenta;
                        break;
                    case "green":
                        CustomColor = Color.green;
                        break;
                    case "purple":
                        CustomColor = new Color32(144, 100, 254, 1);
                        break;
                    case "lime":
                        CustomColor = new Color32(166, 254, 0, 1);
                        break;
                    case "pink":
                        CustomColor = new Color32(242, 0, 254, 1);
                        break;
                    case "maroon":
                        CustomColor = new Color32(114, 3, 3, 1); //new
                        break;
                    case "orange":
                        CustomColor = new Color32(255, 117, 24, 1); //new
                        break;
                    case "sasstro":
                        CustomColor = new Color32(212, 148, 180, 1);
                        break;
                    case "samstro":
                        CustomColor = new Color32(180, 203, 240, 1);
                        break;
                    default:
                        CustomColor = null; //this needs to be null for invalid results to return invalid
                        break;
                }
            }
        }

        public static bool IsHexColorCode(string input)
        {
            // Check if the input is a valid hex color code
            return Regex.IsMatch(input, "^(?:[0-9a-fA-F]{3}){1,2}$");
        }

        public static Color HexToColor(string hex)
        {
            // Convert hex color code to Color
            Color color = Color.white;
            ColorUtility.TryParseHtmlString(hex, out color);
            return color;
        }



        //DynamicCommands Logic
        internal static TerminalNode SetAndSendShipColor(string lightType, string targetColor, out TerminalNode giveNode)
        {
            Plugin.Log.LogInfo($"Attempting to set {lightType} ship light colors to {targetColor}");
            SetCustomColor(targetColor);
            if (CustomColor.HasValue && targetColor != null)
            {
                Color newColor = CustomColor.Value;
                if (lightType.ToLower().Contains("all"))
                    NetHandler.Instance.ShipColorALLServerRpc(newColor, targetColor);
                else if (lightType.ToLower().Contains("front"))
                    NetHandler.Instance.ShipColorFRONTServerRpc(newColor, targetColor);
                else if (lightType.ToLower().Contains("mid"))
                    NetHandler.Instance.ShipColorMIDServerRpc(newColor, targetColor);
                else if (lightType.ToLower().Contains("back"))
                    NetHandler.Instance.ShipColorBACKServerRpc(newColor, targetColor);

                TerminalNode tempNode = CreateTerminalNode($"Color of {lightType} ship lights set to {targetColor}!\r\n");
                giveNode = tempNode;
                return giveNode;
            }
            else
            {
                TerminalNode tempNode = CreateTerminalNode($"Invalid color {targetColor}.\r\n");
                giveNode = tempNode;
                return giveNode;
            }
        }

    }
}

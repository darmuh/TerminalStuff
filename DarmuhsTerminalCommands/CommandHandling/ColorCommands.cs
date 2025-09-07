using GameNetcodeStuff;
using System;
using System.Text.RegularExpressions;
using TerminalStuff.Configs;
using TerminalStuff.Util;
using UnityEngine;
using static TerminalStuff.Util.StringStuff;
using Color = UnityEngine.Color;
using Object = UnityEngine.Object;

namespace TerminalStuff;

internal class ColorCommands
{
    internal static Color? CustomFlashColor; // static variable to store the flashlight color
    internal static string flashLightColor = string.Empty;
    internal static bool usingHexCode = false;
    internal static bool RainbowFlash = false;

    internal static void FlashLightCommandAction(out string displayText)
    {
        RainbowFlash = false;
        Color fColor = CustomFlashColor ?? Color.white; // Use white as a default color
        Loggers.LogInfo($"got {flashLightColor} - {fColor}");

        displayText = $"The next time you turn on your flashlight, the color will be set to {flashLightColor}!\r\n\r\n";
        return;
    }

    internal static void SetCustomColor(string colorKeyword, out Color? customColor)
    {

        if (IsHexColorCode(colorKeyword))
        {
            // If it's a valid hex code, convert it to a Color
            usingHexCode = true;
            customColor = HexToColor("#" + colorKeyword);
            return;
        }
        else
        {
            customColor = colorKeyword.ToLower() switch
            {
                "white" => (Color?)Color.white,
                "normal" => (Color?)Color.white,
                "default" => (Color?)Color.white,
                "red" => (Color?)Color.red,
                "blue" => (Color?)Color.blue,
                "yellow" => (Color?)Color.yellow,
                "cyan" => (Color?)Color.cyan,
                "magenta" => (Color?)Color.magenta,
                "green" => (Color?)Color.green,
                "purple" => (Color?)new Color32(144, 100, 254, 1),
                "lime" => (Color?)new Color32(166, 254, 0, 1),
                "pink" => (Color?)new Color32(242, 0, 254, 1),
                "maroon" => (Color?)new Color32(114, 3, 3, 1),//new
                "orange" => (Color?)new Color32(255, 117, 24, 1),//new
                "sasstro" => (Color?)new Color32(212, 148, 180, 1),
                "samstro" => (Color?)new Color32(180, 203, 240, 1),
                _ => null, //this needs to be null for invalid results to return invalid
            };
        }
    }

    internal static bool IsHexColorCode(string input)
    {
        // Check if the input is a valid hex color code
        return Regex.IsMatch(input, "^(?:[0-9a-fA-F]{3}){1,2}$");
    }

    internal static Color HexToColor(string hex)
    {
        return OpenLib.Common.Misc.HexToColor(hex);
    }

    //dynamic commands logic

    internal static string ShipColorBase()
    {
        string val = GetAfterKeyword(GetKeywordsPerConfigItem(KeywordConfigs.ScolorKeywords.Value));

        if (val.Length < 1)
        {
            string message = ShipColorList();
            Loggers.WARNING("not enough words for the command!");
            return message;
        }
        else if (OpenLib.Common.Misc.StringContainsInvariant(val, "list"))
        {
            string message = ShipColorList();
            Loggers.LogInfo("list requested");
            return message;
        }

        string[] words = val.Split([' '], StringSplitOptions.RemoveEmptyEntries);
        bool sColorCheck = ShipColorCommon(words, out string displayText, out string targetColor, out Color newColor);
        if (!sColorCheck)
            return displayText;

        if (OpenLib.Common.Misc.StringContainsInvariant(val, "all"))
        {
            NetHandler.Instance.ShipColorALLServerRpc(newColor, targetColor);
            return displayText;
        }
        else if (OpenLib.Common.Misc.StringContainsInvariant(val, "front"))
        {
            NetHandler.Instance.ShipColorFRONTServerRpc(newColor, targetColor);
            return displayText;
        }
        else if (OpenLib.Common.Misc.StringContainsInvariant(val, "mid"))
        {
            NetHandler.Instance.ShipColorMIDServerRpc(newColor, targetColor);
            return displayText;
        }
        else if (OpenLib.Common.Misc.StringContainsInvariant(val, "back"))
        {
            NetHandler.Instance.ShipColorBACKServerRpc(newColor, targetColor);
            return displayText;
        }
        else
        {
            Loggers.WARNING("failed to grab specific part of ship lights to change");
            string listContents = ShipColorList();
            return listContents;
        }
    }

    internal static bool ShipColorCommon(string[] words, out string displayText, out string targetColor, out Color newColor)
    {
        if (words.Length < 2)
        {
            displayText = ShipColorList();
            Loggers.LogInfo("not enough words for the command, returning list!");
            targetColor = string.Empty;
            newColor = Color.white;
            return false;
        }

        targetColor = words[1];
        Loggers.LogInfo($"Attempting to set {words[0]} ship light colors to {targetColor}");
        SetCustomColor(targetColor, out Color? ShipColor);
        if (ShipColor.HasValue && targetColor != null)
        {
            newColor = ShipColor.Value;
            displayText = $"Color of {words[0]} ship lights set to {targetColor}!\r\n\r\n";
            return true;
        }
        else
        {
            targetColor = "";
            newColor = Color.white;
            displayText = $"Unable to set {words[0]} ship light color...\r\n\tInvalid color [{targetColor}] detected!\r\n\r\n";
            Loggers.WARNING("invalid color for the color command!");
            return false;
        }
    }

    internal static string ShipColorList()
    {
        string sColor = GetKeywordsPerConfigItem(KeywordConfigs.ScolorKeywords.Value)[0];
        string listContent = $"========= Ship Lights Color Options List =========\r\nColor Name: \"command used\"\r\n\r\nDefault: \"{sColor} all normal\" or \"{sColor} all default\"\r\nRed: \"{sColor} back red\"\r\nGreen: \"{sColor} mid green\"\r\nBlue: \"{sColor} front blue\"\r\nYellow: \"{sColor} middle yellow\"\r\nCyan: \"{sColor} all cyan\"\r\nMagenta: \"{sColor} back magenta\"\r\nPurple: \"{sColor} mid purple\"\r\nLime: \"{sColor} all lime\"\r\nPink: \"{sColor} front pink\"\r\nMaroon: \"{sColor} middle maroon\"\r\nOrange: \"{sColor} back orange\"\r\nSasstro's Color: \"{sColor} all sasstro\"\r\nSamstro's Color: \"{sColor} all samstro\"\r\nANY HEXCODE: \"{sColor} all FF00FF\"\r\n\r\n\r\n";
        return listContent;
    }

    internal static string FlashColorBase()
    {
        string val = GetAfterKeyword(GetKeywordsPerConfigItem(KeywordConfigs.FcolorKeywords.Value));
        string message;

        if (val.Length < 1)
        {
            message = FlashColorList();
            Loggers.WARNING("getting list, not enough words for color command!");
            return message;
        }

        if (OpenLib.Common.Misc.StringContainsInvariant(val, "list"))
        {
            message = FlashColorList();
            Loggers.LogInfo("displaying flashcolor list");
            return message;
        }
        if (OpenLib.Common.Misc.StringContainsInvariant(val, "rainbow"))
        {
            message = FlashColorRainbow();
            Loggers.LogInfo("running rainbow command");
            return message;
        }
        if (OpenLib.Common.Misc.StringContainsInvariant(val, "normal") || OpenLib.Common.Misc.StringContainsInvariant(val, "default"))
        {
            Loggers.LogInfo("Player no longer wants a custom flashlight color!");
            CustomFlashColor = null;
            RainbowFlash = false;
            return "Flashlight color preference set back to default!\n\nFlashlight's with the default color will no longer be updated!\r\n\r\n";
        }

        string targetColor = val.TrimStart();

        Loggers.LogInfo($"Attempting to set flashlight color to {targetColor}");
        SetCustomColor(targetColor, out CustomFlashColor);
        flashLightColor = targetColor;

        if (CustomFlashColor.HasValue)
        {
            Loggers.LogInfo($"Using flashlight color: {targetColor}");
            NetHandler.Instance.endFlashRainbow = true;
            FlashLightCommandAction(out string displayText);
            return displayText;
        }
        else
        {
            string displayText = $"Unable to set flashlight color...\r\n\tInvalid color: [{targetColor}] detected!\r\n\r\n";
            Loggers.WARNING("invalid color for the color command!");
            return displayText;
        }
    }

    internal static string FlashColorList()
    {
        string fColor = GetKeywordsPerConfigItem(KeywordConfigs.FcolorKeywords.Value)[0];
        string listContent = $"========= Flashlight Color Options List =========\r\nColor Name: \"command used\"\r\n\r\nDefault: \"{fColor} normal\" or \"{fColor} default\"\r\nRed: \"{fColor} red\"\r\nGreen: \"{fColor} green\"\r\nBlue: \"{fColor} blue\"\r\nYellow: \"{fColor} yellow\"\r\nCyan: \"{fColor} cyan\"\r\nMagenta: \"{fColor} magenta\"\r\nPurple: \"{fColor} purple\"\r\nLime: \"{fColor} lime\"\r\nPink: \"{fColor} pink\"\r\nMaroon: \"{fColor} maroon\"\r\nOrange: \"{fColor} orange\"\r\nSasstro's Color: \"{fColor} sasstro\"\r\nSamstro's Color: \"{fColor} samstro\"\r\n\r\nRainbow Color (animated): \"{fColor} rainbow\"\r\nANY HEXCODE: \"{fColor} FF00FF\"\r\n\r\n";
        return listContent;
    }

    internal static string FlashColorRainbow()
    {
        if (DoIhaveFlash(StartOfRound.Instance.localPlayerController))
        {
            NetHandler.Instance.CycleThroughRainbowFlash();
            string displayText = $"Flashlight color set to Rainbow Mode! (performance may vary)\r\n\r\n";
            return displayText;
        }
        else
        {
            RainbowFlash = true;
            string displayText = $"The next flashlight you hold will be set to rainbow mode! (performance may vary)\r\n\r\n";
            return displayText;
        }

    }

    private static bool DoIhaveFlash(PlayerControllerB player)
    {
        GrabbableObject[] objectsOfType = Object.FindObjectsOfType<GrabbableObject>();

        foreach (GrabbableObject thisFlash in objectsOfType)
        {
            if (thisFlash.playerHeldBy != null)
            {
                if (thisFlash.playerHeldBy.playerUsername == player.playerUsername && thisFlash.gameObject.name.Contains("Flashlight"))
                {
                    return true;
                }
            }
        }

        return false;
    }

}

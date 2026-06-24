using GameNetcodeStuff;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TerminalStuff.Configs;
using TerminalStuff.Networking;
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
    private static GameObject _frontlight1 = null!;
    private static GameObject _frontlight2 = null!;
    private static GameObject _midlight1 = null!;
    private static GameObject _midlight2 = null!;
    private static GameObject _backlight1 = null!;
    private static GameObject _backlight2 = null!;

    internal static GameObject FrontLight1
    {
        get
        {
            if (_frontlight1 == null)
                _frontlight1 = GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (3)");

            return _frontlight1;
        }
    }
    internal static GameObject FrontLight2
    {
        get
        {
            if (_frontlight2 == null)
                _frontlight2 = GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (9)");

            return _frontlight2;
        }
    }

    internal static GameObject MidLight1
    {
        get
        {
            if (_midlight1 == null)
                _midlight1 = GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (4)");

            return _midlight1;
        }
    }

    internal static GameObject MidLight2
    {
        get
        {
            if (_midlight2 == null)
                _midlight2 = GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (8)");

            return _midlight2;
        }
    }

    internal static GameObject BackLight1
    {
        get
        {
            if (_backlight1 == null)
                _backlight1 = GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (5)");

            return _backlight1;
        }
    }

    internal static GameObject BackLight2
    {
        get
        {
            if (_backlight2 == null)
                _backlight2 = GameObject.Find("Environment/HangarShip/ShipElectricLights/Area Light (7)");

            return _backlight2;
        }
    }

    internal static void SetLightColors(List<GameObject> objects, Color newColor)
    {
        if (objects.Count == 0)
            return;

        foreach(GameObject obj in objects)
        {
            if(obj.GetComponent<Light>() != null)
                obj.GetComponent<Light>().color = newColor;
        }
    }

    internal static void FlashLightCommandAction(out string displayText)
    {
        RainbowFlash = false;
        Color fColor = CustomFlashColor ?? Color.white; // Use white as a default color
        Loggers.LogInfo($"got {flashLightColor} - {fColor}");

        displayText = $"The next time you turn on your flashlight, the color will be set to {flashLightColor}!\n\n";
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
            NetHandler.Instance.ShipColorAllRpc(newColor, targetColor);
            return displayText;
        }
        else if (OpenLib.Common.Misc.StringContainsInvariant(val, "front"))
        {
            NetHandler.Instance.ShipColorFrontRpc(newColor, targetColor);
            return displayText;
        }
        else if (OpenLib.Common.Misc.StringContainsInvariant(val, "mid"))
        {
            NetHandler.Instance.ShipColorMidRpc(newColor, targetColor);
            return displayText;
        }
        else if (OpenLib.Common.Misc.StringContainsInvariant(val, "back"))
        {
            NetHandler.Instance.ShipColorBackRpc(newColor, targetColor);
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
            displayText = $"Color of {words[0]} ship lights set to {targetColor}!\n\n";
            return true;
        }
        else
        {
            targetColor = "";
            newColor = Color.white;
            displayText = $"Unable to set {words[0]} ship light color...\n\tInvalid color [{targetColor}] detected!\n\n";
            Loggers.WARNING("invalid color for the color command!");
            return false;
        }
    }

    internal static string ShipColorList()
    {
        string sColor = GetKeywordsPerConfigItem(KeywordConfigs.ScolorKeywords.Value)[0];
        string listContent = $"========= Ship Lights Color Options List =========\nColor Name: \"command used\"\n\nDefault: \"{sColor} all normal\" or \"{sColor} all default\"\nRed: \"{sColor} back red\"\nGreen: \"{sColor} mid green\"\nBlue: \"{sColor} front blue\"\nYellow: \"{sColor} middle yellow\"\nCyan: \"{sColor} all cyan\"\nMagenta: \"{sColor} back magenta\"\nPurple: \"{sColor} mid purple\"\nLime: \"{sColor} all lime\"\nPink: \"{sColor} front pink\"\nMaroon: \"{sColor} middle maroon\"\nOrange: \"{sColor} back orange\"\nSasstro's Color: \"{sColor} all sasstro\"\nSamstro's Color: \"{sColor} all samstro\"\nANY HEXCODE: \"{sColor} all FF00FF\"\n\n\n";
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
            return "Flashlight color preference set back to default!\n\nFlashlight's with the default color will no longer be updated!\n\n";
        }

        string targetColor = val.TrimStart();

        Loggers.LogInfo($"Attempting to set flashlight color to {targetColor}");
        SetCustomColor(targetColor, out CustomFlashColor);
        flashLightColor = targetColor;

        if (CustomFlashColor.HasValue)
        {
            Loggers.LogInfo($"Using flashlight color: {targetColor}");
            NetHandler.Instance.EndFlashRainbow = true;
            FlashLightCommandAction(out string displayText);
            return displayText;
        }
        else
        {
            string displayText = $"Unable to set flashlight color...\n\tInvalid color: [{targetColor}] detected!\n\n";
            Loggers.WARNING("invalid color for the color command!");
            return displayText;
        }
    }

    internal static string FlashColorList()
    {
        string fColor = GetKeywordsPerConfigItem(KeywordConfigs.FcolorKeywords.Value)[0];
        string listContent = $"========= Flashlight Color Options List =========\nColor Name: \"command used\"\n\nDefault: \"{fColor} normal\" or \"{fColor} default\"\nRed: \"{fColor} red\"\nGreen: \"{fColor} green\"\nBlue: \"{fColor} blue\"\nYellow: \"{fColor} yellow\"\nCyan: \"{fColor} cyan\"\nMagenta: \"{fColor} magenta\"\nPurple: \"{fColor} purple\"\nLime: \"{fColor} lime\"\nPink: \"{fColor} pink\"\nMaroon: \"{fColor} maroon\"\nOrange: \"{fColor} orange\"\nSasstro's Color: \"{fColor} sasstro\"\nSamstro's Color: \"{fColor} samstro\"\n\nRainbow Color (animated): \"{fColor} rainbow\"\nANY HEXCODE: \"{fColor} FF00FF\"\n\n";
        return listContent;
    }

    internal static string FlashColorRainbow()
    {
        if (Bools.StartIsLocalPlayerNull())
            return "";

        if (DoIhaveFlash(StartOfRound.Instance.localPlayerController))
        {
            NetHandler.Instance.CycleThroughRainbowFlash();
            string displayText = $"Flashlight color set to Rainbow Mode! (performance may vary)\n\n";
            return displayText;
        }
        else
        {
            RainbowFlash = true;
            string displayText = $"The next flashlight you hold will be set to rainbow mode! (performance may vary)\n\n";
            return displayText;
        }

    }

    private static bool DoIhaveFlash(PlayerControllerB player)
    {
        foreach (GrabbableObject item in player.ItemSlots)
        {
            if (item is FlashlightItem)
            {
                return true;
            }
        }

        return false;
    }

}

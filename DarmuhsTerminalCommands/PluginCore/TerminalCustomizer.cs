using TerminalStuff.Configs;
using TerminalStuff.SpecialStuff;
using TerminalStuff.VisualCore;
using UnityEngine;
using UnityEngine.UI;
using static TerminalStuff.EventSub.TerminalStart;

namespace TerminalStuff.PluginCore;

internal class TerminalCustomizer
{
    internal static bool defaultsCached = false;
    internal static Image terminalBackground;
    internal static Image moneyBG;

    private static void SetTerminalBodyColors()
    {
        if (!CustomizeConfig.TerminalCustomization.Value)
            return;

        MeshRenderer termMesh = GameObject.Find("Environment/HangarShip/Terminal").GetComponent<MeshRenderer>();

        if (termMesh != null)
        {
            if (termMesh.materials.Length <= 3)
            {
                termMesh.materials[0].color = ColorCommands.HexToColor(CustomizeConfig.TerminalColor.Value); //body
                termMesh.materials[1].color = ColorCommands.HexToColor(CustomizeConfig.TerminalButtonsColor.Value); //glass buttons
                //2 = warning sticker
            }
            else
            {
                Loggers.WARNING("termMesh does not have expected number of materials, only setting terminal body color");
                termMesh.material.color = ColorCommands.HexToColor(CustomizeConfig.TerminalColor.Value);
            }
        }
        else
            Loggers.WARNING("customization failure: termMesh is null");
    }

    private static void GetTerminalBodyColors()
    {
        if (!CustomizeConfig.TerminalCustomization.Value)
            return;

        MeshRenderer termMesh = GameObject.Find("Environment/HangarShip/Terminal").GetComponent<MeshRenderer>();

        if (termMesh != null)
        {
            if (termMesh.materials.Length <= 3)
            {
                CustomTerminalStuff.TerminalBody = termMesh.materials[0].color; //body
                AnnounceColor(CustomTerminalStuff.TerminalBody, "CustomTerminalStuff.TerminalBody");
                CustomTerminalStuff.TerminalButtonsColor = termMesh.materials[1].color; //glass buttons
                AnnounceColor(CustomTerminalStuff.TerminalButtonsColor, "CustomTerminalStuff.TerminalButtonsColor");
                //2 = warning sticker
            }
            else
            {
                Loggers.WARNING("termMesh does not have expected number of materials, only setting terminal body color");
                CustomTerminalStuff.TerminalBody = termMesh.material.color;
                AnnounceColor(CustomTerminalStuff.TerminalBody, "CustomTerminalStuff.TerminalBody");
            }
        }
        else
            Loggers.WARNING("customization failure: termMesh is null\nFailed to get TerminalBody & TerminalButtonsColor");
    }

    private static void SetTerminalKeyboardColors()
    {
        if (!CustomizeConfig.TerminalCustomization.Value)
            return;

        MeshRenderer kbMesh = GameObject.Find("Environment/HangarShip/Terminal/Terminal.003").GetComponent<MeshRenderer>();

        if (kbMesh != null)
        {
            kbMesh.material.color = ColorCommands.HexToColor(CustomizeConfig.TerminalKeyboardColor.Value);
        }
        else
            Loggers.WARNING("customization failure: kbMesh is null");
    }

    private static void GetTerminalKeyboardColors()
    {
        if (!CustomizeConfig.TerminalCustomization.Value)
            return;

        MeshRenderer kbMesh = GameObject.Find("Environment/HangarShip/Terminal/Terminal.003").GetComponent<MeshRenderer>();

        if (kbMesh != null)
        {
            CustomTerminalStuff.TerminalKeyboard = kbMesh.material.color;
            AnnounceColor(CustomTerminalStuff.TerminalKeyboard, "CustomTerminalStuff.TerminalKeyboard");
        }
        else
            Loggers.WARNING("customization failure: kbMesh is null\nFailed to get TerminalKeyboard color");
    }

    internal static void StartNode()
    {
        Loggers.LogDebug("Updating home displaytext");
        startNode = Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[1];
        startNode.displayText = $"{CustomizeConfig.HomeLine1.Value}\r\n{CustomizeConfig.HomeLine2.Value}\r\n\r\n{CustomizeConfig.HomeHelpLines.Value}\r\n{CustomizeConfig.HomeTextArt.Value}\r\n\r\n{CustomizeConfig.HomeLine3.Value}\r\n\r\n";
    }

    private static void AutoResizeMoneyBG()
    {
        if (!CustomizeConfig.AutoResizeMoneyBG.Value || moneyBG == null)
            return;

        CustomAutoSize customSizer = moneyBG.gameObject.AddComponent<CustomAutoSize>();
        customSizer.SetValues(moneyBG.rectTransform, ref Plugin.instance.Terminal.topRightText, new(-195, moneyBG.rectTransform.anchoredPosition.y), new(34, 22));
    }

    private static void SetMoneyBGStuff(Color moneyBGColor)
    {
        moneyBG.color = moneyBGColor;
        AutoResizeMoneyBG();
    }

    private static void GetMoneyBG(Color moneyBGColor)
    {
        if (Plugin.instance.Terminal.terminalUIScreen.gameObject.transform.GetChild(0).childCount >= 6)
        {
            if (Plugin.instance.Terminal.terminalUIScreen.gameObject.transform.GetChild(0).GetChild(5).gameObject.GetComponent<Image>() != null)
            {
                moneyBG = Plugin.instance.Terminal.terminalUIScreen.gameObject.transform.GetChild(0).GetChild(5).gameObject.GetComponent<Image>();
                SetMoneyBGStuff(moneyBGColor);
            }
        }
        else
            Loggers.WARNING("Unable to set TerminalMoneyBG customizations!!");
    }

    internal static void TerminalCustomization()
    {
        StartNode(); //should always be modified, not colors

        if (!CustomizeConfig.TerminalCustomization.Value)
            return;

        if (!defaultsCached)
            CacheDefaults();

        SetTerminalBodyColors();
        SetTerminalKeyboardColors();
        FontStuff.GetAndSetFont();


        Color moneyBGColor = SetColorFor(CustomizeConfig.TerminalMoneyBGColor.Value, CustomTerminalStuff.MoneyBG, CustomizeConfig.TerminalMoneyBGAlpha.Value);
        if (moneyBG != null)
            SetMoneyBGStuff(moneyBGColor);
        else
            GetMoneyBG(moneyBGColor);

        Plugin.instance.Terminal.screenText.textComponent.color = SetColorFor(CustomizeConfig.TerminalTextColor.Value, CustomTerminalStuff.TerminalText);
        Plugin.instance.Terminal.topRightText.color = SetColorFor(CustomizeConfig.TerminalMoneyColor.Value, CustomTerminalStuff.MoneyText);

        Plugin.instance.Terminal.screenText.caretColor = SetColorFor(CustomizeConfig.TerminalCaretColor.Value, CustomTerminalStuff.TextCaret);

        if (Plugin.instance.suitsTerminal)
            SuitsTerminalCompatibility.SetCaretColor(Plugin.instance.Terminal.screenText.caretColor);

        Plugin.instance.Terminal.scrollBarVertical.image.color = SetColorFor(CustomizeConfig.TerminalScrollbarColor.Value, CustomTerminalStuff.Scrollbar);
        Plugin.instance.Terminal.scrollBarVertical.gameObject.GetComponent<Image>().color = SetColorFor(CustomizeConfig.TerminalScrollBGColor.Value, CustomTerminalStuff.ScrollbarBackground);
        Plugin.instance.Terminal.terminalLight.color = SetColorFor(CustomizeConfig.TerminalLightColor.Value, CustomTerminalStuff.TerminalLight);

        if (TerminalClockStuff.textComponent != null)
        {
            Loggers.LogInfo($"setting clock color");
            TerminalClockStuff.textComponent.color = SetColorFor(CustomizeConfig.TerminalClockColor.Value, CustomTerminalStuff.TerminalClock);
        }

        terminalBackground = Plugin.instance.Terminal.terminalUIScreen.gameObject.GetComponentInChildren<Image>();

        if (terminalBackground != null)
        {
            terminalBackground.enabled = CustomizeConfig.TerminalCustomBG.Value;
            terminalBackground.transform.SetParent(Plugin.instance.Terminal.terminalImage.transform);
            terminalBackground.transform.SetAsLastSibling();
            terminalBackground.rectTransform.anchoredPosition = new Vector2(10, 0);
            terminalBackground.rectTransform.sizeDelta = new Vector2(-80, 20);
            Color bgColor = SetColorFor(CustomizeConfig.TerminalCustomBGColor.Value, CustomTerminalStuff.TerminalBackground, CustomizeConfig.TerminalCustomBGAlpha.Value);
            terminalBackground.color = bgColor;
        }
        else
            Loggers.LogDebug("terminalBackground is NULL");

    }

    internal static Color SetColorFor(string config, Color cachedColor)
    {
        if ((config.Length < 1 || config.ToLower() == "default") && cachedColor != null)
        {
            Loggers.LogDebug("setting to cached default value");
            return cachedColor;
        }
        else
        {
            Loggers.LogDebug($"getting color from {config}");
            return ColorCommands.HexToColor(config);
        }
    }

    private static Color SetColorFor(string config, Color cachedColor, float newAlpha)
    {
        if ((config.Length < 1 || config.ToLower() == "default") && cachedColor != null)
        {
            Loggers.LogDebug("setting to cached default value");
            return cachedColor;
        }
        else
        {
            Loggers.LogDebug("getting color from config item");
            Color newColor = ColorCommands.HexToColor(config);
            newColor.a = newAlpha;
            return newColor;
        }
    }

    internal static void GetOtherDefaultColors()
    {
        if (!CustomizeConfig.TerminalCustomization.Value)
            return;

        CustomTerminalStuff.TerminalText = Plugin.instance.Terminal.screenText.textComponent.color;
        AnnounceColor(CustomTerminalStuff.TerminalText, "CustomTerminalStuff.TerminalText");

        CustomTerminalStuff.MoneyText = Plugin.instance.Terminal.topRightText.color;
        AnnounceColor(CustomTerminalStuff.MoneyText, "CustomTerminalStuff.MoneyText");

        if (Plugin.instance.Terminal.terminalUIScreen.gameObject.transform.GetChild(0).GetChild(5).gameObject.GetComponent<Image>() != null)
        {
            CustomTerminalStuff.MoneyBG = Plugin.instance.Terminal.terminalUIScreen.gameObject.transform.GetChild(0).GetChild(5).gameObject.GetComponent<Image>().color;
            AnnounceColor(CustomTerminalStuff.MoneyBG, "CustomTerminalStuff.MoneyBG");
        }


        CustomTerminalStuff.TextCaret = Plugin.instance.Terminal.screenText.caretColor;
        CustomTerminalStuff.Scrollbar = Plugin.instance.Terminal.scrollBarVertical.image.color;
        if (Plugin.instance.Terminal.scrollBarVertical.gameObject.GetComponent<Image>() != null)
            CustomTerminalStuff.ScrollbarBackground = Plugin.instance.Terminal.scrollBarVertical.gameObject.GetComponent<Image>().color;
        else
            Loggers.WARNING("Unable to get scrollbar background color cached default!");
        CustomTerminalStuff.TerminalLight = Plugin.instance.Terminal.terminalLight.color;

        AnnounceColor(CustomTerminalStuff.TextCaret, "CustomTerminalStuff.TextCaret");
        AnnounceColor(CustomTerminalStuff.Scrollbar, "CustomTerminalStuff.Scrollbar");
        AnnounceColor(CustomTerminalStuff.ScrollbarBackground, "CustomTerminalStuff.ScrollbarBackground");
        AnnounceColor(CustomTerminalStuff.TerminalLight, "CustomTerminalStuff.TerminalLight");

        if (TerminalClockStuff.textComponent != null)
        {
            CustomTerminalStuff.TerminalClock = TerminalClockStuff.textComponent.color;
            AnnounceColor(CustomTerminalStuff.TerminalClock, "CustomTerminalStuff.TerminalClock");
        }

        terminalBackground = Plugin.instance.Terminal.terminalUIScreen.gameObject.GetComponentInChildren<Image>();

        if (terminalBackground != null)
        {
            CustomTerminalStuff.TerminalBackground = terminalBackground.color;
            AnnounceColor(CustomTerminalStuff.TerminalBackground, "CustomTerminalStuff.TerminalBackground");
        }
        else
            Loggers.LogDebug("terminalBackground is NULL");

    }

    internal static void CacheDefaults()
    {
        if (defaultsCached)
            return;

        GetTerminalBodyColors();
        GetTerminalKeyboardColors();
        GetOtherDefaultColors();
        defaultsCached = true;
    }

    internal static void AnnounceColor(Color colorValue, string name)
    {
        if (colorValue == null)
        {
            Loggers.LogDebug($"colorValue is null for {name}");
            return;
        }
        string color = ColorUtility.ToHtmlStringRGB(colorValue);
        Loggers.LogDebug($"{name} cached color: {color}");
    }
}

internal class CustomTerminalStuff
{
    internal static Color TerminalBody;
    internal static Color TerminalButtonsColor;
    internal static Color TerminalKeyboard;
    internal static Color TerminalText;
    internal static Color TerminalBackground;
    internal static Color MoneyBG;
    internal static Color MoneyText;
    internal static Color TextCaret;
    internal static Color Scrollbar;
    internal static Color ScrollbarBackground;
    internal static Color TerminalLight;
    internal static Color TerminalClock;
}

using TerminalStuff.SpecialStuff;
using static TerminalStuff.EventSub.TerminalStart;
using UnityEngine;
using UnityEngine.UI;

namespace TerminalStuff.PluginCore
{
    internal class TerminalCustomizer
    {
        internal static bool defaultsCached = false;
        internal static Image terminalBackground;

        private static void SetTerminalBodyColors()
        {
            if (!ConfigSettings.TerminalCustomization.Value)
                return;

            MeshRenderer termMesh = GameObject.Find("Environment/HangarShip/Terminal").GetComponent<MeshRenderer>();

            if (termMesh != null)
            {
                if (termMesh.materials.Length <= 3)
                {
                    termMesh.materials[0].color = ColorCommands.HexToColor(ConfigSettings.TerminalColor.Value); //body
                    termMesh.materials[1].color = ColorCommands.HexToColor(ConfigSettings.TerminalButtonsColor.Value); //glass buttons
                    //2 = warning sticker
                }
                else
                {
                    Plugin.WARNING("termMesh does not have expected number of materials, only setting terminal body color");
                    termMesh.material.color = ColorCommands.HexToColor(ConfigSettings.TerminalColor.Value);
                }
            }
            else
                Plugin.WARNING("customization failure: termMesh is null");
        }

        private static void GetTerminalBodyColors()
        {
            if (!ConfigSettings.TerminalCustomization.Value)
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
                    Plugin.WARNING("termMesh does not have expected number of materials, only setting terminal body color");
                    CustomTerminalStuff.TerminalBody = termMesh.material.color;
                    AnnounceColor(CustomTerminalStuff.TerminalBody, "CustomTerminalStuff.TerminalBody");
                }
            }
            else
                Plugin.WARNING("customization failure: termMesh is null\nFailed to get TerminalBody & TerminalButtonsColor");
        }

        private static void SetTerminalKeyboardColors()
        {
            if (!ConfigSettings.TerminalCustomization.Value)
                return;

            MeshRenderer kbMesh = GameObject.Find("Environment/HangarShip/Terminal/Terminal.003").GetComponent<MeshRenderer>();

            if (kbMesh != null)
            {
                kbMesh.material.color = ColorCommands.HexToColor(ConfigSettings.TerminalKeyboardColor.Value);
            }
            else
                Plugin.WARNING("customization failure: kbMesh is null");
        }

        private static void GetTerminalKeyboardColors()
        {
            if (!ConfigSettings.TerminalCustomization.Value)
                return;

            MeshRenderer kbMesh = GameObject.Find("Environment/HangarShip/Terminal/Terminal.003").GetComponent<MeshRenderer>();

            if (kbMesh != null)
            {
                CustomTerminalStuff.TerminalKeyboard = kbMesh.material.color;
                AnnounceColor(CustomTerminalStuff.TerminalKeyboard, "CustomTerminalStuff.TerminalKeyboard");
            }
            else
                Plugin.WARNING("customization failure: kbMesh is null\nFailed to get TerminalKeyboard color");
        }

        internal static void StartNode()
        {
            Plugin.Spam("Updating home displaytext");
            startNode = Plugin.instance.Terminal.terminalNodes.specialNodes.ToArray()[1];
            string asciiArt = ConfigSettings.HomeTextArt.Value;
            asciiArt = asciiArt.Replace("[leadingSpace]", " ");
            asciiArt = asciiArt.Replace("[leadingSpacex4]", "    ");
            //no known compatibility issues with home screen
            startNode.displayText = $"{ConfigSettings.HomeLine1.Value}\r\n{ConfigSettings.HomeLine2.Value}\r\n\r\n{ConfigSettings.HomeHelpLines.Value}\r\n{asciiArt}\r\n\r\n{ConfigSettings.HomeLine3.Value}\r\n\r\n";
        }

        internal static void TerminalCustomization()
        {
            StartNode(); //should always be modified, not colors

            if (!ConfigSettings.TerminalCustomization.Value)
                return;

            if (!defaultsCached)
                CacheDefaults();

            SetTerminalBodyColors();
            SetTerminalKeyboardColors();
            FontStuff.GetAndSetFont();


            Color moneyBGColor = SetColorFor(ConfigSettings.TerminalMoneyBGColor.Value, CustomTerminalStuff.MoneyBG, ConfigSettings.TerminalMoneyBGAlpha.Value);

            if (Plugin.instance.Terminal.terminalUIScreen.gameObject.transform.GetChild(0).childCount >= 6)
            {
                if (Plugin.instance.Terminal.terminalUIScreen.gameObject.transform.GetChild(0).GetChild(5).gameObject.GetComponent<Image>() != null)
                    Plugin.instance.Terminal.terminalUIScreen.gameObject.transform.GetChild(0).GetChild(5).gameObject.GetComponent<Image>().color = moneyBGColor;
            }
            else
                Plugin.WARNING("Unable to set TerminalMoneyBG customizations!!");
            

            Plugin.instance.Terminal.screenText.textComponent.color = SetColorFor(ConfigSettings.TerminalTextColor.Value, CustomTerminalStuff.TerminalText);
            Plugin.instance.Terminal.topRightText.color = SetColorFor(ConfigSettings.TerminalMoneyColor.Value, CustomTerminalStuff.MoneyText);

            Plugin.instance.Terminal.screenText.caretColor = SetColorFor(ConfigSettings.TerminalCaretColor.Value, CustomTerminalStuff.TextCaret);
            
            if(Plugin.instance.suitsTerminal)
                SuitsTerminalCompatibility.SetCaretColor(Plugin.instance.Terminal.screenText.caretColor);

            Plugin.instance.Terminal.scrollBarVertical.image.color = SetColorFor(ConfigSettings.TerminalScrollbarColor.Value, CustomTerminalStuff.Scrollbar);
            Plugin.instance.Terminal.scrollBarVertical.gameObject.GetComponent<Image>().color = SetColorFor(ConfigSettings.TerminalScrollBGColor.Value, CustomTerminalStuff.ScrollbarBackground);
            Plugin.instance.Terminal.terminalLight.color = SetColorFor(ConfigSettings.TerminalLightColor.Value, CustomTerminalStuff.TerminalLight);

            if (TerminalClockStuff.textComponent != null)
            {
                Plugin.MoreLogs($"setting clock color");
                TerminalClockStuff.textComponent.color = SetColorFor(ConfigSettings.TerminalClockColor.Value, CustomTerminalStuff.TerminalClock);
            }

            terminalBackground = Plugin.instance.Terminal.terminalUIScreen.gameObject.GetComponentInChildren<Image>();

            if (terminalBackground != null)
            {
                terminalBackground.enabled = ConfigSettings.TerminalCustomBG.Value;
                terminalBackground.transform.SetParent(Plugin.instance.Terminal.terminalImage.transform);
                terminalBackground.transform.SetAsLastSibling();
                terminalBackground.rectTransform.anchoredPosition = new Vector2(10, 0);
                terminalBackground.rectTransform.sizeDelta = new Vector2(-80, 20);
                Color bgColor = SetColorFor(ConfigSettings.TerminalCustomBGColor.Value, CustomTerminalStuff.TerminalBackground, ConfigSettings.TerminalCustomBGAlpha.Value);
                terminalBackground.color = bgColor;
            }
            else
                Plugin.Spam("terminalBackground is NULL");

        }

        private static Color SetColorFor(string config, Color cachedColor)
        {
            if ((config.Length < 1 || config.ToLower() == "default") && cachedColor != null)
            {
                Plugin.Spam("setting to cached default value");
                return cachedColor;
            }
            else
            {
                Plugin.Spam($"getting color from {config}");
                return ColorCommands.HexToColor(config);
            }
        }

        private static Color SetColorFor(string config, Color cachedColor, float newAlpha)
        {
            if ((config.Length < 1 || config.ToLower() == "default") && cachedColor != null)
            {
                Plugin.Spam("setting to cached default value");
                return cachedColor;
            }
            else
            {
                Plugin.Spam("getting color from config item");
                Color newColor = ColorCommands.HexToColor(config);
                newColor.a = newAlpha;
                return newColor;
            }
        }

        internal static void GetOtherDefaultColors()
        {
            if (!ConfigSettings.TerminalCustomization.Value)
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
                Plugin.WARNING("Unable to get scrollbar background color cached default!");
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
                Plugin.Spam("terminalBackground is NULL");

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
                Plugin.Spam($"colorValue is null for {name}");
                return;
            }
            string color = ColorUtility.ToHtmlStringRGB(colorValue);
            Plugin.Spam($"{name} cached color: {color}");
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
}

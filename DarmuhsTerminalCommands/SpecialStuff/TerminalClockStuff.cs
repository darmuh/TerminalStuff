using TerminalStuff.Configs;
using TerminalStuff.Util;
using TMPro;
using UnityEngine;

namespace TerminalStuff.SpecialStuff;

internal class TerminalClockStuff
{
    internal static TextMeshProUGUI textComponent = null!;

    public static void ClockUpdate()
    {
        if (textComponent == null || !QoLConfig.TerminalClock.Value || StartOfRound.Instance.inShipPhase || TerminalEvents.clockDisabledByCommand || !StartOfRound.Instance.shipDoorsEnabled)
            return;

        if (Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf && !textComponent.gameObject.activeSelf)
            textComponent.gameObject.SetActive(true);

        string? clockTime = HUDManager.Instance?.clockNumber?.text;
        if (string.IsNullOrEmpty(clockTime))
            return;

        if(clockTime.Contains('\n'))
            textComponent.text = clockTime.Replace("\n", "");
        //Loggers.LogDebug($"Time {textComponent.text}");
    }

    public static bool IsClockVisible()
    {
        if (textComponent == null)
            return false;

        Loggers.LogDebug($"IsClockVisible - {textComponent.gameObject.activeSelf}");

        return textComponent.gameObject.activeSelf;
    }

    public static void SetClockVisible(bool visible)
    {
        if (textComponent == null)
            return;

        if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
        {
            textComponent.gameObject.SetActive(false); //always set textcomponent to false if screen is off
            return;
        }

        if (textComponent.gameObject.activeSelf != visible)
            textComponent.gameObject.SetActive(visible);
    }

    public static void MakeClock()
    {
        if (!QoLConfig.TerminalClock.Value)
            return;

        Loggers.LogDebug("MakeClock!");

        textComponent = MakeTimeText();
        if (textComponent == null)
        {
            Plugin.Log.LogError("CLOCK: Text component creation failed.");
            return;
        }
        Loggers.LogInfo("CLOCK: textcomponent is not null");
    }

    internal static TextMeshProUGUI MakeTimeText()
    {
        Loggers.LogInfo("Start of MakeTimeText");
        // Create a new TextMeshProUGUI GameObject
        GameObject textGO = new("TimeTextAddon");
        Terminal terminal = Plugin.instance.Terminal;

        // Attach the TextMeshProUGUI component to the GameObject
        textComponent = textGO.AddComponent<TextMeshProUGUI>();

        // Set properties of the TextMeshProUGUI
        textComponent.gameObject.SetActive(false);
        textComponent.text = "time";
        textComponent.fontSize = 30;
        textComponent.fontStyle = FontStyles.SmallCaps;
        textComponent.horizontalAlignment = HorizontalAlignmentOptions.Left;
        textComponent.margin = new Vector4(285, -199, 0, 0);
        textComponent.enableWordWrapping = false;


        if (terminal != null && terminal.topRightText != null)
        {
            textComponent.font = terminal.topRightText.font;
            textComponent.color = terminal.topRightText.color;
        }
        else
        {
            textComponent.color = Color.red;
        }

        textComponent.alignment = TextAlignmentOptions.Top;

        // Attach the TextMeshProUGUI to the Canvas
        Canvas canvas = Plugin.instance.Terminal.terminalUIScreen;
        if (canvas != null)
        {
            textGO.transform.SetParent(canvas.transform, false);
            Loggers.LogInfo("textcomponent created and attached to canvas");
            return textComponent;
        }
        else
        {
            Plugin.Log.LogError("CLOCK: Failed to find canvas.");
            return null!;
        }
    }
}

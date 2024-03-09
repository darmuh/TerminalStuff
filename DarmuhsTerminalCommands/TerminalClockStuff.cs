using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.Object;

namespace TerminalStuff
{
    internal class TerminalClockStuff
    {
        internal static TextMeshProUGUI textComponent;
        public static bool showTime = false;

        public static void StartClockCoroutine()
        {
            if (!ConfigSettings.terminalClock.Value)
            {
                Plugin.MoreLogs("clock is not enabled.");
                return;
            }

            Plugin.MoreLogs("StartClockCoroutine called");

            Plugin.Terminal.StartCoroutine(TerminalClockCoroutine(Plugin.Terminal));
        }

        public static void MakeClock()
        {
            if (!ConfigSettings.terminalClock.Value)
                return;

            textComponent = MakeTimeText();
            if (textComponent == null)
            {
                Plugin.Log.LogError("CLOCK: Text component creation failed.");
                return;
            }
            Plugin.MoreLogs("textcomponent is not null");
        }

        internal static IEnumerator TerminalClockCoroutine(Terminal terminal)
        {
            Plugin.MoreLogs("Start of TerminalClock enumerator");
            while (StartOfRound.Instance?.localPlayerController?.isPlayerDead == false &&
                   StartOfRound.Instance.localClientHasControl)
            {
                if (terminal.terminalInUse && showTime)
                {
                    if (!textComponent.gameObject.activeSelf)
                        textComponent.gameObject.SetActive(true);

                    string clockTime = HUDManager.Instance?.clockNumber?.text;
                    if (!string.IsNullOrEmpty(clockTime))
                    {
                        string timeText = clockTime.Replace("\n", "").Replace("\r", "");
                        textComponent.text = timeText;
                    }
                }
                else if (textComponent.gameObject.activeSelf)
                {
                    textComponent.gameObject.SetActive(false);
                }

                yield return new WaitForSecondsRealtime(0.2f);
            }
            Plugin.MoreLogs("while loop ended for clock");
        }

        internal static TextMeshProUGUI MakeTimeText()
        {
            Plugin.MoreLogs("Start of MakeTimeText");
            // Create a new TextMeshProUGUI GameObject
            GameObject textGO = new GameObject("TimeTextAddon");
            Terminal terminal = FindObjectOfType<Terminal>();

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
            Canvas canvas = GameObject.Find("Environment/HangarShip/Terminal/Canvas")?.GetComponent<Canvas>();
            if (canvas != null)
            {
                textGO.transform.SetParent(canvas.transform, false);
                Plugin.MoreLogs("textcomponent created and attached to canvas");
                return textComponent;
            }
            else
            {
                Plugin.Log.LogError("CLOCK: Failed to find canvas.");
                return null;
            }
        }
    }

}

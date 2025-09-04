using TerminalStuff.PluginCore;
using TMPro;
using UnityEngine;

namespace TerminalStuff.VisualCore;

internal class CustomAutoSize : MonoBehaviour
{
    internal RectTransform rect = null!;
    internal TextMeshProUGUI textMesh = null!;
    internal int cachedLength = 0;
    internal float cachedFontSize = 0f;
    internal float fontSizeMultiplier = 1f;
    internal bool initialized = false;

    internal void SetValues(RectTransform rTrans, ref TextMeshProUGUI textComp, Vector2 anchor, Vector3 size)
    {
        rect = rTrans;
        textMesh = textComp;
        rect.anchoredPosition = anchor;
        rect.sizeDelta = size;
        textComp.rectTransform.SetParent(rect);
        Init();
    }

    internal void Init()
    {
        Loggers.LogDebug("Init CustomAutoSize!");
        if (textMesh == null || initialized)
            return;

        textMesh.rectTransform.anchoredPosition = Vector2.zero;
        textMesh.rectTransform.anchorMin = Vector2.zero;
        textMesh.rectTransform.anchorMax = Vector2.one;
        textMesh.rectTransform.sizeDelta = Vector2.zero;
        textMesh.margin = new Vector4(-3, 0, 1, 0);
        textMesh.enableAutoSizing = true;
        initialized = true;
    }

    internal void Update()
    {
        if (textMesh == null)
            return;

        if (textMesh.text == null || !initialized || rect == null)
            return;

        if (cachedLength == textMesh.text.Length)
            return;

        Loggers.LogDebug("Updating rectTransform y value!");

        if (cachedFontSize != textMesh.fontSize)
        {
            fontSizeMultiplier = textMesh.fontSize / 18f;
            cachedFontSize = textMesh.fontSize;
        }

        int multiplier = Mathf.Clamp(textMesh.text.Length - 2, 0, 9);
        int newX = 10 * multiplier;
        int newPosX = 200 - (5 * multiplier);
        newPosX *= -1;

        rect.sizeDelta = new(32 + newX, rect.sizeDelta.y);
        rect.anchoredPosition = new(newPosX, rect.anchoredPosition.y);

        //rect.sizeDelta = new(rect.sizeDelta.x * fontSizeMultiplier, rect.sizeDelta.y * fontSizeMultiplier);
        //rect.anchoredPosition = new(rect.anchoredPosition.x * fontSizeMultiplier, rect.anchoredPosition.y * fontSizeMultiplier);

        cachedLength = textMesh.text.Length;
    }
}

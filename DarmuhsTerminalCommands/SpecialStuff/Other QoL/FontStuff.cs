using BepInEx;
using System;
using System.IO;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TerminalStuff.PluginCore;
using TMPro;
using UnityEngine;

namespace TerminalStuff.SpecialStuff;

internal static class FontStuff
{
    internal static TMP_FontAsset CachedDefault;

    internal static void TestingFonts()
    {
        Loggers.LogDebug("Debug Font Stuff");
        string[] fontNames = Font.GetOSInstalledFontNames();

        foreach (string fontName in fontNames)
        {
            Loggers.LogDebug($"Font: {fontName} detected!");
        }
        // Get paths to OS Fonts
        string[] fontPaths = Font.GetPathsToOSFonts();

        // Create new font object from one of those paths
        //List<Font> osFonts = [];
        foreach (string fontPath in fontPaths)
        {
            Loggers.LogDebug($"FontPath: {fontPath} detected!");
            //Font newFont = new Font(fontPath);
            // Create new dynamic font asset
            //TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(newFont);
        }
    }

    internal static void SetCachedDefault()
    {
        CachedDefault = Plugin.instance.Terminal.screenText.textComponent.font;
        Loggers.LogDebug($"Caching default fontasset");
        //foreach(string font in CachedDefault.sourceFontFile.fontNames)
        //{
        //    Loggers.LogDebug(font);
        //}
    }

    internal static bool TryGetCustomOSFont(string fontName, out TMP_FontAsset CustomFontAsset)
    {
        // Get paths to OS Fonts
        string[] fontPaths = Font.GetPathsToOSFonts();

        // Create new font object from one of those paths
        //List<Font> osFonts = new List<Font>();
        foreach (string fontPath in fontPaths)
        {
            if (fontPath.Contains(fontName))
            {
                Loggers.LogDebug($"FontPath: {fontPath} detected!");
                Font newFont = new(fontPath);
                // Create new dynamic font asset
                CustomFontAsset = TMP_FontAsset.CreateFontAsset(newFont);
                return true;
            }
        }
        CustomFontAsset = null;
        return false;
    }

    internal static void GetAndSetFont()
    {
        if (OpenLib.Common.Misc.CompareStringsInvariant(CustomizeConfig.CustomFontName.Value, "default") || CustomizeConfig.CustomFontName.Value.Length < 1)
        {
            Loggers.LogDebug("assigning cached default font");
            SetTerminalFont(CachedDefault);
            return;
        }

        if (TryGetFontFromCustomPath(CustomizeConfig.CustomFontName.Value, out TMP_FontAsset customFont))
        {
            Loggers.LogDebug($"{CustomizeConfig.CustomFontName.Value} found in custom fonts path - {CustomizeConfig.CustomFontPath.Value}");
            SetTerminalFont(customFont);
            return;
        }

        if (TryGetCustomOSFont(CustomizeConfig.CustomFontName.Value, out TMP_FontAsset osFont))
        {
            Loggers.LogDebug($"{CustomizeConfig.CustomFontName.Value} found in system fonts!");
            SetTerminalFont(osFont);
            return;
        }

        if (TryGetCustomFont(CustomizeConfig.CustomFontName.Value, out TMP_FontAsset newFont))
        {
            Loggers.LogDebug($"{CustomizeConfig.CustomFontName.Value} found in windows custom fonts path");
            SetTerminalFont(newFont);
            return;
        }

        Loggers.LogDebug($"Unable to find {CustomizeConfig.CustomFontName.Value} in system fonts, in windows fonts path, or custom fonts path {CustomizeConfig.CustomFontPath.Value}");
    }

    internal static bool TryGetFontFromCustomPath(string fontName, out TMP_FontAsset CustomFontAsset)
    {
        if (CustomizeConfig.CustomFontPath.Value.Length < 1)
        {
            CustomFontAsset = null;
            return false;
        }

        string path = Path.Combine(Paths.ConfigPath, CustomizeConfig.CustomFontPath.Value);
        Loggers.LogDebug($"custom path: {path}");
        if (Directory.Exists(path))
        {
            string fullPath = Path.Combine(path, fontName);
            if (File.Exists(fullPath))
            {
                Font customFont = new(fullPath);
                Loggers.LogDebug($"attempting to create custom font from fontname: {fontName}");
                if (customFont != null)
                {
                    int fontFaceResult = (int)UnityEngine.TextCore.LowLevel.FontEngine.LoadFontFace(customFont);
                    Loggers.LogDebug(fontName + ": " + fontFaceResult);
                    CustomFontAsset = TMP_FontAsset.CreateFontAsset(customFont);
                    return true;
                }
                Loggers.LogDebug("customFont is null returning false");
                CustomFontAsset = null;
                return false;
            }
            Loggers.LogDebug("Font File could not be found");
            CustomFontAsset = null;
            return false;
        }
        Loggers.LogDebug("Custom Font Directory could not be found");
        CustomFontAsset = null;
        return false;
    }

    internal static bool TryGetCustomFont(string fontName, out TMP_FontAsset CustomFontAsset)
    {
        string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        //Loggers.LogDebug(localAppData);
        string fullPath = Path.Combine(localAppData, "Microsoft\\Windows\\Fonts\\");
        //Loggers.LogDebug(fullPath);
        Loggers.LogDebug(fullPath + fontName);
        if (Directory.Exists(fullPath) && File.Exists(Path.Combine(fullPath + fontName)))
        {
            Font customFont = new(Path.Combine(fullPath + fontName));
            Loggers.LogDebug($"attempting to create custom font from fontname: {fontName}");
            if (customFont != null)
            {
                int fontFaceResult = (int)UnityEngine.TextCore.LowLevel.FontEngine.LoadFontFace(customFont);
                Loggers.LogDebug(fontName + ": " + fontFaceResult);
                CustomFontAsset = TMP_FontAsset.CreateFontAsset(customFont);
                return true;
            }
            Loggers.LogDebug("customFont is null returning false");
            CustomFontAsset = null;
            return false;
        }
        Loggers.LogDebug("Directory & file could not be found");
        CustomFontAsset = null;
        return false;

    }

    internal static void SetTerminalFont(TMP_FontAsset newFont)
    {
        Loggers.LogDebug("SetTerminalFont called to switch font");
        Plugin.instance.Terminal.screenText.fontAsset = newFont;
        Plugin.instance.Terminal.topRightText.font = newFont;
        if (TerminalClockStuff.textComponent != null)
            TerminalClockStuff.textComponent.font = newFont;

        if (CustomizeConfig.CustomFontSizeMain.Value > -1)
        {
            if (Plugin.instance.LethalLevelLoader)
                LLLCompat.UpdateLLLFontSize((float)CustomizeConfig.CustomFontSizeMain.Value);
            else
                Plugin.instance.Terminal.screenText.textComponent.fontSize = CustomizeConfig.CustomFontSizeMain.Value;
        }

        if (CustomizeConfig.CustomFontSizeMoney.Value > -1)
            Plugin.instance.Terminal.topRightText.fontSize = CustomizeConfig.CustomFontSizeMoney.Value;

        if (TerminalClockStuff.textComponent != null && CustomizeConfig.CustomFontSizeClock.Value > -1)
            TerminalClockStuff.textComponent.fontSize = CustomizeConfig.CustomFontSizeClock.Value;
    }
}

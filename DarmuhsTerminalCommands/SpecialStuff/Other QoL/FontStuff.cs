using BepInEx;
using System;
using System.IO;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TMPro;
using UnityEngine;

namespace TerminalStuff.SpecialStuff
{
    internal static class FontStuff
    {
        internal static TMP_FontAsset CachedDefault;

        internal static void TestingFonts()
        {
            Plugin.Spam("Debug Font Stuff");
            string[] fontNames = Font.GetOSInstalledFontNames();

            foreach (string fontName in fontNames)
            {
                Plugin.Spam($"Font: {fontName} detected!");
            }
            // Get paths to OS Fonts
            string[] fontPaths = Font.GetPathsToOSFonts();

            // Create new font object from one of those paths
            //List<Font> osFonts = [];
            foreach (string fontPath in fontPaths)
            {
                Plugin.Spam($"FontPath: {fontPath} detected!");
                //Font newFont = new Font(fontPath);
                // Create new dynamic font asset
                //TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(newFont);
            }
        }

        internal static void SetCachedDefault()
        {
            CachedDefault = Plugin.instance.Terminal.screenText.textComponent.font;
            Plugin.Spam($"Caching default fontasset");
            //foreach(string font in CachedDefault.sourceFontFile.fontNames)
            //{
            //    Plugin.Spam(font);
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
                    Plugin.Spam($"FontPath: {fontPath} detected!");
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
            if (CustomizeConfig.CustomFontName.Value.ToLower() == "default" || CustomizeConfig.CustomFontName.Value.Length < 1)
            {
                Plugin.Spam("assigning cached default font");
                SetTerminalFont(CachedDefault);
                return;
            }

            if (TryGetFontFromCustomPath(CustomizeConfig.CustomFontName.Value, out TMP_FontAsset customFont))
            {
                Plugin.Spam($"{CustomizeConfig.CustomFontName.Value} found in custom fonts path - {CustomizeConfig.CustomFontPath.Value}");
                SetTerminalFont(customFont);
                return;
            }

            if (TryGetCustomOSFont(CustomizeConfig.CustomFontName.Value, out TMP_FontAsset osFont))
            {
                Plugin.Spam($"{CustomizeConfig.CustomFontName.Value} found in system fonts!");
                SetTerminalFont(osFont);
                return;
            }

            if (TryGetCustomFont(CustomizeConfig.CustomFontName.Value, out TMP_FontAsset newFont))
            {
                Plugin.Spam($"{CustomizeConfig.CustomFontName.Value} found in windows custom fonts path");
                SetTerminalFont(newFont);
                return;
            }

            Plugin.Spam($"Unable to find {CustomizeConfig.CustomFontName.Value} in system fonts, in windows fonts path, or custom fonts path {CustomizeConfig.CustomFontPath.Value}");
        }

        internal static bool TryGetFontFromCustomPath(string fontName, out TMP_FontAsset CustomFontAsset)
        {
            if (CustomizeConfig.CustomFontPath.Value.Length < 1)
            {
                CustomFontAsset = null;
                return false;
            }

            string path = Path.Combine(Paths.ConfigPath, CustomizeConfig.CustomFontPath.Value);
            Plugin.Spam($"custom path: {path}");
            if (Directory.Exists(path))
            {
                string fullPath = Path.Combine(path, fontName);
                if (File.Exists(fullPath))
                {
                    Font customFont = new(fullPath);
                    Plugin.Spam($"attempting to create custom font from fontname: {fontName}");
                    if (customFont != null)
                    {
                        int fontFaceResult = (int)UnityEngine.TextCore.LowLevel.FontEngine.LoadFontFace(customFont);
                        Plugin.Spam(fontName + ": " + fontFaceResult);
                        CustomFontAsset = TMP_FontAsset.CreateFontAsset(customFont);
                        return true;
                    }
                    Plugin.Spam("customFont is null returning false");
                    CustomFontAsset = null;
                    return false;
                }
                Plugin.Spam("Font File could not be found");
                CustomFontAsset = null;
                return false;
            }
            Plugin.Spam("Custom Font Directory could not be found");
            CustomFontAsset = null;
            return false;
        }

        internal static bool TryGetCustomFont(string fontName, out TMP_FontAsset CustomFontAsset)
        {
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            //Plugin.Spam(localAppData);
            string fullPath = Path.Combine(localAppData, "Microsoft\\Windows\\Fonts\\");
            //Plugin.Spam(fullPath);
            Plugin.Spam(fullPath + fontName);
            if (Directory.Exists(fullPath) && File.Exists(Path.Combine(fullPath + fontName)))
            {
                Font customFont = new(Path.Combine(fullPath + fontName));
                Plugin.Spam($"attempting to create custom font from fontname: {fontName}");
                if (customFont != null)
                {
                    int fontFaceResult = (int)UnityEngine.TextCore.LowLevel.FontEngine.LoadFontFace(customFont);
                    Plugin.Spam(fontName + ": " + fontFaceResult);
                    CustomFontAsset = TMP_FontAsset.CreateFontAsset(customFont);
                    return true;
                }
                Plugin.Spam("customFont is null returning false");
                CustomFontAsset = null;
                return false;
            }
            Plugin.Spam("Directory & file could not be found");
            CustomFontAsset = null;
            return false;

        }

        internal static void SetTerminalFont(TMP_FontAsset newFont)
        {
            Plugin.Spam("SetTerminalFont called to switch font");
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
}

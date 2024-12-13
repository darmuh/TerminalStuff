using OpenLib.Common;
using OpenLib.CoreMethods;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static OpenLib.ConfigManager.ConfigSetup;
using static OpenLib.CoreMethods.AddingThings;
using static TerminalStuff.BoolStuff;
using static TerminalStuff.EventSub.TerminalQuit;

namespace TerminalStuff
{
    internal class SplitViewChecks : MonoBehaviour
    {
        internal static bool enabledSplitObjects = false;
        internal static bool replacedViewMon = false;
        internal static RawImage miniScreenImage;

        private static void HandleVanillaMap(bool shouldRemove)
        {
            if (shouldRemove)
            {
                ReplaceViewMonitor();
            }
        }

        private static void ReplaceViewMonitor()
        {
            if (!DynamicBools.TryGetKeyword("view monitor"))
            {
                AddNodeManual("View Monitor Not Available", "view monitor", ViewCommands.NoVanillaView, true, 0, defaultListing);
                Plugin.Spam("Added view monitor warning");
            }
            else
                Plugin.Spam("keyword already exists!");
        }

        public static void InitSplitViewObjects()
        {
            if (!ShouldAddCamsLogic())
            {
                HandleVanillaMap(false);
                return;
            }
            else if (Plugin.instance.OpenBodyCamsMod && OpenBodyCamsCompatibility.IsCreatingCommands())
            {
                Plugin.MoreLogs("Not replacing 'view monitor' command, as OpenBodyCams is creating terminal commands");
                return;
            }

            else
                HandleVanillaMap(true);


            if (Plugin.instance.Terminal.terminalImage == null || Plugin.instance.splitViewCreated)
            {
                Plugin.MoreLogs("Original terminalImage not found or split view already created");
                return;
            }

            miniScreenImage = Instantiate(Plugin.instance.Terminal.terminalImage, Plugin.instance.Terminal.terminalImage.transform);

            if (miniScreenImage.gameObject.GetComponent<VideoPlayer>() != null)
            {
                VideoPlayer extraPlayer = miniScreenImage.GetComponent<VideoPlayer>();
                Destroy(extraPlayer);
                Plugin.Spam("extraPlayer deleted");
            }

            miniScreenImage.gameObject.name = "MiniScreen";
            Plugin.instance.Terminal.terminalImage.gameObject.name = "terminalImage";

            Plugin.instance.splitViewCreated = true;
        }

        public static void CheckForSplitView(string whatIsIt)
        {
            if (!Plugin.instance.splitViewCreated && whatIsIt == "mirror")
            {
                DisableVanillaViewMonitor(false);
                UpdatePluginInstanceBools(whatIsIt);
                return;
            }
            else if (!Plugin.instance.splitViewCreated)
            {
                DisableVanillaViewMonitor();
                ResetPluginInstanceBools();
                return;
            }


            List<string> singleViewModes = ["cams", "map", "mirror", "single"];
            List<string> multiViewModes = ["minicams", "minimap", "overlay", "multi"];

            if (enabledSplitObjects == false)
            {
                SetMiniScreen(whatIsIt, false);
                ResetPluginInstanceBools();
            }
            else if (whatIsIt == "neither")
            {
                enabledSplitObjects = false;
                SetMiniScreen(whatIsIt, false);
                ResetPluginInstanceBools();
            }
            else if (enabledSplitObjects == true && (multiViewModes.Contains(whatIsIt)))
            {
                SetMiniScreen(whatIsIt, true);
                ViewCommands.SetAnyCamsTrue();
                UpdatePluginInstanceBools(whatIsIt);
            }
            else if (enabledSplitObjects == true && (singleViewModes.Contains(whatIsIt)))
            {
                SetMiniScreen(whatIsIt, false);
                UpdatePluginInstanceBools(whatIsIt);
            }
            else
            {
                Plugin.WARNING($"No matches for CheckForSplitView [ {whatIsIt} ]");
            }
        }

        private static void SetMiniScreen(string whatIsIt, bool state)
        {
            if (miniScreenImage == null)
                return;

            if (miniScreenImage.enabled == state)
                return;

            miniScreenImage.enabled = state;
            Plugin.MoreLogs($"{whatIsIt} has set miniscreen to {state}");
        }

        internal static void ResetPluginInstanceBools()
        {
            MoreCamStuff.ResetPluginInstanceBools();
        }

        internal static void EnableSplitView(string whatIsIt)
        {
            enabledSplitObjects = true;
            CheckForSplitView(whatIsIt);
            UpdatePluginInstanceBools(whatIsIt);
            ShowCameraView(true);
        }

        internal static void DisableVanillaViewMonitor(bool disableImage = true)
        {
            Plugin.Spam("disabling vanilla view monitor");
            Plugin.instance.Terminal.displayingPersistentImage = null;
            Plugin.instance.Terminal.terminalImage.enabled = !disableImage;
        }

        internal static void DisableSplitView(string whatIsIt)
        {
            if (!Plugin.instance.splitViewCreated)
            {
                DisableVanillaViewMonitor();
                ResetPluginInstanceBools();
                return;
            }

            enabledSplitObjects = false;
            CheckForSplitView(whatIsIt);
            ShowCameraView(false);
        }

        internal static void ShowCameraView(bool state)
        {
            if (ConfigSettings.CamsUseDetectedMods.Value && Plugin.instance.OpenBodyCamsMod)
            {
                OBCTerminalCameraStatus(state);
            }
            else
                CamStuff.HomebrewCameraState(state, ViewCommands.playerCam);
        }

        private static void UpdatePluginInstanceBools(string whatIsIt)
        {
            ResetPluginInstanceBools();

            switch (whatIsIt)
            {
                case "minimap":
                    Plugin.instance.isOnMiniMap = true;
                    break;
                case "minicams":
                    Plugin.instance.isOnMiniCams = true;
                    break;
                case "overlay":
                    Plugin.instance.isOnOverlay = true;
                    break;
                case "cams":
                    Plugin.instance.isOnCamera = true;
                    break;
                case "mirror":
                    Plugin.instance.isOnMirror = true;
                    break;
                case "map":
                    Plugin.instance.isOnMap = true;
                    break;
                case "neither":
                    Plugin.MoreLogs("whatIsIt - neither, all instance bools reset for views");
                    break;
                default:
                    Plugin.MoreLogs($"Unexpected value for whatIsIt: {whatIsIt}");
                    break;
            }
        }
    }
}

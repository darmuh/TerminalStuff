using GameNetcodeStuff;
using OpenLib.Common;
using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using UnityEngine;
using static TerminalStuff.AllMyTerminalPatches;
using static TerminalStuff.ViewCommands;

namespace TerminalStuff
{
    internal class MoreCamStuff //UPDATE excludedNames to configItem Names for Nodes that dont specify nodeName!!!
    {
        internal static void ResetPluginInstanceBools()
        {
            Plugin.instance.isOnMiniMap = false;
            Plugin.instance.isOnMiniCams = false;
            Plugin.instance.isOnMap = false;
            Plugin.instance.isOnCamera = false;
            Plugin.instance.isOnMirror = false;
            Plugin.instance.isOnOverlay = false;
            Plugin.instance.activeCam = false;
            NetHandler.SyncMyCamsBoolToEveryone(false);
        }

        internal static List<string> excludedNames =
                //stuff that should not disable cams
                [
                    "ViewInsideShipCam 1",
                    "TerminalRadarZoom",
                    "terminalStuff Mirror",
                    "TerminalDoor",
                    "TerminalLights",
                    "TerminalAlwaysOnCommand",
                    "Use Inverse Teleporter",
                    "Use Teleporter",
                    "TerminalClear",
                    "TerminalDanger",
                    "TerminalVitals",
                    "TerminalHeal",
                    "TerminalLoot",
                    "TerminalRandomSuit",
                    "TerminalClockCommand",
                    "TerminalPrevious",
                    "SwitchRadarCamPlayer 1",
                    "SwitchedCam",
                    "switchDummy",
                    "EnteredCode",
                    "FlashedRadarBooster",
                    "SendSignalTranslator",
                    "GeneralError",
                    "ParserError1",
                    "ParserError2",
                    "ParserError3",
                    "PingedRadarBooster",
                    "SendSignalTranslator",
                    "FinishedRadarBooster",
                ];

        internal static void VideoPersist(string nodeName)
        {
            if (isVideoPlaying && nodeName != "darmuh's videoPlayer")
            {
                FixVideoPatch.OnVideoEnd(Plugin.instance.Terminal);
                isVideoPlaying = false;
                //Plugin.Log.LogInfo("isVideoPlaying set to FALSE");
                Plugin.MoreLogs("disabling video");
            }
        }

        internal static void CamPersistance(string nodeName, TerminalNode node = null)
        {
            if (!excludedNames.Contains(nodeName) && HideCams())
            {
                SplitViewChecks.DisableSplitView("neither");
                Plugin.MoreLogs("disabling ANY cams views");
            }
            else if (nodeName == "ViewInsideShipCam 1" && node != null)
            {
                if (!IsViewNode(node))
                    ResetPluginInstanceBools();
            }
        }

        private static bool IsViewNode(TerminalNode node)
        {
            if (Commands.ViewConfig.Count == 0)
                return false;

            foreach (var item in Commands.ViewConfig)
            {
                if (item.TerminalNode == node)
                    return true;
            }

            Plugin.Spam("this node is not a managed node, resetting instance variables");
            return false;
        }

        internal static bool HideCams()
        {
            return !ConfigSettings.MonitoringNeverHide.Value;
        }

        internal static Texture GetPlayerCamsFromExternalMod(int newTarget)
        {
            if (Plugin.instance.OpenBodyCamsMod)
            {
                Plugin.Spam("Sending to OBC for camera info");
                OpenLib.Compat.OpenBodyCamFuncs.UpdateCamsTarget(ConfigSettings.ObcResolutionBodyCam.Value);
                return OpenLib.Compat.OpenBodyCamFuncs.GetTexture(OpenLib.Compat.OpenBodyCamFuncs.TerminalBodyCam);
            }
            else if (Plugin.instance.SolosBodyCamsMod || Plugin.instance.HelmetCamsMod)
            {
                Plugin.Spam("Grabbing monitor texture for other external bodycams mods");
                return PlayerCamsCompatibility.PlayerCamTexture();
            }
            else
            {
                Plugin.Spam("No external mods detected, defaulting to internal cams system.");
                    return UpdateCamsTarget(newTarget);
            }
        }

        internal static bool IsExternalCamsPresent()
        {
            if (ConfigSettings.CamsUseDetectedMods.Value && (Plugin.instance.HelmetCamsMod || Plugin.instance.SolosBodyCamsMod || Plugin.instance.OpenBodyCamsMod))
                return true;
            else
                return false;
        }


        internal static Texture UpdateCamsTarget(int targetNum)
        {

            if (ConfigSettings.CamsUseDetectedMods.Value && (Plugin.instance.HelmetCamsMod || Plugin.instance.OpenBodyCamsMod || Plugin.instance.SolosBodyCamsMod))
                return PlayerCamsCompatibility.PlayerCamTexture();

            if (!GameStuff.TerminalMapRenderer.radarTargets[targetNum].isNonPlayer)
            {
                Plugin.Spam($"Using internal mod camera on valid player - {targetNum}");
                return PlayerCamTexture(targetNum);
            }
            else
            {
                Plugin.Spam("Using internal mod camera on valid non-player");
                return RadarCamTexture(targetNum);
            }
        }

        private static Texture PlayerCamTexture(int targetPlayer)
        {

            if (playerCam == null)
            {
                Plugin.MoreLogs("Creating home-brew PlayerCam");
                playerCam = CamStuff.HomebrewCam(ref mycamTexture, ref CamStuff.MyCameraHolder);
            }

            playerCam.orthographic = false;
            playerCam.enabled = true;
            playerCam.cameraType = CameraType.Game;

            Transform camTransform;
            PlayerControllerB targetedPlayer = GameStuff.TerminalMapRenderer.radarTargets[targetPlayer].transform.gameObject.GetComponent<PlayerControllerB>();
            if (targetedPlayer != null)
            {
                camTransform = targetedPlayer.gameplayCamera.transform;
                Plugin.MoreLogs($"Valid player for cams update {targetedPlayer.playerUsername}");
            }
            else
            {
                camTransform = GameStuff.TerminalMapRenderer.radarTargets[targetPlayer].transform;
                Plugin.MoreLogs($"Invalid player{targetPlayer} for cams update, sending to backup trasnsform");
            }

            playerCam.transform.rotation = camTransform.rotation;
            playerCam.transform.position = camTransform.transform.position;

            playerCam.farClipPlane = 25f;
            playerCam.nearClipPlane = 0.5f;
            playerCam.fieldOfView = 90f;
            playerCam.transform.SetParent(camTransform.transform);
            Texture spectateTexture = playerCam.targetTexture;
            return spectateTexture;
        }

        private static Texture RadarCamTexture(int targetNum)
        {

            if (playerCam == null)
            {
                Plugin.MoreLogs("Creating home-brew PlayerCam");
                playerCam = CamStuff.HomebrewCam(ref mycamTexture, ref CamStuff.MyCameraHolder);
            }

            playerCam.orthographic = false;
            playerCam.enabled = true;
            playerCam.cameraType = CameraType.SceneView;
            Transform camTransform = GameStuff.TerminalMapRenderer.radarTargets[targetNum].transform;
            playerCam.transform.rotation = camTransform.rotation;
            playerCam.transform.position = camTransform.transform.position;

            playerCam.farClipPlane = 50f;
            playerCam.nearClipPlane = 0.4f;
            playerCam.fieldOfView = 110f;
            playerCam.transform.SetParent(camTransform.transform);
            Texture spectateTexture = playerCam.targetTexture;
            return spectateTexture;
        }

        internal static int GetNextValidTarget(List<TransformAndName> targets, int initialIndex) //copied from TwoRadarMaps, slightly modified
        {
            int count = targets.Count;
            for (int i = 1; i < count; i++) //modified i to start at 1 to get next target rather than current target
            {
                int num = (initialIndex + i) % count;
                if (TargetIsValid(targets[num]?.transform))
                {
                    return num;
                }
            }

            return initialIndex; //changed this to return the original number if there are no other valid targets than the current one
        }

        internal static int GetPrevValidTarget(List<TransformAndName> targets, int initialIndex)
        {
            int count = targets.Count;
            int nextTarget = initialIndex;
            Plugin.Spam($"Count: {targets.Count}");
            Plugin.Spam($"initialIndex: {initialIndex}");

            // Handle the case when initialIndex is zero
            if (initialIndex == 0)
            {
                nextTarget = count;
                Plugin.Spam($"initialIndex is 0, setting nextTarget to {nextTarget}");
            }

            // Iterate through the list of targets
            for (int i = 1; i < count; i++)
            {
                // Calculate the index of the previous target
                int num = (nextTarget - i) % count;

                Plugin.Spam($"{num} = {nextTarget} - {i} % {count}");
                Plugin.Spam($"{num} + {count} % {count}");
                // Ensure num is non-negative
                num = (num + count) % count;
                Plugin.Spam($"= {num}");
                // Check if the target at the calculated index is valid
                if (TargetIsValid(targets[num]?.transform))
                {
                    return num;
                }
            }

            // If no valid target is found, return the original index
            return initialIndex;
        }
        internal static bool TargetIsValid(Transform targetTransform) //copied from TwoRadarMaps, added log statements just to see how it works
        {
            if (targetTransform == null)
            {
                Plugin.MoreLogs("not a valid target");
                return false;
            }

            PlayerControllerB component = targetTransform.transform.GetComponent<PlayerControllerB>();
            if (component == null)
            {
                Plugin.MoreLogs("Null player component, must be radar (returning true)");
                return true;
            }

            if (!component.isPlayerControlled && !component.isPlayerDead)
            {
                Plugin.MoreLogs($"player is not player controlled and is not dead, redirect to enemy: {component.redirectToEnemy != null}");
                return component.redirectToEnemy != null;
            }

            Plugin.MoreLogs("TargetIsValid, no specific conditions met");
            return true;
        }

        internal static void OnTargetSwitch(int newTarget)
        {
            Plugin.Spam("Target Switch Event!");

            if (!AnyActiveMonitoring())
                return;

            if (newTarget > GameStuff.TerminalMapRenderer.radarTargets.Count)
                return;

            if (!IsExternalCamsPresent())
            {
                Plugin.MoreLogs("Updating homebrew target");
                UpdateCamsTarget(newTarget);
                return;
            }
            else
            {
                if (Plugin.instance.OpenBodyCamsMod && !OpenLib.Compat.OpenBodyCamFuncs.ShowingBodyCam)
                    Plugin.MoreLogs("OBC Terminal Body Cam is NOT active");
                else
                    GetPlayerCamsFromExternalMod(newTarget);
            }
        }
    }
}

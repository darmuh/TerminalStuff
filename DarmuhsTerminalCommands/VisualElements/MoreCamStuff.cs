using GameNetcodeStuff;
using OpenLib.Common;
using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using TerminalStuff.Util;
using UnityEngine;
using static TerminalStuff.Patching.AllMyTerminalPatches;
using static TerminalStuff.CommandHandling.ViewCommands;
using TerminalStuff.Compatibility;

namespace TerminalStuff.VisualElements;

internal class MoreCamStuff
{

    internal static List<string> DontHideMonitoringNodes
    {
        get
        {
            List<string> result = CommonStringStuff.GetKeywordsPerConfigItem(ConfigSettings.KeepMonitoringNodes.Value, ',');

            foreach(var node in Commands.GetSpecialCommands())
            {
                result.Add(node.Name);
            }

            return result;
        }
    }

    internal static void CheckVisualPersistance(string nodeName)
    {
        VideoPersist(nodeName);
        CamPersistance(nodeName);
    }
    private static void VideoPersist(string nodeName)
    {
        if (isVideoPlaying && nodeName != "darmuh's videoPlayer")
        {
            FixVideoPatch.OnVideoEnd();
            isVideoPlaying = false;
            //Plugin.Log.LogInfo("isVideoPlaying set to FALSE");
            Loggers.LogInfo("disabling video");
        }
    }

    private static void CamPersistance(string nodeName)
    {
        // dont mess with vanilla view monitor persistance
        if (CurrentView == ViewMode.Vanilla || (CurrentView == ViewMode.None && nodeName == "ViewInsideShipCam 1"))
            return;

        if (!OpenLib.Common.Misc.DoesListHaveInvariant(DontHideMonitoringNodes, nodeName) && CanHideCams())
        {
            CamEvents.UpdateCamsEvent.Invoke(ViewMode.None);
            Loggers.LogInfo("disabling ANY cams views");
        }
    }

    internal static bool IsViewNode(TerminalNode node)
    {
        var viewCommands = Commands.GetSpecialCommands();
        if (viewCommands.Count == 0)
            return false;

        foreach (var item in viewCommands)
        {
            if (item.terminalNode == node)
                return true;
        }

        Loggers.LogDebug("this node is not a view node, resetting instance variables");
        return false;
    }

    internal static bool CanHideCams()
    {
        return !string.IsNullOrEmpty(ConfigSettings.KeepMonitoringNodes.Value);
    }

    internal static Texture GetPlayerCamsFromExternalMod(int newTarget)
    {
        if (Plugin.instance.OpenBodyCamsMod)
        {
            Loggers.LogDebug("Sending to OBC for camera info");
            OpenLib.Compat.OpenBodyCamFuncs.UpdateCamsTarget(ConfigSettings.ObcResolutionBodyCam.Value);
            return OpenLib.Compat.OpenBodyCamFuncs.GetTexture(OpenLib.Compat.OpenBodyCamFuncs.TerminalBodyCam);
        }
        else if (Plugin.instance.SolosBodyCamsMod || Plugin.instance.HelmetCamsMod)
        {
            Loggers.LogDebug("Grabbing monitor texture for other external bodycams mods");
            return PlayerCamsCompatibility.PlayerCamTexture();
        }
        else
        {
            Loggers.LogDebug("No external mods detected, defaulting to internal cams system.");
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
            Loggers.LogDebug($"Using internal mod camera on valid player - {targetNum}");
            return PlayerCamTexture(targetNum);
        }
        else
        {
            Loggers.LogDebug("Using internal mod camera on valid non-player");
            return RadarCamTexture(targetNum);
        }
    }

    private static RenderTexture PlayerCamTexture(int targetPlayer)
    {
        if (playerCam == null)
        {
            GameObject camHolder = CamEvents.CameraHolder;
            Loggers.LogInfo("Creating home-brew PlayerCam");
            playerCam = CamStuff.HomebrewCam(ref mycamTexture, ref camHolder);
            CamEvents.CameraHolder = camHolder;
        }

        playerCam.orthographic = false;
        ToggleCameraState(true);
        playerCam.cameraType = CameraType.Game;

        Transform camTransform;
        PlayerControllerB targetedPlayer = GameStuff.TerminalMapRenderer.radarTargets[targetPlayer].transform.gameObject.GetComponent<PlayerControllerB>();
        if (targetedPlayer != null)
        {
            camTransform = targetedPlayer.gameplayCamera.transform;
            Loggers.LogInfo($"Valid player for cams update {targetedPlayer.playerUsername}");
        }
        else
        {
            camTransform = GameStuff.TerminalMapRenderer.radarTargets[targetPlayer].transform;
            Loggers.LogInfo($"Invalid player{targetPlayer} for cams update, sending to backup trasnsform");
        }

        playerCam.transform.rotation = camTransform.rotation;
        playerCam.transform.position = camTransform.transform.position;

        playerCam.farClipPlane = 25f;
        playerCam.nearClipPlane = 0.5f;
        playerCam.fieldOfView = 90f;
        playerCam.transform.SetParent(camTransform.transform);
        return playerCam.targetTexture;
    }

    private static RenderTexture RadarCamTexture(int targetNum)
    {
        if (playerCam == null)
        {
            GameObject camHolder = CamEvents.CameraHolder;
            Loggers.LogInfo("Creating home-brew PlayerCam");
            playerCam = CamStuff.HomebrewCam(ref mycamTexture, ref camHolder);
        }

        playerCam.orthographic = false;
        ToggleCameraState(true);
        playerCam.cameraType = CameraType.SceneView;
        Transform camTransform = GameStuff.TerminalMapRenderer.radarTargets[targetNum].transform;
        playerCam.transform.rotation = camTransform.rotation;
        playerCam.transform.position = camTransform.transform.position;

        playerCam.farClipPlane = 50f;
        playerCam.nearClipPlane = 0.4f;
        playerCam.fieldOfView = 110f;
        playerCam.transform.SetParent(camTransform.transform);
        return playerCam.targetTexture;
    }

    internal static int GetNextValidTarget(List<TransformAndName> targets, int initialIndex) //copied from TwoRadarMaps, slightly modified
    {
        int count = targets.Count;
        for (int i = 1; i < count; i++) //modified i to start at 1 to get next target rather than current target
        {
            int num = (initialIndex + i) % count;
            if (TargetIsValid(targets[num]))
                return num;
        }

        return initialIndex; //changed this to return the original number if there are no other valid targets than the current one
    }

    internal static int GetPrevValidTarget(List<TransformAndName> targets, int initialIndex)
    {
        int count = targets.Count;
        int nextTarget = initialIndex;
        Loggers.LogDebug($"Count: {targets.Count}");
        Loggers.LogDebug($"initialIndex: {initialIndex}");

        // Handle the case when initialIndex is zero
        if (initialIndex == 0)
        {
            nextTarget = count;
            Loggers.LogDebug($"initialIndex is 0, setting nextTarget to {nextTarget}");
        }

        // Iterate through the list of targets
        for (int i = 1; i < count; i++)
        {
            // Calculate the index of the previous target
            int num = (nextTarget - i) % count;

            Loggers.LogDebug($"{num} = {nextTarget} - {i} % {count}");
            Loggers.LogDebug($"{num} + {count} % {count}");
            // Ensure num is non-negative
            num = (num + count) % count;
            Loggers.LogDebug($"= {num}");
            // Check if the target at the calculated index is valid
            if (TargetIsValid(targets[num]))
                return num;
        }

        // If no valid target is found, return the original index
        return initialIndex;
    }
    internal static bool TargetIsValid(TransformAndName target) //copied from TwoRadarMaps, added log statements just to see how it works
    {
        if (target == null) return false;

        var targetTransform = target.transform;

        if (targetTransform == null)
        {
            Loggers.LogInfo("not a valid target");
            return false;
        }

        PlayerControllerB component = targetTransform.transform.GetComponent<PlayerControllerB>();
        if (component == null)
        {
            Loggers.LogInfo("Null player component, must be radar (returning true)");
            return true;
        }

        if (!component.isPlayerControlled && !component.isPlayerDead)
        {
            Loggers.LogInfo($"player is not player controlled and is not dead, redirect to enemy: {component.redirectToEnemy != null}");
            return component.redirectToEnemy != null;
        }

        Loggers.LogInfo("TargetIsValid, no specific conditions met");
        return true;
    }

    internal static void OnTargetSwitch(int newTarget)
    {
        Loggers.LogDebug("Target Switch Event!");

        if (!AnyActiveMonitoring())
            return;

        if (newTarget > GameStuff.TerminalMapRenderer.radarTargets.Count)
            return;

        if (!IsExternalCamsPresent())
        {
            Loggers.LogInfo("Updating homebrew target");
            UpdateCamsTarget(newTarget);
            return;
        }
        else
        {
            if (Plugin.instance.OpenBodyCamsMod && !OpenLib.Compat.OpenBodyCamFuncs.ShowingBodyCam)
                Loggers.LogInfo("OBC Terminal Body Cam is NOT active");
            else
                GetPlayerCamsFromExternalMod(newTarget);
        }
    }

    internal static void ToggleCameraState(bool enabled)
    {
        if (ConfigSettings.CamsUseDetectedMods.Value && (Plugin.instance.HelmetCamsMod || Plugin.instance.OpenBodyCamsMod || Plugin.instance.SolosBodyCamsMod))
            return;

        CamEvents.CameraHolder.SetActive(enabled);
        CamStuff.HomebrewCameraState(enabled, playerCam);
    }
}

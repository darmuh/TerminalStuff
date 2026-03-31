using OpenLib.Common;
using TerminalStuff.EventSub;
using UnityEngine;
using UnityEngine.UI;
using static TerminalStuff.VisualElements.MoreCamStuff;
using static TerminalStuff.CommandHandling.ViewCommands;
using TerminalStuff.Util;

namespace TerminalStuff.VisualElements;

public class CamEvents
{
    //public delegate void UpdateTexture(CamsClass cams, Texture texture);
    //public delegate void UpdateStyle(CamsClass cams, Texture mainTexture, Texture smallTexture = null);
    internal static CamsClass CamsThings = new();
    //public static OpenLib.Events.Events.CustomEvent UpdateTextures = new();
    public static OpenLib.Events.Events.CustomEvent<ViewMode> UpdateCamsEvent = new();
    public static OpenLib.Events.Events.CustomEvent<int> UpdateTarget = new();

    internal static RawImage MiniScreenImage = null!;

    private static GameObject? _playerMonitorCam;
    internal static GameObject CameraHolder
    {
        get
        {
            if (_playerMonitorCam == null)
                _playerMonitorCam = new("darmuhsTerminalStuff - PlayerCam Holder");

            return _playerMonitorCam;
        }
        set
        {
            _playerMonitorCam = value;
        }
    }

    internal static void SetTextures(Texture texture, Texture mini = null!)
    {
        Plugin.instance.Terminal.terminalImage.texture = texture;
        if(mini != null)
            MiniScreenImage.texture = mini;
    }

    internal static void UpdateStyle(Texture main, float mainOpacity, Texture mini = null!, float miniOpacity = 0f, bool isOverlay = false)
    {
        if (mini == null)
        {
            SetTextures(main);
            SetRawImageTransparency(Plugin.instance.Terminal.terminalImage, mainOpacity); // Full mainOpacity for map
            SetRawImageDimensions(Plugin.instance.Terminal.terminalImage.rectTransform, isFullScreen: true);
            SetMiniScreenImageEnabled(false);
        }
        else
        {
            SetMiniScreenImageEnabled(true);
            SetTextures(main, mini);
            SetRawImageTransparency(Plugin.instance.Terminal.terminalImage, mainOpacity);
            SetRawImageTransparency(MiniScreenImage, miniOpacity);

            SetRawImageDimensions(MiniScreenImage.rectTransform, isFullScreen: isOverlay);
            SetRawImageDimensions(Plugin.instance.Terminal.terminalImage.rectTransform, isFullScreen: true);
        }
    }

    internal static void StyleNone()
    {
        Plugin.instance.Terminal.displayingPersistentImage = null!;
        SetTextures(null!);
        SetMiniScreenImageEnabled(false);
        ToggleCameraState(false);
    }

    private static void SetMiniScreenImageEnabled(bool enabled)
    {
        if (MiniScreenImage == null)
            return;

        MiniScreenImage.enabled = enabled;
    }

    internal static void OnUpdateCamsEvent(ViewMode mode)
    {
        Loggers.LogDebug("UpdateCams Event!");

        switch (mode)
        {
            case ViewMode.None:
                StyleNone();
                break;
            case ViewMode.Map:
                ToggleCameraState(false);
                CamsThings.radarTexture = UpdateRadarTexture();
                UpdateStyle(CamsThings.radarTexture, 1f);
                break;
            case ViewMode.Camera:
                CamsThings.camsTexture = UpdateCamsTexture();
                UpdateStyle(CamsThings.camsTexture, 1f);
                break;
            case ViewMode.MiniCams:
                CamsThings.radarTexture = UpdateRadarTexture();
                CamsThings.camsTexture = UpdateCamsTexture();
                UpdateStyle(CamsThings.radarTexture, 1f, CamsThings.camsTexture, 0.7f);
                break;
            case ViewMode.MiniMap:
                CamsThings.radarTexture = UpdateRadarTexture();
                CamsThings.camsTexture = UpdateCamsTexture();
                UpdateStyle(CamsThings.camsTexture, 1f, CamsThings.radarTexture, 0.7f);
                break;
            case ViewMode.Overlay:
                CamsThings.radarTexture = UpdateRadarTexture();
                CamsThings.camsTexture = UpdateCamsTexture();
                UpdateStyle(CamsThings.radarTexture, 1f, CamsThings.camsTexture, ConfigSettings.OverlayOpacity.Value / 100f, true);
                break;
            case ViewMode.Mirror:
                UpdateStyle(GetMirrorTexture(), 1f);
                break;
            case ViewMode.Video: 
            case ViewMode.Vanilla:
            default:
                ToggleCameraState(false);
                SetMiniScreenImageEnabled(false);
                break;
        }

        Loggers.LogMessage($"Mode set to - [ {mode} ] OnUpdateCamsEvent");
        CurrentView = mode;
    }

    private static RenderTexture UpdateRadarTexture()
    {

        return GameStuff.TerminalMapRenderer.cam.targetTexture;
    }

    private static Texture UpdateCamsTexture()
    {
        Loggers.LogDebug("Updating Cams");
        if (IsExternalCamsPresent())
            return GetPlayerCamsFromExternalMod(GameStuff.TerminalMapRenderer.targetTransformIndex);
        else
            return UpdateCamsTarget(GameStuff.TerminalMapRenderer.targetTransformIndex);

        //radarTexture = GetTexture("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001", 1);
        //camsTexture = GetTexture("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001", 2);
    }

    private static void SetRawImageTransparency(RawImage rawImage, float Opacity)
    {
        Color currentColor = rawImage.color;
        Color newColor = new(currentColor.r, currentColor.g, currentColor.b, Opacity); // 70% mainOpacity
        rawImage.color = newColor;
    }

    internal static void SetRawImageDimensions(RectTransform rectTrans, bool isFullScreen)
    {
        if (isFullScreen)
        {
            rectTrans.sizeDelta = new Vector2(425, 280);
            rectTrans.anchoredPosition = new Vector2(0, -25);
        }
        else
        {
            rectTrans.sizeDelta = new Vector2(180, 100);
            rectTrans.anchoredPosition = new Vector2(123, 90);
        }
    }

    internal static Texture GetMirrorTexture()
    {
        if (Plugin.instance.OpenBodyCamsMod && ConfigSettings.CamsUseDetectedMods.Value)
        {
            Loggers.LogDebug("Sending to OBC for camera info");
            OpenLib.Compat.OpenBodyCamFuncs.OpenBodyCamsMirrorStatus(true, ConfigSettings.ObcResolutionMirror.Value, ConfigSettings.MirrorZoom.Value, ConfigSettings.Mirror2DStyle.Value, ref CamStuff.ObcCameraHolder);
            return OpenLib.Compat.OpenBodyCamFuncs.GetTexture(OpenLib.Compat.OpenBodyCamFuncs.TerminalMirrorCam);
        }
        else
            return HomebrewMirror();

    }

    //Homebrew Mirror
    private static RenderTexture HomebrewMirror()
    {
        if (playerCam == null)
        {
            Loggers.LogInfo("Creating home-brew PlayerCam");
            GameObject holder = CameraHolder;
            playerCam = CamStuff.HomebrewCam(ref mycamTexture, ref holder);
            CameraHolder = holder;
        }

        CamStuff.HomebrewCameraState(true, playerCam);
        CamStuff.CamInitMirror(CameraHolder, playerCam, ConfigSettings.MirrorZoom.Value, ConfigSettings.Mirror2DStyle.Value);

        return playerCam.targetTexture;
    }

    internal static void SetMirrorState(bool active)
    {
        if (Plugin.instance.OpenBodyCamsMod && ConfigSettings.CamsUseDetectedMods.Value)
        {
            OpenLib.Compat.OpenBodyCamFuncs.OpenBodyCamsMirrorStatus(active, ConfigSettings.ObcResolutionMirror.Value, ConfigSettings.MirrorZoom.Value, ConfigSettings.Mirror2DStyle.Value, ref CamStuff.ObcCameraHolder);
        }
        else
        {
            CamStuff.HomebrewCameraState(active, playerCam);
        }
            
    }
}

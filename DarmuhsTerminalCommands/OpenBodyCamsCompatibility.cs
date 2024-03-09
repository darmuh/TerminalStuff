using OpenBodyCams;
using OpenBodyCams.API;
using UnityEngine;

namespace TerminalStuff
{
    internal class OpenBodyCamsCompatibility
    {
        internal static BodyCamComponent TerminalBodyCam;

        internal static void UpdateCamsTarget()
        {
            Plugin.MoreLogs("Getting ZaggyCam texture");
            if (TerminalBodyCam == null || TerminalBodyCam.gameObject == null)
                CreateTerminalBodyCam();
            Plugin.MoreLogs($"Attempting to grab targetTexture");
            ForceEnableOBC(true);
            TerminalBodyCam.UpdateTargetStatus();
            ViewCommands.SetBodyCamTexture(TerminalBodyCam.GetCamera().targetTexture);
        }

        private static void CameraEvent(Camera cam)
        {
            Plugin.MoreLogs($"Camera created, Updating target.");
            UpdateCamsTarget();
        }

        private static void SetBodyCamTexture(RenderTexture texture)
        {
            Plugin.MoreLogs("RenderTexture Created, updating values");
            ViewCommands.camsTexture = texture;

            if(ViewCommands.AnyActiveMonitoring() && (Plugin.Terminal.terminalInUse || AllMyTerminalPatches.TerminalStartPatch.alwaysOnDisplay))
            {
                Plugin.MoreLogs("Active Cams mode detected on terminal");
                ForceEnableOBC(true);
                ViewCommands.ReInitCurrentMode(texture);
            }
        }

        internal static void CreateTerminalBodyCam()
        {
            if (!Plugin.instance.OpenBodyCamsMod)
                return;

            if (TerminalBodyCam != null && TerminalBodyCam.gameObject != null)
                Object.Destroy(TerminalBodyCam);

            Plugin.MoreLogs("CreateTerminalBodyCam called");

            if (Plugin.instance.TwoRadarMapsMod)
            {
                Plugin.MoreLogs("Tying bodycam to tworadarmaps radarview");
                TerminalBodyCam = BodyCam.CreateBodyCam(Plugin.Terminal.gameObject, screenMaterial: null, TwoRadarMaps.Plugin.TerminalMapRenderer);
                TerminalBodyCam.OnRenderTextureCreated += SetBodyCamTexture;
                TerminalBodyCam.OnBlankedSet += BlankedEvent;
                TerminalBodyCam.OnCameraCreated += CameraEvent;
                TerminalBodyCam.EnsureCameraExists();
                Camera cam = TerminalBodyCam.GetCamera();
                cam.fieldOfView = 100;
                SetBodyCamTexture(cam.targetTexture);
            }
            else
            {
                Plugin.MoreLogs("Attaching bodycam to standard mapscreen");
                TerminalBodyCam = BodyCam.CreateBodyCam(Plugin.Terminal.gameObject, screenMaterial: null, StartOfRound.Instance.mapScreen);
                TerminalBodyCam.OnRenderTextureCreated += ViewCommands.SetBodyCamTexture;
                TerminalBodyCam.EnsureCameraExists();
                Camera cam = TerminalBodyCam.GetCamera();
                cam.fieldOfView = 100;
                ViewCommands.SetBodyCamTexture(cam.targetTexture);
            }

            Plugin.MoreLogs("darmuh's OBC termcam updated!");
        }

        internal static void ForceEnableOBC(bool enabled)
        {
            if (TerminalBodyCam != null)
                TerminalBodyCam.ForceEnableCamera = enabled;
        }

        internal static void BlankedEvent(bool blanked)
        {
            if (TerminalBodyCam != null)
                TerminalBodyCam.ForceEnableCamera = !blanked;
            Plugin.MoreLogs($"Blanked event: {blanked}");
        }
    }
}

using System.Runtime.CompilerServices;
using UnityEngine;
using OpenBodyCams;
using OpenBodyCams.API;

namespace TerminalStuff
{
    internal class OpenBodyCamsCompatibility
    {
        internal static MonoBehaviour TerminalBodyCam;

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void UpdateCamsTarget()
        {
            Plugin.MoreLogs("Getting ZaggyCam texture");
            if (TerminalBodyCam == null || TerminalBodyCam.gameObject == null || ((BodyCamComponent)TerminalBodyCam) == null)
                CreateTerminalBodyCam();
            Plugin.MoreLogs($"Attempting to grab targetTexture");
            ((BodyCamComponent)TerminalBodyCam).UpdateTargetStatus();
            ForceEnableOBC(true);
            SetBodyCamTexture(((BodyCamComponent)TerminalBodyCam).GetCamera().targetTexture);
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

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void CreateTerminalBodyCam()
        {
            if (!Plugin.instance.OpenBodyCamsMod)
                return;

            if (TerminalBodyCam != null && TerminalBodyCam.gameObject != null)
                Object.Destroy(TerminalBodyCam);

            Plugin.MoreLogs("CreateTerminalBodyCam called");

            if (Plugin.instance.TwoRadarMapsMod)
            {
                TwoRadarMapsCamCreate();
            }
            else
            {
                Plugin.MoreLogs("Attaching bodycam to standard mapscreen");
                var terminalBodyCam = BodyCam.CreateBodyCam(Plugin.Terminal.gameObject, screenMaterial: null, StartOfRound.Instance.mapScreen);
                TerminalBodyCam = terminalBodyCam;
                terminalBodyCam.OnRenderTextureCreated += ViewCommands.SetBodyCamTexture;
                terminalBodyCam.EnsureCameraExists();
                Camera cam = terminalBodyCam.GetCamera();
                ViewCommands.SetBodyCamTexture(cam.targetTexture);
            }

            Plugin.MoreLogs("darmuh's OBC termcam updated!");
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void TwoRadarMapsCamCreate()
        {
            Plugin.MoreLogs("Tying bodycam to tworadarmaps radarview");
            var terminalBodyCam = BodyCam.CreateBodyCam(Plugin.Terminal.gameObject, screenMaterial: null, TwoRadarMaps.Plugin.TerminalMapRenderer);
            TerminalBodyCam = terminalBodyCam;
            terminalBodyCam.OnRenderTextureCreated += SetBodyCamTexture;
            terminalBodyCam.OnBlankedSet += BlankedEvent;
            terminalBodyCam.OnCameraCreated += CameraEvent;
            terminalBodyCam.EnsureCameraExists();
            Camera cam = terminalBodyCam.GetCamera();
            SetBodyCamTexture(cam.targetTexture);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void ForceEnableOBC(bool enabled)
        {
            if (TerminalBodyCam != null || ((BodyCamComponent)TerminalBodyCam) != null)
            {
                ((BodyCamComponent)TerminalBodyCam).ForceEnableCamera = enabled;
            }
                
        }

        private static void BlankedEvent(bool blanked)
        {
            if (TerminalBodyCam != null || ((BodyCamComponent)TerminalBodyCam) != null)
            {
                ((BodyCamComponent)TerminalBodyCam).ForceEnableCamera = !blanked;
            }
            Plugin.MoreLogs($"Blanked event: {blanked}");
        }
    }
}

using GameNetcodeStuff;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using static TerminalApi.TerminalApi;
using static TerminalStuff.AllMyTerminalPatches;
using static UnityEngine.Object;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace TerminalStuff
{
    internal class ViewCommands
    {
        internal static List<TerminalNode> termViewNodes = new List<TerminalNode>();
        internal static bool externalcamsmod = false;
        public static bool isVideoPlaying = false;
        internal static Camera playerCam = null;
        private static int lastPlayedIndex = -1;
        private static Texture radarTexture = null;
        internal static Texture camsTexture = null;

        internal static void InitializeTextures()
        {
            Plugin.MoreLogs("Updating Radar");
            UpdateRadarTexture();
            
            Plugin.MoreLogs("Updating Cams");
            if (IsExternalCamsPresent())
                GetPlayerCamsFromExternalMod();
            else if (Plugin.instance.TwoRadarMapsMod)
                TwoRadarMapsCompatibility.UpdateCamsTarget();
            else
                UpdateCamsTarget();

            Plugin.MoreLogs("Textures Updated for both Cams/Radar");

            //radarTexture = GetTexture("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001", 1);
            //camsTexture = GetTexture("Environment/HangarShip/ShipModels2b/MonitorWall/Cube.001", 2);

        }

        private static Texture UpdateRadarTexture()
        {
            if (!Plugin.instance.TwoRadarMapsMod)
                radarTexture = StartOfRound.Instance.mapScreen.cam.targetTexture;
            else
                radarTexture = TwoRadarMapsCompatibility.RadarCamTexture();

            return radarTexture;
        }

        internal static void SetBodyCamTexture(RenderTexture texture)
        {
            camsTexture = texture;
        }

        internal static void InitializeTextures4Mirror()
        {
            camsTexture = MirrorTexture();
        }

        internal static void GetPlayerCamsFromExternalMod()
        {
            if(Plugin.instance.OpenBodyCamsMod)
            {
                Plugin.MoreLogs("Sending to OBC for camera info");
                OpenBodyCamsCompatibility.UpdateCamsTarget();
            }
            else if(Plugin.instance.SolosBodyCamsMod || Plugin.instance.HelmetCamsMod)
            {
                Plugin.MoreLogs("Grabbing monitor texture for other external bodycams mods");
                camsTexture = PlayerCamsCompatibility.PlayerCamTexture();
            }    
            else
            {
                Plugin.MoreLogs("No external mods detected, defaulting to internal cams system.");
                if (Plugin.instance.TwoRadarMapsMod)
                    TwoRadarMapsCompatibility.UpdateCamsTarget();
                else
                    UpdateCamsTarget();
            }
        }

        internal static void DetermineCamsTargets()
        {
            if (IsExternalCamsPresent())
            {
                externalcamsmod = true;
                Plugin.Log.LogInfo("External PlayerCams Mod Detected and will be used for all Cams Commands.");
            }
                    
            else
                externalcamsmod = false;
        }

        internal static bool IsExternalCamsPresent()
        {
            if (ConfigSettings.camsUseDetectedMods.Value && (Plugin.instance.HelmetCamsMod || Plugin.instance.SolosBodyCamsMod || Plugin.instance.OpenBodyCamsMod))
                return true;
            else
                return false;
        }

        internal static void UpdateCamsTarget()
        {
            if (Plugin.instance.activeCam && !Plugin.instance.radarNonPlayer)
            {
                Plugin.MoreLogs("Using internal mod camera on valid player");
                camsTexture = PlayerCamTexture();
            }
            else if (Plugin.instance.activeCam && Plugin.instance.radarNonPlayer)
            {
                Plugin.MoreLogs("Using internal mod camera on valid non-player");
                camsTexture = RadarCamTexture();
            }
        }

        internal static void TermMapEvent(out string displayText)
        {
            TerminalNode node = Plugin.Terminal.currentNode;

            isVideoPlaying = false;
            

            if (StartOfRound.Instance != null && StartOfRound.Instance.shipDoorsEnabled)
            {
                HandleMapEvent(out string message);
                displayText = message;
                return;
            }
            else
            {
                node.name = "NoViewCam";
                HandleOrbitMapEvent(out string message);
                displayText = message;
                return;
            }
        }

        private static void HandleMapEvent(out string displayText)
        {

            if (radarTexture == null)
                InitializeTextures();

            if (Plugin.instance.isOnMap == false)
            {
                SetTexturesAndVisibility(Plugin.Terminal, radarTexture);
                SetRawImageTransparency(Plugin.instance.rawImage2, 1f); // Full opacity for map

                // Set dimensions and position for radar image (rawImage2)
                SetRawImageDimensionsAndPosition(Plugin.instance.rawImage2.rectTransform, 1f, 1f, 0f, 0f);

                // Enable split view and update bools
                SplitViewChecks.EnableSplitView("map");

                DisplayTextUpdater(out string message);
                displayText = message;
                return;
            }
            else if (Plugin.instance.isOnMap)
            {
                SplitViewChecks.DisableSplitView("map");
                displayText = $"{ConfigSettings.mapString2.Value}\r\n";
                return;
            }
            else
            {
                Plugin.Log.LogError("Map command ERROR, isOnMap neither true nor false!!!");
                displayText = "Map Command ERROR, please report this to as a bug";
                return;
            }
        }

        private static void HandleOrbitMapEvent(out string displayText)
        {
            TerminalNode node = Plugin.Terminal.currentNode;

            HideAllTextures(node);
            Plugin.MoreLogs("This should only trigger in orbit");
            node.clearPreviousText = true;
            node.loadImageSlowly = false;
            displayText = "Radar view not available in orbit.\r\n";
            ResetPluginInstanceBools();
            return;
        }

        internal static void HandlePreviousSwitchEvent(out string displayText)
        {
            Plugin.MoreLogs("switching to previous player event detected");

            if (Plugin.instance.TwoRadarMapsMod)
                TwoRadarMapsCompatibility.UpdateTerminalRadarTarget(Plugin.Terminal, -2);
            else
                StartOfRound.Instance.mapScreen.SwitchRadarTargetForward(callRPC: true);

            DisplayTextUpdater(out string message);
            displayText = message;

            return;
        }

        internal static void MirrorEvent(out string displayText)
        {

            isVideoPlaying = false;

            if (Plugin.instance.isOnMirror == false && Plugin.instance.splitViewCreated)
            {
                Plugin.instance.activeCam = true;
                InitializeTextures4Mirror();

                SetTexturesAndVisibility(Plugin.Terminal, camsTexture);
                SetRawImageTransparency(Plugin.instance.rawImage2, 1f); // Full opacity for cams

                // Set dimensions and position for radar image (rawImage2)
                SetRawImageDimensionsAndPosition(Plugin.instance.rawImage2.rectTransform, 1f, 1f, 0f, 0f);

                // Enable split view and update bools
                SplitViewChecks.EnableSplitView("mirror");


                Plugin.MoreLogs("Mirror added to terminal screen");
                //DisplayTextUpdater(out string message);
                displayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMirror Camera added to terminal.\r\n";
                return;
            }
            else if (Plugin.instance.isOnMirror)
            {
                SplitViewChecks.DisableSplitView("mirror");
                displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMirror Camera removed from terminal.\r\n";
                Plugin.MoreLogs("mirror removed");
                return;
            }
            else
            {
                //Plugin.MoreLogs("Unable to run mirror event for some reason...");
                displayText = "Error with Mirror Event! Report this as a bug please.";
                Plugin.Log.LogError("Mirror Command ERROR");
                return;
            }
        }

        internal static void TermCamsEvent(out string displayText)
        {

            isVideoPlaying = false;

            if (Plugin.instance.isOnCamera == false && Plugin.instance.splitViewCreated)
            {
                Plugin.instance.activeCam = true;
                InitializeTextures();
                
                SetTexturesAndVisibility(Plugin.Terminal, camsTexture);
                SetRawImageTransparency(Plugin.instance.rawImage2, 1f); // Full opacity for cams

                // Set dimensions and position for radar image (rawImage2)
                SetRawImageDimensionsAndPosition(Plugin.instance.rawImage2.rectTransform, 1f, 1f, 0f, 0f);

                // Enable split view and update bools
                SplitViewChecks.EnableSplitView("cams");
                

                Plugin.MoreLogs("Cam added to terminal screen");
                DisplayTextUpdater(out string message);
                displayText = message;
                return;
            }
            else if (Plugin.instance.isOnCamera)
            {
                SplitViewChecks.DisableSplitView("cams");
                displayText = $"{ConfigSettings.camString2.Value}\r\n";
                Plugin.MoreLogs("Cams removed");
                return;
            }
            else
            {
                Plugin.MoreLogs("Unable to run cameras event for some reason...");
                displayText = "Error with Cams Event! Report this as a bug please.";
                Plugin.Log.LogError("Cams Command ERROR");
                return;
            }
        }

        internal static void MiniCamsTermEvent(out string displayText)
        {
            TerminalNode node = Plugin.Terminal.currentNode;

            isVideoPlaying = false;
            node.clearPreviousText = true;
            displayText = string.Empty;

            // Extract player name from map screen
            string playerNameText = StartOfRound.Instance.mapScreenPlayerName.text;
            string removeText = "MONITORING: ";
            string playerName = playerNameText.Remove(0, removeText.Length);

            if (Plugin.instance.splitViewCreated && !Plugin.instance.isOnMiniCams)
            {
                Plugin.instance.activeCam = true; //needs to be set before initializing textures

                if (radarTexture == null || camsTexture == null) //get textures for radar/cams
                    InitializeTextures();

                SetTexturesAndVisibility(Plugin.Terminal, radarTexture, camsTexture);

                // Set transparency for rawImage1
                SetRawImageTransparency(Plugin.instance.rawImage1, 0.7f);

                // Set dimensions and position for radar image (rawImage1)
                SetRawImageDimensionsAndPosition(Plugin.instance.rawImage1.rectTransform, 0.2f, 0.25f, 130f, 103f);

                // Enable split view and update bools
                SplitViewChecks.EnableSplitView("minicams");

                // Display text based on whether playerName is empty or not
                displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} {ConfigSettings.mcString.Value}\r\n";
            }
            else if (Plugin.instance.splitViewCreated && Plugin.instance.isOnMiniCams)
            {
                SplitViewChecks.DisableSplitView("minicams");
                displayText = $"{ConfigSettings.mcString2.Value}\r\n";
            }
            else
            {
                Plugin.Log.LogError("Unexpected condition");
            }
        }

        internal static void MiniMapTermEvent(out string displayText)
        {
            TerminalNode node = Plugin.Terminal.currentNode;

            isVideoPlaying = false;
            displayText = string.Empty;
            string playerNameText = StartOfRound.Instance.mapScreenPlayerName.text;
            string removeText = "MONITORING: ";
            string playerName = playerNameText.Remove(0, removeText.Length);
            node.clearPreviousText = true;

            if (Plugin.instance.splitViewCreated && !Plugin.instance.isOnMiniMap)
            {
                Plugin.instance.activeCam = true; //needs to be set before initializing textures

                if (radarTexture == null || camsTexture == null) //get textures for radar/cams
                    InitializeTextures();

                SetTexturesAndVisibility(Plugin.Terminal, camsTexture, radarTexture);

                // Set transparency for rawImage1
                SetRawImageTransparency(Plugin.instance.rawImage1, 0.7f);

                // Set dimensions and position for radar image (rawImage1)
                SetRawImageDimensionsAndPosition(Plugin.instance.rawImage1.rectTransform, 0.2f, 0.25f, 130f, 103f);

                // Enable split view and update bools
                SplitViewChecks.EnableSplitView("minimap");

                // Display text based on whether playerName is empty or not
                displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} {ConfigSettings.mmString.Value}\r\n";
            }
            else if (Plugin.instance.splitViewCreated && Plugin.instance.isOnMiniMap)
            {
                SplitViewChecks.DisableSplitView("minimap");
                displayText = $"{ConfigSettings.mmString2.Value}\r\n";
            }
            else
            {
                Plugin.Log.LogError("Unexpected condition");
            }
        }

        internal static void OverlayTermEvent(out string displayText)
        {
            
            TerminalNode node = Plugin.Terminal.currentNode;

            isVideoPlaying = false;
            node.clearPreviousText = true;
            displayText = string.Empty;
            string playerNameText = StartOfRound.Instance.mapScreenPlayerName.text;
            string removeText = "MONITORING: ";
            string playerName = playerNameText.Remove(0, removeText.Length);
            float opacityConfig = ConfigSettings.ovOpacity.Value / 100f;
            Plugin.MoreLogs($"Overlay Opacity: {opacityConfig}");

            if (Plugin.instance.splitViewCreated && !Plugin.instance.isOnOverlay)
            {
                Plugin.instance.activeCam = true; //needs to be set before initializing textures

                if (radarTexture == null || camsTexture == null) //get textures for radar/cams
                    InitializeTextures();

                SetTexturesAndVisibility(Plugin.Terminal, radarTexture, camsTexture);

                // Set transparency for rawImage1
                SetRawImageTransparency(Plugin.instance.rawImage1, opacityConfig);

                // Set dimensions and position for radar image (rawImage1)
                SetRawImageDimensionsAndPosition(Plugin.instance.rawImage1.rectTransform, 1f, 1f, 0f, 0f);

                // Enable split view and update bools
                SplitViewChecks.EnableSplitView("overlay");

                // Display text based on whether playerName is empty or not
                displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} {ConfigSettings.ovString.Value}\r\n";
            }
            else if (Plugin.instance.splitViewCreated && Plugin.instance.isOnOverlay)
            {
                SplitViewChecks.DisableSplitView("overlay");
                displayText = $"{ConfigSettings.ovString2.Value}\r\n";
            }
            else
            {
                Plugin.Log.LogError("Unexpected condition");
            }
        }

        internal static void ResetPluginInstanceBools()
        {
            Plugin.instance.isOnMiniMap = false;
            Plugin.instance.isOnMap = false;
            Plugin.instance.isOnCamera = false;
            Plugin.instance.isOnMirror = false;
            Plugin.instance.isOnOverlay = false;
            Plugin.instance.activeCam = false;
            DisablePlayerCam();
        }

        private static void HideAllTextures(TerminalNode node)
        {
            node.displayTexture = null;
            SplitViewChecks.DisableSplitView("map");
            DisablePlayerCam();
        }

        private static Texture GetTexture(string gameObjectPath, int materialIndex)
        {
            return GameObject.Find(gameObjectPath).GetComponent<MeshRenderer>().materials[materialIndex].mainTexture;
        }

        internal static void PlayerCamSetup()
        {
            playerCam = Instantiate(StartOfRound.Instance.spectateCamera);
            playerCam.gameObject.SetActive(true);
            Transform termObject = GameObject.Find("Environment/HangarShip/Terminal").GetComponent<Transform>();
            if (termObject != null)
            {
                termObject.gameObject.layer = 0;
                Plugin.MoreLogs("terminal layer changed");
            }
        }

        private static Texture MirrorTexture()
        {
            if (playerCam == null)
            {
                PlayerCamSetup();
            }

            playerCam.enabled = true;
            playerCam.cameraType = CameraType.SceneView;

            Transform termTransform = Plugin.Terminal.transform;
            Transform playerTransform = StartOfRound.Instance.localPlayerController.transform;
            Plugin.MoreLogs("camTransform assigned to terminal");

            // Calculate the opposite direction directly in local space
            Vector3 oppositeDirection = -playerTransform.forward;

            // Calculate the new rotation to look behind
            Quaternion newRotation = Quaternion.LookRotation(oppositeDirection, playerTransform.up);

            // Define the distance to back up the camera
            float distanceBehind = 1f;

            // Set camera's rotation and position
            playerCam.transform.rotation = newRotation;
            playerCam.transform.position = playerTransform.position - oppositeDirection * distanceBehind + playerTransform.up * 2.2f;

            playerCam.cullingMask = 557520731; //grabbed from OpenBodyCam's camera cullingmask during testing
            playerCam.orthographic = true;
            playerCam.orthographicSize = 3.4f;
            playerCam.usePhysicalProperties = false;
            playerCam.farClipPlane = 30f;
            playerCam.nearClipPlane = 0.25f;
            playerCam.fieldOfView = 130f;
            playerCam.transform.SetParent(termTransform);

            Texture spectateTexture = playerCam.targetTexture;
            return spectateTexture;
        }


        private static Texture PlayerCamTexture()
        {
            if (playerCam == null)
            {
                PlayerCamSetup();
            }

            playerCam.orthographic = false;
            playerCam.enabled = true;
            playerCam.cameraType = CameraType.SceneView;
            Transform camTransform = null;
            if (StartOfRound.Instance.mapScreen.targetedPlayer != null)
            {
                camTransform = StartOfRound.Instance.mapScreen.targetedPlayer.gameplayCamera.transform;
                Plugin.MoreLogs("Valid player for cams update");
            }
            else
            {
                camTransform = SwitchRadarPatch.radarTransform;
                Plugin.MoreLogs("Invalid player for cams update, sending to backup");
            }
            
            //
            playerCam.transform.rotation = camTransform.rotation;
            playerCam.transform.position = camTransform.transform.position;
            playerCam.usePhysicalProperties = false;
            playerCam.cullingMask = 20649983;
            //565778431
            playerCam.farClipPlane = 25f;
            playerCam.nearClipPlane = 0.4f;
            playerCam.fieldOfView = 90f;
            playerCam.transform.SetParent(camTransform.transform);
            Texture spectateTexture = playerCam.targetTexture;
            return spectateTexture;
        }

        private static Texture RadarCamTexture()
        {
            if (playerCam == null)
            {
                PlayerCamSetup();
            }

            playerCam.orthographic = false;
            playerCam.enabled = true;
            playerCam.cameraType = CameraType.SceneView;
            Transform camTransform = SwitchRadarPatch.radarTransform;
            playerCam.transform.rotation = camTransform.rotation;
            playerCam.transform.position = camTransform.transform.position;
            playerCam.cullingMask = 20649983;
            playerCam.usePhysicalProperties = true;
            playerCam.farClipPlane = 50f;
            playerCam.nearClipPlane = 0.4f;
            playerCam.fieldOfView = 110f;
            playerCam.transform.SetParent(camTransform.transform);
            Texture spectateTexture = playerCam.targetTexture;
            return spectateTexture;
        }

        internal static void DisablePlayerCam()
        {
            if (playerCam != null)
                playerCam.enabled = false;
        }

        private static void SetTexturesAndVisibility(Terminal getTerm, Texture mainTexture)
        {
            Plugin.instance.rawImage2.texture = mainTexture;
            getTerm.terminalImage.enabled = true;
            Plugin.instance.rawImage2.enabled = true;
            Plugin.instance.rawImage1.enabled = false;
        }
        private static void SetTexturesAndVisibility(Terminal getTerm, Texture mainTexture, Texture smallTexture)
        {
            Plugin.instance.rawImage2.texture = mainTexture;
            Plugin.instance.rawImage1.texture = smallTexture;
            getTerm.terminalImage.enabled = true;
            Plugin.instance.rawImage2.enabled = true;
            Plugin.instance.rawImage1.enabled = true;
        }

        private static void SetRawImageTransparency(RawImage rawImage, float Opacity)
        {
            Color currentColor = rawImage.color;
            Color newColor = new Color(currentColor.r, currentColor.g, currentColor.b, Opacity); // 70% opacity
            rawImage.color = newColor;
        }

        private static void SetRawImageDimensionsAndPosition(RectTransform rectTransform, float heightPercentage, float widthPercentage, float anchoredPosX, float anchoredPosY)
        {
            RectTransform canvasRect = Plugin.instance.terminalCanvas.GetComponent<RectTransform>();
            float height = canvasRect.rect.height * heightPercentage;
            float width = canvasRect.rect.width * widthPercentage;
            rectTransform.sizeDelta = new Vector2(width, height);
            rectTransform.anchoredPosition = new Vector2(anchoredPosX, anchoredPosY);
        }

        internal static void LolVideoPlayerEvent(out string displayText)
        {
            Plugin.MoreLogs("Start of LolEvent");
            
            TerminalNode node = Plugin.Terminal.currentNode;

            displayText = string.Empty;
            node.clearPreviousText = true;
            fixVideoPatch.sanityCheckLOL = true;

            SplitViewChecks.CheckForSplitView("neither"); // Disables split view components if enabled
            DisablePlayerCam();

            //RawImage termRawImage = GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer/ImageContainer/Image (1)").GetComponent<RawImage>();
            VideoPlayer termVP = GameObject.Find("Environment/HangarShip/Terminal/Canvas/MainContainer/ImageContainer/Image (1)").GetComponent<VideoPlayer>();


            if (!isVideoPlaying)
            {
                Plugin.MoreLogs("Video not playing, running LolEvents");

                // Play the next video if not playing
                if (VideoManager.Videos.Count == 0)
                {
                    Plugin.Log.LogError("No videos found.");
                    displayText = string.Empty;
                    return;
                }
                else if (VideoManager.Videos.Count <= 2)
                {
                    lastPlayedIndex = 0;
                    Plugin.MoreLogs("2 or less videos detected");
                }
                else
                {
                    Plugin.MoreLogs("More than 2 videos detected, shuffling");
                    // Shuffle the list of videos to get a random order
                    ShuffleList(VideoManager.Videos);

                    // Always select the first video (except when there are only 1 or 2 videos available)
                    lastPlayedIndex = Mathf.Min(lastPlayedIndex + 1, VideoManager.Videos.Count - 1);
                    Plugin.MoreLogs($"{lastPlayedIndex} - random video selected");
                }

                Plugin.MoreLogs($"Random Clip: {lastPlayedIndex} - {VideoManager.Videos[lastPlayedIndex]}");

                // Set up the video player
                SetupVideoPlayer(Plugin.Terminal, termVP, lastPlayedIndex);
                termVP.Play();
                Plugin.MoreLogs("Video should be playing");

                displayText = $"{ConfigSettings.lolStartString.Value}\n";
            }
            else if (isVideoPlaying)
            {
                Plugin.MoreLogs("Video detected playing, trying to stop it");
                fixVideoPatch.OnVideoEnd(Plugin.Terminal.videoPlayer, Plugin.Terminal);
                displayText = $"{ConfigSettings.lolStopString.Value}\n";
                Plugin.MoreLogs("Lol stop detected");
                return;
            }
        }

        public static void ShuffleList(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int randomIndex = Random.Range(i, list.Count);
                string temp = list[randomIndex];
                list[randomIndex] = list[i];
                list[i] = temp;
            }
        }

        private static void SetupVideoPlayer(Terminal getTerm, VideoPlayer termVP, int randomIndex)
        {
            termVP.Stop(); // Stop for setup
            getTerm.terminalAudio.Stop(); // Fix audio

            termVP.clip = null;
            termVP.url = "file://" + VideoManager.Videos[randomIndex];
            Plugin.MoreLogs("URL:" + termVP.url);

            termVP.renderMode = VideoRenderMode.RenderTexture;
            termVP.aspectRatio = VideoAspectRatio.Stretch;
            termVP.isLooping = false;
            termVP.playOnAwake = false;

            getTerm.terminalImage.texture = getTerm.videoTexture;
            termVP.targetTexture = getTerm.videoTexture;

            termVP.audioOutputMode = VideoAudioOutputMode.AudioSource;
            termVP.controlledAudioTrackCount = 1;

            termVP.SetTargetAudioSource(0, getTerm.terminalAudio);
            termVP.source = VideoSource.Url;
            termVP.enabled = true;
            Plugin.MoreLogs("Videoplayer setup complete");
        }

        internal static void DisplayTextUpdater(out string displayText)
        {
            Plugin.MoreLogs("updating displaytext!!!");
            GetCurrentMode(out string mode);
            string playerName = string.Empty;
            if (!Plugin.instance.TwoRadarMapsMod)
                playerName = Plugin.instance.switchTarget;
            else
                playerName = TwoRadarMapsCompatibility.TargetedPlayerOnSecondRadar();

            displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMonitoring: {playerName} [{mode}]\r\n\n";
            return;
        }

        private static void GetCurrentMode(out string mode)
        {
            if (Plugin.instance.isOnCamera)
            {
                mode = ConfigSettings.camString.Value;
                Plugin.MoreLogs("cams mode detected");
                return;
            }
            else if (Plugin.instance.isOnMap)
            {
                mode = ConfigSettings.mapString.Value;
                Plugin.MoreLogs("map mode detected");
                return;
            }
            else if (Plugin.instance.isOnOverlay)
            {
                mode = ConfigSettings.ovString.Value;
                Plugin.MoreLogs("overlay mode detected");
                return;
            }
            else if (Plugin.instance.isOnMiniMap)
            {
                mode = ConfigSettings.mmString.Value;
                Plugin.MoreLogs("minimap mode detected");
                return;
            }
            else if (Plugin.instance.isOnMiniCams)
            {
                mode = ConfigSettings.mcString.Value;
                Plugin.MoreLogs("minicams mode detected");
                return;
            }
            else if (Plugin.instance.isOnMirror)
            {
                mode = "Mirror";
                Plugin.MoreLogs("Mirror mode detected");
                return;
            }
            else
            {
                Plugin.Log.LogError("Error with mode return, setting to default value");
                mode = ConfigSettings.defaultCamsView.Value;
                return;
            }
        }

        internal static void ReInitCurrentMode(Texture texture)
        {
            camsTexture = texture;

            if (Plugin.instance.isOnCamera)
            {
                SetTexturesAndVisibility(Plugin.Terminal, camsTexture);
                Plugin.MoreLogs("cams mode detected, reinitializing textures");
                return;
            }
            else if (Plugin.instance.isOnMap)
            {
                SetTexturesAndVisibility(Plugin.Terminal, radarTexture);
                Plugin.MoreLogs("map mode detected, reinitializing textures");
                return;
            }
            else if (Plugin.instance.isOnOverlay)
            {
                SetTexturesAndVisibility(Plugin.Terminal, radarTexture, camsTexture);
                Plugin.MoreLogs("overlay mode detected, reinitializing textures");
                return;
            }
            else if (Plugin.instance.isOnMiniMap)
            {
                SetTexturesAndVisibility(Plugin.Terminal, camsTexture, radarTexture);
                Plugin.MoreLogs("minimap mode detected");
                return;
            }
            else if (Plugin.instance.isOnMiniCams)
            {
                SetTexturesAndVisibility(Plugin.Terminal, radarTexture, camsTexture);
                Plugin.MoreLogs("minicams mode detected");
                return;
            }
            else
            {
                Plugin.Log.LogError("Error with mode reinit, disabling any active cams");
                SplitViewChecks.DisableSplitView("neither");
                return;
            }
        }


        internal static void HandleReturnCamsEvent(Terminal term, out string displayText)
        {

            Plugin.MoreLogs($"Handle Return");

            isVideoPlaying = false;

            if (Plugin.instance.isOnMirror)
            {
                Plugin.MoreLogs("Mirror detected during AlwaysON");
                displayText = "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nMirror Active.\r\n";
                return;
            }
            else if (AnyActiveMonitoring())
            {
                if(!externalcamsmod)
                    UpdateCamsTarget();
                else


                Plugin.MoreLogs("CAMS detected during AlwaysON");
                DisplayTextUpdater(out string message);
                displayText = message;
                return;
            }             
            else
            {
                displayText = term.terminalNodes.specialNodes[1].displayText;
                SplitViewChecks.DisableSplitView("neither");
                Plugin.MoreLogs("No active modes detected, sending to home");
                return;
            }
        }

        internal static bool AnyActiveMonitoring()
        {
            return Plugin.instance.isOnMap || Plugin.instance.isOnCamera || Plugin.instance.isOnMiniMap || Plugin.instance.isOnMiniCams || Plugin.instance.isOnOverlay;
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

            // Handle the case when initialIndex is zero
            if (initialIndex == 0)
            {
                // Set initialIndex to the last index
                initialIndex = count - 1;
            }

            // Iterate through the list of targets
            for (int i = 1; i < count; i++)
            {
                // Calculate the index of the previous target
                int num = (initialIndex - i) % count;

                // Ensure num is non-negative
                num = (num + count) % count;

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
                Plugin.MoreLogs("player is not player controlled and is not dead, masked?");
                return component.redirectToEnemy != null;
            }

            Plugin.MoreLogs("returning true, no specific conditions met");
            return true;
        }

    }

}

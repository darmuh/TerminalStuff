using BepInEx;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace TerminalStuff
{
    internal static class VideoManager //grabbed this whole bit of code from TVLoader by Rattenbonkers, credit to them
    {
        public static List<string> Videos = new List<string>();


        public static void Load()
        {
            foreach (string directory in Directory.GetDirectories(Paths.PluginPath))
            {
                //Plugin.Log.LogInfo(")))))))))))))))))Setting directory for terminal videos");
                string path = Path.Combine(Paths.PluginPath, directory, $"{ConfigSettings.videoFolderPath.Value}");
                if (Directory.Exists(path))
                {
                    //Plugin.Log.LogInfo(")))))))))))))))))directory already exists!!!");
                    string[] files = Directory.GetFiles(path, "*.mp4");
                    //Plugin.Log.LogInfo(")))))))))))))))))getting files");
                    VideoManager.Videos.AddRange((IEnumerable<string>)files);
                    Plugin.Log.LogInfo((object)string.Format("{0} has {1} videos.", (object)directory, (object)files.Length));
                }
            }
            string path1 = Path.Combine(Paths.PluginPath, $"{ConfigSettings.videoFolderPath.Value}");
            if (!Directory.Exists(path1))
                Directory.CreateDirectory(path1);
            Plugin.Log.LogInfo(")))))))))))))))))Creating directory if doesn't exist");
            string[] files1 = Directory.GetFiles(path1, "*.mp4");
            //Plugin.Log.LogInfo(")))))))))))))))))getting files again");
            VideoManager.Videos.AddRange((IEnumerable<string>)files1);
            Plugin.Log.LogInfo((object)string.Format("Global has {0} videos.", (object)files1.Length));
            Plugin.Log.LogInfo((object)string.Format("Loaded {0} total.", (object)VideoManager.Videos.Count));
        }
    }

    public class VideoController : MonoBehaviour
    {
        private VideoPlayer videoPlayer;
        //public RawImage rawImage;
        public static VideoPlayer additionalVideoPlayer;  // New VideoPlayer variable

        // Shared variable to store the 'bool'
        public static bool isVideoPlaying = false;

        // Initialization method
        public void Initialize(GameObject targetGameObject)
        {
            // Check if the targetGameObject is not null
            if (targetGameObject != null)
            {
                // Check if an additional VideoPlayer has already been created
                if (additionalVideoPlayer == null)
                {
                    // Check if a VideoPlayer component already exists on the targetGameObject
                    VideoPlayer videoPlayer = targetGameObject.GetComponent<VideoPlayer>();

                    // If the VideoPlayer component doesn't exist, create and attach it
                    if (videoPlayer == null)
                    {
                        // Create a new VideoPlayer component and attach it to the targetGameObject
                        Plugin.Log.LogError("Target GameObject should already have a videoPlayer, something is wrong.");
                    }

                    // Create a new VideoPlayer component for the additional VideoPlayer
                    additionalVideoPlayer = targetGameObject.AddComponent<VideoPlayer>();
                    
                    Plugin.Log.LogInfo(">>>>>>>>>CREATING MY OWN VIDEO PLAYER FOR TERMINAL<<<<<<<<<");
                    additionalVideoPlayer.loopPointReached += OnVideoFinished; // Subscribe to the loopPointReached event
                    GameObject terminalCanvas = GameObject.Find("Environment/HangarShip/Terminal/Canvas");

                    //setting up canvas to display video
                    if (terminalCanvas == null)
                    {
                        Debug.LogError("No Canvas found in the scene.");
                        return;
                    }
                }
                else
                {
                    Plugin.Log.LogInfo("Additional VideoPlayer already exists. Skipping creation.");
                }
            }
            else
            {
                Plugin.Log.LogError("Target GameObject is null... Unable to attach VideoPlayer.");
            }
        }

        private int lastPlayedIndex = -1;
        public void PlayNextVideo()
        {
            if (VideoManager.Videos.Count > 0)
            {
                int randomIndex;

                // Generate a random index that is not the same as the last played index
                do
                {
                    randomIndex = Random.Range(0, VideoManager.Videos.Count);
                } while (randomIndex == lastPlayedIndex);

                lastPlayedIndex = randomIndex;

                Terminal terminalInstance = GameObject.FindObjectOfType<Terminal>();


                Plugin.Log.LogInfo($"Random Clip: {randomIndex} - {VideoManager.Videos[randomIndex]}");

                // Set the URL for the random video
                additionalVideoPlayer.url = "file://" + VideoManager.Videos[randomIndex];
                Plugin.Log.LogInfo("URL:" + additionalVideoPlayer.url);



                additionalVideoPlayer.enabled = true;
                //Plugin.Log.LogInfo(">>>>>>>>>CUSTOM VIDEO PLAYER ENABLED FOR TERMINAL<<<<<<<<<");

                additionalVideoPlayer.renderMode = VideoRenderMode.CameraNearPlane;
                additionalVideoPlayer.aspectRatio = VideoAspectRatio.NoScaling;
                additionalVideoPlayer.transform.localScale = new Vector3(350f, 350f);
                additionalVideoPlayer.transform.localPosition = new Vector3(0f, 0f);
                additionalVideoPlayer.isLooping = false;
                additionalVideoPlayer.playOnAwake = false;
                additionalVideoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
                additionalVideoPlayer.SetTargetAudioSource(0, terminalInstance.terminalAudio);
                additionalVideoPlayer.timeUpdateMode = VideoTimeUpdateMode.UnscaledGameTime;

                //get playercamera
                Camera targetCamera = GameNetworkManager.Instance.localPlayerController.gameplayCamera;
                additionalVideoPlayer.targetCamera = targetCamera;
                Plugin.Log.LogInfo("Target Camera set successfully.");

                additionalVideoPlayer.source = VideoSource.Url;
                additionalVideoPlayer.Play();

                Plugin.Log.LogInfo("Playing video now."); // changed
                isVideoPlaying = true;
                //rawImage.texture = additionalVideoPlayer.texture;
                //Plugin.Log.LogInfo("RawImage Texture: " + rawImage.texture);
                //Plugin.Log.LogInfo("VideoPlayer Texture: " + additionalVideoPlayer.texture);
            }
            else
            {
                Plugin.Log.LogError("No videos found.");
            }
        }

        public void StopAdditionalVideo()
        {
            if (additionalVideoPlayer != null && isVideoPlaying)
            {
                //STOP VIDEO
                additionalVideoPlayer.Stop();

                //unsubscribe before disabling
                additionalVideoPlayer.loopPointReached -= OnVideoFinished; // Unsubscribe
                // Print the state of additionalVideoPlayer.enabled
                //Plugin.Log.LogInfo($"Additional VideoPlayer Enabled State: {additionalVideoPlayer.enabled}");

                // Disable the VideoPlayer
                additionalVideoPlayer.enabled = false;

                // Print the updated state after disabling
                //Plugin.Log.LogInfo($"Additional VideoPlayer Enabled State after disabling: {additionalVideoPlayer.enabled}");


                // Set the flag to indicate that the video is not playing
                isVideoPlaying = false;

                Plugin.Log.LogInfo("Video stopped.");
            }
            else
            {
                Plugin.Log.LogInfo("No video is currently playing.");
            }
        }

        private void OnVideoFinished(VideoPlayer vp)
        {
            // Video is finished playing, hide the VideoPlayer
            vp.enabled = false;
            isVideoPlaying = false;
            Plugin.Log.LogInfo("Video finished playing. Hiding VideoPlayer.");
        }
    }
}
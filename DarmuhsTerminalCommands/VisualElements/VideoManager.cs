using BepInEx;
using System.Collections.Generic;
using System.IO;
using TerminalStuff.Util;
using UnityEngine;
using UnityEngine.Video;
using static TerminalStuff.Patching.AllMyTerminalPatches;
using static TerminalStuff.CommandHandling.ViewCommands;
using Random = System.Random;
using TerminalStuff.Networking;

namespace TerminalStuff.VisualElements;

internal static class VideoManager //reworked this bit of code from TVLoader by Rattenbonkers, credit to them
{
    public static List<string> Videos = [];
    private static int lastPlayedIndex = -1;
    internal static string currentlyPlaying = string.Empty;
    internal static TerminalNode videoPlayerNode = null!;
    internal static bool uniqueShuffled = false;

    public static void Load()
    {
        foreach (string directory in Directory.GetDirectories(Paths.PluginPath))
        {
            string path = Path.Combine(Paths.PluginPath, directory, $"{ConfigSettings.VideoFolderPath.Value}");
            if (Directory.Exists(path))
            {
                string[] files = Directory.GetFiles(path, "*.mp4");
                Videos.AddRange(files);
                Plugin.Log.LogInfo(string.Format("{0} has {1} videos.", directory, files.Length));
                return;
            }
            else if (OpenLib.Common.Misc.StringContainsInvariant(directory, ConfigSettings.VideoFolderPath.Value.ToLower()))
            {
                string path2 = Path.Combine(Paths.PluginPath, directory);
                string[] files = Directory.GetFiles(path2, "*.mp4");
                Videos.AddRange(files);
                Plugin.Log.LogInfo(string.Format("{0} has {1} videos.", directory, files.Length));
                return;
            }
        }

        Loggers.WARNING($"Unable to load video files from path configuration: {ConfigSettings.VideoFolderPath.Value}");
    }

    internal static void PlaySyncedVideo()
    {
        Loggers.LogInfo("Start of synced LolEvent");

        TerminalNode node = Plugin.instance.Terminal.currentNode;

        if (node == null)
        {
            Loggers.WARNING("Attempted to play video on NULL node!");
            return;
        }

        node.clearPreviousText = true;
        FixVideoPatch.VideoCheck = true;

        SplitViewChecks.CheckForSplitView("neither"); // Disables split view components if enabled
        if (!isVideoPlaying)
        {
            SetVideoToPlay(currentlyPlaying);
            SetupVideoPlayer();
            Plugin.instance.Terminal.videoPlayer.Play();
            Loggers.LogInfo("Synced video should be playing");
        }
        else
        {
            Loggers.LogInfo("Video detected already playing, trying to stop it");
            FixVideoPatch.OnVideoEnd();
        }

    }

    internal static string PickVideoToPlay()
    {
        string displayText;

        if (!isVideoPlaying)
        {
            Loggers.LogInfo("Video not playing, running LolEvents");
            DetermineVideoCount();
            Loggers.LogInfo($"Random Clip: {lastPlayedIndex} - {Videos[lastPlayedIndex]}");

            // Set up the video player
            GetVideoToPlay(lastPlayedIndex);
            SetupVideoPlayer();

            Plugin.instance.Terminal.videoPlayer.Play();
            Loggers.LogInfo("Video should be playing");

            displayText = $"{ConfigSettings.VideoStartString.Value}\n";
            return displayText;
        }
        else if (isVideoPlaying)
        {
            Loggers.LogInfo("Video detected playing, trying to stop it");
            FixVideoPatch.OnVideoEnd();
            displayText = $"{ConfigSettings.VideoStopString.Value}\n";
            Loggers.LogInfo("Lol stop detected");
            return displayText;
        }

        displayText = "Unexpected Error with displaying video... \n\n\n";
        return displayText;
    }

    private static void DetermineVideoCount()
    {
        // Play the next video if not playing
        if (Videos.Count == 0)
        {
            Loggers.ERROR("ERROR: No videos found, video player failure.");
            return;
        }
        else if (Videos.Count <= 2)
        {
            lastPlayedIndex = 0;
            Loggers.LogDebug("2 or less videos detected, no shuffle");
        }
        else if (ConfigSettings.AlwaysUniqueVideo.Value)
        {
            if (lastPlayedIndex < 0 || lastPlayedIndex >= Videos.Count - 1)
            {
                Loggers.LogDebug("AlwaysUniqueVideo, shuffling");
                // Shuffle the list of videos to get a random order
                ShuffleList(Videos);
                lastPlayedIndex = 0;
                uniqueShuffled = true;
            }
            else
            {
                lastPlayedIndex++;
                Loggers.LogDebug($"set to {lastPlayedIndex} of {Videos.Count - 1}");
            }
        }
        else
        {
            Loggers.LogInfo("More than 2 videos detected, shuffling");
            // Shuffle the list of videos to get a random order
            ShuffleList(Videos);

            // Always select the first video (except when there are only 1 or 2 videos available)
            lastPlayedIndex = Mathf.Min(lastPlayedIndex + 1, Videos.Count - 1);
            Loggers.LogDebug($"{lastPlayedIndex} - random video selected");
        }
    }

    private static void ShuffleList(List<string> list)
    {
        Random rand = new();
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = rand.Next(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }

    private static void GetVideoToPlay(int randomIndex)
    {
        Plugin.instance.Terminal.videoPlayer.clip = null;
        Plugin.instance.Terminal.videoPlayer.url = "file://" + Videos[randomIndex];
        currentlyPlaying = Videos[randomIndex];
        Loggers.LogInfo("URL:" + Plugin.instance.Terminal.videoPlayer.url);

        if (ConfigSettings.VideoSync.Value && NetHandler.Instance != null && ConfigSettings.NetworkedNodes.Value)
        {
            NetHandler.Instance.SyncMyVideoChoiceToEveryoneRpc(currentlyPlaying);
            Loggers.LogInfo("Video picked and sent to clients");
        }
    }

    private static void SetVideoToPlay(string VideoName)
    {
        Plugin.instance.Terminal.videoPlayer.clip = null;
        Plugin.instance.Terminal.videoPlayer.url = "file://" + VideoName;
        currentlyPlaying = VideoName;
        Loggers.LogInfo("URL:" + Plugin.instance.Terminal.videoPlayer.url);
    }

    private static void SetupVideoPlayer()
    {
        Plugin.instance.Terminal.videoPlayer.Stop(); // Stop for setup
        Plugin.instance.Terminal.terminalAudio.Stop(); // Fix audio

        Plugin.instance.Terminal.videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        Plugin.instance.Terminal.videoPlayer.aspectRatio = VideoAspectRatio.Stretch;
        Plugin.instance.Terminal.videoPlayer.isLooping = false;
        Plugin.instance.Terminal.videoPlayer.playOnAwake = false;

        Plugin.instance.Terminal.terminalImage.texture = Plugin.instance.Terminal.videoTexture;
        Plugin.instance.Terminal.videoPlayer.targetTexture = Plugin.instance.Terminal.videoTexture;

        Plugin.instance.Terminal.videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        Plugin.instance.Terminal.videoPlayer.controlledAudioTrackCount = 1;

        Plugin.instance.Terminal.videoPlayer.SetTargetAudioSource(0, Plugin.instance.Terminal.terminalAudio);
        Plugin.instance.Terminal.videoPlayer.source = VideoSource.Url;
        Plugin.instance.Terminal.videoPlayer.enabled = true;
        Loggers.LogInfo("Videoplayer setup complete");
    }
}
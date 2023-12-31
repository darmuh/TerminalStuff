using BepInEx;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace TerminalStuff
{
    internal static class VideoManager //grabbed this whole bit of code from TVLoader by Rattenbonkers, credit to them
    {
        public static List<string> Videos = new List<string>();


        public static void Load()
        {
            
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
                {
                    Directory.CreateDirectory(path1);
                    Plugin.Log.LogInfo("[VIDEO] Creating directory if doesn't exist");
                }

                string[] files1 = Directory.GetFiles(path1, "*.mp4");
                //Plugin.Log.LogInfo(")))))))))))))))))getting files again");
                VideoManager.Videos.AddRange((IEnumerable<string>)files1);
                //Plugin.Log.LogInfo((object)string.Format("Global has {0} videos.", (object)files1.Length));
                Plugin.Log.LogInfo((object)string.Format("Loaded {0} total videos.", (object)VideoManager.Videos.Count));
            }
        }
    }
}
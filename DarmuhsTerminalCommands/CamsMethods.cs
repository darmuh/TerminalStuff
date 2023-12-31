using System;
using static TerminalApi.TerminalApi;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TerminalStuff
{
    internal class CamsMethods
    {
        // Method to handle the camera case
        public static void HandleCams(Terminal instance, TerminalNode nodeName, string playerName, string displayText1, string displayText2)
        {
            Plugin.instance.isOnCamera = false;
            Plugin.Log.LogInfo("cam added to terminal screen");
            UpdateNodeDisplayText(ConfigSettings.camString.Value, playerName, nodeName, displayText1, displayText2);
            instance.RunTerminalEvents(nodeName);
            UpdateNodeDisplayText(ConfigSettings.camString.Value, playerName, instance.currentNode, displayText1, displayText2);
        }

        // Method to handle the map case
        public static void HandleMap(Terminal instance, TerminalNode nodeName, string playerName, string displayText1, string displayText2)
        {
            Plugin.instance.isOnMap = false;
            Plugin.Log.LogInfo("map radar enabled");
            UpdateNodeDisplayText(ConfigSettings.mapString.Value, playerName, nodeName, displayText1, displayText2);
            instance.RunTerminalEvents(nodeName);
            UpdateNodeDisplayText(ConfigSettings.mapString.Value, playerName, instance.currentNode, displayText1, displayText2);
        }

        // Method to handle the overlay case
        public static void HandleOverlay(Terminal instance, TerminalNode nodeName, string playerName, string displayText1, string displayText2)
        {
            Plugin.Log.LogInfo("Overlay was true, setting to false to run event again");
            Plugin.instance.isOnOverlay = false;
            UpdateNodeDisplayText(ConfigSettings.ovString.Value, playerName, nodeName, displayText1, displayText2);
            instance.RunTerminalEvents(nodeName);
            UpdateNodeDisplayText(ConfigSettings.ovString.Value, playerName, instance.currentNode, displayText1, displayText2);
        }

        // Method to handle the minimap case
        public static void HandleMiniMap(Terminal instance, TerminalNode nodeName, string playerName, string displayText1, string displayText2)
        {
            Plugin.Log.LogInfo("Minimap was true, setting to false to run event again");
            Plugin.instance.isOnMiniMap = false;
            UpdateNodeDisplayText(ConfigSettings.mmString.Value, playerName, nodeName, displayText1, displayText2);
            instance.RunTerminalEvents(nodeName);
            UpdateNodeDisplayText(ConfigSettings.mmString.Value, playerName, instance.currentNode, displayText1, displayText2);
        }

        // Method to handle the minicams case
        public static void HandleMiniCams(Terminal instance, TerminalNode nodeName, string playerName, string displayText1, string displayText2)
        {
            Plugin.Log.LogInfo("Minicams was true, setting to false to run event again");
            Plugin.instance.isOnMiniCams = false;
            UpdateNodeDisplayText(ConfigSettings.mcString.Value, playerName, nodeName, displayText1, displayText2);
            instance.RunTerminalEvents(nodeName);
            UpdateNodeDisplayText(ConfigSettings.mcString.Value, playerName, instance.currentNode, displayText1, displayText2);
        }

        // Method to handle the case when nothing is active
        public static void HandleNothingActive(Terminal instance, string playerName, string displayText1, string displayText2)
        {
            TerminalNode pvNode = CreateTerminalNode("go back to cams\n", true, "minimap");
            TerminalNode mcNode = CreateTerminalNode("go back to minicams\n", true, "minicams");
            TerminalNode ovNode = CreateTerminalNode("go back to cams\n", true, "overlay");
            TerminalNode camsNode = CreateTerminalNode("go back to cams\n", true, "cams");
            TerminalNode mapNode = CreateTerminalNode("go back to map\n", true, "mapEvent");

            Plugin.Log.LogInfo("Nothing was active..");
            if (ConfigSettings.defaultCamsView.Value == "map")
            {
                HandleMap(instance, mapNode, playerName, displayText1, displayText2);
                Plugin.Log.LogInfo($"Loading default node, map.");
            }
            else if (ConfigSettings.defaultCamsView.Value == "cams")
            {
                HandleCams(instance, camsNode, playerName, displayText1, displayText2);
                Plugin.Log.LogInfo($"Loading default node, cams.");
            }
            else if (ConfigSettings.defaultCamsView.Value == "minimap")
            {
                HandleMiniMap(instance, pvNode, playerName, displayText1, displayText2);
                Plugin.Log.LogInfo($"Loading default node, MiniMap.");
            }
            else if (ConfigSettings.defaultCamsView.Value == "minicams")
            {
                HandleMiniCams(instance, mcNode, playerName, displayText1, displayText2);
                Plugin.Log.LogInfo($"Loading default node, Minicams.");
            }
            else if (ConfigSettings.defaultCamsView.Value == "overlay")
            {
                HandleOverlay(instance, ovNode, playerName, displayText1, displayText2);
                Plugin.Log.LogInfo($"Loading default node, Overlay.");
            }
            else
                Plugin.Log.LogInfo("Report error for HandleNothingActive method..");
        }

        // Method to update node display text based on the player name and mode
        public static void UpdateNodeDisplayText(string mode, string playerName, TerminalNode node, string displayText1, string displayText2)
        {
            if (playerName != String.Empty)
            {
                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched to {playerName} ({mode})\n";
            }
            else
            {
                node.displayText = $"\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nSwitched target ({mode})\n";
            }
        }
    }
}

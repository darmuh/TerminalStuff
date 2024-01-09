using HarmonyLib;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static TerminalStuff.LeaveTerminal;
using Object = UnityEngine.Object;
using TerminalApi;
using Plugin = TerminalStuff.Plugin;
using AssetBundle = UnityEngine.AssetBundle;
using static TerminalStuff.AllMyTerminalPatches;


namespace TerminalStuff
{
    [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
    public class OpeningDoorsPatch
    {
        public static string getDangerLevel = "";
        public static void Postfix(ref StartOfRound __instance)
        {
            string dangerLevel = __instance.currentLevel.riskLevel;
            getDangerLevel = dangerLevel;
        }
    }
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    public class StartRoundPatch
    {
        public static void Postfix(ref StartOfRound __instance)
        {
            Plugin.instance.splitViewCreated = false;
            Terminal_RunTerminalEvents_Patch.AddDuplicateRenderObjects(); //addSplitViewObjects
        }

    }
    [HarmonyPatch(typeof(GameNetworkManager), "Start")]
    public class GameStartPatch
    {
        public static void Postfix(ref  GameNetworkManager __instance)
        {
            Plugin.AddKeywords();
            Plugin.Log.LogInfo("Enabled Commands added");
        }
    }
}

using HarmonyLib;
using static TerminalStuff.LeaveTerminal;
using System.Collections.Generic;
using UnityEngine;


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

    public class LoadGrabbablesOnShip
    {
        public static List<GrabbableObject> ItemsOnShip = new List<GrabbableObject>();
        public static void LoadAllItems()
        {
            ItemsOnShip.Clear();
            GameObject ship = GameObject.Find("/Environment/HangarShip");
            var grabbableObjects = ship.GetComponentsInChildren<GrabbableObject>();
            foreach (GrabbableObject item in grabbableObjects)
            {
                ItemsOnShip.Add(item);
                Plugin.Log.LogInfo($"{item.itemProperties.itemName} added to list");
            }

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

using BepInEx.Bootstrap;
using HarmonyLib;
using System.Collections.Generic;
using UnityEngine;
using static TerminalStuff.AllMyTerminalPatches;
using static UnityEngine.Object;


namespace TerminalStuff
{
    [HarmonyPatch(typeof(StartOfRound), "Start")]
    public class StartRoundPatch
    {
        public static void Postfix()
        {
            Plugin.instance.splitViewCreated = false;
            SplitViewChecks.InitSplitViewObjects(); //addSplitViewObjects
        }
    }

    //StartGame
    [HarmonyPatch(typeof(StartOfRound), "StartGame")]
    public class LandingPatch
    {
        public static void Postfix()
        {
            if (!StartOfRound.Instance.inShipPhase && !TerminalEvents.clockDisabledByCommand)
                TerminalClockStuff.showTime = true;

            //Cheat credits, only uncomment when testing and needing credits
            //NetHandler.Instance.SyncCreditsServerRpc(999999, Plugin.Terminal.numberOfItemsInDropship);
        }

    }

    [HarmonyPatch(typeof(ManualCameraRenderer), "updateMapTarget")]
    public class SwitchRadarPatch
    {
        internal static Transform radarTransform;
        internal static TransformAndName radarTargetVal;

        public static void Postfix(ref ManualCameraRenderer __instance, int setRadarTargetIndex)
        {
            if (__instance == null || __instance.radarTargets == null || __instance.radarTargets[setRadarTargetIndex] == null)
            {
                Plugin.Log.LogError("Postfix failed, ManualCameraRenderer instance null");
                return;
            }

            radarTargetVal = __instance.radarTargets[setRadarTargetIndex];
            radarTransform = __instance.radarTargets[setRadarTargetIndex].transform;

            if (radarTargetVal.isNonPlayer)
                Plugin.instance.radarNonPlayer = true;
            else
                Plugin.instance.radarNonPlayer = false;

            Plugin.instance.switchTarget = radarTargetVal.name;
            Plugin.MoreLogs($"Enumerator patch, Name: {Plugin.instance.switchTarget} Non-Player: {Plugin.instance.radarNonPlayer}");

            if (Plugin.instance.TwoRadarMapsMod)
                return;

            if (!ViewCommands.IsExternalCamsPresent() && ViewCommands.AnyActiveMonitoring())
            {
                SwitchedRadarEvent();
            }
            else
                ViewCommands.GetPlayerCamsFromExternalMod();

            UpdateDisplayText();
            

        }

        internal static void UpdateDisplayText()
        {
            Terminal getTerm = FindObjectOfType<Terminal>();

            if (!Plugin.instance.radarNonPlayer && StartOfRound.Instance.mapScreen.targetedPlayer == null)
                return;
            if (!Plugin.instance.activeCam)
                return;

            if (getTerm != null && getTerm.currentNode != null)
            {
                ViewCommands.DisplayTextUpdater(out string displayText);
                getTerm.currentNode.displayText = displayText;
            }
        }

        private static void SwitchedRadarEvent()
        {
            if (Plugin.instance.activeCam && !ViewCommands.externalcamsmod)
            {
                ViewCommands.UpdateCamsTarget();
                return;
            }

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
                Plugin.MoreLogs($"{item.itemProperties.itemName} added to list");
            }

        }

    }

    [HarmonyPatch(typeof(GameNetworkManager), "Start")]
    public class GameStartPatch
    {
        public static void Postfix()
        {
            Plugin.AddKeywords();
            Plugin.Log.LogInfo("Enabled Commands added");
            CompatibilityCheck();
        }

        private static void CompatibilityCheck()
        {
            if (Chainloader.PluginInfos.ContainsKey("com.potatoepet.AdvancedCompany"))
            {
                Plugin.MoreLogs("Advanced Company detected, setting Advanced Company Compatibility options");
                Plugin.instance.CompatibilityAC = true;
                //if (ConfigSettings.ModNetworking.Value)
                    //AdvancedCompanyCompat.AdvancedCompanyStuff();
            }
            if (Chainloader.PluginInfos.ContainsKey("Rozebud.FovAdjust"))
            {
                Plugin.MoreLogs("Rozebud's FovAdjust detected!");
                Plugin.instance.FovAdjust = true;
            }
            if (Chainloader.PluginInfos.ContainsKey("RickArg.lethalcompany.helmetcameras"))
            {
                Plugin.MoreLogs("Helmet Cameras by Rick Arg detected!");
                Plugin.instance.HelmetCamsMod = true;
            }
            if (Chainloader.PluginInfos.ContainsKey("SolosBodycams"))
            {
                Plugin.MoreLogs("SolosBodyCams by CapyCat (Solo) detected!");
                Plugin.instance.SolosBodyCamsMod = true;
            }
            if (Chainloader.PluginInfos.ContainsKey("Zaggy1024.OpenBodyCams"))
            {
                Plugin.MoreLogs("OpenBodyCams by Zaggy1024 detected!");
                Plugin.instance.OpenBodyCamsMod = true;
            }
            if (Chainloader.PluginInfos.ContainsKey("Zaggy1024.TwoRadarMaps"))
            {
                Plugin.MoreLogs("TwoRadarMaps by Zaggy1024 detected!");
                Plugin.instance.TwoRadarMapsMod = true;
            }
            if (Chainloader.PluginInfos.ContainsKey("com.malco.lethalcompany.moreshipupgrades")) //other mods that simply append to the help command
            {
                Plugin.MoreLogs("Lategame Upgrades by malco detected!");
                Plugin.instance.LateGameUpgrades = true;
            }
        }
    }
}

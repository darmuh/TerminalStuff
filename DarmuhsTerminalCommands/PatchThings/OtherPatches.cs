using HarmonyLib;
using System.Collections.Generic;
using TerminalStuff.EventSub;
using TerminalStuff.SpecialStuff;
using TerminalStuff.VisualCore;
using UnityEngine;


namespace TerminalStuff
{
    [HarmonyPatch(typeof(ManualCameraRenderer), "updateMapTarget")]
    public class SwitchRadarPatch
    {
        public static void Postfix(ManualCameraRenderer __instance, int setRadarTargetIndex)
        {
            if (__instance != GameStuff.TerminalMapRenderer)
                return;

            Plugin.Spam($"updateMapTarget: {setRadarTargetIndex}");
            CamEvents.UpdateTarget.Invoke(setRadarTargetIndex);
        }
    }

    //MeetsCameraEnabledConditions
    [HarmonyPatch(typeof(ManualCameraRenderer), "MeetsCameraEnabledConditions")]
    public class CameraEnabledPatch
    {
        public static void Postfix(ManualCameraRenderer __instance, ref bool __result)
        {
            if(__instance == GameStuff.TerminalMapRenderer)
            {
                if (BoolStuff.MapCameraUsed())
                    __result = true;
                else
                    return;
            }
        }
    }

    //SetMapScreenInfoToCurrentLevel
    [HarmonyPatch(typeof(StartOfRound), "SetMapScreenInfoToCurrentLevel")]
    public class SetMapScreenInfoPatch
    {
        public static void Postfix()
        {
            if (!ConfigSettings.TerminalMoonsPlus.Value)
                return;

            MoonsPlus.HideLevelFromMapScreen();
        }
    }

    [HarmonyPatch(typeof(TimeOfDay), "Awake")]
    public class TimeAwakePatch
    {
        public static void Postfix()
        {
            Plugin.MoreLogs("TimeAwakePatch");
            StartCreds();
        }

        private static void StartCreds()
        {
            if (TimeOfDay.Instance.quotaVariables != null && ConfigSettings.StartingCreds.Value > -1)
            {
                TimeOfDay.Instance.quotaVariables.startingCredits = ConfigSettings.StartingCreds.Value;
                Plugin.Log.LogInfo($"Starting credits modified to {TimeOfDay.Instance.quotaVariables.startingCredits}");
            }
        }
    }

    //RefreshClockUI
    [HarmonyPatch(typeof(HUDManager), "SetClock")]
    public class ClockTimePatch
    {
        public static OpenLib.Events.Events.CustomEvent OnRefreshClock = new();
        public static void Postfix()
        {
            OnRefreshClock.Invoke();
        }
    }

            [HarmonyPatch(typeof(FlashlightItem), "Start")]
    public class Flashlights_Start_Patch
    {
        internal static Color? DefaultRegColor { get; private set; }
        internal static Color? DefaultProColor { get; private set; }

        public static void Postfix(FlashlightItem __instance)
        {
            Plugin.Spam($"{__instance.itemProperties.itemName} start!");
            if (DefaultRegColor.HasValue && __instance.itemProperties.itemName.ToLower() == "flashlight")
                return;

            if (DefaultProColor.HasValue && __instance.itemProperties.itemName.ToLower() == "pro-flashlight")
                return;

            if(__instance.itemProperties.itemName.ToLower() == "flashlight")
                DefaultRegColor = __instance.flashlightBulb.color;

            if (__instance.itemProperties.itemName.ToLower() == "pro-flashlight")
                DefaultProColor = __instance.flashlightBulb.color;

            Plugin.Spam($"default color has been set for {__instance.itemProperties.itemName}!");
        }
    }

    [HarmonyPatch(typeof(FlashlightItem), "SwitchFlashlight")]
    public class FlashLights_Color_Patch
    {
        public static void Postfix(FlashlightItem __instance, bool on)
        {
            if (!on)
                return;

            if(!ConfigSettings.ModNetworking.Value)
                return;

            if (ColorCommands.RainbowFlash)
            {
                NetHandler.Instance.CycleThroughRainbowFlash();
                return;
            }
                

            Color def;

            if (__instance.itemProperties.itemName.ToLower() == "flashlight")
                def = Flashlights_Start_Patch.DefaultRegColor.Value;
            else if (__instance.itemProperties.itemName.ToLower() == "pro-flashlight")
                def = Flashlights_Start_Patch.DefaultProColor.Value;
            else
            {
                def = Color.white;
                Plugin.Spam($"Unknown flashlight item [ {__instance.itemProperties.itemName} ]");
            }

            Plugin.Spam($"Color def: {def}\n{__instance.itemProperties.itemName} color: {__instance.flashlightBulb.color}");
            if (__instance.flashlightBulb.color == def)
            {
                if (!ColorCommands.CustomFlashColor.HasValue)
                {
                    if (StartOfRound.Instance.localPlayerController.helmetLight.color != def)
                        NetHandler.Instance.HelmetLightColorServerRpc(def, StartOfRound.Instance.localPlayerController.playerClientId);
                    return;
                }
                    

                Plugin.Spam("Updating from default flashlight color!");
                NetHandler.SetFlash(ref __instance, ColorCommands.CustomFlashColor.Value);
                NetHandler.SetHelmetLight(ColorCommands.CustomFlashColor.Value, StartOfRound.Instance.localPlayerController.playerClientId);
                NetHandler.Instance.FlashColorServerRpc(ColorCommands.CustomFlashColor.Value, StartOfRound.Instance.localPlayerController.playerClientId, StartOfRound.Instance.localPlayerController.playerUsername);
            }
            else
            {
                if (ColorCommands.CustomFlashColor.HasValue && __instance.bulbLight.color == ColorCommands.CustomFlashColor.Value)
                    return;

                Plugin.Spam("Updating to new flashlight color!");
                NetHandler.Instance.HelmetLightColorServerRpc(__instance.bulbLight.color, StartOfRound.Instance.localPlayerController.playerClientId);
            }
        }
    }

    public class LoadGrabbablesOnShip
    {
        public static List<GrabbableObject> ItemsOnShip = [];
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
}

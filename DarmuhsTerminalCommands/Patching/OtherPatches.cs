using HarmonyLib;
using System.Collections.Generic;
using TerminalStuff.Configs;
using TerminalStuff.EventSub;
using TerminalStuff.Networking;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;
using UnityEngine;


namespace TerminalStuff.Patching;

internal class OtherPatches
{

    [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.updateMapTarget))]
    public class SwitchRadarPatch
    {
        public static void Postfix(ManualCameraRenderer __instance, int setRadarTargetIndex)
        {
            if (__instance != GameStuff.TerminalMapRenderer)
                return;

            Loggers.LogDebug($"updateMapTarget: {setRadarTargetIndex}");
            CamEvents.UpdateTarget.Invoke(setRadarTargetIndex);
        }
    }

    //MeetsCameraEnabledConditions
    [HarmonyPatch(typeof(ManualCameraRenderer), nameof(ManualCameraRenderer.MeetsCameraEnabledConditions))]
    public class CameraEnabledPatch
    {
        public static void Postfix(ManualCameraRenderer __instance, ref bool __result)
        {
            if (__instance == GameStuff.TerminalMapRenderer)
            {
                if (Bools.MapCameraUsed())
                    __result = true;
                else
                    return;
            }
        }
    }

    //SetMapScreenInfoToCurrentLevel
    [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.SetMapScreenInfoToCurrentLevel))]
    public class SetMapScreenInfoPatch
    {
        public static void Postfix()
        {
            if (!Commands.TerminalMoonsPlus.Value)
                return;

            MoonsTweaks.MoonsPlus.HideLevelFromMapScreen();
        }
    }

    [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.Awake))]
    public class TimeAwakePatch
    {
        public static void Postfix()
        {
            Loggers.LogInfo("TimeAwakePatch");
            StartCreds();
        }

        private static void StartCreds()
        {
            if (TimeOfDay.Instance.quotaVariables != null && QoLConfig.StartingCreds.Value > -1)
            {
                TimeOfDay.Instance.quotaVariables.startingCredits = QoLConfig.StartingCreds.Value;
                Plugin.Log.LogInfo($"Starting credits modified to {TimeOfDay.Instance.quotaVariables.startingCredits}");
            }
        }
    }

    //RefreshClockUI
    [HarmonyPatch(typeof(HUDManager), nameof(HUDManager.SetClock))]
    public class ClockTimePatch
    {
        public static OpenLib.Events.Events.CustomEvent OnRefreshClock = new();
        public static void Postfix()
        {
            OnRefreshClock.Invoke();
        }
    }

    [HarmonyPatch(typeof(FlashlightItem), nameof(FlashlightItem.Start))]
    public class Flashlights_Start_Patch
    {
        internal static Color? DefaultRegColor { get; private set; } = null!;
        internal static Color? DefaultProColor { get; private set; } = null!;

        public static void Postfix(FlashlightItem __instance)
        {
            Loggers.LogDebug($"{__instance.itemProperties.itemName} start!");
            if (DefaultRegColor.HasValue && OpenLib.Common.Misc.CompareStringsInvariant(__instance.itemProperties.itemName, "flashlight"))
                return;

            if (DefaultProColor.HasValue && OpenLib.Common.Misc.CompareStringsInvariant(__instance.itemProperties.itemName, "pro-flashlight"))
                return;

            if (OpenLib.Common.Misc.CompareStringsInvariant(__instance.itemProperties.itemName, "flashlight"))
                DefaultRegColor = __instance.flashlightBulb.color;

            if (OpenLib.Common.Misc.CompareStringsInvariant(__instance.itemProperties.itemName, "pro-flashlight"))
                DefaultProColor = __instance.flashlightBulb.color;

            Loggers.LogDebug($"default color has been set for {__instance.itemProperties.itemName}!");
        }
    }

    [HarmonyPatch(typeof(FlashlightItem), nameof(FlashlightItem.SwitchFlashlight))]
    public class FlashLights_Color_Patch
    {
        public static void Postfix(FlashlightItem __instance, bool on)
        {
            if (!on)
                return;

            if (!ConfigSettings.ModNetworking.Value)
                return;

            if (ColorCommands.RainbowFlash)
            {
                NetHandler.Instance.CycleThroughRainbowFlash();
                return;
            }

            Color def;

            if (OpenLib.Common.Misc.CompareStringsInvariant(__instance.itemProperties.itemName, "flashlight") && Flashlights_Start_Patch.DefaultRegColor != null)
                def = Flashlights_Start_Patch.DefaultRegColor.Value;
            else if (OpenLib.Common.Misc.CompareStringsInvariant(__instance.itemProperties.itemName, "pro-flashlight") && Flashlights_Start_Patch.DefaultProColor != null)
                def = Flashlights_Start_Patch.DefaultProColor.Value;
            else
            {
                def = Color.white;
                Loggers.LogDebug($"Null default values OR Unknown flashlight item [ {__instance.itemProperties.itemName} ]");
            }

            Loggers.LogDebug($"Color def: {def}\n{__instance.itemProperties.itemName} color: {__instance.flashlightBulb.color}");
            if (__instance.flashlightBulb.color == def)
            {
                if (!ColorCommands.CustomFlashColor.HasValue)
                {
                    if (StartOfRound.Instance.localPlayerController.helmetLight.color != def)
                        NetHandler.Instance.FlashColorRpc(def, StartOfRound.Instance.localPlayerController.playerSteamId);
                    return;
                }


                Loggers.LogDebug("Updating from default flashlight color!");
                NetHandler.Instance.FlashColorRpc(ColorCommands.CustomFlashColor.Value, StartOfRound.Instance.localPlayerController.playerSteamId);
            }
            else
            {
                if (!ColorCommands.CustomFlashColor.HasValue)
                {
                    NetHandler.Instance.FlashColorRpc(def, StartOfRound.Instance.localPlayerController.playerSteamId);
                    return;
                }
                    
                if (__instance.bulbLight.color == ColorCommands.CustomFlashColor.Value)
                    return;

                Loggers.LogDebug("Updating to new flashlight color!");
                NetHandler.Instance.FlashColorRpc(ColorCommands.CustomFlashColor.Value, StartOfRound.Instance.localPlayerController.playerSteamId);
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
                Loggers.LogInfo($"{item.itemProperties.itemName} added to list");
            }
        }
    }
}

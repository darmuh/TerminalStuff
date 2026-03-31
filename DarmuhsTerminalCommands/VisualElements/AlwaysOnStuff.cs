using System.Collections;
using TerminalStuff.CommandHandling;
using TerminalStuff.Configs;
using TerminalStuff.SpecialStuff;
using TerminalStuff.Util;
using UnityEngine;

namespace TerminalStuff.VisualElements;

internal class AlwaysOnStuff
{
    internal static bool delayOff = false;
    internal static ScreenSettings screenSettings = null!;

    internal static void IsPlayerDead()
    {
        if (StartOfRound.Instance.localPlayerController.isPlayerDead && DisableScreenOnDeath())
        {
            if (Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
                Plugin.instance.Terminal.terminalUIScreen.gameObject.SetActive(false);
        }
    }

    internal static void OnSpecateShipCheck()
    {
        if (DisableScreenOnDeath() || !screenSettings.Dynamic)
            return;

        Loggers.LogDebug($"Spectated Player detected in ship [ {StartOfRound.Instance.localPlayerController.spectatedPlayerScript.isInHangarShipRoom} ]");

        if (screenSettings.inUse && !Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
        {
            if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf && StartOfRound.Instance.localPlayerController.spectatedPlayerScript.isInHangarShipRoom && Plugin.instance.Terminal.placeableObject.inUse)
                SetScreenPlus(true);

        }

        if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf && StartOfRound.Instance.localPlayerController.spectatedPlayerScript.isInHangarShipRoom)
            SetScreenPlus(true);
        else if (Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf && !StartOfRound.Instance.localPlayerController.spectatedPlayerScript.isInHangarShipRoom)
            SetScreenPlus(false);
    }

    internal static void SetScreenPlus(bool visible)
    {
        Plugin.instance.Terminal.terminalUIScreen.gameObject.SetActive(visible);

        if (!StartOfRound.Instance.inShipPhase && !TerminalEvents.clockDisabledByCommand)
            TerminalClockStuff.SetClockVisible(visible); //if clock isn't added it will early return
        else
            TerminalClockStuff.SetClockVisible(false);

        if (ViewCommands.AnyActiveMonitoring() && Plugin.instance.splitViewCreated || ViewCommands.CurrentView == ViewCommands.ViewMode.Mirror)
        {
            Loggers.LogInfo("Adjusting camera views to screen status");
            SplitViewChecks.ShowCameraView(visible);
        }
    }

    internal static void PlayerShipChanged()
    {
        if (StartOfRound.Instance.localPlayerController == null || screenSettings == null)
            return;

        Loggers.LogDebug($"Player detected in ship change - {StartOfRound.Instance.localPlayerController.isInHangarShipRoom}");

        if (StartOfRound.Instance.localPlayerController.isPlayerDead && DisableScreenOnDeath())
        {
            if (Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
                SetScreenPlus(false);
        }

        if (StartOfRound.Instance.localPlayerController.isInHangarShipRoom)
        {
            if (screenSettings.Dynamic && !screenSettings.inUse)
            {
                if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
                    SetScreenPlus(true);
            }
            else if (screenSettings.inUse && OpenLib.TerminalUpdatePatch.inUse)
            {
                if (!Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
                    SetScreenPlus(true);
            }
        }
        else
        {
            if (screenSettings.Dynamic)
            {
                Loggers.LogDebug($"disabling screen - screenSetting Dynamic {screenSettings.Dynamic}");
                if (Plugin.instance.Terminal.terminalUIScreen.gameObject.activeSelf)
                {
                    if (QoLConfig.ScreenOffDelay.Value < 1)
                        SetScreenPlus(false);
                    else
                        Plugin.instance.StartCoroutine(DelayScreenOff(QoLConfig.ScreenOffDelay.Value));
                }

            }
        }
    }

    internal static IEnumerator DelayScreenOff(int delay)
    {
        if (delayOff)
            yield break;

        delayOff = true;
        yield return new WaitForSeconds(delay);
        if (!StartOfRound.Instance.localPlayerController.isInHangarShipRoom)
            SetScreenPlus(false);
        delayOff = false;
    }

    internal static bool DisableScreenOnDeath()
    {
        if (QoLConfig.ScreenOnWhileDead.Value)
            return false;

        return StartOfRound.Instance.localPlayerController.isPlayerDead;
    }
}

internal class ScreenSettings
{
    //"nochange", "alwayson", "inship", "inuse"
    internal bool AlwaysOn;
    internal bool Dynamic;
    internal bool inUse;

    internal void Update(string setting)
    {
        if (OpenLib.Common.Misc.CompareStringsInvariant(setting, "alwayson"))
        {
            AlwaysOn = true;
            Dynamic = false;
            inUse = false;
        }
        else if (OpenLib.Common.Misc.CompareStringsInvariant(setting, "inship"))
        {
            AlwaysOn = true;
            Dynamic = true;
            inUse = false;
        }
        else if (OpenLib.Common.Misc.CompareStringsInvariant(setting, "inuse"))
        {
            AlwaysOn = true;
            Dynamic = true;
            inUse = true;
        }
        else
        {
            AlwaysOn = false;
            Dynamic = false;
            inUse = false;
        }

        Loggers.LogDebug($"ScreenSettings set to: {setting}");
    }

    internal ScreenSettings(string setting)
    {
        Update(setting);
    }
}

using OpenLib.CoreMethods;
using OpenLib.Events;
using TerminalStuff.CommandHandling;
using TerminalStuff.Configs;
using TerminalStuff.Patching;
using TerminalStuff.SpecialStuff;
using TerminalStuff.Util;
using TerminalStuff.VisualElements;
using static TerminalStuff.EventSub.TerminalStart;

namespace TerminalStuff.EventSub;

internal class Subscribers
{
    internal static string OriginalOtherText = string.Empty;
    internal static void Subscribe()
    {
        EventManager.TerminalAwake.AddListener(OnTerminalAwake);
        EventManager.TerminalStart.AddListener(OnTerminalStart);
        EventManager.TerminalParseSent.AddListener(TerminalParse.OnParseSent);
        EventManager.TerminalBeginUsing.AddListener(TerminalBeginUsing.OnTerminalBeginUse);
        EventManager.TerminalLoadNewNode.AddListener(TerminalGeneral.OnLoadNode);
        EventManager.TerminalDisable.AddListener(TerminalGeneral.OnTerminalDisable);
        EventManager.TerminalLoadIfAffordable.AddListener(TerminalGeneral.OnLoadAffordable);
        EventManager.TerminalQuit.AddListener(TerminalQuit.OnTerminalQuit);

        //TerminalUpdateStuff
        OpenLib.TerminalUpdatePatch.usePatch = true;
        EventManager.SetTerminalInUse.AddListener(TerminalGeneral.OnSetTerminalInUse);
        EventManager.TerminalKeyPressed.AddListener(TerminalGeneral.OnTerminalKeyPress);

        //TeleporterStuff
        EventManager.NormalTPFound.AddListener(Teleporters.OnNormalAwake);
        EventManager.InverseTPFound.AddListener(Teleporters.OnInverseAwake);

        //GameStuff
        EventManager.GameNetworkManagerStart.AddListener(GameStuff.OnGameStart);
        EventManager.StartOfRoundStart.AddListener(GameStuff.OnStartOfRoundStart);
        EventManager.StartOfRoundChangeLevel.AddListener(GameStuff.OnChangeLevel);
        EventManager.ShipReset.AddListener(GameStuff.OnShipReset);
        EventManager.NextDayEvent.AddListener(GameStuff.OnNextDay);
        EventManager.PlayerSpawn.AddListener(GameStuff.OnPlayerSpawn);
        ClockTimePatch.OnRefreshClock.AddListener(TerminalClockStuff.ClockUpdate);

        //PlayerStuff
        OpenLib.PlayerUpdatePatch.usePatch = true;
        EventManager.PlayerIsInShip.AddListener(AlwaysOnStuff.PlayerShipChanged);
        EventManager.PlayerIsDead.AddListener(AlwaysOnStuff.IsPlayerDead);
        EventManager.SpecatingPlayerIsInShip.AddListener(AlwaysOnStuff.OnSpecateShipCheck);

        //Unique
        EventManager.GetNewDisplayText.AddListener(TerminalParse.OnNewDisplayText);

        //CamEvents
        CamEvents.UpdateCamsEvent.AddListener(CamEvents.OnUpdateCamsEvent);
        //CamEvents.UpdateTextures.AddListener(CamEvents.GetTextures);
        CamEvents.UpdateTarget.AddListener(MoreCamStuff.OnTargetSwitch);
    }

    internal static void OnTerminalAwake(Terminal instance)
    {
        Plugin.instance.Terminal = instance;
        Loggers.LogInfo($"Setting Plugin.instance.Terminal");
        CacheDefaultDisplayTexts();
        FontStuff.SetCachedDefault();
        StuffForLibrary.AddCommands(); //replaced addkeywords
        AlwaysOnStuff.screenSettings ??= new(QoLConfig.TerminalScreen.Value);
        if (Plugin.instance.CruiserTerm)
            Compatibility.CruiserTerm.CreateDenyKeyword();
    }

    internal static void CacheDefaultDisplayTexts()
    {
        if (DynamicBools.TryGetKeyword("Other", out TerminalKeyword otherWord))
        {
            if (GameStuff.OneTimeOnly)
                otherWord.specialKeywordResult.displayText = OriginalOtherText;
            else
                OriginalOtherText = otherWord.specialKeywordResult.displayText;
        }
        else
            Loggers.WARNING("Unable to find other command at awake!");

    }
}

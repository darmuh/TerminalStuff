using OpenLib.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using TerminalStuff.Compatibility;
using TerminalStuff.Configs;
using TerminalStuff.Networking;
using TerminalStuff.Util;
using static TerminalStuff.MoonsTweaks.MoonsPlus;

namespace TerminalStuff.MoonsTweaks;

public class MoonInfo
{
    //public
    public string AdditionalInfo = "";

    //internal
    internal SelectableLevel Level;
    internal string LevelName = "";
    internal int LevelID = -1;
    internal bool OTP = false;
    internal TerminalNode PurchaseNode = null!;
    internal TerminalNode ResultNode = null!;
    internal MoonMenuItem MenuItem;

    //bettermenu

    private bool company;
    internal bool IsCompany
    {
        get
        {
            company = IsThisGordion();
            return company;
        }

        set => company = value;
    }

    private bool disabled;
    internal bool IsMoonDisabled
    {
        get
        {
            if (IsCompany)
                return false;

            bool UseDawn = false;
            bool UseLLL = false;
            disabled = false;

            if (!Plugin.instance.NoLevelLoader)
            {
                UseDawn = Plugin.instance.DawnLibPresent && (!Plugin.instance.DawnLLLCombo || !Dawnlib.UseLLLInstead());
                UseLLL = Plugin.instance.LethalLevelLoader && (!Plugin.instance.DawnLLLCombo || Dawnlib.UseLLLInstead());
            }

            if (UseDawn)
                disabled = Dawnlib.IsDisabled(Level);
            if (UseLLL)
                disabled = LLLCompat.IsDisabled(Level);

            if (disabled)
                Loggers.LogDebug($"{Level} is disabled by LLL!");

            return disabled;
        }

        set => disabled = value;
    }
    private bool isHidden;
    internal bool IsHidden
    {
        get
        {
            if (IsCompany)
                return false;

            if (Plugin.instance.NoLevelLoader)
                isHidden = (!Plugin.instance.Terminal.moonsCatalogueList.Contains(Level) && !IsCurrent);
            else
            {
                bool UseDawn = Plugin.instance.DawnLibPresent && (!Plugin.instance.DawnLLLCombo || !Dawnlib.UseLLLInstead());
                bool UseLLL = Plugin.instance.LethalLevelLoader && (!Plugin.instance.DawnLLLCombo || Dawnlib.UseLLLInstead());

                if (UseDawn)
                    isHidden = Dawnlib.IsHidden(Level);

                if (UseLLL)
                    isHidden = LLLCompat.IsHidden(Level);
            }

            Loggers.LogDebug($"{LevelName} IsHidden - {isHidden}");

            return isHidden;
        }
        set
        {
            if (!IsCompany)
                isHidden = value;
        }
    }

    private bool isLocked;
    internal bool IsLocked
    {
        get
        {
            bool UseDawn = false;
            bool UseLLL = false;
            isLocked = false;

            if (!Plugin.instance.NoLevelLoader)
            {
                UseDawn = Plugin.instance.DawnLibPresent && (!Plugin.instance.DawnLLLCombo || !Dawnlib.UseLLLInstead());
                UseLLL = Plugin.instance.LethalLevelLoader && (!Plugin.instance.DawnLLLCombo || Dawnlib.UseLLLInstead());
            }

            if (UseDawn)
                isLocked = Dawnlib.IsLocked(Level);
                
            if (UseLLL)
                isLocked = LLLCompat.IsLocked(Level);

            Loggers.LogDebug($"{LevelName} IsLocked - {isLocked}");

            return isLocked;
        }

        set => isLocked = value;
    }

    internal bool HaveVisited
    {
        get
        {
            if (StartOfRound.Instance == null)
                return false;

            return HasVisited();
        }
        set
        {
            return; //readonly
        }
    }

    internal bool IsCurrent
    {
        get
        {
            if (StartOfRound.Instance == null)
                return false;

            return Level == StartOfRound.Instance.currentLevel;
        }
        set
        {
            return; //readonly
        }
    }

    private int priceVal;
    internal int Price
    {
        get
        {
            if (StartOfRound.Instance == null)
                return priceVal;

            if (NoPrice())
                return priceVal;

            priceVal = GetPrice();
            return priceVal;
        }

        set => priceVal = value; //readonly
    }

    internal int DisplayPrice
    {
        get
        {
            if (StartOfRound.Instance == null)
                return Price;

            if (NoPrice())
                return 0;
            else
                return Price;
        }
        set
        {
            return; //readonly
        }
    }

    internal MoonInfo(SelectableLevel level, List<TerminalNode> AllNodes)
    {
        Level = level;
        LevelID = level.levelID;
        LevelName = GetNumberless(level.PlanetName);
        PurchaseNode = AllNodes.FirstOrDefault(x => x.displayPlanetInfo == Level.levelID);
        ResultNode = AllNodes.FirstOrDefault(x => x.buyRerouteToMoon == Level.levelID);
        MenuItem = new(LevelName);
        MenuItem.SelectionEvent.AddListener(SelectThisMoon);
        MenuItem.OnPageLoad = AddToMenuName;
        MenuItem.moonInfo = this;

        Loggers.LogDebug($"NEW MOONINFO, {LevelName}");
    }

    internal void Reload(SelectableLevel level, List<TerminalNode> AllNodes)
    {
        //LevelID already exists
        Level = level;
        LevelName = GetNumberless(level.PlanetName);
        PurchaseNode = AllNodes.FirstOrDefault(x => x.displayPlanetInfo == Level.levelID);
        ResultNode = AllNodes.FirstOrDefault(x => x.buyRerouteToMoon == Level.levelID);
        MenuItem ??= new(LevelName);
        MenuItem.SelectionEvent.AddListener(SelectThisMoon);
        MenuItem.OnPageLoad = AddToMenuName;
        MenuItem.moonInfo = this;

        Loggers.LogDebug($"RELOAD MOONINFO, {LevelName}");
    }

    public void AddToMenuName()
    {
        MenuItem.Prefix = "";
        MenuItem.Suffix = "";

        if (IsCompany)
            MenuItem.Name = "Gordion (Company)";
        else if (IsLocked)
            MenuItem.Name = "[ROUTE LOCKED]";
        else if (IsHidden)
            MenuItem.Name = "[ ??? ]";
        else
            MenuItem.Name = LevelName;

        if (FilterView.Styling.HasFlag(FilterView.DisplayStyle.Price))
            MenuItem.Prefix += $"${DisplayPrice} ";

        if (IsCurrent)
        {
            MenuItem.Prefix += "<<";
            MenuItem.Suffix += ">>";
        }

        if (FilterView.Styling.HasFlag(FilterView.DisplayStyle.Weather) && GetWeatherName(Level).Length > 1)
            MenuItem.Suffix += $" ({GetWeatherName(Level)})";

        if (FilterView.Styling.HasFlag(FilterView.DisplayStyle.Difficulty))
            MenuItem.Suffix += $" ({Level.riskLevel})";

        if (AdditionalInfo.Length > 0) //add any additional stuff from other mods accessing this attribute
            MenuItem.Suffix += AdditionalInfo;

        if (DisplayPrice <= Plugin.instance.Terminal.groupCredits && MoonsPlusConfig.AffordableColor.Value.Length > 0)
        {
            MenuItem.Prefix = MenuItem.Prefix.Insert(0, $"<color={MoonsPlusConfig.AffordableColor.Value}>");
            MenuItem.Suffix += "</color>";
        }

        if (DisplayPrice > Plugin.instance.Terminal.groupCredits && MoonsPlusConfig.NotEnoughCredsColor.Value.Length > 0)
        {
            MenuItem.Prefix = MenuItem.Prefix.Insert(0, $"<color={MoonsPlusConfig.NotEnoughCredsColor.Value}>");
            MenuItem.Suffix += "</color>";
        }

    }

    private static void UnableToTravel(bool inMotion, bool isLanded, bool currentLevel)
    {
        if (currentLevel)
        {
            Plugin.Log.LogMessage("You are already orbiting this moon!");
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
            return;
        }

        if (inMotion)
        {
            Plugin.Log.LogMessage("The ship is in motion and cannot change course!");
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
            return;
        }

        if (isLanded)
        {
            Plugin.Log.LogMessage("The ship is not in orbit!");
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
            return;
        }

    }

    internal void SelectThisMoon()
    {
        MoonsPlusMenu.ExitAction = null!;

        if (Level == null)
        {
            Loggers.ERROR($"(SelectThisMoon) - Level at Active Moons Selection is NULL! - {MoonsPlusMenu.ActiveSelection}");
            return;
        }

        if (StartOfRound.Instance.travellingToNewLevel || !StartOfRound.Instance.inShipPhase || StartOfRound.Instance.currentLevel == Level)
        {
            UnableToTravel(StartOfRound.Instance.travellingToNewLevel, !StartOfRound.Instance.inShipPhase, StartOfRound.Instance.currentLevel == Level);
            return;
        }


        if (DisplayPrice > Plugin.instance.Terminal.groupCredits)
        {
            Plugin.instance.Terminal.PlayTerminalAudioServerRpc(1);
            return;
        }

        if (MoonsPlusConfig.UseVanillaPurchaseNodes.Value && PurchaseNode != null)
        {
            MoonsPlusMenu.ExitAction = () =>
            {
                CommonTerminal.LoadNewNode(PurchaseNode);
                Loggers.LogDebug("Loading vanilla node!");
            };
            MoonsPlusMenu.ExitMenu(true);
            return;
        }

        int newCreds = Plugin.instance.Terminal.groupCredits - DisplayPrice;

        StartOfRound.Instance.ChangeLevelServerRpc(Level.levelID, newCreds);
    }

    internal void OneTimePurchaseLoadIn()
    {
        if (!MoonsPlusConfig.OneTimePurchase.Value)
            return;

        if (HaveVisited)
        {
            OTP = true;
            if (PurchaseNode != null)
                PurchaseNode.itemCost = 0;
            if (ResultNode != null)
                ResultNode.itemCost = 0;
        }
    }

    internal bool ShowInListing(bool priceCheck = false, bool weatherCheck = false)
    {
        if (IsCurrent)
            return true;

        if (IsMoonDisabled)
            return false;

        if (!MoonsPlusConfig.IncludeHidden.Value && IsHidden)
            return false;

        if (!MoonsPlusConfig.IncludeLocked.Value && IsLocked)
            return false;

        if (IsHidden && IsLocked)
            return false;

        if (IsManuallyHidden())
            return false;

        if (HasBadWeather(weatherCheck))
            return false;

        if (priceCheck && DisplayPrice > Plugin.instance.Terminal.groupCredits)
            return false;

        return true;
    }

    internal bool IsManuallyHidden()
    {
        if (MoonsPlusConfig.AlwaysHideList.Value.Length < 1)
        {
            Loggers.LogInfo($"{LevelName} does not match any configuration entries for AlwaysHideList");
            return false;
        }

        List<string> moons = CommonStringStuff.GetKeywordsPerConfigItem(MoonsPlusConfig.AlwaysHideList.Value, ',');

        if (moons.Any(m => OpenLib.Common.Misc.CompareStringsInvariant(m, LevelName)))
        {
            Loggers.LogInfo($"Matching moon name found! Hiding {LevelName}");
            return true;
        }

        Loggers.LogInfo($"{LevelName} does not match any configuration entries for AlwaysHideList");
        return false;

    }

    internal bool HasBadWeather(bool weatherCheck)
    {
        if (!weatherCheck || AcceptableWeathers.Count == 0)
            return false;


        string currentWeather = GetWeatherName(Level);
        //Loggers.LogDebug($"Checking {LevelName} weather - {currentWeather}");

        if (currentWeather.Length < 1)
            return false;

        return AcceptableWeathers.Any(w => !OpenLib.Common.Misc.CompareStringsInvariant(w, currentWeather));
    }

    internal bool IsThisGordion()
    {
        if (LevelName == "Gordion" || LevelName == "Company")
        {
            LevelName = "Company";
            return true;
        }
        else
            return false;
    }

    internal static string GetNumberless(string name) //inspired by LLL's GetNumberlessPlanetName
    {
        return new string([.. name.SkipWhile(x => !char.IsLetter(x))]);
    }

    internal bool NoPrice()
    {
        if (NetHandler.Instance == null)
            return false;

        if (StartOfRound.Instance == null)
            return false;

        return MoonsPlusConfig.OneTimePurchase.Value && OTP;
    }

    internal int GetPrice()
    {
        //Loggers.LogDebug($"GETPRICE FOR {LevelName}");
        bool UseDawn = false;
        bool UseLLL = false;

        if (!Plugin.instance.NoLevelLoader)
        {
            UseDawn = Plugin.instance.DawnLibPresent && (!Plugin.instance.DawnLLLCombo || !Dawnlib.UseLLLInstead());
            UseLLL = Plugin.instance.LethalLevelLoader && (!Plugin.instance.DawnLLLCombo || Dawnlib.UseLLLInstead());
        }

        if (UseDawn)
            return Dawnlib.GetPrice(Level);
        
        if (UseLLL)
            return LLLCompat.GetPrice(Level);

        if (PurchaseNode == null)
            return 0;

        return PurchaseNode.itemCost;
    }

    internal void UpdateHistory()
    {
        if (NetHandler.Instance == null)
            return;

        Loggers.LogDebug($"{LevelName} UpdateHistory");

        if (IsDisabled())
            return;

        if (!MoonsPlusConfig.RevealHiddenOnRoute.Value)
            return;

        Loggers.LogDebug($"{LevelName} Revealing Hidden Route!");

        if (IsHidden)
            Hide(false);
    }

    internal bool HasVisited()
    {
        bool value = MoonsVisited.Any(x => x == LevelName);
        Loggers.LogDebug($"{LevelName} has been visited = {value}!!");
        return value;
    }

    internal void UpdateInfo()
    {
        if (NetHandler.Instance == null)
            return;

        if (!IsCurrent)
        {
            if (!IsDisabled() && HaveVisited && IsHidden)
                Hide(false);
        }
        else
        {
            SaveManager.AddToTravelHistory(this);
            if (MoonsPlusConfig.OneTimePurchase.Value && !OTP)
                OTP = true;

            if (!MoonsPlusConfig.RevealHiddenOnRoute.Value)
                return;

            Hide(false);
        }

        Loggers.LogDebug($"===\n{LevelName} ran UpdateInfo:\nHide - {isHidden}\nHaveVisited - {HaveVisited}\n===");
    }

    internal void Hide(bool shouldHide)
    {
        if (NetHandler.Instance == null)
            return;

        Loggers.LogDebug($"Hiding {LevelName}");
        

        if (!Plugin.instance.NoLevelLoader)
        {
            bool UseDawn = Plugin.instance.DawnLibPresent && (!Plugin.instance.DawnLLLCombo || !Dawnlib.UseLLLInstead());
            bool UseLLL = Plugin.instance.LethalLevelLoader && (!Plugin.instance.DawnLLLCombo || Dawnlib.UseLLLInstead());
        
            if (UseDawn)
            {
                Dawnlib.ChangeHiddenStatus(Level, shouldHide);
                return;
            }

            if (UseLLL)
            {
                LLLCompat.ChangeHiddenStatus(Level, shouldHide);
                return;
            }
        }

        // not using dawnlib or LLL
        List<SelectableLevel> catalogue = [.. Plugin.instance.Terminal.moonsCatalogueList];
        catalogue.Remove(Level);

        Plugin.instance.Terminal.moonsCatalogueList = [.. catalogue];            
    }

    // This is unused currently
    internal void UnlockUnhide()
    {
        if (NetHandler.Instance == null)
            return;

        Loggers.LogDebug($"Unlock/Unhide {LevelName}");

        if (!Plugin.instance.LethalLevelLoader)
            Hide(false);
        else
            LLLCompat.UnlockUnhide(Level);
            
    }

    internal bool IsDisabled()
    {
        if (IsCurrent)
            return false;

        if (IsLocked && IsHidden)
            return true;

        return false;
    }

    internal void Reset()
    {
        Loggers.LogDebug($"Reset called on {LevelName}!");
        //IsCurrent = false;
        //hasVisited = false;

        if (OTP)
        {
            Loggers.LogDebug($"Setting {LevelName} terminalnodes back to Price - {Price}");
            if (ResultNode != null)
                ResultNode.itemCost = Price;
            if (PurchaseNode != null)
                PurchaseNode.itemCost = Price;
            OTP = false;
        }
    }

    internal static bool IsRouteEnabled(TerminalNode query)
    {
        return !EventSub.TerminalStart.RouteKeyword.compatibleNouns.Any(x => x.result == query);
    }
}

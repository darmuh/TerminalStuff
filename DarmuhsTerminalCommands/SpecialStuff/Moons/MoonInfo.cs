using System.Collections.Generic;
using System.Linq;
using TerminalStuff.Compatibility;

namespace TerminalStuff.SpecialStuff
{
    public class MoonInfo
    {
        //public
        public string AdditionalInfo = "";

        //internal
        internal SelectableLevel Level;
        internal string LevelName = "";
        internal int LevelID = -1;
        internal bool OTP = false;
        internal TerminalNode purchaseNode = null!;
        internal TerminalNode resultNode = null!;

        private bool company;
        internal bool IsCompany
        {
            get
            {
                company = IsThisGordion();
                return company;
            }
            set
            {
                company = value;
            }
        }

        private bool disabled;
        internal bool IsMoonDisabled
        {
            get
            {
                if(IsCompany)
                    return false;

                if (Plugin.instance.LethalLevelLoader)
                    disabled = LLLCompat.IsDisabled(Level);
                else
                    disabled = false;

                if(disabled)
                    Plugin.Spam($"{Level} is disabled by LLL!");

                return disabled;
            }
            set
            {
                disabled = value;
            }
        }
        private bool isHidden;
        internal bool IsHidden
        {
            get
            {
                if(IsCompany)
                    return false;

                if (Plugin.instance.LethalLevelLoader)
                    isHidden = LLLCompat.IsHidden(Level);
                else
                    isHidden = !Plugin.instance.Terminal.moonsCatalogueList.Contains(Level);

                Plugin.Spam($"{LevelName} IsHidden - {isHidden}");

                return isHidden;
            }
            set
            {
                if(!IsCompany)
                    isHidden = value;
            }
        }

        private bool isLocked;
        internal bool IsLocked
        {
            get
            {
                if (Plugin.instance.LethalLevelLoader)
                    isLocked = LLLCompat.IsLocked(Level);
                else
                    isLocked = false;

                Plugin.Spam($"{LevelName} IsLocked - {isLocked}");

                return isLocked;
            }
            set
            {
                isLocked = value;
            }
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
            set
            {
                priceVal = value; //readonly
            }
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
            purchaseNode = AllNodes.FirstOrDefault(x => x.displayPlanetInfo == Level.levelID);
            resultNode = AllNodes.FirstOrDefault(x => x.buyRerouteToMoon == Level.levelID);
            
            Plugin.Spam($"NEW MOONINFO, {LevelName}");
        }

        internal void Reload(SelectableLevel level, List<TerminalNode> AllNodes)
        {
            //LevelID already exists
            Level = level;
            LevelName = GetNumberless(level.PlanetName);
            purchaseNode = AllNodes.FirstOrDefault(x => x.displayPlanetInfo == Level.levelID);
            resultNode = AllNodes.FirstOrDefault(x => x.buyRerouteToMoon == Level.levelID);

            Plugin.Spam($"RELOAD MOONINFO, {LevelName}");
        }

        internal void OneTimePurchaseLoadIn()
        {

            if (HaveVisited)
            {
                OTP = true;
                if(purchaseNode != null)
                    purchaseNode.itemCost = 0;
                if(resultNode != null)
                    resultNode.itemCost = 0;
            }
        }

        internal bool ShowInListing()
        {
            if (IsCurrent)
                return true;

            if(IsMoonDisabled)
                return false;

            if (!MoonsPlusConfig.IncludeHidden.Value && IsHidden)
                return false;

            if (!MoonsPlusConfig.IncludeLocked.Value && IsLocked)
                return false;

            if (IsHidden && IsLocked)
                return false;

            return true;
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

        internal string GetNumberless(string name) //inspired by LLL's GetNumberlessPlanetName
        {
            return new string(name.SkipWhile(x => !char.IsLetter(x)).ToArray());
        }

        internal bool NoPrice()
        {
            if (!ConfigSettings.ModNetworking.Value)
                return false;

            if (StartOfRound.Instance == null)
                return false;

            return MoonsPlusConfig.OneTimePurchase.Value && OTP;
        }

        internal int GetPrice()
        {
            Plugin.Spam($"GETPRICE FOR {LevelName}");

            if (Plugin.instance.LethalLevelLoader)
                return LLLCompat.GetPrice(Level);

            if (purchaseNode == null)
                return 0;

            return purchaseNode.itemCost;
        }

        internal void UpdateHistory()
        {
            if (!ConfigSettings.ModNetworking.Value)
                return;

            Plugin.Spam($"{LevelName} UpdateHistory");

            if (IsDisabled())
                return;

            if (!MoonsPlusConfig.RevealHiddenOnRoute.Value)
                return;

            Plugin.Spam($"{LevelName} Revealing Hidden Route!");

            if(IsHidden)
                Hide(false);
        }

        internal bool HasVisited()
        {
            bool value = MoonsPlus.MoonsVisited.Any(x => x == LevelName);
            Plugin.Spam($"{LevelName} has been visited = {value}!!");
            return value;     
        }

        internal void UpdateInfo()
        {
            if (!ConfigSettings.ModNetworking.Value)
                return;

            if (!IsCurrent)
            {
                if (!IsDisabled() && HaveVisited && IsHidden)
                    Hide(false);
            }
            else
            {
                PluginCore.SaveManager.AddToTravelHistory(this);
                if (MoonsPlusConfig.OneTimePurchase.Value && !OTP)
                    OTP = true;

                if (!MoonsPlusConfig.RevealHiddenOnRoute.Value)
                    return;

                Hide(false);
            }

            Plugin.Spam($"===\n{LevelName} ran UpdateInfo:\nHide - {isHidden}\nHaveVisited - {HaveVisited}\n===");
        }

        internal void Hide(bool shouldHide)
        {
            if (!ConfigSettings.ModNetworking.Value)
                return;

            Plugin.Spam($"Hiding {LevelName}");
            //IsHidden = shouldHide;

            if (Plugin.instance.LethalLevelLoader)
                LLLCompat.ChangeHiddenStatus(Level, shouldHide);
            else
            {
                List<SelectableLevel> catalogue = [.. Plugin.instance.Terminal.moonsCatalogueList];
                if(catalogue.Contains(Level))
                    catalogue.Remove(Level);

                Plugin.instance.Terminal.moonsCatalogueList = [.. catalogue];
            }
        }

        internal void UnlockUnhide()
        {
            if (!ConfigSettings.ModNetworking.Value)
                return;

            Plugin.Spam($"Unlock/Unhide {LevelName}");

            if (Plugin.instance.LethalLevelLoader)
                LLLCompat.UnlockUnhide(Level);
            else
                Hide(false);
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
            Plugin.Spam($"Reset called on {LevelName}!");
            //IsCurrent = false;
            //hasVisited = false;

            if (OTP)
            {
                Plugin.Spam($"Setting {LevelName} terminalnodes back to Price - {Price}");
                if(resultNode != null)
                    resultNode.itemCost = Price;
                if(purchaseNode != null)
                    purchaseNode.itemCost = Price;
                OTP = false;
            }
        }
    }
}

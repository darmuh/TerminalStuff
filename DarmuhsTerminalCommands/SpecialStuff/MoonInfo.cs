using OpenLib.CoreMethods;
using System.Linq;
using TerminalStuff.Compatibility;
using TerminalStuff.PluginCore;

namespace TerminalStuff.SpecialStuff
{
    public class MoonInfo
    {
        //public
        public string AdditionalInfo = "";

        //internal
        internal SelectableLevel Level;
        internal string LevelName = "";
        internal int indexNum = -1;
        internal bool isCurrent = false;
        internal bool isCompany = false;
        internal int price = 0;
        internal bool isHidden = false;
        internal bool isLocked = false;
        internal bool haveVisited = false;

        internal MoonInfo(SelectableLevel level, int index, string name, bool currentLevel)
        {
            Level = level;
            LevelName = GetNumberless(name);
            isCurrent = currentLevel;
            indexNum = index;
            isCompany = IsThisGordion();
            price = GetPrice();

            if (Plugin.instance.LethalLevelLoader)
            {
                isLocked = LLLCompat.IsLocked(level);
                isHidden = LLLCompat.IsHidden(level);
            }
        }

        internal bool ShowInListing()
        {
            if (isCurrent)
                return true;

            if (!MoonsPlusConfig.IncludeHidden.Value && isHidden)
                return false;

            if (!MoonsPlusConfig.IncludeLocked.Value && isLocked)
                return false;

            if (isHidden && isLocked)
                return false;

            return true;
        }

        internal bool IsThisGordion()
        {
            if (LevelName == "Gordion")
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

        internal int GetPrice()
        {
            if (Plugin.instance.LethalLevelLoader)
                return LLLCompat.GetPrice(this.Level);

            TerminalNode moonNode = LogicHandling.GetAllNodes().FirstOrDefault(x => x.buyRerouteToMoon == Level.levelID);
            
            if (moonNode == null)
                return 0;
            
            return moonNode.itemCost;
        }

        internal void UpdateHistory()
        {
            Plugin.Spam($"{LevelName} UpdateHistory");

            if (!IsDisabled())
            {
                if (!MoonsPlusConfig.RevealHiddenOnRoute.Value)
                    return;

                if(isHidden)
                    this.Hide(false);
            }
        }

        internal void UpdateInfo()
        {
            isCurrent = (StartOfRound.Instance.currentLevel == Level);
            price = GetPrice();

            if(Plugin.instance.LethalLevelLoader)
            {
                if (!isCurrent)
                {
                    isLocked = LLLCompat.IsLocked(Level);
                    isHidden = LLLCompat.IsHidden(Level);

                    if (!IsDisabled() && haveVisited && isHidden)
                        Hide(false);
                }
                else
                {
                    if (!haveVisited)
                    {
                        haveVisited = true;
                        SaveManager.AddToTravelHistory(this);
                    }
                    
                    if (!MoonsPlusConfig.RevealHiddenOnRoute.Value)
                        return;

                    Hide(false);
                }
                
            }
        }

        internal void Hide(bool shouldHide)
        {
            isHidden = shouldHide;

            if (Plugin.instance.LethalLevelLoader)
                LLLCompat.ChangeHiddenStatus(Level, shouldHide);

            if(!isHidden && !MoonsPlus.MoonsUnhidden.Contains(this))
                MoonsPlus.MoonsUnhidden.Add(this);
        }

        internal void UnlockUnhide()
        {
            isLocked = false;
            isHidden = false;

            if (Plugin.instance.LethalLevelLoader)
                LLLCompat.UnlockUnhide(Level);
        }

        internal bool IsDisabled()
        {
            if (isCurrent)
                return false;

            if (isLocked && isHidden) 
                return true;

            return false;
        }
    }
}

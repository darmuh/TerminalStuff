using HarmonyLib;
using static TerminalStuff.LeaveTerminal;

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

    
}

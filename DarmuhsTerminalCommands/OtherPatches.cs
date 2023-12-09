using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TerminalStuff
{
    [HarmonyPatch(typeof(StartOfRound), "openingDoorsSequence")]
    public class RoundStartPatch
    {
        public static string getDangerLevel = "";
        public static void Postfix(ref StartOfRound __instance)
        {
            string dangerLevel = __instance.currentLevel.riskLevel;
            getDangerLevel = dangerLevel;
        }
    }
}

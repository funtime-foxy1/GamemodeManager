using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace GamemodeManager.Patches
{
    [HarmonyPatch(typeof(Terminal))]
    internal class TerminalPatch
    {
        static Terminal terminal;
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void StartPatch(Terminal __instance)
        {
            terminal = __instance;
        }
        public static void CloseTerminal()
        {
            terminal?.QuitTerminal();
            
        }
        public static Terminal GetTerminal()
        {
            return terminal;
        }
    }
}

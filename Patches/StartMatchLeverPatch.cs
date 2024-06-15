using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LethalNetworkAPI;
using Object = UnityEngine.GameObject;

namespace GamemodeManager.Patches
{
    [HarmonyPatch(typeof(StartMatchLever))]
    internal class StartMatchLeverPatch
    {
        //False == return
        //True == keep going

        //private static bool hasDisplayed = false;

        [HarmonyPrefix]
        [HarmonyPatch("BeginHoldingInteractOnLever")]
        private static void BeginHoldingPatch(StartMatchLever __instance)
        {
            /*Plugin.Log.LogWarning("GET ALL PLAYERS TO GET ALL ENABLED MODS");
            if (__instance.playersManager.inShipPhase && !__instance.hasDisplayedTimeWarning && (StartOfRoundPatch.localgamemode.active == true || StartOfRoundPatch.gamemode.Value.active == true) && !GameNetworkManager.Instance.gameHasStarted)
                HUDManager.Instance.DisplayTip("WARNING", "Make sure all of the players have the same gamemode mods downloaded.", isWarning: true);*/
        }

        private static bool PullLevelPatch()
        {
            return true;
        }
    }
}

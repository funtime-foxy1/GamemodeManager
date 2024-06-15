using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GamemodeManager.Patches
{
    [HarmonyPatch(typeof(EntranceTeleport))]
    internal class EntrancePatch
    {
        [HarmonyPrefix]
        [HarmonyPatch(nameof(EntranceTeleport.TeleportPlayer))]
        public static bool TeleportPatch()
        {
            //False == return
            //True == keep going
            if (StartOfRoundPatch.entranceLocked.Value)
            {
                HUDManager.Instance.DisplayTip("Locked", "The entrance appears to be locked. (Use the terminal to unlock.)");
                return false;
            }
            return true;
        }
    }
}

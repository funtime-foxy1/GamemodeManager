using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Object = UnityEngine.GameObject;
using Newtonsoft.Json;
using GamemodeManagerAPI.Mods;
using GamemodeManager.UI;
using Steamworks;
using GUI = GamemodeManager.UI.GUI;
using HarmonyLib;
using LethalNetworkAPI;
using GameNetcodeStuff;

namespace GamemodeManager.Patches
{
    [HarmonyPatch(typeof(StartOfRound))]
    internal class StartOfRoundPatch
    {
        // Server Owned
        [PublicNetworkVariable]
        public static LethalNetworkVariable<Gamemode> gamemode = new LethalNetworkVariable<Gamemode>("activegamemode") { Value = new Gamemode("", false) };
        [PublicNetworkVariable]
        public static LethalNetworkVariable<bool> isDoorLocked = new LethalNetworkVariable<bool>("doorLocked") { Value = false };
        [PublicNetworkVariable]
        public static LethalNetworkVariable<bool> entranceLocked = new LethalNetworkVariable<bool>("entranceLocked") { Value = false };
        [PublicNetworkVariable]
        public static LethalNetworkVariable<List<ulong>> spawnedEnemies = new LethalNetworkVariable<List<ulong>>(identifier: "spawnedEnemies") { Value = new List<ulong>() };


        public static LethalClientMessage<bool> toggleShipDoor = new LethalClientMessage<bool>(identifier: "toggleShipDoor");
        public static LethalClientMessage<ulong> explodeUser = new LethalClientMessage<ulong>(identifier: "explodeUser");

        public static Gamemode localgamemode = new Gamemode("", false);
        public static bool hasStarted = false;

        // allPlayerScripts

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StartOfRound.ChangeLevelServerRpc))]
        public static bool ChangeLevelPrefix(ref int levelID, StartOfRound __instance)
        {
            //False == return
            //True == keep going
            if (gamemode.Value.active == false) { return true; }
            if (levelID == 0) { return true; }
            else
            {
                if ( GameNetworkManager.Instance.gameHasStarted && bool.Parse(gamemode.Value.data["forceOneMoon"].ToString()) ) {
                    return false;
                }
            }
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch(nameof(StartOfRound.StartGame))]
        private static void StartPatch()
        {
            spawnedEnemies.Value = new List<ulong>();
            hasStarted = true;
            gamemode.Value = localgamemode;

            if (gamemode.Value.active == false)
            {
                for (int i = 0; i < ModManager.mods.Count; i++)
                {
                    var mod = ModManager.mods[i];
                    mod.ModActive = true;
                }
                return;
            }

            Plugin.Log.LogWarning("Gamemode");

            for (int i = 0; i < ModManager.mods.Count; i++)
            {
                var mod = ModManager.mods[i];
                mod.ModActive = false;
            }
            var allowedMods = JsonConvert.DeserializeObject<List<string>>((string)gamemode.Value.data["allowedMods"]);
            for (int i = 0; i < allowedMods.Count; i++)
            {
                var mod = allowedMods[i];
                var modd = ModManager.getGamemodeById(mod);
                modd.ModActive = true;
            }
            
        }
    
        [HarmonyPrefix]
        [HarmonyPatch("ShipLeave")]
        private static void LeavePatch()
        {
            hasStarted = false;
            spawnedEnemies.Value = new List<ulong>();
        }
    }
}

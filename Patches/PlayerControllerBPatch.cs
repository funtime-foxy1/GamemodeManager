using Steamworks;
using Steamworks.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using LethalNetworkAPI;
using Object = UnityEngine.GameObject;
using GameNetcodeStuff;
using Newtonsoft.Json;
using System.Net.Sockets;
using UnityEngine;

namespace GamemodeManager.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerBPatch
    {
        static LethalClientMessage<List<string>> getAllMods = new LethalClientMessage<List<string>>(identifier: "getAllMods");
        static LethalServerMessage<List<string>> getAllMods_server = new LethalServerMessage<List<string>>(identifier: "getAllMods");
        [HarmonyPostfix]
        [HarmonyPatch("ConnectClientToPlayerObject")]
        private static void ConnectPatch(PlayerControllerB __instance)
        {
            StartOfRoundPatch.toggleShipDoor.OnReceivedFromClient += ToggleShipDoorServer;
            StartOfRoundPatch.explodeUser.OnReceivedFromClient += ExplodeUser;

            if (__instance.GetClientId() == 0)
            {
                //host
                getAllMods_server.OnReceived += (mods, clientId) =>
                {
                    var allowedMods = JsonConvert.DeserializeObject<List<string>>((string)StartOfRoundPatch.localgamemode.data["allowedMods"]);

                    for (int i = 0; i < allowedMods.Count; i++)
                    {
                        var guid = allowedMods[i];
                        if (!mods.Contains(guid))
                        {
                            for (int j = 0; j < StartOfRound.Instance.allPlayerScripts.Length; j++)
                            {
                                var plr = StartOfRound.Instance.allPlayerScripts[j];
                                if (plr.playerClientId == clientId || plr.actualClientId == clientId)
                                {
                                    StartOfRound.Instance.KickPlayer(j);
                                }
                            }

                            Plugin.Log.LogWarning($"ERROR: Player {(int)clientId} donsn't have the mod [{guid}]!.");
                            
                        }
                    }
                    Plugin.Log.LogInfo("Recive from player: " + (int)clientId);
                };
                return;
            }
            //CLIENT
            Plugin.Log.LogInfo("GUUGUUGUGUHUGHUGUHGHGHGU");
            getAllMods.SendServer(MenuPatch.allInstalledGUIDS);


        }

        private static void ExplodeUser(ulong userToExplode, ulong clientId)
        {
            PlayerControllerB destPlayer = null;
            var allPlayers = StartOfRound.Instance.allPlayerScripts;
            for (int i = 0; i < allPlayers.Length; i++)
            {
                var plr = allPlayers[i];
                if (plr.playerClientId == userToExplode)
                {
                    destPlayer = plr;
                }
            }
            Landmine.SpawnExplosion(destPlayer.transform.position, true);
        }

        private static void ToggleShipDoorServer(bool locked, ulong clientId)
        {
            Plugin.Log.LogInfo("frick this gameeeeeeeeeee");
            var shipDoor = GameObject.Find("AnimatedShipDoor").GetComponent<HangarShipDoor>();
            if (locked)
            {
                shipDoor.SetDoorClosed();
                shipDoor.PlayDoorAnimation(true);
                shipDoor.SetDoorButtonsEnabled(false);
            }
            else
            {
                shipDoor.SetDoorButtonsEnabled(true);
                shipDoor.PlayDoorAnimation(false);
                shipDoor.SetDoorOpen();
            }
        }
        /*[HarmonyPrefix]
[HarmonyPatch("Start")]
private static void Start()
{

}*/
    }
}

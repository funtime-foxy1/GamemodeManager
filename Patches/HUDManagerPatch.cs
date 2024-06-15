using GamemodeManager.Util;
using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace GamemodeManager.Patches
{
    [HarmonyPatch(typeof(HUDManager))]
    internal class HUDManagerPatch
    {
        [HarmonyPatch("SubmitChat_performed")]
        [HarmonyPrefix]
        private static void ChatMessageSent(HUDManager __instance)
        {
            string text = __instance.chatTextField.text;
            string localPlayer = GameNetworkManager.Instance.username;

            Plugin.Log.LogInfo("Sent message: " + text + " by " + localPlayer);

            if (!string.IsNullOrEmpty(text) && text.ToLower().StartsWith("/"))
            {
                string command = text.Substring(1);
                Plugin.Log.LogInfo("Use command: " + command);

                string[] keywords = command.Split(' ');

                if (!StartOfRound.Instance.shipHasLanded)
                {
                    HUDManager.Instance.DisplayTip("no", "Please land the ship.");
                    return;
                }

                switch (keywords[0].ToLower())
                {
                    case "spawn":
                        {
                            if (!PlayerUtil.IsHost())
                            {
                                HUDManager.Instance.DisplayTip("Error", "You must be host to use spawning.");
                                return;
                            }
                            Cursor.lockState = CursorLockMode.None;
                            Cursor.visible = true;

                            var UI = GameObject.Find("Systems").transform.Find("UI").transform.Find("Canvas");
                            var spawnUI = GameObject.Instantiate(MenuPatch.panel__spawn, UI);
                            GamemodeManager.UI.GUI.AddSpawnUIFunctionality(spawnUI.transform);
                            spawnUI.SetActive(true);
                            break;
                        }
                    case "teleport":
                        {
                            if (keywords[1].ToLower() == "main")
                            {
                                var entrance = GameObject.Find("EntranceTeleportA");
                                GameNetworkManager.Instance.localPlayerController.transform.position = entrance.transform.position + entrance.transform.forward;
                            } else if (keywords[1].ToLower() == "ship")
                            {
                                var ship = GameObject.Find("ShipInside");
                                GameNetworkManager.Instance.localPlayerController.transform.position = ship.transform.position;
                                // Nuh uh

                                /*var entrance = GameObject.Find("EntranceTeleportA");
                                GameNetworkManager.Instance.localPlayerController.transform.position = entrance.transform.position + entrance.transform.forward;*/
                            }
                            else
                            {
                                HUDManager.Instance.DisplayTip("Whoops", "Invaild syntax.");
                            }

                            break;
                        }
                    case "lock":
                        {
                            if (keywords[1].ToLower() == "main")
                            {
                                var locked = !StartOfRoundPatch.entranceLocked.Value;

                                StartOfRoundPatch.entranceLocked.Value = locked;
                                if (locked) HUDManager.Instance.DisplayTip("Locked", "The entrance has been locked.");
                                else HUDManager.Instance.DisplayTip("Unlocked", "The entrance has been unlocked.");
                            } else if (keywords[1].ToLower() == "ship")
                            {
                                var locked = !StartOfRoundPatch.isDoorLocked.Value;

                                StartOfRoundPatch.isDoorLocked.Value = locked;
                                if (locked) HUDManager.Instance.DisplayTip("Locked", "The ship has been locked.");
                                else HUDManager.Instance.DisplayTip("Unlocked", "The ship has been unlocked.");
                                StartOfRoundPatch.toggleShipDoor.SendAllClients(locked);
                            }
                            else
                            {
                                HUDManager.Instance.DisplayTip("Whoops", "Invaild syntax.");
                            }

                            break;
                        }
                    case "kill":
                        {
                            PlayerControllerB destPlayer = null;
                            var allPlayers = StartOfRound.Instance.allPlayerScripts;
                            for (int i = 0; i < allPlayers.Length; i++)
                            {
                                var plr = allPlayers[i];
                                if (plr.playerUsername.ToLower() == keywords[1].ToLower().Replace("_", " "))
                                {
                                    destPlayer = plr;
                                }
                            }
                            if (!destPlayer)
                            {
                                HUDManager.Instance.DisplayTip("Whoops", "Player not found.");
                                break;
                            }
                            StartOfRoundPatch.explodeUser.SendAllClients(destPlayer.playerClientId);
                            break;
                        }
                }
            }
        }
    }
}

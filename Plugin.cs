using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using GamemodeManager.GameFile;
using GamemodeManager.Patches;
using HarmonyLib;
using UnityEngine;
using TerminalApi;
using TerminalApi.Classes;
using Newtonsoft.Json;
using Steamworks;
using Unity.Netcode;
using LethalNetworkAPI;
using GamemodeManager.Util;
using GamemodeManagerAPI.Mods;
using GamemodeManager.UI;
using GameNetcodeStuff;

namespace GamemodeManager
{
    [BepInPlugin(GUID, NAME, VERSION)]
    [BepInDependency("atomic.terminalapi")]
    [BepInDependency("funfoxrr.GamemodeManagerAPI")]
    public class Plugin : BaseUnityPlugin
    {
        public const string GUID = "funfoxrr.GamemodeManager";
        public const string NAME = "GamemodeManger";
        public const string VERSION = "1.0.0";

        public static Plugin Instance { get; private set; }

        private readonly Harmony harmony = new Harmony(GUID);

        internal static ManualLogSource Log;
        public static string assemblyLocation;
        public static AssetBundle assets;

        //public static LethalNetworkVariable<Gamemode> gamemode = new LethalNetworkVariable<Gamemode>("activegamemode") { Value = new Gamemode("", false) };

        /*private static void NetcodePatcher()
        {
            var types = Assembly.GetExecutingAssembly().GetTypes();
            foreach (var type in types)
            {
                var methods = type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
                foreach (var method in methods)
                {
                    var attributes = method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false);
                    if (attributes.Length > 0)
                    {
                        method.Invoke(null, null);
                    }
                }
            }
        }*/

        //static LethalClientMessage<List<BepInEx.PluginInfo>> getAllMods = new LethalClientMessage<List<BepInEx.PluginInfo>>(identifier: "getAllMods");

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            Log = BepInEx.Logging.Logger.CreateLogSource(GUID);

            Log.LogInfo(GUID + " has loaded!");

            //NetcodePatcher();

            assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!GameModeFile.DoesExist())
            {
                Log.LogInfo("'gamemodes' doesn't exist. Dont worry, the file is being created now!");
                StreamWriter writer = File.CreateText(Path.Combine(assemblyLocation, "gamemodes"));
                writer.WriteLine("Automaticly created by funfoxrr.GamemodeManager");
                writer.WriteLine("Delete this file to remove all gamemodes. (DONT EDIT THIS FILE!!!)\n\n*start");
                writer.Close();
            } else
            {
                Log.LogWarning(GameModeFile.InstalledGamemodeLength() + " detected gamemodes.");

                //Next line will be a gamemode
            }

            assets = AssetBundle.LoadFromFile(Path.Combine(assemblyLocation, "funfoxrr_gamemodes"));
            if (assets == null)
            {
                Log.LogError("Failed to load custom assets."); // ManualLogSource for your plugin
                return;
            }


            TerminalKeyword verb_before = TerminalApi.TerminalApi.CreateTerminalKeyword("kill", true);
            TerminalKeyword destory_before = TerminalApi.TerminalApi.CreateTerminalKeyword("kill_enemy", true);

            TerminalApi.TerminalApi.AddCommand("gamemode", new CommandInfo()
            {
                Category = "Other",
                Description = "Edit the active gamemode.",
                DisplayTextSupplier = () =>
                {
                    var activeGamemode = StartOfRoundPatch.gamemode.Value;
                    //Check for if gamemodes are active
                    if (activeGamemode.active == false)
                    {
                        if (StartOfRoundPatch.localgamemode.active == true)
                        {
                            return "No gamemode is active, but you have a localy enabled gamemode. Please start the game to enable the gamemode.\n\n";
                        }
                        return "No gamemode is active.\n\n";
                    }

                    return ">MODEINFO\nGet info about the active gamemode\n\n" +
                    ">MODEPANEL\nOpen the admin panel\n\n";
                }
            });
            TerminalUtil.AddCommand("modeinfo", () =>
            {
                var activeGamemode = StartOfRoundPatch.gamemode.Value;
                if (activeGamemode.active == false)
                {
                    if (StartOfRoundPatch.localgamemode.active == true)
                    {
                        return "No gamemode is active, but you have a localy enabled gamemode. Please start the game to enable the gamemode.\n\n";
                    }
                    return "No gamemode is active.\n\n";
                }
                var modename = activeGamemode.GUID;
                Plugin.Log.LogInfo("ALL GAMEMODE DATA: " + JsonConvert.SerializeObject(activeGamemode.data));
                var allowedMods = JsonConvert.DeserializeObject<List<string>>((string)activeGamemode.data["allowedMods"]);
                var mods = "UNDEFINED";
                Plugin.Log.LogInfo($"{allowedMods.Count} :: {ModManager.mods.Count}");
                if (allowedMods.Count >= ModManager.mods.Count)
                {
                    mods = "ALL";
                }
                else if (allowedMods.Count < ModManager.mods.Count && allowedMods.Count > 0)
                {
                    mods = "LIMITED";
                }
                else if (allowedMods.Count == 0)
                {
                    mods = "NONE";
                }
                var player = "CLIENT";
                if (PlayerUtil.IsHost())
                {
                    player = "OWNER";
                }
                return $"> GUID: {modename}\n> Mods: {mods}\n> Local Status: {player}\n\n";
            });
            TerminalUtil.AddCommand("modepanel", () =>
            {
                var activeGamemode = StartOfRoundPatch.gamemode.Value;
                if (activeGamemode.active == false)
                {
                    if (StartOfRoundPatch.localgamemode.active == true)
                    {
                        return "No gamemode is active, but you have a localy enabled gamemode. Please start the game to enable the gamemode.\n\n";
                    }
                    return "No gamemode is active.\n\n";
                }
                if (!PlayerUtil.IsHost())
                {
                    return "Must be owner to access the gamemode panel.\n\n";
                }
                return
                ">SPAWN\nSpawn an enemy (GUI will open)\n\n" +
                ">CLOSESHIP\nLock the ship\n\n" +
                ">CLOSEMAIN\nLock the main entrance and the fire exits.\n\n" +
                ">DIE\nKill youself/others\n\n" +
                ">DESTROY\nRemove enemies\n\n";
            });
            TerminalUtil.AddCommand("spawn", () => {
                var activeGamemode = StartOfRoundPatch.gamemode.Value;
                if (activeGamemode.active == false)
                {
                    if (StartOfRoundPatch.localgamemode.active == true)
                    {
                        return "No gamemode is active, but you have a localy enabled gamemode. Please start the game to enable the gamemode.\n\n";
                    }
                    return "No gamemode is active.\n\n";
                }
                if (!PlayerUtil.IsHost())
                {
                    return "Must be owner to access the gamemode panel.\n\n";
                }

                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;

                var UI = GameObject.Find("Systems").transform.Find("UI").transform.Find("Canvas");
                var spawnUI = Instantiate(MenuPatch.panel__spawn, UI);
                GamemodeManager.UI.GUI.AddSpawnUIFunctionality(spawnUI.transform);
                spawnUI.SetActive(true);

                TerminalPatch.CloseTerminal();

                return "Opened UI.";
            });
            TerminalUtil.AddCommand("closeship", () =>
            {
                var activeGamemode = StartOfRoundPatch.gamemode.Value;
                if (activeGamemode.active == false)
                {
                    if (StartOfRoundPatch.localgamemode.active == true)
                    {
                        return "No gamemode is active, but you have a localy enabled gamemode. Please start the game to enable the gamemode.\n\n";
                    }
                    return "No gamemode is active.\n\n";
                }
                if (!PlayerUtil.IsHost())
                {
                    return "Must be owner to access this command.\n\n";
                }
                if (!StartOfRound.Instance.shipHasLanded)
                {
                    return "Ship must be landed to use this command.\n\n";
                }
                var locked = !StartOfRoundPatch.isDoorLocked.Value;
                
                StartOfRoundPatch.toggleShipDoor.SendServer(locked);
                StartOfRoundPatch.isDoorLocked.Value = locked;
                return locked ? "Ship has been locked." : "Ship has been unlocked.";
            });
            TerminalUtil.AddCommand("closemain", () =>
            {
                var activeGamemode = StartOfRoundPatch.gamemode.Value;
                if (activeGamemode.active == false)
                {
                    if (StartOfRoundPatch.localgamemode.active == true)
                    {
                        return "No gamemode is active, but you have a localy enabled gamemode. Please start the game to enable the gamemode.\n\n";
                    }
                    return "No gamemode is active.\n\n";
                }
                if (!PlayerUtil.IsHost())
                {
                    return "Must be owner to access this command.\n\n";
                }
                if (!StartOfRound.Instance.shipHasLanded)
                {
                    return "Ship must be landed to use this command.\n\n";
                }
                var locked = !StartOfRoundPatch.entranceLocked.Value;

                StartOfRoundPatch.entranceLocked.Value = locked;

                return locked ? "Entrance / fire exits have been locked." : "Entrance / fire exits have been unlocked.";
            });
            TerminalUtil.AddCommand("die", () =>
            {
                var activeGamemode = StartOfRoundPatch.gamemode.Value;
                if (activeGamemode.active == false)
                {
                    if (StartOfRoundPatch.localgamemode.active == true)
                    {
                        return "No gamemode is active, but you have a localy enabled gamemode. Please start the game to enable the gamemode.\n\n";
                    }
                    return "No gamemode is active.\n\n";
                }
                if (!PlayerUtil.IsHost())
                {
                    return "Must be owner to access this command.\n\n";
                }
                if (!StartOfRound.Instance.shipHasLanded)
                {
                    return "Ship must be landed to use this command.\n\n";
                }

                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine(">KILL");
                var players = StartOfRound.Instance.allPlayerScripts;

                var isPlayerAvailiable = false;
                for (int i = 0; i < players.Length; i++)
                {
                    Plugin.Log.LogInfo(players[i].playerSteamId + ", " + players[i].playerUsername);
                    if (players[i].playerSteamId == 0) { continue; }
                    if (players[i].playerSteamId == StartOfRound.Instance.localPlayerController.playerSteamId) { continue; }
                    if (players[i].isPlayerDead) { continue; }
                    stringBuilder.AppendLine(players[i].playerUsername.Replace(" ", "_"));

                    isPlayerAvailiable = true;

                    TerminalApi.TerminalApi.DeleteKeyword(players[i].playerUsername);

                    TerminalKeyword noan_after = TerminalApi.TerminalApi.CreateTerminalKeyword(players[i].playerUsername.Replace(" ", "_"));
                    TerminalNode triggerNode = TerminalApi.TerminalApi.CreateTerminalNode("Killing player.", true, "kill_player:" + players[i].playerClientId);
                    var verb = verb_before.AddCompatibleNoun(noan_after, triggerNode);
                    TerminalApi.TerminalApi.AddTerminalKeyword(verb);
                    TerminalApi.TerminalApi.AddTerminalKeyword(noan_after);
                }
                if (!isPlayerAvailiable)
                {
                    stringBuilder.AppendLine("No clients alive.");
                }
                stringBuilder.AppendLine();

                return stringBuilder.ToString();
            });
            TerminalUtil.AddCommand("destroy", () =>
            {
                var activeGamemode = StartOfRoundPatch.gamemode.Value;
                if (activeGamemode.active == false)
                {
                    if (StartOfRoundPatch.localgamemode.active == true)
                    {
                        return "No gamemode is active, but you have a localy enabled gamemode. Please start the game to enable the gamemode.\n\n";
                    }
                    return "No gamemode is active.\n\n";
                }
                if (!PlayerUtil.IsHost())
                {
                    return "Must be owner to access this command.\n\n";
                }
                StringBuilder final = new StringBuilder();

                final.AppendLine("Destroy an enemy:");

                var allEnemies = RoundManager.Instance.SpawnedEnemies;
                for (int i = 0; i < allEnemies.Count; i++)
                {
                    var enemy = allEnemies[i];
                    EnemyType type = enemy.enemyType;

                    TerminalApi.TerminalApi.DeleteKeyword(enemy.NetworkObjectId.ToString());

                    TerminalKeyword noan_after = TerminalApi.TerminalApi.CreateTerminalKeyword(enemy.NetworkObjectId.ToString());
                    TerminalNode triggerNode = TerminalApi.TerminalApi.CreateTerminalNode($"Destroying {type.enemyName}:{enemy.NetworkObjectId}.", true, "destroy_enemy:" + enemy.NetworkObjectId);
                    var verb = destory_before.AddCompatibleNoun(noan_after, triggerNode);
                    TerminalApi.TerminalApi.AddTerminalKeyword(verb);
                    TerminalApi.TerminalApi.AddTerminalKeyword(noan_after);

                    StringBuilder final_enemy_listing = new StringBuilder();

                    final_enemy_listing.Append($"- {type.enemyName}:{enemy.NetworkObjectId}");

                    Plugin.Log.LogInfo(StartOfRoundPatch.spawnedEnemies.Value + " | " + enemy.NetworkObjectId);

                    if ((bool)(StartOfRoundPatch.spawnedEnemies.Value?.Contains(enemy.NetworkObjectId)))
                    {
                        final_enemy_listing.Append(" [USER_SPAWNED]");
                    }

                    final.AppendLine(final_enemy_listing.ToString());
                }

                final.AppendLine();
                final.AppendLine("To destroy and enemy above, type 'kill_enemy <id>'");
                final.AppendLine();

                return final.ToString();
            });

            MenuPatch.selectedNewGamemode += (gamemode) =>
            {
                Log.LogInfo("Update gamemode: " + gamemode.GUID);
                StartOfRoundPatch.localgamemode = gamemode;
                Log.LogInfo(JsonConvert.SerializeObject(StartOfRoundPatch.gamemode));

            };

            TerminalApi.Events.Events.TerminalAwake += Events_TerminalAwake;

            TerminalApi.Events.Events.TerminalParsedSentence += (object Sender, TerminalApi.Events.Events.TerminalParseSentenceEventArgs e) =>
            {
                

                var sect = e.ReturnedNode.terminalEvent.Split(':');
                if (sect[0] == "kill_player")
                {
                    PlayerControllerB destPlayer = null;
                    var allPlayers = StartOfRound.Instance.allPlayerScripts;
                    for (int i = 0; i < allPlayers.Length; i++)
                    {
                        var plr = allPlayers[i];
                        if (plr.playerClientId.ToString() == sect[1])
                        {
                            destPlayer = plr;
                            break;
                        }
                    }

                    StartOfRoundPatch.explodeUser.SendAllClients(destPlayer.playerClientId);
                    //destPlayer?.KillPlayer(Vector3.up);
                }
                if (sect[0] == "destroy_enemy")
                {
                    var enemyId = sect[1];
                    var allEnemies = RoundManager.Instance.SpawnedEnemies;
                    EnemyAI finalenemy = null;
                    for (int i = 0; i < allEnemies.Count; i++)
                    {
                        var enemy = allEnemies[i];
                        EnemyType type = enemy.enemyType;

                        if (enemy.NetworkObjectId.ToString() == enemyId)
                        {
                            finalenemy = enemy;
                            break;
                        }
                    }

                    finalenemy.KillEnemyServerRpc(false);
                    
                }
            };
            /*getAllMods.OnReceived += (_) =>
            {
                getAllMods.SendServer(MenuPatch.allInstalledMods);
            };*/

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(MenuPatch));
            harmony.PatchAll(typeof(StartOfRoundPatch));
            harmony.PatchAll(typeof(RoundManagerPatch));
            harmony.PatchAll(typeof(StartMatchLeverPatch));
            harmony.PatchAll(typeof(PlayerControllerBPatch));
            harmony.PatchAll(typeof(EntrancePatch));
            harmony.PatchAll(typeof(TerminalPatch));
            harmony.PatchAll(typeof(HUDManagerPatch));
        }

        private void Events_TerminalAwake(object sender, TerminalApi.Events.Events.TerminalEventArgs e)
        {
            if (GameNetworkManager.Instance.gameHasStarted && bool.Parse(StartOfRoundPatch.gamemode.Value.data["forceOneMoon"].ToString()))
            {
                TerminalApi.TerminalApi.NodeAppendLine("help", "[WARNING]: You are locked on this current moon. The comapny building is still avaliable for orbiting. ('Force one moon' is enabled.)");
            }
        }
    }

    /*[HarmonyPatch]
    public class NetworkObjectManager
    {
        [HarmonyPostfix, HarmonyPatch(typeof(GameNetworkManager), "Start")]
        public static void Init()
        {
            if (networkPrefab != null)
                return;

            networkPrefab = (GameObject)Plugin.assets.LoadAsset("GamemodeNetworkHandler");
            networkPrefab.AddComponent<NetworkObject>();
            networkPrefab.AddComponent<GamemodeNetworkHandler>();

            NetworkManager.Singleton.AddNetworkPrefab(networkPrefab);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StartOfRound), "Awake")]
        static void SpawnNetworkHandler()
        {
            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
            {
                var networkHandlerHost = GameObject.Instantiate(networkPrefab, Vector3.zero, Quaternion.identity);
                networkHandlerHost.GetComponent<NetworkObject>().Spawn();
            }
        }

        static GameObject networkPrefab;
    }

    public class GamemodeNetworkHandler : NetworkBehaviour
    {
        public static event Action<String> LevelEvent;

        [ClientRpc]
        public void EventClientRpc(string eventName)
        {
            LevelEvent?.Invoke(eventName);
        }

        public override void OnNetworkSpawn()
        {
            LevelEvent = null;

            if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
                Instance?.gameObject.GetComponent<NetworkObject>().Despawn();
            Instance = this;

            base.OnNetworkSpawn();
        }

        public static GamemodeNetworkHandler Instance { get; private set; }
    }*/

}

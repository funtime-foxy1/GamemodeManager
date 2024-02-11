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

namespace GamemodeManager
{
    [BepInPlugin(GUID, NAME, VERSION)]
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

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            Log = BepInEx.Logging.Logger.CreateLogSource(GUID);

            Log.LogInfo(GUID + " has loaded!");

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

            harmony.PatchAll(typeof(Plugin));
            harmony.PatchAll(typeof(MenuPatch));
        }
    }

}

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
using Unity.Netcode;
using LethalNetworkAPI;

namespace GamemodeManager.Patches
{
    [HarmonyPatch(typeof(RoundManager))]
    internal class RoundManagerPatch
    {
        private static Gamemode localgamemode = new Gamemode("", false);
        public static List<SpawnableEnemyWithRarity> allEntities;

        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        private static void StartPatch()
        {
            allEntities = RoundManager.Instance.currentLevel.Enemies;
        }

        
    }
}

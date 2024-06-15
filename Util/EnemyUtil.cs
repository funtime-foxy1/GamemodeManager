using GamemodeManager.Patches;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

namespace GamemodeManager.Util
{
    internal class EnemyUtil
    {
        // idToEntityName.Single(x => x.enemyName == "Name Here")
        public static NetworkObject SpawnEnemy(EnemyType enemy, Transform transformToSpawn)
        {
            if (enemy == null) { return null; }
            var reference = RoundManager.Instance.SpawnEnemyGameObject(transformToSpawn.position, transformToSpawn.rotation.eulerAngles.y, 0, enemy);
            reference.TryGet(out var netObj);
            var spawned = StartOfRoundPatch.spawnedEnemies.Value;
            spawned.Add(reference.NetworkObjectId);
            StartOfRoundPatch.spawnedEnemies.Value = spawned;
            return netObj;
        }

        public static EnemyType GetEnemyByName(string name)
        {
            var enemies = GetAllEnemies();
            return enemies.FirstOrDefault(tuple => tuple.Name == name).Type;
        }

        public static List<(string Name, EnemyType Type)> GetAllEnemies()
        {
            var qmm =  GameObject.FindFirstObjectByType<QuickMenuManager>();
            if (qmm == null) return new List<(string Name, EnemyType Type)>();

            var allEnemies = new List<(string Name, EnemyType Type)>();

            allEnemies.AddRange(qmm.testAllEnemiesLevel.Enemies.Select(t => (t.enemyType.enemyName, t.enemyType)));
            allEnemies.AddRange(qmm.testAllEnemiesLevel.OutsideEnemies.Select(t => (t.enemyType.enemyName, t.enemyType)));
            allEnemies.AddRange(qmm.testAllEnemiesLevel.DaytimeEnemies.Select(t => (t.enemyType.enemyName, t.enemyType)));

            var uniqueEnemies = new HashSet<(string Name, EnemyType Type)>(allEnemies);

            return uniqueEnemies.ToList();
        }
    }
}

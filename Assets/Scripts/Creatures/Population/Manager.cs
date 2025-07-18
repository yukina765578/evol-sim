using System.Collections.Generic;
using EvolutionSimulator.Creatures.Core;
using UnityEngine;
using UnityEngine.Events;

namespace EvolutionSimulator.Creatures.Population
{
    public class Manager : MonoBehaviour
    {
        [Header("Population Settings")]
        [SerializeField]
        [Range(10, 1000)]
        private int initialPopulationSize = 50;

        [SerializeField]
        [Range(10, 1000)]
        private int maxPopulationSize = 200;

        [SerializeField]
        private bool spawnOnStart = true;

        [Header("Auto-Spawning")]
        [SerializeField]
        private bool enableAutoSpawn = true;

        [SerializeField]
        private float spawnInterval = 5f;

        [Header("Statistics")]
        [SerializeField]
        private int totalCreatureSpawned = 0;

        [SerializeField]
        private int totalCreatureDeaths = 0;

        [Header("Debug")]
        [SerializeField]
        private bool showSpawnProgress = false;

        [SerializeField]
        private bool logDeaths = false;

        private Spawner spawner;
        private List<GameObject> creatures = new List<GameObject>();

        private Dictionary<GameObject, UnityAction> creatureDeathActions =
            new Dictionary<GameObject, UnityAction>();

        private float spawnTimer = 0f;

        public List<GameObject> Creatures => new List<GameObject>(creatures);
        public int MaxPopulationSize => maxPopulationSize;

        void Awake()
        {
            spawner = GetComponent<Spawner>();
            if (spawner == null)
            {
                spawner = gameObject.AddComponent<Spawner>();
            }
        }

        void Start()
        {
            if (spawnOnStart)
            {
                SpawnInitialPopulation();
            }
        }

        void Update()
        {
            HandleAutoSpawning();

            // if (Time.time % 5 < Time.deltaTime)
            // {
            //     EventDebugger.LogCounts();
            // }
        }

        void HandleAutoSpawning()
        {
            if (!enableAutoSpawn || spawner == null)
                return;

            spawnTimer += Time.deltaTime;
            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;
                if (creatures.Count < initialPopulationSize)
                {
                    GameObject creature = SpawnSingleCreature();

                    if (showSpawnProgress)
                    {
                        Debug.Log($"Spawned creature {creatures.Count}/{initialPopulationSize}");
                    }
                }
            }
        }

        [ContextMenu("Spawn Initial Population")]
        public void SpawnInitialPopulation()
        {
            ClearPopulation();

            if (showSpawnProgress)
                Debug.Log($"Spawning {initialPopulationSize} creatures...");

            for (int i = 0; i < initialPopulationSize; i++)
            {
                SpawnSingleCreature($"Creature_{i + 1}");
            }

            if (showSpawnProgress)
            {
                Debug.Log(
                    $"Initial population of {creatures.Count}/{initialPopulationSize} spawned."
                );
            }
        }

        public GameObject SpawnSingleCreature(string creatureName = null)
        {
            if (creatures.Count >= maxPopulationSize)
                return null;

            GameObject creature = spawner.SpawnCreature(creatureName);
            if (creature != null)
            {
                creatures.Add(creature);
                totalCreatureSpawned++;

                Energy energy = creature.GetComponent<Energy>();
                if (energy != null)
                {
                    UnityAction deathAction = () => OnCreatureDeath(creature);
                    energy.OnDeath.AddListener(deathAction);
                    creatureDeathActions[creature] = deathAction;
                    EventDebugger.CreatureDeathListeners++;
                }

                if (showSpawnProgress)
                {
                    Debug.Log($"Spawned creature: {creature.name} (Total: {totalCreatureSpawned})");
                }
            }

            return creature;
        }

        void OnCreatureDeath(GameObject creature)
        {
            creatures.Remove(creature);
            totalCreatureDeaths++;

            Energy energy = creature.GetComponent<Energy>();
            if (
                energy != null
                && creatureDeathActions.TryGetValue(creature, out UnityAction deathAction)
            )
            {
                energy.OnDeath.RemoveListener(deathAction);
                creatureDeathActions.Remove(creature);
                EventDebugger.CreatureDeathListeners--;
            }

            if (logDeaths)
            {
                Debug.Log(
                    $"Creature {creature.name} has died. Total deaths: {totalCreatureDeaths}"
                );
            }
        }

        public void RegisterExistingCreature(GameObject creature)
        {
            if (creature != null && !creatures.Contains(creature))
            {
                Energy energy = creature.GetComponent<Energy>();
                if (energy != null)
                {
                    UnityAction deathAction = () => OnCreatureDeath(creature);
                    energy.OnDeath.AddListener(deathAction);
                    creatureDeathActions[creature] = deathAction;
                    EventDebugger.CreatureDeathListeners++;
                }

                creatures.Add(creature);
                totalCreatureSpawned++;
            }
        }

        public void ClearPopulation()
        {
            foreach (GameObject creature in creatures)
            {
                if (creature != null)
                {
                    Energy energy = creature.GetComponent<Energy>();
                    if (
                        energy != null
                        && creatureDeathActions.TryGetValue(creature, out UnityAction deathAction)
                    )
                    {
                        energy.OnDeath.RemoveListener(deathAction);
                        creatureDeathActions.Remove(creature);
                        EventDebugger.CreatureDeathListeners--;
                    }
                    DestroyImmediate(creature);
                }
            }

            creatures.Clear();
            creatureDeathActions.Clear(); // ✅ Clear the dictionary

            if (showSpawnProgress)
            {
                Debug.Log("All creatures have been cleared from the population.");
            }
        }

        void OnGUI()
        {
            if (!showSpawnProgress)
                return;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"Population Stats:");
            GUILayout.Label($"Active Creatures: {creatures.Count}");
            GUILayout.Label($"Total Spawned: {totalCreatureSpawned}");
            GUILayout.Label($"Total Deaths: {totalCreatureDeaths}");
            GUILayout.Label($"Death Listeners: {EventDebugger.CreatureDeathListeners}");
            GUILayout.EndArea();
        }

        void OnValidate()
        {
            initialPopulationSize = Mathf.Clamp(initialPopulationSize, 1, 1000);
            maxPopulationSize = Mathf.Clamp(maxPopulationSize, initialPopulationSize, 1000);
            spawnInterval = Mathf.Max(0.1f, spawnInterval);
        }

        void OnDestroy()
        {
            ClearPopulation();
        }
    }
}

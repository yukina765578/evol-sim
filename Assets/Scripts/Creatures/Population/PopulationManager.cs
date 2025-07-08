using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class PopulationManager : MonoBehaviour
    {
        [Header("Population Settings")]
        [SerializeField]
        [Range(10, 500)]
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

        [SerializeField]
        private int generationCounter = 0;

        [Header("Statistics")]
        [SerializeField]
        private int totalCreaturesSpawned = 0;

        [SerializeField]
        private int totalCreatureDeaths = 0;

        [SerializeField]
        private float averageLifespan = 0f;

        [Header("Debug")]
        [SerializeField]
        private bool showSpawnProgress = true;

        [SerializeField]
        private bool logDeaths = true;

        private CreatureSpawner creatureSpawner;
        private List<GameObject> activeCreatures = new List<GameObject>();
        private List<float> lifespanHistory = new List<float>();
        private float spawnTimer = 0f;

        public int PopulationSize => initialPopulationSize;
        public int MaxPopulationSize => maxPopulationSize;
        public int ActiveCreatureCount => activeCreatures.Count;
        public int TotalSpawned => totalCreaturesSpawned;
        public int TotalDeaths => totalCreatureDeaths;
        public float AverageLifespan => averageLifespan;
        public List<GameObject> ActiveCreatures => new List<GameObject>(activeCreatures);

        void Awake()
        {
            creatureSpawner = GetComponent<CreatureSpawner>();
            if (creatureSpawner == null)
            {
                creatureSpawner = gameObject.AddComponent<CreatureSpawner>();
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
            CleanupDeadCreatures();
            HandleAutoSpawning();
        }

        void HandleAutoSpawning()
        {
            if (!enableAutoSpawn)
                return;

            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                spawnTimer = 0f;

                if (activeCreatures.Count < initialPopulationSize)
                {
                    string creatureName = $"Gen{generationCounter}_Auto_{totalCreaturesSpawned:D3}";
                    SpawnSingleCreature(creatureName);

                    if (showSpawnProgress)
                    {
                        Debug.Log(
                            $"Auto-spawned: {creatureName} (Population: {activeCreatures.Count}/{initialPopulationSize})"
                        );
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
                SpawnSingleCreature($"Gen{generationCounter}_Creature_{i:D3}");
            }

            if (showSpawnProgress)
            {
                Debug.Log(
                    $"Successfully spawned {activeCreatures.Count}/{initialPopulationSize} creatures"
                );
            }
        }

        public GameObject SpawnSingleCreature(string creatureName = null)
        {
            if (activeCreatures.Count >= maxPopulationSize)
                return null;

            string finalName = creatureName ?? $"Creature_{totalCreaturesSpawned:D3}";

            GameObject creature = creatureSpawner.SpawnCreature(finalName);
            if (creature != null)
            {
                var energy = creature.GetComponent<CreatureEnergy>();
                if (energy != null)
                {
                    energy.OnDeath.AddListener(() => OnCreatureDeath(creature, energy.Age));
                }

                activeCreatures.Add(creature);
                totalCreaturesSpawned++;

                return creature;
            }

            Debug.LogWarning("Failed to spawn creature - no valid spawn position found");
            return null;
        }

        void CleanupDeadCreatures()
        {
            activeCreatures.RemoveAll(creature => creature == null);
        }

        void OnCreatureDeath(GameObject creature, float lifespan)
        {
            if (creature != null)
            {
                activeCreatures.Remove(creature);
                totalCreatureDeaths++;
                lifespanHistory.Add(lifespan);
                CalculateAverageLifespan();

                if (logDeaths)
                {
                    Debug.Log(
                        $"Population: {creature.name} died (Age: {lifespan:F1}s) - Active: {activeCreatures.Count}, Total Deaths: {totalCreatureDeaths}"
                    );
                }
            }
        }

        void CalculateAverageLifespan()
        {
            if (lifespanHistory.Count > 0)
            {
                float total = 0f;
                foreach (float lifespan in lifespanHistory)
                {
                    total += lifespan;
                }
                averageLifespan = total / lifespanHistory.Count;
            }
        }

        [ContextMenu("Clear Population")]
        public void ClearPopulation()
        {
            foreach (GameObject creature in activeCreatures)
            {
                if (creature != null)
                    DestroyImmediate(creature);
            }

            activeCreatures.Clear();
            creatureSpawner.ClearSpawnedPositions();

            if (showSpawnProgress)
            {
                Debug.Log("Population cleared");
            }
        }

        [ContextMenu("Reset Statistics")]
        public void ResetStatistics()
        {
            totalCreaturesSpawned = 0;
            totalCreatureDeaths = 0;
            averageLifespan = 0f;
            lifespanHistory.Clear();
            generationCounter = 0;

            Debug.Log("Population statistics reset");
        }

        public void RemoveCreature(GameObject creature)
        {
            activeCreatures.Remove(creature);
        }

        public void RegisterExistingCreature(GameObject creature)
        {
            if (creature != null && !activeCreatures.Contains(creature))
            {
                var energy = creature.GetComponent<CreatureEnergy>();
                if (energy != null)
                {
                    energy.OnDeath.AddListener(() => OnCreatureDeath(creature, energy.Age));
                }

                activeCreatures.Add(creature);
                totalCreaturesSpawned++;
            }
        }

        void OnValidate()
        {
            initialPopulationSize = Mathf.Clamp(initialPopulationSize, 1, 500);
            spawnInterval = Mathf.Max(0.1f, spawnInterval);
        }
    }
}

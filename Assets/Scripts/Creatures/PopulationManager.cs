using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class PopulationManager : MonoBehaviour
    {
        [Header("Population Settings")]
        [SerializeField]
        [Range(10, 500)]
        private int initialPopulationSize = 10;

        [SerializeField]
        private bool spawnOnStart = true;

        [Header("Debug")]
        [SerializeField]
        private bool showSpawnProgress = true;

        private CreatureSpawner creatureSpawner;
        private List<GameObject> activeCreatures = new List<GameObject>();

        public int PopulationSize => initialPopulationSize;
        public int ActiveCreatureCount => activeCreatures.Count;
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

        [ContextMenu("Spawn Initial Population")]
        public void SpawnInitialPopulation()
        {
            ClearPopulation();

            if (showSpawnProgress)
                Debug.Log($"Spawning {initialPopulationSize} creatures...");

            int successfulSpawns = 0;
            for (int i = 0; i < initialPopulationSize; i++)
            {
                GameObject creature = creatureSpawner.SpawnCreature();
                if (creature != null)
                {
                    creature.name = $"Creature_{i:D3}";
                    activeCreatures.Add(creature);
                    successfulSpawns++;
                }
            }

            if (showSpawnProgress)
            {
                Debug.Log(
                    $"Successfully spawned {successfulSpawns}/{initialPopulationSize} creatures"
                );
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
        }

        public void RemoveCreature(GameObject creature)
        {
            activeCreatures.Remove(creature);
        }

        void OnValidate()
        {
            initialPopulationSize = Mathf.Clamp(initialPopulationSize, 10, 500);
        }
    }
}

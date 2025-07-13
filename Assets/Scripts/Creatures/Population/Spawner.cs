using System.Collections.Generic;
using EvolutionSimulator.Creatures.Genetics;
using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Population
{
    public class Spawner : MonoBehaviour
    {
        [Header("Spawner Settings")]
        [SerializeField]
        private float spawnAreaRatio = 0.8f;

        private Boundaries boundaries;

        void Awake()
        {
            boundaries = GetComponent<Boundaries>();
            if (boundaries == null)
            {
                boundaries = FindFirstObjectByType<Boundaries>();
                if (boundaries == null)
                {
                    Debug.LogError("Spawner requires Boundaries component in scene!");
                }
            }
        }

        public GameObject SpawnCreature(string creatureName = null)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            CreatureGenome genome = Randomizer.GenerateRandomGenome();
            GameObject creature = Builder.BuildCreature(genome, spawnPosition);

            if (!string.IsNullOrEmpty(creatureName))
            {
                creature.name = creatureName;
            }
            return creature;
        }

        Vector3 GetValidSpawnPosition()
        {
            if (boundaries == null)
            {
                Debug.LogError("Boundaries component is not set!");
                return Vector3.zero;
            }

            Bounds worldBounds = boundaries.WorldBounds;
            Vector2 spawnAreaSize = worldBounds.size * spawnAreaRatio;

            Vector2 randomPoint = Random.insideUnitCircle;
            Vector3 spawnPosition = new Vector3(
                randomPoint.x * spawnAreaSize.x * 0.5f,
                randomPoint.y * spawnAreaSize.y * 0.5f,
                0f
            );
            return spawnPosition;
        }
    }
}

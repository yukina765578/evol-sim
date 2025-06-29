using System.Collections.Generic;
using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class CreatureSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField]
        private float minDistanceBetweenCreatures = 4f;

        [SerializeField]
        private int maxPositionAttempts = 50;

        [SerializeField]
        [Range(0.1f, 1f)]
        private float spawnAreaRatio = 0.5f;

        private Boundaries boundaries;
        private List<Vector3> spawnedPositions = new List<Vector3>();

        void Awake()
        {
            boundaries = GetComponent<Boundaries>();
            if (boundaries == null)
            {
                boundaries = FindFirstObjectByType<Boundaries>();
                if (boundaries == null)
                {
                    Debug.LogError("CreatureSpawner requires Boundaries component in scene!");
                }
            }
        }

        public GameObject SpawnCreature(string creatureName = null)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            if (spawnPosition == Vector3.zero)
            {
                Debug.LogWarning("Failed to find valid spawn position after max attempts");
                return null;
            }

            CreatureGenome genome = RandomGeneGenerator.GenerateRandomGenome();
            GameObject creature = CreatureBuilder.BuildCreature(genome, spawnPosition);

            if (creature != null)
            {
                if (!string.IsNullOrEmpty(creatureName))
                    creature.name = creatureName;

                spawnedPositions.Add(spawnPosition);
            }

            return creature;
        }

        // Overload for backward compatibility
        public GameObject SpawnCreature()
        {
            return SpawnCreature(null);
        }

        Vector3 GetValidSpawnPosition()
        {
            if (boundaries == null)
                return Vector3.zero;

            Bounds worldBounds = boundaries.WorldBounds;
            Vector2 spawnAreaSize = worldBounds.size * spawnAreaRatio;

            for (int attempt = 0; attempt < maxPositionAttempts; attempt++)
            {
                Vector2 randomPoint = Random.insideUnitCircle;
                Vector3 candidatePosition = new Vector3(
                    randomPoint.x * spawnAreaSize.x * 0.5f,
                    randomPoint.y * spawnAreaSize.y * 0.5f,
                    0f
                );

                if (IsPositionValid(candidatePosition))
                {
                    return candidatePosition;
                }
            }

            return Vector3.zero;
        }

        bool IsPositionValid(Vector3 position)
        {
            foreach (Vector3 existingPos in spawnedPositions)
            {
                if (Vector3.Distance(position, existingPos) < minDistanceBetweenCreatures)
                {
                    return false;
                }
            }
            return true;
        }

        public void ClearSpawnedPositions()
        {
            spawnedPositions.Clear();
        }

        public void RemoveSpawnPosition(Vector3 position)
        {
            spawnedPositions.Remove(position);
        }
    }
}

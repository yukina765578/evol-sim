using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Environment
{
    public class FoodSpawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField]
        private int maxFoodPerCell = 3;

        [Header("Food Settings")]
        [SerializeField]
        private GameObject foodPrefab;

        [SerializeField]
        private float energyValue = 10f;

        [SerializeField]
        private float respawnDelay = 5f;

        [Header("Debug")]
        [SerializeField]
        private bool enableDebugLogging = false;

        private NoiseManager noiseManager;
        private NoiseSettings noiseSettings;
        private List<FoodItem> activeFoods = new List<FoodItem>();
        private HashSet<Vector2> pendingRespawns = new HashSet<Vector2>();
        private int totalFoodSpawned = 0;
        private int totalFoodConsumed = 0;

        void Start()
        {
            noiseManager = GetComponent<NoiseManager>();
            if (noiseManager == null)
            {
                Debug.LogError("FoodSpawner requires NoiseManager component!");
                return;
            }
            noiseSettings = noiseManager.Settings;
            InitialSpawn();
        }

        void Update()
        {
            CleanupDestroyedFood();
        }

        void CleanupDestroyedFood()
        {
            activeFoods.RemoveAll(food => food == null);
        }

        void InitialSpawn()
        {
            if (noiseManager == null || foodPrefab == null)
                return;

            Bounds worldBounds = noiseManager.WorldBounds;
            Vector2 cellSize = new Vector2(
                worldBounds.size.x / noiseSettings.gridWidth,
                worldBounds.size.y / noiseSettings.gridHeight
            );

            for (int x = 0; x < noiseSettings.gridWidth; x++)
            {
                for (int y = 0; y < noiseSettings.gridHeight; y++)
                {
                    Vector2 cellCenter = new Vector2(
                        worldBounds.min.x + (x + 0.5f) * cellSize.x,
                        worldBounds.min.y + (y + 0.5f) * cellSize.y
                    );

                    float noiseValue = PerlinNoise.SampleRaw(
                        cellCenter.x,
                        cellCenter.y,
                        noiseSettings.scale,
                        noiseSettings.offset,
                        noiseSettings.contrast
                    );

                    int spawnAttempts = Mathf.RoundToInt(noiseValue * maxFoodPerCell);

                    for (int attempt = 0; attempt < spawnAttempts; attempt++)
                    {
                        if (Random.value < noiseValue)
                        {
                            Vector2 spawnPos = GetRandomPositionInCell(cellCenter, cellSize);
                            SpawnFood(spawnPos);
                        }
                    }
                }
            }

            if (enableDebugLogging)
                Debug.Log($"Initial spawn complete: {activeFoods.Count} food items created");
        }

        Vector2 GetRandomPositionInCell(Vector2 cellCenter, Vector2 cellSize)
        {
            float halfWidth = cellSize.x * 0.5f;
            float halfHeight = cellSize.y * 0.5f;

            return new Vector2(
                cellCenter.x + Random.Range(-halfWidth, halfWidth),
                cellCenter.y + Random.Range(-halfHeight, halfHeight)
            );
        }

        void SpawnFood(Vector2 position)
        {
            if (foodPrefab == null)
            {
                Debug.LogError("Food prefab is null!");
                return;
            }

            try
            {
                GameObject foodObj = Instantiate(
                    foodPrefab,
                    position,
                    Quaternion.identity,
                    transform
                );
                FoodItem foodItem = foodObj.GetComponent<FoodItem>();

                if (foodItem == null)
                    foodItem = foodObj.AddComponent<FoodItem>();

                foodItem.Initialize(energyValue, this);
                activeFoods.Add(foodItem);
                totalFoodSpawned++;

                if (enableDebugLogging)
                    Debug.Log($"Food spawned at {position}. Total active: {activeFoods.Count}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to spawn food at {position}: {e.Message}");
            }
        }

        public void OnFoodConsumed(FoodItem food, Vector2 position)
        {
            if (food != null)
                activeFoods.Remove(food);

            totalFoodConsumed++;

            if (enableDebugLogging)
                Debug.Log(
                    $"Food consumed at {position}. Active: {activeFoods.Count}, Pending: {pendingRespawns.Count}"
                );

            // Prevent duplicate respawns at same position
            if (!pendingRespawns.Contains(position))
            {
                pendingRespawns.Add(position);
                StartCoroutine(RespawnFood(position));
            }
        }

        IEnumerator RespawnFood(Vector2 position)
        {
            yield return new WaitForSeconds(respawnDelay);

            // Remove from pending after delay
            pendingRespawns.Remove(position);

            // Validate dependencies still exist
            if (noiseManager == null || noiseSettings == null)
            {
                Debug.LogWarning("NoiseManager or settings became null during respawn");
                yield break;
            }

            // float currentNoise = PerlinNoise.SampleRaw(
            //     position.x,
            //     position.y,
            //     noiseSettings.scale,
            //     noiseSettings.offset,
            //     noiseSettings.contrast
            // );
            //
            // // Ensure minimum respawn chance
            // float respawnChance = Mathf.Max(currentNoise, 0.1f);
            //
            // if (Random.value < respawnChance)
            // {
            //     SpawnFood(position);
            // }
            SpawnFood(position);
            // else if (enableDebugLogging)
            // {
            //     Debug.Log($"Respawn failed at {position} (noise: {currentNoise:F2})");
            // }
        }

        void OnDestroy()
        {
            StopAllCoroutines();
            pendingRespawns.Clear();
        }

        // Debug info
        public int ActiveFoodCount => activeFoods.Count;
        public int PendingRespawnCount => pendingRespawns.Count;
        public int TotalSpawned => totalFoodSpawned;
        public int TotalConsumed => totalFoodConsumed;
    }
}

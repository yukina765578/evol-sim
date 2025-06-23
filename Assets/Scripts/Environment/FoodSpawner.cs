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

        private NoiseManager noiseManager;
        private NoiseSettings noiseSettings;
        private List<FoodItem> activeFoods = new List<FoodItem>();
        private float timeOffset = 0f;

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
            if (noiseSettings.animate)
            {
                timeOffset += noiseSettings.animationSpeed * Time.deltaTime;
            }
        }

        void InitialSpawn()
        {
            if (noiseManager == null)
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
                return;

            GameObject foodObj = Instantiate(foodPrefab, position, Quaternion.identity, transform);
            FoodItem foodItem = foodObj.GetComponent<FoodItem>();

            if (foodItem == null)
                foodItem = foodObj.AddComponent<FoodItem>();

            foodItem.Initialize(energyValue, this);
            activeFoods.Add(foodItem);
        }

        public void OnFoodConsumed(FoodItem food, Vector2 position)
        {
            activeFoods.Remove(food);
            StartCoroutine(RespawnFood(position));
        }

        System.Collections.IEnumerator RespawnFood(Vector2 position)
        {
            yield return new WaitForSeconds(respawnDelay);

            float currentNoise = PerlinNoise.SampleRaw(
                position.x,
                position.y,
                noiseSettings.scale,
                noiseSettings.offset,
                noiseSettings.contrast
            );

            if (Random.value < currentNoise)
            {
                SpawnFood(position);
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Environment
{
    [System.Serializable]
    public struct FoodData
    {
        public Vector2 position;
        public float energy;
        public bool consumed;
        public float respawnTimer;
        public int gridX;
        public int gridY;
    }

    public class FoodManager : MonoBehaviour
    {
        [Header("Food Settings")]
        [SerializeField]
        private float energyValue = 10f;

        [SerializeField]
        private float respawnDelay = 5f;

        [SerializeField]
        private int maxFoodPerCell = 3;

        [Header("Rendering")]
        [SerializeField]
        private Color foodColor = Color.green;

        [SerializeField]
        private float foodSize = 0.5f;

        [Header("Collision Detection")]
        [SerializeField]
        private int gridResolution = 50;

        [SerializeField]
        private float detectionRadius = 1f;

        private FoodData[] foodItems;
        private int maxFoodCount;
        private int activeFoodCount;
        private Matrix4x4[] renderMatrices;
        private List<int>[] spatialGrid;
        private Vector2 cellSize;
        private Bounds worldBounds;

        private Material foodMaterial;
        private Mesh foodMesh;
        private NoiseManager noiseManager;
        private NoiseSettings noiseSettings;

        public int ActiveFoodCount => activeFoodCount;
        public int MaxFoodCount => maxFoodCount;

        void Awake()
        {
            noiseManager = GetComponent<NoiseManager>();
            if (noiseManager == null)
            {
                Debug.LogError("FoodManager requires NoiseManager component!");
                return;
            }
            noiseSettings = noiseManager.Settings;
            InitializeSystem();
        }

        void Start()
        {
            Debug.Log("FoodManager initialized with settings: " + noiseSettings);
            worldBounds = noiseManager.WorldBounds;
            cellSize = new Vector2(
                worldBounds.size.x / gridResolution,
                worldBounds.size.y / gridResolution
            );
            InitialSpawn();
            Debug.Log("Initial food spawn complete. Active food count: " + activeFoodCount);
        }

        void Update()
        {
            ProcessRespawns();
            UpdateSpatialGrid();
            RenderFood();
        }

        void InitializeSystem()
        {
            maxFoodCount = noiseSettings.gridWidth * noiseSettings.gridHeight * maxFoodPerCell;
            foodItems = new FoodData[maxFoodCount];
            renderMatrices = new Matrix4x4[maxFoodCount];
            spatialGrid = new List<int>[gridResolution * gridResolution];

            for (int i = 0; i < spatialGrid.Length; i++)
                spatialGrid[i] = new List<int>();

            CreateFoodMesh();
        }

        void CreateFoodMesh()
        {
            foodMesh = new Mesh();
            Vector3[] vertices =
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
            };

            int[] triangles = { 0, 2, 1, 0, 3, 2 };
            Vector2[] uv =
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };

            foodMesh.vertices = vertices;
            foodMesh.triangles = triangles;
            foodMesh.uv = uv;
            foodMesh.RecalculateNormals();

            foodMaterial = new Material(Shader.Find("Standard"));
            foodMaterial.color = foodColor;
            foodMaterial.enableInstancing = true;
        }

        void InitialSpawn()
        {
            activeFoodCount = 0;
            for (int x = 0; x < noiseSettings.gridWidth; x++)
            {
                for (int y = 0; y < noiseSettings.gridHeight; y++)
                {
                    Vector2 cellCenter = new Vector2(
                        worldBounds.min.x
                            + (x + 0.5f) * (worldBounds.size.x / noiseSettings.gridWidth),
                        worldBounds.min.y
                            + (y + 0.5f) * (worldBounds.size.y / noiseSettings.gridHeight)
                    );
                    float noiseValue = PerlinNoise.SampleRaw(
                        cellCenter.x,
                        cellCenter.y,
                        noiseSettings.scale,
                        noiseSettings.offset,
                        noiseSettings.contrast
                    );
                    int spawnAttempts = Mathf.RoundToInt(noiseValue * maxFoodPerCell);
                    for (
                        int attempt = 0;
                        attempt < spawnAttempts && activeFoodCount < maxFoodCount;
                        attempt++
                    )
                    {
                        if (Random.value < noiseValue)
                        {
                            Vector2 spawnPos = GetRandomPositionInCell(
                                cellCenter,
                                new Vector2(
                                    worldBounds.size.x / noiseSettings.gridWidth,
                                    worldBounds.size.y / noiseSettings.gridHeight
                                )
                            );
                            SpawnFood(spawnPos);
                        }
                    }
                }
            }
        }

        Vector2 GetRandomPositionInCell(Vector2 cellCenter, Vector2 cellSize)
        {
            return new Vector2(
                cellCenter.x + Random.Range(-cellSize.x * 0.5f, cellSize.x * 0.5f),
                cellCenter.y + Random.Range(-cellSize.y * 0.5f, cellSize.y * 0.5f)
            );
        }

        void SpawnFood(Vector2 position)
        {
            if (activeFoodCount >= maxFoodCount)
                return;

            int gridX = Mathf.Clamp(
                Mathf.FloorToInt((position.x - worldBounds.min.x) / cellSize.x),
                0,
                gridResolution - 1
            );
            int gridY = Mathf.Clamp(
                Mathf.FloorToInt((position.y - worldBounds.min.y) / cellSize.y),
                0,
                gridResolution - 1
            );

            foodItems[activeFoodCount] = new FoodData
            {
                position = position,
                energy = energyValue,
                consumed = false,
                respawnTimer = 0f,
                gridX = gridX,
                gridY = gridY,
            };

            activeFoodCount++;
        }

        public bool TryConsumeFood(Vector2 position, float radius, out float energy)
        {
            energy = 0f;
            int gridX = Mathf.FloorToInt((position.x - worldBounds.min.x) / cellSize.x);
            int gridY = Mathf.FloorToInt((position.y - worldBounds.min.y) / cellSize.y);

            for (int x = gridX - 1; x <= gridX + 1; x++)
            {
                for (int y = gridY - 1; y <= gridY + 1; y++)
                {
                    if (x < 0 || x >= gridResolution || y < 0 || y >= gridResolution)
                        continue;

                    int cellIndex = y * gridResolution + x;
                    foreach (int index in spatialGrid[cellIndex])
                    {
                        if (index >= activeFoodCount || foodItems[index].consumed)
                            continue;

                        if (Vector2.Distance(foodItems[index].position, position) <= radius)
                        {
                            // Fix: Modify the actual array element, not a copy
                            var food = foodItems[index];
                            food.consumed = true;
                            food.respawnTimer = respawnDelay;
                            foodItems[index] = food;

                            energy = food.energy;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        void ProcessRespawns()
        {
            for (int i = 0; i < activeFoodCount; i++)
            {
                if (foodItems[i].consumed)
                {
                    var food = foodItems[i];
                    food.respawnTimer -= Time.deltaTime;
                    if (food.respawnTimer <= 0f)
                    {
                        food.consumed = false;
                        food.respawnTimer = 0f;
                        food.position = GetRandomPositionInCell(
                            new Vector2(
                                worldBounds.min.x + (food.gridX + 0.5f) * cellSize.x,
                                worldBounds.min.y + (food.gridY + 0.5f) * cellSize.y
                            ),
                            cellSize
                        );
                    }
                    // Fix: Assign the modified struct back to the array
                    foodItems[i] = food;
                }
            }
        }

        void UpdateSpatialGrid()
        {
            for (int i = 0; i < spatialGrid.Length; i++)
                spatialGrid[i].Clear();

            for (int i = 0; i < activeFoodCount; i++)
            {
                if (!foodItems[i].consumed)
                {
                    int cellIndex = foodItems[i].gridY * gridResolution + foodItems[i].gridX;
                    if (cellIndex >= 0 && cellIndex < spatialGrid.Length)
                        spatialGrid[cellIndex].Add(i);
                }
            }
        }

        void RenderFood()
        {
            int renderCount = 0;
            for (int i = 0; i < activeFoodCount; i++)
            {
                if (!foodItems[i].consumed)
                {
                    renderMatrices[renderCount] = Matrix4x4.TRS(
                        foodItems[i].position,
                        Quaternion.identity,
                        Vector3.one * foodSize
                    );
                    renderCount++;
                }
            }
            Debug.Log($"Rendering {renderCount} food items");

            if (renderCount > 0)
            {
                Graphics.DrawMeshInstanced(foodMesh, 0, foodMaterial, renderMatrices, renderCount);
                Debug.Log($"Drawn {renderCount} food items with color {foodColor}");
            }
        }

        void OnDestroy()
        {
            if (foodMesh != null)
            {
                DestroyImmediate(foodMesh);
            }
            if (foodMaterial != null)
            {
                DestroyImmediate(foodMaterial);
            }
        }
    }
}

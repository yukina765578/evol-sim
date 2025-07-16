using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace EvolutionSimulator.Environment
{
    public class FoodManager : MonoBehaviour
    {
        [Header("Food Settings")]
        [SerializeField]
        private float energyValue = 10f;

        [SerializeField]
        private float respawnDelay = 5f;

        [SerializeField]
        private int maxFoodPerCell = 3;

        [SerializeField]
        private float foodSize = 0.5f;

        [Header("Rendering")]
        [SerializeField]
        private Material foodMaterial;

        [SerializeField]
        private Mesh foodMesh;

        [Header("Collision Detection")]
        [SerializeField]
        private int gridResolution = 50;

        [SerializeField]
        private float detectionRadius = 1f;

        private FoodData[] foodItems;
        private int maxFoodCount;
        private int activeFoodCount;
        private List<int>[] spatialGrid;
        private Vector2 cellSize;
        private Bounds worldBounds;

        private GraphicsBuffer positionBuffer;
        private Vector3[] positionArray; // Fixed-size array
        private int visibleFoodCount = 0; // Track actual visible food count

        private NoiseManager noiseManager;
        private NoiseSettings noiseSettings;

        public int ActiveFoodCount => visibleFoodCount;
        public float FoodEnergy => energyValue;

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
            worldBounds = noiseManager.WorldBounds;
            cellSize = new Vector2(
                worldBounds.size.x / gridResolution,
                worldBounds.size.y / gridResolution
            );
            CreateFoodMesh();

            // Create fixed-size buffer and array
            positionArray = new Vector3[maxFoodCount];
            positionBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                maxFoodCount,
                Marshal.SizeOf<Vector3>()
            );

            InitialSpawn();
            UpdatePositionBuffer();
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
            spatialGrid = new List<int>[gridResolution * gridResolution];

            for (int i = 0; i < spatialGrid.Length; i++)
                spatialGrid[i] = new List<int>();
        }

        void CreateFoodMesh()
        {
            if (foodMesh != null)
                return;

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
                            var food = foodItems[index];
                            food.consumed = true;
                            food.respawnTimer = respawnDelay;
                            foodItems[index] = food;

                            energy = food.energy;
                            UpdatePositionBuffer(); // Update immediately when consumed
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        void ProcessRespawns()
        {
            bool needsUpdate = false;
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
                        needsUpdate = true;
                    }
                    foodItems[i] = food;
                }
            }
            if (needsUpdate)
                UpdatePositionBuffer();
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

        void UpdatePositionBuffer()
        {
            visibleFoodCount = 0;

            // Fill fixed-size array with visible food positions
            for (int i = 0; i < activeFoodCount; i++)
            {
                if (!foodItems[i].consumed)
                {
                    positionArray[visibleFoodCount] = new Vector3(
                        foodItems[i].position.x,
                        foodItems[i].position.y,
                        0
                    );
                    visibleFoodCount++;
                }
            }

            // Update buffer data only - never recreate
            if (positionBuffer != null && visibleFoodCount > 0)
            {
                positionBuffer.SetData(positionArray);
            }
        }

        void RenderFood()
        {
            if (
                foodMaterial == null
                || foodMesh == null
                || positionBuffer == null
                || visibleFoodCount == 0
            )
                return;

            foodMaterial.SetBuffer("_Positions", positionBuffer);
            foodMaterial.SetFloat("_FoodSize", foodSize);

            Graphics.DrawMeshInstancedProcedural(
                foodMesh,
                0,
                foodMaterial,
                new Bounds(Vector3.zero, worldBounds.size),
                visibleFoodCount
            );
        }

        void OnDestroy()
        {
            positionBuffer?.Dispose();
        }
    }

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
}

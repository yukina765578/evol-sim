using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace EvolutionSimulator.Environment
{
    public class PerlinNoiseVisualizer : MonoBehaviour
    {
        [Header("Visualization")]
        [SerializeField]
        private int textureResolution = 256;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugValues = false;

        [SerializeField]
        private Color debugTextColor = Color.white;

        [SerializeField]
        private int debugFontSize = 12;

        private Texture2D noiseTexture;
        private SpriteRenderer spriteRenderer;
        private NoiseManager noiseManager;
        private NoiseSettings noiseSettings;

        // UI Debug elements
        private Canvas debugCanvas;
        private List<Text> debugTexts = new List<Text>();
        private List<Vector2> gridWorldPositions = new List<Vector2>();

        private UnityAction generateGridTextureAction;
        private UnityAction updateDebugTextsAction;

        void Start()
        {
            noiseManager = GetComponent<NoiseManager>();
            if (noiseManager == null)
            {
                Debug.LogError("PerlinNoiseVisualizer requires NoiseManager component!");
                return;
            }
            noiseSettings = noiseManager.Settings;

            SetupRenderer();
            SetupDebugCanvas();
            GenerateGridTexture();
            if (showDebugValues)
            {
                UpdateDebugTexts();
            }

            generateGridTextureAction = GenerateGridTexture;
            updateDebugTextsAction = UpdateDebugTexts;

            // Subscribe to noise updates for synchronization
            noiseManager.OnNoiseUpdated.AddListener(generateGridTextureAction);
            noiseManager.OnNoiseUpdated.AddListener(updateDebugTextsAction);
            EventDebugger.NoiseUpdateListeners += 2;
        }

        void Update()
        {
            // Update debug text positions when camera moves
            if (showDebugValues && debugCanvas != null)
            {
                UpdateDebugTextPositions();
            }
        }

        void OnDestroy()
        {
            if (noiseManager != null)
            {
                if (generateGridTextureAction != null)
                {
                    noiseManager.OnNoiseUpdated.RemoveListener(generateGridTextureAction);
                    EventDebugger.NoiseUpdateListeners--;
                }
                if (updateDebugTextsAction != null)
                {
                    noiseManager.OnNoiseUpdated.RemoveListener(updateDebugTextsAction);
                    EventDebugger.NoiseUpdateListeners--;
                }
            }

            if (noiseTexture != null)
            {
                Destroy(noiseTexture);
                noiseTexture = null;
            }
        }

        void SetupRenderer()
        {
            GameObject visualizer = new GameObject("NoiseVisualizer");
            visualizer.transform.parent = transform;
            visualizer.transform.localPosition = Vector3.zero;

            spriteRenderer = visualizer.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = -10;
        }

        void SetupDebugCanvas()
        {
            // Create Canvas
            GameObject canvasObj = new GameObject("DebugCanvas");
            canvasObj.transform.parent = transform;

            debugCanvas = canvasObj.AddComponent<Canvas>();
            debugCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            debugCanvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Create debug text elements
            CreateDebugTexts();
        }

        void CreateDebugTexts()
        {
            if (noiseManager == null || noiseSettings == null)
                return;

            Bounds worldBounds = noiseManager.WorldBounds;

            Vector2 cellSize = new Vector2(
                worldBounds.size.x / noiseSettings.gridWidth,
                worldBounds.size.y / noiseSettings.gridHeight
            );

            gridWorldPositions.Clear();

            foreach (Text text in debugTexts)
            {
                if (text != null)
                    Destroy(text.gameObject);
            }
            debugTexts.Clear();

            for (int x = 0; x < noiseSettings.gridWidth; x++)
            {
                for (int y = 0; y < noiseSettings.gridHeight; y++)
                {
                    // Calculate world position at grid cell center
                    Vector2 cellCenter = new Vector2(
                        worldBounds.min.x + (x + 0.5f) * cellSize.x,
                        worldBounds.min.y + (y + 0.5f) * cellSize.y
                    );
                    gridWorldPositions.Add(cellCenter);

                    // Create UI Text
                    GameObject textObj = new GameObject($"DebugText_{x}_{y}");
                    textObj.transform.SetParent(debugCanvas.transform, false);

                    Text uiText = textObj.AddComponent<Text>();
                    uiText.text = "0.00";
                    uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                    uiText.fontSize = debugFontSize;
                    uiText.color = debugTextColor;
                    uiText.alignment = TextAnchor.MiddleCenter;

                    // Set RectTransform
                    RectTransform rectTransform = textObj.GetComponent<RectTransform>();
                    rectTransform.sizeDelta = new Vector2(50, 20);

                    debugTexts.Add(uiText);
                    textObj.SetActive(showDebugValues);
                }
            }
        }

        void UpdateDebugTextPositions()
        {
            if (Camera.main == null)
                return;

            for (int i = 0; i < gridWorldPositions.Count && i < debugTexts.Count; i++)
            {
                if (debugTexts[i] == null)
                    continue;

                Vector3 worldPos = new Vector3(gridWorldPositions[i].x, gridWorldPositions[i].y, 0);
                Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);

                // Check if position is on screen
                if (screenPos.z > 0)
                {
                    debugTexts[i].transform.position = screenPos;
                    debugTexts[i].gameObject.SetActive(showDebugValues);
                }
                else
                {
                    debugTexts[i].gameObject.SetActive(false);
                }
            }
        }

        void UpdateDebugTexts()
        {
            if (!showDebugValues || noiseSettings == null)
                return;

            for (int i = 0; i < gridWorldPositions.Count && i < debugTexts.Count; i++)
            {
                if (debugTexts[i] == null)
                    continue;

                Vector2 cellCenter = gridWorldPositions[i];

                float rawValue = PerlinNoise.SampleRaw(
                    cellCenter.x,
                    cellCenter.y,
                    noiseSettings.scale,
                    noiseSettings.offset,
                    noiseSettings.contrast
                );

                debugTexts[i].text = rawValue.ToString("F2");
            }

            UpdateDebugTextPositions();
        }

        void GenerateGridTexture()
        {
            if (noiseManager == null || noiseSettings == null)
                return;

            if (noiseTexture == null)
            {
                noiseTexture = new Texture2D(textureResolution, textureResolution);
                noiseTexture.filterMode = FilterMode.Point;
            }

            Bounds worldBounds = noiseManager.WorldBounds;

            // Sample grid data using NoiseManager's settings and time
            GridData gridData = PerlinNoise.SampleGrid(
                worldBounds,
                noiseSettings.gridWidth,
                noiseSettings.gridHeight,
                noiseSettings.scale,
                noiseSettings.offset,
                noiseSettings.threshold,
                noiseSettings.animate ? noiseManager.CurrentTimeOffset : 0f,
                noiseSettings.contrast
            );

            // Fill texture based on grid
            for (int gridX = 0; gridX < noiseSettings.gridWidth; gridX++)
            {
                for (int gridY = 0; gridY < noiseSettings.gridHeight; gridY++)
                {
                    float noiseValue = gridData.GetValue(gridX, gridY);
                    Color cellColor = Color.Lerp(
                        noiseSettings.lowDensityColor,
                        noiseSettings.highDensityColor,
                        noiseValue
                    );

                    // Calculate pixel boundaries for this grid cell
                    int startX = Mathf.RoundToInt(
                        (float)gridX * textureResolution / noiseSettings.gridWidth
                    );
                    int endX = Mathf.RoundToInt(
                        (float)(gridX + 1) * textureResolution / noiseSettings.gridWidth
                    );
                    int startY = Mathf.RoundToInt(
                        (float)gridY * textureResolution / noiseSettings.gridHeight
                    );
                    int endY = Mathf.RoundToInt(
                        (float)(gridY + 1) * textureResolution / noiseSettings.gridHeight
                    );

                    // Fill texture region
                    for (int x = startX; x < endX; x++)
                    {
                        for (int y = startY; y < endY; y++)
                        {
                            noiseTexture.SetPixel(x, y, cellColor);
                        }
                    }
                }
            }

            noiseTexture.Apply();
            UpdateSprite();
        }

        void UpdateSprite()
        {
            if (noiseManager == null || spriteRenderer == null || noiseTexture == null)
                return;

            Vector2 worldSize = noiseManager.WorldBounds.size;
            Sprite sprite = Sprite.Create(
                noiseTexture,
                new Rect(0, 0, textureResolution, textureResolution),
                new Vector2(0.5f, 0.5f),
                textureResolution / worldSize.x
            );

            spriteRenderer.sprite = sprite;
        }

        public float GetNoiseValueAtPosition(Vector3 position)
        {
            if (noiseManager == null || noiseSettings == null)
                return 0f;

            return PerlinNoise.SampleRaw(
                position.x,
                position.y,
                noiseSettings.scale,
                noiseSettings.offset,
                noiseSettings.contrast
            );
        }

        void OnValidate()
        {
            textureResolution = Mathf.Clamp(textureResolution, 64, 512);
            debugFontSize = Mathf.Clamp(debugFontSize, 8, 24);

            if (Application.isPlaying && debugCanvas != null)
            {
                debugCanvas.gameObject.SetActive(showDebugValues);

                foreach (Text text in debugTexts)
                {
                    if (text != null)
                    {
                        text.color = debugTextColor;
                        text.fontSize = debugFontSize;
                        text.gameObject.SetActive(showDebugValues);
                    }
                }

                if (noiseManager != null)
                {
                    GenerateGridTexture();
                    UpdateDebugTexts();
                }
            }
        }
    }
}

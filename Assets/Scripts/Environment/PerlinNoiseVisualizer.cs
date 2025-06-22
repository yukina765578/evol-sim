using UnityEngine;

namespace EvolutionSimulator.Environment
{
    public class PerlinNoiseVisualizer : MonoBehaviour
    {
        [Header("Noise Settings")]
        [SerializeField]
        private NoiseSettings noiseSettings = new NoiseSettings();

        [Header("Visualization")]
        [SerializeField]
        private int textureResolution = 256;

        [Header("Randomization")]
        [SerializeField]
        private bool randomizeOnStart = true;

        private Texture2D noiseTexture;
        private SpriteRenderer spriteRenderer;
        private Boundaries boundaries;
        private float timeOffset = 0f;

        void Start()
        {
            boundaries = GetComponent<Boundaries>();
            if (boundaries == null)
            {
                Debug.LogError("PerlinNoiseVisualizer requires Boundaries component!");
                return;
            }

            SetupRenderer();

            if (randomizeOnStart)
                RandomizeNoise();
            else
                GenerateGridTexture();
        }

        void SetupRenderer()
        {
            GameObject visualizer = new GameObject("NoiseVisualizer");
            visualizer.transform.parent = transform;
            visualizer.transform.localPosition = Vector3.zero;

            spriteRenderer = visualizer.AddComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = -10;
        }

        void Update()
        {
            if (noiseSettings.animate)
            {
                timeOffset += noiseSettings.animationSpeed * Time.deltaTime;
                GenerateGridTexture();
            }
        }

        void GenerateGridTexture()
        {
            if (noiseTexture == null)
            {
                noiseTexture = new Texture2D(textureResolution, textureResolution);
                noiseTexture.filterMode = FilterMode.Point; // Sharp grid edges
            }

            // Sample grid data
            GridData gridData = PerlinNoise.SampleGrid(
                boundaries.WorldBounds,
                noiseSettings.gridWidth,
                noiseSettings.gridHeight,
                noiseSettings.scale,
                noiseSettings.offset,
                noiseSettings.threshold,
                timeOffset,
                noiseSettings.contrast
            );

            // Fill texture based on grid with proper stretching
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

                    // Calculate exact pixel boundaries for this grid cell
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

                    // Fill texture region for this grid cell
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
            Vector2 worldSize = boundaries.WorldSize;
            Sprite sprite = Sprite.Create(
                noiseTexture,
                new Rect(0, 0, textureResolution, textureResolution),
                new Vector2(0.5f, 0.5f),
                textureResolution / worldSize.x
            );

            spriteRenderer.sprite = sprite;
        }

        [ContextMenu("Randomize Noise")]
        public void RandomizeNoise()
        {
            noiseSettings.offset = new Vector2(
                Random.Range(-1000f, 1000f),
                Random.Range(-1000f, 1000f)
            );
            if (Application.isPlaying)
                GenerateGridTexture();
        }

        public float GetNoiseValueAtPosition(Vector3 position)
        {
            return PerlinNoise.SampleThresholded(
                position.x,
                position.y,
                noiseSettings.scale,
                noiseSettings.offset,
                noiseSettings.threshold,
                timeOffset,
                noiseSettings.contrast
            );
        }

        void OnValidate()
        {
            noiseSettings.gridWidth = Mathf.Clamp(noiseSettings.gridWidth, 10, 200);
            noiseSettings.gridHeight = Mathf.Clamp(noiseSettings.gridHeight, 10, 200);
            textureResolution = Mathf.Clamp(textureResolution, 64, 512);

            if (Application.isPlaying && boundaries != null)
                GenerateGridTexture();
        }
    }
}

using UnityEngine;

namespace EvolutionSimulator.Environment
{
    [System.Serializable]
    public class NoiseSettings
    {
        [Header("Noise Parameters")]
        public float scale = 10f;
        public float threshold = 0.5f;
        public Vector2 offset = Vector2.zero;
        public bool animate = false;
        public float animationSpeed = 0.1f;

        [Header("Remapping")]
        [Range(0.1f, 3f)]
        public float contrast = 1f;

        [Header("Grid Settings")]
        public int gridWidth = 50;
        public int gridHeight = 50;

        [Header("Visualization")]
        public Color lowDensityColor = new Color(0, 0, 0, 0);
        public Color highDensityColor = new Color(1, 0, 0, 0.5f);
    }

    public static class PerlinNoise
    {
        public static float SampleRaw(
            float worldX,
            float worldY,
            float scale,
            Vector2 offset,
            float contrast = 1f
        )
        {
            float x = (worldX / scale) + offset.x;
            float y = (worldY / scale) + offset.y;
            float noise = Mathf.PerlinNoise(x, y);

            // Apply contrast boost
            noise = (noise - 0.5f) * contrast + 0.5f;
            return Mathf.Clamp01(noise);
        }

        public static float SampleRawAnimated(
            float worldX,
            float worldY,
            float scale,
            Vector2 offset,
            float timeOffset,
            float contrast = 1f
        )
        {
            float x = (worldX / scale) + offset.x;
            float y = (worldY / scale) + offset.y;

            // Simulate 3D noise by interpolating between offset layers
            float noise1 = Mathf.PerlinNoise(x, y + timeOffset * 0.1f);
            float noise2 = Mathf.PerlinNoise(x + timeOffset * 0.1f, y);

            // Blend based on time for organic evolution
            float blend = (Mathf.Sin(timeOffset * 2f) + 1f) * 0.5f;
            float noise = Mathf.Lerp(noise1, noise2, blend);

            // Apply contrast boost
            noise = (noise - 0.5f) * contrast + 0.5f;
            return Mathf.Clamp01(noise);
        }

        public static float SampleThresholded(
            float worldX,
            float worldY,
            float scale,
            Vector2 offset,
            float threshold,
            float timeOffset = 0f,
            float contrast = 1f
        )
        {
            float noise =
                timeOffset > 0f
                    ? SampleRawAnimated(worldX, worldY, scale, offset, timeOffset, contrast)
                    : SampleRaw(worldX, worldY, scale, offset, contrast);

            return noise > threshold ? (noise - threshold) / (1f - threshold) : 0f;
        }

        public static bool ShouldSpawnAtPosition(
            Vector3 position,
            float scale,
            Vector2 offset,
            float threshold,
            float timeOffset = 0f
        )
        {
            float noise =
                timeOffset > 0f
                    ? SampleRawAnimated(position.x, position.y, scale, offset, timeOffset)
                    : SampleRaw(position.x, position.y, scale, offset);

            return noise > threshold;
        }

        public static GridData SampleGrid(
            Bounds bounds,
            int gridWidth,
            int gridHeight,
            float scale,
            Vector2 offset,
            float threshold,
            float timeOffset = 0f,
            float contrast = 1f
        )
        {
            var gridData = new GridData(gridWidth, gridHeight);
            Vector2 cellSize = new Vector2(bounds.size.x / gridWidth, bounds.size.y / gridHeight);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2 worldPos = new Vector2(
                        bounds.min.x + (x + 0.5f) * cellSize.x,
                        bounds.min.y + (y + 0.5f) * cellSize.y
                    );

                    float noiseValue = SampleThresholded(
                        worldPos.x,
                        worldPos.y,
                        scale,
                        offset,
                        threshold,
                        timeOffset,
                        contrast
                    );
                    gridData.SetValue(x, y, noiseValue);
                }
            }

            return gridData;
        }

        public static Vector2[] GenerateGridSpawnPoints(
            Bounds bounds,
            int gridWidth,
            int gridHeight,
            float scale,
            Vector2 offset,
            float threshold,
            float timeOffset = 0f
        )
        {
            var spawnPoints = new System.Collections.Generic.List<Vector2>();
            Vector2 cellSize = new Vector2(bounds.size.x / gridWidth, bounds.size.y / gridHeight);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2 worldPos = new Vector2(
                        bounds.min.x + (x + 0.5f) * cellSize.x,
                        bounds.min.y + (y + 0.5f) * cellSize.y
                    );

                    if (ShouldSpawnAtPosition(worldPos, scale, offset, threshold, timeOffset))
                    {
                        spawnPoints.Add(worldPos);
                    }
                }
            }

            return spawnPoints.ToArray();
        }
    }

    [System.Serializable]
    public class GridData
    {
        public int width;
        public int height;
        public float[] values;

        public GridData(int w, int h)
        {
            width = w;
            height = h;
            values = new float[w * h];
        }

        public void SetValue(int x, int y, float value)
        {
            values[y * width + x] = value;
        }

        public float GetValue(int x, int y)
        {
            return values[y * width + x];
        }
    }
}

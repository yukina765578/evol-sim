using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class Node : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private float size = 1f;
        private Color nodeColor = Color.blue;

        void Awake()
        {
            SetupRenderer();
        }

        public void Initialize(float nodeSize, Color color)
        {
            size = nodeSize;
            nodeColor = color;

            UpdateVisuals();
        }

        void SetupRenderer()
        {
            // Get or add SpriteRenderer
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            // Use Unity's built-in circle sprite
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateCircleSprite();
            }

            // Set initial properties
            spriteRenderer.sortingOrder = 1; // Above segments
        }

        void UpdateVisuals()
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = nodeColor;
                transform.localScale = Vector3.one * size;
            }
        }

        Sprite CreateCircleSprite()
        {
            // Create a simple circle texture programmatically
            int resolution = 64;
            Texture2D texture = new Texture2D(resolution, resolution);

            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
            float radius = resolution / 2f - 1f;

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    Vector2 pos = new Vector2(x, y);
                    float distance = Vector2.Distance(pos, center);

                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, Color.white);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();

            return Sprite.Create(
                texture,
                new Rect(0, 0, resolution, resolution),
                new Vector2(0.5f, 0.5f)
            );
        }

        public Vector3 GetPosition()
        {
            return transform.position;
        }

        public float GetSize()
        {
            return size;
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateVisuals();
            }
        }
    }
}

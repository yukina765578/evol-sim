using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class Node : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private float size = 1f;
        private Color nodeColor = Color.blue;
        private Color originalColor = Color.blue;

        private Vector3 prevPosition;

        void Awake()
        {
            SetupRenderer();
        }

        public void Initialize(float nodeSize, Color color)
        {
            size = nodeSize;
            nodeColor = color;
            originalColor = color;
            prevPosition = transform.localPosition;

            UpdateVisuals();
        }

        public void SetColor(Color newColor)
        {
            nodeColor = newColor;
            UpdateVisuals();
        }

        public void RestoreOriginalColor()
        {
            nodeColor = originalColor;
            UpdateVisuals();
        }

        void LateUpdate()
        {
            prevPosition = transform.localPosition;
        }

        void SetupRenderer()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateCircleSprite();
            }

            spriteRenderer.sortingOrder = 1;
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

        public Vector2 GetPositionDelta()
        {
            Vector2 delta = (Vector2)(transform.localPosition - prevPosition);
            return delta;
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

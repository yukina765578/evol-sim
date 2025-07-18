using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class Node : MonoBehaviour
    {
        private SpriteRenderer spriteRenderer;
        private float size = 1f;
        private Color nodeColor = Color.blue;
        private Vector3 prevPosition;

        private Energy energyComponent;

        private Texture2D nodeTexture;

        private static Texture2D sharedNodeTexture;
        private static int textureReferenceCount = 0;

        void Awake()
        {
            SetupRenderer();
        }

        void Update()
        {
            if (energyComponent != null && energyComponent.IsReproductionReady)
            {
                UpdateNodeColor(Color.red);
            }
            else
            {
                UpdateNodeColor(Color.blue); // Default color
            }
        }

        public void Initialize(float nodeSize, Color color)
        {
            size = nodeSize;
            nodeColor = color;
            prevPosition = transform.localPosition;
            energyComponent = GetComponentInParent<Energy>();

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
            {
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
                spriteRenderer.sprite = CreateNodeSprite();
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

        void UpdateNodeColor(Color color)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
            }
        }

        Sprite CreateNodeSprite()
        {
            if (sharedNodeTexture == null)
            {
                // Create circle texture for node
                int resolution = 64;
                sharedNodeTexture = new Texture2D(resolution, resolution);

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
                            sharedNodeTexture.SetPixel(x, y, Color.white);
                        }
                        else
                        {
                            sharedNodeTexture.SetPixel(x, y, Color.clear);
                        }
                    }
                }

                sharedNodeTexture.Apply();
            }

            textureReferenceCount++;

            return Sprite.Create(
                sharedNodeTexture,
                new Rect(0, 0, sharedNodeTexture.width, sharedNodeTexture.height),
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

        void OnDestroy()
        {
            textureReferenceCount--;
            if (textureReferenceCount <= 0 && sharedNodeTexture != null)
            {
                Destroy(sharedNodeTexture);
                sharedNodeTexture = null;
                textureReferenceCount = 0;
            }
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

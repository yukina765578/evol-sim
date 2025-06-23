using UnityEngine;

namespace EvolutionSimulator.Creatures
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
            // Use Unity's built-in circle sprite from UI resources
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
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

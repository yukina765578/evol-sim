using UnityEngine;

namespace EvolutionSimulator.Environment
{
    public class FoodItem : MonoBehaviour
    {
        [Header("Food Properties")]
        [SerializeField]
        private float energyValue = 10f;

        [SerializeField]
        private Color foodColor = Color.green;

        [SerializeField]
        private float foodSize = 0.5f;

        [Header("Collision Detection")]
        [SerializeField]
        private bool enableAutoConsumption = false; // Disabled by default now

        private FoodSpawner parentSpawner;
        private SpriteRenderer spriteRenderer;
        private CircleCollider2D foodCollider;
        private bool isConsumed = false;

        public float EnergyValue => energyValue;
        public bool IsConsumed => isConsumed;

        void Awake()
        {
            SetupVisuals();
            SetupCollider();
        }

        void SetupVisuals()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer == null)
                spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

            // Create simple circle sprite if none assigned
            if (spriteRenderer.sprite == null)
            {
                spriteRenderer.sprite = CreateCircleSprite();
            }

            spriteRenderer.color = foodColor;
            transform.localScale = Vector3.one * foodSize;
        }

        void SetupCollider()
        {
            foodCollider = GetComponent<CircleCollider2D>();
            if (foodCollider == null)
                foodCollider = gameObject.AddComponent<CircleCollider2D>();

            foodCollider.isTrigger = true;
            foodCollider.radius = 0.5f; // Matches sprite size
        }

        Sprite CreateCircleSprite()
        {
            // Use Unity's built-in circle sprite
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        }

        public void Initialize(float energy, FoodSpawner spawner)
        {
            energyValue = energy;
            parentSpawner = spawner;
            isConsumed = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Only auto-consume if enabled (disabled by default now)
            if (!enableAutoConsumption || isConsumed)
                return;

            // Check if the colliding object can consume food
            if (other.CompareTag("Creature") || other.name.Contains("Motor"))
            {
                ConsumeFood(other.gameObject);
            }
        }

        public void ConsumeFood(GameObject consumer)
        {
            if (isConsumed)
                return;

            isConsumed = true;

            if (parentSpawner != null)
            {
                parentSpawner.OnFoodConsumed(this, transform.position);
            }
            Destroy(gameObject);
        }

        void OnValidate()
        {
            foodSize = Mathf.Clamp(foodSize, 0.1f, 2f);

            if (Application.isPlaying && spriteRenderer != null)
            {
                spriteRenderer.color = foodColor;
                transform.localScale = Vector3.one * foodSize;
            }
        }
    }
}

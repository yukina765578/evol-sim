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
        private Rigidbody2D foodRigidbody;
        private bool isConsumed = false;

        public float EnergyValue => energyValue;
        public bool IsConsumed => isConsumed;

        void Awake()
        {
            SetupLayer();
            SetupRigidbody();
            SetupVisuals();
            SetupCollider();
        }

        void SetupLayer()
        {
            int foodLayer = LayerMask.NameToLayer("Food");
            if (foodLayer == -1)
            {
                Debug.LogError(
                    "Food layer not found! Please create a 'Food' layer in the project settings."
                );
            }
            else
            {
                gameObject.layer = foodLayer;
            }
        }

        void SetupRigidbody()
        {
            foodRigidbody = GetComponent<Rigidbody2D>();
            if (foodRigidbody == null)
            {
                foodRigidbody = gameObject.AddComponent<Rigidbody2D>();
            }
            foodRigidbody.bodyType = RigidbodyType2D.Kinematic;
            foodRigidbody.gravityScale = 0f;
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
            return Resources.GetBuiltinResource<Sprite>("UI/Skin/Knob.psd");
        }

        public void Initialize(float energy, FoodSpawner spawner)
        {
            energyValue = energy;
            parentSpawner = spawner;
            isConsumed = false;
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

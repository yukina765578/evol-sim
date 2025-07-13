using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class CollisionDetector : MonoBehaviour
    {
        // Collision detection settings
        private float foodDetectionRadius = 1f;
        private LayerMask foodLayerMask = -1;

        private float creatureDetectionRadius = 3f;
        private LayerMask creatureLayerMask = -1;

        private float SEGMENT_LENGTH = 2f;

        private Energy energy;
        private CircleCollider2D foodDetector;
        private CircleCollider2D creatureDetector;
        private CreatureGenome genome;

        void Awake()
        {
            energy = GetComponentInParent<Energy>();
            if (energy == null)
            {
                Debug.LogError("CollisionDetector requires Energy component in parent!");
            }
            genome = GetComponentInParent<CreatureGenome>();
            if (genome == null)
            {
                Debug.LogError("CollisionDetector requires CreatureGenome component in parent!");
            }
            SetupCollider();
            energy.OnReproductionReadyChanged += HandleReproductionReadyChanged;
        }

        void SetupCollider()
        {
            foodDetector = GetComponent<CircleCollider2D>();
            if (foodDetector == null)
                foodDetector = gameObject.AddComponent<CircleCollider2D>();
            foodDetector.isTrigger = true;
            foodDetector.radius = foodDetectionRadius;

            creatureDetector = GetComponent<CircleCollider2D>();
            if (creatureDetector == null)
                creatureDetector = gameObject.AddComponent<CircleCollider2D>();
            creatureDetector.isTrigger = false;

            creatureDetectionRadius = CalculateCreatureDetectionRadii();
            creatureDetector.radius = creatureDetectionRadius;
        }

        float CalculateCreatureDetectionRadii()
        {
            if (genome == null)
                genome = GetComponentInParent<CreatureGenome>();
            if (genome == null)
            {
                Debug.LogError("CollisionDetector requires CreatureGenome component in parent!");
                return 0f;
            }
            float maxParentIndex = genome.GetMaxParentIndex();
            float radius = maxParentIndex * SEGMENT_LENGTH;
            return radius;
        }

        void HandleReproductionReadyChanged(bool isReady)
        {
            creatureDetector.enabled = isReady;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!energy.IsAlive)
                return;

            FoodItem foodItem = other.GetComponent<FoodItem>();
            if (foodItem != null && !foodItem.IsConsumed)
            {
                ConsumeFood(foodItem);
            }
        }

        void OnCreatureEnterRange(GameObject creature)
        {
            if (!energy.IsAlive)
                return;
            OnCreatureDetected?.Invoke(creature);
        }

        void ConsumeFood(FoodItem foodItem)
        {
            if (foodItem.IsConsumed)
                return;
            float energyGain = foodItem.EnergyValue;
            energy.AddEnergy(energyGain);

            foodItem.ConsumeFood(gameObject);
        }

        void OnValidate()
        {
            foodDetectionRadius = Mathf.Max(0.1f, foodDetectionRadius);
            creatureDetectionRadius = Mathf.Max(0.1f, creatureDetectionRadius);
            if (Application.isPlaying && detector != null)
            {
                foodDetector.radius = foodDetectionRadius;
                creatureDetector.radius = creatureDetectionRadius;
            }
        }
    }
}

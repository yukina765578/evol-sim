using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Detectors
{
    public class FoodDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField]
        private float detectionRadius = 1f;

        [SerializeField]
        private LayerMask foodLayerMask = -1;

        [SerializeField]
        private Color debugColor = Color.green;

        private Energy creatureEnergy;
        private CircleCollider2D detector;

        void Awake()
        {
            SetupCollider();
            creatureEnergy = GetComponentInParent<Energy>();

            if (creatureEnergy == null)
            {
                Debug.LogError("FoodDetector requires CreatureEnergy component in parent!");
            }
            foodLayerMask = 1 << LayerMask.NameToLayer("Food");
        }

        void SetupCollider()
        {
            var colliders = GetComponents<CircleCollider2D>();
            detector = null;

            foreach (var col in colliders)
            {
                if (col.name == "FoodDetector")
                {
                    detector = col as CircleCollider2D;
                    break;
                }
            }
            if (detector == null)
            {
                detector = gameObject.AddComponent<CircleCollider2D>();
                detector.name = "FoodDetector";
            }

            detector.isTrigger = true;
            detector.radius = detectionRadius;
            detector.enabled = true;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!creatureEnergy.IsAlive)
                return;

            FoodItem foodItem = other.GetComponent<FoodItem>();
            if (foodItem != null && !foodItem.IsConsumed)
            {
                ConsumeFood(foodItem);
            }
        }

        void ConsumeFood(FoodItem foodItem)
        {
            if (foodItem.IsConsumed)
                return;

            float energyGained = foodItem.EnergyValue;
            creatureEnergy.AddEnergy(energyGained);

            foodItem.ConsumeFood(gameObject);
        }

        void OnValidate()
        {
            detectionRadius = Mathf.Max(0.1f, detectionRadius);

            if (Application.isPlaying && detector != null)
            {
                detector.radius = detectionRadius;
            }
        }
    }
}

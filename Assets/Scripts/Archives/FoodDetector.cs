using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class FoodDetector : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField]
        private float detectionRadius = 1f;

        [SerializeField]
        private LayerMask foodLayerMask = -1;

        [Header("Debug")]
        [SerializeField]
        private bool showDetectionRadius = false;

        [SerializeField]
        private Color debugColor = Color.green;

        private CreatureEnergy creatureEnergy;
        private CircleCollider2D detector;

        void Awake()
        {
            SetupCollider();
            creatureEnergy = GetComponentInParent<CreatureEnergy>();

            if (creatureEnergy == null)
            {
                Debug.LogError("FoodDetector requires CreatureEnergy component in parent!");
            }

            foodLayerMask = 1 << LayerMask.NameToLayer("Food");
        }

        void SetupCollider()
        {
            detector = GetComponent<CircleCollider2D>();
            if (detector == null)
                detector = gameObject.AddComponent<CircleCollider2D>();

            detector.isTrigger = true;
            detector.radius = detectionRadius;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            Debug.Log($"FoodDetector triggered by: {other.name}");
            if (!creatureEnergy.IsAlive)
                return;

            FoodItem foodItem = other.GetComponent<FoodItem>();
            if (foodItem != null && !foodItem.IsConsumed)
            {
                // Check if this food hasn't been consumed yet
                // Check if energy is not already full
                if (creatureEnergy.CurrentEnergy > creatureEnergy.MaxEnergy - foodItem.EnergyValue)
                    return;
                ConsumeFood(foodItem);
            }
        }

        void ConsumeFood(FoodItem foodItem)
        {
            // Double-check to prevent double consumption
            if (foodItem.IsConsumed)
                return;

            float energyGained = foodItem.EnergyValue;
            creatureEnergy.AddEnergy(energyGained);

            // Trigger the FoodItem's existing consumption system
            // This will handle destruction + notify spawner for respawn
            foodItem.ConsumeFood(gameObject);
        }

        void OnDrawGizmos()
        {
            if (!showDetectionRadius)
                return;

            Gizmos.color = debugColor;

            // Draw wire circle using multiple line segments
            Vector3 center = transform.position;
            int segments = 32;
            float angleStep = 360f / segments;

            for (int i = 0; i < segments; i++)
            {
                float angle1 = i * angleStep * Mathf.Deg2Rad;
                float angle2 = (i + 1) * angleStep * Mathf.Deg2Rad;

                Vector3 point1 =
                    center
                    + new Vector3(
                        Mathf.Cos(angle1) * detectionRadius,
                        Mathf.Sin(angle1) * detectionRadius,
                        0
                    );
                Vector3 point2 =
                    center
                    + new Vector3(
                        Mathf.Cos(angle2) * detectionRadius,
                        Mathf.Sin(angle2) * detectionRadius,
                        0
                    );

                Gizmos.DrawLine(point1, point2);
            }
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

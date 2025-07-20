using EvolutionSimulator.Environment;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Sensors
{
    public class FoodSensor : MonoBehaviour
    {
        [Header("Sensor Settings")]
        [SerializeField]
        private float sightRange = 15f;

        [SerializeField]
        private float sightAngle = 120f; // Field of view in degrees (total cone)

        private FoodManager foodManager;

        // Neural network outputs
        public float FoodPresence { get; private set; } // 0 or 1
        public float FoodDirection { get; private set; } // 0 to 2π
        public float FoodDistance { get; private set; } // 0 to 1 (normalized)

        void Start()
        {
            foodManager = FindFirstObjectByType<FoodManager>();
            if (foodManager == null)
            {
                Debug.LogError("FoodSensor requires FoodManager in scene!");
            }
        }

        void Update()
        {
            DetectFood();
        }

        void DetectFood()
        {
            if (foodManager == null)
            {
                SetNoFoodDetected();
                return;
            }

            // Get nearest food within sight cone and range
            Vector3 nearestVisibleFood = foodManager.GetNearestFoodInCone(
                transform.position,
                transform.up, // facing direction
                sightRange,
                sightAngle
            );

            if (nearestVisibleFood == Vector3.zero) // No food in sight
            {
                SetNoFoodDetected();
                return;
            }

            // Calculate outputs for the visible food
            Vector3 directionToFood = nearestVisibleFood - transform.position;
            FoodPresence = 1f;
            FoodDirection = Mathf.Atan2(directionToFood.y, directionToFood.x);
            if (FoodDirection < 0)
                FoodDirection += 2f * Mathf.PI;
            FoodDistance = directionToFood.magnitude / sightRange;
        }

        void SetNoFoodDetected()
        {
            FoodPresence = 0f;
            FoodDirection = 0f;
            FoodDistance = 1f; // Max distance when no food
        }

        void OnValidate()
        {
            sightRange = Mathf.Max(0.1f, sightRange);
            sightAngle = Mathf.Clamp(sightAngle, 10f, 360f);
        }
    }
}

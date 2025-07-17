using EvolutionSimulator.Environment;
using UnityEngine;
using UnityEngine.Events;

namespace EvolutionSimulator.Creatures.Core
{
    public class PhysicsSystem : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField]
        private float foodDetectionRadius = 5f;

        [SerializeField]
        private float reproductionDetectionRadius = 10f;

        private CreatureState creatureState;
        private Energy energy;
        private FoodManager foodManager;
        private CircleCollider2D reproductionDetector;

        public UnityEvent<Controller> OnCreatureDetected = new UnityEvent<Controller>();

        private UnityAction reproductionChangedAction;

        public void Initialize(CreatureState state)
        {
            creatureState = state;
            energy = GetComponent<Energy>();
            foodManager = FindFirstObjectByType<FoodManager>();

            SetupReproductionDetection();
        }

        void SetupReproductionDetection()
        {
            // Only create reproduction detection collider
            reproductionDetector = gameObject.AddComponent<CircleCollider2D>();
            reproductionDetector.radius = reproductionDetectionRadius;
            reproductionDetector.isTrigger = true;
            reproductionDetector.enabled = false; // Enabled only when ready to reproduce

            if (energy != null)
            {
                reproductionChangedAction = HandleReproductionChanged;
                energy.OnReproductionReadyChanged.AddListener(reproductionChangedAction);
                EventDebugger.ReproductionListeners++;
            }
        }

        void Update()
        {
            if (!creatureState.isInitialized)
                return;

            HandleFoodDetection();
        }

        void HandleFoodDetection()
        {
            if (energy == null || foodManager == null)
                return;

            if (energy.IsAlive && energy.CurrentEnergy < energy.MaxEnergy - foodManager.FoodEnergy)
            {
                Vector2 position = transform.position;
                if (
                    foodManager.TryConsumeFood(
                        position,
                        foodDetectionRadius,
                        out float energyGained
                    )
                )
                {
                    energy.AddEnergy(energyGained);
                }
            }
        }

        void HandleReproductionChanged()
        {
            if (energy == null)
                return;

            bool isReproductionReady = energy.IsReproductionReady;
            if (reproductionDetector != null)
                reproductionDetector.enabled = isReproductionReady;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            // Only handle reproduction detection
            if (energy == null || !energy.IsAlive || !energy.IsReproductionReady)
                return;
            if (other.transform.root == transform.root) // Don't detect self
                return;

            var detectedCreature = other.GetComponentInParent<Controller>();
            if (detectedCreature == null)
                return;

            Energy partnerEnergy = detectedCreature.GetComponent<Energy>();
            if (partnerEnergy != null && partnerEnergy.IsAlive && partnerEnergy.IsReproductionReady)
                OnCreatureDetected?.Invoke(detectedCreature);
        }

        public void UpdateCreatureState(CreatureState newState)
        {
            creatureState = newState;
        }

        void OnDestroy()
        {
            if (energy != null && reproductionChangedAction != null)
            {
                energy.OnReproductionReadyChanged.RemoveListener(reproductionChangedAction);
                EventDebugger.ReproductionListeners--;
            }
        }

        void OnValidate()
        {
            foodDetectionRadius = Mathf.Max(0.1f, foodDetectionRadius);
            reproductionDetectionRadius = Mathf.Max(0.1f, reproductionDetectionRadius);
        }
    }
}

using EvolutionSimulator.Creatures.Core;
using UnityEngine;
using UnityEngine.Events;

namespace EvolutionSimulator.Creatures.Detectors
{
    public class CreatureDetector : MonoBehaviour
    {
        private float detectionRadius = 3f;
        private LayerMask creatureLayerMask = -1;

        private Core.Energy creatureEnergy;
        private CircleCollider2D detector;
        private bool isReproductionReady = false;

        public UnityEvent<Controller> OnCreatureDetected = new UnityEvent<Controller>();

        void Awake()
        {
            SetupCollider();
            creatureEnergy = GetComponentInParent<Core.Energy>();

            if (creatureEnergy == null)
            {
                Debug.LogError("CreatureDetector requires Energy component in parent!");
                return;
            }

            creatureEnergy.OnReproductionReadyChanged.AddListener(HandleReproductionChanged);
            creatureLayerMask = 1 << LayerMask.NameToLayer("Creatures");
        }

        void SetupCollider()
        {
            var colliders = GetComponents<CircleCollider2D>();
            detector = null;
            foreach (var col in colliders)
            {
                if (col.name == "CreatureDetector")
                {
                    detector = col;
                    break;
                }
            }
            if (detector == null)
            {
                detector = gameObject.AddComponent<CircleCollider2D>();
                detector.name = "CreatureDetector";
            }
            detector.isTrigger = true;
            detector.radius = detectionRadius;
            detector.enabled = false; // Initially disabled until reproduction is ready
        }

        void HandleReproductionChanged()
        {
            isReproductionReady = creatureEnergy.IsReproductionReady;
            if (detector != null)
                detector.enabled = isReproductionReady;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!creatureEnergy.IsAlive || !isReproductionReady)
                return;

            // Prevent self-detection
            if (other.transform.root == transform.root)
                return;

            // Get Controller component from the detected creature
            var detectedCreature = other.GetComponentInParent<Controller>();
            if (detectedCreature == null)
                return;

            Energy partnerEnergy = detectedCreature.GetComponent<Energy>();
            if (
                partnerEnergy == null
                || !partnerEnergy.IsAlive
                || !partnerEnergy.IsReproductionReady
            )
                return;
            // Check if the detected creature is alive and ready for reproduction
            OnCreatureDetected?.Invoke(detectedCreature);
        }

        void OnValidate()
        {
            detectionRadius = Mathf.Max(0.1f, detectionRadius);

            if (Application.isPlaying && detector != null)
            {
                detector.radius = detectionRadius;
            }
        }

        void OnDestroy()
        {
            if (creatureEnergy != null)
            {
                creatureEnergy.OnReproductionReadyChanged.RemoveListener(HandleReproductionChanged);
            }
        }
    }
}

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

        public UnityEvent<GameObject> OnCreatureDetected = new UnityEvent<GameObject>();

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
        }

        void SetupCollider()
        {
            detector = GetComponent<CircleCollider2D>();
            if (detector == null)
                detector = gameObject.AddComponent<CircleCollider2D>();

            detector.isTrigger = true;
            detector.radius = detectionRadius;
            detector.enabled = false; // Start disabled
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

            int otherLayer = 1 << other.gameObject.layer;
            if ((creatureLayerMask & otherLayer) == 0)
                return;

            OnCreatureDetected?.Invoke(other.gameObject);
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

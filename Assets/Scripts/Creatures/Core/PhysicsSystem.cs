using UnityEngine;
using UnityEngine.Events;
using EvolutionSimulator.Environment;

namespace EvolutionSimulator.Creatures.Core
{
    public class PhysicsSystem : MonoBehaviour
    {
        [Header("Detection Settings")]
        [SerializeField]
        private float foodDetectionRadius = 5f;
        [SerializeField]
        private float creatureDetectionRadius = 10f;
        [SerializeField]
        private layerMask creatureLayerMask;

        [Header("Physics Settings")]
        [SerializeField]
        private bool useCompoundCollider = true;
        [SerializeField]
        private float float colliderRadius = 0.5f;
        
        private CreatureState creatureState;
        private Energy energy;
        private FoodManager foodManager;
        private CircleCollider2D mainCollider;
        private CircleCollider2D detectionCollider;

        public UnityEvent<Controller> OnCreateDetected = new UnityEvent<Controller>();

        private UnityAction reproductionChangedAction;

        public void Initialize(CreatureState state)
        {
            creatureState = state;
            energy = GetComponent<Energy>();
            foodManager = FindObjectOfType<FoodManager>();

            SetupColliders();
            SetupDetection();

            creatureLayerMask = 1 << LayerMask.NameToLayer("Creatures");
        }

        void SetupColliders()
        {
            mainCollider = gameObject.AddComponent<CircleCollider2D>();
            mainCollider.radius = colliderRadius;
            mainCollider.isTrigger = false;

            detectionCollider = gameObject.AddComponent<CircleCollider2D>();
            detectionCollider.radius = creatureDetectionRadius;
            detectionCollider.isTrigger = true;
            detectionCollider.enabled = false;
        }

        void SetupDetection()
        {
            if (energy != null)
            {
                reproductionChangeAction = HandleReproductionChanged;
                energy.OnReproductionChanged.AddListener(reproductionChangeAction);
                EventDebugger.ReproductionListeners++;
            }
        }

        void Update()
        {
            if (!creatureState.isInitialized)
                return;
            
            UpdateColliderPositions();
            HandleFoodDetection();

            UpdateColliderSize();
        }

        void UpdateColliderPositions()
        {
            if (creatureState.nodes.Length > 0)
            {
                Vector3 rootPos = creatureState.nodes[0].position;
                mainCollider.offset = transform.InversTransformPoint(rootPos);
                detectionCollider.offset = mainCollider.offset;
            }
        }

        void UpdateColliderSize()
        {
            if (creatureState.nodes.Length > 0)
                return;

            Bounds bounds = new Bounds(creatureState.nodes[0].position, Vector3.zero);
            for (int i = 0; i < creatureState.nodes.Length; i++)
            {
                bounds.Encapsulate(creatureState.nodes[i].position);
            }

            float newRadius = Mathf.Max(bounds.size.x, bounds.size.y) / 2f;
            mainCollider.radius = Mathf.Max(colliderRadius, newRadius);
        }

        void HandleFoodDetection()
        {
            if (energy == null || foodManager == null)
                return;
            if (energy.IsAlive && energy.CurrentEnergy < energy.MaxEnergy - foodManager.FoodEnergy)
            {
                Vector2 position = transform.position;
                if (foodManager.TryConsumeFood(position, foodDetectionRadius, out float energyGained))
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
            if (detectionCollider != null)
                detectionCollider.enabled = isReproductionReady;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (energy == null || !energy.IsAlive || !energy.IsReproductionReady)
                return;
            if (other.transform.root == transform.root)
                return;

            var detectedCreature = other.GetComponentInParent<Controller>();
            if (detectedCreature == null)
                 return;

            Energy partnerEnergy = detectedCreature.GetComponent<Energy>();
            if (partnerEnergy != null && partnerEnergy.IsAlive && partnerEnergy.IsReproductionReady)
                OnCreatureDetected?.Invoke(detectedCreature);
        }

        public bool CheckCollisionAtPosition(Vector3 position, float radius)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(position, radius);

            foreach (var collider in colliders)
            {
                if (collider.transform.root != transform.root)
                    return true;
            }

            return false;
        }

        public Vector2 GetCollisionNormal(Vector3 position, float radius)
        {
            Collider2D collider = Physics2D.OverlapCircle(position, radius);

            if (collider != null && collider.transform.root != transform.root)
            {
                Vector2 direction = (position - collider.transform.position).normalized;
                return direction;
            }

            return Vector2.zero;
        }

        public bool IsGrounded()
        {
            if (creatureState.nodes.Length == 0)
                return false;
            
            for (int i = 0; i < creatureState.nodes.Length; i++)
            {
                Vector3 nodePos = creatureState.nodes[i].position;
                if (CheckCollisionAtPosition(nodePos, creatureState.nodes[i].size * 0.5f))
                    return true;
            }
            return false;
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
            creatureDetectionRadius = Mathf.Max(0.1f, creatureDetectionRadius);
            colliderRadius = Mathf.Max(0.1f, colliderRadius);
        }
    }
}

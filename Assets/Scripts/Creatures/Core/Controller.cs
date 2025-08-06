using EvolutionSimulator.Creatures.Genetics;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class Controller : MonoBehaviour
    {
        [Header("Debug")]
        [SerializeField]
        private bool showVelocityDebug = false;

        private CreatureGenome genome;
        private bool isDead = false;

        private Rigidbody2D creatureRigidbody;
        private SegmentRenderer segmentRenderer;
        private LineRenderer velocityDebugLine;

        private float startTime;
        private const float STARTUP_DELAY = 5f;
        private bool canApplyThrust = false;
        private const float VELOCITY_FREEZE_DURATION = 0.5f;
        private const float DEBUG_DURATION = 1f;
        private bool isSpawnedCreature = false;

        public bool IsSpawnedCreature => isSpawnedCreature;

        public CreatureGenome GetGenome() => genome;

        public void Initialize(CreatureGenome creatureGenome, SegmentRenderer renderer = null)
        {
            genome = creatureGenome;
            segmentRenderer = renderer;

            if (genome == null)
            {
                Debug.LogError("CreatureController requires a valid CreatureGenome!");
                return;
            }
            startTime = Time.time;
        }

        void Start()
        {
            SetupComponents();
            SetupVelocityDebug();
        }

        void Update()
        {
            if (!canApplyThrust && Time.time - startTime > STARTUP_DELAY)
            {
                canApplyThrust = true;
            }

            // Spawn physics control
            if (isSpawnedCreature)
            {
                float spawnElapsed = Time.time - startTime;

                if (spawnElapsed < VELOCITY_FREEZE_DURATION)
                {
                    creatureRigidbody.linearVelocity = Vector2.zero;
                    creatureRigidbody.angularVelocity = 0f;
                }

                if (spawnElapsed >= DEBUG_DURATION)
                {
                    isSpawnedCreature = false;
                }
            }

            // Note: Segment rotations now handled by SegmentRenderer automatically

            if (canApplyThrust)
            {
                ApplyThrust();
                if (showVelocityDebug)
                    UpdateVelocityDebug();
            }
        }

        public void SetSpawnedCreature(bool isSpawned)
        {
            isSpawnedCreature = isSpawned;
        }

        void SetupComponents()
        {
            creatureRigidbody = GetComponent<Rigidbody2D>();
            if (creatureRigidbody == null)
                creatureRigidbody = gameObject.AddComponent<Rigidbody2D>();

            creatureRigidbody.gravityScale = 0f;

            // Get SegmentRenderer if not provided
            if (segmentRenderer == null)
                segmentRenderer = GetComponent<SegmentRenderer>();
        }

        void SetupVelocityDebug()
        {
            GameObject debugObj = new GameObject("VelocityDebug");
            debugObj.transform.SetParent(transform);

            velocityDebugLine = debugObj.AddComponent<LineRenderer>();
            velocityDebugLine.material = new Material(Shader.Find("Sprites/Default"));
            velocityDebugLine.positionCount = 2;
            velocityDebugLine.useWorldSpace = true;
            velocityDebugLine.sortingOrder = 10;
            velocityDebugLine.startWidth = 0.1f;
            velocityDebugLine.endWidth = 0.1f;
            velocityDebugLine.startColor = Color.red;
            velocityDebugLine.endColor = Color.red;
            velocityDebugLine.enabled = showVelocityDebug;
        }

        void ApplyThrust()
        {
            if (segmentRenderer == null)
                return;

            Vector2 currentVelocity = creatureRigidbody.linearVelocity;

            // Get total thrust and drag from SegmentRenderer
            Vector2 totalThrust = segmentRenderer.GetTotalThrust();

            float maxTotalDrag = currentVelocity.magnitude * 0.1f;
            float maxDragPerSegment =
                genome.NodeCount > 1 ? maxTotalDrag / (genome.NodeCount - 1) : 0f;
            Vector2 totalDrag = segmentRenderer.GetTotalWaterDrag(
                currentVelocity,
                maxDragPerSegment
            );

            creatureRigidbody.AddForce(totalThrust, ForceMode2D.Force);
            if (totalDrag.magnitude > 0f)
            {
                creatureRigidbody.AddForce(totalDrag, ForceMode2D.Force);
            }
        }

        void UpdateVelocityDebug()
        {
            if (velocityDebugLine == null)
            {
                Debug.LogError("VelocityDebugLine not set up correctly.");
                return;
            }

            velocityDebugLine.enabled = showVelocityDebug;

            if (showVelocityDebug)
            {
                Vector2 velocity = creatureRigidbody.linearVelocity;
                Vector3 rootPosition = transform.position;
                Vector3 startPosition = rootPosition + (Vector3)velocity.normalized * 1.2f;
                Vector3 endPosition = startPosition + (Vector3)velocity * 2f;

                velocityDebugLine.SetPosition(0, startPosition);
                velocityDebugLine.SetPosition(1, endPosition);
            }
        }

        public void HandleDeath(string cause)
        {
            if (isDead)
                return;
            isDead = true;
            Destroy(gameObject);
        }
    }
}

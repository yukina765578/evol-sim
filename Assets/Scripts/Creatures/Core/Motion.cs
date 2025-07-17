using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class Motion : MonoBehaviour
    {
        [Header("Motion Settings")]
        [SerializeField]
        private bool enableThrust = true;

        [SerializeField]
        private bool enableDrag = true;

        [SerializeField]
        private float dragMultiplier = 1f;

        private CreatureState creatureState;
        private Rigidbody2D rigidBody;
        private Energy energy;

        // Cached values for performance
        private Vector2 totalThrust;
        private Vector2 totalDrag;
        private Vector2 currentVelocity;

        public void Initialize(CreatureState state)
        {
            creatureState = state;
            rigidBody = GetComponent<Rigidbody2D>();
            energy = GetComponent<Energy>();

            if (rigidBody == null)
            {
                Debug.LogError("Motion component requires Rigidbody2D!");
                return;
            }

            SetupRigidbody();
        }

        void SetupRigidbody()
        {
            rigidBody.gravityScale = 0f;
            rigidBody.drag = 0f;
            rigidBody.angularDrag = 0f;
        }

        void Update()
        {
            if (!creatureState.isInitialized || !enableThrust)
                return;

            UpdateMotion();
        }

        void UpdateMotion()
        {
            // Update segment rotations first
            for (int i = 0; i < creatureState.segments.Length; i++)
            {
                SegmentData segment = creatureState.segments[i];
                Utils.UpdateSegmentRotation(ref segment, creatureState.nodes, Time.deltaTime);
                creatureState.segments[i] = segment;
            }

            // Update node positions based on segment rotations
            UpdateNodePositions();

            // Calculate and apply forces
            CalculateForces();
            ApplyForces();

            // Update previous positions
            creatureState.UpdatePrevPositions();
        }

        void UpdateNodePositions()
        {
            // Root node doesn't move relative to creature
            // Update child nodes based on their parent segments
            for (int i = 0; i < creatureState.segments.Length; i++)
            {
                SegmentData segment = creatureState.segments[i];

                if (
                    segment.parentNodeIndex >= creatureState.nodes.Length
                    || segment.childNodeIndex >= creatureState.nodes.Length
                )
                    continue;

                NodeData parentNode = creatureState.nodes[segment.parentNodeIndex];
                NodeData childNode = creatureState.nodes[segment.childNodeIndex];

                Utils.UpdateNodePosition(ref childNode, parentNode, segment);
                creatureState.nodes[segment.childNodeIndex] = childNode;
            }
        }

        void CalculateForces()
        {
            totalThrust = Vector2.zero;
            totalDrag = Vector2.zero;
            currentVelocity = rigidBody.linearVelocity;

            // Calculate thrust from all segments
            for (int i = 0; i < creatureState.segments.Length; i++)
            {
                SegmentData segment = creatureState.segments[i];

                // Calculate thrust
                Vector2 segmentThrust = Utils.CalculateThrust(segment, creatureState.nodes);
                totalThrust += segmentThrust;

                // Calculate drag if enabled
                if (enableDrag)
                {
                    float maxDragPerSegment =
                        currentVelocity.magnitude * 0.9f / creatureState.segments.Length;
                    Vector2 segmentDrag = Utils.CalculateWaterDrag(
                        segment,
                        creatureState.nodes,
                        currentVelocity,
                        maxDragPerSegment
                    );
                    totalDrag += segmentDrag;
                }

                // Apply energy cost for movement
                if (energy != null)
                {
                    float angleChange = Mathf.Abs(segment.currentAngle - segment.prevAngle);
                    energy.ConsumeMovementEnergy(angleChange);
                }
            }

            // Apply drag multiplier
            totalDrag *= dragMultiplier;
        }

        void ApplyForces()
        {
            // Apply thrust
            if (totalThrust.magnitude > 0.01f)
            {
                rigidBody.AddForce(totalThrust, ForceMode2D.Force);
            }

            // Apply drag
            if (enableDrag && totalDrag.magnitude > 0.01f)
            {
                rigidBody.AddForce(totalDrag, ForceMode2D.Force);
            }
        }

        public Vector2 GetTotalThrust()
        {
            return totalThrust;
        }

        public Vector2 GetTotalDrag()
        {
            return totalDrag;
        }

        public Vector2 GetCurrentVelocity()
        {
            return currentVelocity;
        }

        public void SetThrustEnabled(bool enabled)
        {
            enableThrust = enabled;
        }

        public void SetDragEnabled(bool enabled)
        {
            enableDrag = enabled;
        }

        public void UpdateCreatureState(CreatureState newState)
        {
            creatureState = newState;
        }

        // Calculate movement efficiency for fitness evaluation
        public float GetMovementEfficiency()
        {
            if (currentVelocity.magnitude < 0.01f)
                return 0f;

            float energySpent = 0f;
            for (int i = 0; i < creatureState.segments.Length; i++)
            {
                SegmentData segment = creatureState.segments[i];
                float angleChange = Mathf.Abs(segment.currentAngle - segment.prevAngle);
                energySpent += angleChange * 0.001f; // Energy cost calculation
            }

            return energySpent > 0f ? currentVelocity.magnitude / energySpent : 0f;
        }

        // Get creature's forward direction based on movement
        public Vector2 GetForwardDirection()
        {
            return currentVelocity.magnitude > 0.01f ? currentVelocity.normalized : Vector2.right;
        }

        // Calculate total kinetic energy
        public float GetKineticEnergy()
        {
            if (rigidBody == null)
                return 0f;

            float mass = rigidBody.mass;
            float velocity = currentVelocity.magnitude;
            return 0.5f * mass * velocity * velocity;
        }

        void OnValidate()
        {
            dragMultiplier = Mathf.Max(0f, dragMultiplier);
        }
    }
}

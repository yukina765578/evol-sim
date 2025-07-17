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
            rigidBody.gravityScale = 0f; // Swimming - no gravity
            rigidBody.linearDamping = 0f;
            rigidBody.angularDamping = 0f;
        }

        void Update()
        {
            if (!creatureState.isInitialized || !enableThrust)
                return;

            UpdateMotion();
        }

        void UpdateMotion()
        {
            // Update segment rotations
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
            for (int i = 0; i < creatureState.segments.Length; i++)
            {
                SegmentData segment = creatureState.segments[i];

                if (
                    segment.parentIndex >= creatureState.nodes.Length
                    || segment.childIndex >= creatureState.nodes.Length
                )
                    continue;

                NodeData parentNode = creatureState.nodes[segment.parentIndex];
                NodeData childNode = creatureState.nodes[segment.childIndex];

                Utils.UpdateNodePosition(ref childNode, parentNode, segment);
                creatureState.nodes[segment.childIndex] = childNode;
            }
        }

        void CalculateForces()
        {
            totalThrust = Vector2.zero;
            totalDrag = Vector2.zero;
            currentVelocity = rigidBody.linearVelocity;
            float maxTotalDrag = currentVelocity.magnitude * 0.9f;
            float dragPerSegment = maxTotalDrag / creatureState.segments.Length;

            for (int i = 0; i < creatureState.segments.Length; i++)
            {
                SegmentData segment = creatureState.segments[i];

                // Calculate thrust for swimming
                Vector2 segmentThrust = Utils.CalculateThrust(segment, creatureState.nodes);
                totalThrust += segmentThrust;

                // Calculate water drag
                Vector2 segmentDrag = Utils.CalculateWaterDrag(
                    segment,
                    creatureState.nodes,
                    currentVelocity,
                    dragPerSegment
                );
                totalDrag += segmentDrag;

                // Energy cost for movement
                if (energy != null)
                {
                    float angleChange = Mathf.Abs(segment.currentAngle - segment.prevAngle);
                    energy.ConsumeMovementEnergy(angleChange);
                }
            }
        }

        void ApplyForces()
        {
            if (totalThrust.magnitude > 0.01f)
                rigidBody.AddForce(totalThrust, ForceMode2D.Force);

            if (enableDrag && totalDrag.magnitude > 0.01f)
                rigidBody.AddForce(totalDrag, ForceMode2D.Force);

            Debug.Log($"Total Thrust: {totalThrust}, Total Drag: {totalDrag}");
            currentVelocity = rigidBody.linearVelocity;
            Debug.Log($"Current Velocity: {currentVelocity}");
        }

        // Essential getters for force analysis
        public Vector2 GetCurrentVelocity() => currentVelocity;

        public Vector2 GetTotalThrust() => totalThrust;

        public Vector2 GetTotalDrag() => totalDrag;

        // Control methods
        public void SetThrustEnabled(bool enabled) => enableThrust = enabled;

        public void SetDragEnabled(bool enabled) => enableDrag = enabled;

        public void UpdateCreatureState(CreatureState newState) => creatureState = newState;

        public float GetMovementEfficiency()
        {
            if (currentVelocity.magnitude < 0.01f)
                return 0f;

            float energySpent = 0f;
            for (int i = 0; i < creatureState.segments.Length; i++)
            {
                float angleChange = Mathf.Abs(
                    creatureState.segments[i].currentAngle - creatureState.segments[i].prevAngle
                );
                energySpent += angleChange * 0.001f;
            }
            return energySpent > 0f ? currentVelocity.magnitude / energySpent : 0f;
        }

        public Vector2 GetForwardDirection()
        {
            return currentVelocity.magnitude > 0.01f ? currentVelocity.normalized : Vector2.right;
        }

        void OnValidate()
        {
            dragMultiplier = Mathf.Max(0f, dragMultiplier);
        }
    }
}

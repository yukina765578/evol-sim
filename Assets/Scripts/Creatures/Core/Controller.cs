using EvolutionSimulator.Creatures.Genetics;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class Controller : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField]
        private bool showThrustDebug = false;

        [SerializeField]
        private bool showVelocityDebug = false;

        [SerializeField]
        private bool showCreatureInfo = false;

        private CreatureGenome genome;
        private CreatureState creatureState;
        private bool isDead = false;

        // System components
        private RenderSystem renderSystem;
        private PhysicsSystem physicsSystem;
        private Motion motionSystem;
        private Energy energySystem;

        // Debug tracking
        private bool prevThrustDebug = false;
        private bool prevVelocityDebug = false;

        public CreatureGenome GetGenome() => genome;

        public CreatureState GetCreatureState() => creatureState;

        public bool IsDead => isDead;

        public void Initialize(CreatureGenome creatureGenome)
        {
            genome = creatureGenome;
            if (genome == null)
            {
                Debug.LogError("Controller requires a valid CreatureGenome!");
                return;
            }

            BuildCreatureData();
            SetupSystems();
        }

        void BuildCreatureData()
        {
            int nodeCount = genome.NodeCount;
            int segmentCount = nodeCount - 1; // Root node has no segment

            creatureState = new CreatureState(nodeCount, segmentCount);

            // Build nodes
            for (int i = 0; i < nodeCount; i++)
            {
                Vector3 position = CalculateNodePosition(i);
                bool isRoot = i == 0;
                int parentIndex = isRoot ? -1 : genome.nodes[i].parentIndex;

                creatureState.nodes[i] = new NodeData(
                    position,
                    DataConstants.DEFAULT_NODE_SIZE,
                    DataConstants.DEFAULT_NODE_COLOR,
                    isRoot,
                    parentIndex
                );
            }

            // Build segments
            for (int i = 1; i < nodeCount; i++)
            {
                NodeGene gene = genome.nodes[i];
                int segmentIndex = i - 1;

                creatureState.segments[segmentIndex] = new SegmentData(
                    gene.parentIndex,
                    i,
                    DataConstants.DEFAULT_SEGMENT_LENGTH,
                    DataConstants.DEFAULT_SEGMENT_WIDTH,
                    DataConstants.DEFAULT_SEGMENT_COLOR,
                    gene
                );
            }

            creatureState.Initialize();
        }

        Vector3 CalculateNodePosition(int nodeIndex)
        {
            if (nodeIndex == 0)
                return Vector3.zero; // Root at origin

            NodeGene gene = genome.nodes[nodeIndex];
            Vector3 parentPosition = CalculateNodePosition(gene.parentIndex);

            float angle = gene.baseAngle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(
                DataConstants.DEFAULT_SEGMENT_LENGTH * Mathf.Cos(angle),
                DataConstants.DEFAULT_SEGMENT_LENGTH * Mathf.Sin(angle),
                0f
            );

            return parentPosition + offset;
        }

        void SetupSystems()
        {
            // Setup render system
            renderSystem = gameObject.AddComponent<RenderSystem>();
            renderSystem.Initialize(creatureState);

            // Setup physics system
            physicsSystem = gameObject.AddComponent<PhysicsSystem>();
            physicsSystem.Initialize(creatureState);

            // Setup motion system
            motionSystem = gameObject.AddComponent<Motion>();
            motionSystem.Initialize(creatureState);

            // Get energy system (should be added by Builder)
            energySystem = GetComponent<Energy>();
            if (energySystem == null)
            {
                Debug.LogWarning("Energy component not found on creature!");
            }
        }

        void Start()
        {
            // Systems should be initialized by now
            if (renderSystem == null || physicsSystem == null || motionSystem == null)
            {
                Debug.LogError("Failed to initialize creature systems!");
                return;
            }

            // Initial debug state
            UpdateDebugSettings();
        }

        void Update()
        {
            if (isDead)
                return;

            // Update all systems with current creature state
            UpdateSystems();

            // Handle debug setting changes
            if (showThrustDebug != prevThrustDebug || showVelocityDebug != prevVelocityDebug)
            {
                UpdateDebugSettings();
                prevThrustDebug = showThrustDebug;
                prevVelocityDebug = showVelocityDebug;
            }
        }

        void UpdateSystems()
        {
            // Motion system updates creature state during its Update()
            // Other systems read the updated state

            // Update all systems with current state
            if (renderSystem != null)
                renderSystem.UpdateCreatureState(creatureState);

            if (physicsSystem != null)
                physicsSystem.UpdateCreatureState(creatureState);

            if (motionSystem != null)
                motionSystem.UpdateCreatureState(creatureState);
        }

        void UpdateDebugSettings()
        {
            if (renderSystem != null)
            {
                renderSystem.SetDebugMode(showThrustDebug, showVelocityDebug);
            }
        }

        public void HandleDeath(string cause)
        {
            if (isDead)
                return;

            isDead = true;

            // Disable systems
            if (motionSystem != null)
                motionSystem.SetThrustEnabled(false);

            Debug.Log($"Creature {name} died from: {cause}");

            // Destroy after brief delay to allow cleanup
            Destroy(gameObject, 0.1f);
        }

        // Public API for other systems
        public Vector2 GetCurrentVelocity()
        {
            return motionSystem != null ? motionSystem.GetCurrentVelocity() : Vector2.zero;
        }

        public float GetMovementEfficiency()
        {
            return motionSystem != null ? motionSystem.GetMovementEfficiency() : 0f;
        }

        public Vector2 GetForwardDirection()
        {
            return motionSystem != null ? motionSystem.GetForwardDirection() : Vector2.right;
        }

        public bool IsGrounded()
        {
            return physicsSystem != null ? physicsSystem.IsGrounded() : false;
        }

        public int GetNodeCount()
        {
            return creatureState.nodes.Length;
        }

        public int GetSegmentCount()
        {
            return creatureState.segments.Length;
        }

        void OnGUI()
        {
            if (!showCreatureInfo)
                return;

            GUILayout.BeginArea(new Rect(10, 100, 300, 200));
            GUILayout.Label($"Creature: {name}");
            GUILayout.Label($"Nodes: {GetNodeCount()}");
            GUILayout.Label($"Segments: {GetSegmentCount()}");
            GUILayout.Label($"Velocity: {GetCurrentVelocity().magnitude:F2}");
            GUILayout.Label($"Efficiency: {GetMovementEfficiency():F2}");
            GUILayout.Label($"Grounded: {IsGrounded()}");

            if (energySystem != null)
            {
                GUILayout.Label(
                    $"Energy: {energySystem.CurrentEnergy:F1}/{energySystem.MaxEnergy:F1}"
                );
                GUILayout.Label($"Reproduction Ready: {energySystem.IsReproductionReady}");
            }

            GUILayout.EndArea();
        }

        void OnValidate()
        {
            // Update debug settings if changed in inspector
            if (Application.isPlaying)
            {
                UpdateDebugSettings();
            }
        }
    }
}

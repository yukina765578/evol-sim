using EvolutionSimulator.Creatures.Genetics;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class Controller : MonoBehaviour
    {
        [Header("Debug Settings")]
        [SerializeField]
        private bool showVelocityDebug = false;

        private CreatureGenome genome;
        private CreatureState creatureState;
        private bool isDead = false;

        // System components
        private RenderSystem renderSystem;
        private PhysicsSystem physicsSystem;
        private Motion motionSystem;
        private Energy energySystem;

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
            int segmentCount = nodeCount - 1;

            creatureState = new CreatureState(nodeCount, segmentCount);

            // Build nodes with relative positions
            for (int i = 0; i < nodeCount; i++)
            {
                Vector3 relativePosition = CalculateRelativeNodePosition(i);
                bool isRoot = i == 0;
                int parentIndex = isRoot ? -1 : genome.nodes[i].parentIndex;

                creatureState.nodes[i] = new NodeData(
                    relativePosition,
                    relativePosition, // position, prevPosition
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

        Vector3 CalculateRelativeNodePosition(int nodeIndex)
        {
            // Root node is always at local origin
            if (nodeIndex == 0)
                return Vector3.zero;

            NodeGene gene = genome.nodes[nodeIndex];
            Vector3 parentPosition = CalculateRelativeNodePosition(gene.parentIndex);
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
            renderSystem = gameObject.AddComponent<RenderSystem>();
            renderSystem.Initialize(creatureState);

            physicsSystem = gameObject.AddComponent<PhysicsSystem>();
            physicsSystem.Initialize(creatureState);

            motionSystem = gameObject.AddComponent<Motion>();
            motionSystem.Initialize(creatureState);

            energySystem = GetComponent<Energy>();
            if (energySystem == null)
                Debug.LogWarning("Energy component not found on creature!");
        }

        void Start()
        {
            if (renderSystem == null || physicsSystem == null || motionSystem == null)
            {
                Debug.LogError("Failed to initialize creature systems!");
                return;
            }
            UpdateDebugSettings();
        }

        void Update()
        {
            if (isDead)
                return;

            // Update all systems with current creature state
            renderSystem?.UpdateCreatureState(creatureState);
            physicsSystem?.UpdateCreatureState(creatureState);
            motionSystem?.UpdateCreatureState(creatureState);
        }

        void UpdateDebugSettings()
        {
            renderSystem?.SetupDebugMode(false, showVelocityDebug);
        }

        public void HandleDeath(string cause)
        {
            if (isDead)
                return;
            isDead = true;

            motionSystem?.SetThrustEnabled(false);
            Debug.Log($"Creature {name} died from: {cause}");
            Destroy(gameObject, 0.1f);
        }

        // Public API for swimming creatures
        public Vector2 GetCurrentVelocity() => motionSystem?.GetCurrentVelocity() ?? Vector2.zero;

        public float GetMovementEfficiency() => motionSystem?.GetMovementEfficiency() ?? 0f;

        public Vector2 GetForwardDirection() =>
            motionSystem?.GetForwardDirection() ?? Vector2.right;

        public int GetNodeCount() => creatureState.nodes.Length;

        public int GetSegmentCount() => creatureState.segments.Length;

        void OnValidate()
        {
            if (Application.isPlaying)
                UpdateDebugSettings();
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolutionSimulator.Creature
{
    public class CreatureTest : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField]
        private bool spawnOnStart = true;

        [SerializeField]
        private Vector3 spawnPosition = Vector3.zero;

        [SerializeField]
        private bool enableBrain = true;

        [Header("Sample Genes")]
        [SerializeField]
        private NodeGene[] testNodes = new NodeGene[]
        {
            new NodeGene(-1, 0f, 0f), // Root node (no movement)
            new NodeGene(0, 0f, 45f), // Connect to root
            new NodeGene(0, 120f, -45f), // Also connect to root
            new NodeGene(1, 45f, -60f), // Connect to node 1
            new NodeGene(2, 180f, 120f), // Connect to node 2
        };

        private GameObject currentCreature;

        void Start()
        {
            if (spawnOnStart)
                SpawnTestCreature();
        }

        void Update()
        {
            if (Keyboard.current == null)
                return;

            if (Keyboard.current.spaceKey.wasPressedThisFrame)
                SpawnTestCreature();

            if (Keyboard.current.cKey.wasPressedThisFrame)
                ClearCreature();

            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RandomizeNodes();
                SpawnTestCreature();
            }

            if (Keyboard.current.bKey.wasPressedThisFrame)
            {
                enableBrain = !enableBrain;
                Debug.Log($"Brain control: {(enableBrain ? "Enabled" : "Disabled")}");
                SpawnTestCreature();
            }
        }

        [ContextMenu("Spawn Test Creature")]
        public void SpawnTestCreature()
        {
            ClearCreature();

            CreatureGenome bodyGenome = new CreatureGenome(testNodes);
            NEATGenome brainGenome = null;

            if (enableBrain)
            {
                int segmentCount = bodyGenome.NodeCount - 1;
                brainGenome = RandomGeneGenerator.GenerateRandomBrain(segmentCount);
            }

            currentCreature = CreatureBuilder.BuildCreature(bodyGenome, brainGenome, spawnPosition);

            string brainStatus = enableBrain ? "with brain" : "without brain";
            Debug.Log($"Spawned creature with {bodyGenome.NodeCount} nodes {brainStatus}");
        }

        [ContextMenu("Clear Creature")]
        public void ClearCreature()
        {
            if (currentCreature != null)
            {
                DestroyImmediate(currentCreature);
                currentCreature = null;
            }
        }

        [ContextMenu("Randomize Nodes")]
        public void RandomizeNodes()
        {
            testNodes = RandomGeneGenerator.GenerateRandomBodyGenome().nodes;
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 180));
            GUILayout.Label("Brain-Controlled Creature Test:");
            GUILayout.Label("SPACE - Spawn creature");
            GUILayout.Label("C - Clear creature");
            GUILayout.Label("R - Randomize & spawn");
            GUILayout.Label("B - Toggle brain control");
            GUILayout.Label($"Current: {(currentCreature ? "Active" : "None")}");
            GUILayout.Label($"Nodes: {testNodes.Length}");
            GUILayout.Label($"Brain: {(enableBrain ? "ON" : "OFF")}");
            GUILayout.EndArea();
        }
    }
}

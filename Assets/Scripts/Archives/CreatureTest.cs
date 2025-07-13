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

        [Header("Sample Gene")]
        [SerializeField]
        private NodeGene[] testNodes = new NodeGene[]
        {
            new NodeGene(-1, 0f, 0f, 0f, 0f), // Root node (no movement)
            new NodeGene(0, 0f, 3f, 45f, 0.2f), // Connect to root
            new NodeGene(0, 120f, 3.5f, -45f, 0.6f), // Also connect to root
            new NodeGene(1, 45f, 4f, -60f, 0.5f), // Connect to node 1
            new NodeGene(2, 180f, 2.5f, 120f, 0.3f), // Connect to node 2
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
        }

        [ContextMenu("Spawn Test Creature")]
        public void SpawnTestCreature()
        {
            ClearCreature();
            CreatureGenome genome = new CreatureGenome(testNodes);
            currentCreature = CreatureBuilder.BuildCreature(genome, spawnPosition);
            Debug.Log($"Spawned creature with {genome.NodeCount} nodes");
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
            testNodes = RandomGeneGenerator.GenerateRandomGenome().nodes;
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 150));
            GUILayout.Label("Sequential Creature Test:");
            GUILayout.Label("SPACE - Spawn creature");
            GUILayout.Label("C - Clear creature");
            GUILayout.Label("R - Randomize & spawn");
            GUILayout.Label($"Current: {(currentCreature ? "Active" : "None")}");
            GUILayout.Label($"Nodes: {testNodes.Length}");
            GUILayout.EndArea();
        }
    }
}

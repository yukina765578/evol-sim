using EvolutionSimulator.Creatures.Genetics;
using EvolutionSimulator.Creatures.Population;
using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolutionSimulator.Creatures.Test
{
    public class TestCreature : MonoBehaviour
    {
        [Header("Test Controls")]
        [SerializeField]
        private bool spawnOnStart = true;

        [SerializeField]
        private Vector3 spawnPosition = Vector3.zero;

        [Header("Sample Genes")]
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

            try
            {
                CreatureGenome genome = new CreatureGenome(testNodes);
                currentCreature = Builder.BuildCreature(genome, spawnPosition);

                if (currentCreature != null)
                {
                    Debug.Log($"Spawned creature with {genome.NodeCount} nodes at {spawnPosition}");
                }
                else
                {
                    Debug.LogError("Failed to create creature - Builder returned null");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error spawning creature: {e.Message}");
            }
        }

        [ContextMenu("Clear Creature")]
        public void ClearCreature()
        {
            if (currentCreature != null)
            {
                DestroyImmediate(currentCreature);
                currentCreature = null;
                Debug.Log("Cleared current creature");
            }
        }

        [ContextMenu("Randomize Nodes")]
        public void RandomizeNodes()
        {
            try
            {
                testNodes = Randomizer.GenerateRandomGenome().nodes;
                Debug.Log($"Generated random genome with {testNodes.Length} nodes");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error generating random genome: {e.Message}");
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));

            GUILayout.Label("Test Creature Controls:");
            GUILayout.Label("Space: Spawn Test Creature");
            GUILayout.Label("C: Clear Current Creature");
            GUILayout.Label("R: Randomize Nodes and Spawn");

            GUILayout.Space(10);

            if (currentCreature != null)
            {
                GUILayout.Label($"Current Creature: {currentCreature.name}");
                GUILayout.Label($"Node Count: {testNodes.Length}");
                GUILayout.Label($"Position: {currentCreature.transform.position}");

                // Show creature health status
                var energy =
                    currentCreature.GetComponent<EvolutionSimulator.Creatures.Core.Energy>();
                if (energy != null)
                {
                    GUILayout.Label("Status: Alive");
                }
            }
            else
            {
                GUILayout.Label("No creature spawned.");
            }

            GUILayout.EndArea();
        }
    }
}

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

        [Header("Sample Gene Data")]
        [SerializeField]
        private SegmentGene[] testGenes = new SegmentGene[]
        {
            // Parent segments (connect to root node at 0,0)
            new SegmentGene(-1, 0f, 2f, 45f, 0.2f, 2), // Parent 0: extends right, has 2 children
            new SegmentGene(-1, 120f, 3f, -30f, 0.8f, 1), // Parent 1: extends up-left, has 1 child
            // Child segments (connect to parent nodes)
            new SegmentGene(0, 45f, 1.5f, 90f, 0.15f, 0), // Child of Parent 0: extends up-right
            new SegmentGene(0, 90f, 4f, -60f, 0.5f, 0), // Child of Parent 0: extends down-right
            new SegmentGene(1, 180f, 2.5f, 120f, 0.3f, 0), // Child of Parent 1: extends left
        };

        private GameObject currentCreature;

        void Start()
        {
            if (spawnOnStart)
            {
                SpawnTestCreature();
            }
        }

        void Update()
        {
            // Check if keyboard exists
            if (Keyboard.current == null)
                return;

            // Press Space to spawn new creature
            if (Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                SpawnTestCreature();
            }

            // Press C to clear current creature
            if (Keyboard.current.cKey.wasPressedThisFrame)
            {
                ClearCreature();
            }

            // Press R to randomize genes and spawn
            if (Keyboard.current.rKey.wasPressedThisFrame)
            {
                RandomizeGenes();
                SpawnTestCreature();
            }
        }

        [ContextMenu("Spawn Test Creature")]
        public void SpawnTestCreature()
        {
            // Clear existing creature
            ClearCreature();

            // Create genome from test genes
            CreatureGenome genome = new CreatureGenome(testGenes);

            // Build the creature
            currentCreature = CreatureBuilder.BuildCreature(genome, spawnPosition);

            Debug.Log($"Spawned creature with {genome.TotalActiveSegments} active segments");
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

        [ContextMenu("Randomize Genes")]
        public void RandomizeGenes()
        {
            // Create 1-3 random parents
            int parentCount = Random.Range(1, 4);
            int totalChildren = Random.Range(1, 6);

            testGenes = new SegmentGene[parentCount + totalChildren];

            // Create parents
            for (int i = 0; i < parentCount; i++)
            {
                testGenes[i] = new SegmentGene(
                    -1, // parentIndex
                    Random.Range(0f, 360f), // baseAngle
                    Random.Range(0.5f, 8f), // oscSpeed
                    Random.Range(-180f, 180f), // maxAngle
                    Random.Range(0.01f, 0.99f), // forwardRatio
                    totalChildren / parentCount // childCount (roughly even distribution)
                );
            }

            // Create children
            for (int i = 0; i < totalChildren; i++)
            {
                testGenes[parentCount + i] = new SegmentGene(
                    Random.Range(0, parentCount), // parentIndex (random parent)
                    Random.Range(0f, 360f), // baseAngle
                    Random.Range(0.5f, 8f), // oscSpeed
                    Random.Range(-180f, 180f), // maxAngle
                    Random.Range(0.01f, 0.99f), // forwardRatio
                    0 // childCount (children have no children)
                );
            }
        }

        void OnGUI()
        {
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("Creature Test Controls:");
            GUILayout.Label("SPACE - Spawn creature");
            GUILayout.Label("C - Clear creature");
            GUILayout.Label("R - Randomize & spawn");
            GUILayout.Label($"Current creature: {(currentCreature ? "Active" : "None")}");
            GUILayout.EndArea();
        }
    }
}

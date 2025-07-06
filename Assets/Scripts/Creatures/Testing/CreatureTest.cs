using UnityEngine;
using UnityEngine.InputSystem;

namespace EvolutionSimulator.Creature
{
    public class CreatureTest : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField]
        private bool spawnOnStart = true;

        [SerializeField]
        private Vector3 spawnPosition = Vector3.zero;

        [SerializeField]
        private bool enableBrain = true;

        [Header("Test Genes")]
        [SerializeField]
        private NodeGene[] testNodes = new NodeGene[]
        {
            new NodeGene(-1, 0f, 0f),
            new NodeGene(0, 0f, 45f),
            new NodeGene(0, 120f, -45f),
            new NodeGene(1, 45f, -60f),
            new NodeGene(2, 180f, 120f),
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
                SpawnTestCreature();
            }
        }

        [ContextMenu("Spawn Test Creature")]
        public void SpawnTestCreature()
        {
            ClearCreature();

            CreatureGenome bodyGenome = new CreatureGenome(testNodes);
            NEATGenome brainGenome = enableBrain
                ? RandomGeneGenerator.GenerateRandomBrain(bodyGenome.NodeCount - 1)
                : null;

            currentCreature = CreatureBuilder.BuildCreature(bodyGenome, brainGenome, spawnPosition);

            Debug.Log(
                $"Spawned creature: {bodyGenome.NodeCount} nodes, brain {(enableBrain ? "ON" : "OFF")}"
            );
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
    }
}

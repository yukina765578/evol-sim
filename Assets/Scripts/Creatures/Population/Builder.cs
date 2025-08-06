using System.Collections.Generic;
using EvolutionSimulator.Creatures.Biology;
using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Creatures.Detectors;
using EvolutionSimulator.Creatures.Genetics;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Population
{
    public static class Builder
    {
        private const float SEGMENT_LENGTH = 2f;
        private const float NODE_SIZE = 1f;
        private const float SEGMENT_WIDTH = 0.1f;
        private static readonly Color NODE_COLOR = Color.blue;
        private static readonly Color SEGMENT_COLOR = Color.white;

        public static GameObject BuildCreature(CreatureGenome genome, Vector3 position)
        {
            GameObject creatureObj = new GameObject("Creature");
            creatureObj.transform.position = position;

            // Set tag safely
            try
            {
                creatureObj.tag = "Creature";
            }
            catch (UnityException)
            {
                Debug.LogWarning("Create 'Creature' tag in Tag Manager");
            }

            int creatureLayer = LayerMask.NameToLayer("Creatures");
            if (creatureLayer == -1)
            {
                Debug.LogWarning("Create 'Creature' layer in Layer Manager");
            }
            else
            {
                creatureObj.layer = creatureLayer;
            }

            // Add physics component
            var rigidbody = creatureObj.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.angularVelocity = 0f;

            // Add energy system
            var energy = creatureObj.AddComponent<Energy>();
            var reproductionController = creatureObj.AddComponent<ReproductionController>();

            // Build creature body with data-only segments
            var (nodes, segmentRenderer) = CreateSequentialCreature(genome, creatureObj);

            SetupDetectioNode(nodes[0]);

            // Add controller last (after body is built)
            var controller = creatureObj.AddComponent<Controller>();
            controller.Initialize(genome, segmentRenderer);

            return creatureObj;
        }

        static (List<Node> nodes, SegmentRenderer segmentRenderer) CreateSequentialCreature(
            CreatureGenome genome,
            GameObject creatureObj
        )
        {
            var nodes = new List<Node>();

            // Create root node
            Node rootNode = CreateNode("RootNode", Vector3.zero, creatureObj.transform);
            nodes.Add(rootNode);

            // Create child nodes (no individual segment GameObjects)
            for (int i = 1; i < genome.NodeCount; i++)
            {
                NodeGene nodeGenome = genome.nodes[i];
                Node parentNode = nodes[nodeGenome.parentIndex];

                // Calculate child position based on base angle
                float angle = nodeGenome.baseAngle * Mathf.Deg2Rad;
                Vector3 nodePosition =
                    parentNode.transform.localPosition
                    + new Vector3(
                        SEGMENT_LENGTH * Mathf.Cos(angle),
                        SEGMENT_LENGTH * Mathf.Sin(angle),
                        0
                    );

                // Create only the node - no segment GameObject
                Node newNode = CreateNode($"Node_{i}", nodePosition, creatureObj.transform);
                nodes.Add(newNode);
            }

            // Create and initialize SegmentRenderer with data
            var segmentRenderer = creatureObj.AddComponent<SegmentRenderer>();
            segmentRenderer.Initialize(genome, nodes.ToArray());

            return (nodes, segmentRenderer);
        }

        static void SetupDetectioNode(Node detectionNode)
        {
            GameObject nodeObj = detectionNode.gameObject;

            // Add Kinematic Rigidbody2D to parent node
            var nodeRigidbody = nodeObj.AddComponent<Rigidbody2D>();
            nodeRigidbody.bodyType = RigidbodyType2D.Kinematic;
            nodeRigidbody.gravityScale = 0f;

            // Create separate child for food detection
            GameObject foodDetectorObj = new GameObject("FoodDetector");
            foodDetectorObj.transform.SetParent(nodeObj.transform);
            foodDetectorObj.transform.localPosition = Vector3.zero;
            foodDetectorObj.layer = LayerMask.NameToLayer("Creatures");
            foodDetectorObj.AddComponent<FoodDetector>();

            // Create separate child for creature detection
            GameObject creatureDetectorObj = new GameObject("CreatureDetector");
            creatureDetectorObj.transform.SetParent(nodeObj.transform);
            creatureDetectorObj.transform.localPosition = Vector3.zero;
            creatureDetectorObj.layer = LayerMask.NameToLayer("Creatures");
            creatureDetectorObj.AddComponent<CreatureDetector>();
        }

        static Node CreateNode(string name, Vector3 position, Transform parent)
        {
            GameObject nodeObj = new GameObject(name);
            nodeObj.transform.SetParent(parent);
            nodeObj.transform.localPosition = position;

            var node = nodeObj.AddComponent<Node>();
            node.Initialize(NODE_SIZE, NODE_COLOR);
            return node;
        }
    }
}

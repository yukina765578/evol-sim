using System.Collections.Generic;
using EvolutionSimulator.Creatures.Core;
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

            // Add physics component
            var rigidbody = creatureObj.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;

            // Add energy system
            var energy = creatureObj.AddComponent<Energy>();

            // Build creature body
            var (nodes, segments) = CreateSequentialCreature(genome, creatureObj);

            if (nodes.Count > 0)
                nodes[0].gameObject.AddComponent<CollisionDetector>();

            // Add controller last (after body is built)
            var controller = creatureObj.AddComponent<Controller>();

            return creatureObj;
        }

        static (List<Node> nodes, List<Segment> segments) CreateSequentialCreature(
            CreatureGenome genome,
            GameObject creatureObj
        )
        {
            var nodes = new List<Node>();
            var segments = new List<Segment>();

            // Create root node
            Node rootNode = CreateNode("RootNode", Vector3.zero, creatureObj.transform);
            nodes.Add(rootNode);

            // Create child nodes and segments
            for (int i = 1; i < genome.NodeCount; i++)
            {
                NodeGene nodeGenome = genome.nodes[i];
                Node parentNode = nodes[nodeGenome.parentIndex];

                // Calculate child position
                float angle = nodeGenome.baseAngle * Mathf.Deg2Rad;
                Vector3 nodePosition =
                    parentNode.transform.localPosition
                    + new Vector3(
                        SEGMENT_LENGTH * Mathf.Cos(angle),
                        SEGMENT_LENGTH * Mathf.Sin(angle),
                        0
                    );

                // Create node and segment
                Node newNode = CreateNode($"Node_{i}", nodePosition, creatureObj.transform);
                nodes.Add(newNode);

                Segment newSegment = CreateSegment(
                    $"Segment_{i}",
                    nodeGenome,
                    parentNode,
                    newNode,
                    creatureObj.transform
                );
                segments.Add(newSegment);
            }

            return (nodes, segments);
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

        static Segment CreateSegment(
            string name,
            NodeGene gene,
            Node parentNode,
            Node childNode,
            Transform parent
        )
        {
            GameObject segmentObj = new GameObject(name);
            segmentObj.transform.SetParent(parent);

            var segment = segmentObj.AddComponent<Segment>();
            segment.Initialize(
                SEGMENT_LENGTH,
                SEGMENT_WIDTH,
                SEGMENT_COLOR,
                gene.oscSpeed,
                gene.maxAngle,
                gene.forwardRatio,
                gene.baseAngle, // Fixed: Added missing baseAngle parameter
                parentNode,
                childNode
            );

            return segment;
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class CreatureBuilder
    {
        // Constants for creature creation
        private const float SEGMENT_LENGTH = 2f;
        private const float NODE_SIZE = 1f;
        private const float SEGMENT_WIDTH = 0.1f;
        private static readonly Color NODE_COLOR = Color.blue;
        private static readonly Color SEGMENT_COLOR = Color.white;

        public static GameObject BuildCreature(CreatureGenome genome, Vector3 position)
        {
            GameObject creatureObj = new GameObject("Creature");
            creatureObj.transform.position = position;

            // Setup physics (same as SimpleCreature2)
            var rigidbody = creatureObj.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;

            // Create creature structure
            var (nodes, segments) = CreateCreature(genome, creatureObj);

            // Add movement controller
            var controller = creatureObj.AddComponent<CreatureController>();

            return creatureObj;
        }

        static (List<Node> nodes, List<Segment> segments) CreateCreature(
            CreatureGenome genome,
            GameObject creatureObj
        )
        {
            var nodes = new List<Node>();
            var segments = new List<Segment>();

            // Create root node at (0,0)
            Node rootNode = CreateNode("RootNode", Vector3.zero, creatureObj.transform);
            nodes.Add(rootNode);

            var parentGenes = genome.GetParentGenes();
            var childGenes = genome.GetChildGenes();

            // Create parent segments (from root to parent nodes)
            for (int i = 0; i < parentGenes.Length; i++)
            {
                float angle = parentGenes[i].baseAngle * Mathf.Deg2Rad;
                Vector3 parentPos = new Vector3(
                    SEGMENT_LENGTH * Mathf.Cos(angle),
                    SEGMENT_LENGTH * Mathf.Sin(angle),
                    0
                );

                Node parentNode = CreateNode($"ParentNode_{i}", parentPos, creatureObj.transform);
                Segment parentSegment = CreateSegment(
                    $"ParentSegment_{i}",
                    parentGenes[i],
                    rootNode,
                    parentNode,
                    creatureObj.transform
                );

                nodes.Add(parentNode);
                segments.Add(parentSegment);
            }

            // Create child segments (from parent nodes to child nodes)
            for (int i = 0; i < childGenes.Length; i++)
            {
                var childGene = childGenes[i];
                if (childGene.parentIndex >= parentGenes.Length)
                    continue;

                Node parentNode = nodes[1 + childGene.parentIndex]; // +1 because root is at index 0

                float angle = childGene.baseAngle * Mathf.Deg2Rad;
                Vector3 childPos =
                    parentNode.transform.localPosition
                    + new Vector3(
                        SEGMENT_LENGTH * Mathf.Cos(angle),
                        SEGMENT_LENGTH * Mathf.Sin(angle),
                        0
                    );

                Node childNode = CreateNode($"ChildNode_{i}", childPos, creatureObj.transform);
                Segment childSegment = CreateSegment(
                    $"ChildSegment_{i}",
                    childGene,
                    parentNode,
                    childNode,
                    creatureObj.transform
                );

                nodes.Add(childNode);
                segments.Add(childSegment);
            }

            return (nodes, segments);
        }

        static Node CreateNode(string name, Vector3 position, Transform parent)
        {
            GameObject nodeObj = new GameObject(name);
            nodeObj.transform.SetParent(parent);
            nodeObj.transform.localPosition = position;

            Node node = nodeObj.AddComponent<Node>();
            node.Initialize(NODE_SIZE, NODE_COLOR);
            return node;
        }

        static Segment CreateSegment(
            string name,
            SegmentGene gene,
            Node parentNode,
            Node childNode,
            Transform parent
        )
        {
            GameObject segmentObj = new GameObject(name);
            segmentObj.transform.SetParent(parent);

            Segment segment = segmentObj.AddComponent<Segment>();
            segment.Initialize(
                SEGMENT_LENGTH,
                SEGMENT_WIDTH,
                SEGMENT_COLOR,
                gene.oscSpeed,
                gene.maxAngle,
                gene.forwardRatio,
                gene.baseAngle,
                parentNode,
                childNode
            );

            return segment;
        }
    }
}

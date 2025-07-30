using System.Collections.Generic;
using EvolutionSimulator.Creatures.Biology;
using EvolutionSimulator.Creatures.Core;
using EvolutionSimulator.Creatures.Detectors;
using EvolutionSimulator.Creatures.Genetics;
using EvolutionSimulator.Creatures.Rendering;
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

            SetupCreatureGameObject(creatureObj);
            AddPhysicsComponents(creatureObj);
            AddBiologyComponents(creatureObj);

            var (nodes, segments) = CreatePhysicsStructure(genome, creatureObj);
            SetupDetectors(nodes[0]);

            var controller = creatureObj.AddComponent<Controller>();
            controller.Initialize(genome);

            // Create render data
            var renderData = CreateRenderData(genome, nodes, segments);
            controller.SetRenderData(renderData);

            return creatureObj;
        }

        static void SetupCreatureGameObject(GameObject creatureObj)
        {
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
                Debug.LogWarning("Create 'Creatures' layer in Layer Manager");
            }
            else
            {
                creatureObj.layer = creatureLayer;
            }
        }

        static void AddPhysicsComponents(GameObject creatureObj)
        {
            var rigidbody = creatureObj.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;
            rigidbody.linearVelocity = Vector2.zero;
            rigidbody.angularVelocity = 0f;
        }

        static void AddBiologyComponents(GameObject creatureObj)
        {
            creatureObj.AddComponent<Energy>();
            creatureObj.AddComponent<ReproductionController>();
        }

        static (List<Node> nodes, List<Segment> segments) CreatePhysicsStructure(
            CreatureGenome genome,
            GameObject creatureObj
        )
        {
            var nodes = new List<Node>();
            var segments = new List<Segment>();

            // Create root node
            Node rootNode = CreatePhysicsNode("RootNode", Vector3.zero, creatureObj.transform);
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
                        0f
                    );

                Node newNode = CreatePhysicsNode($"Node_{i}", nodePosition, creatureObj.transform);
                nodes.Add(newNode);

                Segment newSegment = CreatePhysicsSegment(
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

        static Node CreatePhysicsNode(string name, Vector3 position, Transform parent)
        {
            GameObject nodeObj = new GameObject(name);
            nodeObj.transform.SetParent(parent);
            nodeObj.transform.localPosition = position;

            var node = nodeObj.AddComponent<Node>();
            node.InitializePhysicsOnly(NODE_SIZE, NODE_COLOR);
            return node;
        }

        static Segment CreatePhysicsSegment(
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
            segment.InitializePhysicsOnly(
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

        static void SetupDetectors(Node rootNode)
        {
            GameObject nodeObj = rootNode.gameObject;

            var nodeRigidbody = nodeObj.AddComponent<Rigidbody2D>();
            nodeRigidbody.bodyType = RigidbodyType2D.Kinematic;
            nodeRigidbody.gravityScale = 0f;

            // Food detector
            GameObject foodDetectorObj = new GameObject("FoodDetector");
            foodDetectorObj.transform.SetParent(nodeObj.transform);
            foodDetectorObj.transform.localPosition = Vector3.zero;
            foodDetectorObj.layer = LayerMask.NameToLayer("Creatures");
            foodDetectorObj.AddComponent<FoodDetector>();

            // Creature detector
            GameObject creatureDetectorObj = new GameObject("CreatureDetector");
            creatureDetectorObj.transform.SetParent(nodeObj.transform);
            creatureDetectorObj.transform.localPosition = Vector3.zero;
            creatureDetectorObj.layer = LayerMask.NameToLayer("Creatures");
            creatureDetectorObj.AddComponent<CreatureDetector>();
        }

        static CreatureRenderData CreateRenderData(
            CreatureGenome genome,
            List<Node> nodes,
            List<Segment> segments
        )
        {
            // Create node data
            NodeData[] nodeData = new NodeData[nodes.Count];
            for (int i = 0; i < nodes.Count; i++)
            {
                nodeData[i] = new NodeData(
                    nodes[i].transform.localPosition,
                    NODE_SIZE,
                    NODE_COLOR,
                    i == 0 ? -1 : genome.nodes[i].parentIndex
                );
            }

            // Create segment data
            SegmentData[] segmentData = new SegmentData[segments.Count];
            for (int i = 0; i < segments.Count; i++)
            {
                NodeGene nodeGene = genome.nodes[i + 1]; // segments start from node 1
                segmentData[i] = new SegmentData(
                    nodeGene.parentIndex,
                    i + 1,
                    SEGMENT_WIDTH,
                    SEGMENT_COLOR
                );
            }

            return new CreatureRenderData(nodeData, segmentData);
        }
    }
}

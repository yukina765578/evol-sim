using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class CreatureBuilder
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

            try
            {
                creatureObj.tag = "Creature";
            }
            catch (UnityException)
            {
                Debug.LogWarning("Create 'Creature' tag in Tag Manager");
            }

            var rigidbody = creatureObj.AddComponent<Rigidbody2D>();
            rigidbody.gravityScale = 0f;

            var energy = creatureObj.AddComponent<CreatureEnergy>();
            var reproduction = creatureObj.AddComponent<ReproductionController>();
            var reproductionTrigger = creatureObj.AddComponent<ReproductionTrigger>();
            reproductionTrigger.Initialize(reproduction);

            var (nodes, segments) = CreateSequentialCreature(genome, creatureObj);

            if (nodes.Count > 0)
                nodes[0].gameObject.AddComponent<FoodDetector>();

            var controller = creatureObj.AddComponent<CreatureController>();
            energy.OnDeath.AddListener(() => OnCreatureDeath(creatureObj));

            return creatureObj;
        }

        static (List<Node> nodes, List<Segment> segments) CreateSequentialCreature(
            CreatureGenome genome,
            GameObject creatureObj
        )
        {
            var nodes = new List<Node>();
            var segments = new List<Segment>();

            // Create root node at origin
            Node rootNode = CreateNode("RootNode", Vector3.zero, creatureObj.transform);
            nodes.Add(rootNode);

            // Create remaining nodes sequentially
            for (int i = 1; i < genome.NodeCount; i++)
            {
                var nodeGene = genome.nodes[i];
                Node parentNode = nodes[nodeGene.parentIndex];

                // Calculate position based on parent and base angle
                float angle = nodeGene.baseAngle * Mathf.Deg2Rad;
                Vector3 nodePos =
                    parentNode.transform.localPosition
                    + new Vector3(
                        SEGMENT_LENGTH * Mathf.Cos(angle),
                        SEGMENT_LENGTH * Mathf.Sin(angle),
                        0
                    );

                Node newNode = CreateNode($"Node_{i}", nodePos, creatureObj.transform);
                Segment segment = CreateSegment(
                    $"Segment_{i}",
                    nodeGene,
                    parentNode,
                    newNode,
                    creatureObj.transform
                );

                nodes.Add(newNode);
                segments.Add(segment);
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
            NodeGene gene,
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

        static void OnCreatureDeath(GameObject creature)
        {
            if (creature != null)
            {
                var nodes = creature.GetComponentsInChildren<Node>();
                foreach (var node in nodes)
                    if (node != null)
                        node.GetComponent<SpriteRenderer>().color = Color.red;

                Object.Destroy(creature, 0.5f);
            }
        }
    }
}

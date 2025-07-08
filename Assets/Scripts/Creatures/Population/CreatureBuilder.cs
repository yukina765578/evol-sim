using System.Collections;
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

        // Layer constants
        private const int CREATURE_LAYER = 8; // "Creatures" layer
        private const int ENVIRONMENT_LAYER = 9; // "Environment" layer

        public static GameObject BuildCreature(CreatureGenome genome, Vector3 position)
        {
            return BuildCreature(genome, null, position);
        }

        public static GameObject BuildCreature(
            CreatureGenome genome,
            NEATGenome brain,
            Vector3 position
        )
        {
            GameObject creature = new GameObject("Creature");
            creature.transform.position = position;
            creature.layer = CREATURE_LAYER;

            SetupComponents(creature, brain);
            var (nodes, segments) = BuildBody(genome, creature);

            if (nodes.Count > 0)
                nodes[0].gameObject.AddComponent<FoodDetector>();

            // Initialize physics properly
            InitializePhysics(creature);

            return creature;
        }

        static void SetupComponents(GameObject creature, NEATGenome brain)
        {
            try
            {
                creature.tag = "Creature";
            }
            catch (UnityException)
            {
                Debug.LogWarning("Create 'Creature' tag in Tag Manager");
            }

            var rb = creature.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 0f;
            rb.angularDamping = 0f;

            var energy = creature.AddComponent<CreatureEnergy>();
            var sensor = creature.AddComponent<CreatureSensor>();
            var reproduction = creature.AddComponent<ReproductionController>();

            creature.AddComponent<CreatureController>();

            if (brain != null)
            {
                var brainComponent = creature.AddComponent<CreatureBrain>();
                brainComponent.Initialize(brain);
            }

            energy.OnDeath.AddListener(() => OnCreatureDeath(creature));
        }

        static void InitializePhysics(GameObject creature)
        {
            var rb = creature.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                // Reset all velocities
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;

                // Disable physics briefly to prevent spawn momentum
                rb.bodyType = RigidbodyType2D.Kinematic;

                // Re-enable physics after a frame
                var mono = creature.GetComponent<CreatureController>();
                if (mono != null)
                {
                    mono.StartCoroutine(ReenablePhysics(rb));
                }
            }
        }

        static IEnumerator ReenablePhysics(Rigidbody2D rb)
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();

            if (rb != null)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        static (List<Node>, List<Segment>) BuildBody(CreatureGenome genome, GameObject creature)
        {
            var nodes = new List<Node>();
            var segments = new List<Segment>();

            Node rootNode = CreateNode("RootNode", Vector3.zero, creature.transform);
            nodes.Add(rootNode);

            for (int i = 1; i < genome.NodeCount; i++)
            {
                var gene = genome.nodes[i];
                Node parent = nodes[gene.parentIndex];

                float angle = gene.baseAngle * Mathf.Deg2Rad;
                Vector3 pos =
                    parent.transform.localPosition
                    + new Vector3(
                        SEGMENT_LENGTH * Mathf.Cos(angle),
                        SEGMENT_LENGTH * Mathf.Sin(angle),
                        0
                    );

                Node node = CreateNode($"Node_{i}", pos, creature.transform);
                Segment segment = CreateSegment(
                    $"Segment_{i}",
                    gene,
                    parent,
                    node,
                    creature.transform
                );

                nodes.Add(node);
                segments.Add(segment);
            }

            return (nodes, segments);
        }

        static Node CreateNode(string name, Vector3 position, Transform parent)
        {
            GameObject nodeObj = new GameObject(name);
            nodeObj.transform.SetParent(parent);
            nodeObj.transform.localPosition = position;
            nodeObj.layer = CREATURE_LAYER;

            Node node = nodeObj.AddComponent<Node>();
            node.Initialize(NODE_SIZE, NODE_COLOR);
            return node;
        }

        static Segment CreateSegment(
            string name,
            NodeGene gene,
            Node parent,
            Node child,
            Transform creatureParent
        )
        {
            GameObject segmentObj = new GameObject(name);
            segmentObj.transform.SetParent(creatureParent);
            segmentObj.layer = CREATURE_LAYER;

            Segment segment = segmentObj.AddComponent<Segment>();
            segment.Initialize(
                SEGMENT_LENGTH,
                SEGMENT_WIDTH,
                SEGMENT_COLOR,
                gene.maxAngle,
                gene.baseAngle,
                parent,
                child
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

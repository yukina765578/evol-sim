using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class RandomGeneGenerator
    {
        private const int MIN_NODES = 2;
        private const int MAX_NODES = 20;

        public static CreatureGenome GenerateRandomGenome()
        {
            int nodeCount = Random.Range(MIN_NODES, MAX_NODES + 1);
            NodeGene[] nodes = new NodeGene[nodeCount];

            nodes[0] = new NodeGene(-1, 0f, 0f, 0f, 0f);

            for (int i = 1; i < nodeCount; i++)
            {
                int parentIndex = Random.Range(0, i); // Ensure valid parent index
                nodes[i] = new NodeGene(
                    parentIndex,
                    Random.Range(0f, 360f), // base angle
                    Random.Range(0.5f, 8f), // oscillation speed
                    Random.Range(-180f, 180f), // max angle
                    Random.Range(0.01f, 0.5f) // forward ratio
                );
            }

            return new CreatureGenome(nodes);
        }

        public static CreatureGenome GenerateMinimalGenome()
        {
            NodeGene[] nodes = new NodeGene[2];
            nodes[0] = new NodeGene(-1, 0f, 0f, 0f, 0f); // Root node
            nodes[1] = new NodeGene(0, 0f, 2f, 45f, 0.2f); // Single child node
            return new CreatureGenome(nodes);
        }

        public static CreatureGenome GenerateTestGenome()
        {
            NodeGene[] nodes = new NodeGene[5];
            nodes[0] = new NodeGene(-1, 0f, 0f, 0f, 0f);
            nodes[1] = new NodeGene(0, 30f, 2f, 45f, 0.2f);
            nodes[2] = new NodeGene(1, 60f, 3f, -30f, 0.3f);
            nodes[3] = new NodeGene(1, -45f, 4f, 60f, 0.4f);
            nodes[4] = new NodeGene(2, 90f, 5f, -90f, 0.5f);
            return new CreatureGenome(nodes);
        }
    }
}

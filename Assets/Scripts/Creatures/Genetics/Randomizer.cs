using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    public static class Randomizer
    {
        private const int MIN_NODES = 3;
        private const int MAX_NODES = 20;

        public static CreatureGenome GenerateRandomGenome()
        {
            int nodeCount = Random.Range(MIN_NODES, MAX_NODES);
            NodeGene[] nodes = new NodeGene[nodeCount];

            nodes[0] = new NodeGene(-1, 0f, 0f, 0f, 0f);

            for (int i = 1; i < nodeCount; i++)
            {
                int parentIndex = Random.Range(0, i);
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
    }
}

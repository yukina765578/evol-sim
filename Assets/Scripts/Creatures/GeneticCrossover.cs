using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class GeneticCrossover
    {
        private const float MUTATION_RATE = 0.1f;
        private const float STRUCTURAL_MUTATION_RATE = 0.05f;
        private const int MAX_NODES = 20;

        public static (CreatureGenome, CreatureGenome) CrossoverGenomes(
            CreatureGenome parent1,
            CreatureGenome parent2
        )
        {
            int minNodes = Mathf.Min(parent1.NodeCount, parent2.NodeCount);

            var cutPoints = GenerateCutPoints(minNodes);
            var offSpringNodes1 = new List<NodeGene>();
            var offSpringNodes2 = new List<NodeGene>();

            bool swapping = false;
            int cutIndex = 0;

            for (int i = 0; i < minNodes; i++)
            {
                if (cutIndex < cutPoints.Length && i == cutPoints[cutIndex])
                {
                    swapping = !swapping;
                    cutIndex++;
                }

                NodeGene node1 = parent1.nodes[i];
                NodeGene node2 = parent2.nodes[i];

                if (swapping)
                {
                    offSpringNodes1.Add(node2);
                    offSpringNodes2.Add(node1);
                }
                else
                {
                    offSpringNodes1.Add(node1);
                    offSpringNodes2.Add(node2);
                }
            }

            CreatureGenome offspringGenome1 = new CreatureGenome(offSpringNodes1.ToArray());
            CreatureGenome offspringGenome2 = new CreatureGenome(offSpringNodes2.ToArray());

            MutateGenome(offspringGenome1);
            MutateGenome(offspringGenome2);

            return (offspringGenome1, offspringGenome2);
        }

        static int[] GenerateCutPoints(int nodeCount)
        {
            if (nodeCount <= 2)
                return new int[0];

            int numCuts = Random.Range(1, Mathf.Min(4, nodeCount - 1));
            List<int> cutPoints = new List<int>();

            while (cutPoints.Count < numCuts)
            {
                int cutPoint = Random.Range(1, nodeCount - 1);
                if (!cutPoints.Contains(cutPoint))
                {
                    cutPoints.Add(cutPoint);
                }
            }

            cutPoints.Sort();
            return cutPoints.ToArray();
        }

        public static void MutateGenome(CreatureGenome genome)
        {
            for (int i = 0; i < genome.nodes.Length; i++)
            {
                genome.nodes[i] = MutateNode(genome.nodes[i]);
            }
        }

        static NodeGene MutateNode(NodeGene node)
        {
            var mutated = node;

            if (Random.value < MUTATION_RATE)
                mutated.baseAngle = Mathf.Clamp(
                    mutated.baseAngle + Random.Range(-18f, 18f),
                    0f,
                    360f
                );

            if (Random.value < MUTATION_RATE)
                mutated.oscSpeed = Mathf.Clamp(
                    mutated.oscSpeed + Random.Range(-0.4f, 0.4f),
                    0.5f,
                    8f
                );

            if (Random.value < MUTATION_RATE)
                mutated.maxAngle = Mathf.Clamp(
                    mutated.maxAngle + Random.Range(-18f, 18f),
                    -180f,
                    180f
                );

            if (Random.value < MUTATION_RATE)
                mutated.forwardRatio = Mathf.Clamp(
                    mutated.forwardRatio + Random.Range(-0.05f, 0.05f),
                    0.01f,
                    0.99f
                );

            return mutated;
        }
    }
}

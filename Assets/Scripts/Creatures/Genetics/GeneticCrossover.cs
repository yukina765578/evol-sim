using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    public static class GeneticCrossover
    {
        private const float MUTATION_RATE = 0.1f;
        private const float STRUCTURAL_MUTATION_RATE = 0.05f;
        private const int MAX_NODES = 20;

        public static CreatureGenome CrossoverGenomes(
            CreatureGenome parent1,
            CreatureGenome parent2
        )
        {
            int minNodes = Mathf.Min(parent1.NodeCount, parent2.NodeCount);
            int maxNodes = Mathf.Max(parent1.NodeCount, parent2.NodeCount);

            var cutPoints = GenerateCutPoints(minNodes);
            var offSpringNodes = new List<NodeGene>();

            bool swapping = false;
            int cutIndex = 0;
            for (int i = 0; i < maxNodes; i++)
            {
                if (cutIndex < cutPoints.Length && i == cutPoints[cutIndex])
                {
                    swapping = !swapping;
                    cutIndex++;
                }

                bool hasNode1 = i < parent1.NodeCount;
                bool hasNode2 = i < parent2.NodeCount;

                if (swapping)
                {
                    if (hasNode2)
                        offSpringNodes.Add(parent2.nodes[i]);
                    else
                        break;
                }
                else
                {
                    if (hasNode1)
                        offSpringNodes.Add(parent1.nodes[i]);
                    else
                        break;
                }
            }

            CreatureGenome offspringGenome = new CreatureGenome(offSpringNodes.ToArray());

            System.Array.Copy(
                Random.value < 0.5f ? parent1.mainBrainWeights : parent2.mainBrainWeights,
                offspringGenome.mainBrainWeights,
                GeneticsConstants.MAIN_BRAIN_WEIGHTS
            );

            MutateGenome(offspringGenome);

            return offspringGenome;
        }

        static int[] GenerateCutPoints(int nodeCount)
        {
            if (nodeCount <= 2)
                return new int[0];

            int numCuts = Random.Range(1, nodeCount - 2);
            List<int> cutPoints = new List<int>();

            while (cutPoints.Count < numCuts)
            {
                int cutPoint = Random.Range(2, nodeCount - 1);
                if (!cutPoints.Contains(cutPoint))
                {
                    cutPoints.Add(cutPoint);
                }
            }
            cutPoints.Sort();
            return cutPoints.ToArray();
        }

        static void MutateGenome(CreatureGenome genome)
        {
            MutateNeuralWeights(genome.mainBrainWeights);

            StructuralMutations(genome);
            for (int i = 0; i < genome.NodeCount; i++)
            {
                NodeGene node = genome.nodes[i];
                if (Random.value < MUTATION_RATE)
                {
                    node.baseAngle = Mathf.Clamp(
                        node.baseAngle + Random.Range(-10f, 10f),
                        GeneticsConstants.MIN_BASE_ANGLE,
                        GeneticsConstants.MAX_BASE_ANGLE
                    );
                }
                if (Random.value < MUTATION_RATE)
                {
                    node.maxAngle = Mathf.Clamp(
                        node.maxAngle + Random.Range(-10f, 10f),
                        GeneticsConstants.MIN_MAX_ANGLE,
                        GeneticsConstants.MAX_MAX_ANGLE
                    );
                }

                MutateNeuralWeights(node.segmentBrainWeights);
                genome.nodes[i] = node;
            }
        }

        static void MutateNeuralWeights(float[] weights)
        {
            for (int i = 0; i < weights.Length; i++)
            {
                if (Random.value < MUTATION_RATE)
                {
                    weights[i] += Random.Range(
                        -GeneticsConstants.NEURAL_MUTATION_STRENGTH,
                        GeneticsConstants.NEURAL_MUTATION_STRENGTH
                    );
                    weights[i] = Mathf.Clamp(
                        weights[i],
                        GeneticsConstants.MIN_NEURAL_WEIGHT,
                        GeneticsConstants.MAX_NEURAL_WEIGHT
                    );
                }
            }
        }

        static void StructuralMutations(CreatureGenome genome)
        {
            if (genome.NodeCount < MAX_NODES && Random.value < STRUCTURAL_MUTATION_RATE)
                AddRandomNode(genome);
            if (genome.NodeCount > 2 && Random.value < STRUCTURAL_MUTATION_RATE)
                RemoveRandomNode(genome);
        }

        static void AddRandomNode(CreatureGenome genome)
        {
            int parentIndex = Random.Range(0, genome.NodeCount);
            NodeGene newNode = new NodeGene(
                parentIndex,
                Random.Range(0f, 360f), // baseAngle
                Random.Range(-180f, 180f) // maxAngle
            );

            // Initialize neural network weights
            Randomizer.InitializeSegmentBrainWeights(ref newNode);
            NodeGene[] newNodes = new NodeGene[genome.NodeCount + 1];
            System.Array.Copy(genome.nodes, newNodes, genome.NodeCount);
            newNodes[genome.NodeCount] = newNode;
            genome.nodes = newNodes;
        }

        static void RemoveRandomNode(CreatureGenome genome)
        {
            int removeIndex = Random.Range(1, genome.NodeCount - 1);
            int parentOfRemovedNode = genome.nodes[removeIndex].parentIndex;

            for (int i = 0; i < genome.nodes.Length; i++)
            {
                if (genome.nodes[i].parentIndex == removeIndex)
                {
                    var node = genome.nodes[i];
                    node.parentIndex = parentOfRemovedNode;
                    genome.nodes[i] = node;
                }
            }

            for (int i = 0; i < genome.nodes.Length; i++)
            {
                if (genome.nodes[i].parentIndex > removeIndex)
                {
                    var node = genome.nodes[i];
                    node.parentIndex--;
                    genome.nodes[i] = node;
                }
            }

            NodeGene[] newNodes = new NodeGene[genome.NodeCount - 1];
            System.Array.Copy(genome.nodes, 0, newNodes, 0, removeIndex);
            System.Array.Copy(
                genome.nodes,
                removeIndex + 1,
                newNodes,
                removeIndex,
                genome.NodeCount - removeIndex - 1
            );
            genome.nodes = newNodes;
        }
    }
}

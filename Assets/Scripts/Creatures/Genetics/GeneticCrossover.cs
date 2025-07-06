using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class GeneticCrossover
    {
        private const float MUTATION_RATE = 0.1f;
        private const float STRUCTURAL_MUTATION_RATE = 0.05f;
        private const int MAX_NODES = 20;

        // Original method for backward compatibility
        public static (CreatureGenome, CreatureGenome) CrossoverGenomes(
            CreatureGenome parent1,
            CreatureGenome parent2
        )
        {
            var (offspring1, offspring2, _, _, _, _) = CrossoverGenomes(
                parent1,
                null,
                parent2,
                null
            );
            return (offspring1, offspring2);
        }

        // New method with brain crossover
        public static (
            CreatureGenome,
            CreatureGenome,
            NEATGenome,
            NEATGenome,
            int offspring1Length,
            int offspring2Length
        ) CrossoverGenomes(
            CreatureGenome parent1Body,
            NEATGenome parent1Brain,
            CreatureGenome parent2Body,
            NEATGenome parent2Brain
        )
        {
            // Crossover body structures
            var (offspring1Body, offspring2Body) = CrossoverBodies(parent1Body, parent2Body);

            // Calculate segment counts for each offspring
            int offspring1Length = offspring1Body.NodeCount - 1;
            int offspring2Length = offspring2Body.NodeCount - 1;

            NEATGenome offspring1Brain = null;
            NEATGenome offspring2Brain = null;

            // Crossover brains if both parents have them
            if (parent1Brain != null && parent2Brain != null)
            {
                var (brain1, brain2) = NEATCrossover.CrossoverGenomes(parent1Brain, parent2Brain);

                // Adjust output nodes to match offspring segment counts
                offspring1Brain = AdjustOutputNodes(brain1, offspring1Length);
                offspring2Brain = AdjustOutputNodes(brain2, offspring2Length);
            }

            return (
                offspring1Body,
                offspring2Body,
                offspring1Brain,
                offspring2Brain,
                offspring1Length,
                offspring2Length
            );
        }

        static (CreatureGenome, CreatureGenome) CrossoverBodies(
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

        static NEATGenome AdjustOutputNodes(NEATGenome brain, int requiredOutputs)
        {
            if (brain == null)
                return null;

            var adjustedBrain = brain.Clone();
            var currentOutputs = adjustedBrain.GetOutputNodes();
            int currentOutputCount = currentOutputs.Length;

            if (currentOutputCount == requiredOutputs)
                return adjustedBrain;

            // Remove excess output nodes
            if (currentOutputCount > requiredOutputs)
            {
                for (int i = requiredOutputs; i < currentOutputCount; i++)
                {
                    adjustedBrain.nodes.RemoveAll(n => n.id == currentOutputs[i].id);
                }
            }
            // Add missing output nodes
            else
            {
                int maxId = adjustedBrain.nodes.Count > 0 ? adjustedBrain.nodes.Max(n => n.id) : 0;
                for (int i = currentOutputCount; i < requiredOutputs; i++)
                {
                    adjustedBrain.AddNode(new NodeGeneNEAT(maxId + 1 + i, NodeType.Output));
                }
            }

            return adjustedBrain;
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
                    cutPoints.Add(cutPoint);
            }

            cutPoints.Sort();
            return cutPoints.ToArray();
        }

        public static void MutateGenome(CreatureGenome genome)
        {
            StructuralMutations(genome);
            for (int i = 0; i < genome.nodes.Length; i++)
                genome.nodes[i] = MutateNode(genome.nodes[i]);
        }

        static void StructuralMutations(CreatureGenome genome)
        {
            if (genome.NodeCount < MAX_NODES && Random.value < 0.01f)
                AddRandomNode(genome);
            if (genome.NodeCount > 2 && Random.value < 0.01f)
                RemoveRandomNode(genome);
        }

        static void AddRandomNode(CreatureGenome genome)
        {
            int parentIndex = Random.Range(0, genome.NodeCount);
            NodeGene newNode = new NodeGene(
                parentIndex,
                Random.Range(0f, 360f),
                Random.Range(-180f, 180f)
            );
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
                mutated.maxAngle = Mathf.Clamp(
                    mutated.maxAngle + Random.Range(-18f, 18f),
                    -180f,
                    180f
                );

            return mutated;
        }
    }
}

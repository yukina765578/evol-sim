using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class NEATCrossover
    {
        private const float MUTATION_RATE = 0.1f;
        private const float DISABLE_GENE_RATE = 0.75f;

        public static (NEATGenome, NEATGenome) CrossoverGenomes(
            NEATGenome parent1,
            NEATGenome parent2
        )
        {
            // Create offspring with same input/output structure as parents
            int inputCount = parent1.GetInputNodes().Length;
            int outputCount = parent1.GetOutputNodes().Length;

            NEATGenome offspring1 = new NEATGenome(inputCount, outputCount);
            NEATGenome offspring2 = new NEATGenome(inputCount, outputCount);

            // Clear default nodes to rebuild from crossover
            offspring1.nodes.Clear();
            offspring2.nodes.Clear();

            // Collect all unique node IDs
            HashSet<int> allNodeIds = parent1
                .nodes.Select(n => n.id)
                .Union(parent2.nodes.Select(n => n.id))
                .ToHashSet();

            // Add all referenced nodes to both offspring
            foreach (int nodeId in allNodeIds)
            {
                NodeGeneNEAT node1 = parent1.nodes.FirstOrDefault(n => n.id == nodeId);
                NodeGeneNEAT node2 = parent2.nodes.FirstOrDefault(n => n.id == nodeId);

                if (node1.id != 0 && node2.id != 0) // Both parents have this node
                {
                    offspring1.AddNode(Random.value < 0.5f ? node1 : node2);
                    offspring2.AddNode(Random.value < 0.5f ? node1 : node2);
                }
                else if (node1.id != 0) // Only parent1 has this node
                {
                    if (Random.value < 0.5f)
                        offspring1.AddNode(node1);
                    if (Random.value < 0.5f)
                        offspring2.AddNode(node1);
                }
                else if (node2.id != 0) // Only parent2 has this node
                {
                    if (Random.value < 0.5f)
                        offspring1.AddNode(node2);
                    if (Random.value < 0.5f)
                        offspring2.AddNode(node2);
                }
            }

            // Crossover connections
            CrossoverConnections(parent1, parent2, offspring1, offspring2);

            // Mutate offspring
            MutateGenome(offspring1);
            MutateGenome(offspring2);

            return (offspring1, offspring2);
        }

        static void CrossoverConnections(
            NEATGenome parent1,
            NEATGenome parent2,
            NEATGenome offspring1,
            NEATGenome offspring2
        )
        {
            Dictionary<int, ConnectionGene> p1Connections = parent1.connections.ToDictionary(c =>
                c.innovation
            );
            Dictionary<int, ConnectionGene> p2Connections = parent2.connections.ToDictionary(c =>
                c.innovation
            );

            IEnumerable<int> allInnovations = p1Connections.Keys.Union(p2Connections.Keys);

            foreach (int innovation in allInnovations)
            {
                bool hasP1 = p1Connections.ContainsKey(innovation);
                bool hasP2 = p2Connections.ContainsKey(innovation);

                if (hasP1 && hasP2) // Matching genes
                {
                    ConnectionGene conn1 =
                        Random.value < 0.5f ? p1Connections[innovation] : p2Connections[innovation];
                    ConnectionGene conn2 =
                        Random.value < 0.5f ? p1Connections[innovation] : p2Connections[innovation];

                    if (NodesExist(offspring1, conn1))
                        offspring1.AddConnection(conn1);
                    if (NodesExist(offspring2, conn2))
                        offspring2.AddConnection(conn2);
                }
                else // Disjoint/excess genes
                {
                    ConnectionGene conn = hasP1
                        ? p1Connections[innovation]
                        : p2Connections[innovation];

                    if (Random.value < 0.5f && NodesExist(offspring1, conn))
                        offspring1.AddConnection(conn);
                    if (Random.value < 0.5f && NodesExist(offspring2, conn))
                        offspring2.AddConnection(conn);
                }
            }
        }

        static bool NodesExist(NEATGenome genome, ConnectionGene connection)
        {
            return genome.nodes.Any(n => n.id == connection.inputId)
                && genome.nodes.Any(n => n.id == connection.outputId);
        }

        static void MutateGenome(NEATGenome genome)
        {
            // Mutate connection weights
            for (int i = 0; i < genome.connections.Count; i++)
            {
                if (Random.value < MUTATION_RATE)
                {
                    ConnectionGene conn = genome.connections[i];
                    conn.weight += Random.Range(-0.5f, 0.5f);
                    conn.weight = Mathf.Clamp(conn.weight, -3f, 3f);
                    genome.connections[i] = conn;
                }
            }

            // Mutate node biases
            for (int i = 0; i < genome.nodes.Count; i++)
            {
                if (Random.value < MUTATION_RATE)
                {
                    NodeGeneNEAT node = genome.nodes[i];
                    node.bias += Random.Range(-0.3f, 0.3f);
                    node.bias = Mathf.Clamp(node.bias, -2f, 2f);
                    genome.nodes[i] = node;
                }
            }
        }
    }
}

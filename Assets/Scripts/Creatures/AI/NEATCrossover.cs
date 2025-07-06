using System.Collections.Generic;
using System.Linq;
using EvolutionSimulator.Creature;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class NEATCrossover
    {
        private const float MUTATION_RATE = 0.1f;
        private const int FIXED_INPUT_COUNT = 12;

        public static (NEATGenome, NEATGenome) CrossoverGenomes(
            NEATGenome parent1,
            NEATGenome parent2,
            int offspring1OutputCount,
            int offspring2OutputCount
        )
        {
            // Phase 1: Create offspring with correct structure
            NEATGenome offspring1 = CreateOffspringStructure(offspring1OutputCount);
            NEATGenome offspring2 = CreateOffspringStructure(offspring2OutputCount);

            // Phase 2: Crossover hidden nodes
            CrossoverHiddenNodes(parent1, parent2, offspring1, offspring2);

            // Phase 3: Inherit input→hidden connections
            InheritInputToHiddenConnections(parent1, parent2, offspring1, offspring2);

            // Phase 4: Rebuild hidden→output connections with proportional mapping
            RebuildOutputConnections(parent1, offspring1);
            RebuildOutputConnections(parent2, offspring2);

            // Phase 5: Mutate offspring
            MutateGenome(offspring1);
            MutateGenome(offspring2);

            return (offspring1, offspring2);
        }

        static NEATGenome CreateOffspringStructure(int outputCount)
        {
            NEATGenome offspring = new NEATGenome();

            // Always create 12 input nodes (IDs 0-11)
            for (int i = 0; i < FIXED_INPUT_COUNT; i++)
                offspring.AddNode(new NodeGeneNEAT(i, NodeType.Input));

            // Create output nodes starting at ID 12
            for (int i = 0; i < outputCount; i++)
                offspring.AddNode(new NodeGeneNEAT(FIXED_INPUT_COUNT + i, NodeType.Output));

            return offspring;
        }

        static void CrossoverHiddenNodes(
            NEATGenome parent1,
            NEATGenome parent2,
            NEATGenome offspring1,
            NEATGenome offspring2
        )
        {
            var p1Hidden = parent1.nodes.Where(n => n.type == NodeType.Hidden).ToArray();
            var p2Hidden = parent2.nodes.Where(n => n.type == NodeType.Hidden).ToArray();

            // Combine all unique hidden nodes
            var allHiddenIds = p1Hidden
                .Select(n => n.id)
                .Union(p2Hidden.Select(n => n.id))
                .ToHashSet();

            foreach (int hiddenId in allHiddenIds)
            {
                var node1 = p1Hidden.FirstOrDefault(n => n.id == hiddenId);
                var node2 = p2Hidden.FirstOrDefault(n => n.id == hiddenId);

                // Add to offspring based on availability and random choice
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
        }

        static void InheritInputToHiddenConnections(
            NEATGenome parent1,
            NEATGenome parent2,
            NEATGenome offspring1,
            NEATGenome offspring2
        )
        {
            var p1InputConnections = parent1
                .connections.Where(c => c.inputId < FIXED_INPUT_COUNT)
                .ToArray();
            var p2InputConnections = parent2
                .connections.Where(c => c.inputId < FIXED_INPUT_COUNT)
                .ToArray();

            // Group by innovation for matching
            var p1Dict = p1InputConnections.ToDictionary(c => c.innovation, c => c);
            var p2Dict = p2InputConnections.ToDictionary(c => c.innovation, c => c);

            var allInnovations = p1Dict.Keys.Union(p2Dict.Keys);

            foreach (int innovation in allInnovations)
            {
                bool hasP1 = p1Dict.ContainsKey(innovation);
                bool hasP2 = p2Dict.ContainsKey(innovation);

                if (hasP1 && hasP2) // Matching connections
                {
                    var conn1 = Random.value < 0.5f ? p1Dict[innovation] : p2Dict[innovation];
                    var conn2 = Random.value < 0.5f ? p1Dict[innovation] : p2Dict[innovation];

                    if (NodeExists(offspring1, conn1.outputId))
                        offspring1.AddConnection(conn1);
                    if (NodeExists(offspring2, conn2.outputId))
                        offspring2.AddConnection(conn2);
                }
                else // Disjoint connections
                {
                    var conn = hasP1 ? p1Dict[innovation] : p2Dict[innovation];

                    if (Random.value < 0.5f && NodeExists(offspring1, conn.outputId))
                        offspring1.AddConnection(conn);
                    if (Random.value < 0.5f && NodeExists(offspring2, conn.outputId))
                        offspring2.AddConnection(conn);
                }
            }
        }

        static void RebuildOutputConnections(NEATGenome parent, NEATGenome offspring)
        {
            var parentOutputs = parent.GetOutputNodes();
            var offspringOutputs = offspring.GetOutputNodes();
            var offspringHiddens = offspring.nodes.Where(n => n.type == NodeType.Hidden).ToArray();

            if (parentOutputs.Length == 0 || offspringOutputs.Length == 0)
                return;

            // For each hidden node, map its output connections proportionally
            foreach (var hiddenNode in offspringHiddens)
            {
                var hiddenOutputConnections = parent
                    .connections.Where(c =>
                        c.inputId == hiddenNode.id
                        && parent.nodes.Any(n => n.id == c.outputId && n.type == NodeType.Output)
                    )
                    .ToArray();

                if (hiddenOutputConnections.Length == 0)
                    continue;

                // Distribute connections across new outputs using round-robin
                for (int i = 0; i < offspringOutputs.Length; i++)
                {
                    var sourceConnection = hiddenOutputConnections[
                        i % hiddenOutputConnections.Length
                    ];

                    // Divide weight to maintain total effect strength
                    float adjustedWeight =
                        sourceConnection.weight
                        / (float)offspringOutputs.Length
                        * hiddenOutputConnections.Length;

                    int innovation = InnovationManager.Instance.GetConnectionInnovation(
                        hiddenNode.id,
                        offspringOutputs[i].id
                    );

                    offspring.AddConnection(
                        new ConnectionGene(
                            hiddenNode.id,
                            offspringOutputs[i].id,
                            adjustedWeight,
                            innovation,
                            sourceConnection.enabled
                        )
                    );
                }
            }
        }

        static bool NodeExists(NEATGenome genome, int nodeId)
        {
            return genome.nodes.Any(n => n.id == nodeId);
        }

        static void MutateGenome(NEATGenome genome)
        {
            // Structural mutations first (lower probability)
            if (Random.value < 0.03f) // 3% chance
                AddNodeMutation(genome);

            if (Random.value < 0.05f) // 5% chance
                AddConnectionMutation(genome);

            if (Random.value < 0.01f) // 1% chance
                ToggleConnectionMutation(genome);

            // Weight mutations (higher probability)
            for (int i = 0; i < genome.connections.Count; i++)
            {
                if (Random.value < MUTATION_RATE)
                {
                    var conn = genome.connections[i];
                    conn.weight += Random.Range(-0.5f, 0.5f);
                    conn.weight = Mathf.Clamp(conn.weight, -3f, 3f);
                    genome.connections[i] = conn;
                }
            }

            // Bias mutations
            for (int i = 0; i < genome.nodes.Count; i++)
            {
                if (Random.value < MUTATION_RATE)
                {
                    var node = genome.nodes[i];
                    node.bias += Random.Range(-0.3f, 0.3f);
                    node.bias = Mathf.Clamp(node.bias, -2f, 2f);
                    genome.nodes[i] = node;
                }
            }
        }

        static void AddNodeMutation(NEATGenome genome)
        {
            var enabledConnections = genome.connections.Where(c => c.enabled).ToArray();
            if (enabledConnections.Length == 0)
                return;

            // Select random connection to split
            var connectionToSplit = enabledConnections[Random.Range(0, enabledConnections.Length)];

            // Get new node ID from innovation manager
            int newNodeId = InnovationManager.Instance.GetNodeSplitId(connectionToSplit.innovation);

            // Add new hidden node
            genome.AddNode(new NodeGeneNEAT(newNodeId, NodeType.Hidden, Random.Range(-0.5f, 0.5f)));

            // Disable original connection
            for (int i = 0; i < genome.connections.Count; i++)
            {
                if (genome.connections[i].innovation == connectionToSplit.innovation)
                {
                    var conn = genome.connections[i];
                    conn.enabled = false;
                    genome.connections[i] = conn;
                    break;
                }
            }

            // Add two new connections
            int innovation1 = InnovationManager.Instance.GetConnectionInnovation(
                connectionToSplit.inputId,
                newNodeId
            );
            int innovation2 = InnovationManager.Instance.GetConnectionInnovation(
                newNodeId,
                connectionToSplit.outputId
            );

            genome.AddConnection(
                new ConnectionGene(connectionToSplit.inputId, newNodeId, 1.0f, innovation1, true)
            );
            genome.AddConnection(
                new ConnectionGene(
                    newNodeId,
                    connectionToSplit.outputId,
                    connectionToSplit.weight,
                    innovation2,
                    true
                )
            );
        }

        static void AddConnectionMutation(NEATGenome genome)
        {
            var allNodes = genome.nodes.ToArray();
            if (allNodes.Length < 2)
                return;

            // Try to find valid connection (not already existing)
            for (int attempt = 0; attempt < 20; attempt++)
            {
                var inputNode = allNodes[Random.Range(0, allNodes.Length)];
                var outputNode = allNodes[Random.Range(0, allNodes.Length)];

                // Valid connection rules
                if (inputNode.id == outputNode.id)
                    continue; // No self-connections
                if (inputNode.type == NodeType.Output)
                    continue; // Outputs can't be inputs
                if (outputNode.type == NodeType.Input)
                    continue; // Inputs can't be outputs

                // Check if connection already exists
                bool connectionExists = genome.connections.Any(c =>
                    c.inputId == inputNode.id && c.outputId == outputNode.id
                );

                if (!connectionExists)
                {
                    int innovation = InnovationManager.Instance.GetConnectionInnovation(
                        inputNode.id,
                        outputNode.id
                    );
                    genome.AddConnection(
                        new ConnectionGene(
                            inputNode.id,
                            outputNode.id,
                            Random.Range(-2f, 2f),
                            innovation,
                            true
                        )
                    );
                    break;
                }
            }
        }

        static void ToggleConnectionMutation(NEATGenome genome)
        {
            if (genome.connections.Count == 0)
                return;

            int index = Random.Range(0, genome.connections.Count);
            var conn = genome.connections[index];
            conn.enabled = !conn.enabled;
            genome.connections[index] = conn;
        }
    }
}

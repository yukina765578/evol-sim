using EvolutionSimulator.Creature;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class RandomGeneGenerator
    {
        private const int MIN_NODES = 2;
        private const int MAX_NODES = 20;
        private const int BRAIN_INPUTS = 12;

        public static CreatureGenome GenerateRandomGenome()
        {
            var (bodyGenome, _) = GenerateRandomGenomeWithBrain();
            return bodyGenome;
        }

        public static (CreatureGenome, NEATGenome) GenerateRandomGenomeWithBrain()
        {
            CreatureGenome body = GenerateRandomBodyGenome();
            NEATGenome brain = GenerateRandomBrain(body.NodeCount - 1);
            return (body, brain);
        }

        public static CreatureGenome GenerateRandomBodyGenome()
        {
            int nodeCount = Random.Range(MIN_NODES, MAX_NODES + 1);
            NodeGene[] nodes = new NodeGene[nodeCount];

            nodes[0] = new NodeGene(-1, 0f, 0f); // Root node

            for (int i = 1; i < nodeCount; i++)
            {
                nodes[i] = new NodeGene(
                    Random.Range(0, i), // Parent index
                    Random.Range(0f, 360f), // Base angle
                    Random.Range(-180f, 180f) // Max angle
                );
            }

            return new CreatureGenome(nodes);
        }

        // Update the GenerateRandomBrain method in RandomGeneGenerator.cs

        public static NEATGenome GenerateRandomBrain(int outputCount)
        {
            NEATGenome brain = new NEATGenome(BRAIN_INPUTS, outputCount);

            // Connect inputs to outputs using InnovationManager
            for (int output = 0; output < outputCount; output++)
            {
                int outputNodeId = BRAIN_INPUTS + output;

                for (int input = 0; input < BRAIN_INPUTS; input++)
                {
                    if (Random.value < 0.7f) // 70% connection chance
                    {
                        int innovation = InnovationManager.Instance.GetConnectionInnovation(
                            input,
                            outputNodeId
                        );
                        brain.AddConnection(
                            new ConnectionGene(
                                input,
                                outputNodeId,
                                Random.Range(-2f, 2f),
                                innovation
                            )
                        );
                    }
                }
            }

            // Add hidden nodes using InnovationManager
            int hiddenCount = Random.Range(0, 3);
            Debug.Log($"Generating brain with {hiddenCount} hidden nodes");
            for (int h = 0; h < hiddenCount; h++)
            {
                int hiddenId = InnovationManager.Instance.GetNextNodeId();
                Debug.Log($"Creating hidden node with ID: {hiddenId}");
                brain.AddNode(new NodeGeneNEAT(hiddenId, NodeType.Hidden, Random.Range(-1f, 1f)));

                // Input to hidden connections
                for (int input = 0; input < BRAIN_INPUTS; input++)
                {
                    if (Random.value < 0.5f)
                    {
                        int innovation = InnovationManager.Instance.GetConnectionInnovation(
                            input,
                            hiddenId
                        );
                        brain.AddConnection(
                            new ConnectionGene(input, hiddenId, Random.Range(-2f, 2f), innovation)
                        );
                    }
                }

                // Hidden to output connections
                for (int output = 0; output < outputCount; output++)
                {
                    if (Random.value < 0.5f)
                    {
                        int outputNodeId = BRAIN_INPUTS + output;
                        int innovation = InnovationManager.Instance.GetConnectionInnovation(
                            hiddenId,
                            outputNodeId
                        );
                        brain.AddConnection(
                            new ConnectionGene(
                                hiddenId,
                                outputNodeId,
                                Random.Range(-2f, 2f),
                                innovation
                            )
                        );
                    }
                }
            }

            return brain;
        }

        public static CreatureGenome GenerateMinimalGenome()
        {
            return new CreatureGenome(
                new NodeGene[] { new NodeGene(-1, 0f, 0f), new NodeGene(0, 0f, 45f) }
            );
        }

        public static (CreatureGenome, NEATGenome) GenerateMinimalGenomeWithBrain()
        {
            CreatureGenome body = GenerateMinimalGenome();
            NEATGenome brain = GenerateRandomBrain(1);
            return (body, brain);
        }

        public static CreatureGenome GenerateTestGenome()
        {
            return new CreatureGenome(
                new NodeGene[]
                {
                    new NodeGene(-1, 0f, 0f),
                    new NodeGene(0, 30f, 45f),
                    new NodeGene(1, 60f, -30f),
                    new NodeGene(1, -45f, 60f),
                    new NodeGene(2, 90f, -90f),
                }
            );
        }

        public static (CreatureGenome, NEATGenome) GenerateTestGenomeWithBrain()
        {
            CreatureGenome body = GenerateTestGenome();
            NEATGenome brain = GenerateRandomBrain(4);
            return (body, brain);
        }
    }
}

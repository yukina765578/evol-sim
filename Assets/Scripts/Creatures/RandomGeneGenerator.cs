using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public static class RandomGeneGenerator
    {
        private const int MIN_NODES = 2;
        private const int MAX_NODES = 20;
        private const int BRAIN_INPUTS = 2; // food distance, energy

        // Original method for backward compatibility
        public static CreatureGenome GenerateRandomGenome()
        {
            var (bodyGenome, _) = GenerateRandomGenomeWithBrain();
            return bodyGenome;
        }

        // New method that generates both body and brain
        public static (CreatureGenome, NEATGenome) GenerateRandomGenomeWithBrain()
        {
            CreatureGenome bodyGenome = GenerateRandomBodyGenome();
            int segmentCount = bodyGenome.NodeCount - 1;
            NEATGenome brainGenome = GenerateRandomBrain(segmentCount);
            return (bodyGenome, brainGenome);
        }

        public static CreatureGenome GenerateRandomBodyGenome()
        {
            int nodeCount = Random.Range(MIN_NODES, MAX_NODES + 1);
            NodeGene[] nodes = new NodeGene[nodeCount];

            nodes[0] = new NodeGene(-1, 0f, 0f);

            for (int i = 1; i < nodeCount; i++)
            {
                int parentIndex = Random.Range(0, i);
                nodes[i] = new NodeGene(
                    parentIndex,
                    Random.Range(0f, 360f), // baseAngle
                    Random.Range(-180f, 180f) // maxAngle
                );
            }

            return new CreatureGenome(nodes);
        }

        public static NEATGenome GenerateRandomBrain(int outputCount)
        {
            NEATGenome brain = new NEATGenome(BRAIN_INPUTS, outputCount);

            // Add random connections between inputs and outputs
            for (int output = 0; output < outputCount; output++)
            {
                int outputNodeId = BRAIN_INPUTS + output;

                // Connect each input to this output with random probability
                for (int input = 0; input < BRAIN_INPUTS; input++)
                {
                    if (Random.value < 0.7f) // 70% chance of connection
                    {
                        brain.AddConnection(
                            new ConnectionGene(
                                input,
                                outputNodeId,
                                Random.Range(-2f, 2f),
                                input * outputCount + output,
                                true
                            )
                        );
                    }
                }
            }

            // Add some hidden nodes and connections for complexity
            int hiddenNodeCount = Random.Range(0, 3);
            for (int h = 0; h < hiddenNodeCount; h++)
            {
                int hiddenId = BRAIN_INPUTS + outputCount + h;
                brain.AddNode(new NodeGeneNEAT(hiddenId, NodeType.Hidden, Random.Range(-1f, 1f)));

                // Connect inputs to hidden
                for (int input = 0; input < BRAIN_INPUTS; input++)
                {
                    if (Random.value < 0.5f)
                    {
                        brain.AddConnection(
                            new ConnectionGene(
                                input,
                                hiddenId,
                                Random.Range(-2f, 2f),
                                1000 + h * BRAIN_INPUTS + input,
                                true
                            )
                        );
                    }
                }

                // Connect hidden to outputs
                for (int output = 0; output < outputCount; output++)
                {
                    if (Random.value < 0.5f)
                    {
                        brain.AddConnection(
                            new ConnectionGene(
                                hiddenId,
                                BRAIN_INPUTS + output,
                                Random.Range(-2f, 2f),
                                2000 + h * outputCount + output,
                                true
                            )
                        );
                    }
                }
            }

            return brain;
        }

        public static CreatureGenome GenerateMinimalGenome()
        {
            NodeGene[] nodes = new NodeGene[2];
            nodes[0] = new NodeGene(-1, 0f, 0f);
            nodes[1] = new NodeGene(0, 0f, 45f);
            return new CreatureGenome(nodes);
        }

        public static (CreatureGenome, NEATGenome) GenerateMinimalGenomeWithBrain()
        {
            CreatureGenome bodyGenome = GenerateMinimalGenome();
            NEATGenome brainGenome = GenerateRandomBrain(1); // 1 segment
            return (bodyGenome, brainGenome);
        }

        public static CreatureGenome GenerateTestGenome()
        {
            NodeGene[] nodes = new NodeGene[5];
            nodes[0] = new NodeGene(-1, 0f, 0f);
            nodes[1] = new NodeGene(0, 30f, 45f);
            nodes[2] = new NodeGene(1, 60f, -30f);
            nodes[3] = new NodeGene(1, -45f, 60f);
            nodes[4] = new NodeGene(2, 90f, -90f);
            return new CreatureGenome(nodes);
        }

        public static (CreatureGenome, NEATGenome) GenerateTestGenomeWithBrain()
        {
            CreatureGenome bodyGenome = GenerateTestGenome();
            NEATGenome brainGenome = GenerateRandomBrain(4); // 4 segments
            return (bodyGenome, brainGenome);
        }
    }
}

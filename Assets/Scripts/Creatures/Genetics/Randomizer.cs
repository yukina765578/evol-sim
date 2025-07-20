using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    public static class Randomizer
    {
        public static CreatureGenome GenerateRandomGenome()
        {
            int nodeCount = Random.Range(GeneticsConstants.MIN_NODES, GeneticsConstants.MAX_NODES);
            NodeGene[] nodes = new NodeGene[nodeCount];

            // Create root node
            nodes[0] = new NodeGene(-1, 0f, 0f);
            InitializeSegmentBrainWeights(ref nodes[0]);

            // Create child nodes
            for (int i = 1; i < nodeCount; i++)
            {
                int parentIndex = Random.Range(0, i);
                nodes[i] = new NodeGene(
                    parentIndex,
                    Random.Range(
                        GeneticsConstants.MIN_BASE_ANGLE,
                        GeneticsConstants.MAX_BASE_ANGLE
                    ),
                    Random.Range(GeneticsConstants.MIN_MAX_ANGLE, GeneticsConstants.MAX_MAX_ANGLE)
                );
                InitializeSegmentBrainWeights(ref nodes[i]);
            }

            CreatureGenome genome = new CreatureGenome(nodes);
            InitializeMainBrainWeights(genome);

            return genome;
        }

        static void InitializeMainBrainWeights(CreatureGenome genome)
        {
            for (int i = 0; i < GeneticsConstants.MAIN_BRAIN_WEIGHTS; i++)
            {
                genome.mainBrainWeights[i] = Random.Range(
                    GeneticsConstants.MIN_NEURAL_WEIGHT,
                    GeneticsConstants.MAX_NEURAL_WEIGHT
                );
            }
        }

        static void InitializeSegmentBrainWeights(ref NodeGene node)
        {
            for (int i = 0; i < GeneticsConstants.SEGMENT_BRAIN_WEIGHTS; i++)
            {
                node.segmentBrainWeights[i] = Random.Range(
                    GeneticsConstants.MIN_NEURAL_WEIGHT,
                    GeneticsConstants.MAX_NEURAL_WEIGHT
                );
            }
        }
    }
}

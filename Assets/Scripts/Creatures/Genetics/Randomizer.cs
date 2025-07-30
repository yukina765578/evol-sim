using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    public static class Randomizer
    {
        public static CreatureGenome GenerateRandomGenome(int min_nodes = GeneticsConstants.MIN_NODES, int max_nodes = GeneticsConstants.MAX_NODES)
        {
            int nodeCount = Random.Range(min_nodes, max_nodes);
            NodeGene[] nodes = new NodeGene[nodeCount];

            nodes[0] = new NodeGene(-1, 0f, 0f, 0f, 0f);

            for (int i = 1; i < nodeCount; i++)
            {
                int parentIndex = Random.Range(0, i);
                nodes[i] = new NodeGene(
                    parentIndex,
                    Random.Range(
                        GeneticsConstants.MIN_BASE_ANGLE,
                        GeneticsConstants.MAX_BASE_ANGLE
                    ), // base angle
                    Random.Range(GeneticsConstants.MIN_OSC_SPEED, GeneticsConstants.MAX_OSC_SPEED), // oscillation speed
                    Random.Range(GeneticsConstants.MIN_MAX_ANGLE, GeneticsConstants.MAX_MAX_ANGLE), // max angle
                    Random.Range(
                        GeneticsConstants.MIN_FORWARD_RATIO,
                        GeneticsConstants.MAX_FORWARD_RATIO
                    ) // forward ratio
                );
            }

            return new CreatureGenome(nodes);
        }
    }
}

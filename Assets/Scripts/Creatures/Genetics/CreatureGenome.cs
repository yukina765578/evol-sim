using System;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    [Serializable]
    public class CreatureGenome
    {
        public NodeGene[] nodes;

        // Main brain neural network weights
        // 10 inputs -> 2 outputs = 2 perceptrons x 11 values (10 weights + 1 bias)
        public float[] mainBrainWeights; // 22 values

        public CreatureGenome(NodeGene[] nodeArray)
        {
            nodes = nodeArray ?? new NodeGene[1] { new NodeGene(-1, 0f, 0f) }; // ✅ 3 parameters
            mainBrainWeights = new float[GeneticsConstants.MAIN_BRAIN_WEIGHTS];
        }

        public int NodeCount => nodes.Length;
        public NodeGene RootNode => nodes.Length > 0 ? nodes[0] : new NodeGene(-1, 0f, 0f); // ✅ 3 parameters
    }
}

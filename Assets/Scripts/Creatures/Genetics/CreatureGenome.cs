using System;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    [Serializable]
    public class CreatureGenome
    {
        public NodeGene[] nodes;

        public CreatureGenome(NodeGene[] nodeArray)
        {
            nodes = nodeArray ?? new NodeGene[1] { new NodeGene(-1, 0, 0, 0, 0) };
            // Optionally validate
        }

        public int NodeCount => nodes.Length;
        public NodeGene RootNode => nodes.Length > 0 ? nodes[0] : new NodeGene(-1, 0, 0, 0, 0);
    }
}

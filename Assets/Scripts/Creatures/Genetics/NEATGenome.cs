using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    [System.Serializable]
    public enum NodeType
    {
        Input,
        Hidden,
        Output,
    }

    [System.Serializable]
    public struct NodeGeneNEAT
    {
        public int id;
        public NodeType type;
        public float bias;

        public NodeGeneNEAT(int nodeId, NodeType nodeType, float nodeBias = 0f)
        {
            this.id = nodeId;
            this.type = nodeType;
            this.bias = nodeBias;
        }
    }

    [System.Serializable]
    public struct ConnectionGene
    {
        public int inputId;
        public int outputId;
        public float weight;
        public bool enabled;
        public int innovation;

        public ConnectionGene(int input, int output, float w, int innov, bool isEnabled = true)
        {
            this.inputId = input;
            this.outputId = output;
            this.weight = w;
            this.innovation = innov;
            this.enabled = isEnabled;
        }
    }

    [System.Serializable]
    public class NEATGenome
    {
        public List<NodeGeneNEAT> nodes = new List<NodeGeneNEAT>();
        public List<ConnectionGene> connections = new List<ConnectionGene>();

        public int NodeCount => nodes.Count;
        public int ConnectionCount => connections.Count;

        public NEATGenome()
        {
            // Empty constructor for crossover
        }

        public NEATGenome(int inputs, int outputs)
        {
            for (int i = 0; i < inputs; i++)
                nodes.Add(new NodeGeneNEAT(i, NodeType.Input));

            for (int i = 0; i < outputs; i++)
                nodes.Add(new NodeGeneNEAT(inputs + i, NodeType.Output));
        }

        public void AddNode(NodeGeneNEAT node)
        {
            if (!nodes.Any(n => n.id == node.id))
                nodes.Add(node);
        }

        public void AddConnection(ConnectionGene connection)
        {
            connections.Add(connection);
        }

        public NodeGeneNEAT[] GetInputNodes()
        {
            return nodes.Where(n => n.type == NodeType.Input).ToArray();
        }

        public NodeGeneNEAT[] GetOutputNodes()
        {
            return nodes.Where(n => n.type == NodeType.Output).ToArray();
        }

        public ConnectionGene[] GetActiveConnections()
        {
            return connections.Where(c => c.enabled).ToArray();
        }

        public NEATGenome Clone()
        {
            NEATGenome clone = new NEATGenome();
            clone.nodes = new List<NodeGeneNEAT>(nodes);
            clone.connections = new List<ConnectionGene>(connections);
            return clone;
        }
    }
}

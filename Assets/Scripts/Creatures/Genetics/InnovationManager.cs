using System.Collections.Generic;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    [System.Serializable]
    public struct ConnectionKey
    {
        public int inputId;
        public int outputId;

        public ConnectionKey(int input, int output)
        {
            inputId = input;
            outputId = output;
        }

        public override bool Equals(object obj)
        {
            if (obj is ConnectionKey other)
                return inputId == other.inputId && outputId == other.outputId;
            return false;
        }

        public override int GetHashCode()
        {
            return inputId * 10000 + outputId;
        }
    }

    [System.Serializable]
    public struct NodeSplitKey
    {
        public int connectionInnovation;

        public NodeSplitKey(int innovation)
        {
            connectionInnovation = innovation;
        }

        public override bool Equals(object obj)
        {
            if (obj is NodeSplitKey other)
                return connectionInnovation == other.connectionInnovation;
            return false;
        }

        public override int GetHashCode()
        {
            return connectionInnovation;
        }
    }

    public class InnovationManager
    {
        private static InnovationManager instance;
        public static InnovationManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new InnovationManager();
                return instance;
            }
        }

        // Global counters
        private int nextInnovationNumber = 10000;
        private int nextNodeId = 1000;

        // Historical tracking
        private Dictionary<ConnectionKey, int> connectionInnovations =
            new Dictionary<ConnectionKey, int>();
        private Dictionary<NodeSplitKey, int> nodeSplitInnovations =
            new Dictionary<NodeSplitKey, int>();

        private InnovationManager()
        {
            Reset();
        }

        public void Reset()
        {
            nextInnovationNumber = 10000;
            nextNodeId = 1000;
            connectionInnovations.Clear();
            nodeSplitInnovations.Clear();
        }

        public int GetConnectionInnovation(int inputId, int outputId)
        {
            ConnectionKey key = new ConnectionKey(inputId, outputId);

            if (connectionInnovations.ContainsKey(key))
            {
                return connectionInnovations[key];
            }
            else
            {
                int innovation = nextInnovationNumber++;
                connectionInnovations[key] = innovation;
                return innovation;
            }
        }

        public int GetNodeSplitId(int connectionInnovation)
        {
            NodeSplitKey key = new NodeSplitKey(connectionInnovation);

            if (nodeSplitInnovations.ContainsKey(key))
            {
                return nodeSplitInnovations[key];
            }
            else
            {
                int nodeId = nextNodeId++;
                nodeSplitInnovations[key] = nodeId;
                return nodeId;
            }
        }

        public int GetNextNodeId()
        {
            return nextNodeId++;
        }

        // Debug information
        public int TotalConnectionInnovations => connectionInnovations.Count;
        public int TotalNodeInnovations => nodeSplitInnovations.Count;
        public int CurrentInnovationNumber => nextInnovationNumber;
        public int CurrentNodeId => nextNodeId;

        public void LogStats()
        {
            Debug.Log(
                $"Innovation Manager Stats - Connections: {TotalConnectionInnovations}, "
                    + $"Nodes: {TotalNodeInnovations}, Next Innovation: {CurrentInnovationNumber}, "
                    + $"Next Node ID: {CurrentNodeId}"
            );
        }
    }
}

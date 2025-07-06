using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class CreatureBrain : MonoBehaviour
    {
        [Header("Brain Settings")]
        [SerializeField]
        private NEATGenome genome;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugValues = false;

        private Dictionary<int, float> nodeValues = new Dictionary<int, float>();
        private FoodDetector foodDetector;
        private CreatureEnergy creatureEnergy;
        private Segment[] segments;

        public void Initialize(NEATGenome brainGenome)
        {
            genome = brainGenome;
            foodDetector = GetComponentInChildren<FoodDetector>();
            creatureEnergy = GetComponent<CreatureEnergy>();
            segments = GetComponentsInChildren<Segment>();
        }

        void Update()
        {
            if (genome == null || !creatureEnergy.IsAlive)
                return;

            ProcessNetwork();
            ApplyOutputs();
        }

        void ProcessNetwork()
        {
            nodeValues.Clear();

            // Set input values
            SetInputs();

            // Process each node in topological order
            var sortedNodes = TopologicalSort();
            foreach (var node in sortedNodes)
            {
                if (node.type == NodeType.Input)
                    continue;

                float sum = node.bias;
                var incomingConnections = genome
                    .GetActiveConnections()
                    .Where(c => c.outputId == node.id);

                foreach (var conn in incomingConnections)
                {
                    if (nodeValues.ContainsKey(conn.inputId))
                        sum += nodeValues[conn.inputId] * conn.weight;
                }

                nodeValues[node.id] = ActivationFunction(sum);
            }
        }

        void SetInputs()
        {
            var inputNodes = genome.GetInputNodes();

            if (inputNodes.Length >= 2)
            {
                // Input 0: Food distance (normalized)
                float foodDistance = GetNearestFoodDistance();
                nodeValues[inputNodes[0].id] = Mathf.Clamp01(foodDistance / 10f);

                // Input 1: Energy ratio
                nodeValues[inputNodes[1].id] = creatureEnergy.EnergyRatio;
            }
        }

        void ApplyOutputs()
        {
            var outputNodes = genome.GetOutputNodes();

            for (int i = 0; i < segments.Length && i < outputNodes.Length; i++)
            {
                if (nodeValues.ContainsKey(outputNodes[i].id))
                {
                    float output = nodeValues[outputNodes[i].id]; // 0 to 1 from sigmoid
                    float coefficient = (output - 0.5f) * 2f; // Map to -1 to 1
                    segments[i].SetTargetAngle(coefficient);
                }
            }
        }

        NodeGeneNEAT[] TopologicalSort()
        {
            // Simple sort: Input -> Hidden -> Output
            return genome.nodes.OrderBy(n => (int)n.type).ToArray();
        }

        float ActivationFunction(float input)
        {
            return 1f / (1f + Mathf.Exp(-input)); // Sigmoid
        }

        float GetNearestFoodDistance()
        {
            // Placeholder - implement food detection
            return Random.Range(1f, 10f);
        }
    }
}

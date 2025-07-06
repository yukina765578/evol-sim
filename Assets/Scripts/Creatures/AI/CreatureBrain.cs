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

        private Dictionary<int, float> nodeValues = new Dictionary<int, float>();
        private CreatureSensor sensor;
        private CreatureEnergy energy;
        private Rigidbody2D rb;
        private Segment[] segments;

        public void Initialize(NEATGenome brainGenome)
        {
            genome = brainGenome;
            sensor = GetComponent<CreatureSensor>();
            energy = GetComponent<CreatureEnergy>();
            rb = GetComponent<Rigidbody2D>();
            segments = GetComponentsInChildren<Segment>();
        }

        void Start()
        {
            segments = GetComponentsInChildren<Segment>();
        }

        void Update()
        {
            if (genome == null || !energy.IsAlive)
                return;

            ProcessNetwork();
            ApplyOutputs();
        }

        void ProcessNetwork()
        {
            nodeValues.Clear();
            SetInputs();

            var sortedNodes = genome.nodes.OrderBy(n => (int)n.type).ToArray();

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

                nodeValues[node.id] = Sigmoid(sum);
            }
        }

        void SetInputs()
        {
            var inputNodes = genome.GetInputNodes();

            if (inputNodes.Length >= 12)
            {
                nodeValues[inputNodes[0].id] = sensor.FoodDistance;
                nodeValues[inputNodes[1].id] = sensor.FoodDirection;
                nodeValues[inputNodes[2].id] = sensor.FoodDetected;
                nodeValues[inputNodes[3].id] = energy.EnergyRatio;
                nodeValues[inputNodes[4].id] = sensor.OthersDistance;
                nodeValues[inputNodes[5].id] = sensor.OthersDirection;
                nodeValues[inputNodes[6].id] = sensor.OthersDetected;
                nodeValues[inputNodes[7].id] = sensor.MateDistance;
                nodeValues[inputNodes[8].id] = sensor.MateDirection;
                nodeValues[inputNodes[9].id] = sensor.MateDetected;

                // Add this at the end of SetInputs() method in CreatureBrain.cs

                // DEBUG: Override with random values for testing
                if (inputNodes.Length >= 2)
                {
                    nodeValues[inputNodes[0].id] = Random.Range(0f, 1f); // Random food detection
                    nodeValues[inputNodes[1].id] = Random.Range(0f, 1f); // Random food direction
                }

                // Velocity inputs
                Vector2 velocity = rb.linearVelocity;
                nodeValues[inputNodes[10].id] = Mathf.Clamp01(velocity.magnitude / 10f);

                if (velocity.magnitude > 0.1f)
                {
                    float moveAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
                    float relativeAngle = Mathf.DeltaAngle(transform.eulerAngles.z, moveAngle);
                    nodeValues[inputNodes[11].id] = Mathf.Clamp(relativeAngle / 180f, -1f, 1f);
                }
                else
                {
                    nodeValues[inputNodes[11].id] = 0f;
                }
            }
        }

        void ApplyOutputs()
        {
            var outputNodes = genome.GetOutputNodes();

            for (int i = 0; i < segments.Length && i < outputNodes.Length; i++)
            {
                if (nodeValues.ContainsKey(outputNodes[i].id))
                {
                    float output = nodeValues[outputNodes[i].id];
                    float coefficient = (output - 0.5f) * 2f; // Map 0-1 to -1 to 1
                    segments[i].SetTargetAngle(coefficient);
                }
            }
        }

        float Sigmoid(float input)
        {
            return 1f / (1f + Mathf.Exp(-input));
        }
    }
}

using EvolutionSimulator.Creatures.Genetics;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class SegmentBrain : MonoBehaviour
    {
        private float[] weights; // 14 weights from gene
        private MainBrain mainBrain;
        private Node parentNode;
        private Vector3 lastParentPosition;

        // Current outputs
        public float OscillationSpeed { get; private set; } // 1.5 to 8.0
        public float ForwardRatio { get; private set; } // 0.01 to 0.99

        public void Initialize(NodeGene gene, MainBrain brain, Node parent = null)
        {
            weights = new float[GeneticsConstants.SEGMENT_BRAIN_WEIGHTS];
            System.Array.Copy(gene.segmentBrainWeights, weights, weights.Length);

            mainBrain = brain;
            parentNode = parent;
            lastParentPosition = parentNode?.transform.position ?? Vector3.zero;
        }

        void Update()
        {
            if (weights == null || mainBrain == null)
                return;

            float[] inputs = CollectInputs();
            ExecuteNeuralNetwork(inputs);
        }

        float[] CollectInputs()
        {
            Vector3 currentParentPos = parentNode?.transform.position ?? Vector3.zero;
            Vector3 parentDelta = currentParentPos - lastParentPosition;
            lastParentPosition = currentParentPos;

            return new float[GeneticsConstants.SEGMENT_BRAIN_INPUTS]
            {
                mainBrain.MovementDirection / (2f * Mathf.PI), // desiredDirection (0 to 1)
                mainBrain.MovementIntensity, // desiredIntensity (0 to 1)
                0f, // currentJointAngle (TODO: get from segment)
                0f, // localContact (TODO: implement contact detection)
                parentDelta.x, // parentPositionDeltaX
                parentDelta.y, // parentPositionDeltaY
            };
        }

        void ExecuteNeuralNetwork(float[] inputs)
        {
            // Perceptron 1: Oscillation Speed
            float speedSum = weights[6]; // bias
            for (int i = 0; i < 6; i++)
                speedSum += inputs[i] * weights[i];
            OscillationSpeed = Mathf.Lerp(
                GeneticsConstants.MIN_OSC_SPEED,
                GeneticsConstants.MAX_OSC_SPEED,
                Sigmoid(speedSum)
            );

            // Perceptron 2: Forward Ratio
            float ratioSum = weights[13]; // bias
            for (int i = 0; i < 6; i++)
                ratioSum += inputs[i] * weights[i + 7];
            ForwardRatio = Mathf.Lerp(
                GeneticsConstants.MIN_FORWARD_RATIO,
                GeneticsConstants.MAX_FORWARD_RATIO,
                Sigmoid(ratioSum)
            );
        }

        float Sigmoid(float x) => 1f / (1f + Mathf.Exp(-x));
    }
}

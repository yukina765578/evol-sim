using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    [System.Serializable]
    public struct NodeGene
    {
        public int parentIndex; // -1 for root, 0-(n-1) for sequential connections
        public float baseAngle; // 0-360 degrees
        public float maxAngle; // -180 to 180 degrees

        // Neural Network Parameters
        // 6 inputs -> 2 outputs = 2 perceptrons x 7 values (6 weights + 1 bias)
        public float[] segmentBrainWeights; // 14 values

        public NodeGene(int parent, float angle, float max)
        {
            parentIndex = parent;
            if (parent == -1)
            {
                baseAngle = 0f;
                maxAngle = 0f;
                segmentBrainWeights = new float[GeneticsConstants.SEGMENT_BRAIN_WEIGHTS];
            }
            else
            {
                baseAngle = Mathf.Clamp(
                    angle,
                    GeneticsConstants.MIN_BASE_ANGLE,
                    GeneticsConstants.MAX_BASE_ANGLE
                );
                maxAngle = Mathf.Clamp(
                    max,
                    GeneticsConstants.MIN_MAX_ANGLE,
                    GeneticsConstants.MIN_MAX_ANGLE
                );
                segmentBrainWeights = new float[GeneticsConstants.SEGMENT_BRAIN_WEIGHTS];
            }
        }

        public bool IsRoot => parentIndex == -1;

        public bool IsValidAtPosition(int position)
        {
            if (IsRoot)
                return position == 0;
            return parentIndex >= 0 && parentIndex < position;
        }
    }
}

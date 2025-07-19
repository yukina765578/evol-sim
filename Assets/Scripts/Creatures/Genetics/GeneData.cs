using UnityEngine;

namespace EvolutionSimulator.Creatures.Genetics
{
    [System.Serializable]
    public struct NodeGene
    {
        public int parentIndex; // -1 for root, 0-(n-1) for sequential connections
        public float baseAngle; // 0-360 degrees
        public float oscSpeed; // 0.5-8.0 oscillation speed
        public float maxAngle; // -180 to 180 degrees
        public float forwardRatio; // 0.1 to 0.5 forward ratio

        public NodeGene(int parent, float angle, float speed, float max, float ratio)
        {
            parentIndex = parent;
            if (parent == -1)
            {
                baseAngle = 0f;
                oscSpeed = 0f;
                maxAngle = 0f;
                forwardRatio = 0f;
            }
            else
            {
                baseAngle = Mathf.Clamp(
                    angle,
                    GeneticsConstants.MIN_BASE_ANGLE,
                    GeneticsConstants.MAX_BASE_ANGLE
                );
                oscSpeed = Mathf.Clamp(
                    speed,
                    GeneticsConstants.MIN_OSC_SPEED,
                    GeneticsConstants.MAX_OSC_SPEED
                );
                maxAngle = Mathf.Clamp(
                    max,
                    GeneticsConstants.MIN_MAX_ANGLE,
                    GeneticsConstants.MIN_MAX_ANGLE
                );
                forwardRatio = Mathf.Clamp(
                    ratio,
                    GeneticsConstants.MIN_FORWARD_RATIO,
                    GeneticsConstants.MAX_FORWARD_RATIO
                );
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

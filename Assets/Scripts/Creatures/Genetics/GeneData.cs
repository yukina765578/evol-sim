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
        public float forwardRatio; // 0.01-0.99 forward ratio

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
                baseAngle = Mathf.Clamp(angle, 0f, 360f);
                oscSpeed = Mathf.Clamp(speed, 0.5f, 8f);
                maxAngle = Mathf.Clamp(max, -180f, 180f);
                forwardRatio = Mathf.Clamp(ratio, 0.01f, 0.99f);
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

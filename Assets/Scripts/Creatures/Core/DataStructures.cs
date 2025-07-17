using EvolitionSimulator.Creatures.Genetics;
using UnityEngine;

namespace EvolitionSimulator.Creatures.Core
{
    [System.Serializable]
    public class NodeData
    {
        public Vector3 pos;
        public Vector3 prePos;
        public float size;
        public Color color;
        public bool isRoot;
        public int parentIndex;

        public NodeData(
            Vector3 pos,
            Vector3 prePos,
            float size,
            Color color,
            bool isRoot,
            int parentIndex
        )
        {
            this.pos = pos;
            this.prePos = prePos;
            this.size = size;
            this.color = color;
            this.isRoot = isRoot;
            this.parentIndex = parentIndex;
        }

        public Vector2 GetPositionDelta()
        {
            return (Vector2)(pos - prePos);
        }

        public void UpdatePreviousPosition()
        {
            prePos = pos;
        }
    }

    [System.Serializable]
    public struct SegmentData
    {
        public int parentIndex;
        public int childIndex;
        public float length;
        public float width;
        public Color color;

        // Motion parameters
        public float baseAngle;
        public float oscSpeed;
        public float forwardRatio;
        public float currentAngle;
        public float prevAngle;

        public float thrustCoef;

        public SegmentData(
            int parentIndex,
            int childIndex,
            float length,
            float width,
            Color color,
            NodeGene nodeGene
        )
        {
            this.parentIndex = parentIndex;
            this.childIndex = childIndex;
            this.length = length;
            this.width = width;
            this.color = color;

            // Initialize motion parameters
            baseAngle = nodeGene.baseAngle;
            oscSpeed = nodeGene.oscSpeed;
            forwardRatio = nodeGene.forwardRatio;
            currentAngle = 0f;
            prevAngle = 0f;

            thrustCoef = 15f;
        }

        public Vector2 GetThrustDirection(NodeData[] nodes)
        {
            if (
                parentIndex < 0
                || parentIndex >= nodes.Length
                || childIndex < 0
                || childIndex >= nodes.Length
            )
                return Vector2.zero;

            Vector2 parentDelta = nodes[parentIndex].GetPositionDelta();
            Vector2 childDelta = nodes[childIndex].GetPositionDelta();
            Vector2 direction = (childDelta + parentDelta).normalized;
            return -direction;
        }
    }

    public static GetDirection()
    {
        if (parentIndex < 0 || parentIndex >= nodes.Length || childIndex < 0 || childIndex >= nodes.Length)
            return Vector2.zero;
        
        return (nodes[childIndex].pos - nodes[parentIndex].pos).normalized;
    }

    [System.Serializable]
    public struct CreatureData
    {
        public NodeData[] nodes;
        public SegmentData[] segments;
        public bool isInitialized;
        public float totalMass;
        public Vector3 centerOfMass;

        public CreatureState(int nodeCount, int segmentCount)
        {
            nodes = new NodeData[nodeCount];
            segments = new SegmentData[segmentCount];
            isInitialized = false;
            totalMass = 0f;
            centerOfMass = Vector3.zero;
        }

        public void Initialize()
        {
            isInitialized = true;
            CalculateMass();
        }

        public void CalculateMass()
        {
            totalMass = nodes.Length;

            // TODO: Maybe add weight to root node
            Vector3 massCenter = Vector3.zero;
            for (int i = 0; i < nodes.Length; i++)
            {
                massCenter += nodes[i].pos;
            }
            centerOfMass = massCenter / nodes.Length;
        }

        public void UpdateNodePositions()
        {
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].UpdatePreviousPosition();
            }
        }
    }

    public static class DataConstants
    {
        public const float DEFAULT_NODE_SIZE = 1f;
        public const float DEFAULT_SEGMENT_LENGTH = 2f;
        public const float DEFAULT_SEGMENT_WIDTH = 0.5f;
        public const float THRUST_COEFFICIENT = 15f;

        public static readonly Color DEFAULT_NODE_COLOR = Color.blue;
        public static readonly Color DEFAULT_SEGMENT_COLOR = Color.white;
        public static readonly Color DEFAULT_ROOT_COLOR = Color.red;
    }
}

using EvolutionSimulator.Creatures.Genetics;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    [System.Serializable]
    public class NodeData
    {
        public Vector3 position;
        public Vector3 prevPosition;
        public float size;
        public Color color;
        public bool isRoot;
        public int parentIndex;

        public NodeData(
            Vector3 pos,
            Vector3 prevPos,
            float nodeSize,
            Color nodeColor,
            bool root,
            int parent
        )
        {
            position = pos;
            prevPosition = prevPos;
            size = nodeSize;
            color = nodeColor;
            isRoot = root;
            parentIndex = parent;
        }

        public Vector2 GetPositionDelta()
        {
            return (Vector2)(position - prevPosition);
        }

        public void UpdatePreviousPosition()
        {
            prevPosition = position;
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
        public float maxAngle;
        public float forwardRatio;
        public float currentAngle;
        public float prevAngle;
        public float thrustCoef;

        public SegmentData(
            int parent,
            int child,
            float segmentLength,
            float segmentWidth,
            Color segmentColor,
            NodeGene nodeGene
        )
        {
            parentIndex = parent;
            childIndex = child;
            length = segmentLength;
            width = segmentWidth;
            color = segmentColor;

            baseAngle = nodeGene.baseAngle;
            oscSpeed = nodeGene.oscSpeed;
            maxAngle = nodeGene.maxAngle;
            forwardRatio = nodeGene.forwardRatio;
            currentAngle = 0f;
            prevAngle = 0f;
            thrustCoef = 30f;
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

        public Vector2 GetDirection(NodeData[] nodes)
        {
            if (
                parentIndex < 0
                || parentIndex >= nodes.Length
                || childIndex < 0
                || childIndex >= nodes.Length
            )
                return Vector2.zero;

            return (nodes[childIndex].position - nodes[parentIndex].position).normalized;
        }
    }

    [System.Serializable]
    public struct CreatureState
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

            Vector3 massCenter = Vector3.zero;
            for (int i = 0; i < nodes.Length; i++)
            {
                massCenter += nodes[i].position;
            }
            centerOfMass = massCenter / nodes.Length;
        }

        public void UpdatePrevPositions()
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
        public static readonly Color REPRODUCTION_COLOR = Color.red;
    }
}

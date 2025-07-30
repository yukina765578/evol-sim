using UnityEngine;

namespace EvolutionSimulator.Creatures.Rendering
{
    [System.Serializable]
    public class NodeData
    {
        public Vector3 localPosition;
        public Vector3 worldPosition;
        public float size;
        public Color color;
        public int parentIndex; // -1 for root

        public NodeData(Vector3 localPos, float nodeSize, Color nodeColor, int parent = -1)
        {
            localPosition = localPos;
            worldPosition = localPos;
            size = nodeSize;
            color = nodeColor;
            parentIndex = parent;
        }
    }

    [System.Serializable]
    public class SegmentData
    {
        public int parentNodeIndex;
        public int childNodeIndex;
        public Vector3 startPoint;
        public Vector3 endPoint;
        public float width;
        public Color color;

        public SegmentData(int parentIdx, int childIdx, float segmentWidth, Color segmentColor)
        {
            parentNodeIndex = parentIdx;
            childNodeIndex = childIdx;
            width = segmentWidth;
            color = segmentColor;
            startPoint = Vector3.zero;
            endPoint = Vector3.zero;
        }

        public void UpdatePoints(Vector3 start, Vector3 end)
        {
            startPoint = start;
            endPoint = end;
        }
    }

    [System.Serializable]
    public class CreatureRenderData
    {
        public NodeData[] nodes;
        public SegmentData[] segments;
        public Vector3 rootPosition;
        public bool isAlive;

        public CreatureRenderData(NodeData[] nodeArray, SegmentData[] segmentArray)
        {
            nodes = nodeArray ?? new NodeData[0];
            segments = segmentArray ?? new SegmentData[0];
            rootPosition = Vector3.zero;
            isAlive = true;
        }

        public void UpdateWorldPositions(Vector3 creaturePosition)
        {
            rootPosition = creaturePosition;

            // Update node world positions
            for (int i = 0; i < nodes.Length; i++)
            {
                nodes[i].worldPosition = creaturePosition + nodes[i].localPosition;
            }

            // Update segment endpoints
            for (int i = 0; i < segments.Length; i++)
            {
                if (
                    segments[i].parentNodeIndex >= 0
                    && segments[i].parentNodeIndex < nodes.Length
                    && segments[i].childNodeIndex >= 0
                    && segments[i].childNodeIndex < nodes.Length
                )
                {
                    segments[i]
                        .UpdatePoints(
                            nodes[segments[i].parentNodeIndex].worldPosition,
                            nodes[segments[i].childNodeIndex].worldPosition
                        );
                }
            }
        }
    }
}

using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public static class Utils
    {
        public static void UpdateSegmentRotation(
            ref SegmentData segment,
            NodeData[] nodes,
            float deltaTime
        )
        {
            if (segment.parentIndex >= nodes.Length || segment.childIndex >= nodes.Length)
                return;

            segment.prevAngle = segment.currentAngle;
            float cycleTime = (Time.time / segment.oscSpeed) % segment.oscSpeed;
            float modifiedTime;

            if (cycleTime < segment.oscSpeed * segment.forwardRatio)
            {
                modifiedTime = (cycleTime / (segment.oscSpeed * segment.forwardRatio)) * Mathf.PI;
            }
            else
            {
                float remainingTime = cycleTime - (segment.oscSpeed * segment.forwardRatio);
                float backDuration = segment.oscSpeed * (1 - segment.forwardRatio);
                modifiedTime = Mathf.PI + (remainingTime / backDuration) * Mathf.PI;
            }

            segment.currentAngle =
                ((Mathf.Sin(modifiedTime - Mathf.PI / 2f) + 1f) / 2f) * segment.maxAngle;
        }

        public static void UpdateNodePosition(
            ref NodeData childNode,
            NodeData parentNode,
            SegmentData segment,
            float accumulatedAngle = 0f
        )
        {
            float totalAngle =
                (segment.baseAngle + accumulatedAngle + segment.currentAngle) * Mathf.Deg2Rad;

            Vector3 offset = new Vector3(
                segment.length * Mathf.Cos(totalAngle),
                segment.length * Mathf.Sin(totalAngle),
                0f
            );
            childNode.position = parentNode.position + offset;
        }

        public static Vector2 CalculateThrust(SegmentData segment, NodeData[] nodes)
        {
            if (segment.parentIndex >= nodes.Length || segment.childIndex >= nodes.Length)
                return Vector2.zero;

            Vector2 parentMovement = nodes[segment.parentIndex].GetPositionDelta();
            Vector2 childMovement = nodes[segment.childIndex].GetPositionDelta();
            Vector2 thrust = (childMovement + parentMovement) / 2f;
            Vector2 thrustDirection = -thrust.normalized;

            float thrustMagnitude = Mathf.Pow(thrust.magnitude, 2f) * DataConstants.THRUST_COEF;
            Vector2 result = thrustDirection * thrustMagnitude;

            return result;
        }

        public static Vector2 CalculateWaterDrag(
            SegmentData segment,
            NodeData[] nodes,
            Vector2 velocity,
            float maxDrag
        )
        {
            if (
                velocity.magnitude < 0.01f
                || segment.parentIndex >= nodes.Length
                || segment.childIndex >= nodes.Length
            )
                return Vector2.zero;

            Vector2 segmentDirection = segment.GetDirection(nodes);
            float angle = Vector2.Angle(segmentDirection, velocity.normalized);
            float normalizedAngle = angle / 90f;
            float dragMagnitude = Mathf.Lerp(0.1f, maxDrag, normalizedAngle);

            return -velocity.normalized * dragMagnitude;
        }

        public static Texture2D CreateCircleTexture(int resolution, Color color)
        {
            Texture2D texture = new Texture2D(resolution, resolution);
            Vector2 center = new Vector2(resolution / 2f, resolution / 2f);
            float radius = resolution / 2f - 1f;

            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    Vector2 pixelPos = new Vector2(x, y);
                    float distance = Vector2.Distance(pixelPos, center);

                    if (distance <= radius)
                    {
                        texture.SetPixel(x, y, color);
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }

            texture.Apply();
            return texture;
        }

        public static Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices =
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
            };

            int[] triangles = { 0, 2, 1, 0, 3, 2 };

            Vector2[] uv =
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();

            return mesh;
        }

        public static bool IsWithinBounds(Vector3 position, Bounds bounds)
        {
            return bounds.Contains(position);
        }

        public static float CalculateNodeDistance(NodeData nodeA, NodeData nodeB)
        {
            return Vector3.Distance(nodeA.position, nodeB.position);
        }

        public static Vector3 GetSegmentCenter(SegmentData segment, NodeData[] nodes)
        {
            if (
                segment.parentIndex < 0
                || segment.parentIndex >= nodes.Length
                || segment.childIndex >= nodes.Length
            )
                return Vector3.zero;

            return (nodes[segment.parentIndex].position + nodes[segment.childIndex].position) / 2f;
        }
    }
}

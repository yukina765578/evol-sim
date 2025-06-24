using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class Segment : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private float length = 2f;
        private float width = 0.1f;
        private Color segmentColor = Color.white;

        void Awake()
        {
            SetupLineRenderer();
        }

        public void Initialize(float segmentLength, float segmentWidth, Color color)
        {
            length = segmentLength;
            width = segmentWidth;
            segmentColor = color;

            UpdateVisuals();
        }

        void SetupLineRenderer()
        {
            // Get or add LineRenderer
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
                lineRenderer = gameObject.AddComponent<LineRenderer>();

            // Configure LineRenderer
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 0; // Below nodes
        }

        void UpdateVisuals()
        {
            if (lineRenderer != null)
            {
                // Use gradient for LineRenderer color in newer Unity versions
                Gradient gradient = new Gradient();
                gradient.SetKeys(
                    new GradientColorKey[]
                    {
                        new GradientColorKey(segmentColor, 0.0f),
                        new GradientColorKey(segmentColor, 1.0f),
                    },
                    new GradientAlphaKey[]
                    {
                        new GradientAlphaKey(segmentColor.a, 0.0f),
                        new GradientAlphaKey(segmentColor.a, 1.0f),
                    }
                );
                lineRenderer.colorGradient = gradient;
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;
            }
        }

        public void UpdateRotation(Vector3 anchorPosition, float angleInDegrees)
        {
            if (lineRenderer == null)
                return;

            // Convert angle to radians
            float angleInRadians = angleInDegrees * Mathf.Deg2Rad;

            // Calculate endpoint position using trigonometry
            Vector3 endPosition =
                anchorPosition
                + new Vector3(
                    length * Mathf.Cos(angleInRadians),
                    length * Mathf.Sin(angleInRadians),
                    0f
                );

            // Update line positions
            lineRenderer.SetPosition(0, anchorPosition); // Start at node center
            lineRenderer.SetPosition(1, endPosition); // End at calculated position
        }

        public void UpdateRotation(Vector3 startPosition, Vector3 endPosition)
        {
            if (lineRenderer == null)
                return;

            // Update line positions to connect two nodes
            lineRenderer.SetPosition(0, startPosition);
            lineRenderer.SetPosition(1, endPosition);
        }

        public float GetLength()
        {
            return length;
        }

        void OnValidate()
        {
            if (Application.isPlaying)
            {
                UpdateVisuals();
            }
        }
    }
}

using UnityEngine;

namespace EvolutionSimulator.Creatures
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
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 1;
        }

        void UpdateVisuals()
        {
            if (lineRenderer == null)
            {
                SetupLineRenderer();
            }
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
            lineRenderer.startColor = segmentColor;
            lineRenderer.endColor = segmentColor;
        }

        public void UpdateRotation(Vector3 nodePosition, float angle)
        {
            if (lineRenderer == null)
                return;

            float angleInRadians = angle * Mathf.Deg2Rad;
            Vector3 endPosition =
                nodePosition
                + new Vector3(
                    length * Mathf.Cos(angleInRadians),
                    length * Mathf.Sin(angleInRadians),
                    0f
                );

            lineRenderer.SetPosition(0, nodePosition);
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

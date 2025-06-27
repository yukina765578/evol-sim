using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class Segment : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private float length = 2f;
        private float width = 0.1f;
        private Color segmentColor = Color.white;

        private Node parentNode;
        private Node childNode;

        private float oscillationSpeed = 2f;
        private float maxAngle = 30f;
        private float prevAngle = 0f;
        private float currentAngle = 0f;

        private Vector2 accelaration = Vector2.zero;

        void Awake()
        {
            SetupLineRenderer();
        }

        public void Initialize(
            float segmentLength,
            float segmentWidth,
            Color color,
            float segmentOscillationSpeed,
            float segmentMaxAngle,
            Node parent,
            Node child
        )
        {
            length = segmentLength;
            width = segmentWidth;
            segmentColor = color;
            oscillationSpeed = segmentOscillationSpeed;
            maxAngle = segmentMaxAngle;

            parentNode = parent;
            childNode = child;

            UpdateVisuals();
        }

        void SetupLineRenderer()
        {
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

        public void UpdateRotation(float accumulatedAngle = 0f, float phaseOffset = 0f)
        {
            if (lineRenderer == null)
                return;
            prevAngle = currentAngle;
            float cycleTime = (Time.time / oscillationSpeed + phaseOffset) % oscillationSpeed;
            float modifiedT;
            if (cycleTime < oscillationSpeed * 0.25)
            {
                modifiedT = (cycleTime / (oscillationSpeed * 0.25f)) * Mathf.PI;
            }
            else
            {
                float remainingTime = cycleTime - (oscillationSpeed * 0.25f);
                float slowDuration = oscillationSpeed * 0.75f;
                modifiedT = Mathf.PI + (remainingTime / slowDuration) * Mathf.PI;
            }
            currentAngle = ((Mathf.Sin(modifiedT - Mathf.PI / 2) + 1f) / 2f) * maxAngle;
            Debug.Log($"Segment modifiedT: {modifiedT}, currentAngle: {currentAngle}");
            Vector3 anchorPosition = parentNode.transform.position;
            Vector3 childPosition =
                anchorPosition
                + new Vector3(
                    length * Mathf.Cos((accumulatedAngle + currentAngle) * Mathf.Deg2Rad),
                    length * Mathf.Sin((accumulatedAngle + currentAngle) * Mathf.Deg2Rad),
                    0f
                );
            lineRenderer.SetPosition(0, anchorPosition);
            lineRenderer.SetPosition(1, childPosition);
            childNode.transform.position = childPosition;
        }

        public Vector2 GetThrust()
        {
            Vector2 parentDelta = parentNode.GetPositionDelta();
            Vector2 childDelta = childNode.GetPositionDelta();

            Vector2 segmentThrust = (parentDelta + childDelta) / 2f;
            Vector2 thrustDirection = -segmentThrust.normalized;

            float thrustCoefficient = 5f;
            float thrustMagnitude = segmentThrust.magnitude * thrustCoefficient;
            return thrustDirection * thrustMagnitude;
        }

        public float GetLength()
        {
            return length;
        }

        public float GetCurrentAngle()
        {
            return currentAngle;
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

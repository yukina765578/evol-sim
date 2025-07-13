using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class Segment : MonoBehaviour
    {
        private LineRenderer lineRenderer;
        private LineRenderer thrustDebugLine;
        private float length = 2f;
        private float width = 0.1f;
        private Color segmentColor = Color.white;

        private Node parentNode;
        private Node childNode;

        private float oscillationSpeed = 2f;
        private float maxAngle;
        private float prevAngle;
        private float currentAngle;
        private float forwardRatio;
        private float baseAngle;

        private float thrustCoefficient = 15f;

        private bool debugMode = false;
        private Energy energy;

        void Awake()
        {
            SetupLineRenderer();
            SetupDebugLine();
        }

        void Start()
        {
            energy = GetComponentInParent<Energy>();
        }

        public void Initialize(
            float segmentLength,
            float segmentWidth,
            Color color,
            float segmentOscillationSpeed,
            float segmentMaxAngle,
            float segmentForwardRatio,
            float segmentBaseAngle,
            Node parent,
            Node child
        )
        {
            length = segmentLength;
            width = segmentWidth;
            segmentColor = color;
            oscillationSpeed = segmentOscillationSpeed;
            maxAngle = segmentMaxAngle;
            forwardRatio = segmentForwardRatio;
            baseAngle = segmentBaseAngle;

            parentNode = parent;
            childNode = child;

            UpdateVisuals();
        }

        void SetupLineRenderer()
        {
            lineRenderer = GetComponent<LineRenderer>();
            if (lineRenderer == null)
                lineRenderer = gameObject.AddComponent<LineRenderer>();

            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.sortingOrder = 0;
        }

        void SetupDebugLine()
        {
            GameObject debugObj = new GameObject("ThrustDebug");
            debugObj.transform.SetParent(transform);

            thrustDebugLine = debugObj.AddComponent<LineRenderer>();
            thrustDebugLine.material = new Material(Shader.Find("Sprites/Default"));
            thrustDebugLine.positionCount = 2;
            thrustDebugLine.useWorldSpace = true;
            thrustDebugLine.sortingOrder = 5;
            thrustDebugLine.startWidth = 0.05f;
            thrustDebugLine.endWidth = 0.05f;
            thrustDebugLine.enabled = false;
            thrustDebugLine.startColor = Color.red;
            thrustDebugLine.endColor = Color.red;
        }

        void UpdateVisuals()
        {
            if (lineRenderer != null)
            {
                lineRenderer.startColor = segmentColor;
                lineRenderer.endColor = segmentColor;
                lineRenderer.startWidth = width;
                lineRenderer.endWidth = width;
            }
        }

        public void SetDebugMode(bool enabled)
        {
            debugMode = enabled;
            if (thrustDebugLine != null)
                thrustDebugLine.enabled = enabled;
        }

        public void UpdateRotation(float accumulatedAngle = 0f, float phaseOffset = 0f)
        {
            if (lineRenderer == null || parentNode == null || childNode == null)
                Debug.LogError("LineRenderer or Nodes not set up correctly in Segment.");

            // Calculate the angle based on oscillation speed and phase offset
            prevAngle = currentAngle;
            float cycleTime = (Time.time / oscillationSpeed + phaseOffset) % oscillationSpeed;
            float modifiedT;
            if (cycleTime < oscillationSpeed * forwardRatio)
            {
                modifiedT = (cycleTime / (oscillationSpeed * forwardRatio)) * Mathf.PI;
            }
            else
            {
                float remainingTime = cycleTime - (oscillationSpeed * forwardRatio);
                float slowDuration = oscillationSpeed * (1f - forwardRatio);
                modifiedT = Mathf.PI + (remainingTime / slowDuration) * Mathf.PI;
            }

            currentAngle = ((Mathf.Sin(modifiedT - Mathf.PI / 2f) + 1f) / 2f) * maxAngle;

            Vector3 anchorPosition = parentNode.transform.position;
            Vector3 childPosition =
                anchorPosition
                + new Vector3(
                    length
                        * Mathf.Cos((baseAngle + accumulatedAngle + currentAngle) * Mathf.Deg2Rad),
                    length
                        * Mathf.Sin((baseAngle + accumulatedAngle + currentAngle) * Mathf.Deg2Rad),
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
            Vector2 thrust = (childDelta + parentDelta) * 0.5f;
            Vector2 thrustDirection = -thrust.normalized;

            float thrustMagnitude = thrust.magnitude * thrustCoefficient;
            Vector2 result = thrustDirection * thrustMagnitude;

            if (debugMode && thrustDebugLine != null)
            {
                UpdateThrustDebugLine(thrustDirection, thrustMagnitude);
            }

            return result;
        }

        public Vector2 GetWaterDrag(Vector2 velocity, float maxDrag)
        {
            if (velocity.magnitude < 0.01f)
            {
                return Vector2.zero;
            }
            Vector2 segmentDirection = (
                childNode.transform.position - parentNode.transform.position
            ).normalized;
            float angle = Vector2.Angle(segmentDirection, velocity.normalized);
            float normalizedAngle = angle / 90f;
            float dragMagnitude = Mathf.Lerp(0.1f, maxDrag, normalizedAngle);
            Vector2 dragForce = -velocity.normalized * dragMagnitude;
            return dragForce;
        }

        void UpdateThrustDebugLine(Vector2 thrustDirection, float thrustMagnitude)
        {
            Vector3 segmentCenter =
                (parentNode.transform.position + childNode.transform.position) / 2f;
            Vector3 thrustEnd = segmentCenter + (Vector3)(thrustDirection * thrustMagnitude * 5f);
            thrustDebugLine.SetPosition(0, segmentCenter);
            thrustDebugLine.SetPosition(1, thrustEnd);
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

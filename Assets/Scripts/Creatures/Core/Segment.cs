using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class Segment : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField]
        private float angleTransitionSpeed = 2f;

        private LineRenderer lineRenderer;
        private LineRenderer thrustDebugLine;
        private float length = 2f;
        private float width = 0.1f;
        private Color segmentColor = Color.white;

        private Node parentNode;
        private Node childNode;

        private float maxAngle = 30f;
        private float prevAngle = 0f;
        private float currentAngle = 0f;
        private float baseAngle = 0f;

        // Brain control fields
        private float targetAngle = 0f;
        private bool brainControlled = false;

        private bool debugMode = false;
        private CreatureEnergy creatureEnergy;

        void Awake()
        {
            SetupLineRenderer();
            SetupDebugLine();
        }

        void Start()
        {
            creatureEnergy = GetComponentInParent<CreatureEnergy>();
        }

        public void Initialize(
            float segmentLength,
            float segmentWidth,
            Color color,
            float segmentMaxAngle,
            float segmentBaseAngle,
            Node parent,
            Node child
        )
        {
            length = segmentLength;
            width = segmentWidth;
            segmentColor = color;
            maxAngle = segmentMaxAngle;
            baseAngle = segmentBaseAngle;

            parentNode = parent;
            childNode = child;

            UpdateVisuals();
        }

        public void SetTargetAngle(float coefficient)
        {
            targetAngle = Mathf.Clamp(coefficient * maxAngle, -maxAngle, maxAngle);
            brainControlled = true;
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
            thrustDebugLine.startColor = Color.green;
            thrustDebugLine.endColor = Color.green;
        }

        void UpdateVisuals()
        {
            if (lineRenderer != null)
            {
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

        public void SetDebugMode(bool enabled)
        {
            debugMode = enabled;
            if (thrustDebugLine != null)
                thrustDebugLine.enabled = enabled;
        }

        public void UpdateRotation(float accumulatedAngle = 0f, float phaseOffset = 0f)
        {
            if (lineRenderer == null)
                return;

            prevAngle = currentAngle;

            if (brainControlled)
            {
                currentAngle = Mathf.LerpAngle(
                    currentAngle,
                    targetAngle,
                    angleTransitionSpeed * Time.deltaTime
                );
            }
            else
            {
                // Default to neutral position if no brain control
                currentAngle = 0f;
            }

            // Calculate energy cost for movement
            if (creatureEnergy != null && creatureEnergy.IsAlive)
            {
                float angleChange = Mathf.Abs(currentAngle - prevAngle) / Time.deltaTime;
                if (angleChange > 0.001f)
                {
                    creatureEnergy.ConsumeMovementEnergy(angleChange);
                }
            }

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

            Vector2 segmentThrust = (parentDelta + childDelta) / 2f;
            Vector2 thrustDirection = -segmentThrust.normalized;

            float thrustCoefficient = 60f;
            float thrustMagnitude = segmentThrust.magnitude * thrustCoefficient;
            Vector2 result = thrustDirection * thrustMagnitude;

            UpdateThrustDebug(thrustDirection);
            return result;
        }

        void UpdateThrustDebug(Vector2 thrustDirection)
        {
            if (!debugMode || thrustDebugLine == null)
                return;

            Vector3 segmentCenter =
                (parentNode.transform.position + childNode.transform.position) / 2f;
            Vector3 thrustEnd = segmentCenter + (Vector3)thrustDirection * 1f;

            thrustDebugLine.SetPosition(0, segmentCenter);
            thrustDebugLine.SetPosition(1, thrustEnd);
        }

        public float GetLength()
        {
            return length;
        }

        public float GetCurrentAngle()
        {
            return currentAngle;
        }

        public float GetMaxAngle()
        {
            return maxAngle;
        }

        void OnValidate()
        {
            angleTransitionSpeed = Mathf.Max(0.1f, angleTransitionSpeed);

            if (Application.isPlaying)
            {
                UpdateVisuals();
            }
        }
    }
}

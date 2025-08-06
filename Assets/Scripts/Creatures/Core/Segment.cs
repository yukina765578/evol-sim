using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class Segment : MonoBehaviour
    {
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

        private float thrustCoefficient = 30f;
        private Energy energy;

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
        }

        void Start()
        {
            energy = GetComponentInParent<Energy>();
        }

        public void UpdateRotation(float accumulatedAngle = 0f, float phaseOffset = 0f)
        {
            if (parentNode == null || childNode == null)
            {
                Debug.LogError("Parent or child node not set up correctly in Segment.");
                return;
            }

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

            // Consume energy based on movement
            float angleChange = Mathf.Abs(currentAngle - prevAngle);
            if (energy != null)
                energy.ConsumeMovementEnergy(angleChange);

            // Update positions
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

            childNode.transform.position = childPosition;
        }

        public Vector2 GetThrust()
        {
            Vector2 parentDelta = parentNode.GetPositionDelta();
            Vector2 childDelta = childNode.GetPositionDelta();
            Vector2 thrust = (childDelta + parentDelta) * 0.5f;
            Vector2 thrustDirection = -thrust.normalized;

            float thrustMagnitude = Mathf.Pow(thrust.magnitude, 1.5f) * thrustCoefficient;
            Vector2 result = thrustDirection * thrustMagnitude;

            return result;
        }

        public Vector2 GetWaterDrag(Vector2 velocity, float maxDrag)
        {
            if (velocity.magnitude < 0.01f)
                return Vector2.zero;

            Vector2 segmentDirection = (
                childNode.transform.position - parentNode.transform.position
            ).normalized;
            float angle = Vector2.Angle(segmentDirection, velocity.normalized);
            float normalizedAngle = angle / 90f;
            float dragMagnitude = Mathf.Lerp(0.1f, maxDrag, normalizedAngle);
            Vector2 dragForce = -velocity.normalized * dragMagnitude;

            return dragForce;
        }

        // New methods for SegmentRenderer
        public Matrix4x4 GetRenderMatrix()
        {
            if (parentNode == null || childNode == null)
                return Matrix4x4.identity;

            // Get positions
            Vector3 parentPos = parentNode.transform.position;
            Vector3 childPos = childNode.transform.position;

            // Calculate segment center
            Vector3 center = (parentPos + childPos) * 0.5f;

            // Calculate direction and length
            Vector3 direction = childPos - parentPos;
            float actualLength = direction.magnitude;

            // Calculate rotation (align with direction)
            Quaternion rotation = Quaternion.identity;
            if (actualLength > 0.001f)
            {
                // Calculate angle from direction vector
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                rotation = Quaternion.Euler(0, 0, angle);
            }

            // Calculate scale (length along X, width along Y)
            Vector3 scale = new Vector3(actualLength, width, 1f);

            // Create transformation matrix
            return Matrix4x4.TRS(center, rotation, scale);
        }

        public Color GetSegmentColor()
        {
            return segmentColor;
        }

        public void SetDebugMode(bool enabled)
        {
            // Debug mode removed in instanced rendering version
            // This method kept for Controller compatibility
        }

        void OnValidate()
        {
            length = Mathf.Max(0.1f, length);
            width = Mathf.Max(0.01f, width);
            oscillationSpeed = Mathf.Max(0.1f, oscillationSpeed);
        }
    }
}

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

        public void InitializePhysicsOnly(
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

            energy = GetComponentInParent<Energy>();
        }

        public void UpdateRotation(float accumulatedAngle = 0f, float phaseOffset = 0f)
        {
            if (parentNode == null || childNode == null)
            {
                Debug.LogError("Parent or child node missing in Segment");
                return;
            }

            // Calculate oscillation
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
            {
                energy.ConsumeMovementEnergy(angleChange);
            }

            // Update child node position
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
            if (parentNode == null || childNode == null)
                return Vector2.zero;

            Vector2 parentDelta = parentNode.GetPositionDelta();
            Vector2 childDelta = childNode.GetPositionDelta();
            Vector2 thrust = (childDelta + parentDelta) * 0.5f;
            Vector2 thrustDirection = -thrust.normalized;

            float thrustMagnitude = Mathf.Pow(thrust.magnitude, 1.5f) * thrustCoefficient;
            return thrustDirection * thrustMagnitude;
        }

        public Vector2 GetWaterDrag(Vector2 velocity, float maxDrag)
        {
            if (velocity.magnitude < 0.01f || parentNode == null || childNode == null)
            {
                return Vector2.zero;
            }

            Vector2 segmentDirection = (
                childNode.transform.position - parentNode.transform.position
            ).normalized;
            float angle = Vector2.Angle(segmentDirection, velocity.normalized);
            float normalizedAngle = angle / 90f;
            float dragMagnitude = Mathf.Lerp(0.1f, maxDrag, normalizedAngle);

            return -velocity.normalized * dragMagnitude;
        }

        public Node GetParentNode()
        {
            return parentNode;
        }

        public Node GetChildNode()
        {
            return childNode;
        }

        public float GetWidth()
        {
            return width;
        }

        public Color GetColor()
        {
            return segmentColor;
        }
    }
}

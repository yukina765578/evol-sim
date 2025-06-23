using UnityEngine;

namespace EvolutionSimulator.Creatures
{
    public class SimpleCreature : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField]
        private GameObject nodePrefab;

        [SerializeField]
        private GameObject segmentPrefab;

        [Header("Movement Settings")]
        [SerializeField]
        private float oscillationSpeed = 2f; // cycles per second

        [SerializeField]
        private float maxAngle = 30f; // degrees each side

        [SerializeField]
        private float oscillationSpeed2 = 2f; // second segment speed

        [SerializeField]
        private float maxAngle2 = 45f; // second segment max angle

        [SerializeField]
        private float phaseOffset = 0.5f; // wave delay for second segment

        [SerializeField]
        private float thrustEfficiency = 5f; // force multiplier

        [SerializeField]
        private float dragCoefficient = 2f; // water resistance

        [Header("Visual Settings")]
        [SerializeField]
        private float segmentLength = 2f;

        [SerializeField]
        private float nodeSize = 1f;

        [SerializeField]
        private Color nodeColor = Color.blue;

        [SerializeField]
        private Color segmentColor = Color.white;

        [SerializeField]
        private float segmentWidth = 0.1f;

        private GameObject nodeObject;
        private GameObject node2Object;
        private GameObject segmentObject;
        private GameObject segment2Object;
        private Node node;
        private Node node2;
        private Segment segment;
        private Segment segment2;
        private Rigidbody2D creatureRigidbody;
        private float currentAngle = 0f;
        private float previousAngle = 0f;

        void Start()
        {
            SetupRigidbody();
            SetupComponents();
        }

        void Update()
        {
            UpdateRotation();
            ApplySwimmingForces();
        }

        void SetupRigidbody()
        {
            creatureRigidbody = GetComponent<Rigidbody2D>();
            if (creatureRigidbody == null)
                creatureRigidbody = gameObject.AddComponent<Rigidbody2D>();

            creatureRigidbody.gravityScale = 0f; // No gravity for swimming
            creatureRigidbody.linearDamping = dragCoefficient;
            creatureRigidbody.angularDamping = 5f;
        }

        void SetupComponents()
        {
            if (nodePrefab == null || segmentPrefab == null)
            {
                Debug.LogError("Node and Segment prefabs must be assigned!");
                return;
            }

            // Create first node at creature center
            nodeObject = Instantiate(
                nodePrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            node = nodeObject.GetComponent<Node>();

            // Create second node
            node2Object = Instantiate(
                nodePrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            node2 = node2Object.GetComponent<Node>();

            // Create segment connecting both nodes
            segmentObject = Instantiate(
                segmentPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            segment = segmentObject.GetComponent<Segment>();

            // Create second segment extending from node2
            segment2Object = Instantiate(
                segmentPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            segment2 = segment2Object.GetComponent<Segment>();

            if (node == null || node2 == null || segment == null || segment2 == null)
            {
                Debug.LogError("Prefabs must have Node and Segment components!");
                return;
            }

            node.Initialize(nodeSize, nodeColor);
            node2.Initialize(nodeSize, nodeColor);
            segment.Initialize(segmentLength, segmentWidth, segmentColor);
            segment2.Initialize(segmentLength, segmentWidth, segmentColor);
        }

        void UpdateRotation()
        {
            if (node == null || node2 == null || segment == null || segment2 == null)
                return;

            previousAngle = currentAngle;

            // Oscillate back and forth using sine wave
            float time = Time.time * oscillationSpeed;
            currentAngle = Mathf.Sin(time) * maxAngle;

            // Position second node at end of first segment
            Vector3 node1Pos = transform.position;
            Vector3 node2Pos =
                node1Pos
                + new Vector3(
                    segmentLength * Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                    segmentLength * Mathf.Sin(currentAngle * Mathf.Deg2Rad),
                    0f
                );

            node2Object.transform.position = node2Pos;

            // Update first segment (connects node1 to node2)
            segment.UpdateRotation(node1Pos, node2Pos);

            // Update second segment (extends from node2)
            Vector3 segment2End =
                node2Pos
                + new Vector3(
                    segmentLength * Mathf.Cos(currentAngle * Mathf.Deg2Rad),
                    segmentLength * Mathf.Sin(currentAngle * Mathf.Deg2Rad),
                    0f
                );
            segment2.UpdateRotation(node2Pos, segment2End);
        }

        void ApplySwimmingForces()
        {
            if (creatureRigidbody == null)
                return;

            // Calculate angular velocity of segment
            float angleChange = currentAngle - previousAngle;
            float angularVelocity = angleChange / Time.deltaTime;

            // Calculate thrust based on segment pushing against water
            // Segment angle relative to creature's orientation
            float absoluteAngle = transform.eulerAngles.z + currentAngle;

            // Direction the segment is "pushing" water
            Vector2 segmentDirection = new Vector2(
                Mathf.Cos(absoluteAngle * Mathf.Deg2Rad),
                Mathf.Sin(absoluteAngle * Mathf.Deg2Rad)
            );

            // Thrust is opposite to segment push direction (Newton's 3rd law)
            Vector2 thrustDirection = -segmentDirection;

            // Thrust magnitude based on how fast segment is moving
            float thrustMagnitude = Mathf.Abs(angularVelocity) * thrustEfficiency;

            // Apply calculated thrust
            Vector2 finalThrust = thrustDirection * thrustMagnitude * Time.deltaTime;
            creatureRigidbody.AddForce(finalThrust);
        }

        void OnValidate()
        {
            oscillationSpeed = Mathf.Clamp(oscillationSpeed, 0f, 10f);
            maxAngle = Mathf.Clamp(maxAngle, 5f, 90f);
            thrustEfficiency = Mathf.Clamp(thrustEfficiency, 0f, 50f);
            dragCoefficient = Mathf.Clamp(dragCoefficient, 0.1f, 10f);
            segmentLength = Mathf.Clamp(segmentLength, 0.5f, 10f);
            nodeSize = Mathf.Clamp(nodeSize, 0.1f, 3f);
            segmentWidth = Mathf.Clamp(segmentWidth, 0.01f, 1f);
        }
    }
}

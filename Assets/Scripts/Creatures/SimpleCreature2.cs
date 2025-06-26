using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class SimpleCreature2 : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField]
        private float oscillationSpeed = 2f;

        [SerializeField]
        private float maxAngle = 30f;

        [SerializeField]
        private float oscillationSpeed1 = 2f;

        [SerializeField]
        private float maxAngle1 = 45f;

        [SerializeField]
        private float phaseOffset = 0.5f;

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

        [Header("Water Physics")]
        [SerializeField]
        private float thrustMultiplier = 5f;

        [SerializeField]
        private float waterResistance = 0.1f;

        [SerializeField]
        private float rotationalResistance = 0.05f;

        private GameObject node0Object;
        private GameObject node1Object;
        private GameObject node2Object;
        private GameObject segment0Object;
        private GameObject segment1Object;

        private Node node0;
        private Node node1;
        private Node node2;

        private Segment segment0;
        private Segment segment1;

        private float previousSegment0Angle = 0f;
        private float previousSegment1Angle = 0f;
        private float segment0Angle = 0f;
        private float segment1Angle = 0f;

        private Rigidbody2D creatureRigidbody;

        void Start()
        {
            SetupRigidbody();
            SetupComponents();
        }

        void Update()
        {
            UpdateRotation(); // Visual updates
            ApplyThrust(); // Apply thrust based on segment angles
        }

        void SetupRigidbody()
        {
            creatureRigidbody = gameObject.AddComponent<Rigidbody2D>();
            if (creatureRigidbody == null)
            {
                Debug.LogError("Rigidbody2D component could not be added to the creature.");
            }

            creatureRigidbody.gravityScale = 0f; // No gravity in water
        }

        void SetupComponents()
        {
            // Create nodes
            node0Object = new GameObject("Node0");
            node0Object.transform.SetParent(transform);
            node0Object.transform.localPosition = Vector3.zero;
            node0 = node0Object.AddComponent<Node>();
            node0.Initialize(nodeSize, nodeColor);

            node1Object = new GameObject("Node1");
            node1Object.transform.SetParent(transform);
            node1Object.transform.localPosition = Vector3.right * segmentLength;
            node1 = node1Object.AddComponent<Node>();
            node1.Initialize(nodeSize, nodeColor);

            node2Object = new GameObject("Node2");
            node2Object.transform.SetParent(transform);
            node2Object.transform.localPosition = Vector3.right * (segmentLength * 2);
            node2 = node2Object.AddComponent<Node>();
            node2.Initialize(nodeSize, nodeColor);

            // Create segments
            segment0Object = new GameObject("Segment0");
            segment0Object.transform.SetParent(transform);
            segment0 = segment0Object.AddComponent<Segment>();
            segment0.Initialize(
                segmentLength,
                segmentWidth,
                segmentColor,
                oscillationSpeed,
                maxAngle,
                node0,
                node1
            );

            segment1Object = new GameObject("Segment1");
            segment1Object.transform.SetParent(transform);
            segment1 = segment1Object.AddComponent<Segment>();
            segment1.Initialize(
                segmentLength,
                segmentWidth,
                segmentColor,
                oscillationSpeed1,
                maxAngle1,
                node1,
                node2
            );
        }

        void UpdateRotation()
        {
            segment0.UpdateRotation();
            float segment0Angle = segment0.GetCurrentAngle();
            segment1.UpdateRotation(segment0Angle, phaseOffset);
        }

        void ApplyThrust()
        {
            if (creatureRigidbody == null)
                return;
            Vector2 segment0Thrust = segment0.GetThrust();
            Vector2 segment1Thrust = segment1.GetThrust();
            Vector2 totalThrust = segment0Thrust + segment1Thrust;
            creatureRigidbody.AddForce(totalThrust * thrustMultiplier);

            Debug.Log($"Thrust applie: {totalThrust * thrustMultiplier}");
            Debug.Log($"Segment 0 Thrust: {segment0Thrust}, Segment 1 Thrust: {segment1Thrust}");
            Debug.Log(
                $"Segment 0 Angle: {segment0.GetCurrentAngle()}, Segment 1 Angle: {segment1.GetCurrentAngle()}"
            );
        }

        void ApplyWaterResistance()
        {
            if (creatureRigidbody == null)
                return;

            Vector2 velocity = creatureRigidbody.linearVelocity;

            // Water resistance proportional to velocity squared
            float speed = velocity.magnitude;
            if (speed > 0.01f)
            {
                Vector2 resistance = -velocity.normalized * speed * speed * waterResistance;
                creatureRigidbody.AddForce(resistance);
            }

            // Angular resistance for rotation
            float angularVel = creatureRigidbody.angularVelocity;
            if (Mathf.Abs(angularVel) > 0.01f)
            {
                float angularResistance =
                    -angularVel * Mathf.Abs(angularVel) * rotationalResistance;
                creatureRigidbody.AddTorque(angularResistance);
            }
        }

        void OnValidate()
        {
            oscillationSpeed = Mathf.Clamp(oscillationSpeed, 0.1f, 10f);
            maxAngle = Mathf.Clamp(maxAngle, 0f, 180f);
            oscillationSpeed1 = Mathf.Clamp(oscillationSpeed1, 0.1f, 10f);
            maxAngle1 = Mathf.Clamp(maxAngle1, 0f, 180f);
            nodeSize = Mathf.Clamp(nodeSize, 0.1f, 5f);
            segmentWidth = Mathf.Clamp(segmentWidth, 0.01f, 1f);
            thrustMultiplier = Mathf.Clamp(thrustMultiplier, 0f, 20f);
            waterResistance = Mathf.Clamp(waterResistance, 0f, 1f);
        }
    }
}

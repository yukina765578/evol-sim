using UnityEngine;

namespace EvolutionSimulator.Creature
{
    public class SimpleCreature2 : MonoBehaviour
    {
        [Header("Prefab References")]
        [SerializeField]
        private GameObject nodePrefab;

        [SerializeField]
        private GameObject segmentPrefab;

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

        private GameObject node0Object;
        private GameObject node1Object;
        private GameObject segment0Object;
        private GameObject segment1Object;

        private Node node0;
        private Node node1;

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
            UpdateRotation();
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
            if (nodePrefab == null || segmentPrefab == null)
            {
                Debug.LogError("Node and Segment prefabs must be assigned!");
                return;
            }

            node0Object = Instantiate(
                nodePrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            node0 = node0Object.GetComponent<Node>();

            segment0Object = Instantiate(
                segmentPrefab,
                transform.position,
                Quaternion.identity,
                transform
            );
            segment0 = segment0Object.GetComponent<Segment>();

            node1Object = Instantiate(
                nodePrefab,
                transform.position + Vector3.right * segmentLength,
                Quaternion.identity,
                transform
            );
            node1 = node1Object.GetComponent<Node>();
            segment1Object = Instantiate(
                segmentPrefab,
                transform.position + Vector3.right * segmentLength / 2,
                Quaternion.identity,
                transform
            );
            segment1 = segment1Object.GetComponent<Segment>();

            // Set up nodes and segments
            node0.Initialize(nodeSize, nodeColor);
            node1.Initialize(nodeSize, nodeColor);
            segment0.Initialize(segmentLength, segmentWidth, segmentColor);
            segment1.Initialize(segmentLength, segmentWidth, segmentColor);
        }

        void UpdateRotation()
        {
            if (node0 == null || node1 == null || segment0 == null || segment1 == null)
            {
                Debug.LogError("Creature components are not properly initialized!");
                return;
            }

            float time = Time.time;
            // Segment 0 oscillation
            previousSegment0Angle = segment0Angle;
            float segment0Oscillation = time * oscillationSpeed;
            segment0Angle = Mathf.Sin(segment0Oscillation) * maxAngle;

            Vector3 node0Position = transform.position;
            Vector3 node1Position =
                node0Position
                + new Vector3(
                    segmentLength * Mathf.Cos(segment0Angle * Mathf.Deg2Rad),
                    segmentLength * Mathf.Sin(segment0Angle * Mathf.Deg2Rad),
                    0f
                );

            node1Object.transform.position = node1Position;
            segment0.UpdateRotation(node0Position, node1Position);

            // Segment 1 oscillation
            previousSegment1Angle = segment1Angle;
            float segment1Oscillation = time * oscillationSpeed1 + phaseOffset;
            segment1Angle = Mathf.Sin(segment1Oscillation) * maxAngle1 + segment0Angle;
            Vector3 segment1EndPosition =
                node1Position
                + new Vector3(
                    segmentLength * Mathf.Cos(segment1Angle * Mathf.Deg2Rad),
                    segmentLength * Mathf.Sin(segment1Angle * Mathf.Deg2Rad),
                    0f
                );
            segment1.UpdateRotation(node1Position, segment1EndPosition);
        }

        

        void OnValidate()
        {
            oscillationSpeed = Mathf.Clamp(oscillationSpeed, 0.1f, 10f);
            nodeSize = Mathf.Clamp(nodeSize, 0.1f, 5f);
            segmentWidth = Mathf.Clamp(segmentWidth, 0.01f, 1f);
        }
    }
}

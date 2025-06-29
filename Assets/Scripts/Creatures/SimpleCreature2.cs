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

        [SerializeField]
        private float forwardRatio = 0.05f;

        [SerializeField]
        private float segment0BaseAngle = 0f;

        [SerializeField]
        private float segment1BaseAngle = 45f;

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

        [Header("Debug Settings")]
        [SerializeField]
        private bool showThrustDebug = false;

        [SerializeField]
        private bool showVelocityDebug = false;

        [Header("Physics")]
        [SerializeField]
        private float slidingFriction = 0.5f;

        private Node node0,
            node1,
            node2;
        private Segment segment0,
            segment1;
        private Rigidbody2D creatureRigidbody;
        private LineRenderer velocityDebugLine;

        void Start()
        {
            SetupRigidbody();
            CreateNodes();
            CreateSegments();
            SetupVelocityDebug();
            UpdateDebugMode();
        }

        void Update()
        {
            UpdateRotation();
            ApplyThrust();
            ApplySlidingFriction();
            UpdateVelocityDebug();
        }

        void SetupRigidbody()
        {
            creatureRigidbody = gameObject.AddComponent<Rigidbody2D>();
            creatureRigidbody.gravityScale = 0f;
        }

        void CreateNodes()
        {
            node0 = CreateNode("Node0", Vector3.zero);
            node1 = CreateNode("Node1", Vector3.right * segmentLength);
            node2 = CreateNode("Node2", Vector3.right * segmentLength * 2);
        }

        Node CreateNode(string name, Vector3 position)
        {
            GameObject nodeObj = new GameObject(name);
            nodeObj.transform.SetParent(transform);
            nodeObj.transform.localPosition = position;
            Node node = nodeObj.AddComponent<Node>();
            node.Initialize(nodeSize, nodeColor);
            return node;
        }

        void CreateSegments()
        {
            segment0 = CreateSegment(
                "Segment0",
                oscillationSpeed,
                maxAngle,
                segment0BaseAngle,
                node0,
                node1
            );
            segment1 = CreateSegment(
                "Segment1",
                oscillationSpeed1,
                maxAngle1,
                segment1BaseAngle,
                node1,
                node2
            );
        }

        Segment CreateSegment(
            string name,
            float speed,
            float angle,
            float baseAngle,
            Node parent,
            Node child
        )
        {
            GameObject segmentObj = new GameObject(name);
            segmentObj.transform.SetParent(transform);
            Segment segment = segmentObj.AddComponent<Segment>();
            segment.Initialize(
                segmentLength,
                segmentWidth,
                segmentColor,
                speed,
                angle,
                forwardRatio,
                baseAngle,
                parent,
                child
            );
            return segment;
        }

        void SetupVelocityDebug()
        {
            GameObject debugObj = new GameObject("VelocityDebug");
            debugObj.transform.SetParent(transform);

            velocityDebugLine = debugObj.AddComponent<LineRenderer>();
            velocityDebugLine.material = new Material(Shader.Find("Sprites/Default"));
            velocityDebugLine.positionCount = 2;
            velocityDebugLine.useWorldSpace = true;
            velocityDebugLine.sortingOrder = 10;
            velocityDebugLine.startWidth = 0.1f;
            velocityDebugLine.endWidth = 0.1f;

            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[]
                {
                    new GradientColorKey(Color.red, 0.0f),
                    new GradientColorKey(Color.red, 1.0f),
                },
                new GradientAlphaKey[]
                {
                    new GradientAlphaKey(1.0f, 0.0f),
                    new GradientAlphaKey(1.0f, 1.0f),
                }
            );
            velocityDebugLine.colorGradient = gradient;
            velocityDebugLine.enabled = false;
        }

        void UpdateDebugMode()
        {
            segment0?.SetDebugMode(showThrustDebug);
            segment1?.SetDebugMode(showThrustDebug);
            if (velocityDebugLine != null)
                velocityDebugLine.enabled = showVelocityDebug;
        }

        void UpdateRotation()
        {
            segment0.UpdateRotation();
            segment1.UpdateRotation(segment0.GetCurrentAngle(), phaseOffset);
        }

        void ApplyThrust()
        {
            Vector2 totalThrust = segment0.GetThrust() + segment1.GetThrust();
            creatureRigidbody.AddForce(totalThrust);
        }

        void ApplySlidingFriction()
        {
            Vector2 velocity = creatureRigidbody.linearVelocity;
            float speed = velocity.magnitude;

            if (speed > 0.001f)
            {
                float frictionCoeff = slidingFriction / (speed + 0.1f);
                Vector2 frictionForce = -velocity * frictionCoeff;
                creatureRigidbody.AddForce(frictionForce);
            }
        }

        void UpdateVelocityDebug()
        {
            if (!showVelocityDebug || velocityDebugLine == null)
                return;

            Vector2 velocity = creatureRigidbody.linearVelocity;
            Vector3 nodePos = node0.transform.position;
            float offset = nodeSize + 0.2f;

            Vector3 startPos = nodePos + (Vector3)velocity.normalized * offset;
            Vector3 endPos = startPos + (Vector3)velocity.normalized * velocity.magnitude * 3f;

            velocityDebugLine.SetPosition(0, startPos);
            velocityDebugLine.SetPosition(1, endPos);
        }

        void OnValidate()
        {
            oscillationSpeed = Mathf.Clamp(oscillationSpeed, 0.1f, 10f);
            maxAngle = Mathf.Clamp(maxAngle, -180f, 180f);
            oscillationSpeed1 = Mathf.Clamp(oscillationSpeed1, 0.1f, 10f);
            maxAngle1 = Mathf.Clamp(maxAngle1, -180f, 180f);
            nodeSize = Mathf.Clamp(nodeSize, 0.1f, 5f);
            segmentWidth = Mathf.Clamp(segmentWidth, 0.01f, 1f);
            slidingFriction = Mathf.Clamp(slidingFriction, 0f, 2f);
            forwardRatio = Mathf.Clamp(forwardRatio, 0.01f, 0.99f);

            if (Application.isPlaying)
                UpdateDebugMode();
        }
    }
}

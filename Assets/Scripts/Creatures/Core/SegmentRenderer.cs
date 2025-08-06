using EvolutionSimulator.Creatures.Genetics;
using UnityEngine;
using UnityEngine.Rendering;

namespace EvolutionSimulator.Creatures.Core
{
    [System.Serializable]
    public struct SegmentData
    {
        public float length;
        public float width;
        public Color color;
        public float oscillationSpeed;
        public float maxAngle;
        public float forwardRatio;
        public float baseAngle;
        public int parentNodeIndex;
        public int childNodeIndex;

        // Runtime state
        public float currentAngle;
        public float prevAngle;
        public float thrustCoefficient;
    }

    public class SegmentRenderer : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField]
        private Material segmentMaterial;

        [SerializeField]
        private bool useTestShader = true;

        [Header("Debug")]
        [SerializeField]
        private bool showSegmentCount = false;

        private Mesh segmentMesh;
        private SegmentData[] segments;
        private Node[] nodes;
        private Matrix4x4[] matrices;
        private RenderParams renderParams;
        private int segmentCount;
        private Energy energy;

        void Awake()
        {
            CreateSegmentMesh();
            SetupMaterial();
        }

        void Start()
        {
            energy = GetComponent<Energy>();
        }

        public void Initialize(CreatureGenome genome, Node[] creatureNodes)
        {
            nodes = creatureNodes;
            CreateSegmentData(genome);
            SetupRenderParams();
            InitializeMatrices();

            if (showSegmentCount)
                Debug.Log($"SegmentRenderer initialized with {segmentCount} segments on {name}");
        }

        void CreateSegmentData(CreatureGenome genome)
        {
            segmentCount = genome.NodeCount - 1; // No segment for root node
            if (segmentCount <= 0)
                return;

            segments = new SegmentData[segmentCount];

            for (int i = 1; i < genome.NodeCount; i++) // Start from 1 (skip root)
            {
                NodeGene nodeGene = genome.nodes[i];
                int segmentIndex = i - 1;

                segments[segmentIndex] = new SegmentData
                {
                    length = 2f, // Default length
                    width = 0.1f, // Default width
                    color = Color.white,
                    oscillationSpeed = nodeGene.oscSpeed,
                    maxAngle = nodeGene.maxAngle,
                    forwardRatio = nodeGene.forwardRatio,
                    baseAngle = nodeGene.baseAngle,
                    parentNodeIndex = nodeGene.parentIndex,
                    childNodeIndex = i,
                    currentAngle = 0f,
                    prevAngle = 0f,
                    thrustCoefficient = 30f,
                };
            }
        }

        void CreateSegmentMesh()
        {
            segmentMesh = new Mesh();

            Vector3[] vertices = new Vector3[]
            {
                new Vector3(-0.5f, -0.5f, 0),
                new Vector3(0.5f, -0.5f, 0),
                new Vector3(0.5f, 0.5f, 0),
                new Vector3(-0.5f, 0.5f, 0),
            };

            Vector2[] uvs = new Vector2[]
            {
                new Vector2(0, 0),
                new Vector2(1, 0),
                new Vector2(1, 1),
                new Vector2(0, 1),
            };

            int[] triangles = new int[] { 0, 1, 2, 0, 2, 3 };

            segmentMesh.vertices = vertices;
            segmentMesh.uv = uvs;
            segmentMesh.triangles = triangles;
            segmentMesh.RecalculateNormals();
            segmentMesh.RecalculateBounds();
        }

        void SetupMaterial()
        {
            if (segmentMaterial == null && useTestShader)
            {
                Shader testShader = Shader.Find("Custom/Unlit2DInstanced");
                if (testShader != null)
                {
                    segmentMaterial = new Material(testShader);
                    segmentMaterial.color = Color.white;
                    segmentMaterial.enableInstancing = true;
                }
                else
                {
                    Debug.LogWarning("Test shader not found! Using default material.");
                    segmentMaterial = new Material(Shader.Find("Sprites/Default"));
                    segmentMaterial.enableInstancing = true;
                }
            }

            if (segmentMaterial != null)
                segmentMaterial.enableInstancing = true;
        }

        void SetupRenderParams()
        {
            if (segmentMaterial == null)
                return;

            renderParams = new RenderParams(segmentMaterial)
            {
                receiveShadows = false,
                shadowCastingMode = ShadowCastingMode.Off,
                layer = gameObject.layer,
                worldBounds = new Bounds(transform.position, Vector3.one * 100f),
            };
        }

        void InitializeMatrices()
        {
            if (segmentCount > 0)
                matrices = new Matrix4x4[segmentCount];
        }

        void Update()
        {
            if (segments == null || nodes == null || segmentCount == 0)
                return;

            UpdateSegmentRotations();
            UpdateRenderMatrices();
            RenderSegments();
        }

        void UpdateSegmentRotations()
        {
            for (int i = 0; i < segmentCount; i++)
            {
                var segment = segments[i];

                // Update oscillation
                segment.prevAngle = segment.currentAngle;
                float cycleTime = (Time.time / segment.oscillationSpeed) % segment.oscillationSpeed;
                float modifiedT;

                if (cycleTime < segment.oscillationSpeed * segment.forwardRatio)
                {
                    modifiedT =
                        (cycleTime / (segment.oscillationSpeed * segment.forwardRatio)) * Mathf.PI;
                }
                else
                {
                    float remainingTime =
                        cycleTime - (segment.oscillationSpeed * segment.forwardRatio);
                    float slowDuration = segment.oscillationSpeed * (1f - segment.forwardRatio);
                    modifiedT = Mathf.PI + (remainingTime / slowDuration) * Mathf.PI;
                }

                segment.currentAngle =
                    ((Mathf.Sin(modifiedT - Mathf.PI / 2f) + 1f) / 2f) * segment.maxAngle;

                // Consume energy
                float angleChange = Mathf.Abs(segment.currentAngle - segment.prevAngle);
                if (energy != null)
                    energy.ConsumeMovementEnergy(angleChange);

                // Update child node position
                if (segment.parentNodeIndex < nodes.Length && segment.childNodeIndex < nodes.Length)
                {
                    Vector3 parentPos = nodes[segment.parentNodeIndex].transform.position;
                    Vector3 childPos =
                        parentPos
                        + new Vector3(
                            segment.length
                                * Mathf.Cos(
                                    (segment.baseAngle + segment.currentAngle) * Mathf.Deg2Rad
                                ),
                            segment.length
                                * Mathf.Sin(
                                    (segment.baseAngle + segment.currentAngle) * Mathf.Deg2Rad
                                ),
                            0f
                        );
                    nodes[segment.childNodeIndex].transform.position = childPos;
                }

                segments[i] = segment; // Write back the struct
            }
        }

        void UpdateRenderMatrices()
        {
            renderParams.worldBounds = new Bounds(transform.position, Vector3.one * 100f);
            for (int i = 0; i < segmentCount; i++)
            {
                var segment = segments[i];

                if (
                    segment.parentNodeIndex >= nodes.Length
                    || segment.childNodeIndex >= nodes.Length
                )
                    continue;

                Vector3 parentPos = nodes[segment.parentNodeIndex].transform.position;
                Vector3 childPos = nodes[segment.childNodeIndex].transform.position;

                Vector3 center = (parentPos + childPos) * 0.5f;
                Vector3 direction = childPos - parentPos;
                float actualLength = direction.magnitude;

                Quaternion rotation = Quaternion.identity;
                if (actualLength > 0.001f)
                {
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    rotation = Quaternion.Euler(0, 0, angle);
                }

                Vector3 scale = new Vector3(actualLength, segment.width, 1f);
                matrices[i] = Matrix4x4.TRS(center, rotation, scale);
            }
        }

        void RenderSegments()
        {
            if (
                segmentMaterial == null
                || segmentMesh == null
                || segmentCount <= 0
                || matrices == null
            )
                return;

            try
            {
                Graphics.RenderMeshInstanced(renderParams, segmentMesh, 0, matrices, segmentCount);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"SegmentRenderer error: {e.Message}");
                enabled = false;
            }
        }

        // Public methods for Controller physics
        public Vector2 GetTotalThrust()
        {
            Vector2 totalThrust = Vector2.zero;

            for (int i = 0; i < segmentCount; i++)
            {
                var segment = segments[i];
                if (
                    segment.parentNodeIndex >= nodes.Length
                    || segment.childNodeIndex >= nodes.Length
                )
                    continue;

                Vector2 parentDelta = nodes[segment.parentNodeIndex].GetPositionDelta();
                Vector2 childDelta = nodes[segment.childNodeIndex].GetPositionDelta();
                Vector2 thrust = (childDelta + parentDelta) * 0.5f;
                Vector2 thrustDirection = -thrust.normalized;

                float thrustMagnitude =
                    Mathf.Pow(thrust.magnitude, 1.5f) * segment.thrustCoefficient;
                totalThrust += thrustDirection * thrustMagnitude;
            }

            return totalThrust;
        }

        public Vector2 GetTotalWaterDrag(Vector2 velocity, float maxDragPerSegment)
        {
            Vector2 totalDrag = Vector2.zero;

            if (velocity.magnitude < 0.01f)
                return totalDrag;

            for (int i = 0; i < segmentCount; i++)
            {
                var segment = segments[i];
                if (
                    segment.parentNodeIndex >= nodes.Length
                    || segment.childNodeIndex >= nodes.Length
                )
                    continue;

                Vector3 parentPos = nodes[segment.parentNodeIndex].transform.position;
                Vector3 childPos = nodes[segment.childNodeIndex].transform.position;
                Vector2 segmentDirection = (childPos - parentPos).normalized;

                float angle = Vector2.Angle(segmentDirection, velocity.normalized);
                float normalizedAngle = angle / 90f;
                float dragMagnitude = Mathf.Lerp(0.1f, maxDragPerSegment, normalizedAngle);
                totalDrag += -velocity.normalized * dragMagnitude;
            }

            return totalDrag;
        }

        void OnDestroy()
        {
            if (segmentMesh != null)
                DestroyImmediate(segmentMesh);
            if (segmentMaterial != null && useTestShader)
                DestroyImmediate(segmentMaterial);
        }
    }
}

using EvolutionSimulator.Creatures.Core;
using UnityEngine;
using UnityEngine.Rendering;

namespace EvolutionSimulator.Creatures.Core
{
    public class NodeRenderer : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField]
        private Material nodeMaterial;

        [SerializeField]
        private float nodeSize = 1f;

        [Header("Colors")]
        [SerializeField]
        private Color normalColor = Color.blue;

        [SerializeField]
        private Color reproductionColor = Color.red;

        private Mesh nodeMesh;
        private Matrix4x4[] matrices;
        private RenderParams renderParams;
        private int nodeCount;
        private Energy creatureEnergy;
        private SegmentRenderer segmentRenderer;

        void Awake()
        {
            CreateNodeMesh();
            SetupMaterial();
        }

        void Start()
        {
            creatureEnergy = GetComponent<Energy>();
            segmentRenderer = GetComponent<SegmentRenderer>();

            if (segmentRenderer == null)
            {
                Debug.LogError("NodeRenderer requires SegmentRenderer component!");
                enabled = false;
                return;
            }
        }

        public void Initialize(int totalNodeCount)
        {
            nodeCount = totalNodeCount;
            matrices = new Matrix4x4[nodeCount];
            SetupRenderParams();
        }

        void CreateNodeMesh()
        {
            nodeMesh = new Mesh();

            // Create quad vertices (circle will be created in shader)
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

            nodeMesh.vertices = vertices;
            nodeMesh.uv = uvs;
            nodeMesh.triangles = triangles;
            nodeMesh.RecalculateNormals();
            nodeMesh.RecalculateBounds();
        }

        void SetupMaterial()
        {
            if (nodeMaterial == null)
            {
                // Use the existing CircleInstanced shader
                Shader circleShader = Shader.Find("EvolutionSimulator/CircleInstanced");
                if (circleShader != null)
                {
                    nodeMaterial = new Material(circleShader);
                    nodeMaterial.SetColor("_Color", normalColor);
                    nodeMaterial.SetFloat("_CircleRadius", 0.5f);
                    nodeMaterial.enableInstancing = true;
                }
                else
                {
                    Debug.LogWarning("CircleInstanced shader not found! Using default material.");
                    nodeMaterial = new Material(Shader.Find("Sprites/Default"));
                    nodeMaterial.color = normalColor;
                    nodeMaterial.enableInstancing = true;
                }
            }
        }

        void SetupRenderParams()
        {
            if (nodeMaterial == null)
                return;

            renderParams = new RenderParams(nodeMaterial)
            {
                receiveShadows = false,
                shadowCastingMode = ShadowCastingMode.Off,
                layer = gameObject.layer,
                worldBounds = new Bounds(transform.position, Vector3.one * 100f),
            };
        }

        void Update()
        {
            if (segmentRenderer == null || nodeCount == 0 || matrices == null)
                return;

            UpdateNodeColor();
            UpdateMatrices();
            RenderNodes();
        }

        void UpdateNodeColor()
        {
            if (nodeMaterial == null)
                return;

            // Change color based on reproduction readiness
            Color currentColor =
                (creatureEnergy != null && creatureEnergy.IsReproductionReady)
                    ? reproductionColor
                    : normalColor;

            // Update material color based on shader type
            if (nodeMaterial.HasProperty("_Color"))
                nodeMaterial.SetColor("_Color", currentColor);
            else
                nodeMaterial.color = currentColor;
        }

        void UpdateMatrices()
        {
            // Get current node positions from SegmentRenderer
            Vector3[] nodePositions = segmentRenderer.GetNodePositions();

            if (nodePositions == null || nodePositions.Length != nodeCount)
            {
                Debug.LogWarning(
                    $"NodeRenderer: Position array mismatch. Expected {nodeCount}, got {nodePositions?.Length ?? 0}"
                );
                return;
            }

            for (int i = 0; i < nodeCount; i++)
            {
                Vector3 nodePosition = nodePositions[i];
                Vector3 scale = Vector3.one * nodeSize;

                matrices[i] = Matrix4x4.TRS(nodePosition, Quaternion.identity, scale);
            }
        }

        void RenderNodes()
        {
            if (nodeMaterial == null || nodeMesh == null || nodeCount <= 0 || matrices == null)
                return;

            try
            {
                Graphics.RenderMeshInstanced(renderParams, nodeMesh, 0, matrices, nodeCount);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"NodeRenderer error: {e.Message}");
                enabled = false;
            }
        }

        void OnDestroy()
        {
            if (nodeMesh != null)
                DestroyImmediate(nodeMesh);
            if (nodeMaterial != null)
                DestroyImmediate(nodeMaterial);
        }

        void OnValidate()
        {
            nodeSize = Mathf.Max(0.1f, nodeSize);
        }

        // Debug helper
        void OnDrawGizmosSelected()
        {
            if (segmentRenderer == null || !Application.isPlaying)
                return;

            Vector3[] positions = segmentRenderer.GetNodePositions();
            if (positions == null)
                return;

            Gizmos.color = normalColor;
            for (int i = 0; i < positions.Length; i++)
            {
                Gizmos.DrawWireSphere(positions[i], nodeSize * 0.5f);

                // Draw node index for debugging
#if UNITY_EDITOR
                UnityEditor.Handles.Label(positions[i] + Vector3.up * 0.3f, i.ToString());
#endif
            }
        }
    }
}

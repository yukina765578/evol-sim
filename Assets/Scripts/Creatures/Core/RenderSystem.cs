using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class RenderSystem : MonoBehaviour
    {
        [Header("Rendering Settings")]
        [SerializeField]
        private bool enableRendering = true;

        [SerializeField]
        private bool enableDebugThrust = false;

        [SerializeField]
        private bool enableVelocityDebug = false;

        [Header("Materials")]
        [SerializeField]
        private Material nodeMaterial;

        [SerializeField]
        private Material segmentMaterial;

        [SerializeField]
        private Material debugMaterial;

        private CreatureState creatureState;
        private Mesh nodeMesh;
        private LineRenderer[] segmentRenderers;
        private LineRenderer velocityRenderer;

        private Material nodeMatInstance;
        private Material segmentMatInstance;
        private Material debugMatInstance;

        public void Initialize(CreatureState state)
        {
            creatureState = state;
            SetupMaterials();
            SetupMeshes();
            SetupLineRenderers();
            SetupDebugRenderer();
        }

        void SetupMaterials()
        {
            // Create material instances to avoid modifying shared materials
            nodeMatInstance = new Material(Shader.Find("Sprites/Default"));
            segmentMatInstance = new Material(Shader.Find("Sprites/Default"));
            debugMatInstance = new Material(Shader.Find("Sprites/Default"));
        }

        void SetupMeshes()
        {
            nodeMesh = Utils.CreateQuadMesh();
        }

        void SetupLineRenderers()
        {
            segmentRenderers = new LineRenderer[creatureState.segments.Length];

            for (int i = 0; i < creatureState.segments.Length; i++)
            {
                GameObject lineObj = new GameObject($"SegmentLine_{i}");
                lineObj.transform.SetParent(transform);

                LineRenderer line = lineObj.AddComponent<LineRenderer>();
                line.material = segmentMatInstance;
                line.positionCount = 2;
                line.useWorldSpace = true;
                line.sortingOrder = 0;
                line.startWidth = creatureState.segments[i].width;
                line.endWidth = creatureState.segments[i].width;
                line.startColor = creatureState.segments[i].color;
                line.endColor = creatureState.segments[i].color;

                segmentRenderers[i] = line;
            }
        }

        void SetupDebugRenderer()
        {
            GameObject debugObj = new GameObject("DebugRenderer");
            debugObj.transform.SetParent(transform);

            velocityRenderer = debugObj.AddComponent<LineRenderer>();
            velocityRenderer.material = debugMatInstance;
            velocityRenderer.positionCount = 2;
            velocityRenderer.useWorldSpace = true;
            velocityRenderer.sortingOrder = 10;
            velocityRenderer.startWidth = 0.1f;
            velocityRenderer.endWidth = 0.1f;
            velocityRenderer.startColor = Color.red;
            velocityRenderer.endColor = Color.red;
            velocityRenderer.enabled = enableVelocityDebug;
        }

        void Update()
        {
            if (!enableRendering || !creatureState.isInitialized)
                return;

            RenderNodes();
            RenderSegments();

            if (enableVelocityDebug)
                UpdateVelocityDebug();
        }

        void RenderNodes()
        {
            for (int i = 0; i < creatureState.nodes.Length; i++)
            {
                NodeData node = creatureState.nodes[i];
                Color nodeColor = DataConstants.DEFAULT_NODE_COLOR;

                Energy energy = GetComponent<Energy>();
                if (energy != null && energy.IsReproductionReady)
                {
                    nodeColor = DataConstants.REPRODUCTION_COLOR;
                }

                Matrix4x4 matrix = Matrix4x4.TRS(
                    node.position,
                    Quaternion.identity,
                    Vector3.one * node.size
                );

                nodeMatInstance.color = nodeColor;
                Graphics.DrawMesh(nodeMesh, matrix, nodeMatInstance, 0);
            }
        }

        void RenderSegments()
        {
            for (int i = 0; i < creatureState.segments.Length; i++)
            {
                SegmentData segment = creatureState.segments[i];

                if (segmentRenderers[i] == null)
                    continue;

                Vector3 startPos = creatureState.nodes[segment.parentIndex].position;
                Vector3 endPos = creatureState.nodes[segment.childIndex].position;
                segmentRenderers[i].SetPosition(0, startPos);
                segmentRenderers[i].SetPosition(1, endPos);
                segmentRenderers[i].enabled = enableRendering;
            }
        }

        void UpdateVelocityDebug()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null || !enableVelocityDebug)
                return;

            Vector2 velocity = rb.linearVelocity;
            Vector3 rootPos = creatureState.nodes[0].position;
            Vector3 startPos = rootPos + (Vector3)velocity.normalized * 1.2f;
            Vector3 endPos = startPos + (Vector3)velocity * 2f;

            velocityRenderer.SetPosition(0, startPos);
            velocityRenderer.SetPosition(1, endPos);
        }

        public void SetDebugMode(bool enableThrust, bool enableVelocity)
        {
            enableDebugThrust = enableThrust;
            enableVelocityDebug = enableVelocity;

            if (velocityRenderer != null)
                velocityRenderer.enabled = enableVelocityDebug;
        }

        public void UpdateCreatureState(CreatureState newState)
        {
            creatureState = newState;
        }

        void OnDestroy()
        {
            if (nodeMatInstance != null)
                Destroy(nodeMatInstance);
            if (segmentMatInstance != null)
                Destroy(segmentMatInstance);
            if (debugMatInstance != null)
                Destroy(debugMatInstance);

            if (nodeMesh != null)
                Destroy(nodeMesh);
        }

        void OnValidate()
        {
            if (Application.isPlaying && velocityRenderer != null)
            {
                velocityRenderer.enabled = enableVelocityDebug;
            }
        }
    }
}

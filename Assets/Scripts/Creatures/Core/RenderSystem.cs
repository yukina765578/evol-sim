using UnityEngine;

namespace EvolutionSimulator.Creatures.Core
{
    public class RenderSystem : MonoBehaviour
    {
        [Header("Render Settings")]
        [SerializeField]
        private bool enableDebugThrust = false;

        [SerializeField]
        private bool enableDebugVelocity = false;

        [Header("Node Texture")]
        [SerializeField]
        private Texture2D nodeTexture;

        [Header("Materials")]
        [SerializeField]
        private Material nodeMaterial;

        [SerializeField]
        private Material segmentMaterial;

        [SerializeField]
        private Material debugMaterial;

        [SerializeField]
        private Material debugThrustMaterial;

        private CreatureState creatureState;
        private Mesh nodeMesh;
        private LineRenderer[] segmentRenderers;
        private LineRenderer velocityRenderer;
        private LineRenderer thrustRenderer;

        private Material nodeMatInstance;
        private Material segmentMatInstance;
        private Material debugMatInstance;
        private Material debugThrustMatInstance;

        public void Initialize(CreatureState state)
        {
            creatureState = state;
            SetupMaterials();
            SetupMeshes();
            SetupLineRenderers();
            SetupDebugRenderers();
        }

        void SetupMaterials()
        {
            // Create material instance to avoid modifying shared materials
            nodeMatInstance = new Material(Shader.Find("Sprites/Default"));
            segmentMatInstance = new Material(Shader.Find("Sprites/Default"));
            debugMatInstance = new Material(Shader.Find("Sprites/Default"));
            debugThrustMatInstance = new Material(Shader.Find("Sprites/Default"));

            // Auto-assign node texture if not manually set
            if (nodeTexture == null)
            {
                nodeTexture = Resources.Load<Texture2D>("Circle");
                if (nodeTexture == null)
                {
                    // Fallback: search for any texture named "Circle"
                    Texture2D[] allTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
                    foreach (Texture2D tex in allTextures)
                    {
                        if (tex.name == "Circle")
                        {
                            nodeTexture = tex;
                            break;
                        }
                    }
                }
            }

            // Apply the texture
            if (nodeTexture != null)
            {
                nodeMatInstance.mainTexture = nodeTexture;
            }
            else
            {
                Debug.LogWarning("Circle texture not found! Nodes will be squares.");
            } // Auto-assign node texture if not manually set
            if (nodeTexture == null)
            {
                nodeTexture = Resources.Load<Texture2D>("Circle");
                if (nodeTexture == null)
                {
                    // Fallback: search for any texture named "Circle"
                    Texture2D[] allTextures = Resources.FindObjectsOfTypeAll<Texture2D>();
                    foreach (Texture2D tex in allTextures)
                    {
                        if (tex.name == "Circle")
                        {
                            nodeTexture = tex;
                            break;
                        }
                    }
                }
            }

            // Apply the texture
            if (nodeTexture != null)
            {
                nodeMatInstance.mainTexture = nodeTexture;
            }
            else
            {
                Debug.LogWarning("Circle texture not found! Nodes will be squares.");
            }
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
                GameObject lineObj = new GameObject($"SegmentRenderer_{i}");
                lineObj.transform.SetParent(transform);

                LineRenderer lr = lineObj.AddComponent<LineRenderer>();
                lr.material = segmentMatInstance;
                lr.positionCount = 2;
                lr.useWorldSpace = false;
                lr.sortingOrder = 0;
                lr.startWidth = DataConstants.DEFAULT_SEGMENT_WIDTH;
                lr.endWidth = DataConstants.DEFAULT_SEGMENT_WIDTH;
                lr.startColor = DataConstants.DEFAULT_SEGMENT_COLOR;
                lr.endColor = DataConstants.DEFAULT_SEGMENT_COLOR;
                segmentRenderers[i] = lr;
            }
        }

        void SetupDebugRenderers()
        {
            GameObject velocityObj = new GameObject("VelocityRenderer");
            velocityObj.transform.SetParent(transform);

            velocityRenderer = velocityObj.AddComponent<LineRenderer>();
            velocityRenderer.material = debugMatInstance;
            velocityRenderer.positionCount = 2;
            velocityRenderer.useWorldSpace = false;
            velocityRenderer.startWidth = 0.1f;
            velocityRenderer.endWidth = 0.1f;
            velocityRenderer.startColor = Color.green;
            velocityRenderer.endColor = Color.green;
            velocityRenderer.enabled = enableDebugVelocity;
            velocityRenderer.sortingOrder = 10;

            GameObject thrustObj = new GameObject("ThrustRenderer");
            thrustObj.transform.SetParent(transform);

            thrustRenderer = thrustObj.AddComponent<LineRenderer>();
            thrustRenderer.material = debugThrustMatInstance;
            thrustRenderer.positionCount = 2;
            thrustRenderer.useWorldSpace = false;
            thrustRenderer.startWidth = 0.1f;
            thrustRenderer.endWidth = 0.1f;
            thrustRenderer.startColor = Color.red;
            thrustRenderer.endColor = Color.red;
            thrustRenderer.enabled = enableDebugThrust;
            thrustRenderer.sortingOrder = 10;
        }

        void Update()
        {
            if (!creatureState.isInitialized)
                return;

            RenderNodes();
            RenderSegments();

            if (enableDebugVelocity)
                UpdateVelocityDebug();

            if (enableDebugThrust)
                UpdateThrustDebug();
        }

        void RenderNodes()
        {
            for (int i = 0; i < creatureState.nodes.Length; i++)
            {
                NodeData node = creatureState.nodes[i];
                Color nodeColor = DataConstants.DEFAULT_NODE_COLOR;

                Energy energy = GetComponent<Energy>();
                if (energy != null)
                    nodeColor = DataConstants.REPRODUCTION_COLOR;

                Vector3 localPosition = node.position;
                Vector3 worldPosition = transform.TransformPoint(localPosition);

                Matrix4x4 matrix = Matrix4x4.TRS(
                    worldPosition,
                    transform.rotation,
                    Vector3.one * DataConstants.DEFAULT_NODE_SIZE
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
            }
        }

        void UpdateVelocityDebug()
        {
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb == null || !enableDebugVelocity)
                return;

            Vector2 velocity = rb.linearVelocity;
            Vector3 rootPos = creatureState.nodes[0].position;

            Vector2 localVelocity = transform.InverseTransformDirection(velocity);

            Vector3 startPos = rootPos + (Vector3)localVelocity.normalized * 1.2f;
            Vector3 endPos = startPos + (Vector3)localVelocity * 2f;

            velocityRenderer.SetPosition(0, startPos);
            velocityRenderer.SetPosition(1, endPos);
        }

        void UpdateThrustDebug()
        {
            // TODO: Implement thrust debug visualization
            // This is a placeholder for thrust visualization logic
        }

        public void SetupDebugMode(bool enableThrust, bool enableVelocity)
        {
            enableDebugThrust = enableThrust;
            enableDebugVelocity = enableVelocity;

            if (velocityRenderer != null)
                velocityRenderer.enabled = enableDebugVelocity;

            if (thrustRenderer != null)
                thrustRenderer.enabled = enableDebugThrust;
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
            if (debugThrustMatInstance != null)
                Destroy(debugThrustMatInstance);
            if (nodeMesh != null)
                Destroy(nodeMesh);
        }

        void OnValidate()
        {
            if (Application.isPlaying && velocityRenderer != null)
            {
                velocityRenderer.enabled = enableDebugVelocity;
            }
        }
    }
}

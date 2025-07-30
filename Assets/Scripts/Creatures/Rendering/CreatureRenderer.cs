using System.Collections.Generic;
using System.Runtime.InteropServices;
using EvolutionSimulator.Creatures.Population;
using UnityEngine;

namespace EvolutionSimulator.Creatures.Rendering
{
    public class CreatureRenderer : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField]
        private Material nodeMaterial;

        [SerializeField]
        private Material segmentMaterial;

        [SerializeField]
        private int maxNodes = 10000;

        [SerializeField]
        private int maxSegments = 10000;

        [Header("Meshes")]
        [SerializeField]
        private Mesh nodeMesh;

        [SerializeField]
        private Mesh segmentMesh;

        private Manager populationManager;
        private List<Core.Controller> trackedCreatures = new List<Core.Controller>();

        // GPU Buffers
        private GraphicsBuffer nodePositionBuffer;
        private GraphicsBuffer nodeDataBuffer;
        private GraphicsBuffer segmentPositionBuffer;
        private GraphicsBuffer segmentDataBuffer;

        // CPU Arrays
        private Vector3[] nodePositions;
        private NodeRenderInfo[] nodeRenderData;
        private Vector3[] segmentPositions;
        private SegmentRenderInfo[] segmentRenderData;

        private int activeNodeCount = 0;
        private int activeSegmentCount = 0;
        private Bounds renderBounds;

        [StructLayout(LayoutKind.Sequential)]
        public struct NodeRenderInfo
        {
            public Vector3 position;
            public float size;
            public Vector4 color;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct SegmentRenderInfo
        {
            public Vector3 startPos;
            public Vector3 endPos;
            public float width;
            public Vector4 color;
        }

        void Start()
        {
            populationManager = FindFirstObjectByType<Manager>();
            if (populationManager == null)
            {
                Debug.LogError("CreatureRenderer requires Manager in scene!");
                return;
            }

            CreateMeshes();
            InitializeBuffers();
            renderBounds = new Bounds(Vector3.zero, Vector3.one * 500f);
        }

        void CreateMeshes()
        {
            if (nodeMesh == null)
            {
                nodeMesh = CreateQuadMesh();
            }

            if (segmentMesh == null)
            {
                segmentMesh = CreateQuadMesh();
            }
        }

        Mesh CreateQuadMesh()
        {
            Mesh mesh = new Mesh();
            Vector3[] vertices =
            {
                new Vector3(-0.5f, -0.5f, 0f),
                new Vector3(0.5f, -0.5f, 0f),
                new Vector3(0.5f, 0.5f, 0f),
                new Vector3(-0.5f, 0.5f, 0f),
            };
            int[] triangles = { 0, 2, 1, 0, 3, 2 };
            Vector2[] uvs =
            {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f),
            };

            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            return mesh;
        }

        void InitializeBuffers()
        {
            // Node buffers
            nodePositions = new Vector3[maxNodes];
            nodeRenderData = new NodeRenderInfo[maxNodes];

            nodePositionBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                maxNodes,
                Marshal.SizeOf<Vector3>()
            );
            nodeDataBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                maxNodes,
                Marshal.SizeOf<NodeRenderInfo>()
            );

            // Segment buffers
            segmentPositions = new Vector3[maxSegments];
            segmentRenderData = new SegmentRenderInfo[maxSegments];

            segmentPositionBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                maxSegments,
                Marshal.SizeOf<Vector3>()
            );
            segmentDataBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                maxSegments,
                Marshal.SizeOf<SegmentRenderInfo>()
            );
        }

        void Update()
        {
            UpdateCreatureList();
            CollectRenderData();
            UpdateBuffers();
            RenderCreatures();
        }

        void UpdateCreatureList()
        {
            trackedCreatures.RemoveAll(creature => creature == null);

            if (populationManager != null)
            {
                foreach (GameObject creatureObj in populationManager.Creatures)
                {
                    if (creatureObj != null)
                    {
                        var controller = creatureObj.GetComponent<Core.Controller>();
                        if (controller != null && !trackedCreatures.Contains(controller))
                        {
                            trackedCreatures.Add(controller);
                        }
                    }
                }
            }
        }

        void CollectRenderData()
        {
            activeNodeCount = 0;
            activeSegmentCount = 0;

            foreach (var controller in trackedCreatures)
            {
                if (controller?.GetRenderData() == null)
                    continue;

                var renderData = controller.GetRenderData();

                // Collect nodes
                for (int i = 0; i < renderData.nodes.Length && activeNodeCount < maxNodes; i++)
                {
                    var node = renderData.nodes[i];
                    nodeRenderData[activeNodeCount] = new NodeRenderInfo
                    {
                        position = node.worldPosition,
                        size = node.size,
                        color = new Vector4(node.color.r, node.color.g, node.color.b, node.color.a),
                    };
                    activeNodeCount++;
                }

                // Collect segments
                for (
                    int i = 0;
                    i < renderData.segments.Length && activeSegmentCount < maxSegments;
                    i++
                )
                {
                    var segment = renderData.segments[i];
                    segmentRenderData[activeSegmentCount] = new SegmentRenderInfo
                    {
                        startPos = segment.startPoint,
                        endPos = segment.endPoint,
                        width = segment.width,
                        color = new Vector4(
                            segment.color.r,
                            segment.color.g,
                            segment.color.b,
                            segment.color.a
                        ),
                    };
                    activeSegmentCount++;
                }
            }
        }

        void UpdateBuffers()
        {
            if (activeNodeCount > 0)
            {
                nodeDataBuffer.SetData(nodeRenderData, 0, 0, activeNodeCount);
            }

            if (activeSegmentCount > 0)
            {
                segmentDataBuffer.SetData(segmentRenderData, 0, 0, activeSegmentCount);
            }
        }

        void RenderCreatures()
        {
            // Render nodes
            if (nodeMaterial != null && nodeMesh != null && activeNodeCount > 0)
            {
                nodeMaterial.SetBuffer("_NodeData", nodeDataBuffer);
                Graphics.DrawMeshInstancedProcedural(
                    nodeMesh,
                    0,
                    nodeMaterial,
                    renderBounds,
                    activeNodeCount
                );
            }

            // Render segments
            if (segmentMaterial != null && segmentMesh != null && activeSegmentCount > 0)
            {
                segmentMaterial.SetBuffer("_SegmentData", segmentDataBuffer);
                Graphics.DrawMeshInstancedProcedural(
                    segmentMesh,
                    0,
                    segmentMaterial,
                    renderBounds,
                    activeSegmentCount
                );
            }
        }

        void OnDestroy()
        {
            nodePositionBuffer?.Dispose();
            nodeDataBuffer?.Dispose();
            segmentPositionBuffer?.Dispose();
            segmentDataBuffer?.Dispose();
        }
    }
}

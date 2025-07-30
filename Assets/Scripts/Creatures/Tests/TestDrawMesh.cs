using System.Runtime.InteropServices;
using UnityEngine;

namespace TestGPUInstancing
{
    public class CircleInstancedTest : MonoBehaviour
    {
        [SerializeField]
        private Material material;

        [SerializeField]
        private int circleCount = 100;

        [SerializeField]
        private float spawnRadius = 20f;

        [SerializeField]
        private float circleSize = 1f;

        private Mesh circleMesh;
        private GraphicsBuffer positionBuffer;
        private Vector3[] positions;
        private Bounds renderBounds;

        void Start()
        {
            CreateQuadMesh();
            GenerateInstances();
            SetupBuffer();
        }

        void CreateQuadMesh()
        {
            circleMesh = new Mesh();
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

            circleMesh.vertices = vertices;
            circleMesh.triangles = triangles;
            circleMesh.uv = uvs;
        }

        void GenerateInstances()
        {
            positions = new Vector3[circleCount];

            for (int i = 0; i < circleCount; i++)
            {
                Vector3 position = Random.insideUnitSphere * spawnRadius;
                position.z = 0f; // Keep in 2D plane
                positions[i] = position;
            }

            renderBounds = new Bounds(Vector3.zero, Vector3.one * spawnRadius * 2f);
        }

        void SetupBuffer()
        {
            positionBuffer = new GraphicsBuffer(
                GraphicsBuffer.Target.Structured,
                circleCount,
                Marshal.SizeOf<Vector3>()
            );

            positionBuffer.SetData(positions);
            material.SetBuffer("_Positions", positionBuffer);
            material.SetFloat("_CircleSize", circleSize);
        }

        void Update()
        {
            if (circleMesh != null && material != null && positionBuffer != null)
            {
                Graphics.DrawMeshInstancedProcedural(
                    circleMesh,
                    0,
                    material,
                    renderBounds,
                    circleCount
                );
            }
        }

        void OnDestroy()
        {
            positionBuffer?.Dispose();
            if (circleMesh != null)
            {
                DestroyImmediate(circleMesh);
            }
        }

        [ContextMenu("Regenerate Instances")]
        void RegenerateInstances()
        {
            GenerateInstances();
            if (positionBuffer != null)
            {
                positionBuffer.SetData(positions);
            }
        }
    }
}

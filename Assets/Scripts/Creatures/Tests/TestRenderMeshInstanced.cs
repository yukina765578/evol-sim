using UnityEngine;
using UnityEngine.Rendering;

public class RenderMeshInstancedTest : MonoBehaviour
{
    [Header("Rendering")]
    [SerializeField]
    public Material circleMaterial;

    [SerializeField]
    public int instanceCount = 100;

    [SerializeField]
    public float circleSize = 0.5f;

    [Header("Movement")]
    [SerializeField]
    public float moveSpeed = 2f;

    [SerializeField]
    public float spawnRadius = 10f;

    private Mesh quadMesh;
    private Matrix4x4[] matrices;
    private Vector2[] positions;
    private Vector2[] velocities;
    private RenderParams renderParams;

    void Start()
    {
        CreateQuadMesh();
        InitializeInstances();
        SetupRenderParams();
    }

    void CreateQuadMesh()
    {
        quadMesh = new Mesh();

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

        int[] triangles = new int[] { 0, 2, 1, 0, 3, 2 };

        quadMesh.vertices = vertices;
        quadMesh.uv = uvs;
        quadMesh.triangles = triangles;
        quadMesh.RecalculateNormals();
        quadMesh.RecalculateBounds();
    }

    void InitializeInstances()
    {
        matrices = new Matrix4x4[instanceCount];
        positions = new Vector2[instanceCount];
        velocities = new Vector2[instanceCount];

        for (int i = 0; i < instanceCount; i++)
        {
            // Random starting position
            positions[i] = Random.insideUnitCircle * spawnRadius;

            // Random starting velocity
            velocities[i] = Random.insideUnitCircle.normalized * moveSpeed;

            // Create initial matrix
            matrices[i] = Matrix4x4.TRS(
                new Vector3(positions[i].x, positions[i].y, 0),
                Quaternion.identity,
                Vector3.one * circleSize
            );
        }
    }

    void SetupRenderParams()
    {
        if (circleMaterial == null)
        {
            Debug.LogError("Circle material is not assigned!");
            return;
        }

        renderParams = new RenderParams(circleMaterial)
        {
            receiveShadows = false,
            shadowCastingMode = ShadowCastingMode.Off,
            layer = gameObject.layer,
            worldBounds = new Bounds(Vector3.zero, Vector3.one * spawnRadius * 2),
        };
    }

    void Update()
    {
        UpdatePositions();
        UpdateMatrices();
        RenderInstances();
    }

    void UpdatePositions()
    {
        for (int i = 0; i < instanceCount; i++)
        {
            // Move position
            positions[i] += velocities[i] * Time.deltaTime;

            // Bounce off boundaries
            if (Mathf.Abs(positions[i].x) > spawnRadius)
            {
                velocities[i].x *= -1;
                positions[i].x = Mathf.Clamp(positions[i].x, -spawnRadius, spawnRadius);
            }

            if (Mathf.Abs(positions[i].y) > spawnRadius)
            {
                velocities[i].y *= -1;
                positions[i].y = Mathf.Clamp(positions[i].y, -spawnRadius, spawnRadius);
            }

            // Occasionally change direction
            if (Random.value < 0.01f)
            {
                velocities[i] = Random.insideUnitCircle.normalized * moveSpeed;
            }
        }
    }

    void UpdateMatrices()
    {
        for (int i = 0; i < instanceCount; i++)
        {
            matrices[i] = Matrix4x4.TRS(
                new Vector3(positions[i].x, positions[i].y, 0),
                Quaternion.identity,
                Vector3.one * circleSize
            );
        }
    }

    void RenderInstances()
    {
        if (circleMaterial != null && quadMesh != null)
        {
            Graphics.RenderMeshInstanced(renderParams, quadMesh, 0, matrices, instanceCount);
        }
    }

    void OnDestroy()
    {
        if (quadMesh != null)
        {
            DestroyImmediate(quadMesh);
        }
    }

    void OnValidate()
    {
        instanceCount = Mathf.Max(1, instanceCount);
        circleSize = Mathf.Max(0.1f, circleSize);
        moveSpeed = Mathf.Max(0f, moveSpeed);
        spawnRadius = Mathf.Max(1f, spawnRadius);
    }
}

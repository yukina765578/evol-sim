using UnityEngine;
using UnityEngine.Rendering;

public class RectangleInstance
{
    public Vector3 position;
    public Vector2 size;
    public Vector2 rotationPivot;
    public float rotationSpeed;
    public float currentRotation;

    public RectangleInstance(Vector3 pos, Vector2 sz, Vector2 pivot, float speed)
    {
        position = pos;
        size = sz;
        rotationPivot = pivot;
        rotationSpeed = speed;
        currentRotation = Random.Range(0f, 360f);
    }
}

public class InstancedRectangleRenderer : MonoBehaviour
{
    [Header("基本設定")]
    public Material rectangleMaterial;
    public int instanceCount = 50;

    [Header("インスタンス設定")]
    public Vector2 spawnAreaSize = new Vector2(20f, 15f);
    public Vector2 rectangleSize = new Vector2(0.5f, 3f); // sizeRange から rectangleSize に変更
    public Vector2 rotationSpeedRange = new Vector2(10f, 60f);
    public bool randomizePivots = true;

    [Header("デバッグ")]
    public bool showGizmos = false;

    private Mesh rectangleMesh;
    private RenderParams renderParams;
    private RectangleInstance[] instances;
    private Matrix4x4[] matrices;

    void Start()
    {
        CreateRectangleMesh();
        SetupRenderParams();
        InitializeInstances();
    }

    void CreateRectangleMesh()
    {
        rectangleMesh = new Mesh();

        // 長方形の頂点座標（中心が原点）
        Vector3[] vertices = new Vector3[4]
        {
            new Vector3(-0.5f, -0.5f, 0), // 左下
            new Vector3(0.5f, -0.5f, 0), // 右下
            new Vector3(0.5f, 0.5f, 0), // 右上
            new Vector3(-0.5f, 0.5f, 0), // 左上
        };

        // UV座標
        Vector2[] uvs = new Vector2[4]
        {
            new Vector2(0, 0), // 左下
            new Vector2(1, 0), // 右下
            new Vector2(1, 1), // 右上
            new Vector2(0, 1), // 左上
        };

        // 三角形のインデックス（時計回り）
        int[] triangles = new int[6]
        {
            0,
            1,
            2, // 最初の三角形
            0,
            2,
            3, // 二番目の三角形
        };

        rectangleMesh.vertices = vertices;
        rectangleMesh.uv = uvs;
        rectangleMesh.triangles = triangles;
        rectangleMesh.RecalculateNormals();
        rectangleMesh.RecalculateBounds();
    }

    void SetupRenderParams()
    {
        renderParams = new RenderParams(rectangleMaterial);

        // 2D用の設定
        renderParams.camera = Camera.main;
        renderParams.layer = gameObject.layer;
        renderParams.renderingLayerMask = 1;
    }

    void InitializeInstances()
    {
        instances = new RectangleInstance[instanceCount];
        matrices = new Matrix4x4[instanceCount];

        for (int i = 0; i < instanceCount; i++)
        {
            // ランダムな位置（スポーンエリア内）
            Vector3 randomPosition = new Vector3(
                Random.Range(-spawnAreaSize.x * 0.5f, spawnAreaSize.x * 0.5f),
                Random.Range(-spawnAreaSize.y * 0.5f, spawnAreaSize.y * 0.5f),
                0
            );

            // 固定サイズを使用
            Vector2 instanceSize = rectangleSize;

            // ランダムな回転軸
            Vector2 randomPivot = Vector2.zero;
            if (randomizePivots)
            {
                randomPivot = new Vector2(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f));
            }

            // ランダムな回転速度
            float randomSpeed = Random.Range(rotationSpeedRange.x, rotationSpeedRange.y);
            // ランダムに回転方向を決める
            if (Random.value < 0.5f)
                randomSpeed *= -1f;

            instances[i] = new RectangleInstance(
                randomPosition,
                instanceSize,
                randomPivot,
                randomSpeed
            );
        }
    }

    void Update()
    {
        if (rectangleMesh != null && rectangleMaterial != null && instances != null)
        {
            UpdateInstanceTransforms();

            // 全インスタンスを一度にレンダリング
            Graphics.RenderMeshInstanced(renderParams, rectangleMesh, 0, matrices);
        }
    }

    void UpdateInstanceTransforms()
    {
        for (int i = 0; i < instances.Length; i++)
        {
            var instance = instances[i];

            // 回転の更新
            instance.currentRotation += instance.rotationSpeed * Time.deltaTime;
            instance.currentRotation = instance.currentRotation % 360f;

            // 変換マトリックスの計算
            matrices[i] = CalculateTransformMatrix(instance);
        }
    }

    Matrix4x4 CalculateTransformMatrix(RectangleInstance instance)
    {
        // スケールマトリックス
        Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(instance.size.x, instance.size.y, 1));

        // 回転軸へのオフセット（スケール適用後の座標系で）
        Vector3 pivotOffset = new Vector3(
            instance.rotationPivot.x * instance.size.x,
            instance.rotationPivot.y * instance.size.y,
            0
        );

        // 回転マトリックス（Z軸回転）
        Matrix4x4 rotationMatrix = Matrix4x4.Rotate(
            Quaternion.Euler(0, 0, instance.currentRotation)
        );

        // 位置マトリックス
        Matrix4x4 positionMatrix = Matrix4x4.Translate(instance.position);

        // 回転軸オフセットマトリックス
        Matrix4x4 pivotTranslate = Matrix4x4.Translate(pivotOffset);
        Matrix4x4 pivotTranslateInverse = Matrix4x4.Translate(-pivotOffset);

        // 変換の順序：
        // 1. スケール適用
        // 2. 回転軸へ移動
        // 3. 回転
        // 4. 回転軸から戻す
        // 5. 最終位置へ移動
        return positionMatrix
            * pivotTranslate
            * rotationMatrix
            * pivotTranslateInverse
            * scaleMatrix;
    }

    void OnDestroy()
    {
        if (rectangleMesh != null)
        {
            DestroyImmediate(rectangleMesh);
        }
    }

    // Gizmosでスポーンエリアとインスタンスを表示
    void OnDrawGizmos()
    {
        // スポーンエリアを表示
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(
            transform.position,
            new Vector3(spawnAreaSize.x, spawnAreaSize.y, 0.1f)
        );

        if (!showGizmos || instances == null)
            return;

        // 各インスタンスの情報を表示
        for (int i = 0; i < instances.Length; i++)
        {
            var instance = instances[i];

            // 長方形の外枠
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(
                instance.position,
                new Vector3(instance.size.x, instance.size.y, 0.1f)
            );

            // 回転軸
            Gizmos.color = Color.red;
            Vector3 pivotWorldPos =
                instance.position
                + new Vector3(
                    instance.rotationPivot.x * instance.size.x,
                    instance.rotationPivot.y * instance.size.y,
                    0
                );
            Gizmos.DrawWireSphere(pivotWorldPos, 0.05f);
        }
    }

    // インスペクター用のボタン
    [ContextMenu("Regenerate Instances")]
    public void RegenerateInstances()
    {
        if (Application.isPlaying)
        {
            InitializeInstances();
        }
    }
}

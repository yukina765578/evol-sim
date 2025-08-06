using UnityEngine;

public class InstancedRectangleManager : MonoBehaviour
{
    [Header("Manager Settings")]
    public int objectCount = 500;

    [Header("Rectangle Settings")]
    public Material rectangleMaterial;
    public int instanceCount = 100;
    public Vector2 rectangleSize = new Vector2(1f, 0.5f);
    public Vector2 spawnAreaSize = new Vector2(20f, 15f);
    public Vector2 rotationSpeedRange = new Vector2(10f, 60f);
    public bool randomizePivots = true;

    [Header("Movement Settings")]
    public bool enableMovement = false;
    public float moveSpeed = 2f;
    public float moveRadius = 5f;

    [Header("Distribution Settings")]
    public float objectSpacing = 25f; // オブジェクト間の距離
    public int objectsPerRow = 10; // 1行あたりのオブジェクト数

    [Header("Variation Settings")]
    public bool varyInstanceCount = false;
    public Vector2Int instanceCountRange = new Vector2Int(50, 150);
    public bool varySizes = false;
    public Vector2 sizeVariationRange = new Vector2(0.5f, 2f);

    private GameObject[] objects;
    private Vector3[] initialPositions;

    void Start()
    {
        CreateInstancedObjects();
    }

    void CreateInstancedObjects()
    {
        objects = new GameObject[objectCount];
        initialPositions = new Vector3[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            // オブジェクト作成
            GameObject obj = new GameObject($"InstancedRectangles_{i:D3}");
            obj.transform.parent = this.transform;

            // 位置計算（グリッド配置）
            int row = i / objectsPerRow;
            int col = i % objectsPerRow;
            Vector3 position = new Vector3(
                col * objectSpacing - (objectsPerRow - 1) * objectSpacing * 0.5f,
                row * objectSpacing - (objectCount / objectsPerRow) * objectSpacing * 0.5f,
                0
            );

            obj.transform.position = position;
            initialPositions[i] = position;

            // InstancedRectangleRenderer コンポーネント追加
            var instanced = obj.AddComponent<InstancedRectangleRenderer>();

            // 設定を渡す
            ConfigureInstancedRenderer(instanced, i);

            objects[i] = obj;
        }

        Debug.Log(
            $"Created {objectCount} objects with {instanceCount} instances each. Total rectangles: {objectCount * instanceCount}"
        );
    }

    void ConfigureInstancedRenderer(InstancedRectangleRenderer renderer, int objectIndex)
    {
        // 基本設定
        renderer.rectangleMaterial = rectangleMaterial;

        // インスタンス数の設定
        if (varyInstanceCount)
        {
            renderer.instanceCount = Random.Range(instanceCountRange.x, instanceCountRange.y);
        }
        else
        {
            renderer.instanceCount = instanceCount;
        }

        // サイズの設定
        if (varySizes)
        {
            float sizeMultiplier = Random.Range(sizeVariationRange.x, sizeVariationRange.y);
            renderer.rectangleSize = rectangleSize * sizeMultiplier;
        }
        else
        {
            renderer.rectangleSize = rectangleSize;
        }

        // スポーンエリアの設定（わずかにバリエーション）
        renderer.spawnAreaSize = spawnAreaSize + Vector2.one * (objectIndex * 0.1f);

        // 回転設定
        renderer.rotationSpeedRange = rotationSpeedRange;
        renderer.randomizePivots = randomizePivots;

        // その他の設定
        renderer.showGizmos = false; // パフォーマンスのため無効
    }

    void Update()
    {
        if (enableMovement && objects != null)
        {
            UpdateObjectMovement();
        }
    }

    void UpdateObjectMovement()
    {
        for (int i = 0; i < objects.Length; i++)
        {
            if (objects[i] != null)
            {
                // 円運動
                float angle = Time.time * moveSpeed + i * 0.5f;
                Vector3 offset = new Vector3(
                    Mathf.Cos(angle) * moveRadius,
                    Mathf.Sin(angle) * moveRadius,
                    0
                );

                objects[i].transform.position = initialPositions[i] + offset;
            }
        }
    }

    // インスペクター用の便利メソッド
    [ContextMenu("Regenerate All Objects")]
    public void RegenerateAllObjects()
    {
        if (Application.isPlaying)
        {
            // 既存のオブジェクトを削除
            DestroyAllObjects();

            // 新しく作成
            CreateInstancedObjects();
        }
    }

    [ContextMenu("Destroy All Objects")]
    public void DestroyAllObjects()
    {
        if (objects != null)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                if (objects[i] != null)
                {
                    if (Application.isPlaying)
                        Destroy(objects[i]);
                    else
                        DestroyImmediate(objects[i]);
                }
            }
            objects = null;
        }
    }

    void OnDestroy()
    {
        DestroyAllObjects();
    }

    // Gizmosでオブジェクトの配置を表示
    void OnDrawGizmos()
    {
        if (objects == null || objects.Length == 0)
        {
            // 配置予定位置を表示
            Gizmos.color = Color.gray;
            for (int i = 0; i < objectCount; i++)
            {
                int row = i / objectsPerRow;
                int col = i % objectsPerRow;
                Vector3 position =
                    transform.position
                    + new Vector3(
                        col * objectSpacing - (objectsPerRow - 1) * objectSpacing * 0.5f,
                        row * objectSpacing - (objectCount / objectsPerRow) * objectSpacing * 0.5f,
                        0
                    );

                Gizmos.DrawWireCube(position, Vector3.one * 2f);
            }
        }
    }

    // 統計情報を取得
    public int GetTotalRectangleCount()
    {
        if (objects == null)
            return 0;

        int total = 0;
        foreach (var obj in objects)
        {
            if (obj != null)
            {
                var renderer = obj.GetComponent<InstancedRectangleRenderer>();
                if (renderer != null)
                {
                    total += renderer.instanceCount;
                }
            }
        }
        return total;
    }

    // パフォーマンス情報
    [ContextMenu("Show Performance Info")]
    public void ShowPerformanceInfo()
    {
        int totalRectangles = GetTotalRectangleCount();
        int drawCalls = objects != null ? objects.Length : 0;

        Debug.Log(
            $"Performance Info:"
                + $"\n- Objects: {objectCount}"
                + $"\n- Total Rectangles: {totalRectangles}"
                + $"\n- Draw Calls: {drawCalls}"
                + $"\n- Rectangles per Draw Call: {(drawCalls > 0 ? totalRectangles / drawCalls : 0)}"
        );
    }
}

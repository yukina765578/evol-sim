using UnityEngine;

public class TestRenderMeshInstancedManager : MonoBehaviour
{
    [Header("Instanced Object Settings")]
    public int objectCount = 500;
    public Material circleMaterial;
    public int instanceCount = 100;
    public float circleSize = 0.5f;
    public float moveSpeed = 2f;
    public float spawnRadius = 10f;

    private GameObject[] objects;

    void Start()
    {
        objects = new GameObject[objectCount];

        for (int i = 0; i < objectCount; i++)
        {
            GameObject obj = new GameObject("InstancedObj_" + i);
            obj.transform.parent = this.transform;
            var instanced = obj.AddComponent<RenderMeshInstancedTest>();

            // 必要な設定をここで渡す
            instanced.circleMaterial = circleMaterial;
            instanced.instanceCount = instanceCount;
            instanced.circleSize = circleSize;
            instanced.moveSpeed = moveSpeed;
            instanced.spawnRadius = spawnRadius + i * 0.1f; // 例えばわずかにズラす例

            objects[i] = obj;
        }
    }
}

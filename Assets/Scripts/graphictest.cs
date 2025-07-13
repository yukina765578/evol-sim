using UnityEngine;

public class GPUCheck : MonoBehaviour
{
    void Start()
    {
        Debug.Log("GPU: " + SystemInfo.graphicsDeviceName);
    }
}
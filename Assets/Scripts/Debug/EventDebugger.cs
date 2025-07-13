using UnityEngine;

public static class EventDebugger
{
    public static int CreatureDeathListeners = 0;
    public static int ReproductionListeners = 0;
    public static int CreatureDetectionListeners = 0;
    public static int NoiseUpdateListeners = 0;

    public static void LogCounts()
    {
        Debug.Log($"CreatureDeathListeners: {CreatureDeathListeners}");
        Debug.Log($"ReproductionListeners: {ReproductionListeners}");
        Debug.Log($"CreatureDetectionListeners: {CreatureDetectionListeners}");
        Debug.Log($"NoiseUpdateListeners: {NoiseUpdateListeners}");
    }
}

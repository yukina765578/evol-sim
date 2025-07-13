using System.Collections.Generic;
using UnityEngine;

public static class EventDebugger
{
    public static int CreatureDeathListeners = 0;
    public static int ReproductionListeners = 0;
    public static int CreatureDetectionListeners = 0;
    public static int NoiseUpdateListeners = 0;

    private static List<int> deathListenerHistory = new List<int>();
    private static List<int> reproductionListenerHistory = new List<int>();
    private static List<int> detectionListenerHistory = new List<int>();
    private static List<int> noiseListenerHistory = new List<int>();

    private static float lastLogTime = 0f;
    private static int maxHistorySize = 100;

    public static void LogCounts()
    {
        float currentTime = Time.time;
        if (currentTime - lastLogTime < 5f)
            return;

        lastLogTime = currentTime;

        // Track history
        RecordHistory();

        // Calculate trends
        string trendInfo = GetTrendInfo();

        Debug.Log($"=== EVENT LISTENER MONITORING (t={currentTime:F1}s) ===");
        Debug.Log(
            $"CreatureDeathListeners: {CreatureDeathListeners} {GetTrend(deathListenerHistory)}"
        );
        Debug.Log(
            $"ReproductionListeners: {ReproductionListeners} {GetTrend(reproductionListenerHistory)}"
        );
        Debug.Log(
            $"CreatureDetectionListeners: {CreatureDetectionListeners} {GetTrend(detectionListenerHistory)}"
        );
        Debug.Log($"NoiseUpdateListeners: {NoiseUpdateListeners} {GetTrend(noiseListenerHistory)}");

        CheckForLeaks();
    }

    static void RecordHistory()
    {
        AddToHistory(deathListenerHistory, CreatureDeathListeners);
        AddToHistory(reproductionListenerHistory, ReproductionListeners);
        AddToHistory(detectionListenerHistory, CreatureDetectionListeners);
        AddToHistory(noiseListenerHistory, NoiseUpdateListeners);
    }

    static void AddToHistory(List<int> history, int value)
    {
        history.Add(value);
        if (history.Count > maxHistorySize)
            history.RemoveAt(0);
    }

    static string GetTrend(List<int> history)
    {
        if (history.Count < 2)
            return "";

        int last = history[history.Count - 1];
        int previous = history[history.Count - 2];
        int change = last - previous;

        if (change > 0)
            return $"(+{change})";
        else if (change < 0)
            return $"({change})";
        else
            return "(=)";
    }

    static string GetTrendInfo()
    {
        // Calculate overall trend over last 10 samples
        int sampleSize = Mathf.Min(10, deathListenerHistory.Count);
        if (sampleSize < 2)
            return "No trend data";

        int totalTrend = 0;
        for (
            int i = deathListenerHistory.Count - sampleSize;
            i < deathListenerHistory.Count - 1;
            i++
        )
        {
            totalTrend += deathListenerHistory[i + 1] - deathListenerHistory[i];
        }

        return totalTrend > 0 ? "INCREASING"
            : totalTrend < 0 ? "DECREASING"
            : "STABLE";
    }

    static void CheckForLeaks()
    {
        if (deathListenerHistory.Count >= 10)
        {
            int recent = GetAverageOfLast(deathListenerHistory, 3);
            int older = GetAverageOfLast(deathListenerHistory, 10, 3);

            if (recent > older + 5)
            {
                Debug.LogWarning(
                    $"⚠️ POTENTIAL DEATH LISTENER LEAK: Recent avg {recent} vs older avg {older}"
                );
            }
        }

        if (reproductionListenerHistory.Count >= 10)
        {
            int recent = GetAverageOfLast(reproductionListenerHistory, 3);
            int older = GetAverageOfLast(reproductionListenerHistory, 10, 3);

            if (recent > older + 5)
            {
                Debug.LogWarning(
                    $"⚠️ POTENTIAL REPRODUCTION LISTENER LEAK: Recent avg {recent} vs older avg {older}"
                );
            }
        }

        if (CreatureDeathListeners > 500)
            Debug.LogError($"🚨 CRITICAL: Death listeners = {CreatureDeathListeners} (too high!)");
        if (ReproductionListeners > 1000)
            Debug.LogError(
                $"🚨 CRITICAL: Reproduction listeners = {ReproductionListeners} (too high!)"
            );
        if (CreatureDetectionListeners > 1000)
            Debug.LogError(
                $"🚨 CRITICAL: Detection listeners = {CreatureDetectionListeners} (too high!)"
            );
    }

    static int GetAverageOfLast(List<int> history, int count, int offset = 0)
    {
        int start = history.Count - count - offset;
        int end = history.Count - offset;

        if (start < 0)
            start = 0;
        if (end > history.Count)
            end = history.Count;

        int sum = 0;
        int samples = 0;
        for (int i = start; i < end; i++)
        {
            sum += history[i];
            samples++;
        }

        return samples > 0 ? sum / samples : 0;
    }

    public static void ValidateListenerCounts()
    {
        // This can be called after major operations to ensure counts are correct
        LogCounts();
    }

    public static void ResetCounts()
    {
        // Emergency reset if counts get corrupted
        CreatureDeathListeners = 0;
        ReproductionListeners = 0;
        CreatureDetectionListeners = 0;
        NoiseUpdateListeners = 0;

        deathListenerHistory.Clear();
        reproductionListenerHistory.Clear();
        detectionListenerHistory.Clear();
        noiseListenerHistory.Clear();

        Debug.Log("🔄 EventDebugger counts reset");
    }
}

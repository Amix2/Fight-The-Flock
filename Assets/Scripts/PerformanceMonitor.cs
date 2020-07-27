#define PERFORMANCE_MONITOR
using Unity.Jobs;

public static class PerformanceMonitor
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DEBUG_BeginSample(JobHandle jobHandle, string name)
    {
#if UNITY_EDITOR
        jobHandle.Complete();
        UnityEngine.Profiling.Profiler.BeginSample(name + "  Monitor");
#endif
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DEBUG_EndSample(JobHandle jobHandle)
    {
#if UNITY_EDITOR
        jobHandle.Complete();
        UnityEngine.Profiling.Profiler.EndSample();
#endif
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DEBUG_BeginSample(string name)
    {
#if UNITY_EDITOR
        UnityEngine.Profiling.Profiler.BeginSample(name + "  Monitor");
#endif
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void DEBUG_EndSample()
    {
#if UNITY_EDITOR
        UnityEngine.Profiling.Profiler.EndSample();
#endif
    }
}
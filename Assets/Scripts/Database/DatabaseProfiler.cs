using System;
using System.Collections;
using System.Diagnostics;

public static class DatabaseProfiler
{
    public static T Measure<T>(string operationName, Func<T> action)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        T result = action();
        sw.Stop();

        Log(operationName, sw.ElapsedMilliseconds);
        return result;
    }

    public static void Measure(string operationName, Action action)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        action();
        sw.Stop();

        Log(operationName, sw.ElapsedMilliseconds);
    }

    private static void Log(string operation, long ms)
    {
        string logLine = $"{DateTime.Now:HH:mm:ss}, {operation}, {ms}ms";
        string path = System.IO.Path.Combine(UnityEngine.Application.persistentDataPath, "db_performance.csv");
        System.IO.File.AppendAllText(path, logLine + "\n");
        UnityEngine.Debug.Log($"{operation} took {ms}ms");
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.IO;
using System.Linq;

// Implement this interface Tester in any testing script we write.
public interface Tester
{
    System.Action OnTestsCompleted { get; set; }
    void StartTests();
}

// This runner script will take in an array of objects implementing the Tester interface
// and will run them all in sequence.

// After these are run, it will start the python script.

// Right now the python script is empty, but we can think about how we want to process or transform the
// data we collect.

// This class is referenced, but originally started by the DataFlowManager script, which
// should be instanced in any scene we wish to test and get data from.

public class DataFlowRunner
{
    static string pythonScriptPath = Path.Combine(Application.dataPath, "DataPipeline", "TransformLogs.py");
    static string pythonPath = Path.Combine(Application.dataPath,
        "DataPipeline", "venv", Application.platform == RuntimePlatform.WindowsEditor ? "Scripts" : "bin",
        Application.platform == RuntimePlatform.WindowsEditor ? "python.exe" : "python");
    static string logOutputDir = Path.Combine(Application.persistentDataPath, "Logs");

    public static IEnumerator RunTestsAndScript(Tester[] testers)
    {
        foreach (Tester tester in testers)
        {
            UnityEngine.Debug.Log($"Running {tester.GetType().Name}.");
            bool finished = false;

            tester.OnTestsCompleted = () => finished = true;
            tester.StartTests();

            while (!finished)
            {
                yield return null;
            }

            UnityEngine.Debug.Log($"Tester {tester.GetType().Name} completed tests.");
        }

        UnityEngine.Debug.Log("All testers finished! Running Python script.");
        RunPythonScript();
    }

    private static void RunPythonScript()
    {
        if (!File.Exists(pythonScriptPath))
        {
            UnityEngine.Debug.LogError($"No Python script exists at {pythonScriptPath}!");
            return;
        }

        ProcessStartInfo info = new ProcessStartInfo()
        {
            FileName = pythonPath,
            Arguments = $"-u \"{pythonScriptPath}\" \"{logOutputDir}\"",
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        Process process = new Process();
        process.StartInfo = info;
        process.OutputDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.Log(args.Data);
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            if (!string.IsNullOrEmpty(args.Data))
                UnityEngine.Debug.Log(args.Data);
        };

        process.Start();
        process.BeginErrorReadLine();
        process.BeginOutputReadLine();
        process.WaitForExit();

        UnityEngine.Debug.Log("Python script finished!");
    }
}

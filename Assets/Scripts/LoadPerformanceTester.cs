using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using System;
using UnityEngine;
using System.IO;
using UnityEngine.SceneManagement;

public class LoadPerformanceTester : MonoBehaviour
{
    private float startTime;
    private string sceneToBeLoaded;

    string practiceSceneName = "Practice";
    string videoSceneName = "Video";
    string loginSceneName = "Login";
    string mainMenuSceneName = "MainMenu";
    string tuneSceneName = "Tune";
    string loadTimeOutputFile;
    string logOutputDir;
    

    void Awake()
    {
        logOutputDir = Path.Combine(Application.persistentDataPath, "Logs");
        loadTimeOutputFile = Path.Combine(logOutputDir, "SceneLoadPerformance.csv");

        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;

        InitCSV(loadTimeOutputFile);
    }

    public void StartSceneLoadTimer(string sceneName)
    {
        sceneToBeLoaded = sceneName;
        startTime = Time.realtimeSinceStartup;
        Debug.Log("Started loading scene: " + sceneName);
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode sceneMode)
    {
        if (scene.name == sceneToBeLoaded)
        {
            float duration = Time.realtimeSinceStartup - startTime;
            LogResult(loadTimeOutputFile, scene.name, duration);
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void LogResult(string filePath, string sceneLoaded, float elapsedMs = 0)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string line = $"{timestamp},{sceneLoaded},{elapsedMs}";
        File.AppendAllText(filePath, line + "\n");
        UnityEngine.Debug.Log($"Scene '{sceneLoaded}' took {elapsedMs}ms to load");
    }

    private void InitCSV(string path)
    {
        if (!File.Exists(path))
            File.WriteAllText(path, "Timestamp,SceneLoaded,ResponseTime(ms)\n");
    }
}

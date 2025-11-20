using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PerformanceLogger_TimeInEachScene : MonoBehaviour
{
    string logPath;

    int currentSceneIndex;
    // Don't change this order, knowing the specific structure lets the 396 parser be more efficient
    // -Alex
    List<(string scene, float time)> sceneTimes = new List<(string, float)>(){
        ("MainMenu", 0f),
        ("Login", 0f),
        ("Practice", 0f),
        ("Tune", 0f)
    };

    void Start(){
        // Write the log when play mode or the application is being exited
        // Avoids the need for separate files or parsing through the log during runtime
        // -Alex
        Application.quitting += WriteLog;

        logPath += Application.dataPath;
        logPath += "/Logs/TimeInEachSceneLog.txt";

        if(!File.Exists(logPath)){
            File.Create(logPath);
        }

        File.WriteAllText(logPath, string.Empty);
    }

    public void UpdateCurrentScene(string newScene){
        // Ensure scene is known and defined in sceneTimes
        int index = 0;
        foreach(var sceneTime in sceneTimes){
            if(sceneTime.scene.Equals(newScene)){
                currentSceneIndex = index;
                return;
            }
            index++;
        }

        // Unrecognized scene
        Debug.LogError("Unrecognized scene, disabling time in each scene logger");
        enabled = false;
    }

    void Update(){
        UpdateData();
    }

    void UpdateData(){
        string scene = sceneTimes[currentSceneIndex].scene;
        float time = sceneTimes[currentSceneIndex].time;
        sceneTimes[currentSceneIndex] = (scene, time + Time.deltaTime);
    }

    void WriteLog(){
        foreach(var sceneTime in sceneTimes){
            File.AppendAllText(logPath, sceneTime.scene + ", " + sceneTime.time + "\n");
        }
    }
}

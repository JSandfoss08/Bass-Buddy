using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PerformanceLogger : MonoBehaviour
{
    public static PerformanceLogger Instance {get; private set;}

    // Log Information
    // string filePath = @"C:\Users\alexj\Guitar Tutor\Guitar Tutor\Assets\Logs\Log1.txt";
    string framerateLogPath = @"C:\Users\alexj\Guitar Tutor\Guitar Tutor\Assets\Logs\FramerateLog.txt";
    // string utilizationLogPath = @"C:\Users\alexj\Guitar Tutor\Guitar Tutor\Assets\Logs\UtilizationLog.txt";
    string sceneLogPath = @"C:\Users\alexj\Guitar Tutor\Guitar Tutor\Assets\Logs\SceneLog.txt";
    int frame = 0;
    float elapsedTime;

    // Framerate Information
    public int fps;
    public float fpsLogTimer = 0f;
    public int frames = 0;

    // State Information
    public string scene;
    public int actionCnt = 0;
    public string[] actions = new string[10];

    // Action Information
    List<string> actionsToLog = new List<string>();

    void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    /*
    void Start(){
        File.WriteAllText(filePath, string.Empty);
        File.WriteAllText(framerateLogPath, string.Empty);
        File.WriteAllText(sceneLogPath, string.Empty);
        File.WriteAllText(utilizationLogPath, string.Empty);
    }

    void Update(){
        CalculateFramerate();
    }
    */

    void CalculateFramerate(){
        if(fpsLogTimer >= 0.1f){
            fps = frames * 10;
            Debug.Log("FPS == " + fps);
            fpsLogTimer -= 0.1f;
            frames = 0;
            MakeLogs();
        }else{
            frames++;
        }

        fpsLogTimer += Time.deltaTime;
        elapsedTime += Time.deltaTime;
        frame++;
    }

    public void UpdateScene(string newScene){
        scene = newScene;
    }

    void MakeLogs(){
        LogFramerate();
        LogScene();
    }

    void LogFramerate(){
        File.AppendAllText(framerateLogPath, elapsedTime + ", " + fps + "\n");
    }

    void LogScene(){
        File.AppendAllText(sceneLogPath, scene + "\n");
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PerformanceLogger_FPS : MonoBehaviour
{
    string logPath;

    int frameCount = 0;
    float secondTimer = 0f;

    void Start(){
        logPath += Application.dataPath;
        logPath += "/Logs/FPSLog.txt";
        File.WriteAllText(logPath, string.Empty);
    }

    void Update(){
        secondTimer += Time.deltaTime;
        frameCount++;

        if(secondTimer >= 1f){
            float estimatedFramerate = frameCount / (secondTimer / 1);
            WriteLog(estimatedFramerate.ToString());
            secondTimer -= 1f;
            frameCount = 0;
        }
    }

    void WriteLog(string log){
        File.AppendAllText(logPath, log + "\n");
    }
}

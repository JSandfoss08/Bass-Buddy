using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

// This is the script that should be added to a gameobject in the scene we
// want to test components in.

// It's probably easiest to simply make a "tester" gameobject with each tested script
// added to the same object as this manager script
public class DataFlowManager : MonoBehaviour
{
    private Coroutine currentPipeline;

    [ContextMenu("Start Data Pipeline")]
    public void StartPipeline()
    {
        if (currentPipeline != null)
        {
            Debug.LogWarning("Pipeline already running!");
            return;
        }

        Tester[] testers = FindObjectsOfType<MonoBehaviour>().OfType<Tester>().ToArray();
        if (testers.Length == 0)
        {
            Debug.LogError("No testers found in scene!");
            return;
        }

        Debug.Log("Starting tests...");
        currentPipeline = StartCoroutine(DataFlowRunner.RunTestsAndScript(testers));
    }

    [ContextMenu("Stop Data Pipeline")]
    public void StopPipeline()
    {
        if (currentPipeline != null)
        {
            StopCoroutine(currentPipeline);
            currentPipeline = null;
            Debug.Log("Stopped pipeline!");
        }
    }
}

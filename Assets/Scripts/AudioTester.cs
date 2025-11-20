using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using System.IO;

public class AudioTester : MonoBehaviour, Tester
{
    public Action OnTestsCompleted { get; set; }

    [Header("UI References")]
    public TMP_Text noteNameText;
    public TMP_Text notePitchText;

    [Header("Test Parameters")]
    public int iterationCount = 10;
    public float detectionInterval = 2f;
   
    private int currentIteration = 0;
    private string logsDir;
    private string csvLogPath;

    void Awake()
    {
        logsDir = Path.Combine(Application.persistentDataPath, "Logs");
        csvLogPath = Path.Combine(logsDir, "Pitch_to_Note.csv");
        Directory.CreateDirectory(logsDir);
    }

    [ContextMenu("Run Audio Tests")]
    public void StartTests()
    {
        if (!File.Exists(csvLogPath))
            File.WriteAllText(csvLogPath, "Timestamp,NoteName,DetectedPitch\n");
        StartCoroutine(Test());
    }

    private IEnumerator Test()
    {
        Debug.Log("Starting Audio Pitch Detectinon test.");

        Audio_Input_Manager.Instance.ActivateMicrophoneInput();
        currentIteration = 0;

        while (currentIteration < iterationCount)
        {
            yield return new WaitForSeconds(detectionInterval);

            float measuredPitch = Audio_Input_Manager.Instance.pitchEstimator.Estimate(Audio_Input_Manager.Instance.microphoneAudioSource);
            Note detectedNote = Audio_Input_Manager.GetNoteFromFrequency(measuredPitch);

            string detectedNoteName = detectedNote.GetName();
            float detectedPitch = measuredPitch;

            noteNameText.text = detectedNoteName;
            notePitchText.text = detectedPitch.ToString();

            string message = $"{detectedNoteName},{detectedPitch}";
            Log(message);
            Debug.Log($"[{currentIteration + 1}/{iterationCount}] {detectedNoteName} ({measuredPitch:F2} Hz)");
            currentIteration++;
        }
        Debug.Log("Audio Pitch Detection finished testing!");
        Audio_Input_Manager.Instance.DeactivateMicrophoneInput();
        OnTestsCompleted?.Invoke();
    }

    void Log(string message)
    {
        string timestamp = $"{System.DateTime.Now:HH:mm:ss}";
        File.AppendAllText(csvLogPath, $"{timestamp},{message}\n");
    }
}
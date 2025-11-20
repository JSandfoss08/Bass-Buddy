using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Audio_Input_Manager : MonoBehaviour
{
    private static readonly string[] noteNames = 
        { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };

    public static Audio_Input_Manager Instance;

    private AudioClip microphoneInputClip;
    private string micName;
    public bool microphoneInitialized = false;
    public bool microphoneActive = false;

    public AudioPitchEstimator pitchEstimator;
    public AudioSource microphoneAudioSource;
    public Note detectedNote = Note.None;
    private bool isTuneMode = false;

    public int currentNoteIndex = 0;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        pitchEstimator = GetComponent<AudioPitchEstimator>();
        microphoneAudioSource = GetComponent<AudioSource>();

        StartCoroutine(InitializeMicrophone());
    }

    void Update()
    {
        // Reconnect if Unity lost mic link or scene reloaded
        if (microphoneInitialized && !microphoneAudioSource.isPlaying && microphoneActive)
        {
            Debug.LogWarning("Mic disconnected or stopped — restarting...");
            StartCoroutine(ReinitializeMicrophone());
            return;
        }

        if (microphoneInitialized && microphoneActive)
            UpdateDetectedNote();
    }

    private void UpdateDetectedNote()
    {
        float hertz = ListenForPitch();

        // Early exit if invalid pitch (below threshold)
        if (float.IsNaN(hertz) || hertz <= 0f)
        {
            // If not in tune mode, update detected note as often as possible even if no note played
            if (!isTuneMode)
                detectedNote = Note.None;
            // if in tune mode, only update when a valid note is detected
            else
                return;
        }
        else
            detectedNote = GetNoteFromFrequency(hertz);
        Debug.Log($"Detected Note: {detectedNote.GetName()} ({hertz:F1} Hz)");
    }

    private IEnumerator InitializeMicrophone()
    {
        if (Microphone.devices.Length == 0)
        {
            Debug.LogWarning("No microphone devices found.");
            yield break;
        }

        micName = Microphone.devices[0];
        Debug.Log($"Using microphone: {micName}");

        // Start recording
        microphoneInputClip = Microphone.Start(micName, true, 1, 44100);
        microphoneAudioSource.loop = true;

        // Wait until the microphone starts producing samples
        while (Microphone.GetPosition(micName) <= 0)
        {
            yield return null;
        }

        microphoneAudioSource.clip = microphoneInputClip;
        microphoneInitialized = true;

        Debug.Log("Microphone initialized and playing.");
    }

    private IEnumerator ReinitializeMicrophone()
    {
        microphoneInitialized = false;
        microphoneActive = false;

        if (Microphone.IsRecording(micName))
            Microphone.End(micName);

        yield return new WaitForSeconds(0.3f);
        yield return InitializeMicrophone();
    }

    public Note GetNote()
    {
        return detectedNote;
    }

    private void SetCurrentNoteIndex(int index)
    {
        currentNoteIndex = index;
    }

    public static int GetCurrentNoteIndex()
    {
        return Instance.currentNoteIndex;
    }

    public static Note GetNoteFromFrequency(float frequency)
    {
        float semitoneOffset = 12f * Mathf.Log(frequency / 440f, 2f);
        int nearestNote = Mathf.RoundToInt(semitoneOffset);
        int noteIndex = (nearestNote + 9) % 12;

        if (noteIndex < 0)
            noteIndex += 12;

        Instance.SetCurrentNoteIndex(noteIndex);

        return new Note(noteNames[noteIndex], 0, 0);
    }

    public void SetTuneMode(bool isTuneMode)
    {
        this.isTuneMode = isTuneMode;
    }

    public float GetVolume()
    {
        if (microphoneInputClip == null)
            return 0f;

        float[] data = new float[256];
        int micPosition = Microphone.GetPosition(null) - data.Length + 1;
        if (micPosition < 0) return 0f;

        microphoneInputClip.GetData(data, micPosition);
        float sum = 0f;
        for (int i = 0; i < data.Length; i++)
            sum += data[i] * data[i];

        // RMS volume
        return Mathf.Sqrt(sum / data.Length);
    }

    public void ToggleMicrophone()
    {
        if (microphoneActive)
            DeactivateMicrophoneInput();
        else
            ActivateMicrophoneInput();
    }

    public float ListenForPitch()
    {
        return pitchEstimator.Estimate(microphoneAudioSource);
    }

    public void ActivateMicrophoneInput()
    {
        if (!microphoneInitialized)
        {
            StartCoroutine(InitializeMicrophone());
            return;
        }

        if (!microphoneAudioSource.isPlaying)
            microphoneAudioSource.Play();
            Debug.Log("Microphone audio source started playing.");

        microphoneActive = true;
    }

    public void DeactivateMicrophoneInput()
    {
        Debug.Log("Micronphone stopped");
        microphoneAudioSource.Stop();
        microphoneActive = false;
    }
}

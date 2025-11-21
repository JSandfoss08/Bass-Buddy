using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using static LessonContent;

public class LessonGameplay : MonoBehaviour
{
    public UI_Practice ui;
    public Note targetNote;

    [Tooltip("If the practice scene is entered from the editor rather than the main menu, this lesson will be played")]
    public Lessons lessonToTest;

    private bool isPaused = false;

    private int lessonID;
    private int userID;
    private int stringIndex;

    public float gracePeriodEarly = 0.4f;
    public float gracePeriodLate = 0.4f;
    public int totalNotes;
    public int testedNoteCount;
    public int correctNotes = 0;
    public float minPassingPercentage;
    private bool passed = false;

    LessonContent lesson;
    LessonContent lessonContent;
    public bool success = false;
    public bool isChecking = false;

    bool runTimer = false;
    public float timeBetweenPlays;
    public float time = 0f;
    int lessonIndex = 0;
    public float noteDetectionVolumeThreshold = 0.05f;

    void Start()
    {
        ui.ClearGuitar();
        runTimer = false;

        Pause();
        ui.Pause();
        ui.Unpause();

        // Get user ID and lesson ID from PlayerPrefs
        userID = PlayerPrefs.GetInt("CurrentUserID", -1);
        lessonID = PlayerPrefs.GetInt("CurrentLessonID", -1);
        if (userID == -1 || lessonID == -1)
        {
            Debug.LogError("No user or lesson ID found!");
            return;
        }

        // Get lesson content from LessonManager
        lessonContent = LessonManager.Instance.StartLesson(userID, lessonID);

        // Flip guitar UI if user is congigured as left handed
        if (DatabaseHandler.Instance.UserIsLeftHanded(userID))
            ui.FlipDominantHand();

        if (lessonContent != null)
        {
            SetLesson();
        }
        else
        {
            Debug.LogError($"Could not load lesson content for lesson ID {lessonID}");
        }

        Audio_Input_Manager.Instance.SetTuneMode(false);

        StartCoroutine(StartMic());
        StartProgressBar();
    }

    IEnumerator StartMic()
    {
        // Wait until Audio_Input_Manager exists and is fully initialized
        yield return new WaitUntil(() => 
            Audio_Input_Manager.Instance != null && 
            Audio_Input_Manager.Instance.microphoneInitialized);

        Audio_Input_Manager.Instance.ActivateMicrophoneInput();
    }

    void Update()
    {
        if (runTimer)
        {
            RunTimer();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (!isPaused)
        {
            isPaused = true;
            ui.Pause();
        }
        else
        {
            isPaused = false;
            ui.Unpause();
        }
    }

    void RunTimer(){
        time -= Time.deltaTime;

        // Only start checking once per note when grace period starts
        if (time <= gracePeriodEarly && time > -gracePeriodLate && !isChecking)
        {
            isChecking = true;
            StartCoroutine(CheckNote());
        }
    }

    void ResetTimer()
    {
        time = timeBetweenPlays;
    }

    public void Pause()
    {
        Time.timeScale = 0;
        runTimer = false;
    }

    IEnumerator CheckNote()
    {
        success = false;
        // Small delay before checking, lets the mic get the note in time
        yield return new WaitForSeconds(0.2f);

        Note detectedNote = Note.None;
        int stableFrames = 0;
        float checkingElapsedTime = 0;

        while (checkingElapsedTime <= gracePeriodEarly + gracePeriodLate)
        {
            Debug.Log("Checking Note");
            checkingElapsedTime += Time.deltaTime;
            detectedNote = Audio_Input_Manager.Instance.GetNote();

            float volume = Audio_Input_Manager.Instance.GetVolume();
            // Debug.Log($"Volume: {volume}");
            if (volume < noteDetectionVolumeThreshold)
            {
                Debug.LogWarning("Skipped note detection");
                stableFrames = 0;
                yield return null;
                continue;
            }

            if (detectedNote != Note.None && detectedNote.GetName() == targetNote.GetName())
            {
                stableFrames++;
                if (stableFrames >= 5)
                {
                    ui.DisplaySuccess();
                    Debug.Log("Display success called");
                    correctNotes++;
                    success = true;
                    break;
                }
            }
            else
                stableFrames = 0;
            yield return null;
        }

        if (!success)
        {
            // If a note is played but timing is off, mark as timing error
            bool playedTooLate = detectedNote == Note.None;
            Debug.Log(detectedNote.GetName());
            ui.DisplayFailure(playedTooLate);
            Debug.Log($"Display failure called. Too late = {playedTooLate}");
        }
        testedNoteCount++;
        ui.UpdateProgress(correctNotes, testedNoteCount, totalNotes);
        isChecking = false;

        LoadNextNote();
        ResetTimer();
        yield return null;
        Debug.Log("isChecking reset and next note loaded");
    }

    private void StartProgressBar()
    {
        float duration = totalNotes * timeBetweenPlays;
        ui.StartProgressBar(duration);
    }

    [ContextMenu("Load Next Note")]
    void LoadNextNote()
    {
        if (lessonContent == null)
        {
            Debug.LogError("No lesson content loaded!");
            return;
        }

        targetNote = lessonContent.GetNote(lessonIndex);
        lessonIndex++;
        
        if(targetNote == Note.None)
        {
            runTimer = false;
            OnLessonEnd();
        }
        else
        {
            ui.LoadNote(targetNote, timeBetweenPlays - 0.2f);
        }
    }

    public void SetLesson() 
    {
        lesson = LoadSceneManager.Instance.GetLessonContents();

        if(lesson == null) {
            enabled = false;
            return;
        }

        timeBetweenPlays = lesson.GetTimeBetween();
        totalNotes = lesson.GetNumOfNotes();
        minPassingPercentage = lessonContent.GetMinPassingPercentage();
        ui.UpdateProgress(0, testedNoteCount, totalNotes);
        LoadNextNote();
        ResetTimer();
        runTimer = true;
    }

    public void Unpause()
    {
        Time.timeScale = 1;
        runTimer = true;
    }

    public void UpdateDB()
    {
        if (totalNotes == 0) return;

        float percentage = (correctNotes / (float)totalNotes) * 100f;
        UserLessonProgress currentProgress = DatabaseHandler.Instance.GetLessonProgress(userID, lessonID);

        if (percentage > currentProgress.ProgressPercentage)
        {
            DatabaseHandler.Instance.UpdateLessonProgress(userID, lessonID, minPassingPercentage, percentage);
            Debug.Log($"Progress updated: {correctNotes}/{totalNotes} = {percentage}%");
        }
        else
        {
            Debug.Log($"Progress not updated (existing {currentProgress.ProgressPercentage}% >= {percentage}%)");
        }
    }

    void OnLessonEnd()
    {
        Debug.Log("Lesson completed!");
        // Check user pass condition
        if (correctNotes / (float)totalNotes * 100 >= minPassingPercentage)
            passed = true;

        // Final update to database
        UpdateDB();

        // Show completion UI with score
        ui.ShowLessonEnd(passed, correctNotes, totalNotes);
    }
}

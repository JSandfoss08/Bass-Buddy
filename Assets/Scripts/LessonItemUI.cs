using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System;

// Class for configuring lesson select tabs with content such
// as lesson name, score, completion status, and lessonIDs
public class LessonItemUI : MonoBehaviour
{   
    [Header("UI References")]
    public TMP_Text lessonNameText;
    public TMP_Text difficultyText;
    public TMP_Text statusText;
    public TMP_Text scoreText;
    public Slider progressBar;
    public Button startButton;
    public Image lockedIcon;
    public Image lessonTypePreview;
    public Sprite videoLessonIcon;
    public Sprite practiceLessonIcon;

    [Header("Description Dropdown")]
    public Button descriptionDropdownButton;
    public GameObject descriptionDropdownButtonText;
    public GameObject descriptionDropdownObject;
    public Button dropdownClickBlocker; 
    public TMP_Text descriptionText;
    private bool dropdownIsOpen = false;
    public Animator dropdownAnimator;

    [Header("Status Colors")]
    public Color unopenedColor = Color.gray;
    public Color inProgressColor = Color.yellow;
    public Color completeColor = Color.green;

    private bool isLessonUnlocked = false;
    private bool isVideoLesson = false;
    private LessonInfo lesson;
    public int lessonID = -1;
    private UserLessonProgress progress;
    private int userId;
    public TMP_Text feedbackText;

    // Setup UI
    public void Setup(LessonInfo lessonData, UserLessonProgress progressData, int currentUserId)
    {
        lesson = lessonData;
        lessonID = lessonData.LessonId;
        progress = progressData;
        userId = currentUserId;

        isLessonUnlocked = LessonManager.Instance.IsLessonUnlocked(userId, lesson.LessonId);
        isVideoLesson = DatabaseHandler.Instance.GetLesson(lesson.LessonId).VideoName != null;
        lockedIcon.gameObject.SetActive(!isLessonUnlocked);
        dropdownAnimator = GetComponent<Animator>();

        // Set icons for lesson types
        if (isVideoLesson)
            lessonTypePreview.sprite = videoLessonIcon;
        else
            lessonTypePreview.sprite = practiceLessonIcon;

        // Set lesson info
        lessonNameText.text = lesson.LessonName;
        feedbackText.text = "";

        if (descriptionText != null)
            descriptionText.text = lesson.Description;
        if (difficultyText != null)
            difficultyText.text = lesson.Difficulty;

        // Set progress info
        if (progress != null)
        {
            statusText.text = FormatStatus(progress.Status);
            scoreText.text = $"Score: {Math.Round(progressData.ProgressPercentage).ToString()}%";

            if (progressBar != null)
                progressBar.value = progress.ProgressPercentage / 100f;

            // Set status color
            switch (progress.Status)
            {
                case "unopened":
                    if (statusText != null) statusText.color = unopenedColor;
                    startButton.GetComponentInChildren<TMP_Text>().text = "Start";
                    break;
                case "in_progress":
                    if (statusText != null) statusText.color = inProgressColor;
                    startButton.GetComponentInChildren<TMP_Text>().text = "Continue";
                    break;
                case "complete":
                    if (statusText != null) statusText.color = completeColor;
                    startButton.GetComponentInChildren<TMP_Text>().text = "Replay";
                    break;
            }
        }
        if (progress.Status == "unopened")
            scoreText.gameObject.SetActive(false);

        startButton.onClick.AddListener(OnStartLesson);
        descriptionDropdownButton.onClick.AddListener(ToggleDropdown);

        Canvas rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
        dropdownClickBlocker.gameObject.transform.SetParent(rootCanvas.transform, false);
        dropdownClickBlocker.gameObject.transform.SetAsLastSibling();
        dropdownClickBlocker.gameObject.SetActive(false);
        dropdownClickBlocker.onClick.AddListener(HideDescription);
    }

    public void OnStartLesson()
    {
        if (!isLessonUnlocked)
        {
            StartCoroutine(ShowFeedback("Complete previous lesson before moving on.", Color.red));
            return;
        }
        // Store lesson ID for the lesson scene
        PlayerPrefs.SetInt("CurrentLessonID", lessonID);
        PlayerPrefs.Save();
        Debug.Log($"UserID: {PlayerPrefs.GetInt("CurrentUserID")}. LessonID: {PlayerPrefs.GetInt("CurrentLessonID")}");

        // Update status to in_progress if unopened
        if (progress.Status == "unopened")
        {
            DatabaseHandler.Instance.UpdateLessonStatus(userId, lesson.LessonId, "in_progress");
        }

        // Check if the lesson is a video lesson, and load the video scene if so
        if (isVideoLesson)
            LoadSceneManager.Instance.LoadVideoLesson();
        // Load practice lesson scene otherwise
        else
            LoadSceneManager.Instance.LoadPractice(lessonID);
    }

    private void ToggleDropdown()
    {
        if (dropdownIsOpen)
            HideDescription();
        else
            ShowDescription();
    }

    private void ShowDescription()
    {
        dropdownAnimator.SetBool("Show", true);
        dropdownIsOpen = true;
        dropdownClickBlocker.gameObject.SetActive(true);
        descriptionDropdownButtonText.transform.Rotate(new Vector3(0, 0, 180));
        dropdownClickBlocker.gameObject.SetActive(true);
    }
    
    private void HideDescription()
    {
        dropdownAnimator.SetBool("Show", false);
        dropdownIsOpen = false;
        dropdownClickBlocker.gameObject.SetActive(false);
        descriptionDropdownButtonText.transform.Rotate(new Vector3(0, 0, -180));
    }

    public IEnumerator ShowFeedback(string message, Color color)
    {
        feedbackText.color = color;
        feedbackText.text = message;
        yield return new WaitForSeconds(2);
        feedbackText.text = "";
    }

    public string FormatStatus(string status)
    {
        switch (status)
        {
            case "unopened": return "Not Started";
            case "in_progress": return "In Progress";
            case "complete": return "Completed";
            default: return status;
        }
    }
}
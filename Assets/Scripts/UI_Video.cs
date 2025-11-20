using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

public class UI_Video : MonoBehaviour
{
    public VideoGameplay videoManager;

    [Header("Completion UI")]
    public GameObject completionPanel;
    public TMP_Text completionTitleText;
    public Button completionPanelReturnButton;
    public Button replayButton;
    public Doggo doggo;

    [Header("Controls")]
    public Button inVideoReturnButton;
    public Button playbackButton;
    public TMP_Text playbackButtonText;

    [Header("Progress UI")]
    public Slider progressBar;

    private bool isPlaying;

    void Start()
    {
        if (completionPanel != null)
            completionPanel.SetActive(false);

        // Hook up button listeners
        if (completionPanelReturnButton != null)
            completionPanelReturnButton.onClick.AddListener(Return);
        if (inVideoReturnButton != null)
            inVideoReturnButton.onClick.AddListener(Return);

        if (replayButton != null)
            replayButton.onClick.AddListener(OnRetry);

        if (playbackButton != null)
            playbackButton.onClick.AddListener(TogglePlayback);

        playbackButtonText.text = "Play";
    }

    public void EnablePlayButton(bool enabled)
    {
        if (playbackButton != null)
            playbackButton.interactable = enabled;
    }

    public void ShowLessonEnd(bool passed)
    {
        if (completionPanel == null)
        {
            Debug.LogWarning("Completion panel not assigned!");
            return;
        }

        completionPanel.SetActive(true);
        doggo.MakeSuperHappy();

        if (completionTitleText != null)
            completionTitleText.text = passed ? "Lesson Complete!" : "Lesson Failed.";
    }

    void TogglePlayback()
    {
        if (!isPlaying)
            OnPlay();
        else
            OnPause();
    }

    void OnPlay()
    {
        isPlaying = true;
        videoManager.PlayVideo();
        playbackButtonText.text = "Pause";
    }

    void OnPause()
    {
        isPlaying = false;
        videoManager.PauseVideo();
        playbackButtonText.text = "Play";
    }

    void OnRetry()
    {
        completionPanel.SetActive(false);
        videoManager.ReplayVideo();
        isPlaying = true;
        playbackButtonText.text = "Pause";
    }

    public void Return()
    {
        Debug.Log("Loaded Main Menu");
        LoadSceneManager.Instance.LoadMainMenu();
    }
}

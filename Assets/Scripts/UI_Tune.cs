using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using JetBrains.Annotations;
using System;
using UnityEngine.Video;

public class UI_Tune : MonoBehaviour
{
    public TMP_Text[] TextMeshProsToUpdate;

    public bool tuneUp = false;
    public bool tuneDown = false;
    // Target note indices for standard guitar tuning EADG
    // Refer to Audio_Input_Manager.noteNames for mapping
    public int[] targetNoteIndex = { 4, 9, 2, 7 };
    public int targetNoteSemitones = 4;

    private bool isListening = false;
    public TMP_Text listeningText;
    public GameObject tuneIndicator;
    public Image tuneIndicatorImage;
    public Color correctTuneColor = Color.green;
    public Color incorrectTuneColor = Color.red;
    public Sprite correctTuneIndicatorImage;
    public Sprite incorrectTuneIndicatorImage;
    public TMP_Text tuneDirectionText;

    private float defaultIndicatorRotation = 0f;
    public int currentString = 0;
    public Image[] stringIndicators;
    public GameObject[] StringButtons;
    public Color selectedStringColor;
    public Color defaultStringColor;

    [Header("Tune Buttons")]
    public Button beginTuneButton;
    public Button backButton;
    public Button videoHelpButton;

    [Header("Video Help Popup")]
    public TMP_Text videoPlaybackButtonText;
    public VideoClip videoClip;
    public VideoPlayer videoPlayer;
    public GameObject videoHelpPopupObject;
    public Button videoTogglePlaybackButton;
    public Button videoHelpBackButton;
    
    void Awake()
    {
        tuneIndicatorImage = tuneIndicator.GetComponent<Image>();
        incorrectTuneIndicatorImage = tuneIndicatorImage.sprite;
        defaultIndicatorRotation = tuneIndicator.transform.rotation.eulerAngles.z;
    }
    // Start is called before the first frame update
    void Start()
    {
        SelectString(0);

        beginTuneButton.onClick.AddListener(ToggleListener);
        backButton.onClick.AddListener(Return);
        videoHelpButton.onClick.AddListener(ShowVideoPopup);
        videoHelpBackButton.onClick.AddListener(HideVideoPopup);
        videoTogglePlaybackButton.onClick.AddListener(ToggleVideoPlayback);

        videoHelpPopupObject.SetActive(false);
        Audio_Input_Manager.Instance.DeactivateMicrophoneInput();
        Audio_Input_Manager.Instance.SetTuneMode(true);
        videoPlaybackButtonText.text = "Play";

        tuneDirectionText.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        int currentNoteIndex = Audio_Input_Manager.GetCurrentNoteIndex();

        // Note is too low
        if (currentNoteIndex < targetNoteIndex[currentString])
        {
            DisplayTuneUp();
        }
        // Note is too high
        else if (currentNoteIndex > targetNoteIndex[currentString])
        {
            DisplayTuneDown();
        }
        // Note is juuuussst right!
        else
        {
            DisplayTuneCorrect();
        }

        for (int i = 0; i < TextMeshProsToUpdate.Length; i++)
        {
            Note currentNote = Audio_Input_Manager.Instance.detectedNote;

            if (currentNote != null)
            {
                if (TextMeshProsToUpdate[i] != null && currentNote.GetName() != "Invalid")
                    TextMeshProsToUpdate[i].text = Audio_Input_Manager.Instance.detectedNote.GetName();
                else if (currentNote.GetName() == "None")
                    TextMeshProsToUpdate[i].text = "Play a Note";
            }
        }
    }

    public void SelectString(int stringIndex)
    {
        if (stringIndex >= 0 && stringIndex < targetNoteIndex.Length)
        {
            for (int i = 0; i < stringIndicators.Length; i++)
            {
                stringIndicators[i].color = defaultStringColor;
            }
            currentString = stringIndex;
            stringIndicators[stringIndex].color = selectedStringColor;
            targetNoteSemitones = targetNoteIndex[currentString];
            MoveIndicator(stringIndex);
        }
    }

    public void Return(){
        LoadSceneManager.Instance.LoadMainMenu();
    }
    
    private void MoveIndicator(int stringIndex)
    {
        tuneIndicator.transform.SetParent(StringButtons[stringIndex].transform);
        tuneIndicator.transform.localPosition = new Vector3(-60, 0, 0);
    }

    private void UpdateIndicator(float angle)
    {
        Quaternion currentRotation = tuneIndicator.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, angle);
        tuneIndicator.transform.localRotation = Quaternion.Slerp(currentRotation, targetRotation, Time.deltaTime * 5f);
    }
    
    public void ToggleListener()
    {
        if (isListening)
        {
            listeningText.text = "Begin Tune";
            isListening = false;
        }

        else
        {
            listeningText.text = "Listening";
            isListening = true;
        }
        Debug.Log("Toggled");
        Audio_Input_Manager.Instance.ToggleMicrophone();
    }

    public void DisplayTuneUp()
    {
        UpdateIndicator(defaultIndicatorRotation);
        tuneUp = true;
        tuneDown = false;

        tuneIndicatorImage.sprite = incorrectTuneIndicatorImage;
        tuneIndicatorImage.color = incorrectTuneColor;

        tuneDirectionText.color = incorrectTuneColor;
        tuneDirectionText.text = "Slowly twist: counterclockwise";
    }

    public void DisplayTuneDown()
    {
        UpdateIndicator(defaultIndicatorRotation - 180f);
        tuneUp = false;
        tuneDown = true;

        tuneIndicatorImage.sprite = incorrectTuneIndicatorImage;
        tuneIndicatorImage.color = incorrectTuneColor;

        tuneDirectionText.color = incorrectTuneColor;
        tuneDirectionText.text = "Slowly twist: clockwise";
    }

    public void DisplayTuneCorrect()
    {
        UpdateIndicator(defaultIndicatorRotation);
        tuneUp = false;
        tuneDown = false;

        tuneIndicatorImage.sprite = correctTuneIndicatorImage;
        tuneIndicatorImage.color = correctTuneColor;

        tuneDirectionText.color = correctTuneColor;
        tuneDirectionText.text = "Good!";
    }
    
    // ----- Help popup functions -----
    public void ShowVideoPopup()
    {
        videoHelpPopupObject.SetActive(true);
    }

    public void HideVideoPopup()
    {
        videoHelpPopupObject.SetActive(false);
    }

    public void ToggleVideoPlayback()
    {
        if (videoPlayer.isPlaying)
            PauseVideo();
        else
            PlayVideo();
    }

    public void PauseVideo()
    {
        videoPlayer.Pause();
        videoPlaybackButtonText.text = "Play";
    }

    public void PauseVideo(VideoPlayer vp)
    {
        videoPlayer.loopPointReached -= PauseVideo;
        videoPlayer.Pause();
        videoPlaybackButtonText.text = "Replay";
    }

    public void PlayVideo()
    {
        videoPlayer.loopPointReached += PauseVideo;
        videoPlayer.Play();
        videoPlaybackButtonText.text = "Pause";
    }
}

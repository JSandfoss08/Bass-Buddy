using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System.IO;

public class VideoGameplay : MonoBehaviour
{
    public UI_Video ui;
    public VideoPlayer videoPlayer;
    public RawImage videoDisplay;
    public AudioSource audioSource;

    private int lessonID;
    private int userID;
    private LessonInfo lesson;

    public string videoName;

    public bool hasShownCompletion = false;

    void Awake()
    {
        if (videoPlayer == null)
            videoPlayer = GetComponent<VideoPlayer>();
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);
    }

    void Start()
    {
        userID = PlayerPrefs.GetInt("CurrentUserID", -1);
        lessonID = PlayerPrefs.GetInt("CurrentLessonID", -1);

        if (userID == -1 || lessonID == -1)
        {
            Debug.LogError("No user or lesson ID found!");
            return;
        }

        lesson = DatabaseHandler.Instance.GetLesson(lessonID);

        videoName = lesson.VideoName;

        if (string.IsNullOrEmpty(videoName))
        {
            Debug.LogError("No video name found for this lesson!");
            return;
        }

        string videoPath = Path.Combine(Application.streamingAssetsPath, "Videos", videoName);

        if (!File.Exists(videoPath))
        {
            Debug.LogError($"Video not found at: {videoPath}");
            return;
        }

        videoPlayer.url = videoPath;
        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;

        // Listen for when video ends
        videoPlayer.loopPointReached += OnVideoEnd;
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoDisplay.texture = vp.texture;
        Debug.Log("Video ready — waiting for user to press Play.");
        ui.EnablePlayButton(true);
    }

    private void OnVideoEnd(VideoPlayer vp)
    {
        if (!hasShownCompletion)
        {
            hasShownCompletion = true;
            ui.ShowLessonEnd(true);
            DatabaseHandler.Instance.UpdateLessonProgress(userID, lessonID, 100f, 100);
            Debug.Log("Updated users progress to 100%");
            videoPlayer.loopPointReached -= OnVideoEnd;
        }
        else
        {
            Debug.Log("Video replay finished — no popup this time.");
        }
    }

    public void PlayVideo()
    {
        Debug.Log("Playing video...");
        videoPlayer.Play();
        UserLessonProgress prevProgress = DatabaseHandler.Instance.GetLessonProgress(userID, lessonID);
        if (prevProgress != null && prevProgress.ProgressPercentage == 0)
        {
            DatabaseHandler.Instance.UpdateLessonProgress(userID, lessonID, 100f, 50f);
            Debug.Log("Updated users progress to 50%");
        }
    }

    public void PauseVideo()
    {
        videoPlayer.Pause();
    }

    public void StopVideo()
    {
        videoPlayer.Stop();
    }

    public void ReplayVideo()
    {
        Debug.Log("Replaying video...");

        hasShownCompletion = true; // don’t show completion popup again

        videoPlayer.Stop();

        // Re-prepare video properly before playing
        videoPlayer.prepareCompleted -= OnReplayPrepared;
        videoPlayer.prepareCompleted += OnReplayPrepared;
        videoPlayer.Prepare(); // don’t call Play() yet
    }

    private void OnReplayPrepared(VideoPlayer vp)
    {
        vp.prepareCompleted -= OnReplayPrepared;

        if (videoDisplay != null)
            videoDisplay.texture = vp.texture;

        // Give the audio buffer a little time to fill
        StartCoroutine(PlayAfterBuffer(vp));
    }

    private IEnumerator PlayAfterBuffer(VideoPlayer vp)
    {
        yield return new WaitForSeconds(0.1f); // allow 1-2 audio frames to preload
        vp.Play();
    }
}

//using System.Collections;
//using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Button playButton;
    public Button stopButton;

    void Start()
    {
        // Add button listeners
        playButton.onClick.AddListener(PlayVideo);
        stopButton.onClick.AddListener(StopVideo);
    }

    void PlayVideo()
    {
        if (!videoPlayer.isPlaying)
            videoPlayer.Play();
    }

    void StopVideo()
    {
        if (videoPlayer.isPlaying)
            videoPlayer.Pause();  // Use Stop() if you want it to reset
    }
}


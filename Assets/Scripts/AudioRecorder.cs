using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSaver : MonoBehaviour
{
    public Audio_Input_Manager audioInputManager;

    public string fileName;

    AudioClip sourceClip;
    AudioClip recording;

    // Helpful example:
    // https://gist.github.com/darktable/2317063

    public AudioSource microphone;
    public AudioClip lastClip;
    public int lastPosition;

    public List<AudioClip> clips;

    void Awake(){
        // Just need this to not be null so NewClip() can safely conduct checks without checking
        // if it's null when that's only necessary once
        // Needed to check if currentClip was anyways I hate efficiency
        // - Alex
        lastClip = AudioClip.Create("Not-Null", 1, 1, 44100, false);
    }

    void Start(){
        microphone = Audio_Input_Manager.Instance.microphoneAudioSource;
    }

    string CompileFileName(){
        return null;
    }

    void Update(){
        Debug.Log("Hi");
        AudioClip clip = NewClip();
        if(clip){
            Debug.Log("Hi2");
            clips.Add(clip);
            lastClip = clip;
        }
    }

    AudioClip NewClip(){
        AudioClip currentClip = microphone.clip;
        if(currentClip == null){
            Debug.Log("Current clip is null");
            return null;
        }

        int currentPosition = Microphone.GetPosition(null);
        if(currentPosition < lastPosition){
            Debug.Log("Looped!");
            
            // The microphone rewrites over the same clip, so the data has to be moved to a new instance
            float[] data = new float[currentClip.samples * currentClip.channels];
            currentClip.GetData(data, 0);
            AudioClip currentClipCopy = AudioClip.Create("Clip" + clips.Count, currentClip.samples, currentClip.channels, 44100, false);
            
            lastPosition = currentPosition;
            
            return currentClipCopy;
        }

        lastPosition = currentPosition;

        // if(!currentClip.Equals(lastClip)){
        //     return currentClip;
        // }
        return null;
    }

    [ContextMenu("Save Recording")]
    public void SaveRecording(){
        SavWav.Save(fileName, clips);

        // string savePath = Application.dataPath + "/Recordings/NewRecording.wav";

        // int length = sourceClip.samples / 2;
        // float[] data = new float[length];
        // sourceClip.GetData(data, 0);
        // recording.SetData(data, 0);
    }
}

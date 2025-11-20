using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FramerateManager : MonoBehaviour 
{
    public bool capFramerate;
	public int targetFrameRate = 60;
	public static FramerateManager Instance;

	private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

	private void Start()
	{
        if(!capFramerate) return;
        
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = targetFrameRate;
	}
}

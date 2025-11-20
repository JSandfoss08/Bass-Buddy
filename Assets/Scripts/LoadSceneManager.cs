using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    // Singleton implementation
    // Reference via "UI_SceneManagement.Instance"
    // -Alex
    public static LoadSceneManager Instance {get; private set;}
    // Singleton ensurance
    // Awake is called regardless of if script is enabled, unlike Start()
    // -Alex
    void Awake(){
        if(Instance != null && Instance != this){
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(this);
    }

    public LessonContent lesson;

    // Think of these strings like a Scene object. The Scene object is literally a string,
    // and doesn't have any drag-drop features in the inspector, otherwise I'd use it.
    // Don't change these unless you change the corresponding Scene asset's name to match.
    // This script setup can be added to every scene then necessary buttons can be set up.
    // -Alex
    string mainMenu = "MainMenu";
    string login = "Login";
    string practice = "Practice";
    string video = "Video";
    string tuning = "Tune";

    public void LoadMainMenu()
    {
        try
        {
            FindObjectOfType<LoadPerformanceTester>().StartSceneLoadTimer("MainMenu");
        }
        catch
        {
            Debug.Log("No SceneLoad tester found.");
        }

        SceneManager.LoadScene(mainMenu);
        if (PerformanceLogger.Instance != null)
        {
            PerformanceLogger.Instance.UpdateScene(mainMenu);
        } 
    }

    public void LoadLogin()
    {
        try
        {
            FindObjectOfType<LoadPerformanceTester>().StartSceneLoadTimer("Login");
        }
        catch
        {
            Debug.Log("No SceneLoad tester found.");
        }
        
        SceneManager.LoadScene(login);
        if (PerformanceLogger.Instance != null)
        {
            PerformanceLogger.Instance.UpdateScene(mainMenu);
        }
    }

    public void LoadPractice(int lessonID)
    {
        lesson = LessonManager.Instance.GetLessonContent(lessonID);

        try
        {
            FindObjectOfType<LoadPerformanceTester>().StartSceneLoadTimer("Practice");
        }
        catch
        {
            Debug.Log("No SceneLoad tester found.");
        }

        SceneManager.LoadScene(practice);
        if (PerformanceLogger.Instance != null)
        {
            PerformanceLogger.Instance.UpdateScene(mainMenu);
        }
    }

    public void LoadTuning()
    {   
        try
        {
            FindObjectOfType<LoadPerformanceTester>().StartSceneLoadTimer("Tune");
        }
        catch
        {
            Debug.Log("No SceneLoad tester found.");
        }

        SceneManager.LoadScene(tuning);
    }
    
    public void LoadVideoLesson()
    {
        try
        {
            FindObjectOfType<LoadPerformanceTester>().StartSceneLoadTimer("Video");
        }
        catch
        {
            Debug.Log("No SceneLoad tester found.");
        }

        SceneManager.LoadScene(video);
    }

    public LessonContent GetLessonContents()
    {
        return lesson;
    }

    public void ExitApplication(){
        // This should be reduced to just Application.Quit() for the final build,
        // but this covers all bases for testing purposes.
        // -Alex
        /*
        if(Application.isEditor){
            EditorApplication.isPlaying = false;
        }else{
            Application.Quit();
        }
        */
        Application.Quit();
    }
}

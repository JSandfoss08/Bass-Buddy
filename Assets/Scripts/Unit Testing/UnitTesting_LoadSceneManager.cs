using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UnitTesting_LoadSceneManager : MonoBehaviour
{
    string testScene;

    void Start(){
        Initialize();
        StartCoroutine(PerformTestCases());
    }

    void Initialize(){
        testScene = SceneManager.GetActiveScene().name;
        DontDestroyOnLoad(this);
        Application.quitting += ConfirmApplicationExited;
    }

    IEnumerator PerformTestCases(){
        Debug.Log("Unit Test Starting: LoadSceneManager");
        yield return new WaitForSeconds(3.0f);

        StartCoroutine(TestSingleton());
        yield return new WaitForSeconds(3.0f);

        StartCoroutine(TestLoadMainMenu());
        yield return new WaitForSeconds(3.0f);

        StartCoroutine(TestLoadLogin());
        yield return new WaitForSeconds(3.0f);

        StartCoroutine(TestLoadPractice());
        yield return new WaitForSeconds(3.0f);

        StartCoroutine(TestLoadTuning());
        yield return new WaitForSeconds(3.0f);

        TestGetLessonContents();
        yield return new WaitForSeconds(3.0f);

        TestExitApplication();
    }

    IEnumerator TestSingleton(){
        Instantiate(LoadSceneManager.Instance);
        yield return new WaitForSeconds(3.0f);
        LoadSceneManager[] loadSceneManagers = FindObjectsOfType<LoadSceneManager>();

        if(loadSceneManagers.Length != 1){
            Debug.LogWarning("Failed Test: Singleton w/ " + loadSceneManagers.Length + " instances");
            yield break;
        }

        Debug.Log("Passed Test: Singleton");
        SceneManager.LoadScene(testScene);
    }

    IEnumerator TestLoadMainMenu(){
        LoadSceneManager.Instance.LoadMainMenu();
        yield return new WaitForSeconds(3.0f);

        Scene loadedScene = SceneManager.GetActiveScene();
        if(loadedScene.name != "MainMenu"){
            Debug.LogWarning("Failed Test: Load Main Menu w/ loaded scene == " + loadedScene.name);
            SceneManager.LoadScene(testScene);
            yield break;
        }

        Debug.Log("Passed Test: Load Main Menu");
        SceneManager.LoadScene(testScene);
    }

    IEnumerator TestLoadLogin(){
        LoadSceneManager.Instance.LoadLogin();
        yield return new WaitForSeconds(3.0f);

        Scene loadedScene = SceneManager.GetActiveScene();
        if(loadedScene.name != "Login"){
            Debug.LogWarning("Failed Test: Load Login w/ loaded scene == " + loadedScene.name);
            SceneManager.LoadScene(testScene);
            yield break;
        }

        Debug.Log("Passed Test: Load Login");
        SceneManager.LoadScene(testScene);
    }

    IEnumerator TestLoadPractice(){
        // LoadSceneManager.Instance.LoadPractice(LessonContent.DefaultLesson);
        yield return new WaitForSeconds(3.0f);

        Scene loadedScene = SceneManager.GetActiveScene();
        if(loadedScene.name != "Practice"){
            Debug.LogWarning("Failed Test: Load Practice w/ loaded scene == " + loadedScene.name);
            SceneManager.LoadScene(testScene);
            yield break;
        }

        Debug.Log("Passed Test: Load Practice");
        SceneManager.LoadScene(testScene);
    }

    IEnumerator TestLoadTuning(){
        LoadSceneManager.Instance.LoadTuning();
        yield return new WaitForSeconds(3.0f);

        Scene loadedScene = SceneManager.GetActiveScene();
        if(loadedScene.name != "Tuning"){
            Debug.LogWarning("Failed Test: Load Tuning w/ loaded scene == " + loadedScene.name);
            SceneManager.LoadScene(testScene);
            yield break;
        }

        Debug.Log("Passed Test: Load Tuning");
        SceneManager.LoadScene(testScene);
    }

    // Is this depreciated now?
    void TestGetLessonContents(){
        LessonContent lessonContent = LoadSceneManager.Instance.GetLessonContents();

        Debug.Log("Passed Test: Get Lesson Contents");
    }

    void TestExitApplication(){
        LoadSceneManager.Instance.ExitApplication();

        Invoke("ApplicationFailedToExit", 5f);
    }

    void ApplicationFailedToExit(){
        Debug.LogWarning("Failed Test: Exit Application");
    }

    void ConfirmApplicationExited(){
        Debug.Log("Passed Test: Exit Application");
        Debug.Log("Unit Test Completed: LoadSceneManager");
    }

    // Manual Context Menu Testing
    // Coroutines aren't supported outside of runtime, so these are more limited. Just for debugging
    [ContextMenu("Manual Test: Load Tuning")]
    void ManualTestLoadTuning(){
        LoadSceneManager.Instance.LoadTuning();
    }
}

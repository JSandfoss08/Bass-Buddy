using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitTesting_LessonManager : MonoBehaviour
{
    public int newUserId;
    public int completionistUserId;

    void Start(){
        Initialize();
        StartCoroutine(PerformTestCases());
    }

    void Initialize(){
        // Need to create a user that has no data and a user that has all
        // Then conduct tests for each one
        // newUserId = ?
        // completionistUserId = ?
    }

    IEnumerator PerformTestCases(){
        Debug.Log("Unit Test Starting: LessonManager");
        yield return new WaitForSeconds(1.0f);

        StartCoroutine(TestSingleton());
        yield return new WaitForSeconds(1.5f);

        TestGetLessonContent();
    }

    IEnumerator TestSingleton(){
        Instantiate(LoadSceneManager.Instance);
        yield return new WaitForSeconds(1.0f);

        LessonManager[] lessonManagers = FindObjectsOfType<LessonManager>();
        if(lessonManagers.Length != 1){
            Debug.LogWarning("Failed Test: Singleton w/ " + lessonManagers.Length);
            yield break;
        }

        Debug.Log("Passed Test: Singleton");
    }

    void TestGetLessonContent(){
        // Just ensuring something is returned for now, I still need to full
        // compare the LessonContent recieved to what is expected
        for(int lessonId = 1; lessonId <= 4; lessonId++){
            LessonContent lessonContent = LessonManager.Instance.GetLessonContent(lessonId);
            if(lessonContent == null){
                Debug.LogWarning("Faild Test: Get Lesson Content @ lessonId == " + lessonId + " w/ lessonContent == " + lessonContent);
                return;
            }
        }

        Debug.Log("Passed Test: Get Lesson Content");
    }

    void TestGetLessonInfo(){
        // Same as with LessonContent
        for(int lessonId = 1; lessonId <= 4; lessonId++){
            LessonInfo lessonInfo = LessonManager.Instance.GetLessonInfo(lessonId);
            if(lessonInfo == null){
                Debug.LogWarning("Failed Test: Get Lesson Info @ lessonId == " + lessonId + " w/ lessonInfo == " + lessonInfo);
                return;
            }
        }

        Debug.Log("Passed Test: Get Lesson Info");
    }

    // This could be conducted a lot more efficiently with the tests for Info and Content done on the returns,
    // but I think that goes against the intent of unit testing
    void TestGetCompleteLesson(){
        // Same as Content/Info
        for(int lessonId = 1; lessonId <= 4; lessonId++){
            // Giving the tuple a name isn't necessary but I think it's clearer
            (LessonInfo info, LessonContent content) completeLesson = LessonManager.Instance.GetCompleteLesson(lessonId);
            if(completeLesson.info == null){
                Debug.LogWarning("Failed Test: Get Complete Lesson @ lessonId == " + lessonId + " w/ lessonInfo == " + completeLesson.info);
                return;
            }
            if(completeLesson.content == null){
                Debug.LogWarning("Failed Test: Get Complete Lesson @ lessonId == " + lessonId + " w/ lessonContent == " + completeLesson.content);
                return;
            }
        }

        Debug.Log("Passed Test: Get Complete Lesson");
    }

    void TestStartLesson(){
        // Need to create test cases for every check in StartLesson()
    }

    void TestUpdateProgress(){
        // Create a dummy user, update their progress, the check their progress
        // What do I call to check user progress?
    }

    void TestCompleteLesson(){
        // Go through new user, checking if all lessons are false

        // Go through completionist user, checking if all lessons are true
    }

    void TestIsLessonUnlocked(){
        // Go through new user, checking if all lessons except the first are false

        // Go through completionist user, checking if all lessons are true
    }

    void TestGetTotalLessonCount(){
        int totalLessonCount = LessonManager.Instance.GetTotalLessonCount();
        if(totalLessonCount != 5){
            Debug.LogWarning("Failed Test: Get Total Lesson Count w/ totalLessonCount == " + totalLessonCount);
            return;
        }

        Debug.Log("Passed Test: Get Total Lesson Count");
    }

    void TestHasLessonContent(){
        // Known lessons, tests expecting true
        for(int lessonId = 1; lessonId <= 4; lessonId++){
            bool hasLessonContent = LessonManager.Instance.HasLessonContent(lessonId);
            if(!hasLessonContent){
                Debug.LogWarning("Failed Test: Has Lesson Content @ lessonId == " + lessonId + " w/ hasLessonContent == " + hasLessonContent);
                return;
            }
        }

        // Unknown lessons, tests expecting false
        for(int lessonId = 5; lessonId <= 10000; lessonId++){
            bool hasLessonContent = LessonManager.Instance.HasLessonContent(lessonId);
            if(hasLessonContent){
                Debug.LogWarning("Failed Test: Has Lesson Content @ lessonId == " + lessonId + " w/ hasLessonContent == " + hasLessonContent);
                return;
            }
        }

        Debug.Log("Passed Test: Has Lesson Content");
    }
}

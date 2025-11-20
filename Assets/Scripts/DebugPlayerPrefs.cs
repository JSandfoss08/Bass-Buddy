using UnityEngine;

public class DebugPlayerPrefs : MonoBehaviour
{
    [ContextMenu("Check PlayerPrefs")]
    void CheckPlayerPrefs()
    {
        int userId = PlayerPrefs.GetInt("CurrentUserID", -1);
        int lessonId = PlayerPrefs.GetInt("CurrentLessonID", -1);
        
        Debug.Log($"CurrentUserID: {userId}");
        Debug.Log($"CurrentLessonID: {lessonId}");
    }

    [ContextMenu("Clear PlayerPrefs")]
    void ClearPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("PlayerPrefs cleared");
    }

    [ContextMenu("Set Test Values")]
    void SetTestValues()
    {
        PlayerPrefs.SetInt("CurrentUserID", 1);
        PlayerPrefs.SetInt("CurrentLessonID", 1);
        PlayerPrefs.Save();
        Debug.Log("Test values set: UserID=1, LessonID=1");
    }
}
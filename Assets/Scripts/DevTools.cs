using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DevTools : MonoBehaviour
{
    public UI_MainMenu mainMenuUI;
    private int currentUserID = -1;
    public TMP_Text loggedInAsText;

    public void ResetLessons()
    {
        DatabaseHandler.Instance.ResetAllData();
        if (mainMenuUI != null)
        {
            mainMenuUI.LoadLessons();
            Debug.Log("Loaded lessons");

        }
        PrintDB();
    }

    public void DeleteLessons()
    {
        DatabaseHandler.Instance.DeleteLessonsFromDatabase();
        Debug.Log("Deleted lessons");
        PrintDB();
    }

    public void DeleteAll()
    {
        DatabaseHandler.Instance.DeleteDatabase();
        PrintDB();
    }

    public void LoginJared()
    {
        string username = "jared";
        currentUserID = DatabaseHandler.Instance.Login(username, "jmsjms");

        PlayerPrefs.SetInt("CurrentUserID", currentUserID);
        PlayerPrefs.Save();
        mainMenuUI.LoadLessons();
        
        Debug.Log($"Logged in with ID {currentUserID}");
        PlayerPrefs.SetInt("CurrentUserID", currentUserID);
        loggedInAsText.text = $"Logged in as user {currentUserID} | {username}";
    }

    public void LoginAlex()
    {
        string username = "alex";
        currentUserID = DatabaseHandler.Instance.Login(username, "123456");

        PlayerPrefs.SetInt("CurrentUserID", currentUserID);
        PlayerPrefs.Save();
        mainMenuUI.LoadLessons();
        
        Debug.Log($"Logged in with ID {currentUserID}");
        PlayerPrefs.SetInt("CurrentUserID", currentUserID);
        loggedInAsText.text = $"Logged in as user {currentUserID} | {username}";
    }

    public void PrintDB()
    {
        PrintUsers();
        PrintLessonsAndProgress();
    }

    public void PrintLessonsAndProgress()
    {
        List<User> users = DatabaseHandler.Instance.GetAllUsers();
        List<LessonWithProgress> lessonsWithProgress = new List<LessonWithProgress>();

        foreach (User user in users)
        {
            List<LessonWithProgress> userLessonAndProgress = DatabaseHandler.Instance.GetLessonsWithProgress(user.UserId);
            lessonsWithProgress.AddRange(userLessonAndProgress);
        }

        string lessonString = "Lessons in DB|  ";

        foreach (LessonWithProgress lesson in lessonsWithProgress)
        {
            lessonString += $"Lesson name: {lesson.Lesson.LessonName} | Progress User ID: {lesson.Progress.UserId} Progress Lesson ID: {lesson.Progress.LessonId}";
        }
        Debug.Log(lessonString);
    }

    public void PrintUsers()
    {
        string userString = "Users in DB|  ";
        List<User> users = DatabaseHandler.Instance.GetAllUsers();

        foreach (User user in users)
        {
            userString += $"Name: {user.Username} | ID: {user.UserId}";
        }
        Debug.Log(userString);
    }
}

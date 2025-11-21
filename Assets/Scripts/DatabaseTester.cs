using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Diagnostics;
using System.Threading;

public class DatabaseTester : MonoBehaviour, Tester
{
    public Action OnTestsCompleted { get; set; }

    public int iterations = 100;

    private string logsDir;
    private string addUserLog;
    private string removeUserLog;
    private string updateLessonLog;
    private string integrationAddRemoveLog;
    private string integrationAddUpdateRemoveLog;

    private DatabaseHandler db_handler;

    void Awake()
    {
        logsDir = Path.Combine(Application.persistentDataPath, "Logs");
        Directory.CreateDirectory(logsDir);

        addUserLog = Path.Combine(logsDir, "AddUser_UnitTest.csv");
        removeUserLog = Path.Combine(logsDir, "RemoveUser_UnitTest.csv");
        updateLessonLog = Path.Combine(logsDir, "UpdateLessonProgress_UnitTest.csv");
        integrationAddRemoveLog = Path.Combine(logsDir, "AddRemove_IntegrationTest.csv");
        integrationAddUpdateRemoveLog = Path.Combine(logsDir, "AddUpdateRemove_IntegrationTest.csv");

        InitCSV(addUserLog);
        InitCSV(removeUserLog);
        InitCSV(updateLessonLog);
        InitCSV(integrationAddRemoveLog);
        InitCSV(integrationAddUpdateRemoveLog);
    }

    private void InitCSV(string path)
    {
        if (!File.Exists(path))
            File.WriteAllText(path, "Timestamp,TestName,Result,ResponseTime(ms),Details\n");
    }

    [ContextMenu("Run All Database Tests")]
    public void StartTests()
    {
        db_handler = DatabaseHandler.Instance;
        if (db_handler == null)
        {
            UnityEngine.Debug.LogError("DatabaseHandler not found!");
            return;
        }

        StartCoroutine(RunAllTests());
    }

    private IEnumerator RunAllTests()
    {
        yield return RunUnitTestsCoroutine();
        yield return RunIntegrationTestsCoroutine();

        UnityEngine.Debug.Log("All tests completed!");
        OnTestsCompleted?.Invoke();
    }

    // Unit tests
    [ContextMenu("Run Unit Tests")]
    public void RunUnitTests()
    {
        StartCoroutine(RunUnitTestsCoroutine());
    }

    private IEnumerator RunUnitTestsCoroutine()
    {
        UnityEngine.Debug.Log("RUNNING UNIT TESTS");

        yield return Test_AddUser();
        yield return Test_RemoveUser();
        yield return Test_UpdateLessonProgress();

        UnityEngine.Debug.Log("UNIT TESTS COMPLETE");
    }

    private IEnumerator Test_AddUser()
    {
        for (int i = 0; i < iterations; i++)
        {
            string username = "Unit_AddUser";
            db_handler.RemoveUser(username, "test123");

            Stopwatch timer = Stopwatch.StartNew();
            bool success = db_handler.AddUser(username, "test123", false);
            timer.Stop();

            User user = db_handler.GetUser(username);

            string result = (success && user != null) ? "PASS" : "FAIL";
            LogResult(addUserLog, "AddUser", result, success ? $"UserID={user.UserId}" : "User creation failed", timer.ElapsedMilliseconds);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator Test_RemoveUser()
    {
        for (int i = 0; i < iterations; i++)
        {
            string username = "Unit_RemoveUser";
            db_handler.AddUser(username, "test123", false);

            Stopwatch timer = Stopwatch.StartNew();
            bool success = db_handler.RemoveUser(username, "test123");
            timer.Stop();

            User user = db_handler.GetUser(username);

            string result = (success && user == null) ? "PASS" : "FAIL";
            LogResult(removeUserLog, "RemoveUser", result, user == null ? "User removed" : "User still exists", timer.ElapsedMilliseconds);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator Test_UpdateLessonProgress()
    {
        for (int i = 0; i < iterations; i++)
        {
            string username = "Unit_UpdateLesson";
            db_handler.AddUser(username, "test123", false);
            int userID = db_handler.GetUserID(username);
            List<LessonInfo> lessons = db_handler.GetAllLessons();
            int lessonID = lessons[0].LessonId;

            Stopwatch timer = Stopwatch.StartNew();
            db_handler.UpdateLessonProgress(userID, lessonID, 100f, 90f);
            timer.Stop();

            UserLessonProgress progress = db_handler.GetLessonProgress(userID, lessonID);

            string result = (progress != null && progress.ProgressPercentage >= 90f) ? "PASS" : "FAIL";
            LogResult(updateLessonLog, "UpdateLessonProgress", result, $"Progress={progress?.ProgressPercentage}%", timer.ElapsedMilliseconds);

            db_handler.RemoveUser(username, "test123");
            yield return new WaitForSeconds(0.2f);         
        }
    }

    // Integration tests
    [ContextMenu("Run Integration Tests")]
    public void RunIntegrationTests()
    {
        StartCoroutine(RunIntegrationTestsCoroutine());
    }

    private IEnumerator RunIntegrationTestsCoroutine()
    {
        UnityEngine.Debug.Log("RUNNING INTEGRATION TESTS");
        yield return Integration_AddRemove();
        yield return Integration_AddUpdateRemove();
        UnityEngine.Debug.Log("INTEGRATION TESTS COMPLETE");
    }

    private IEnumerator Integration_AddUpdateRemove()
    {
        for (int i = 0; i < iterations; i++)
        {
            string username = "IntegrationUser";
            db_handler.RemoveUser(username, "test123");

            Stopwatch timer = Stopwatch.StartNew();
            bool added = db_handler.AddUser(username, "test123", false);
            int userID = db_handler.GetUserID(username);

            if (added && userID != -1)
                db_handler.UpdateLessonProgress(userID, db_handler.GetAllLessons()[0].LessonId, 100f, 100f);

            bool removed = db_handler.RemoveUser(username, "test123");
            timer.Stop();

            bool success = added && removed && db_handler.GetUser(username) == null;

            LogResult(integrationAddUpdateRemoveLog, "Integration_AddUpdateRemove",
                success ? "PASS" : "FAIL",
                success ? "Full pipeline succeeded" : "Integration error",
                timer.ElapsedMilliseconds);

            yield return new WaitForSeconds(0.2f);
        }
    }

    private IEnumerator Integration_AddRemove()
    {
        for (int i = 0; i < iterations; i++)
        {
            string username = "IntegrationUser";
            db_handler.RemoveUser(username, "test123");

            Stopwatch timer = Stopwatch.StartNew();
            bool added = db_handler.AddUser(username, "test123", false); 
            bool removed = db_handler.RemoveUser(username, "test123");
            timer.Stop();

            bool success = added && removed && db_handler.GetUser(username) == null;

            LogResult(integrationAddRemoveLog, "Integration_AddRemove",
                success ? "PASS" : "FAIL",
                success ? "Full pipeline succeeded" : "Integration error",
                timer.ElapsedMilliseconds);

            yield return new WaitForSeconds(0.2f);
        }
    }

    // Logging
    private void LogResult(string filePath, string testName, string result, string details, float elapsedMs = 0)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string line = $"{timestamp},{testName},{result},{elapsedMs},{details}";
        File.AppendAllText(filePath, line + "\n");
        UnityEngine.Debug.Log($"[{result}] {testName} | {elapsedMs} | {details}");
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SQLite4Unity3d;
using System.Security.Cryptography;
using System.Linq;
using System;
using System.Data.Common;
using System.IO;

public class DatabaseHandler : MonoBehaviour
{
    private SQLiteConnection db;
    private static DatabaseHandler _instance;

    public static DatabaseHandler Instance
    {
        get { return _instance; }
    }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    void Start()
    {
        string sourcePath = Path.Combine(Application.streamingAssetsPath, "Database", "UserDatabase.db");
        string destDir = Path.Combine(Application.persistentDataPath, "Database");
        string destPath = Path.Combine(destDir, "UserDatabase.db");

        if (!Directory.Exists(destDir))
            Directory.CreateDirectory(destDir);

        if (!File.Exists(destPath))
        {
            Debug.Log("Copying database to persistentDataPath...");
            File.Copy(sourcePath, destPath);
        }

        db = new SQLiteConnection(destPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
        Debug.Log("Database connected at: " + destPath);
        InitializeDatabase();
    }

    void InitializeDatabase()
    {
        db.CreateTable<User>();
        db.CreateTable<LessonInfo>();
        db.CreateTable<UserLessonProgress>();

        // Create unique constraint for user-lesson combination
        try
        {
            db.Execute("CREATE UNIQUE INDEX IF NOT EXISTS idx_user_lesson ON UserLessonProgress(UserId, LessonId)");
        }
        catch (Exception e)
        {
            Debug.Log("Index already exists or error: " + e.Message);
        }

        InitializeLessons();
    }

    public void UpdateUserHandedness(int userID, bool isLeftHanded)
    {
        var user = db.Table<User>().Where(u => u.UserId == userID).FirstOrDefault();
        if (user != null)
        {
            user.IsLeftHanded = isLeftHanded;
            db.Update(user);
            Debug.Log($"Updated handedness for user {userID} to {(isLeftHanded ? "Left" : "Right")}");
        }
        else
        {
            Debug.LogWarning($"User with ID {userID} not found for handedness update.");
        }
    }

    public void InitializeLessons()
    {
        // Check if lessons already exist
        if (db.Table<LessonInfo>().Count() == 0)
        {
            var lessons = new List<LessonInfo>
            {
                // Beginner Lessons
                new LessonInfo {
                    LessonName = "Basic Bass Hand Placement",
                    LessonOrder = 1,
                    Description = "Learn proper bass holding technique and hand positioning.",
                    Difficulty = "Beginner",
                    VideoName = "HandPlacement.mp4"
                },

                new LessonInfo {
                    LessonName = "Single Note",
                    LessonOrder = 2,
                    Description = "Learn how to pluck a single note at the right time.",
                    Difficulty = "Beginner"
                },

                new LessonInfo {
                    LessonName = "Basic Major Scale",
                    LessonOrder = 3,
                    Description = "Learn your first major scale pattern.",
                    Difficulty = "Beginner"
                },

                new LessonInfo {
                    LessonName = "Simple Bass Lines",
                    LessonOrder = 4,
                    Description = "Play your first complete bass line.",
                    Difficulty = "Beginner"
                },

                new LessonInfo {
                    LessonName = "Random Notes",
                    LessonOrder = 4,
                    Description = "Take on the challenge of quick finger placement!",
                    Difficulty = "Intermediate"
                },

                new LessonInfo {
                    LessonName = "Major Scale: Faster Tempo",
                    LessonOrder = 4,
                    Description = "Pick up the speed with a quick major scale!",
                    Difficulty = "Intermediate"
                },

                // Idk if we'll use all these but we can add/remove from these if we want
            };

            db.InsertAll(lessons);
            Debug.Log("Initialized " + lessons.Count + " bass lessons");
        }
    }

    //
    // User management
    //

    public int Login(string username, string password)
    {
        string hashString = hashPassword(password);

        var user = db.Table<User>().Where(u => u.Username == username && u.PasswordHash == hashString).FirstOrDefault();
        if (user != null)
        {
            Debug.Log("Login successful for user: " + user.Username);

            user.LastLoggedInAt = DateTime.Now;
            db.Update(user);
            
            return user.UserId;
        }
        else
        {
            Debug.LogWarning("Login failed for user: " + username);
            return -1;
        }
    }

    public bool RemoveUser(string username, string password)
    {
        try
        {
            string hashString = hashPassword(password);
            // Get user from databse
            User user = db.Table<User>()
                         .Where(u => u.Username == username && u.PasswordHash == hashString)
                         .FirstOrDefault();

            if (user == null)
            {
                Debug.LogWarning($"RemoveUser failed: user '{username}' not found or incorrect password.");
                return false;
            }

            // User userId to delete user from database
            int lessonCount = db.Delete<User>(user.UserId);
            db.Commit();

            Debug.Log($"Removed user '{username}' and deleted {lessonCount} progress records.");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error removing user '{username}': {e.Message}");
            return false;
        }
    }

    public bool UserIsLeftHanded(int userID)
    {
        User user = db.Table<User>().Where(u => u.UserId == userID).First();
        return user.IsLeftHanded;
    }

    public bool AddUser(string username, string password, bool isLeftHanded)
    {
        if (GetUserID(username) != -1)
        {
            Debug.LogWarning("User already exists: " + username);
            return false;
        }

        string hashString = hashPassword(password);
        var newUser = new User { Username = username, PasswordHash = hashString, IsLeftHanded = isLeftHanded };
        db.Insert(newUser);

        // Initialize default progress for all lessons for this new user
        List<LessonInfo> lessons = db.Table<LessonInfo>().ToList();
        foreach (var lesson in lessons)
        {
            db.Insert(new UserLessonProgress
            {
                UserId = newUser.UserId,
                LessonId = lesson.LessonId,
                Status = "unopened",
                ProgressPercentage = 0f
            });
        }
        db.Commit();
        Debug.Log("User created and lesson progress initialized: " + username);
        return true;
    }

    public void AssignDefaultProgress(int userId)
    {
        var lessons = db.Table<LessonInfo>().ToList();
        foreach (var lesson in lessons)
        {
            db.Insert(new UserLessonProgress
            {
                UserId = userId,
                LessonId = lesson.LessonId,
                Status = "unopened",
                ProgressPercentage = 0f
            });
        }
    }

    public int GetUserID(string username)
    {
        var user = db.Table<User>().Where(u => u.Username == username).FirstOrDefault();
        if (user != null)
        {
            Debug.Log("User " + user.UserId + " found: " + user.Username);
            return user.UserId;
        }
        else
        {
            Debug.Log("User not found: " + username);
        }
        return -1;
    }

    public User GetUser(string username)
    {
        var user = db.Table<User>().Where(u => u.Username == username).FirstOrDefault();
        if (user != null)
        {
            Debug.Log("User " + user.UserId + " found: " + user.Username);
            return user;
        }
        else
        {
            Debug.Log("User not found: " + username);
        }
        return null;
    }

    public User GetUser(int userID)
    {
        var user = db.Table<User>().Where(u => u.UserId == userID).FirstOrDefault();
        if (user != null)
        {
            Debug.Log("User " + user.UserId + " found: " + user.Username);
            return user;
        }
        else
        {
            Debug.Log("UserID not found: " + userID);
        }
        return null;
    }

    //
    // Lesson management
    //

    public List<LessonInfo> GetAllLessons()
    {
        if (db.Table<LessonInfo>() != null)
        {
            return db.Table<LessonInfo>().OrderBy(l => l.LessonOrder).ToList();
        }
        return null;
    }

    public List<User> GetAllUsers()
    {
        return db.Table<User>().ToList();
    }

    public LessonInfo GetLesson(int lessonId)
    {
        return db.Table<LessonInfo>().Where(l => l.LessonId == lessonId).FirstOrDefault();
    }

    //
    // Lesson progress management
    //

    public List<UserLessonProgress> GetUserProgress(int userId)
    {
        return db.Table<UserLessonProgress>()
                  .Where(p => p.UserId == userId)
                  .ToList();
    }

    public UserLessonProgress GetLessonProgress(int userId, int lessonId)
    {
        return db.Table<UserLessonProgress>()
                  .Where(p => p.UserId == userId && p.LessonId == lessonId)
                  .FirstOrDefault();
    }

    public void UpdateLessonStatus(int userId, int lessonId, string status)
    {
        var progress = GetLessonProgress(userId, lessonId);

        if (progress != null)
        {
            progress.Status = status;
            progress.LastAccessed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            if (status == "complete")
            {
                progress.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                progress.ProgressPercentage = 100f;
            }

            db.Update(progress);
            Debug.Log($"Updated lesson {lessonId} status to {status} for user {userId}");
        }
        else
        {
            Debug.LogWarning($"Progress not found for user {userId}, lesson {lessonId}");
        }
    }

    // Percentage is in the form of XX.XX%
    public void UpdateLessonProgress(int userId, int lessonId, float minPassingPercentage, float percentage)
    {
        var progress = GetLessonProgress(userId, lessonId);

        if (progress != null)
        {
            float clampedPercentage = Mathf.Clamp(percentage, 0f, 100f);

            if (clampedPercentage > progress.ProgressPercentage)
            {
                progress.ProgressPercentage = Mathf.Clamp(percentage, 0f, 100f);
                progress.LastAccessed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                if (progress.Status == "unopened" && percentage > 0)
                {
                    progress.Status = "in_progress";
                }

                if (percentage >= minPassingPercentage && progress.Status != "complete")
                {
                    progress.Status = "complete";
                    progress.CompletedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                }

                db.Update(progress);
                db.Commit();
                Debug.Log($"Updated lesson {lessonId} progress to {percentage}% for user {userId}");
            }
            else
            {
                Debug.Log($"Skipped update: new progress ({clampedPercentage}%) is not greater than previous ({progress.ProgressPercentage}%).");
            }
        }
        else
        {
            Debug.LogWarning($"Progress not found for user {userId}, lesson {lessonId}");
        }
    }

    public List<LessonWithProgress> GetLessonsWithProgress(int userId)
    {
        var lessons = GetAllLessons();
        var progressList = GetUserProgress(userId);
        var progressDict = progressList.ToDictionary(p => p.LessonId);

        var result = new List<LessonWithProgress>();

        if (lessons != null)
        {
            foreach (var lesson in lessons)
            {
                var lessonWithProgress = new LessonWithProgress
                {
                    Lesson = lesson,
                    Progress = progressDict.ContainsKey(lesson.LessonId) ? progressDict[lesson.LessonId] : null
                };
                result.Add(lessonWithProgress);
            }
        }

        return result;
    }

    //
    // Utilities
    //

    public string hashPassword(string password)
    {
        SHA256 sha256 = SHA256.Create();
        var passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);
        var hashBytes = sha256.ComputeHash(passwordBytes);
        var hashString = System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        return hashString;
    }

    void OnApplicationQuit()
    {
        if (db != null)
        {
            db.Close();
            Debug.Log("Database connection closed");
        }
    }

     public void DeleteDatabase()
    {
        db.DeleteAll<LessonInfo>();
        db.DeleteAll<UserLessonProgress>();
        db.DeleteAll<User>();
        db.Commit();
    }

    public void DeleteDatabaseFile()
    {
        try
        {
            // Close connection first
            if (db != null)
            {
                db.Close();
                db = null;
            }

            string dbPath = Application.persistentDataPath + "/UserDatabase.db";

            if (File.Exists(dbPath))
            {
                File.Delete(dbPath);
                Debug.Log("Database file deleted");
            }

            // Reinitialize
            Start();
        }
        catch (Exception e)
        {
            Debug.LogError("Error deleting database file: " + e.Message);
        }
    }

    public void ResetAllData()
    {
        try
        {
            // Delete all data
            db.DeleteAll<UserLessonProgress>();
            db.DeleteAll<User>();
            db.DeleteAll<LessonInfo>();

            // Reset auto-increment counters
            db.Execute("DELETE FROM sqlite_sequence WHERE name='Lessons'");
            db.Execute("DELETE FROM sqlite_sequence WHERE name='UserLessonProgress'");
            db.Execute("DELETE FROM sqlite_sequence WHERE name='Users'");
            db.Commit();

            // Reinitialize lessons
            InitializeLessons();
            Debug.Log("Database reset with fresh IDs");
        }
        catch (Exception e)
        {
            Debug.LogError("Error resetting database: " + e.Message);
        }
    }

    [ContextMenu("Reset Database")]
    public void ContextMenuReset()
    {
        ResetAllData();
    }

    [ContextMenu("Delete Database File")]
    public void ContextMenuDeleteFile()
    {
        DeleteDatabaseFile();
    }

    public void DeleteLessonsFromDatabase()
    {
        db.DeleteAll<LessonInfo>();
        db.DeleteAll<UserLessonProgress>();
        db.Commit();
    }
}

// Helper class for UI display
public class LessonWithProgress
{
    public LessonInfo Lesson { get; set; }
    public UserLessonProgress Progress { get; set; }
}
using System.Collections.Generic;
using UnityEngine;

// This script bridges the lesson gameplay and the database functionality
public class LessonManager : MonoBehaviour
{
    private static LessonManager _instance;
    public static LessonManager Instance
    {
        get { return _instance; }
    }

    // Dictionary maps database lesson IDs to gameplay content
    private Dictionary<int, LessonContent> lessonContentMap;

    void Awake()
    {
        // Singleton pattern - only one LessonManager exists
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);
        InitializeLessonContent();
    }

    void InitializeLessonContent()
    {
        /*
        CONFIGURE NEW LESSONS HERE
        */

        // Map database lesson IDs to their gameplay content
        // The IDs match the LessonOrder from DatabaseHandler

        lessonContentMap = new Dictionary<int, LessonContent>();

        // Hand placement video lesson
        lessonContentMap[1] = new LessonContent(
            3f,
            new List<Note>() {},
            .90f
        );
        
        // Single note lesson
        lessonContentMap[2] = new LessonContent(
            2.5f,
            new List<Note>() {
                Note.E, Note.E, Note.E,
                Note.E, Note.E, Note.E,
                Note.E, Note.E, Note.E,
                Note.E
            },
            0.90f
        );

        // Basic major scale
        lessonContentMap[3] = new LessonContent(
            2.5f,
            new List<Note>() {
                Note.C, Note.D, Note.E,
                Note.F, Note.G, Note.A,
                Note.B, Note.A, Note.G,
                Note.F, Note.E, Note.D,
                Note.C
            },
            0.90f
        );

        // Basic bassline
        lessonContentMap[4] = new LessonContent(
            2.5f,
            new List<Note>() {
                Note.E, Note.F, Note.FSharp,
                Note.A, Note.ASharp, Note.B,
                Note.D, Note.DSharp, Note.E,
                Note.G, Note.GSharp, Note.A
            },
            0.90f
        );

        // Random Notes
        lessonContentMap[4] = new LessonContent(
            2.5f,
            new List<Note>() {
                Note.GetRandomNote(), Note.GetRandomNote(), Note.GetRandomNote(),
                Note.GetRandomNote(), Note.GetRandomNote(), Note.GetRandomNote(),
                Note.GetRandomNote(), Note.GetRandomNote(), Note.GetRandomNote(),
                Note.GetRandomNote()
            },
            0.90f
        );

        // Basic major scale, faster
        // Is this meant to be index 5?
        lessonContentMap[3] = new LessonContent(
            1.8f,
            new List<Note>() {
                Note.C, Note.D, Note.E,
                Note.F, Note.G, Note.A,
                Note.B, Note.A, Note.G,
                Note.F, Note.E, Note.D,
                Note.C
            },
            0.90f
        );

        Debug.Log($"LessonManager: Initialized {lessonContentMap.Count} lesson contents");
    }

    // Get the gameplay content for a specific lesson
    public LessonContent GetLessonContent(int lessonId)
    {
        if (lessonContentMap.ContainsKey(lessonId))
        {
            return lessonContentMap[lessonId];
        }

        Debug.LogWarning($"LessonManager: No content found for lesson {lessonId}");
        return null;
    }

    // Get database lesson info (name, description, difficulty, etc.)
    public LessonInfo GetLessonInfo(int lessonId)
    {
        return DatabaseHandler.Instance.GetLesson(lessonId);
    }

    // Get both database info AND gameplay content together
    public (LessonInfo info, LessonContent content) GetCompleteLesson(int lessonId)
    {
        return (GetLessonInfo(lessonId), GetLessonContent(lessonId));
    }

    // Start a lesson - updates database status and returns content
    public LessonContent StartLesson(int userId, int lessonId)
    {
        if (!IsLessonUnlocked(userId, lessonId))
            return null;

        // Get the database info
        var info = GetLessonInfo(lessonId);
        if (info == null)
        {
            Debug.LogError($"LessonManager: Lesson {lessonId} does not exist in database!");
            return null;
        }

        // Get the gameplay content
        var content = GetLessonContent(lessonId);
        if (content == null)
        {
            Debug.LogError($"LessonManager: No gameplay content for lesson {lessonId}!");
            return null;
        }

        // Update database: mark lesson as "in_progress" if it was "unopened"
        var progress = DatabaseHandler.Instance.GetLessonProgress(userId, lessonId);
        if (progress != null && progress.Status == "unopened")
        {
            DatabaseHandler.Instance.UpdateLessonStatus(userId, lessonId, "in_progress");
            Debug.Log($"LessonManager: Started lesson '{info.LessonName}' for user {userId}");
        }
        else if (progress != null)
        {
            Debug.Log($"LessonManager: Continuing lesson '{info.LessonName}' (status: {progress.Status})");
        }

        return content;
    }

    // Update progress during gameplay (wrapper for convenience)
    public void UpdateProgress(int userId, int lessonId, int completedNotes, int totalNotes)
    {
        if (totalNotes == 0)
        {
            Debug.LogWarning("LessonManager: Cannot update progress with 0 total notes");
            return;
        }

        float percentage = (completedNotes / (float)totalNotes) * 100f;
        DatabaseHandler.Instance.UpdateLessonProgress(userId, lessonId, percentage);
    }

    // Complete a lesson (mark as 100%)
    public void CompleteLesson(int userId, int lessonId)
    {
        DatabaseHandler.Instance.UpdateLessonProgress(userId, lessonId, 100f);

        var info = GetLessonInfo(lessonId);
        if (info != null)
        {
            Debug.Log($"LessonManager: Lesson '{info.LessonName}' completed by user {userId}!");
        }
    }

    // Check if a lesson is unlocked (for sequential progression)
    public bool IsLessonUnlocked(int userId, int lessonId)
    {
        // First lesson is always unlocked
        if (lessonId == 1) return true;

        // Check if previous lesson is completed
        var previousProgress = DatabaseHandler.Instance.GetLessonProgress(userId, lessonId - 1);

        if (previousProgress == null)
        {
            Debug.LogWarning($"LessonManager: No progress found for previous lesson {lessonId - 1}");
            return false;
        }

        return previousProgress.Status == "complete";
    }

    // Get the number of lessons available
    public int GetTotalLessonCount()
    {
        return lessonContentMap.Count;
    }

    // Check if lesson content exists
    public bool HasLessonContent(int lessonId)
    {
        return lessonContentMap.ContainsKey(lessonId);
    }
}
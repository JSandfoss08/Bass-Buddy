using SQLite4Unity3d;
using System;

[Table("Users")]
public class User
{
    [PrimaryKey, AutoIncrement]
    public int UserId { get; set; }
    
    [Unique, NotNull]
    public string Username { get; set; }
    
    [NotNull]
    public string PasswordHash { get; set; }
    
    public DateTime CreatedAt { get; set; }

    public DateTime LastLoggedInAt { get; set; }

    public bool IsLeftHanded { get; set; }
    
    public User()
    {
        CreatedAt = DateTime.Now;
        LastLoggedInAt = DateTime.Now;
    }
}

[Table("Lessons")]
public class LessonInfo
{
    [PrimaryKey, AutoIncrement]
    public int LessonId { get; set; }
    
    [NotNull]
    public string LessonName { get; set; }
    
    public int LessonOrder { get; set; }
    
    public string Description { get; set; }

    public string VideoName { get; set; }
    
    public string Difficulty { get; set; }
}

[Table("UserLessonProgress")]
public class UserLessonProgress
{
    [PrimaryKey, AutoIncrement]
    public int ProgressId { get; set; }
    
    [NotNull, Indexed]
    public int UserId { get; set; }
    
    [NotNull, Indexed]
    public int LessonId { get; set; }
    
    [NotNull]
    public string Status { get; set; } // "unopened", "in_progress", "complete"
    
    public float ProgressPercentage { get; set; }
    
    public String LastAccessed { get; set; }
    
    public String CompletedAt { get; set; }
    
    public UserLessonProgress()
    {
        Status = "unopened";
        ProgressPercentage = 0f;
    }
}
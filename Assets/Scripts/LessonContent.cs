using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LessonContent
{
    float timeBetween;
    float minPassingPercentage;
    List<Note> notes = new List<Note>();
    List<Chord> chords = new List<Chord>();
    string videoName;

    public LessonContent(float timeBetween_, List<Note> notes_, float minPercent)
    {
        timeBetween = timeBetween_;
        notes = notes_;
        minPassingPercentage = minPercent;
        videoName = null;
    }

    public LessonContent(float timeBetween_, List<Note> notes_) {
        timeBetween = timeBetween_;
        minPassingPercentage = 100f;
        videoName = null;
    }

    public LessonContent( string videoName_) {
        timeBetween = 0;
        chords = null;
        minPassingPercentage = 100;
        videoName = videoName_;
    }

    public float GetTimeBetween()
    {
        return timeBetween;
    }

    public bool IsVideoLesson()
    {
        return videoName != null;
    }
    
    public string GetVideoName()
    {
        return videoName;
    }

    public int GetLength() {
        return notes.Count;
    }

    public Note GetNote(int index)
    {
        if (index >= notes.Count)
        {
            return Note.None;
        }
        return notes[index];
    }

    public int GetNumOfNotes()
    {
        return notes.Count;
    }

    public float GetMinPassingPercentage()
    {
        return minPassingPercentage;
    }

    public Chord GetChord(int index)
    {
        if (index >= chords.Count)
        {
            return Chord.None;
        }
        return chords[index];
    }
    
    public static LessonContent DefaultLesson = new LessonContent(2f, new List<Note>(){Note.A, Note.A, Note.A, Note.A});

    public static LessonContent TestLesson = new LessonContent(
        2f, 
        new List<Note>(){
            Note.C, 
            Note.CSharp, 
            Note.D, 
            Note.DSharp, 
            Note.E, 
            Note.F, 
            Note.FSharp, 
            Note.G, 
            Note.GSharp, 
            Note.A, 
            Note.ASharp, 
            Note.B
        }
    );

    public static LessonContent Lesson1 = new LessonContent(3f, new List<Note>() { Note.A, Note.ASharp, Note.G, Note.F });
    public static LessonContent Lesson2 = new LessonContent(2f, new List<Note>(){Note.A, Note.ASharp, Note.G, Note.F});
    public static LessonContent Lesson3 = new LessonContent(1f, new List<Note>(){Note.A, Note.ASharp, Note.G, Note.F});

    public enum Lessons{
        DefaultLesson,
        TestLesson,
        Lesson1,
        Lesson2,
        Lesson3
    }
}
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note
{
    // Visualization/UI
    string name;
    int fretIndex;
    int stringIndex;

    public Note(string name_){
        name = name_;
    }

    public Note(string name_, int stringIndex_, int fretIndex_){
        name = name_;
        stringIndex = stringIndex_;
        fretIndex = fretIndex_;
    }

    public int GetStringIndex(){
        return stringIndex;
    }

    public int GetFretIndex(){
        return fretIndex;
    }

    public string GetName(){
        return name;
    }

    // float semitoneOffset = 12f * Mathf.Log(frequency / 440f, 2f);
    // private static readonly string[] noteNames = 
    //     { "C", "C#", "D", "D#", "E", "F", "F#", "G", "G#", "A", "A#", "B" };
    
    public static readonly Note C = new Note("C", 1, 2);
    public static readonly Note CSharp = new Note("C#", 1, 1);
    public static readonly Note D = new Note("D", 1, 0);
    public static readonly Note DSharp = new Note("D#", 2, 4);
    public static readonly Note E = new Note("E", 2, 3);
    public static readonly Note F = new Note("F", 2, 2);
    public static readonly Note FSharp = new Note("F#", 2, 1);
    public static readonly Note G = new Note("G", 0, 2);
    public static readonly Note GSharp = new Note("G#", 0, 3);
    public static readonly Note A = new Note("A", 3, 3);
    public static readonly Note ASharp = new Note("A#", 3, 2);
    public static readonly Note B = new Note("B", 1, 3);

    // Order copied 
    public static readonly List<Note> AllNotes = new List<Note>(){
        C, CSharp, D, DSharp, E, F, FSharp, G, GSharp, A, ASharp, B
    };

    public static readonly Note None = new Note("None");

    public static Note GetRandomNote(){
        return AllNotes[Random.Range(0, AllNotes.Count)];
    }
}
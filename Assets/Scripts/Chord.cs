using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chord
{
    // Visualization/UI
    string name;
    Note[] notes;
    int stringCnt;

    // presses_ relate to which fret, starting at 1. A value of 0 is read as no presses for that string
    // stringsPlayed_ relate to how many of the strings should be played, starting at 1.
    // -Alex
    public Chord(string name_, Note[] notes_, int stringCnt_){
        name = name_;
        notes = notes_;
        stringCnt = stringCnt_;
    }

    public string GetName(){
        return name;
    }

    public Note[] GetNotes(){
        return notes;
    }

    public int GetStringCnt(){
        return stringCnt;
    }

    public static readonly Chord G5 = new Chord("G5", new Note[]{Note.G, Note.D}, 2);
    public static readonly Chord GMin = new Chord("GMin", new Note[]{Note.G, Note.ASharp}, 3);
    public static readonly Chord GMaj = new Chord("GMaj", new Note[]{Note.G, Note.B}, 3);
    public static readonly Chord G7 = new Chord("G7", new Note[]{Note.G, Note.D, Note.F, Note.B}, 4);
    public static readonly Chord GMaj7 = new Chord("GMaj7", new Note[]{Note.D, Note.D, Note.FSharp, Note.B}, 4);
    public static readonly Chord GMin7 = new Chord("GMin7", new Note[]{Note.G, Note.D, Note.F, Note.ASharp}, 4);
    public static readonly Chord GSus2 = new Chord("GSus2", new Note[]{Note.G}, 3);
    public static readonly Chord GSus4 = new Chord("GSus4", new Note[]{Note.G, Note.C}, 3);
    public static readonly Chord G6 = new Chord("G6", new Note[]{Note.G, Note.D, Note.E, Note.B}, 4);
    public static readonly Chord G9 = new Chord("G9", new Note[]{Note.G, Note.B, Note.F, Note.A}, 4);
    public static readonly Chord GDim = new Chord("GDim", new Note[]{Note.G, Note.CSharp, Note.G, Note.ASharp}, 4);
    public static readonly Chord GAug = new Chord("GAug", new Note[]{Note.G, Note.B, Note.DSharp}, 3);

    public static readonly List<Chord> AllChords = new List<Chord>{
        G5, GMin, GMaj, G7, GMaj7, GMin7, GSus2, GSus4, G6, G9, GDim, GAug
    };

    public static readonly Chord None = new Chord("None", new Note[0], 0);

    public static Chord GetRandomChord(){
        return AllChords[Random.Range(0, AllChords.Count)];
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

// MusicNote
using MusicNote;

/** MusicScore Class **
 * This is a simple wrapper class
 * for storing the parsed MusicXML
 * beatmap.
 * 
 * Consumes `MusicNote`s with constructor
 * and manages absolute time calculation
 * with each note loaded.
 * 
 * Then, the class only provides a public
 * "dequeue" method for reading from the
 * score.
 * TODO: maybe update "dequeue" with a
 * list read. if level restarts, don't 
 * necessarily want to always recompute
 * music score... can just do it once
 * then reset the read pointer.
 * 
 */
public class MusicScore
{
    /***** Private members ***************************************************/
    private double originTime = 0.0;    // the base absolute reference time
    private int numTotalNotes;          // the length of score in num of notes
    private Difficulty difficulty;
    private int BPM;
    private double secondsPerBeat;
    private int beatDuration;


    /***** Private Helper Methods ***************************************************/
    // Construct beatmap with MusicNotes
    private void LoadNote(Note note)
    {
        // If is a tied note do not enqueue, but still increment originTime
        if (!note.isTied)
        {
            beatMap.Enqueue(new Tuple<Note, double>(note, originTime));
        }

        // Update origin time with the note just enqueued's duration
        // (so that the next note played right after is correctly offset)
        originTime += CalculateNoteRelativeDuration(note);

        // Update total num notes count
        ++numTotalNotes;
    }


    // Calculate relative duration of a note from ONLY note properties
    private double CalculateNoteRelativeDuration(Note note)
    {
        double noteRelativeBeats = (double)beatDuration / note.Length;
        
        // If dotted, note length is 1.5 beats longer
        if (note.isDotted)
        {
            noteRelativeBeats *= 1.5;
        }

        return secondsPerBeat * noteRelativeBeats;
    }


    /***** Public members ***************************************************/
    public Queue<Tuple<Note, double>> beatMap;


    /***** Public methods ***************************************************/
    // Constructor
    public MusicScore(
        List<Note> notes, 
        Difficulty songDifficulty, 
        int songBPM, 
        NoteLength songBeatDuration)
    {
        difficulty = songDifficulty;
        BPM = songBPM;
        beatDuration = (int)songBeatDuration;

        secondsPerBeat = (double)60 / songBPM;

        Debug.Log("(MusicScore) Creating music score with difficulty " + difficulty
                    + " at BPM " + BPM
                    + " and beat duration of " + beatDuration);

        beatMap = new();
        foreach (Note note in notes) 
        {
            LoadNote(note);
        }
    }

    
    // Method for reading a note from map
    public Tuple<Note, double> readNote()
    {
        if (beatMap.Count > 0)
        {
            return beatMap.Dequeue();
        }
        else
        {
            return null;
        } 
    }

    // Method for peeking at a note from map
    public Tuple<Note, double> peekNote()
    {
        if (beatMap.Count > 0)
        {
            return beatMap.Peek();
        }
        else
        {
            return null;
        }
    }


    // Get remaining number of notes in song
    public int GetNumRemainingNotes()
    {
        return beatMap.Count;
    }

    // Get absolute number of notes in song
    public int GetNumTotalNotes()
    {
        return numTotalNotes;
    }

}

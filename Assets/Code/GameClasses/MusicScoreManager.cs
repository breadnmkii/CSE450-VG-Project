using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/* Docs:
 * Goal of this class is to provide an OOP oriented method
 * of organizing our "songs" into actual game elements.
 * 
 * Makes heavy use of actual music theory to organize how 
 * we can layout/manage the creation of obstacles throughout
 * the song and level.
 * 
 * General Notes/Context:
 *  Since we want to make note spawning
 *  easy to implement and the fact that this game is 
 *  continuous-timed, there are two timelines to keep track
 *  of.
 *                                  
 *  1. beat-timeline: these are the "measures" of our song,
 *     and organizes our level into discrete beats that
 *     determine when exactly we can "compose" a note that
 *     is in sync with our rhythm.
 *                                     
 *     this timeline is easy to compute by calculating the
 *     inverse BPM (MPB) as a delta-time for progressing 
 *     the level forwards by one counted beat. subdivision
 *     is accounted for by spawning subdivided notes at 
 *     fractions of the delta-time)
 *                                     
 *  2. real-timeline: this is the true timeline at which
 *     our game moves. it is the underlying concept of time
 *     that unity ticks at, and thus powers our beat-timeline.
 *                                     
 *     this timeline is at a much higher frequency to enable
 *     the polling of our beat-timeline delta-time. 
 *     provided by a "milliseconds since level start" or
 *     similar.
 * 
 * This class includes:
 *   timeSignature      --> tuple of beats per measure and note length counted as beat  (const)
 *                          [0] int: beats per measure
 *                              + less important to game as it is to us as developers,
 *                                the number of beats in a "measure" to let us know 
 *                                where we are in the song when creating a level.
 *                          [1] NoteType: counted beat
 *                              + either quarter, eighth, etc.
 *                                This essentially determines the delta-time by which 
 *                                the song progresses, and equivalently, the note that 
 *                                the BPM is relevant to.
 *                                
 *   BPM                --> obstacle density for a given song duration                  (const)
 *   songDuration       --> song duration in Math.floor(minutes)                        (const)
 *                              + flooring b/c i dont want to deal with seconds
 *                                precision greater than 2... (60s/16th)
 *                                
 *   songDurationBeats  --> base duration of song duration in beats. Calculated from
 *                          song BPM and duration
 *                          
 *                          e.g.    songBPM = 100
 *                                  songDuration = 4 (min)
 *                                  songDurationBeats = songBPM * songDuration = 400 b
 *                                  
 *                                  so we have 400 beats at which we may spawn obstacles        
 *                          
 *                              
 *   musicScore         --> The temporal location of notes within the beat-timeline     (const)
 *                              + the beat when note is registered in player zone 
 *                                (note that this does not determine when note is 
 *                                spawned...explained below)
 *   
 *   difficulty         --> The factor at which speed of projectiles is scaled
 *                          
 *                          cont.   this is an interesting aspect of the rhythm game...
 *                                  since we will spawn notes before they actually reach
 *                                  the character, there is an element of hysteresis
 *                                  
 *                                  given known distance from note spawn to character
 *                                  and any desired velocity of the note, we can 
 *                                  determine when to spawn the note some time t' before 
 *                                  time t when the note is actually played on the staff.
 *                                  
 *                                  t' = d/(v*v_diff)
 *                                  
 *                                  where v_diff is some factor defined by our difficulty
 *                                  
 *                                  tl:dr; to support spawning hysteresis, where the ACTUAL
 *                                  spawning is handled, must use real-timeline
 *                                  to spawn at time t' before beat-timeline time t.
 *   
 *   
 * The class should also be (maybe) responsible for:
 *   Obstacle lifetime (spawning and destroying outside of camera frame)
 *   
 * 
 */

public class MusicScoreManager : MonoBehaviour
{
    // Private members for defining interal song properties
    private readonly int[] _timeSignature = new int[2] { 4, NoteLength.Quarter }; // DEBUG: hardcode for now
    private readonly int _BPM = 60;     // DEBUG: hardcode for now
    private readonly int _songDuration = 4; // DEBUG: hardcode for now
    private readonly List<Note> _musicScore = new List<Note>(); // DEBUG: hardcode for now

    private int _currentBeat = 0;          // counter index to current beat in beat-timeline
    private double _timeLastBeat = 0;      // delta timer for last time in real-timeline


    // Public members for song properties (readonly)
    public readonly int songDurationBeats;
    public readonly MusicScene.Difficulty difficulty;
    

    // Constructor
    MusicScoreManager(int[] timeSignature,
                      List<Note> notes, 
                      MusicScene.Difficulty diff,
                      int BPM,
                      int songDuration)
    {
        /* Pre Checks */
        if (timeSignature.Length != 2)
        {
            throw new Exception("Invalid time signature length!");
        }
        if (BPM <= 0)
        {
            throw new Exception("BPM must be a positive valued number!");
        }

        // Define private members
        _timeSignature = timeSignature;
        _BPM = BPM;
        _songDuration = songDuration;
        _musicScore = notes;

        // Define public members
        songDurationBeats = _BPM * _songDuration;
        difficulty = diff;

        /* Post Checks */
        // checking that number of worst case (lowest granularity (sixteenth)) notes
        // can fit within duration
        if (_musicScore.Count > songDurationBeats * (NoteLength.Sixteenth/_timeSignature[1]))
        {
            throw new Exception("Incompatible staff and song duration!");
        }
    }
    
    private void Start()
    {
        // MusicScoreManager _ = gameObject.AddComponent<MusicScoreManager>();
    }
    

    // Update is called once per frame
    void Update()
    {
        print(Time.time);
        print(_BPM);
        /*
        if (Time.time >= (60 / _BPM) + _timeLastBeat)
        {
            _timeLastBeat = Time.time;
            print("Beating every " + (60 / _BPM) + "seconds per beat...");
            spawnNote();
        }
        */
    }

    /* Object lifetime management functions */
    void spawnNote()
    {
        print("Hello from MusicScoreManager!");
        print(NoteLength.Sixteenth / 16);
        // Reads staff and select notes
        // TODO: figure out how to integrate staff and notes to spawn object
    }

    void cleanupNotes()
    {
        // Determines if any notes are outside camera view
        // and deletes
        // TODO: figure out...
    }

    
}

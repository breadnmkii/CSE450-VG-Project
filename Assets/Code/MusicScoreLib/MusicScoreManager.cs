using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

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

// Speed factors
public enum Difficulty
{
    protege,    // Easy
    concert,    // Normal
    virtuoso,   // Hard
    prodigy     // rhythm-game-addict
}


public class MusicScoreManager : MonoBehaviour
{
    // Public members for song properties (readonly)
    public int BPM;
    public int songDuration;
    public List<int> timeSignature;
    public string scorePath;
    public Difficulty difficulty;

    // Private members for defining interal song properties
    private readonly int _songDurationBeats;
    private readonly List<(double, List<Note>)> _musicScore;
    private double _nowTime;                // var to hold current real-time
    private int _currBeat;                  // counter index to current beat in beat-time
    private double _timeSinceLastBeat;      // delta time for last time in real-time


    // Constructor
    public MusicScoreManager(List<int> timeSignature,
                             int BPM,
                             int songDuration,
                             string scorePath,
                             Difficulty diff)
    {
        /* Pre Checks */
        if (timeSignature.Count != 2)
        {
            throw new Exception("Invalid time signature!");
        }
        if (BPM <= 0)
        {
            throw new Exception("BPM must be a positive valued number!");
        }
        
        // Define private members
        _musicScore = processMusicScoreJSON(scorePath);
        _songDurationBeats = BPM * songDuration;

        /* Post Checks */
        // checking that number of worst case (lowest granularity (sixteenth)) notes
        // can fit within duration
        if (_musicScore.Count > _songDurationBeats * ((int)NoteLength.Sixteenth/timeSignature[1]))
        {
            throw new Exception("Incompatible staff and song duration!");
        }
    }


    // Methods
    private void Start()
    {
        _currBeat = 0;  // Every song begins on beat 0
        _timeSinceLastBeat = 0;
        // song.Begin();
        print("Time now: " + Time.time);
    }

    private void Update()
    {
        _nowTime = Time.time;
        print(_nowTime);

        if (_nowTime >= _timeSinceLastBeat + 60/BPM)
        {
            print("A beat just went off at..." + _currBeat);
            ++_currBeat;
        }



    }

    /* Reads an input string path to a Music Score JSON file
     * and parses into a list of 2-tuples of beat and list of notes
     */
    private List<(double, List<Note>)> processMusicScoreJSON(string scorePath)
    {
        List<(double, List<Note>)> score = new();

        // TODO: currently hardcoding
        print("Processing music score at " + scorePath + "...");
        for (int line=0; line < 10; ++line)
        {
            double beat = line; // Beat will eventually be read from line's json
            List<Note> notes = new();

            int numNotes = 1;   // Each beat can have up to a 4-note chord
            for (int currNote = 0; currNote < numNotes; ++currNote)
            {
                Note note = new(NoteLength.Quarter,
                                NoteLocation.Lane1,
                                NoteVoice.Melody,
                                NoteType.BlockObstacle);
                notes.Add(note);
            }

            (double, List<Note>) beatNotes = (beat, notes);
            score.Add(beatNotes);
        }
        print("Processed music score!");

        return score;
    }

    /* Object lifetime management functions */
    void spawnNote()
    {
        // Reads staff and select notes
        // TODO: figure out how to integrate staff and notes to spawn object
    }
    
}

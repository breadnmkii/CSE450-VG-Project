using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

/* Docs:
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
    /* Outlets */
    // Public lane locations
    public GameObject[] lanes;

    // Public obstacle prefabs
    // (Note: must contain Obstacles component)
    public GameObject ballProjectileA;
    public GameObject ballProjectileB;
    public GameObject wallObstacle;

    // Public members for song properties (readonly)
    public int BPM;
    public int songDuration;
    public int songNumNotes;    // DO NOT CHANGE IN INSPECTOR
    public List<int> timeSignature;
    public string scorePath;
    public Difficulty difficulty;

    // Private members for defining interal song properties
    private int _songDurationBeats;
    private Queue<Note> _musicScore;
    private double _nowTime;                // var to hold current real-time
    private int _currBeat;                  // counter index to current beat in beat-time
    private double _timeDeltaBeat;          // delta time for beat-time
    private double _timeSinceLastBeat;      // timer for last beat time in beat-time
    private double _timeSinceLastNote;      // timer for last note time in real-time

    /* Unity Loop Methods */
    private void Start()
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

        /* Define private members */
        // Beat-time delta time vars
        _currBeat = 0;  // Every song begins on beat 0
        _timeDeltaBeat = (double)60 / BPM;
        _timeSinceLastBeat = 0;
        _timeSinceLastNote = 0;

        // Music score vars
        _musicScore = ProcessMusicScoreJSON(scorePath);
        songNumNotes = _musicScore.Count();
        print("Song num notes: " + songNumNotes);
        _songDurationBeats = BPM * songDuration;


        /* Post Checks */
        // checking that number of worst case (lowest granularity (sixteenth)) notes
        // can fit within duration
        if (_musicScore.Count > _songDurationBeats * (int)NoteLength.Sixteenth)
        {
            throw new Exception("Cannot fit all of score's notes into song!");
        }

        /* Begin playing song audio */
        print("Playing song at difficulty " + difficulty);
        print("Time now: " + Time.timeSinceLevelLoad);
        // song.Begin();
    }

    private void Update()
    {
        // Play while song is still playing
        if (_currBeat < _songDurationBeats)
        {
            // Beat beat-time loop
            _nowTime = Time.timeSinceLevelLoad;
            if (_nowTime >= _timeSinceLastBeat + _timeDeltaBeat)
            {
                _timeSinceLastBeat += _timeDeltaBeat;
                print("Beat " + _currBeat + " just went off at " + _nowTime);

                ++_currBeat;
            }

            // Note real-time loop
            _nowTime = Time.timeSinceLevelLoad;
            if (_nowTime >= _timeSinceLastNote + _timeDeltaBeat &&
                _musicScore.Count > 0)
            {
                _timeSinceLastNote += SpawnNote(_musicScore.Dequeue());
                print("Note spawned at " + _nowTime);
            }
        }

    }


    /* Class Methods */


    // Obtains the specified value for a difficulty enum type
    static float GetDifficultyFactor(Difficulty diff)
    {
        switch (diff)
        {
            case Difficulty.protege:
                return 1F;
            case Difficulty.concert:
                return 2;
            case Difficulty.virtuoso:
                return 4;
            case Difficulty.prodigy:
                return 6;
            default:
                break;
        }
        return 0;
    }


    // Reads an input JSON file to process into queue of notes
    private Queue<Note> ProcessMusicScoreJSON(string scorePath)
    {
        Queue<Note> score = new();
        print("Processing music score at " + scorePath + "...");

        // TODO: currently hardcoding
        score.Enqueue(new(NoteLength.Quarter,
                      new List<NoteLocation> { NoteLocation.Lane1 },
                      NoteVoice.Melody,
                      NoteType.BallProjectileA));
        score.Enqueue(new(NoteLength.Whole,
                      new List<NoteLocation> { NoteLocation.Lane1 },
                      NoteVoice.Melody,
                      NoteType.BallProjectileA));
        score.Enqueue(new(NoteLength.Eighth,
                      new List<NoteLocation> { NoteLocation.Lane1 },
                      NoteVoice.Melody,
                      NoteType.BallProjectileA));
        score.Enqueue(new(NoteLength.Eighth,
                      new List<NoteLocation> { NoteLocation.Lane1 },
                      NoteVoice.Melody,
                      NoteType.BallProjectileA));
        score.Enqueue(new(NoteLength.Sixteenth,
                      new List<NoteLocation> { NoteLocation.Lane1 },
                      NoteVoice.Melody,
                      NoteType.BallProjectileA));
        score.Enqueue(new(NoteLength.Quarter,
                      new List<NoteLocation> { NoteLocation.Lane1 },
                      NoteVoice.Melody,
                      NoteType.BallProjectileA));
        score.Enqueue(new(NoteLength.Sixteenth,
                      new List<NoteLocation> { NoteLocation.Lane1 },
                      NoteVoice.Melody,
                      NoteType.BallProjectileA));
        score.Enqueue(new(NoteLength.Sixteenth,
                      new List<NoteLocation> { NoteLocation.Lane1 },
                      NoteVoice.Melody,
                      NoteType.BallProjectileA));
        //for (int line=0; line < 10; ++line)
        //{
        //    List<NoteLocation> chord = new List<NoteLocation> { NoteLocation.Lane1 };
        //    Note note = new(NoteLength.Quarter,
        //                    chord,
        //                    NoteVoice.Melody,
        //                    NoteType.BlockObstacle);
        //    score.Add(note);
        //}
        print("Processed music score!");

        return score;
    }


    // Method to spawn a note
    // Returns the amount of delta time to add to timer (based on note length)
    double SpawnNote(Note currNote)
    {
        // TODO: get each note's properties and spawn into world accordingly
        print("Read note from score containing metadata:");
        print("Length: " + currNote.Length);
        print("Chord:");
        foreach (NoteLocation loc in currNote.Location)
        {
            print(loc);
        }
        print("Type: " + currNote.Type);
        print("Voice: " + currNote.Voice);

        // Spawn obstacle with given note type
        GameObject songNote;
        switch (currNote.Type)
        {
            case (int)NoteType.Rest:
                songNote = null;
                break;
            case (int)NoteType.BallProjectileA:
                songNote = ballProjectileA;
                break;
            case (int)NoteType.BallProjectileB:
                songNote = ballProjectileB;
                break;
            case (int)NoteType.WallObstacle:
                songNote = wallObstacle;
                break;
            default:
                songNote = null;
                print("MusicScoreManager: (Warn) Unknown note type!");
                break;
        }

        // Configure obstacle physics
        if (songNote != null)
        {
            // TODO: move songNote to respective lane based on note loc (chord)
            for (int lane = 0; lane < currNote.Location.Count; ++lane)
            {
                GameObject songNoteSpawn = Instantiate(songNote);

                Util.Move(songNoteSpawn, lanes[lane]);
                Util.SetSpeed(songNoteSpawn.GetComponent<Rigidbody2D>(),
                    GetDifficultyFactor(difficulty) *
                    songNote.GetComponent<Obstacles>().baseSpeed * Vector2.left);
            }
        }

        // relative len = BPM/60 * countedBeat/Note.NoteLength
        return _timeDeltaBeat * timeSignature[1] / currNote.Length;
    }

}

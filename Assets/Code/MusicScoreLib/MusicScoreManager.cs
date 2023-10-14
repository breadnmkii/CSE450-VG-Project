using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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
    // AudioSource
    AudioSource _as;
    public AudioClip songAudio;
    public AudioClip metroUpAudio;
    public AudioClip metroAudio;


    // Public members for song properties (readonly)
    public TextAsset scoreFile;
    public int songDurationMinutes;
    public int songDurationSeconds;
    public int BPM;
    public List<int> timeSignature;
    public Difficulty difficulty;

    // Public lane locations
    public GameObject[] lanes;
    public GameObject collisionChecker;

    // Public obstacle prefabs
    // (Note: must contain Obstacles component)
    public GameObject ballProjectileA;
    public GameObject ballProjectileB;
    public GameObject wallObstacle;

    // Private geometry variables
    private double _spawnToZoneDistance;

    // Private members for defining interal song properties
    private Queue<Note> _musicScore;        // music score containing queue of all notes
    private int _musicNumNotes;             // number of actual notes in score (not counting rests)
    private int _songDurationBeats;         // total number of beats in song
    private int _songStartupBeats;          // number of empty beats prior to starting song
    private bool _songStarted;

    private int _currBeat;                  // counter index to current beat in beat-time
    private double _nowTime;                // var to hold current real-time
    private double _timeSpawnDelay;            // var to hold delay time from spawn to zone of note travel
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

        // Get attached AudioSource
        _as = gameObject.GetComponent<AudioSource>();

        /* Define private members */
        // Beat-time delta time vars
        _currBeat = 0;  // Every song begins on beat 0
        _timeDeltaBeat = (double)60 / BPM;
        _timeSinceLastBeat = 0;
        _timeSinceLastNote = 0;
        _songStartupBeats = 4;

        // Process music score and its properties
        (Queue<Note>, int) musicScoreProcess = ProcessMusicScoreJSON(_songStartupBeats);
        _musicScore = musicScoreProcess.Item1;
        _musicNumNotes = musicScoreProcess.Item2;
        _songDurationBeats = (BPM/60) * (songDurationMinutes*60 + songDurationSeconds);

        // Calculate distance from spawn to note hit zone
        _spawnToZoneDistance = Math.Abs(collisionChecker.transform.position[0] - lanes[0].transform.position[0]);

        /* Post Checks */
        // checking that number of worst case (lowest granularity (sixteenth)) notes
        // can fit within duration
        if (_musicScore.Count > _songDurationBeats * (int)NoteLength.Sixteenth)
        {
            throw new Exception("Cannot fit all of score's notes into song!");
        }

        /* Calculate first upcoming note's spawn delay */
        _timeSpawnDelay = GetSpawnDelay(_musicScore.Peek());

        /* Prepare playing song audio */
        Debug.Log("(MSM) Playing song at difficulty " + difficulty);
        _songStarted = false;
        _as.Stop();
        Debug.Log("WTF IS WRONG WIT THIS");
    }

    private void Update()
    {
        // Play while song is still playing
        if (_currBeat < _songDurationBeats + _songStartupBeats)
        {
            // Beat beat-time loop
            _nowTime = Time.timeSinceLevelLoad;
            if (_nowTime >= _timeSinceLastBeat + _timeDeltaBeat)
            {
                _timeSinceLastBeat += _timeDeltaBeat;
                // Metronome sound
                if (_currBeat < 4)
                {
                    if (_currBeat == 0)
                    {
                        _as.PlayOneShot(metroUpAudio);
                    }
                    else
                    {
                        _as.PlayOneShot(metroAudio);
                    }
                }

                // Song
                if (!_songStarted && _currBeat >= _songStartupBeats)
                {
                    _songStarted = true;
                    _as.PlayOneShot(songAudio);
                    Debug.Log("(MSM) Started music at " + Time.timeSinceLevelLoad);
                }

                if (_currBeat % timeSignature[0] == 0)
                {
                    Debug.Log("(MSM) Measure " + _currBeat / timeSignature[0]);
                }

                // print("(MSM) Beat " + _currBeat + ": " + _nowTime);
                ++_currBeat;
            }
            

            // Note real-time loop
            _nowTime = Time.timeSinceLevelLoad;
            if (_musicScore.Count > 0) {
                if ((_nowTime >= _timeSinceLastNote + _timeDeltaBeat - _timeSpawnDelay) || 
                    (_timeSpawnDelay == -1))
                {
                    _timeSinceLastNote += SpawnNote(_musicScore.Dequeue());
                    if (_musicScore.Count > 1)
                    {
                        // Get upcoming note's spawn delay
                        _timeSpawnDelay = GetSpawnDelay(_musicScore.Peek());
                        if (_timeSpawnDelay != -1)
                        {
                            // Debug info for non-rest notes
                            Debug.Log("(MSM) Upcoming note spawn advance: " + _timeSpawnDelay + " to arrive at: " + (Time.timeSinceLevelLoad + _timeSpawnDelay));
                        }
                    }

                    Debug.Log("(MSM) Spawned: " + _nowTime);

                }
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

    
    // Getter for number of music notes
    public int GetTotalNumMusicNotes()
    {
        return _musicNumNotes;
    }


    // Reads an input JSON file to process into queue of notes
    private (Queue<Note>, int) ProcessMusicScoreJSON(int startupBeats)
    {
        int numRests = startupBeats;

        // Inject startup beats as rests to "spawn"
        Queue<Note> score = new();
        NoteLength countedBeat = (NoteLength)timeSignature[1];
        for (int i=0; i<startupBeats; ++i)
        {
            score.Enqueue(new(countedBeat,
                          new() { },
                          NoteType.Rest));
        }

        // Read CSV music map file
        var lines = scoreFile.text.Split('\n');
        foreach (string line in lines)
        {
            // Do not parse empty or comment lines
            if (line.Length > 1 && !line[0].Equals('#'))
            {
                Debug.Log(line);
                // formatted as: type, length, location0(,...,location3)
                var metadata = line.Split(',');
                NoteType noteType = (NoteType)int.Parse(metadata[0]);
                NoteLength noteLen = (NoteLength)int.Parse(metadata[1]);
                List<NoteLocation> chord = new();
                for (int i = 2; i < metadata.Length; ++i)
                {
                    chord.Add((NoteLocation)int.Parse(metadata[i]));
                }

                Note note = new(noteLen,
                                chord,
                                noteType);
                score.Enqueue(note);

                // Increment number of rests to not count as physical note
                if (noteType == NoteType.Rest)
                {
                    ++numRests;
                }
            }
        }
        Debug.Log("Processed music score!");

        // Do not count rests towards number of actual notes
        int numSongNotes = score.Count - numRests;

        return (score, numSongNotes);
    }


    // Method to spawn a note
    // Returns the amount of delta time to add to timer (based on note length)
    double SpawnNote(Note currNote)
    {
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
                Debug.Log("(MSM - Warn) Unknown note type!");
                break;
        }

        // Configure obstacle physics
        if (songNote != null)
        {
            // For every note in chord
            foreach (NoteLocation loc in currNote.Location)
            {
                // Spawn note
                GameObject songNoteSpawn = Instantiate(songNote);

                // Move to correct lane and set layer
                Util.Move(songNoteSpawn, lanes[(int)loc]);
                songNoteSpawn.layer = MusicNote.GetLayerFromNoteloc(loc);

                // Set speed with difficulty factor
                Util.SetSpeed(songNoteSpawn.GetComponent<Rigidbody2D>(),
                    GetDifficultyFactor(difficulty) *
                    songNote.GetComponent<Obstacles>().baseSpeed * Vector2.left);
            }
        }

        // relative len = BPM/60 * countedBeat/Note.NoteLength
        return _timeDeltaBeat * timeSignature[1] / currNote.Length;
    }


    // Method to calculate the time a Note will take to travel from
    // a lane spawn to collision checker
    double GetSpawnDelay(Note songNote)
    {
        double songNoteSpeed = songNote.Type switch
        {
            (int)NoteType.BallProjectileA => ballProjectileA.GetComponent<Obstacles>().baseSpeed,
            (int)NoteType.BallProjectileB => ballProjectileB.GetComponent<Obstacles>().baseSpeed,
            (int)NoteType.WallObstacle => wallObstacle.GetComponent<Obstacles>().baseSpeed,
            _ => (double)0,
        };

        double noteVelocity = GetDifficultyFactor(difficulty) * songNoteSpeed;
        if (noteVelocity == 0)
        {
            // case for non-speed notes (i.e. rests)
            // we want to return a large early spawn to ensure
            // we get through the rests and calculate the next
            // physical note spawn time in advance to not miss
            return -1;
        }
        return _spawnToZoneDistance / noteVelocity;
    }

    public void pauseSong()
    {
        _as.Pause();
    }

    public void resumeSong()
    {
        _as.UnPause();
    }
}

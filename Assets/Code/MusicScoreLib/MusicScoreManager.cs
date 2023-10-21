using System;
using System.Collections.Generic;
using UnityEngine;

// MusicNote
using MusicNote;

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
    private MusicScore _musicScore;         // music score containing queue of all notes
    private int _musicNumNotes;             // number of actual notes in score (not counting rests)
    private int _songDurationBeats;         // total number of beats in song
    private int _songStartupBeats;          // number of empty beats prior to starting song
    private bool _songStarted;

    private int _currBeat;                  // counter index to current beat in beat-time
    private double _nowTime;                // var to hold current real-time
    private double _timeSpawnDelay;         // var to hold delay time from spawn to zone of note travel
    private double _timeDeltaBeat;          // delta time for beat-time
    private double _timeSinceLastBeat;      // timer for last beat time in beat-time
    private double _timeSinceLastNote;      // timer for last note time in real-time
    private double _songStartTime;          // actual time that the song started playing
    private double _songTime;               // time within the song
    private Tuple<Note, double> _nextNote;  // next note in the note queue

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

        // Process music xml file and level properties to create music score (beatmap)
        Debug.Log("(MSM) Processing music score");
        _musicScore = MSMUtil.ProcessMusicScore(scoreFile, "P1", difficulty);
        Debug.Log("(MSM) Finished processing");
        _songDurationBeats = (BPM/60) * (songDurationMinutes*60 + songDurationSeconds);

        /* Post Checks */
        if (_musicScore.GetNumTotalNotes() > _songDurationBeats * (int)NoteLength.Sixteenth)
        {
            throw new Exception("Cannot fit all of score's notes into song!");
        }

        /* Prepare playing song audio */
        Debug.Log("(MSM) Playing song at difficulty " + difficulty);
        _songStarted = false;
        _as.Stop();
    }

    private void Update()
    {
        /*
        // Play while song is still playing
        /*if (_currBeat < _songDurationBeats + _songStartupBeats)
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
                        // DEPRECATED: this is trash now
                        // _timeSpawnDelay = GetSpawnDelay(_musicScore.Peek());
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
        */

        // Song
        if (!_songStarted)
        {
            _songStarted =true;
            _as.PlayOneShot(songAudio);
            _songStartTime = Time.timeSinceLevelLoad;
            Debug.Log("(MSM) Started music at " + _songStartTime);
        }
        
        // Spawn notes
        else
        {
            _nowTime = Time.timeSinceLevelLoad;
            _songTime = _nowTime - _songStartTime;
            _nextNote = _musicScore.peekNote();

            // Spawn note before it reaches player using the spawn delay
            if (_songTime >= _nextNote.Item2 - MSMUtil.timeFromSpawnToHitzone(_nextNote.Item1, difficulty))
            {
                SpawnNote(_musicScore.readNote().Item1);
                Debug.Log("(MSM) Spawned: " + _nowTime);
            }
        }
    }


    /* Class Methods */
    // Getter for number of music notes
    public int GetTotalNumMusicNotes()
    {
        return _musicNumNotes;
    }

    // Method to spawn a note
    void SpawnNote(Note currNote)
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
                songNoteSpawn.layer = MusicNoteHelper.GetLayerFromNoteloc(loc);

                // Set speed with difficulty factor
                Util.SetSpeed(songNoteSpawn.GetComponent<Rigidbody2D>(),
                    MSMUtil.GetDifficultyFactor(difficulty) *
                    songNote.GetComponent<Obstacles>().baseSpeed * Vector2.left);
            }
        }
    }
}

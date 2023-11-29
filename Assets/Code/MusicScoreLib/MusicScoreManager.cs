using System;
using System.Collections.Generic;
using UnityEngine;

// MusicNote
using MusicNote;
using System.Collections;
using UnityEditor.Experimental.GraphView;
using TMPro;

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
    public int LevelIndex;
    public AudioClip songAudio;
    public AudioClip metroUpAudio;
    public AudioClip metroAudio;


    // Public members for song properties (readonly)
    public TextAsset scoreFile;
    public double BPM;
    public static Difficulty difficulty = Difficulty.concert;   // Default concert difficulty
    // DIFFICULTY ON TIME OFFSET DIFFERENCE
    // protege --> 2    (0)
    // concert --> 0.9  (-1.1)
    // virtuoso --> ??
    // prodigy --> ????

    // Public lane locations
    public GameObject[] lanes;
    public GameObject collisionChecker;

    // Public obstacle prefabs
    // (Note: must contain Obstacles component)
    public GameObject ballProjectileA;
    public GameObject ballProjectileB;
    public GameObject[] wallObstacles;
    public GameObject boss;

    public double TOTALLY_PROGRAMMATIC_NOT_HARDCODED_NOTE_SPAWN_offset;

    // Private members for defining interal song properties
    private MusicScore _musicScore;         // music score containing queue of all notes
    private int _currStartUpBeat;           // counter of the current start up beat
    private int _songStartupBeats;          // number of empty beats prior to starting song
    private bool _gameStarted;              // flag indicating whether or not the game's level has begun
    private bool _musicStarted;             // flag indicating whether or not the actual audio song has started
    //private bool _finalAttackSpawned;       // flag indicating whether or not the final attack has been spawned
    private bool _gameWon;                  // flag indicating whether or not the player has won

    private double _nowTime;                    // var to hold current real-time
    private double _timeDeltaStartUpBeat;       // delta time for start-up beats
    private double _timeSinceLastStartUpBeat;   // timer for last start-up beat in beat-time
    private double _songStartTime;              // actual time that the song started playing
    private double _songTime;                   // time within the song
    private Tuple<Note, double> _nextNote;      // next note in the note queue

    public int GetTotalNotes()
    {
        return _musicScore.GetNumTotalNotes();
    }

    public int GetRemainingNotes()
    {
        return _musicScore.GetNumRemainingNotes();
    }

    /* Unity Loop Methods */
    private void Start()
    {
        // Pre checks
        if (BPM <= 0)
        {
            throw new Exception("BPM must be a positive number!");
        }

        // Get attached AudioSource
        _as = gameObject.GetComponent<AudioSource>();

        /* Define private members */
        _currStartUpBeat = 0;
        _songStartupBeats = 4;
        _timeSinceLastStartUpBeat = 0;
        _timeDeltaStartUpBeat = 60 / BPM;
        //_finalAttackSpawned = false;
        _gameWon = false;

        // Process music xml file and level properties to create music score (beatmap)
        Debug.Log("(MSM) Processing music score");
        _musicScore = MSMUtil.ProcessMusicScore(scoreFile, "P1", difficulty);
        Debug.Log("(MSM) Finished processing");

        /* Prepare playing level */
        Debug.Log("(MSM) Playing song at difficulty " + difficulty);


        _gameStarted = false;
        _musicStarted = false;
        _as.Stop();
    }

    private void Update()
    {
        // Play the start up audio for the first 4 beats
        _nowTime = Time.timeSinceLevelLoad;

        // Verify we are still in the start up phase to play metronome
        if (_currStartUpBeat < _songStartupBeats)
        {
            // Only play the start up metronome beats in intervals of 60/BPM
            if (_nowTime >= _timeSinceLastStartUpBeat + _timeDeltaStartUpBeat)
            {
                // Play the metronone sound
                if (_currStartUpBeat == 0)
                {
                    _as.PlayOneShot(metroUpAudio);
                }
                else
                {
                    _as.PlayOneShot(metroAudio);
                }

                _timeSinceLastStartUpBeat += _timeDeltaStartUpBeat;
                _currStartUpBeat++;
            }
        }
        else
        {
            // Begin with actual song start time (reset origintime to post startup beats time)
            if (!_gameStarted)
            {
                _gameStarted = true;
                _songStartTime = Time.timeSinceLevelLoad;
                Debug.Log("(MSM) Started game. Resetting origin time to " + _songStartTime);
            }
            else
            {
                // Compute song time
                _nowTime = Time.timeSinceLevelLoad;
                _songTime = _nowTime - _songStartTime;

                // Delay oneshot audio by PROGRAMMATIC_OFFSET delay to "shift"
                // song backwards to account for constant note travel delay

                double spawnDelay = TOTALLY_PROGRAMMATIC_NOT_HARDCODED_NOTE_SPAWN_offset;

                spawnDelay /= Math.Pow(2,MSMUtil.GetDifficultyFactor(difficulty)); // scale offset by difficuly factor

                if (!_musicStarted && _songTime >= spawnDelay)
                {
                    _as.PlayOneShot(songAudio);
                    _musicStarted = true;
                    Debug.Log("(MSM) Started music at " + _songTime);
                }

                // Spawn notes
                _nextNote = _musicScore.peekNote();

                if (_nextNote != null)
                {
                    double actualSpawnTime = _nextNote.Item2; //- avgDelayOffset;

                    //double advanceSpawnTime = MSMUtil.TimeForNoteToTravelDistance(_nextNote.Item1,
                    //                                                                difficulty,
                    //                                                                _spawnToZoneDistance);


                    // If Rest note, remove immediately from queue (to see next real note)
                    //if (_nextNote.Item1.Type == NoteType.Rest || _nextNote.Item1.isTied)
                    //{
                    //    // Debug.Log("(MSM) Removed rest note");
                    //    _musicScore.readNote();
                    //}

                    // Spawn based on note actual spawn time
                    if (_songTime >= actualSpawnTime) // - advanceSpawnTime)
                    {
                        // Do this check here to always dequeue the next note even if it should not be 
                        SpawnNote(_nextNote.Item1);

                        Debug.Log("(MSM) Note with demanded spawn time: " + _nextNote.Item2
                                + " actually spawned at: " + _songTime);

                        // Advance noteQueue
                        _musicScore.readNote();
                    }
                }

                // next note is null, so the song is over
                else if (!_gameWon)
                {
                    _gameWon = true;
                    if (DidPassLevel())
                    {
                        //Debug.Log("YAYY YOU PASSED LEVEL");
                        boss.GetComponent<Animator>().SetTrigger("dead");
                    }
                    //Debug.Log("Begin onWin() coroutine...");
                    StartCoroutine(onWin());
                }
                //else if (!_finalAttackSpawned)
                //{
                //    SpawnFinalAttack();
                //    _finalAttackSpawned = true;
                //}

            }

                
        }
    }


    /* Class Methods */
    // Getter for number of music notes
    public int GetTotalNumMusicNotes()
    {
        return _musicScore.GetNumTotalNotes();
    }

    // Method to determine if the player passed level or not
    public bool DidPassLevel()
    {
        double passingGradePercentInCSE450 = 0.7;
        Debug.Log("Did pass score: ");
        Debug.Log(Player.instance.GetScore() / (GetTotalNotes() * Player.instance.greatHitReward));
        return (Player.instance.GetScore() / (GetTotalNotes() * Player.instance.greatHitReward)) > passingGradePercentInCSE450;
    }

    // Method to spawn a note
    void SpawnNote(Note currNote)
    {
        // Spawn obstacle with given note type
        GameObject songNote;
        switch (currNote.Type)
        {
            case NoteType.Rest:
                // Determine random wall projectile sprite object to spawn
                songNote = wallObstacles[UnityEngine.Random.Range(0, 4)];
                break;
            case NoteType.BallProjectileA:
                songNote = ballProjectileA;
                break;
            case NoteType.BallProjectileB:
                songNote = ballProjectileB;
                break;
            default:
                songNote = null;
                Debug.Log("(MSM - Warn) Reading NULL note (is it an ignored rest?)!");
                break;
        }

        // Configure obstacle physics AND Do not spawn rest powerups on harder difficulties
        if (songNote != null && !(currNote.Type == NoteType.Rest && difficulty > Difficulty.concert))
        {
            // For every note in chord
            foreach (NoteLocation loc in currNote.Location)
            {
                int noteLocation = (int)loc;
                // Spawn note
                GameObject songNoteSpawn = Instantiate(songNote);

                if (currNote.Type != NoteType.Rest)
                {
                    songNoteSpawn.GetComponent<Animator>().SetTrigger("start");
                }

                // Move to correct lane and set layer, randomize rest note location
                if (currNote.Type == NoteType.Rest)
                {
                    noteLocation = UnityEngine.Random.Range(0, 4);
                }
                Util.Move(songNoteSpawn, lanes[noteLocation]);
                songNoteSpawn.layer = MusicNoteHelper.GetLayerFromNoteloc((NoteLocation)noteLocation);

                // Set speed with difficulty factor
                Util.SetSpeed(songNoteSpawn.GetComponent<Rigidbody2D>(),
                    MSMUtil.GetDifficultyFactor(difficulty) *
                    songNote.GetComponent<Obstacles>().baseSpeed * Vector2.left);
            }
        }
    }

    // Spawn the final attack in each lane
    //public void SpawnFinalAttack()
    //{
    //    for (int i = 0; i < 4; i++)
    //    {
    //        GameObject attack = Instantiate(finalAttack);
    //        Util.Move(attack, lanes[i]);
    //        attack.layer = MusicNoteHelper.GetLayerFromNoteloc((NoteLocation)i);
    //    }
    //}

    public void pauseSong()
    {
        AudioListener.pause = true;
    }

    public void resumeSong()
    {
        AudioListener.pause = false;
    }

    // Show win screen
    IEnumerator onWin()
    {
        yield return new WaitForSeconds(3);

        // update high score if necessary
        int currentHighScore = Util.GetHighScore(LevelIndex);
        if (Player.instance.GetScore() > currentHighScore)
        {
            Util.SetHighScore(LevelIndex, (int)Player.instance.GetScore());
        }

        // calculate stats
        int totalNotes = GetTotalNotes();
        int goodHits = Player.instance.numGoodHits;
        int greatHits = Player.instance.numGreatHits;
        int perfectHits = Player.instance.numPerfectHits;
        int totalNotesHit = goodHits + greatHits + perfectHits;
        int missedNotes = totalNotes - totalNotesHit;
        int bonusPoints = perfectHits * ((int)Player.instance.perfHitReward - (int)Player.instance.greatHitReward);

        // clear UI elements
        BossHPBar.instance.gameObject.SetActive(false);
        Destroy(Player.instance.scoreUI);

        // display stats
        Player.instance.WinUI.SetActive(true);
        Player.instance.WinUI.transform.GetChild(1).GetChild(1).GetChild(0).GetChild(0).GetComponent<TMP_Text>().text = Player.instance.GetScore().ToString();
        Player.instance.WinUI.transform.GetChild(1).GetChild(1).GetChild(1).GetChild(0).GetComponent<TMP_Text>().text = totalNotesHit + " / " + totalNotes;
        Player.instance.WinUI.transform.GetChild(1).GetChild(1).GetChild(2).GetChild(0).GetComponent<TMP_Text>().text = missedNotes + " / " + totalNotes;
        Player.instance.WinUI.transform.GetChild(1).GetChild(1).GetChild(3).GetChild(0).GetComponent<TMP_Text>().text = bonusPoints.ToString();
        Player.instance.WinUI.transform.GetChild(1).GetChild(1).GetChild(4).GetChild(0).GetComponent<TMP_Text>().text = Player.instance.numNotesDamaged.ToString() + " Notes";

        // display grade
        double maxScore = GetTotalNotes() * Player.instance.greatHitReward;
        double myScore = Player.instance.GetScore();
        double grade = myScore / maxScore;
        Player.instance.WinUI.transform.GetChild(1).GetChild(3).GetComponent<TMP_Text>().text = "Grade: " + getLetterGrade(grade);
    }

    // calculate grade
    private String getLetterGrade(double grade)
    {
        String retGrade = "";

        if (grade < 0.6)
        {
            retGrade = "F";
        }
        else if (grade < 0.7)
        {
            retGrade = "D";
        }
        else if (grade < 0.8)
        {
            retGrade = "C";
        }
        else if (grade < 0.9)
        {
            retGrade = "B";
        }
        else
        {
            retGrade = "A";
        }

        return retGrade;
    }
}

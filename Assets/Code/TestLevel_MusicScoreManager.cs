using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestLevel_MusicScoreManager : MonoBehaviour
{

    // Outlets
    MusicScoreManager _msm;

    // Configuration
    public List<int> levelTimeSignature;
    public int levelBPM;
    public int levelSongDurationMins;
    public SortedDictionary<double, Note> levelMusicScore;
    public Difficulty levelDifficulty;

    // Start is called before the first frame update
    void Start()
    {
        // Read some file (JSON?) to populate music score
        // DEBUG: hardcoded music score for now

        Note note_1 = new Note(NoteLength.Quarter,
                               NoteLocation.Lane1,
                               NoteVoice.Bass,
                               NoteType.BlockObstacle);
        Note note_2 = new Note(NoteLength.Quarter,
                               NoteLocation.Lane2,
                               NoteVoice.Bass,
                               NoteType.BlockObstacle);
        Note note_3 = new Note(NoteLength.Quarter,
                               NoteLocation.Lane3,
                               NoteVoice.Bass,
                               NoteType.BlockObstacle);
        Note note_4 = new Note(NoteLength.Quarter,
                               NoteLocation.Lane4,
                               NoteVoice.Bass,
                               NoteType.BlockObstacle);

        // TODO(bzyang): figure out how to build the music score
        // with the correct number of beats/notes/positions.

        // TODO(bzyang): then spawn the notes based on projectiles
        // yonghao made into the scene with the methods of the
        // music score manager


        // Construct score manager
        _msm = new MusicScoreManager(levelTimeSignature,
                                     levelBPM,
                                     levelSongDurationMins,
                                     levelMusicScore,
                                     levelDifficulty);       
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

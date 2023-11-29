using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CarouselCardController : MonoBehaviour
{
    private string[] difficulties={"Easy", "Medium", "Hard", "Hell"};
    private int difficulty;
    // Start is called before the first frame update

    public TMP_Text difficulty_Text;
    public TMP_Text highScoreText;
    public int LevelIndex;

    void Start(){
        difficulty = 0;
        highScoreText.text = "High Score: " + GetHighScore(LevelIndex).ToString();
    }

    public void changeDifficulty(int n){

        difficulty = (difficulty+n+4) % 4;

        difficulty_Text.text = difficulties[difficulty];
        MusicScoreManager.difficulty = (Difficulty)difficulty;
    }

    public void startSong(string song){
        Util.LoadScene(song);
    }

    private int GetHighScore(int levelIndex)
    {
        switch (levelIndex)
        {
            case 0:
                return PlayerPrefs.GetInt("HighScoreBUGBUG");
            case 1:
                return PlayerPrefs.GetInt("HighScoreCATCAT");
            case 2:
                return PlayerPrefs.GetInt("HighScoreCOCO");
            case 3:
                return PlayerPrefs.GetInt("HighScoreGASGAS");
            case 4:
                return PlayerPrefs.GetInt("HighScoreJOJO");
            case 5:
                return PlayerPrefs.GetInt("HighScoreSHELTSHELT");
            default:
                return 0;
        }
    }
}

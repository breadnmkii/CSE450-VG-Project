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

    void Start(){
        difficulty = 0;
    }

    public void changeDifficulty(int n){

        difficulty = (difficulty+n+4) % 4;

        difficulty_Text.text = difficulties[difficulty];
        MusicScoreManager.difficulty = (Difficulty)difficulty;
    }

    public void startSong(string song){
        Util.LoadScene(song);
    }
}

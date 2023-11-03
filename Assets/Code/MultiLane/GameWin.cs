using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameWin : MonoBehaviour
{
    //private RawImage img;

    public int curLevel = 0;

    public GameObject ButtonRestart;

    public GameObject ButtonNextL;

    // Start is called before the first frame update
    void Start()
    {
        //img = GetComponent<RawImage>();
    }

    void OnEnable()
    {
        Time.timeScale = 0;
        if (curLevel != Util.Levels.Length - 1)
        {
            ButtonRestart.SetActive(false);
            ButtonNextL.SetActive(true);
        }
        else
        {
            ButtonRestart.SetActive(true);
            ButtonNextL.SetActive(false);
        }
    }

    public void restart()
    {
        Time.timeScale = 1;
        Util.LoadScene(0);
    }

    public void QuitGame()
    {
        Application.Quit();

    }

    public void NextLevel()
    {
        curLevel++;
        Util.LoadScene(curLevel);
    }

    public void enterDino()
    {
        Util.LoadScene(1);
    }
}

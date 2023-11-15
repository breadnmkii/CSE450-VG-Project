using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameWin : MonoBehaviour
{
    //private RawImage img;

    private int curLevel;

    public GameObject ButtonRestart;

    public GameObject ButtonNextL;

    // Start is called before the first frame update
    void Start()
    {
        //img = GetComponent<RawImage>();
        curLevel = SceneManager.GetActiveScene().buildIndex;
    }

    void OnEnable()
    {
        Time.timeScale = 0;
        if (curLevel != Util.Levels.Length - 1)
        {
            
            ButtonNextL.SetActive(true);
        }
        else
        {
            ButtonNextL.SetActive(false);
        }
        ButtonRestart.SetActive(true);
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

    public void toLevel(int i)
    {
        Util.LoadScene(i);
    }

    public void enterDino()
    {
        Util.LoadScene(1);
    }
}

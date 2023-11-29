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

    public GameObject ButtonHome;

    // Start is called before the first frame update
    void Start()
    {
        //img = GetComponent<RawImage>();
        curLevel = SceneManager.GetActiveScene().buildIndex;
    }

    void OnEnable()
    {
        Time.timeScale = 0;
        ButtonHome.SetActive(true);
        ButtonRestart.SetActive(true);
    }

    public void restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Time.timeScale = 1;
        Util.LoadScene("Scenes/mainmenu");
    }

    public void enterDino()
    {
        Util.LoadScene(3);
    }
}

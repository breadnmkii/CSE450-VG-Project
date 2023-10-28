using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameWin : MonoBehaviour
{
    private RawImage img;

    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<RawImage>();

    }

    void OnEnable()
    {
        Time.timeScale = 0;
    }

    public void restart()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("Scenes/StartScene");
    }

    public void QuitGame()
    {
        Application.Quit();

    }

    public void enterDino()
    {
        SceneManager.LoadScene("Scenes/DinoBasic");
    }
}

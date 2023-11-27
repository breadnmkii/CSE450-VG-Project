using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UIController : MonoBehaviour
{
    public GameObject Level;
    public GameObject MainMenu;
    public GameObject Difficulty;
    public GameObject Options;

    // Start is called before the first frame update
    void Start()
    {
        ShowMenu();
    }

    public void ShowMenu()
    {
        MainMenu.SetActive(false);
        Difficulty.SetActive(false);
        Level.SetActive(false);
        Options.SetActive(false);

        MainMenu.SetActive(true);
    }
    public void ShowLevel()
    {
        MainMenu.SetActive(false);
        Difficulty.SetActive(false);
        Level.SetActive(false);
        Options.SetActive(false);

        Level.SetActive(true);
    }
    public void ShowDifficulty()
    {
        MainMenu.SetActive(false);
        Difficulty.SetActive(false);
        Level.SetActive(false);
        Options.SetActive(false);

        Difficulty.SetActive(true);
    }

    public void ShowOpt()
    {
        MainMenu.SetActive(false);
        Difficulty.SetActive(false);
        Level.SetActive(false);
        Options.SetActive(false);

        Options.SetActive(true);
    }

    public void setDifficulty(int tar_difficulty)
    {
        MusicScoreManager.difficulty = (Difficulty)tar_difficulty;
        ShowMenu();
        Debug.Log("Set difficulty!");
    }

    public void test()
    {
        Debug.Log("Pressed");
    }

    public void ToScene(int index)
    {
        Util.LoadScene(index);
    }
}

using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Search;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class MenuAndStoryControl : MonoBehaviour
{

    public GameObject[] Pages;
    public GameObject Level;
    public GameObject MainMenu;
    public GameObject Difficulty;
    public GameObject backGround;
    public GameObject title;
    public TMP_Text play;
    bool storyEnds;
    int index;
    string[] playLine = {"Go!", "Run!", "Escape!" };
    int difficulty;

    // Start is called before the first frame update
    void Start()
    {
        index = 0;
        storyEnds = false;
        Show();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Show()
    {
        for (int i = 0; i < Pages.Length; i++)
        {
            Pages[i].SetActive(false);
        }
        MainMenu.SetActive(false);
        Difficulty.SetActive(false);
        Level.SetActive(false);

        Pages[index].SetActive(true);
    }

    public void showNextPage()
    {
        Pages[index].SetActive(false);
        index++;
        Pages[index].SetActive(true);
    }

    public void ShowMenu()
    {
        if (!storyEnds)
        {
            Pages[index].SetActive(false);
            backGround.SetActive(true);
            title.SetActive(true);
        }
        

        MainMenu.SetActive(false);
        Difficulty.SetActive(false);
        Level.SetActive(false);

        MainMenu.SetActive(true);
    }
    public void ShowLevel()
    {
        MainMenu.SetActive(false);
        Difficulty.SetActive(false);
        Level.SetActive(false);

        Level.SetActive(true);
    }
    public void ShowDifficulty()
    {
        MainMenu.SetActive(false);
        Difficulty.SetActive(false);
        Level.SetActive(false);

        Difficulty.SetActive(true);
    }

    public void setDifficulty(int tar_difficulty)
    {
        difficulty = tar_difficulty;
        play.text = playLine[difficulty];
        ShowMenu();
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

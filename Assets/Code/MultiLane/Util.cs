using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


// This is the class holding every util method for other scripts to use.
// Make sure methods here are for general purpose that might be needed for plural scripts.
public class Util
{

    public static string[] Levels = { "Scenes/mainmenu", "Scenes/StartScene", "Scenes/Carousel", "Scenes/DinoBasic", "Scenes/Tutorial", "Scenes/Level_JOJO", "Scenes/Level_GASGAS", "Scenes/Level_CATCAT", "Scenes/Level_SHELTSHELT", "Scenes/Level_COCO", "Scenes/TransitionScene", };
    // Usage: To create a delay in execution some codes.
    // Input: time -- The delay time (in seconds);
    //        action -- Actual codes/methods hope to be executed after the delay.
    public static IEnumerator WaitForSec(float time, Action action = null)
    {
        yield return new WaitForSeconds(time);
        action?.Invoke();
    }

    // Usage:To change an obj's position from one to another.
    // Input: obj -- Gameobject to be moved or to change the collision layer;
    //        target -- Gameobject holding the target position info.
    //        layer -- The name of the target collision layer.
    // Todo: Modify to become more generalized.
    public static void Move(GameObject obj, GameObject target, string layer = null)
    {
        obj.transform.position = target.transform.position;
        if (layer != null)
        {
            obj.layer = LayerMask.NameToLayer(layer);
        }
        return;
    }

    // Usage:To change an obj's velocity. (Rigidbody 2D required)
    // Input: rb -- Target rigidbody2D
    //        speed -- Vector2 object representing the target speed;
    public static void SetSpeed(Rigidbody2D rb, Vector2 speed)
    {
        rb.velocity = speed;
    }

    // Usage:To change an obj's layer from one to another.
    // Input: tar -- Gameobject to change the collision layer;
    //        layername -- The name of the target collision layer.
    public static void SetLayer(GameObject tar, string layername)
    {
        tar.layer = LayerMask.NameToLayer(layername);
    }


    public static void LoadScene(string name)
    {
        SceneManager.LoadScene(name);
    }

    public static void LoadScene(int id)
    {
        SceneManager.LoadScene(Levels[id]);
    }

    public static int GetHighScore(int levelIndex)
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

    public static void SetHighScore(int levelIndex, int score)
    {
        switch (levelIndex)
        {
            case 0:
                PlayerPrefs.SetInt("HighScoreBUGBUG", score);
                break;
            case 1:
                PlayerPrefs.SetInt("HighScoreCATCAT", score);
                break;
            case 2:
                PlayerPrefs.SetInt("HighScoreCOCO", score);
                break;
            case 3:
                PlayerPrefs.SetInt("HighScoreGASGAS", score);
                break;
            case 4:
                PlayerPrefs.SetInt("HighScoreJOJO", score);
                break;
            case 5:
                PlayerPrefs.SetInt("HighScoreSHELTSHELT", score);
                break;
            default:
                break;
        }
    }
}

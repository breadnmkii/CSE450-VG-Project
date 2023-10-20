using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseUi : MonoBehaviour
{

    bool isPausing;

    public MusicScoreManager musicManger;

    public Player player;

    public ObstacleChecker checker;

    private int operationType;

    public TextMeshProUGUI opText;

    public void Start()
    {
        operationType = 0;
        isPausing = false;
        //gameObject.GetComponent<Canvas>().enabled = false;
        //gameObject.GetComponent<GraphicRaycaster>().enabled = false;
        transform.Find("BackGround").gameObject.SetActive(false);
        transform.Find("L1").gameObject.SetActive(false);
        transform.Find("L2").gameObject.SetActive(false);
    }
    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !isPausing)
        {
            transform.Find("BackGround").gameObject.SetActive(true);

            //gameObject.GetComponent<Canvas>().enabled = true;
            //gameObject.GetComponent<GraphicRaycaster>().enabled = true;

            transform.Find("L1").gameObject.SetActive(true);
            transform.Find("L2").gameObject.SetActive(false);
           
            if (musicManger != null)
            {
                musicManger.pauseSong();
            }

            Time.timeScale = 0f;
            Debug.Log("paused");
            isPausing = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && isPausing)
        {
            transform.Find("BackGround").gameObject.SetActive(false);

            //gameObject.GetComponent<Canvas>().enabled = false;
            //gameObject.GetComponent<GraphicRaycaster>().enabled = false;
            transform.Find("L1").gameObject.SetActive(false);
            transform.Find("L2").gameObject.SetActive(false);
            isPausing = false;
            if (musicManger != null)
            {
                musicManger.resumeSong();
            }

            Time.timeScale = 1f;
        }
    }

    public void onClickResume()
    {

        transform.Find("BackGround").gameObject.SetActive(false);
        transform.Find("L1").gameObject.SetActive(false);
        //gameObject.GetComponent<Canvas>().enabled = false;
        //gameObject.GetComponent<GraphicRaycaster>().enabled = false;
        if (musicManger != null)
        {
            musicManger.resumeSong();
        }
        Time.timeScale = 1f;
        isPausing = false;
    }

    public void onClickSettings()
    {

        transform.Find("L2").gameObject.SetActive(true);
        transform.Find("L1").gameObject.SetActive(false);
        //----------------------------Following is temporary-----------------
        if( player == null || checker == null )
        {
            opText.text = "----";
        }
    }

    public void onClickBack()
    {

        transform.Find("L1").gameObject.SetActive(true);
        transform.Find("L2").gameObject.SetActive(false);
    }

    public void onClickOperationType()
    {
        if (player == null || checker == null)
        {
            return;
        }
        operationType = 1 - operationType;
        player.setOperationType(operationType);
        checker.setOperationType(operationType);
        if (operationType == 0)
        {
            opText.text = "Right-handed";
        }
        else if (operationType == 1)
        {
            opText.text = "Left-handed";
        }
        
    }

    public void onClickRestart()
    {

        Time.timeScale = 1f;
        SceneManager.LoadScene("Scenes/DinoBasic");
    }
}

using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine.UI;
using UnityEngine;


// Basic manager for multilane scene.
// It controls the lane changing part and the correlated animation.
// Not involving actuall character behaviors.
// Todo: figure out whether need to do the rhythm check part in this class or not. Need to discuss detailed control.

public class HPBar: MonoBehaviour
{
    public Player player;

    public GameObject[] plots;

    bool init;

    int maxHP;

    int curHP;

    private void Start()
    {
        init = false;
    }

    private void Update()
    {
        if (!init)
        {
            maxHP = player.MaxHP;
            curHP = player.getHP();
            initialize();
            init = true;
        }
        curHP = player.getHP();
        for (int i = 0; i < curHP; ++i)
        {
            plots[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
        for (int i = curHP; i < maxHP; ++i)
        {
            plots[i].GetComponent<SpriteRenderer>().color = Color.black;
        }

    }

    void initialize()
    {
        for (int i = 0; i < maxHP; i++)
        {
            plots[i].SetActive(true);
            plots[i].GetComponent<SpriteRenderer>().color = Color.white;
        }
        for (int i = maxHP; i < plots.Length; i++)
        {
            plots[i].SetActive(false);
        }
    }
}

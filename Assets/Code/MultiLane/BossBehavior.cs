using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{

    private int HPMax;
    private int HPCur;
    // TODO: fix hardcoded animation interval vars
    private int BPM = 135;
    private int atkAnimInterval = 8;
    private int currBeat = 0;

    private bool dead;

    // Outlets
    public GameObject WinUI;
    public MusicScoreManager LevelMusicManager;  // music score manager

    Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        HPMax = LevelMusicManager.GetTotalNumMusicNotes();

        HPCur = HPMax;
        dead = false;
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: correct hardcoded BPM and add death animation
        // Gives current number of beats
        currBeat = Mathf.FloorToInt((Time.timeSinceLevelLoad * (BPM/60)));
        if (currBeat % atkAnimInterval == 0) {
            animator.SetTrigger("atk");
        }
        // if (dead)
        // {
        //     animator.SetTrigger("dead");
        // }
    }
    public void doDamage(int damage)
    {
        if (!dead)
        {
            HPCur -= damage;
            dead = HPCur <= 0;
        }
    }

    public int GetMaxHP()
    {
        return HPMax;
    }

    public int getHP()
    {
        return HPCur;
    }
}

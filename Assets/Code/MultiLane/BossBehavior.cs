using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{

    private int HPMax;
    private int HPCur;
    private bool dead;
    private SpriteRenderer sprite;
    private Animator anim;

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
        // Gives current number of beats
        // currBeat = Time.timeSinceLevelLoad() * (BPM/60);
        // if (currBeat % atkAnimInterval == 0) {
        //     animator.SetTrigger("atk");
        // }
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
        anim.SetBool("dead", dead);
    }

    IEnumerator deadAnim()
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.down * 2f;
        yield return new WaitForSeconds(1f);
        WinUI.SetActive(true);
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBehavior : MonoBehaviour
{

    public int HPMax;
    private int HPCur;
    private bool dead;
    private SpriteRenderer sprite;
    private Animator anim;
    public GameObject WinUI;
    // Start is called before the first frame update
    void Start()
    {
        HPCur = HPMax;
        dead = false;
        sprite = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (dead)
        {
            onDead();
            
        }
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

    private void onDead()
    {
        sprite.flipY = true;
        sprite.color = Color.red;
        StartCoroutine(deadAnim());
    }

    IEnumerator deadAnim()
    {
        gameObject.GetComponent<Rigidbody2D>().velocity = Vector2.down * 2f;
        yield return new WaitForSeconds(1f);
        WinUI.SetActive(true);
    }

    public int getHP()
    {
        return HPCur;
    }
}
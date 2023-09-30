using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorPlayer : MonoBehaviour
{

    private Animator myAnimator;
    public bool isGround;
    public bool dead;
    // Start is called before the first frame update
    void Start()
    {
        dead = false;
        isGround = true;
        myAnimator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        myAnimator.SetFloat("HorizontalVelocity", gameObject.GetComponent<Rigidbody2D>().velocity.y);
        myAnimator.SetBool("isGround", isGround);
        myAnimator.SetBool("Dead", dead);
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            StartCoroutine(jumptest());
        }
        if (Input.GetKeyDown(KeyCode.J))
        {
            dead = true;
        }
    }

    IEnumerator jumptest()
    {
        isGround = false;
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 1);
        yield return new WaitForSeconds(1f);
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, -1);
        yield return new WaitForSeconds(1f);
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        isGround = true;
    }
}

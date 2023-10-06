using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using UnityEngine;


// Basic manager for multilane scene.
// It controls the lane changing part and the correlated animation.
// Not involving actuall character behaviors.
// Todo: figure out whether need to do the rhythm check part in this class or not. Need to discuss detailed control.

public class Player : MonoBehaviour
{
    // Outlets
    Rigidbody2D _rb;

    // A tmp Character to solve possible animation problem.
    // public GameObject CharacterShadow;
    // List of lane pivot point, to hold the positions for player to move.
    public List<GameObject> Lanes;
    // duration between shadow and character animation.
    public float duration;
    // Max HP
    public int MaxHP;
    // Health point
    private int HP;

    // number of jumps left for player
    public int jumpsLeft;

    // index of lane that the player is on.
    private int lane_No;

    // Gameover UI Object
    public GameObject DeadUI;

    // Player's animator
    private Animator myAni;

    //Initialize
    private void Start()
    {
        lane_No = 0;
        HP = MaxHP;
        myAni = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        myAni.SetBool("isGround", jumpsLeft > 0);
        myAni.SetFloat("HorizontalVelocity", _rb.velocity.y);
        if ( this.HP <= 0 )
        {
            myAni.SetBool("Dead", true);
        }

        // Go up lane
        if (Input.GetKeyDown(KeyCode.UpArrow) && lane_No < 3)
        {
            DisableLane(lane_No);
            lane_No += 1;
            EnableLane(lane_No);
            Util.Move(gameObject, Lanes[lane_No].transform.GetChild(1).gameObject, "Lane" + lane_No.ToString());
            /*StartCoroutine(Util.WaitForSec(duration, () =>
            {
                Util.Move(CharacterShadow, gameObject);
            }));*/
            // Debug.Log(lane_No);
        }

        // Go down lane
        if (Input.GetKeyDown(KeyCode.DownArrow) && lane_No > 0)
        {
            DisableLane(lane_No);
            lane_No -= 1;
            EnableLane(lane_No);
            Util.Move(gameObject, Lanes[lane_No].transform.GetChild(1).gameObject, "Lane" + lane_No.ToString());
            /*StartCoroutine(Util.WaitForSec(duration, () =>
            {
                Util.Move(CharacterShadow, gameObject);
            }));*/
            // Debug.Log(lane_No);
        }

        // Jump
        if (Input.GetKey(KeyCode.Space) ||
            Input.GetKey(KeyCode.W))
        {
            if (jumpsLeft > 0)
            {
                jumpsLeft--;
                _rb.AddForce(Vector2.up * 25f, ForceMode2D.Impulse);
            }
        }

        // Slam to ground
        if (Input.GetKeyDown(KeyCode.S))
        {
            Util.Move(gameObject, Lanes[lane_No].transform.GetChild(1).gameObject, "Lane" + lane_No.ToString());
        }
    }


    // HP Getter
    public int getHP()
    {
        return this.HP;
    }


    // Add current HP by integer n.
    // n range [-Inf, Inf].
    // Constraint current HP within range [0, MaxHP] 
    public void ModifyHP(int n)
    {
        this.HP += n;
        if (HP < 0)
        {
            HP = 0;
        }
        if (HP > MaxHP)
        {
            HP = MaxHP;
        }
    }

    // check for collisions
    void OnCollisionStay2D(Collision2D other)
    {
        // Check that we collided with Ground
        if (other.gameObject.layer == LayerMask.NameToLayer("LaneGround"))
        {
            // Check what is directly below our character's feet
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, 1.1f );

            // Debug.DrawRay(transform.position, Vector2.down * 1.1f);

            // We might have multiple things below character's feet
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];

                // Check that we collided with ground below our feet
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("LaneGround"))
                {
                    // Reset jump count
                    jumpsLeft = 1;
                }
            }
        }
    }

    // Disable collision for a lane
    private void DisableLane(int laneNo)
    {
        Lanes[laneNo].transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;
    }

    // Enable collision for a lane
    private void EnableLane(int laneNo)
    {
        Lanes[laneNo].transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = true;
    }

    // Death effects and UI Call
    public void onDead()
    {
        // Destroy(CharacterShadow);
        DeadUI.SetActive(true); // does setting this active call on enable for gameover.cs?
    }
}

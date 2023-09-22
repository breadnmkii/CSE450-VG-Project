using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


// Basic manager for multilane scene.
// It controls the lane changing part and the correlated animation.
// Not involving actuall character behaviors.
// Todo: figure out whether need to do the rhythm check part in this class or not. Need to discuss detailed control.

public class Player : MonoBehaviour
{

    // A tmp Character to solve possible animation problem.
    public GameObject CharacterShadow;
    // List of lane pivot point, to hold the positions for player to move.
    public List<GameObject> Lanes;
    // duration between shadow and character animation.
    public float duration;
    // Max HP
    public int MaxHP;
    // Health point
    private int HP;

    // index of lane that the player is on.
    private int lane_No;

    //Initialize
    private void Start()
    {
        lane_No = 0;
        HP = MaxHP;
    }

    private void Update()
    {
        if ( this.HP <= 0 )
        {
            Destroy(CharacterShadow);
            Destroy(gameObject);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow) && lane_No < 3)
        {
            lane_No += 1;
            Util.Move(gameObject, Lanes[lane_No], "Lane" + lane_No.ToString());
            StartCoroutine(Util.WaitForSec(duration, () =>
            {
                Util.Move(CharacterShadow, gameObject);
            }));
            Debug.Log(lane_No);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && lane_No > 0)
        {
            lane_No -= 1;
            Util.Move(gameObject, Lanes[lane_No], "Lane" + lane_No.ToString());
            StartCoroutine(Util.WaitForSec(duration, () =>
            {
                Util.Move(CharacterShadow, gameObject);
            }));
            Debug.Log(lane_No);
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
}

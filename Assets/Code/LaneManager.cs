using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;


// Basic manager for multilane scene.
// It controls the lane changing part and the correlated animation.
// Not involving actuall character behaviors.
// Todo: figure out whether need to do the rhythm check part in this class or not. Need to discuss detailed control.

public class LaneManager: MonoBehaviour
{

    // Player Character object.
    public GameObject Character;
    // A tmp Character to solve possible animation problem.
    public GameObject CharacterShadow;
    // List of lane pivot point, to hold the positions for player to move.
    public List<GameObject> Lanes;
    // duration between shadow and character animation.
    public float duration;


    // Created for rhythm check, currently no use.
    private int timer;
    // index of lane that the player is on.
    private int lane_No;

    //Initialize
    private void Start()
    {
        lane_No = 0;
    }

    private void Update()
    {
        timer += 1;
        if (Input.GetKeyDown(KeyCode.UpArrow) && lane_No < 3)
        {
            lane_No += 1;
            Util.Move(Character, Lanes[lane_No], "Lane"+lane_No.ToString());
            StartCoroutine(Util.WaitForSec(duration, () =>
            {
                Util.Move(CharacterShadow, Character);
            }));
            Debug.Log(lane_No);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow) && lane_No > 0)
        {
            lane_No -= 1;
            Util.Move(Character, Lanes[lane_No], "Lane" + lane_No.ToString());
            StartCoroutine(Util.WaitForSec(duration, () =>
            {
                Util.Move(CharacterShadow, Character);
            }));
            Debug.Log(lane_No);
        }
    }

    
}

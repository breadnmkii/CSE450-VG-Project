using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fungus;

public class TutorialController : MonoBehaviour
{
    public Fungus.Flowchart flowchart;
    public GameObject player;

    public GameObject projectile;

    public List<Transform> lanes;

    public int Step;
    private int LastStep;

    private bool press1;
    private bool press2;
    private bool press3;
    private bool press4;

    void Start()
    {
        Step = flowchart.GetIntegerVariable("Step");
        LastStep = -1;
        press1 = false;
        press2 = false;
        press3 = false;
        press4 = false;
    }

    // Update is called once per frame
    void Update()
    {
        Step = flowchart.GetIntegerVariable("Step");
        if (LastStep == Step)
        {

        }
        else if (Step == 0)
        {
            player.GetComponent<Player>().enabled = false;
        }
        else if (Step == 1)
        {
            player.GetComponent<Player>().enabled = true;
        }
        else if (Step == 2)
        {
            player.GetComponent<Player>().enabled = true;
        }
        else if (Step == 3)
        {
            SpawnProjectile();
        }
        else if (Step == 4)
        {
            projectile.GetComponent<Rigidbody2D>().velocity = Vector2.left * 4;
            projectile.layer = player.layer;
            player.GetComponent<Player>().enabled = true;
            StartCoroutine(atkTut());
        }
        LastStep = Step;

        if (Input.GetKeyDown(KeyCode.A))
        {
            press1 = true;
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            press2 = true;
        }
        if (Input.GetKeyDown(KeyCode.D))
        {
            press3 = true;
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            press4 = true;
        }
        if ( press1 && press2 && press3 && press4 && Step == 1)
        {
            Debug.Log("to Step 2");
            flowchart.ExecuteBlock("New Block");
            flowchart.SetIntegerVariable("Step", 2);
        }
    }

    public void SpawnProjectile()
    {
        projectile.transform.position = lanes[player.layer - 6].position;
    }

    IEnumerator atkTut()
    {
        yield return new WaitForSeconds(5);

        flowchart.ExecuteBlock("EndTutorial");
    }
}

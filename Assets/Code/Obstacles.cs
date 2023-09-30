using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;


// General script for all types of obstacles.
public class Obstacles : MonoBehaviour
{
    // Represent the tag.
    public int type;
    Rigidbody2D _rb;
    public GameObject startPoint;

    private void Start()
    {
        if(gameObject.tag == "Breakable") { type = 1; }
        else { type = 0; }
        _rb = gameObject.GetComponent<Rigidbody2D>();
        Util.Move(gameObject, startPoint);
        Util.SetSpeed(_rb, Vector2.left * 5);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        
        if (collision.gameObject.GetComponent<Player>())
        {
             Debug.Log("Touch Player");
             collision.gameObject.GetComponent<Player>().ModifyHP(-1);
             Util.Move(gameObject, startPoint);
             Util.SetSpeed(_rb, Vector2.left * 0);
        }
        if (collision.gameObject.GetComponent<ObstacleChecker>())
        {
            Debug.Log("Ready for check");
            if (type == 1)
            {
                collision.gameObject.GetComponent<ObstacleChecker>().AddObstacle(gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (type == 1 && collision.gameObject.GetComponent<ObstacleChecker>())
        {
            Debug.Log("Out Check");
            collision.gameObject.GetComponent<ObstacleChecker>().RemoveObstacle();
        }
    }
}

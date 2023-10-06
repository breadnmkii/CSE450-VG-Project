using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Diagnostics;


/* General script for all types of obstacles.
 * 
 * This script encodes all the necessary data of an obstacle, like
 * - baseSpeed:     the base speed of the obstacle
 * - isBreakable:   whether the player can "hit" the note
 * 
 * The script also manages the lifetime of obstacles and trigger collision
 * detection.
 */
public class Obstacles : MonoBehaviour
{
    /* Public properties */
    // Outlets
    Rigidbody2D _rb;

    // Obstacle properties
    public int baseSpeed;
    public bool isBreakable;


    private void Start()
    {
        // Connect outlets
        _rb = gameObject.GetComponent<Rigidbody2D>();
        // isBreakable = gameObject.CompareTag("Breakable");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.GetComponent<Player>())
        {
            Player player = other.gameObject.GetComponent<Player>();
            // Debug.Log("Touch Player");
            player.ModifyHP(-1);
            
        }
        if (other.gameObject.GetComponent<ObstacleChecker>())
        {
            // Debug.Log("Ready for check");
            Debug.Log("Enter zone at " + Time.timeSinceLevelLoad);
            if (isBreakable)
            {
                other.gameObject.GetComponent<ObstacleChecker>().AddObstacle(gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isBreakable && other.gameObject.GetComponent<ObstacleChecker>())
        {
            // Debug.Log("Out Check");
            Debug.Log("Exit zone at " + Time.timeSinceLevelLoad);
            other.gameObject.GetComponent<ObstacleChecker>().RemoveObstacle(gameObject.layer);
        }
    }
}

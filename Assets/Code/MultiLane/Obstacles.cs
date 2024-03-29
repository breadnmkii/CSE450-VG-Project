using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    }

    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.GetComponent<Player>())
        {
            Player player = other.gameObject.GetComponent<Player>();
            Animator playerAni = player.GetComponent<Animator>();

            // Update health based on obstacle hit (projectile vs HP powerup)
            if (CompareTag("PowerupHP"))
            {
                player.ModifyHP(1);
            }
            else if (!player.isImmune)
            {
                player.ModifyHP(-1);
                player.PlayDamageSound();
                player.LosePointsFromDamage();
                playerAni.SetTrigger("harm");
            }
            

            // Destroy projectile on hitting player
            Destroy(gameObject);
            
        }
        if (other.gameObject.GetComponent<ObstacleChecker>())
        {
            //Debug.Log("Enter zone at " + Time.timeSinceLevelLoad);
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
            //Debug.Log("Exit zone at " + Time.timeSinceLevelLoad);
            other.gameObject.GetComponent<ObstacleChecker>().RemoveObstacle(gameObject.layer);
        }
    }

}

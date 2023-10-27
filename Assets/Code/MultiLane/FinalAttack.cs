using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalAttack : MonoBehaviour
{
    Rigidbody2D _rb;

    public float speed;

    void Start()
    {
        _rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Auto scroll
        _rb.velocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<Player>())
        {
            Player player = other.gameObject.GetComponent<Player>();

            // player animation
            Animator playerAni = player.GetComponent<Animator>();
            playerAni.SetTrigger("harm");

            // Update health
            player.ModifyHP(-2 * player.getHP());
            player.PlayDamageSound();

        }
        if (other.gameObject.GetComponent<ObstacleChecker>())
        {
            other.gameObject.GetComponent<ObstacleChecker>().AddObstacle(gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<ObstacleChecker>())
        {
            //Debug.Log("Exit zone at " + Time.timeSinceLevelLoad);
            other.gameObject.GetComponent<ObstacleChecker>().RemoveObstacle(gameObject.layer);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CleanupZone : MonoBehaviour
{
    // Outlets
    public GameObject player;

    // Damage player
    void OnTriggerEnter2D(Collider2D other)
    {
        // On obstacle collision
        if (other.gameObject.GetComponent<Obstacles>())
        {
            player.GetComponent<Player>().ModifyHP(-1);
            Destroy(other);
        }
        
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

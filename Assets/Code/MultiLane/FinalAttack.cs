using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinalAttack : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Player>())
        {
            Player tar = collision.gameObject.GetComponent<Player>();
            tar.ModifyHP(-2*tar.getHP());
        }
    }
}

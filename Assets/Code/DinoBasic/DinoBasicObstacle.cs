using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoBasic
{
    public class DinoBasicObstacle : MonoBehaviour
    {


        // Outlet
        Rigidbody2D _rb;

        // Start is called before the first frame update
        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            // Auto scroll
            _rb.velocity = transform.right * -7f;
        }

        void OnCollisionEnter2D(Collision2D other)
        {

            // Reload scene when colliding with obstacle
            if (other.gameObject.GetComponent<DinoBasicController>())
            {
                other.gameObject.GetComponent<Animator>().SetTrigger("dead");

                other.gameObject.GetComponent<DinoBasicController>().Dead();
            }

            // Despawn after reaching despawn zone
            if (other.gameObject.layer == LayerMask.NameToLayer("CactiDespawn"))
            {
                Destroy(gameObject);
            }
        }
    }
}

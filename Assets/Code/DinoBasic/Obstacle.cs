using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoBasic
{
    public class Obstacle : MonoBehaviour
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
            _rb.AddForce(Vector2.left * 25f * Time.deltaTime, ForceMode2D.Impulse);
        }

        void OnCollisionEnter2D(Collision2D other)
        {
            // Reload scene when colliding with obstacle
            if (other.gameObject.GetComponent<DinoBasicController>())
            {
                other.gameObject.GetComponent<DinoBasicController>().Dead();
            }
        }
    }
}

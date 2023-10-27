using System.Collections;
using System.Collections.Generic;
using DinoBasic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DinoBasic
{
    public class Goal : MonoBehaviour
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
            // Load next scene when colliding with goal
            if (other.gameObject.GetComponent<DinoBasicController>())
            {
                SceneManager.LoadScene("Tutorial");
            }
        }
    }
}

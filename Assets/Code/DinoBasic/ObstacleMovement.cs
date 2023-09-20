using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DinoBasic
{
    public class ObstacleMovement : MonoBehaviour
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
            _rb.AddForce(Vector2.left * 15f * Time.deltaTime, ForceMode2D.Impulse);
        }
    }
}

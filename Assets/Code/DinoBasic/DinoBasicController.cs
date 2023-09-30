using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DinoBasic
{
    public class DinoBasicController : MonoBehaviour
    {
        // Outlet
        Rigidbody2D _rb;

        // State tracking
        public int jumpsLeft;

        // Gameover UI Object
        public GameObject DeadUI;

        // Start is called before the first frame update
        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
        }

        // Update is called once per frame
        void Update()
        {
            // Jump
            if(Input.GetKeyDown(KeyCode.Space) || 
                Input.GetKeyDown(KeyCode.UpArrow) || 
                Input.GetKeyDown(KeyCode.W))
            {
                if(jumpsLeft > 0)
                {
                    jumpsLeft--;
                    _rb.AddForce(Vector2.up * 25f, ForceMode2D.Impulse);
                }
            }
        }

        public void Dead()
        {
            DeadUI.SetActive(true);
            Time.timeScale = 0;
        }

        // check for collisions
        void OnCollisionStay2D(Collision2D other)
        {
            // Check that we collided with Ground
            if(other.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                // Check what is directly below our character's feet
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, 0.8f);

                Debug.DrawRay(transform.position, Vector2.down * 0.8f);

                // We might have multiple things below character's feet
                for(int i = 0; i < hits.Length; i++)
                {
                    RaycastHit2D hit = hits[i];

                    // Check that we collided with ground below our feet
                    if(hit.collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
                    {
                        // Reset jump count
                        jumpsLeft = 1;
                    }
                }
            }
        }
    }
}

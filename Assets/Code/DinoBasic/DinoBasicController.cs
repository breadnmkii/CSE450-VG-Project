using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DinoBasic
{
    public class DinoBasicController : MonoBehaviour
    {
        // Outlet
        Rigidbody2D _rb;

        // Sound
        AudioSource audioSource;
        public AudioClip jumpSound;
        public AudioClip slamSound;
        public float volume;

        // State tracking
        public int jumpsLeft;

        // Gameover UI Object
        public GameObject DeadUI;

        // Lane position
        Vector3 PlayerStartPosition;

        Animator animator;

        // Start is called before the first frame update
        void Start()
        {
            _rb = GetComponent<Rigidbody2D>();
            PlayerStartPosition = gameObject.transform.position;

            animator = GetComponent<Animator>();
            audioSource = GetComponent<AudioSource>();
            audioSource.volume = volume;

        }

        // Update is called once per frame
        void Update()
        {
            // Jump
            if(Input.GetKey(KeyCode.Space))
            {
                animator.SetBool("stand", true);
                if(jumpsLeft > 0)
                {
                    jumpsLeft--;
                    _rb.AddForce(Vector2.up * 25f, ForceMode2D.Impulse);
                    audioSource.PlayOneShot(jumpSound);
                }
            }

            // Slam to ground
            if (Input.GetKeyDown(KeyCode.S))
            {
                gameObject.transform.position = PlayerStartPosition;
                audioSource.PlayOneShot(slamSound);
            }
        }

        public void Dead()
        {
            animator.SetTrigger("dead");
            
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
                        animator.SetBool("stand", false);
                        // Reset jump count
                        jumpsLeft = 1;
                    }
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DinoBasic
{
    public class StartGame : MonoBehaviour
    {
        // Outlets
        public GameObject[] backgroundObjects;

        // State Tracking
        bool gameStarted;

        // Start is called before the first frame update
        void Start()
        {
            gameStarted = false;
            Time.timeScale = 0f;
            
            // Hide all background objects until the game starts
            for (int i = 0; i < backgroundObjects.Length; i++)
            {
                backgroundObjects[i].SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (!gameStarted)
            {
                Time.timeScale = 0f;

                if (Input.GetKey(KeyCode.Space))
                {
                    gameStarted = true;
                    Time.timeScale = 1f;

                    // Load all background objects when the game starts
                    for (int i = 0; i < backgroundObjects.Length; i++)
                    {
                        backgroundObjects[i].SetActive(true);
                    }
                }
            }
        }
    }
}

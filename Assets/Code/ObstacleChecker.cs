using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleChecker : MonoBehaviour
{
    // Queue to store all notes coming in this lane in order.
    public Queue<GameObject> obs;

    // Start is called before the first frame update
    void Start()
    {
        obs = new Queue<GameObject>();
    }

    // Add tar into the queue;
    public void AddObstacle(GameObject tar)
    {
        obs.Enqueue(tar);
        Debug.Log(obs.Count);
    }

    // Simply Dequeue
    public void RemoveObstacle()
    {
        obs.Dequeue();
        Debug.Log(obs.Count);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && obs.Count>0)
        {
            //GameObject tmp = obs.Dequeue();
            Destroy(obs.Peek());
        }
    }
}

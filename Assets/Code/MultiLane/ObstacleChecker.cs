using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class ObstacleChecker : MonoBehaviour
{
    // Queue to store all notes coming in this lane in order.
    public Queue<GameObject> obs_Lane_0;
    public Queue<GameObject> obs_Lane_1;
    public Queue<GameObject> obs_Lane_2;
    public Queue<GameObject> obs_Lane_3;
    public GameObject Player;
    public BossBehavior Boss;

    // Start is called before the first frame update
    void Start()
    {
        obs_Lane_0 = new Queue<GameObject>();
        obs_Lane_1 = new Queue<GameObject>();
        obs_Lane_2 = new Queue<GameObject>();
        obs_Lane_3 = new Queue<GameObject>();
    }

    // Add tar into the queue;
    public void AddObstacle(GameObject tar)
    {
        if (tar.layer == LayerMask.NameToLayer("Lane0"))
        {
            obs_Lane_0.Enqueue(tar);
            Debug.Log("Item Added, Current length:" + obs_Lane_0.Count);
        }
        else if (tar.layer == LayerMask.NameToLayer("Lane1"))
        {
            obs_Lane_1.Enqueue(tar);
        }
        else if (tar.layer == LayerMask.NameToLayer("Lane2"))
        {
            obs_Lane_2.Enqueue(tar);
        }
        else if (tar.layer == LayerMask.NameToLayer("Lane3"))
        {
            obs_Lane_3.Enqueue(tar);
        }
    }

    // Simply Dequeue
    public void RemoveObstacle(int layer)
    {
        if (layer == 6)
        {
            obs_Lane_0.Dequeue();
            Debug.Log("Item Removed, Current length:" + obs_Lane_0.Count);
        }
        else if (layer == 7)
        {
            obs_Lane_1.Dequeue();
        }
        else if (layer == 8)
        {
            obs_Lane_2.Dequeue();
        }
        else if (layer == 9)
        {
            obs_Lane_3.Dequeue();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            if (Player.layer == 6 && obs_Lane_0.Count > 0)
            {
                Destroy(obs_Lane_0.Peek());
                Boss.doDamage(1);
            }
            else if (Player.layer == 7 && obs_Lane_1.Count > 0)
            {
                Destroy(obs_Lane_1.Peek());
                Boss.doDamage(1);
            }
            else if (Player.layer == 8 && obs_Lane_2.Count > 0)
            {
                Destroy(obs_Lane_2.Peek());
                Boss.doDamage(1);
            }
            else if (Player.layer == 9 && obs_Lane_3.Count > 0)
            {
                Destroy(obs_Lane_3.Peek());
                Boss.doDamage(1);
            }
        }
    }
}
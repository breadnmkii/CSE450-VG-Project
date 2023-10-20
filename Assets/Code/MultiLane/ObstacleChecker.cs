using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleChecker : MonoBehaviour
{
    // Queue to store all notes coming in this lane in order.
    public Queue<GameObject> obs_Lane_0;
    public Queue<GameObject> obs_Lane_1;
    public Queue<GameObject> obs_Lane_2;
    public Queue<GameObject> obs_Lane_3;
    public GameObject Player;
    public BossBehavior Boss;

    private KeyCode[] AttackAKey;
    private KeyCode[] AttackBKey;

    private int operationType;


    // Start is called before the first frame update
    void Start()
    {
        operationType = 0;
        obs_Lane_0 = new Queue<GameObject>();
        obs_Lane_1 = new Queue<GameObject>();
        obs_Lane_2 = new Queue<GameObject>();
        obs_Lane_3 = new Queue<GameObject>();
        AttackAKey = new KeyCode[2];
        AttackAKey[0] = KeyCode.J;
        AttackAKey[1] = KeyCode.F;
        AttackBKey = new KeyCode[2];
        AttackBKey[0] = KeyCode.K;
        AttackBKey[1] = KeyCode.D;


    }

    // Add tar into the queue;
    public void AddObstacle(GameObject tar)
    {
        if (tar.layer == LayerMask.NameToLayer("Lane0"))
        {
            obs_Lane_0.Enqueue(tar);
            Debug.Log("Item Added 0, Current length:" + obs_Lane_0.Count);
        }
        else if (tar.layer == LayerMask.NameToLayer("Lane1"))
        {
            obs_Lane_1.Enqueue(tar);
            Debug.Log("Item Added 1, Current length:" + obs_Lane_1.Count);
        }
        else if (tar.layer == LayerMask.NameToLayer("Lane2"))
        {
            obs_Lane_2.Enqueue(tar);
            Debug.Log("Item Added 2, Current length:" + obs_Lane_2.Count);
        }
        else if (tar.layer == LayerMask.NameToLayer("Lane3"))
        {
            obs_Lane_3.Enqueue(tar);
            Debug.Log("Item Added 3, Current length:" + obs_Lane_3.Count);
        }
    }

    // Simply Dequeue
    public void RemoveObstacle(int layer)
    {
        if (layer == 6)
        {
            obs_Lane_0.Dequeue();
            // Debug.Log("Item Removed, Current length:" + obs_Lane_0.Count);
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
        // Attack
        if (Input.GetKeyDown(AttackAKey[operationType]) ||
            Input.GetKeyDown(AttackBKey[operationType]))
        {
            Player.GetComponent<Animator>().SetBool("atk", true);
            Player.GetComponent<Animator>().SetBool("atk", false);
            
            if (Player.layer == 6 && obs_Lane_0.Count > 0)
            {

              
                Destroy(obs_Lane_0.Peek());
                GameObject destroyed = Instantiate(
                    GameController.instance.destoryedPrefab,
                    GameController.instance.lanes[0].transform
                        .GetChild(1).gameObject.transform.position,
                    Quaternion.identity
                );
                Destroy(destroyed, 0.65f);
                
                Boss.doDamage(1);
            }
            else if (Player.layer == 7 && obs_Lane_1.Count > 0)
            {
                // obs_Lane_1.Peek().GetComponent<Animator>().SetBool("shieldon", true);

                Destroy(obs_Lane_1.Peek());
                GameObject destroyed = Instantiate(
                    GameController.instance.destoryedPrefab,
                    GameController.instance.lanes[1].transform
                        .GetChild(1).gameObject.transform.position,
                    Quaternion.identity
                );
                Destroy(destroyed, 0.65f);

                Boss.doDamage(1);
            }
            else if (Player.layer == 8 && obs_Lane_2.Count > 0)
            {
                // obs_Lane_2.Peek().GetComponent<Animator>().SetBool("shieldon", true);

                Destroy(obs_Lane_2.Peek());
                GameObject destroyed = Instantiate(
                    GameController.instance.destoryedPrefab,
                    GameController.instance.lanes[2].transform
                        .GetChild(1).gameObject.transform.position,
                    Quaternion.identity
                );
                Destroy(destroyed, 0.65f);

                Boss.doDamage(1);
            }
            else if (Player.layer == 9 && obs_Lane_3.Count > 0)
            {

                Destroy(obs_Lane_3.Peek());
                GameObject destroyed = Instantiate(
                    GameController.instance.destoryedPrefab,
                    GameController.instance.lanes[3].transform
                        .GetChild(1).gameObject.transform.position,
                    Quaternion.identity
                );
                Destroy(destroyed, 0.65f);

                // obs_Lane_3.Peek().GetComponent<Animator>().SetBool("shieldon", true);
                Boss.doDamage(1);
            }

        }
    }

    // Change Operation Type;
    public void setOperationType(int type)
    {
        this.operationType = type;
    }
}



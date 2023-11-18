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
    public GameObject MyPlayer;
    public BossBehavior Boss;

    // Sound
    AudioSource audioSource;
    public AudioClip hitSound;
    public float hitSoundVolume;

    private KeyCode[] AttackAKey;
    private KeyCode[] AttackBKey;

    private int operationType;

    // Bounds for good, great, and perfect hit zones
    private double goodHitUpperBound;
    private double goodHitLowerBound;
    private double perfHitLowerBound;
    private double perfHitUpperBound;

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
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = hitSoundVolume;
        goodHitUpperBound = -5.95;
        goodHitLowerBound = -5.05;
        perfHitLowerBound = -5.65;
        perfHitUpperBound = -5.35;
    }

    // Add tar into the queue;
    public void AddObstacle(GameObject tar)
    {
        if (tar.layer == LayerMask.NameToLayer("Lane0"))
        {
            obs_Lane_0.Enqueue(tar);
            // Debug.Log("Item Added 0, Current length:" + obs_Lane_0.Count);
        }
        else if (tar.layer == LayerMask.NameToLayer("Lane1"))
        {
            obs_Lane_1.Enqueue(tar);
            // Debug.Log("Item Added 1, Current length:" + obs_Lane_1.Count);
        }
        else if (tar.layer == LayerMask.NameToLayer("Lane2"))
        {
            obs_Lane_2.Enqueue(tar);
            // Debug.Log("Item Added 2, Current length:" + obs_Lane_2.Count);
        }
        else if (tar.layer == LayerMask.NameToLayer("Lane3"))
        {
            obs_Lane_3.Enqueue(tar);
            // Debug.Log("Item Added 3, Current length:" + obs_Lane_3.Count);
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
        // A Attack
        if (Input.GetKeyDown(AttackAKey[operationType]))
        {
            MyPlayer.GetComponent<Animator>().SetTrigger("atkkk");

            GameObject laneProjectile = null;
            int laneAttackedIn = -1;

            if (MyPlayer.layer == 6 && obs_Lane_0.Count > 0)
            {
                // Verify is not a B projectile
                laneProjectile = obs_Lane_0.Peek();
                laneAttackedIn = 0;
            }
            else if (MyPlayer.layer == 7 && obs_Lane_1.Count > 0)
            {
                laneProjectile = obs_Lane_1.Peek();
                laneAttackedIn = 1;
            }
            else if (MyPlayer.layer == 8 && obs_Lane_2.Count > 0)
            {
                laneProjectile = obs_Lane_2.Peek();
                laneAttackedIn = 2;
            }
            else if (MyPlayer.layer == 9 && obs_Lane_3.Count > 0)
            {
                laneProjectile = obs_Lane_3.Peek();
                laneAttackedIn = 3;
            }

            if (laneProjectile != null && !laneProjectile.CompareTag("ProjectileB"))
            {
                // Did hit an A note with attack A, apply score
                EarnPointsFromHit(laneProjectile.transform.position.x);

                Destroy(laneProjectile);

                GameObject destroyedAnimation = Instantiate(
                        GameController.instance.destroyedPrefabA,
                        GameController.instance.lanes[laneAttackedIn].transform
                            .GetChild(1).gameObject.transform.position,
                        Quaternion.identity
                    );

                Destroy(destroyedAnimation, 0.65f);
                PlayHitSound();

                if (Boss != null)
                {
                    Boss.doDamage(1);
                }
            }
            else
            {
                // No notes hit, penalize score
                Player.instance.LostPointsFromMiss();
            }
        }

        // B Attack
        if (Input.GetKeyDown(AttackBKey[operationType])) {
            MyPlayer.GetComponent<Animator>().SetTrigger("atkkk");

            GameObject laneProjectile = null;
            int laneAttackedIn = -1;

            if (MyPlayer.layer == 6 && obs_Lane_0.Count > 0)
            {
                // Verify is not a B projectile
                laneProjectile = obs_Lane_0.Peek();
                laneAttackedIn = 0;
            }
            else if (MyPlayer.layer == 7 && obs_Lane_1.Count > 0)
            {
                laneProjectile = obs_Lane_1.Peek();
                laneAttackedIn = 1;
            }
            else if (MyPlayer.layer == 8 && obs_Lane_2.Count > 0)
            {
                laneProjectile = obs_Lane_2.Peek();
                laneAttackedIn = 2;
            }
            else if (MyPlayer.layer == 9 && obs_Lane_3.Count > 0)
            {
                laneProjectile = obs_Lane_3.Peek();
                laneAttackedIn = 3;
            }

            if (laneProjectile != null && laneProjectile.CompareTag("ProjectileB"))
            {
                // Did hit an A note with attack A, apply score
                EarnPointsFromHit(laneProjectile.transform.position.x);

                Destroy(laneProjectile);

                GameObject destroyedAnimation = Instantiate(
                        GameController.instance.destroyedPrefabB,
                        GameController.instance.lanes[laneAttackedIn].transform
                            .GetChild(1).gameObject.transform.position,
                        Quaternion.identity
                    );

                Destroy(destroyedAnimation, 0.65f);
                PlayHitSound();

                if (Boss != null)
                {
                    Boss.doDamage(1);
                }
            }
            else
            {
                // No notes hit, penalize score
                Player.instance.LostPointsFromMiss();
            }
        }
    }

    // Change Operation Type;
    public void setOperationType(int type)
    {
        this.operationType = type;
    }

    // Play the hit sound
    public void PlayHitSound()
    {
        audioSource.PlayOneShot(hitSound);
    }

    // Earn points for hitting a note
    private void EarnPointsFromHit(double xPos)
    {
        if (xPos > goodHitLowerBound)
        {
            Player.instance.EarnPointsFromGoodHit();
        }
        else if (xPos > perfHitUpperBound && xPos <= goodHitLowerBound)
        {
            Player.instance.EarnPointsFromGreatHit();
        }
        else if (xPos >= perfHitLowerBound && xPos <= perfHitUpperBound)
        {
            Player.instance.EarnPointsFromPerfHit();
        }
        else if (xPos >= goodHitUpperBound && xPos < perfHitLowerBound)
        {
            Player.instance.EarnPointsFromGreatHit();
        }
        else // same as xPos < goodHitUpperBound
        {
            Player.instance.EarnPointsFromGoodHit();
        }
    }
}



using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading;
using UnityEngine;
using TMPro;


// Basic manager for multilane scene.
// It controls the lane changing part and the correlated animation.
// Not involving actuall character behaviors.
// Todo: figure out whether need to do the rhythm check part in this class or not. Need to discuss detailed control.

public enum HitIndication
{
    Good = 0,
    Great = 1,
    Perfect = 2
}

public class Player : MonoBehaviour
{
    public static Player instance;

    // Outlets
    Rigidbody2D _rb;

    // Sound
    AudioSource audioSource;
    public AudioClip damageSound;
    public float damageSoundVolume;
    public AudioClip jumpSound;
    public AudioClip slamSound;

    public bool isImmune;

    // A tmp Character to solve possible animation problem.
    // public GameObject CharacterShadow;
    // List of lane pivot point, to hold the positions for player to move.
    public List<GameObject> Lanes;
    // duration between shadow and character animation.
    public float duration;
    // Max HP
    public int MaxHP;
    // Health point
    private int HP;

    // number of jumps left for player
    public int jumpsLeft;

    // index of lane that the player is on.
    private int lane_No;

    // representing current operation type: 0 = right-handed, 1 = left-handed;
    private int operationType;

    private KeyCode[,] MoveLaneKeys;


    // Gameover UI Object
    public GameObject DeadUI;

    // Win UI Object
    public GameObject WinUI;

    // Player's animator
    private Animator myAni;

    // Score Tracking
    private double score = 0;
    public double goodHitReward;
    public double greatHitReward;
    public double perfHitReward;
    public double damagePenalty;
    public double missPenalty;
    public TMP_Text scoreUI;
    public int numGoodHits;
    public int numGreatHits;
    public int numPerfectHits;
    public int numMisses;
    public int numNotesDamaged;

    // Hit Indications
    public GameObject[] hitIndications;
    private double _nowTime;
    private bool _hitIndicationActive;
    private double _hitIndicationDisplayStartTime;
    private const double _hitIndicationDisplayDuration = 0.5;

    void Awake()
    {
        instance = this;
    }

    //Initialize
    private void Start()
    {
        isImmune = false;
        operationType = 0;
        lane_No = 0;
        if (MaxHP > 8)
        {
            MaxHP = 8;
        }
        if (MaxHP < 1)
        {
            MaxHP = 1;
        }
        HP = MaxHP;
        myAni = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        MoveLaneKeys = new KeyCode[2, 4];
        MoveLaneKeys[0, 0] = KeyCode.A;
        MoveLaneKeys[0, 1] = KeyCode.S;
        MoveLaneKeys[0, 2] = KeyCode.D;
        MoveLaneKeys[0, 3] = KeyCode.F;
        MoveLaneKeys[1, 0] = KeyCode.Semicolon;
        MoveLaneKeys[1, 1] = KeyCode.L;
        MoveLaneKeys[1, 2] = KeyCode.K;
        MoveLaneKeys[1, 3] = KeyCode.J;
        audioSource.volume = damageSoundVolume;
        score = 0;
        numGoodHits = 0;
        numGreatHits = 0;
        numPerfectHits = 0;
        numMisses = 0;
        numNotesDamaged = 0;
        _hitIndicationActive = false;
        _hitIndicationDisplayStartTime = 0;
    }

    private void Update()
    {
        _nowTime = Time.timeSinceLevelLoad;

        // Update hit indication
        if (_hitIndicationActive)
        {
            if (_nowTime >= (_hitIndicationDisplayStartTime + _hitIndicationDisplayDuration))
            {
                HideHitIndications();
            }
        }

        // Update UI
        scoreUI.text = "Score: " + GetScore().ToString();

        myAni.SetBool("isGround", jumpsLeft > 0);
        myAni.SetFloat("HorizontalVelocity", _rb.velocity.y);
        if (this.HP <= 0)
        {
            myAni.SetBool("Dead", true);
            GetComponent<Rigidbody2D>().gravityScale = 0f ;
            GetComponent<CapsuleCollider2D>().enabled = false;
        }

        // Go up lane
        if (Input.GetKeyDown(MoveLaneKeys[operationType, 0]))
        {
            DisableLane(lane_No);
            lane_No = 3;
            EnableLane(lane_No);
            Util.Move(gameObject, Lanes[lane_No].transform.GetChild(1).gameObject, "Lane" + lane_No.ToString());
        }
        else if (Input.GetKeyDown(MoveLaneKeys[operationType, 1]))
        {
            DisableLane(lane_No);
            lane_No = 2;
            EnableLane(lane_No);
            Util.Move(gameObject, Lanes[lane_No].transform.GetChild(1).gameObject, "Lane" + lane_No.ToString());
        }
        else if (Input.GetKeyDown(MoveLaneKeys[operationType, 2]))
        {
            DisableLane(lane_No);
            lane_No = 1;
            EnableLane(lane_No);
            Util.Move(gameObject, Lanes[lane_No].transform.GetChild(1).gameObject, "Lane" + lane_No.ToString());
        }
        else if (Input.GetKeyDown(MoveLaneKeys[operationType, 3]))
        {
            DisableLane(lane_No);
            lane_No = 0;
            EnableLane(lane_No);
            Util.Move(gameObject, Lanes[lane_No].transform.GetChild(1).gameObject, "Lane" + lane_No.ToString());
        }

        // Jump
        if (Input.GetKey(KeyCode.Space))
        {
            if (jumpsLeft > 0)
            {
                jumpsLeft--;
                _rb.AddForce(Vector2.up * 25f, ForceMode2D.Impulse);
                audioSource.PlayOneShot(jumpSound);
            }
        }

        // Slam to ground
        if (Input.GetKeyDown(KeyCode.Space) && (jumpsLeft == 0))
        {
            Util.Move(gameObject, Lanes[lane_No].transform.GetChild(1).gameObject, "Lane" + lane_No.ToString());
            audioSource.PlayOneShot(slamSound);
        }
    }


    // HP Getter
    public int getHP()
    {
        return HP;
    }


    // Add current HP by integer n.
    // n range [-Inf, Inf].
    // Constraint current HP within range [0, MaxHP] 
    public void ModifyHP(int n)
    {
        if(n < 0 && !isImmune)
        {
            HP += n;
            isImmune = true;
            StartCoroutine(immune());
        }
        else if (n > 0)
        {
            HP += n;
        }
        
        if (HP < 0 )
        {
            HP = 0;
        }
        if (HP > MaxHP)
        {
            HP = MaxHP;
        }
    }

    IEnumerator immune()
    {
        yield return new WaitForSeconds(1);

        isImmune = false;
    }

    // check for collisions
    void OnCollisionStay2D(Collision2D other)
    {
        // Check that we collided with Ground
        if (other.gameObject.layer == LayerMask.NameToLayer("LaneGround"))
        {
            // Check what is directly below our character's feet
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, Vector2.down, 1.1f);

            // Debug.DrawRay(transform.position, Vector2.down * 1.1f);

            // We might have multiple things below character's feet
            for (int i = 0; i < hits.Length; i++)
            {
                RaycastHit2D hit = hits[i];

                // Check that we collided with ground below our feet
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("LaneGround"))
                {
                    // Reset jump count
                    jumpsLeft = 1;
                }
            }
        }
    }

    // Disable collision for a lane
    private void DisableLane(int laneNo)
    {
        Lanes[laneNo].transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = false;
    }

    // Enable collision for a lane
    private void EnableLane(int laneNo)
    {
        Lanes[laneNo].transform.GetChild(0).GetComponent<BoxCollider2D>().enabled = true;
    }

    // Death effects and UI Call
    public void onDead()
    {
        // Destroy(CharacterShadow);
        DeadUI.SetActive(true);
    }

    // Show win screen
    public void onWin()
    {
        WinUI.SetActive(true);
    }

    //Change Operation Type;
    public void setOperationType(int type)
    {
        this.operationType = type;
    }

    // Play the damage sound
    public void PlayDamageSound()
    {
        audioSource.PlayOneShot(damageSound);
    }

    // Retrieve score
    public double GetScore()
    {
        return score;
    }

    // Modify score
    private void ModifyScore(double s)
    {
        score += s;
        if (score < 0)
        {
            score = 0;
        }
    }

    // Earn points from hitting a note in the "good" zone
    public void EarnPointsFromGoodHit()
    {
        DisplayHitIndication(HitIndication.Good);
        ModifyScore(goodHitReward);
        numGoodHits++;
    }

    // Earn points from hitting a note in the "great" zone
    public void EarnPointsFromGreatHit()
    {
        DisplayHitIndication(HitIndication.Great);
        ModifyScore(greatHitReward);
        numGreatHits++;
    }

    // Earn points from hitting a note in the "perfect" zone
    public void EarnPointsFromPerfHit()
    {
        DisplayHitIndication(HitIndication.Perfect);
        ModifyScore(perfHitReward);
        numPerfectHits++;
    }

    // Lose points from taking damage
    public void LosePointsFromDamage()
    {
        ModifyScore(-damagePenalty);
        numNotesDamaged++;
    }

    // Lose points from missing a note
    public void LostPointsFromMiss()
    {
        ModifyScore(-missPenalty);
        numMisses++;
    }

    // Display a hit indication
    private void DisplayHitIndication(HitIndication type)
    {
        if (hitIndications.Length == 0) { return; }
        HideHitIndications();

        
        hitIndications[(int)type].SetActive(true);
        _hitIndicationActive = true;
        _hitIndicationDisplayStartTime = _nowTime;
    }

    // Hide a hit indication
    private void HideHitIndications()
    {
        if (hitIndications.Length == 0) { return; }
        for (int i = 0; i < hitIndications.Length; i++)
        {
            hitIndications[i].SetActive(false);
        }

        _hitIndicationActive = false;
    }
}

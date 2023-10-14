using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CactiSpawner : MonoBehaviour
{
    // Cacti objects that can be spawned
    public GameObject[] Cacti;

    // Goal cactus
    public GameObject GoalCactus;

    // Number of cacti to spawn
    public int NumCacti;

    // Time intervals between cacti spawning
    public double[] CactiSpawnIntervals;

    // Time interval between normal cacti and goal cactus
    public double GoalCactusDelay;

    private int _numCactiTypes;             // number of cacti types that can be spawned
    private int _numCactiSpawnIntervals;    // number of cacit spawn intervals
    private double _nowTime;                // current time since level load
    private double _nextSpawnTime;          // next time to spawn a cactus
    private int _cactiRemaining;            // number of cacti left to spawn
    private bool _spawningFinished;         // whether or not we are done spawning cacti

    // Start is called before the first frame update
    void Start()
    {
        _numCactiTypes = Cacti.Length;
        _numCactiSpawnIntervals = CactiSpawnIntervals.Length;
        _nowTime = Time.timeSinceLevelLoad;
        _cactiRemaining = NumCacti;
        _spawningFinished = false;

        // Generate first random spawn time
        int timeIndex = Random.Range(0, _numCactiSpawnIntervals);
        _nextSpawnTime = _nowTime + CactiSpawnIntervals[timeIndex];
    }

    // Update is called once per frame
    void Update()
    {
        // Stop spawning cacti after specified amount are spawned
        if (_cactiRemaining > 0)
        {
            _nowTime = Time.timeSinceLevelLoad;
            
            if (_nowTime > _nextSpawnTime)
            {
                // Spawn random cactus
                int cactiIndex = Random.Range(0, _numCactiTypes);
                GameObject cactus = Instantiate(Cacti[cactiIndex]);
                cactus.transform.position = transform.position;

                // Update number of remaining cacti
                _cactiRemaining--;

                // Next spawn time depends on whether or not the next cactus is the goal cactus
                if (_cactiRemaining > 0)
                {
                    // Generate next random spawn time
                    int timeIndex = Random.Range(0, _numCactiSpawnIntervals);
                    _nextSpawnTime = _nowTime + CactiSpawnIntervals[timeIndex];
                }

                // Goal cactus is next
                else
                {
                    _nextSpawnTime = _nowTime + GoalCactusDelay;
                }
            }
        }

        // Spawn the goal cactus
        else
        {
            _nowTime = Time.timeSinceLevelLoad;

            if (_nowTime > _nextSpawnTime && !_spawningFinished)
            {
                // Spawn goal cactus
                GameObject cactus = Instantiate(GoalCactus);
                cactus.transform.position = transform.GetChild(0).transform.position;
                _spawningFinished = true;
            }
        }
    }
}

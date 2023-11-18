using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{
    public static GameController instance;

    //outlets
    public GameObject[] lanes;
    public GameObject destroyedPrefabA;
    public GameObject destroyedPrefabB;
    public GameObject formingPrefab; 



    void Awake(){
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class transition : MonoBehaviour
{

    
    // Start is called before the first frame update
    void Start()
    { 
    }

    // Update is called once per frame
    void Update()
    {
        StartCoroutine(trans());
    }

    IEnumerator trans()
    {
        yield return new WaitForSeconds(21);

        Util.LoadScene(0);
    }
}

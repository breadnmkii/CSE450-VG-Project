using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class transition : MonoBehaviour
{

    
    // Start is called before the first frame update
    void Start()
    {
        PlayVideo();
    }

    public void PlayVideo()
    {
        VideoPlayer _vp = GetComponent<VideoPlayer>();

        if (_vp)
        {
            string vidPath = System.IO.Path.Combine(Application.streamingAssetsPath, "scene 1_1.mp4");
            Debug.Log("Playing video path of: " + vidPath);
            _vp.url = vidPath;
            _vp.Play();
        }
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

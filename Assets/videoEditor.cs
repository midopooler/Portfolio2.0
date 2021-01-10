using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class videoEditor : MonoBehaviour
{
    public VideoPlayer video;
    void Start()
    {
       video = this.gameObject.GetComponent<VideoPlayer>();
        video.url = System.IO.Path.Combine(Application.streamingAssetsPath, "MY INTERESTS (2).mp4");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

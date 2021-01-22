using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class audioplayer : MonoBehaviour
{
    public AudioClip[] audios;
    void Start()
    {
    }

        // Update is called once per frame
        void Update()
    {
        
    }

  void OnCollisionEnter(Collision collision)
    {
        Debug.Log("hit");
        // if (collision.gameObject.tag == "Player"|| collision.gameObject.tag == "road" || collision.gameObject.tag == "brick")
        //{ 
        Random rand = new Random();

        this.gameObject.GetComponent<AudioSource>().clip = audios[Random.Range(0, audios.Length)];
            this.gameObject.GetComponent<AudioSource>().Play();
            
        //}
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class audioplayer : MonoBehaviour
{
    public AudioClip[] audios;
    public AudioSource source;
    void Start()
    {
        source = this.gameObject.GetComponent<AudioSource>();
    }

   

  void OnCollisionEnter(Collision collision)
    {
        Debug.Log("hit");
        // if (collision.gameObject.tag == "Player"|| collision.gameObject.tag == "road" || collision.gameObject.tag == "brick")
        //{ 
        Random rand = new Random();

        source.clip = audios[Random.Range(0, audios.Length-1)];
            source.Play();
            
        //}
    }
}

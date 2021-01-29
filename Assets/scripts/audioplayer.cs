using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class audioplayer : MonoBehaviour
{
    public AudioClip[] audios;
    public AudioSource source;
    private void Awake()
    {
        
            source = this.gameObject.GetComponent<AudioSource>();
        source.volume = 0;
        Invoke("audioon", 3f);
        
    }
   

   

  void OnCollisionEnter(Collision collision)
    {
       // Debug.Log("hit");
        // if (collision.gameObject.tag == "Player"|| collision.gameObject.tag == "road" || collision.gameObject.tag == "brick")
        //{ 
        Random rand = new Random();

        source.clip = audios[Random.Range(0, audios.Length-1)];
            source.Play();
            
        //}
    } 

  void  audioon()
    {
        source.volume = 100;
    }
}

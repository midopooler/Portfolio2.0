using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openButtonScript : MonoBehaviour
{
    public Animator buttonOpen;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("Car Entered");
            buttonOpen.SetTrigger("EnterButton");
        } 
    
    }
}

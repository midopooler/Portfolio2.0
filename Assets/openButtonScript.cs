using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openButtonScript : MonoBehaviour
{
    public Animator buttonOpen;
    public GameObject buttonOpener;
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            buttonOpener.transform.position = new Vector3(this.transform.position.x, buttonOpener.transform.position.y, this.transform.position.z);
            Debug.Log("Car Entered");
            buttonOpen.SetTrigger("EnterButton");
            
        } 
    
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.gameObject.CompareTag("Player"))
        {
            Debug.Log("CarExit");
            buttonOpen.SetTrigger("ExitButton");
            
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openButtonScript : MonoBehaviour
{
    public Animator buttonOpen;
    public GameObject buttonOpener;
    public string URL_toOpen;
    private void OnTriggerEnter(Collider other) 
    {
        if (other.gameObject.CompareTag("Player"))
        {
            buttonOpener.transform.position = new Vector3(this.transform.position.x, buttonOpener.transform.position.y, this.transform.position.z);
           
            buttonOpen.SetTrigger("EnterButton");

        }

    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            
            buttonOpen.SetTrigger("ExitButton");

        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp("mouse 0"))
            {
                Application.OpenURL(URL_toOpen);
            }
        }
    }
}

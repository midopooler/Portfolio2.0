using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Runtime.InteropServices;

public class openButtonScript : MonoBehaviour
{
    public Button openerbutton;
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
            if (Input.GetKeyUp(KeyCode.Return))
            {
                OpenPage();
            }
            if (openerbutton != null)
            {
                openerbutton.onClick.RemoveAllListeners();
                openerbutton.onClick.AddListener(OpenPage);

            }        
           
            
            
        } 
        
    }
    public void OpenPage()
    {
        Debug.Log(URL_toOpen);
       
       

#if UNITY_WEBGL

        openPage(URL_toOpen);

#else
        Application.OpenURL("www.google.com");
        Debug.Log("test");
       
#endif
    }


    [DllImport("__Internal")]
    private static extern void openPage(string url);
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class openinfo : MonoBehaviour
{
    public bool canvaopen;
    public Animator canvaopeneranim;
    public void canvaopener()
    {
        if (canvaopen == true)
        {
            Debug.Log("canvas should close now");
            canvaopen = false;
            canvaopeneranim.SetTrigger("CloseCanva");
        }
        else
        {
            Debug.Log("canvas should open now");
            canvaopen = true;
            canvaopeneranim.SetTrigger("OpenCanva");
        }
    }
}

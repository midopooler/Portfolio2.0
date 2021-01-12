using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubeController : MonoBehaviour
{
    public FixedJoystick fixedJoystick;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
       Vector3 translation = Vector3.forward * fixedJoystick.Horizontal + Vector3.right*fixedJoystick.Vertical;
        translation *= Time.deltaTime*2;
       
        transform.Translate(translation);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resetcarposition : MonoBehaviour
{
    void Update()
    {
      if (Input.GetKeyUp(KeyCode.R))
            { 
            this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            this.gameObject.transform.eulerAngles = new Vector3(this.gameObject.transform.eulerAngles.x, this.gameObject.transform.eulerAngles.y, 0);
            this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y+2.0f, this.gameObject.transform.position.z);
            
            // this.gameObject.transform.rotation = new Quaternion(this.gameObject.transform.rotation.x, this.gameObject.transform.rotation.y, 180, this.gameObject.transform.rotation.w);
            
            }
    }
}

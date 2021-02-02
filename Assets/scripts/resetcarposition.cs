using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class resetcarposition : MonoBehaviour
{
    public bool isinputenabled = true;
    public Transform defaultpos;
    public Transform car;
    
    void Update()
    {
        if (isinputenabled)
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                this.gameObject.transform.eulerAngles = new Vector3(this.gameObject.transform.eulerAngles.x, this.gameObject.transform.eulerAngles.y, 0);
                this.gameObject.transform.position = new Vector3(this.gameObject.transform.position.x, this.gameObject.transform.position.y + 2.0f, this.gameObject.transform.position.z);
                this.gameObject.GetComponent<Rigidbody>().freezeRotation = true;


                StartCoroutine(resetcar());


                // this.gameObject.transform.rotation = new Quaternion(this.gameObject.transform.rotation.x, this.gameObject.transform.rotation.y, 180, this.gameObject.transform.rotation.w);

            }
            if (Input.GetKeyUp(KeyCode.R) || car.transform.position.y < -100)
            {
                this.gameObject.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                this.gameObject.transform.eulerAngles = defaultpos.rotation.eulerAngles;
                this.gameObject.transform.position = defaultpos.position;
                this.gameObject.GetComponent<Rigidbody>().freezeRotation = true;
                StartCoroutine(resetcar());
            }
        }
    }

    IEnumerator resetcar()
    {
        isinputenabled = false;
        yield return new WaitForSeconds(1f);
        isinputenabled = true;
        this.gameObject.GetComponent<Rigidbody>().freezeRotation = false;
    }
}

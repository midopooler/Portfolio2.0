using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DeviceCheck : MonoBehaviour
{
    public TextMeshProUGUI devicename;
    void Start()
    {
        if(SystemInfo.deviceType == DeviceType.Handheld)
        {
            devicename.text = "moglail phone";
        } 
        else
        {
            devicename.text = "kamputer";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

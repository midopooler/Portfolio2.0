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
            devicename.text = "mobile phone";
        } 
        else if (SystemInfo.deviceType == DeviceType.Desktop)
        {
            devicename.text = "computer";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

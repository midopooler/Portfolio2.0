using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Runtime.InteropServices;

public class ScreenOrientationEventHandler : MonoBehaviour
{
    public bool isLand;

    [DllImport("__Internal")]
    private static extern void GoFullscreen();

    [DllImport("__Internal")]
    private static extern bool IsMobile();
     
    [DllImport("__Internal")]
    private static extern bool CheckOrientation();


    //Whether your webgl is being playing on mobile devices or not.
    public bool isMobile()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
             return IsMobile();
#endif
        return false;
    }

    //Activate Fullscreen.
    public static void ActivateFullscreen()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
            GoFullscreen();
#endif
    }

    //Check current orientation.
    public bool isLandScape()
    {
#if !UNITY_EDITOR && UNITY_WEBGL
             return CheckOrientation();
#endif
        return false;
    }

    //When your fullscreen button is clicked(touched).
    public void OnPointerClick()
    {
        //If you're using mobile devices.
        if (isMobile())
        {
            if (SystemInfo.operatingSystem.Contains("iOS"))
            {
                //Do Something.
            }
            else if (SystemInfo.operatingSystem.Contains("Android"))
            {
                if (isLand)
                {
                    //If Android and current Orientation is landscape-primary, Activate Fullscreen.
                    ActivateFullscreen();
                }
            }
        }
    }

    void Update()
    {
        //Keep on checking the orientation.
        if (isMobile())
        {
            if (isLandScape())
            {
                isLand = true;
            }
            else if (!isLandScape())
            {
                isLand = false;
            }
        }
    }
}
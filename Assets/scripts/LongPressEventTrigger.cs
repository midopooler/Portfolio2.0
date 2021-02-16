using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using VehicleBehaviour;

public class LongPressEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    private float holdTime = 0.0001f;
    public GameObject car;
    
    //private bool held = false;
    //public UnityEvent onClick = new UnityEvent();
    
    public UnityEvent onLongPress = new UnityEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        //held = false;
        Invoke("OnLongPress", holdTime);
       
        car.GetComponent<WheelVehicle>().boostonbuttonpress();
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelInvoke("OnLongPress");
        car.GetComponent<WheelVehicle>().stopboost();
        //if (!held)
        // onClick.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelInvoke("OnLongPress");
        
        car.GetComponent<WheelVehicle>().stopboost();
    }

    private void OnLongPress()
    {
        //held = true;
        onLongPress.Invoke();
    }
}
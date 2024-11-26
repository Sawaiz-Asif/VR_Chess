
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public interface IInputHandler
{
    void ProcessInput(Vector3 inputPosition, GameObject selectObject, UnityAction callback);
    void ProcessStartDrag(Vector3 inputPosition, GameObject selectObject, UnityAction callback);
    void ProcessDrag(Vector3 inputPosition, GameObject selectObject); // For Drag
    void ProcessStopDrag(Vector3 inputPosition, GameObject selectObject); // For StopDrag

}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


public interface IInputHandler
{
    void ProcessInput(Vector3 inputPosition, GameObject selectObject, UnityAction callback);
}

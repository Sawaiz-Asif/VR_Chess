using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectTweener : MonoBehaviour
{
    void MoveTo(Transform transform, Vector3 targetPosition);
}

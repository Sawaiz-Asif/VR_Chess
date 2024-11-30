using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateSystem : MonoBehaviour
{
    public GameObject objectToRotate;
    Quaternion targetRotation;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        CheckRotation();
    }

    void CheckRotation()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            targetRotation = Quaternion.Euler(objectToRotate.transform.eulerAngles.x, objectToRotate.transform.eulerAngles.y + 90, objectToRotate.transform.eulerAngles.z);
        }
        objectToRotate.transform.rotation = targetRotation;
    }
}

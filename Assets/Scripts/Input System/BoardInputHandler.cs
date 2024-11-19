using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
// leap motion code 
using Leap;

// ...

[RequireComponent(typeof(Board))]
public class BoardInputHandler : MonoBehaviour, IInputHandler
{
    private Board board;

    private void Awake(){
        board = GetComponent<Board>();
    }

    public void ProcessInput(Vector3 inputPosition, GameObject selectObject, UnityAction callback){
        board.OnSquareSelected(inputPosition);
    }

    // leap motion code
    public void ProcessLeapInput(Frame frame)
    {
        // Handle Leap Motion input data from the Leap Frame
        if (frame.Hands.Count > 0)
        {
            Hand firstHand = frame.Hands[0]; // Use the first detected hand

            if (firstHand.IsPinching())
            {
                Vector3 pinchPosition = firstHand.GetPinchPosition();
                Debug.Log("Leap Motion Pinch Detected at: " + pinchPosition);

                // Send pinch position to Board for square selection
                board.OnSquareSelected(pinchPosition);
            }
        }
    }
    // ...
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

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
    
    public void ProcessStartDrag(Vector3 inputPosition, GameObject selectObject, UnityAction callback){
        board.OnSquareSelected(inputPosition);
        board.StartDragging();
    }
    public void ProcessDrag(Vector3 inputPosition, GameObject selectObject){
        board.Drag(inputPosition);
    }
    public void ProcessStopDrag(Vector3 inputPosition, GameObject selectObject){
        // board.StopDragging(inputPosition);
        board.OnSquareSelected(inputPosition);
    }
}

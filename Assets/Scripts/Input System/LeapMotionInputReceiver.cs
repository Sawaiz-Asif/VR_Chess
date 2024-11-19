using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Leap;

public class LeapMotionInputReceiver : InputReciever
{
    [SerializeField] private LeapProvider leapProvider; // Reference to LeapProvider
    private bool isPinching;
    private Vector3 pinchPosition;
    private Piece selectedPiece = null; // The currently pinched piece


    private void Awake()
    {
        // Ensure inputHandlers is populated
        inputHandlers = GetComponents<IInputHandler>();
        
        if (inputHandlers == null || inputHandlers.Length == 0)
        {
            Debug.LogError("No IInputHandler components found! Ensure the BoardInputHandler and other handlers are attached.");
        }
        else
        {
            Debug.Log($"Input handlers found: {inputHandlers.Length}");
        }
    }


    private void Update()
    {
        if (leapProvider != null)
        {
            Frame frame = leapProvider.CurrentFrame;
            if (frame.Hands.Count > 0)
            {
                Hand hand = frame.Hands[0]; // Use the first detected hand
                isPinching = hand.IsPinching();
                pinchPosition = hand.GetPinchPosition();

                if (true)
                {
                    OnInputRecieved();
                }
            }
        }
    }

public override void OnInputRecieved()
{
    if (leapProvider == null)
    {
        Debug.LogError("LeapProvider is null in OnInputRecieved!");
        return;
    }

    Frame frame = leapProvider.CurrentFrame;

    if (inputHandlers == null || inputHandlers.Length == 0)
    {
        Debug.LogError("No input handlers found!");
        return;
    }

    foreach (var handler in inputHandlers)
    {
        if (handler is BoardInputHandler boardInputHandler)
        {
           // Debug.Log("Processing input with BoardInputHandler...");

            if (frame.Hands.Count > 0)
            {
                Hand firstHand = frame.Hands[0];
                bool isCurrentlyPinching = firstHand.IsPinching();

                if (firstHand.IsPinching())
                {
                    Vector3 pinchPosition = firstHand.GetPinchPosition();
                    Ray ray = new Ray(Camera.main.transform.position, pinchPosition - Camera.main.transform.position);

                    if (Physics.Raycast(ray, out RaycastHit hitInfo))
                    {
                        Piece piece = hitInfo.collider.GetComponent<Piece>();

                        if (piece != null)
                        {
                            if (selectedPiece == null)
                            {
                                // Start pinching: Select the new piece
                                selectedPiece = piece;
                                selectedPiece.StartMoveWithLeapMotion();
                                Debug.Log($"Started moving piece: {selectedPiece.name}");
                            }
                            else if (selectedPiece == piece)
                            {
                                // Continue moving the already selected piece
                                selectedPiece.MoveWithLeapMotion(pinchPosition);
                            }
                            else
                            {
                                Debug.LogWarning($"Another piece detected ({piece.name}), but ignoring it since {selectedPiece.name} is already selected.");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log($"Dropped piece: {selectedPiece.name} at position {selectedPiece.transform.position}");
                    // Stop pinching: Drop the currently selected piece
                    if (selectedPiece != null)
                    {
                        selectedPiece.StopMoveWithLeapMotion();
                        Debug.Log($"Dropped piece: {selectedPiece.name} at position {selectedPiece.transform.position}");
                        selectedPiece = null; // Reset selectedPiece
                    }
                }
            }
        }
    }
}

}

using System.Collections;
using UnityEngine;
using Leap;

public class LeapMotionInputReciever : InputReciever
{
    [SerializeField] private LeapProvider leapProvider;
    private Vector3 pinchPosition;
    private bool isPinching = false;

    private void Update()
    {
        if (leapProvider == null)
        {
            Debug.LogError("LeapProvider is not assigned.");
            return;
        }

        Frame frame = leapProvider.CurrentFrame;

        if (frame.Hands.Count > 0)
        {
            Hand firstHand = frame.Hands[0];
            bool currentlyPinching = firstHand.IsPinching();
            pinchPosition = firstHand.GetPinchPosition();

            foreach (var handler in inputHandlers)
            {
                if (currentlyPinching && !isPinching)
                {
                    isPinching = true;
                    handler.ProcessStartDrag(pinchPosition, null, null);
                }
                else if (currentlyPinching)
                {
                    handler.ProcessDrag(pinchPosition, null);
                }
                else if (!currentlyPinching && isPinching)
                {
                    isPinching = false;
                    handler.ProcessStopDrag(pinchPosition, null);
                }
            }
        }
    }

    public override void OnInputRecieved() { }
}

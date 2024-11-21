using System.Collections;
using UnityEngine;
using Leap;

public class LeapMotionInputReceiver : InputReciever
{
    [SerializeField] private LeapProvider leapProvider; // Reference to LeapProvider
    private bool isPinching;
    private Vector3 pinchPosition;
    private Piece selectedPiece = null; // Currently pinched piece
    private LineRenderer lineRenderer; // For showing the landing ray

    private void Awake()
    {
        // Initialize inputHandlers from attached components
        inputHandlers = GetComponents<IInputHandler>();

        if (inputHandlers == null || inputHandlers.Length == 0)
        {
            Debug.LogWarning("No IInputHandler components found. Attach appropriate handlers like BoardInputHandler.");
        }

        SetupLineRenderer();
    }

    private void Update()
    {
        if (leapProvider == null)
        {
            Debug.LogError("LeapProvider is not assigned. Please attach a LeapProvider component.");
            return;
        }

        Frame frame = leapProvider.CurrentFrame;
        if (frame.Hands.Count > 0)
        {
            Hand firstHand = frame.Hands[0];
            pinchPosition = firstHand.GetPinchPosition();
            isPinching = firstHand.IsPinching();

            OnInputRecieved();
        }
        else
        {
            HandleNoHands();
        }
    }

    public override void OnInputRecieved()
    {
        if (isPinching)
        {
            if (selectedPiece == null)
            {
                // Try to select a piece
                if (TryRaycastToPiece(pinchPosition, out Piece piece))
                {
                    SelectPiece(piece);
                }
            }
            else
            {
                // Move the selected piece directly with Leap Motion
                MovePieceToPinchPosition(selectedPiece, pinchPosition);
                DrawLandingRay(selectedPiece);
            }
        }
        else if (selectedPiece != null)
        {
            HandleRelease();
        }
    }

    private bool TryRaycastToPiece(Vector3 position, out Piece piece)
    {
        piece = null;
        Ray ray = new Ray(Camera.main.transform.position, position - Camera.main.transform.position);
        Debug.DrawRay(ray.origin, ray.direction * 10, Color.red); // Visualize ray

        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            piece = hitInfo.collider.GetComponent<Piece>();
            if (piece != null)
            {
                Debug.Log($"Ray hit piece: {piece.name}");
                return true;
            }
        }

        Debug.Log("Ray did not hit any piece.");
        return false;
    }

    private void SelectPiece(Piece piece)
    {
        selectedPiece = piece;
        Debug.Log($"Selected piece: {selectedPiece.name}");
        selectedPiece.HighlightPiece(Color.blue);
    }

    private void MovePieceToPinchPosition(Piece piece, Vector3 targetPosition)
    {
        piece.transform.position = Vector3.Lerp(piece.transform.position, targetPosition, Time.deltaTime * 10f);
        Debug.Log($"Moving piece {piece.name} to position {targetPosition}");
    }

    private void HandleRelease()
    {
        Debug.Log($"Released piece: {selectedPiece.name}");
        Ray ray = new Ray(selectedPiece.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            // Use the ray hit position to finalize the piece's movement
            selectedPiece.MovePieceToPosition(hitInfo.point);
            Debug.Log($"Piece moved to: {hitInfo.point}");
        }
        selectedPiece.RemoveHighlight();
        selectedPiece = null; // Clear the selection
        lineRenderer.enabled = false;
    }

    private void HandleNoHands()
    {
        lineRenderer.enabled = false; // Hide the line if no hands are detected
    }

    private void DrawLandingRay(Piece piece)
    {
        Ray ray = new Ray(piece.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, piece.transform.position);
            lineRenderer.SetPosition(1, hitInfo.point);

            // Determine if the landing position is valid
            if (IsMoveValid(hitInfo.point))
            {
                lineRenderer.startColor = Color.green;
                lineRenderer.endColor = Color.green;
            }
            else
            {
                lineRenderer.startColor = Color.red;
                lineRenderer.endColor = Color.red;
            }
        }
        else
        {
            lineRenderer.enabled = false; // Hide the line if no valid target is found
        }
    }

    private bool IsMoveValid(Vector3 targetPosition)
    {
        // Add logic to check if the move is valid (e.g., based on board rules)
        // For now, we assume all moves are valid
        return true;
    }

    private void SetupLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.useWorldSpace = true;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.enabled = false;
    }
}

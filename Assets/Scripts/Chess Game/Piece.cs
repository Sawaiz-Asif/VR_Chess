using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Leap;

[RequireComponent(typeof(IObjectTweener))]
[RequireComponent(typeof(MaterialSetter))]
public abstract class Piece : MonoBehaviour
{
    private MaterialSetter materialSetter;
    public Board board { protected get; set; }
    public Vector2Int occupiedSquare { get; set; }
    public TeamColor team { get; set; }
    public bool hasMoved { get; private set; }
    public List<Vector2Int> availableMoves;

    private IObjectTweener tweener;
    private LeapProvider leapProvider; // LeapProvider reference
    private bool isPinching = false;
    private Vector3 pinchPosition;
    private bool isPieceSelected = false;
    private Piece selectedPiece = null;

    public abstract List<Vector2Int> SelectAvailableSquares();

    private void OnEnable()
    {
        LeapServiceProvider provider = FindObjectOfType<LeapServiceProvider>();
        if (provider != null)
        {
            AssignLeapProvider(provider);
            Debug.Log("LeapProvider assigned successfully in OnEnable!");
        }
        else
        {
            Debug.LogError("LeapServiceProvider not found in OnEnable!");
        }
    }
    private void Awake()
    {
        availableMoves = new List<Vector2Int>();
        tweener = GetComponent<IObjectTweener>();
        materialSetter = GetComponent<MaterialSetter>();
        hasMoved = false;
    }

    public void AssignLeapProvider(LeapProvider provider)
    {
        leapProvider = provider;
        if (leapProvider != null)
        {
            leapProvider.OnUpdateFrame += OnUpdateFrame;
        }
    }

    private void OnDisable()
    {
        if (leapProvider != null)
        {
            leapProvider.OnUpdateFrame -= OnUpdateFrame;
        }
    }
public void MovePieceToPinchPosition(Vector3 targetPosition)
{
    // Smoothly move the piece to the pinch position
    transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
}

private void OnUpdateFrame(Frame frame)
{
    if (frame.Hands.Count > 0)
    {
        Hand firstHand = frame.Hands[0];
        bool isCurrentlyPinching = firstHand.IsPinching();
        pinchPosition = firstHand.GetPinchPosition();

        if (isCurrentlyPinching)
        {
            Debug.Log("Pinch detected!");

            Ray ray = new Ray(Camera.main.transform.position, pinchPosition - Camera.main.transform.position);
            if (Physics.Raycast(ray, out RaycastHit hitInfo))
            {
                Piece piece = hitInfo.collider.GetComponent<Piece>();

                // If pinching a piece and no piece is currently selected
                if (piece != null && !isPieceSelected)
                {
                    Debug.Log($"Pinching piece: {piece.name}");
                    selectedPiece = piece;
                    isPieceSelected = true;
                }
            }

            // Move the selected piece if it's being pinched
            if (isPieceSelected && selectedPiece != null)
            {
                selectedPiece.MovePieceToPinchPosition(pinchPosition);
            }
        }
        else
        {
            // If pinch is released, stop moving the piece
            if (isPieceSelected)
            {
                Debug.Log("Pinch released, dropping piece.");
                isPieceSelected = false;
                selectedPiece = null;
            }
        }
    }
    else
    {
        Debug.Log("No hands detected.");
    }
}


    private void MovePieceToPosition(Vector3 targetPosition)
    {
        Vector2Int coords = InvokeBoardMethod<Vector2Int>("CalculateCoordsFromPosition", targetPosition);
        if (InvokeBoardMethod<bool>("CheckIfCoordinateAreOnBoard", coords) && CanMoveTo(coords))
        {
            MovePiece(coords);
        }
    }

    private T InvokeBoardMethod<T>(string methodName, params object[] parameters)
    {
        MethodInfo methodInfo = typeof(Board).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        if (methodInfo != null)
        {
            return (T)methodInfo.Invoke(board, parameters);
        }
        throw new Exception($"Method {methodName} not found in Board class.");
    }

    public void SetMaterial(Material material)
    {
        if (materialSetter == null)
            materialSetter = GetComponent<MaterialSetter>();
        materialSetter.SetSingleMaterial(material);
    }

    public bool CanMoveTo(Vector2Int coords)
    {
        return availableMoves.Contains(coords);
    }

    public bool IsFromSameTeam(Piece piece){
        return team == piece.team;
    }


    public virtual void MovePiece(Vector2Int coords)
    {
        Vector3 targetPosition = board.CalculatePositionFromCoords(coords);
        occupiedSquare = coords;
        hasMoved = true;
        tweener.MoveTo(transform, targetPosition);
    }
    public void TryToAddMove(Vector2Int coords){
        availableMoves.Add(coords);
    }

    public void SetData(Vector2Int coords, TeamColor team, Board board)
    {
        occupiedSquare = coords;
        this.team = team;
        this.board = board;
        transform.position = board.CalculatePositionFromCoords(coords);
    }
}

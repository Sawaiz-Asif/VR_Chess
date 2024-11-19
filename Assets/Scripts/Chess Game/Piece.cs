using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IObjectTweener))]
[RequireComponent(typeof(MaterialSetter))]

public abstract class Piece : MonoBehaviour
{
    private MaterialSetter materialSetter;
    public Board board {protected get; set;}
    public Vector2Int occupiedSquare { get; set;}
    public TeamColor team { get; set;}
    public bool hasMoved { get; private set; }
    public List<Vector2Int> availableMoves;

    private IObjectTweener tweener;

    public abstract List<Vector2Int> SelectAvailableSquares();


    // Leap motion Code 

    // Existing variables and methods remain as-is...

    private bool isBeingMoved = false; // Track if the piece is being moved by Leap Motion

    public void MoveWithLeapMotion(Vector3 targetPosition)
    {
        if (isBeingMoved)
        {
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
        }
    }

    public void StartMoveWithLeapMotion()
    {
        isBeingMoved = true;
    }

    public void StopMoveWithLeapMotion()
    {
        isBeingMoved = false;
    }


    // ...............

    private void Awake(){
        availableMoves = new List<Vector2Int>();
        tweener = GetComponent<IObjectTweener>();
        materialSetter = GetComponent<MaterialSetter>();
        hasMoved = false;
        // Ensure the piece has a collider
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>(); // Add a BoxCollider if none exists
            Debug.Log($"Collider added to {name}");
        }
    }

    public void SetMaterial(Material material){
        if (materialSetter == null)
            materialSetter = GetComponent<MaterialSetter>();
        materialSetter.SetSingleMaterial(material);
    }

    public bool IsFromSameTeam(Piece piece){
        return team == piece.team;
    }

    public bool CanMoveTo(Vector2Int coords){
        return availableMoves.Contains(coords);
    }

    public virtual void MovePiece(Vector2Int coords){
        Vector3 targetPosition = board.CalculatePositionFromCoords(coords);
        occupiedSquare = coords;
        hasMoved = true;
        tweener.MoveTo(transform, targetPosition);
    }

    public void TryToAddMove(Vector2Int coords){
        availableMoves.Add(coords);
    }

    public void SetData(Vector2Int coords, TeamColor team, Board board){
        occupiedSquare = coords;
        this.team = team;
        this.board = board;
        transform.position = board.CalculatePositionFromCoords(coords);
    }

    public bool IsAttackingPieceOfType<T>() where T : Piece {
        foreach (var square in availableMoves){
            if (board.GetPieceOnSquare(square) is T){
                return true;
            }
        }
        return false;
    }

    protected Piece GetPieceInDirection<T>(TeamColor team, Vector2Int direction) where T : Piece{
        for (int i = 1; i<=8; i++){
            Vector2Int nextCoords = occupiedSquare + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if (!board.CheckIfCoordinateAreOnBoard(nextCoords))
                return null;
            if (piece != null){
                if (piece.team != team || !(piece is T))
                    return null;
                else if (piece.team == team && piece is T)
                    return piece;
            }
        }
        return null;
    }
}

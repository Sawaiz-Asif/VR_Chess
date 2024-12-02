using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IObjectTweener))]
[RequireComponent(typeof(MaterialSetter))]



public abstract class Piece : MonoBehaviour
{
    private MaterialSetter materialSetter;
    public Board board { get; set;}
    public Vector2Int occupiedSquare { get; set;}
    public TeamColor team { get; set;}
    public bool hasMoved { get; private set; }
    public List<Vector2Int> availableMoves;

    private IObjectTweener tweener;

    public abstract List<Vector2Int> SelectAvailableSquares();

    // new implementation 
    private Color originalColor; // Store the original color
    public LineRenderer lineRenderer;
    private Renderer pieceRenderer;
    // Method to highlight the piece
    public void HighlightPiece(Color highlightColor)
    {
        pieceRenderer.material.color = highlightColor; // Change to highlight color
    }
    public void RemoveHighlight()
    {
        pieceRenderer.material.color = originalColor; // Revert to original color
    }
    private void SetupLineRenderer()
    {
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.positionCount = 2;
        lineRenderer.startWidth = 0.05f;
        lineRenderer.endWidth = 0.05f;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startColor = Color.green;
        lineRenderer.endColor = Color.green;
        lineRenderer.enabled = false;
    }
    public void ShowGuidedRay()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, hitInfo.point);

            // Change color based on validity
            if (CanMoveTo(board.CalculateCoordsFromPosition(hitInfo.point)))
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
            lineRenderer.SetPosition(0, transform.position);
            lineRenderer.SetPosition(1, transform.position + Vector3.down * 10f);
            lineRenderer.startColor = Color.red;
            lineRenderer.endColor = Color.red;
        }
    }

    // ...
    private void Awake(){
        availableMoves = new List<Vector2Int>();
        tweener = GetComponent<IObjectTweener>();
        materialSetter = GetComponent<MaterialSetter>();
        hasMoved = false;
        // leap motion code :Sawaiz
        pieceRenderer = GetComponent<Renderer>();
        originalColor = pieceRenderer.material.color; // Save the original color
        // Ensure the piece has a collider
        if (GetComponent<Collider>() == null)
        {
            gameObject.AddComponent<BoxCollider>(); // Add a BoxCollider if none exists
        }
        // Initialize the LineRenderer
        SetupLineRenderer();
        // ...
    }

    public void SetMaterial(Material material){
        if (materialSetter == null)
            materialSetter = GetComponent<MaterialSetter>();
        materialSetter.SetSingleMaterial(material);
        // Capture the assigned color
        originalColor = material.color;
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

    public void SetPosition(Vector2Int coords, Quaternion rotationinput){
        transform.position = board.CalculatePositionFromCoords(coords);
        transform.rotation = rotationinput;
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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;

    private Piece[,] grid;
    private Piece selectedPiece;
    private ChessGameController chessController;

    private void Awake(){
        CreateGrid();
    }

    public void setDependencies(ChessGameController chessController){
        this.chessController = chessController;
    }

    private void CreateGrid(){
        grid = new Piece[8,8];
    }

    public Vector3 CalculatePositionFromCoords(Vector2Int coords){
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0f, coords.y * squareSize);
        
    }

    private Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition){
        int x = Mathf.FloorToInt(inputPosition.x / squareSize) + 4;
        int y = Mathf.FloorToInt(inputPosition.z / squareSize) + 4;
        return new Vector2Int(x, y);
    }

    public void OnSquareSelected(Vector3 inputPosition){
        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        if(selectedPiece){
            if(piece != null && selectedPiece == piece)
                DeselectPiece();
            else if(piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team))
                SelectPiece(piece);
            else if(selectedPiece.CanMoveTo(coords))
                OnSelectedPieceMoved(coords, selectedPiece);
        }
        else{
            if(piece != null && chessController.IsTeamTurnActive(piece.team)){
                SelectPiece(piece);
            }
        }
    }

    private void EndTurn(){
        chessController.EndTurn();
    }

    private void DeselectPiece(){
        selectedPiece = null;
    }

    private void SelectPiece(Piece piece){
        Debug.Log("piece selected");
        selectedPiece = piece;
        foreach(var item in selectedPiece.availableMoves){
            Debug.Log("x-" + item.x.ToString() + ", y-" + item.y.ToString());
        }
    }
    
    private void OnSelectedPieceMoved(Vector2Int coords, Piece piece){
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(coords);
        DeselectPiece();
        EndTurn();
    }

    private void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece){
        grid[oldCoords.x, oldCoords.y] = oldPiece;
        grid[newCoords.x, newCoords.y] = newPiece;
    }

    public Piece GetPieceOnSquare(Vector2Int coords){
        if(CheckIfCoordinateAreOnBoard(coords))
            return grid[coords.x, coords.y];
        return null;
    }

    public bool CheckIfCoordinateAreOnBoard(Vector2Int coords){
        if(coords.x < 0 || coords.y < 0 || coords.x >= 8 || coords.y >= 8)
            return false;
        return true;
    }

    public bool HasPiece(Piece piece){
        for(int i = 0; i < 8; i++) {
            for(int j = 0; j < 8; j++) {
                if(grid[i,j] == piece){
                    return true;
                }
            }          
        }
        return false;
    }

    public void SetPieceOnBoard(Vector2Int coords, Piece piece){
        if (CheckIfCoordinateAreOnBoard(coords)){
            grid[coords.x, coords.y] = piece;
        }    

    }
    
}

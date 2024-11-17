using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SquareSelectorCreator))]

public class Board : MonoBehaviour
{
    [SerializeField] private Transform bottomLeftSquareTransform;
    [SerializeField] private float squareSize;

    private Piece[,] grid;
    private Piece selectedPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;

    private void Awake(){
        squareSelector = GetComponent<SquareSelectorCreator>();
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
        if (!chessController.IsGameInProgress()){
            return;
        }
        Vector2Int coords = CalculateCoordsFromPosition(inputPosition);
        Piece piece = GetPieceOnSquare(coords);
        Debug.Log("GetPieceOnSquare done");
        if(selectedPiece){
            if(piece != null && selectedPiece == piece){
                DeselectPiece();
                Debug.Log("DeselectPiece done");
            }
            else if(piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team)){
                SelectPiece(piece);
                Debug.Log("SelectPiece done");
            }
            else if(selectedPiece.CanMoveTo(coords)){
                OnSelectedPieceMoved(coords, selectedPiece);
                Debug.Log("OnSelectedPieceMoved done");
            }
        }
        else{
            if(piece != null && chessController.IsTeamTurnActive(piece.team)){
                SelectPiece(piece);
                Debug.Log("SelectPiece done");
            }
        }
    }

    private void EndTurn(){
        chessController.EndTurn();
    }

    private void DeselectPiece(){
        selectedPiece = null;
        squareSelector.ClearSelection();
    }

    private void SelectPiece(Piece piece){
        //Debug.Log("piece selected");
        chessController.RemoveMovesEnablingAttackOnPieceOfType<King>(piece);
        selectedPiece = piece;
        //foreach(var item in selectedPiece.availableMoves){
        //    Debug.Log("x-" + item.x.ToString() + ", y-" + item.y.ToString());
        //}
        List<Vector2Int> selection = selectedPiece.availableMoves;
        ShowSelectionSquares(selection);
    }
    
    private void ShowSelectionSquares(List<Vector2Int> selection){
        Dictionary<Vector3, bool> squaresData = new Dictionary<Vector3, bool>();
        for (int i = 0; i < selection.Count; i++){
            Vector3 position = CalculatePositionFromCoords(selection[i]);
            bool isSquareFree = GetPieceOnSquare(selection[i]) == null;
            squaresData.Add(position, isSquareFree);
        }
        squareSelector.ShowSelection(squaresData);
    }   

    private void OnSelectedPieceMoved(Vector2Int coords, Piece piece){
        TryToTakeOppositePiece(coords);
        UpdateBoardOnPieceMove(coords, piece.occupiedSquare, piece, null);
        selectedPiece.MovePiece(coords);
        DeselectPiece();
        EndTurn();
    }

    private void TryToTakeOppositePiece(Vector2Int coords){
        Piece piece = GetPieceOnSquare(coords);
        if (piece != null && !selectedPiece.IsFromSameTeam(piece)){
            TakePiece(piece);
        }
    }

    private void TakePiece(Piece piece){
        if (piece){
            grid[piece.occupiedSquare.x, piece.occupiedSquare.y] = null;
            chessController.OnPieceRemoved(piece);
        }
    }

    public void UpdateBoardOnPieceMove(Vector2Int newCoords, Vector2Int oldCoords, Piece newPiece, Piece oldPiece){
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

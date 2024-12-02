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
    private Piece dragPiece;
    private ChessGameController chessController;
    private SquareSelectorCreator squareSelector;

    // Drag and drop logic for the leap motion is added here :sawaiz
    private bool isBeingDragged = false;
    public void StartDragging()
    {
        if(selectedPiece != null)
        {
            selectedPiece.lineRenderer.enabled = true;
            isBeingDragged = true;
        }
    }

    public void Drag(Vector3 pointerPosition)
    {
        if (isBeingDragged && selectedPiece !=null )
        {
            selectedPiece.transform.position = pointerPosition;
            selectedPiece.ShowGuidedRay();
        }
    }
    private void movePieceBack()
    {
        
        Ray ray = new Ray(selectedPiece.transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hitInfo))
        {
            if (!selectedPiece.CanMoveTo(CalculateCoordsFromPosition(hitInfo.point))){
                selectedPiece.transform.position = CalculatePositionFromCoords(selectedPiece.occupiedSquare); // Reset to original position
            }
        }
        else
        {
            selectedPiece.transform.position = CalculatePositionFromCoords(selectedPiece.occupiedSquare); // Reset to original position
        }
    }
    // ...

    public void Awake(){
        Debug.Log("Board awake");
        squareSelector = GetComponent<SquareSelectorCreator>();
        CreateGrid();
    }

    private void Update(){
        for (int i=0; i<8; i++){
            for (int j=0; j<8; j++){
                Piece piece = grid[i,j];
                if (piece != null){
                    // SetPieceOnBoard(piece.occupiedSquare, piece);
                    piece.SetPosition(piece.occupiedSquare, this.transform.rotation);
                }
            }
        }       
    }

    public void setDependencies(ChessGameController chessController){
        this.chessController = chessController;
    }

    private void CreateGrid(){
        grid = new Piece[8,8];
    }

    public Vector3 CalculatePositionFromCoords(Vector2Int coords){
        return bottomLeftSquareTransform.position + new Vector3(coords.x * squareSize, 0.0f, coords.y * squareSize);
    }

    public Vector2Int CalculateCoordsFromPosition(Vector3 inputPosition){
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
        if(selectedPiece){
            if(piece != null && selectedPiece == piece){  // if piece is selected and trying to drop on the same location where piece was before :sawaiz
                DeselectPiece();
            }
            else if(piece != null && selectedPiece != piece && chessController.IsTeamTurnActive(piece.team)){ 
                if(isBeingDragged) // IN CASE OF DRAG AND DROP WE CAN NOT SELECT TWO PIECE AT ONCE // ONLY FOR LEAP MOTION :sawaiz  
                {
                    DeselectPiece();
                }
                else{ // IN CASE OF MOUSE WE SELECT OTHER PIECE 
                    SelectPiece(piece);
                }
            }
            else if(selectedPiece.CanMoveTo(coords)){ // in case of we have selected the empty square or other player piece :sawaiz
                OnSelectedPieceMoved(coords, selectedPiece);
            }
            else if(isBeingDragged) // ONLY FOR LEAP MOTION :sawaiz  
            {
                DeselectPiece();
            }
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
        selectedPiece.RemoveHighlight();
        if(isBeingDragged){ // ONLY FOR LEAP MOTION :sawaiz  
            selectedPiece.lineRenderer.enabled = false; // Disable the ray
            movePieceBack(); // leap
            isBeingDragged = false; //leap
        }
        selectedPiece = null;
        squareSelector.ClearSelection();
    }

    private void SelectPiece(Piece piece){
        if(selectedPiece != null)
        {
            selectedPiece.RemoveHighlight();
        }
        chessController.RemoveMovesEnablingAttackOnPieceOfType<King>(piece);
        selectedPiece = piece;
        selectedPiece.HighlightPiece(Color.blue);
        List<Vector2Int> selection = selectedPiece.availableMoves;
        ShowSelectionSquares(selection);
        if(isBeingDragged){ // ONLY FOR LEAP MOTION :sawaiz    
            selectedPiece.lineRenderer.enabled = true;
        }
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
    
    public void PromotePiece(Piece piece){
        TakePiece(piece);
        chessController.CreatePieceAndInitialize(piece.occupiedSquare, piece.team, typeof(Queen));
    }
}

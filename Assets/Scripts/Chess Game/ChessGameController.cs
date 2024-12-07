using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting;

[RequireComponent(typeof(PieceCreator))]

public class ChessGameController : MonoBehaviour
{
    private enum GameState {Init, Play, Finished}
    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;
    private PieceCreator pieceCreator;

    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer activePlayer;
    private GameState state;

    private Vector3 WhiteOut = new Vector3(5, 0, -5);
    private int WC = 0;
    private Vector3 BlackOut = new Vector3(-5, 0, 5);
    private int BC = 0;

    [SerializeField] private DisplayTimer WhiteTimer;
    [SerializeField] private DisplayTimer BlackTimer;

    bool isFirstMove = true;

    private void Awake(){
        setDependencies();
        CreatePlayers();
    }

    private void setDependencies(){
        pieceCreator = GetComponent<PieceCreator>();
    }

    private void CreatePlayers(){
        whitePlayer = new ChessPlayer(TeamColor.White, board);
        blackPlayer = new ChessPlayer(TeamColor.Black, board);
    }

    void Start()
    {
        StartNewGame();        
    }

    private void StartNewGame()
    {
        SetGameState(GameState.Init);
        board.setDependencies(this);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);
        SetGameState(GameState.Play);
    }

    private void SetGameState(GameState state){
        this.state = state;
    }

    public bool IsGameInProgress(){
        return state == GameState.Play;
    }
    
    private void CreatePiecesFromLayout(BoardLayout layout){
        
        for (int i = 0; i < layout.GetPiecesCount(); i++){
            Vector2Int squareCoords = layout.GetSquareCoordsAtIndex(i);
            TeamColor team = layout.GetSquareTeamColorAtIndex(i);
            string typeName = layout.GetSquarePieceNameAtIndex(i);
            
            Type type = Type.GetType(typeName);
            CreatePieceAndInitialize(squareCoords, team, type);
        }
    }

    public void EndTurn(){
        GenerateAllPossiblePlayerMoves(activePlayer);
        GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        if (CheckIfGameIsFinished()){
            EndGame();
        } else {
            if (isFirstMove)
            {
                BlackTimer.StartTimer();
                isFirstMove = false;
            }
            else if(activePlayer.team == TeamColor.White){
                WhiteTimer.StopTimer(false);
                BlackTimer.StartTimer();
            }
            else{
                BlackTimer.StopTimer(false);
                WhiteTimer.StartTimer();
                
            }
            ChangeActiveTeam();

        }
    }

    private bool CheckIfGameIsFinished(){
        Piece[] kingAttackingPieces = activePlayer.GetPiecesAttackingOppositePieceOfType<King>();
        if (kingAttackingPieces.Length > 0){
            ChessPlayer oppositePlayer = GetOpponentToPlayer(activePlayer);
            Piece attackedKing = oppositePlayer.GetPiecesPieceOfType<King>().FirstOrDefault();
            oppositePlayer.RemoveMovesEnablingAttackOnPiece<King>(activePlayer, attackedKing);

            int availableKingMoves = attackedKing.availableMoves.Count;
            if (availableKingMoves == 0) {
                bool canCoverKing = oppositePlayer.CanHidePieceFromAttacking<King>(activePlayer);
                if (!canCoverKing){
                    return true;
                }
            }
        }
        return false;
    }

    private void EndGame(){
        Debug.Log("Game Ended");
        WhiteTimer.StopTimer(true);
        BlackTimer.StopTimer(true);
        SetGameState(GameState.Finished);
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player){
        return player.team == whitePlayer.team ? blackPlayer : whitePlayer;
    }

    private void ChangeActiveTeam(){
        activePlayer = activePlayer.team == whitePlayer.team ? blackPlayer : whitePlayer;
    }

    public bool IsTeamTurnActive(TeamColor team){
        return activePlayer.team == team;
    }

    public void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type){
        Piece newPiece = pieceCreator.CreatePiece(type).GetComponent<Piece>();
        newPiece.SetData(squareCoords, team, board);

        Material teamMaterial = pieceCreator.GetTeamMaterial(team);
        newPiece.SetMaterial(teamMaterial);

        board.SetPieceOnBoard(squareCoords, newPiece);

        ChessPlayer currentPlayer = team == TeamColor.White ? whitePlayer : blackPlayer;
        currentPlayer.AddPiece(newPiece);
    }

    private void GenerateAllPossiblePlayerMoves(ChessPlayer player){
        player.GenerateAllPossibleMoves();
    }

    public void RemoveMovesEnablingAttackOnPieceOfType<T>(Piece piece) where T : Piece {
        activePlayer.RemoveMovesEnablingAttackOnPiece<T>(GetOpponentToPlayer(activePlayer), piece);
    }

    public void OnPieceRemoved(Piece piece){
        ChessPlayer pieceOwner = (piece.team == TeamColor.White) ? whitePlayer : blackPlayer;
        pieceOwner.RemovePiece(piece);
        
        Vector3 moveOut = new Vector3(0, 0, 0);

        if(piece.team == TeamColor.White){
            if(WC % 3 == 0){
                moveOut = WhiteOut + 2 * Vector3.forward + Vector3.right;
                WhiteOut = moveOut;
                WC++;
            }
            else{
                moveOut = WhiteOut + Vector3.back;
                WhiteOut = moveOut;
                WC++;
            }
            piece.ReMovePiece(moveOut);
        }
        else{
            if(BC % 3 == 0){
                moveOut = BlackOut + 2 * Vector3.back + Vector3.left;
                BlackOut = moveOut;
                BC++;
            }
            else{
                moveOut = BlackOut + Vector3.forward;
                BlackOut = moveOut;
                BC++;
            }
            piece.ReMovePiece(moveOut);
        }
        
    }
}
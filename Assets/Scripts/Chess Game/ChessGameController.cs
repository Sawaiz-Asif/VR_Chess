using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PieceCreator))]

public class ChessGameController : MonoBehaviour
{
    [SerializeField] private BoardLayout startingBoardLayout;
    [SerializeField] private Board board;
    private PieceCreator pieceCreator;

    private ChessPlayer whitePlayer;
    private ChessPlayer blackPlayer;
    private ChessPlayer activePlayer;
    

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
        board.setDependencies(this);
        CreatePiecesFromLayout(startingBoardLayout);
        activePlayer = whitePlayer;
        GenerateAllPossiblePlayerMoves(activePlayer);

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
        //GenerateAllPossiblePlayerMoves(GetOpponentToPlayer(activePlayer));
        ChangeActiveTeam();
        GenerateAllPossiblePlayerMoves(activePlayer);
        
    }

    private ChessPlayer GetOpponentToPlayer(ChessPlayer player){
        return player == whitePlayer ? blackPlayer : whitePlayer;
    }

    private void ChangeActiveTeam(){
        activePlayer = activePlayer.team == whitePlayer.team ? blackPlayer :  whitePlayer;
    }

    public bool IsTeamTurnActive(TeamColor team){
        return activePlayer.team == team;
    }

    private void CreatePieceAndInitialize(Vector2Int squareCoords, TeamColor team, Type type){
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
}

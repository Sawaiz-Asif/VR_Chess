using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pawn : Piece
{
    public override List<Vector2Int> SelectAvailableSquares(){
        availableMoves.Clear();
        Vector2Int direction = team == TeamColor.White ? new Vector2Int(0, 1) : new Vector2Int(0, -1);
        float range = hasMoved ? 1 : 2;
        for (int i = 1; i <= range; i++){
            Vector2Int nextCoords = occupiedSquare + direction * i;
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if(!board.CheckIfCoordinateAreOnBoard(nextCoords)){
                break;
            }
            if (piece == null) {
                TryToAddMove(nextCoords);
            } else if (piece.IsFromSameTeam(this)){
                break;
            }
        }

        Vector2Int[] takeDirections = new Vector2Int[] {new Vector2Int(1, direction.y), new Vector2Int(-1, direction.y)};
        for (int i = 0; i < takeDirections.Length; i++){
            Vector2Int nextCoords = occupiedSquare + takeDirections[i];
            Piece piece = board.GetPieceOnSquare(nextCoords);
            if(!board.CheckIfCoordinateAreOnBoard(nextCoords)){
                break;
            }
            if (piece != null && !piece.IsFromSameTeam(this)){
                TryToAddMove(nextCoords);
            }
        }
        return availableMoves;
    }

    public override void MovePiece(Vector2Int coords)
    {
        base.MovePiece(coords);
        CheckPromotion();
    }

    private void CheckPromotion(){
        int endOfBoardYCoordinate = team == TeamColor.White ? 7 : 0;
        if (occupiedSquare.y == endOfBoardYCoordinate){
            board.PromotePiece(this);
            Destroy(this.gameObject);
        }
    }
}


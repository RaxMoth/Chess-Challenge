using ChessChallenge.API;
using System;

public class MyBot : IChessBot
{
    // Piece values: null, pawn, knight, bishop, rook, queen, king
    private int[] pieceValues = { 0, 1, 3, 3, 5, 9, 10 };

    public Move Think(Board board, Timer timer)
    {
        Move[] allMoves = board.GetLegalMoves();

        // Initialize move to play
        Move moveToPlay = new Move();
        int highestValueCapture = 0;

        foreach (Move move in allMoves)
        {
           
            
            // Always play checkmate in one
            if (MoveIsCheckmate(board, move))
            {
                moveToPlay = move;
                break;
            }
             if (move.IsPromotion && (int)move.PromotionPieceType == 5)
            {
                return move;
            }

        
            Piece capturedPiece = board.GetPiece(move.TargetSquare);
            int capturedPieceValue = pieceValues[(int)capturedPiece.PieceType];

            if (capturedPieceValue > highestValueCapture && BeneficialCapture(board, move))
            {
                moveToPlay = move;
                highestValueCapture = capturedPieceValue;
            }
        }


        // If no defensive moves found, find a safe move
        if (moveToPlay.IsNull)
        {
            Random rng = new();
            for (int i = 0; i < allMoves.Length; i++)
            {
                moveToPlay = allMoves[rng.Next(allMoves.Length)];
                if (SafeMove(board, moveToPlay))
                {
                    break;
                }
            }
        }

        Piece movingPiece = board.GetPiece(moveToPlay.StartSquare);
        int movingPieceValue = pieceValues[(int)movingPiece.PieceType];
        Console.WriteLine($"Chosen move: {moveToPlay}, Moving piece value: {movingPieceValue}");

        return moveToPlay;
    }

    // Test if this move gives checkmate
    bool MoveIsCheckmate(Board board, Move move)
    {
        board.MakeMove(move);
        bool isMate = board.IsInCheckmate();
        board.UndoMove(move);
        return isMate;
    }

    // Check if a capture is beneficial
    bool BeneficialCapture(Board board, Move move)
    {
        // Check the value of our piece
        Piece ourPiece = board.GetPiece(move.StartSquare);
        int ourPieceValue = pieceValues[(int)ourPiece.PieceType];

        // Execute the move
        board.MakeMove(move);

        // Get all of the opponent's legal moves
        Move[] opponentMoves = board.GetLegalMoves();

        bool isBeneficial = true;

        foreach (Move opponentMove in opponentMoves)
        {
            // If the opponent can capture our piece on their next move, check the value of the capture
            if (opponentMove.TargetSquare == move.TargetSquare)
            {
                Piece capturingPiece = board.GetPiece(opponentMove.StartSquare);  // Change this to StartSquare to get the piece that will capture us.
                int capturingPieceValue = pieceValues[(int)capturingPiece.PieceType];

                // If the value of the capturing piece is less than the value of our piece, then the capture is not beneficial
                if (capturingPieceValue < ourPieceValue)
                {
                    isBeneficial = false;
                }
            }
        }

        // Revert the move before returning
        board.UndoMove(move);
        return isBeneficial;
    }

    // Check if a move is safe
    bool SafeMove(Board board, Move move)
    {
        // Execute the move
        board.MakeMove(move);

        // Get all of the opponent's legal moves
        Move[] opponentMoves = board.GetLegalMoves();

        // Assume the move is safe until proven otherwise
        bool isSafe = true;

        foreach (Move opponentMove in opponentMoves)
        {
            // If the opponent can capture our piece on their next move, the move is not safe
            if (opponentMove.TargetSquare == move.TargetSquare)
            {
                isSafe = false;
                break;
            }
        }

        // Revert the move before returning
        board.UndoMove(move);
        return isSafe;
    }

  
}

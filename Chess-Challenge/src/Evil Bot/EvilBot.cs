using ChessChallenge.API;
using System;
using System.Linq;

namespace ChessChallenge.Example
{
    public class EvilBot : IChessBot
    {
        // Piece values: null, pawn, knight, bishop, rook, queen, king
        private readonly int[] pieceValues = { 0, 1, 3, 3, 5, 9, 10 };

        public Move Think(Board board, Timer timer)
        {
            var allMoves = board.GetLegalMoves().OrderByDescending(m => MoveValue(board, m)).ThenBy(m => Guid.NewGuid()).ToArray();  // Order by move value, then shuffle equally valued moves
            Move bestMove = allMoves[0];
            int bestValue = int.MinValue;

            foreach (Move move in allMoves)
            {
                board.MakeMove(move);
                int moveValue = Minimax(board, 4, bestValue, int.MaxValue, true);
                board.UndoMove(move);

                if (moveValue > bestValue)
                {
                    bestValue = moveValue;
                    bestMove = move;
                }
            }

            var movingPiece = board.GetPiece(bestMove.StartSquare);
            int movingPieceValue = pieceValues[(int)movingPiece.PieceType];
            Console.WriteLine($"Chosen move: {bestMove}, Moving piece value: {movingPieceValue}");

            return bestMove;
        }

        int MoveValue(Board board, Move move)
        {
            if (MoveIsCheckmate(board, move))
                return int.MaxValue;
            if (BeneficialCapture(board, move))
                return pieceValues[(int)board.GetPiece(move.TargetSquare).PieceType] - pieceValues[(int)board.GetPiece(move.StartSquare).PieceType];
            if (SafeMove(board, move))
                return 0;
            return int.MinValue;
        }

        bool MoveIsCheckmate(Board board, Move move)
        {
            board.MakeMove(move);
            var isMate = board.IsInCheckmate();
            board.UndoMove(move);
            return isMate;
        }

        bool BeneficialCapture(Board board, Move move)
        {
            var ourPiece = board.GetPiece(move.StartSquare);
            var ourPieceValue = pieceValues[(int)ourPiece.PieceType];

            if (board.GetPiece(move.TargetSquare) == null)
                return false;

            var theirPiece = board.GetPiece(move.TargetSquare);
            var theirPieceValue = pieceValues[(int)theirPiece.PieceType];

            return ourPieceValue <= theirPieceValue;
        }

        bool SafeMove(Board board, Move move)
        {
            board.MakeMove(move);
            var isSafe = !board.GetLegalMoves().Any(m => m.TargetSquare == move.TargetSquare);
            board.UndoMove(move);

            return isSafe;
        }

        int Minimax(Board board, int depth, int alpha, int beta, bool maximizingPlayer)
        {
            if (depth == 0)
            {
                return EvaluateBoard(board);
            }

            var moves = board.GetLegalMoves().OrderByDescending(m => MoveValue(board, m)).ThenBy(m => Guid.NewGuid()).ToArray();  // Order by move value, then shuffle equally valued moves

            if (maximizingPlayer)
            {
                int maxEval = int.MinValue;

                foreach (var move in moves)
                {
                    board.MakeMove(move);
                    int eval = Minimax(board, depth - 1, alpha, beta, false);
                    board.UndoMove(move);
                    maxEval = Math.Max(maxEval, eval);

                    alpha = Math.Max(alpha, eval);
                    if (beta <= alpha)
                        break;
                }
                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;

                foreach (var move in moves)
                {
                    board.MakeMove(move);
                    int eval = Minimax(board, depth - 1, alpha, beta, true);
                    board.UndoMove(move);
                    minEval = Math.Min(minEval, eval);

                    beta = Math.Min(beta, eval);
                    if (beta <= alpha)
                        break;
                }
                return minEval;
            }
        }

        int EvaluateBoard(Board board)
        {
            int score = 0;
            foreach (var pieceList in board.GetAllPieceLists())
            {
                foreach (var piece in pieceList)
                {
                    // Adjust the score based on the piece color
                    int pieceScore = pieceValues[(int)piece.PieceType];
                    score += piece.IsWhite ? pieceScore : -pieceScore;
                }
            }
            return score;
        }
    }
}

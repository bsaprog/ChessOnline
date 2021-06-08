using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using System;

namespace ChessLogic
{
    public class ChessGame
    {
        private ChessGameState _state;
        private readonly ChessBoard _board;
        private readonly FigureMoveManager _moveManager;

        public ChessGame(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            _board = new ChessBoard(fen);
            _state = new ChessGameState(fen);
            _moveManager = new FigureMoveManager(_board, _state);
        }

        internal void UpdateState(ChessGameState state)
        {
            _state = state;
        }

        public bool MakeMove(string path)
        {
            return _moveManager.MakeMove(path);
        }

        public string GetStateString()
        {
            return
                $"Turn #{_state.Turn}. Turn owner: {_state.TurnOwner}. Rule of 50: {_state.RuleOf50}\n" +
                $"CastlingAvailable:\n" +
                $"  White King: {_state.WhiteKingCastlingAvailable}\n" +
                $"  White Queen: {_state.WhiteQueenCastlingAvailable}\n" +
                $"  Black King: {_state.BlackKingCastlingAvailable}\n" +
                $"  Black Queen: {_state.BlackQueenCastlingAvailable}\n" +
                $"Pawn on the pass: {ChessUtils.GetAddresFromPosition(_state.PawnOnThePassant.HitPosition)}";
        }

        public List<(KnownColor, string)> GetBoardTextTuple()
        {
            var result = new List<(KnownColor, string)>();

            result.Add((KnownColor.DarkGray, "  +---------------+\n"));
            
            for(int y = 8; y > 0; y--)
            {
                result.Add((KnownColor.DarkGray, $" {y}|"));
                for (int x = 1; x <= 8; x++)
                {
                    Figure figure = _board.GetCellByPosition(new Vector2(x-1, y-1)).Figure;
                    if (figure == null)
                    {
                        result.Add((KnownColor.White, " "));
                    }
                    else
                    {
                        result.Add((figure.Color == KnownColor.White ? KnownColor.White : KnownColor.DarkRed, ((Char)figure.Type).ToString()));
                    }
                    result.Add((KnownColor.DarkGray, "|"));
                }
                result.Add((KnownColor.DarkGray, "\n"));
            }

            result.Add((KnownColor.DarkGray, "  +---------------+\n"));
            result.Add((KnownColor.DarkGray, "   a b c d e f g h\n"));

            return result;
        }
    }
}
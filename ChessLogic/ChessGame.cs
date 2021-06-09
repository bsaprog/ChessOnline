using System.Collections.Generic;
using System.Numerics;
using System.Drawing;
using System;

namespace ChessLogic
{
    public class ChessGame
    {
        private readonly ChessBoard _board;
        private readonly FigureMoveManager _moveManager;

        public ChessGame(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            _board = new ChessBoard(fen);
            _moveManager = new FigureMoveManager(_board, new ChessGameState(fen));
        }

        public bool MakeMove(string path)
        {
            return _moveManager.MakeMove(path);
        }

        public bool MakeRandomMove()
        {
            return _moveManager.MakeRandomMove();
        }

        public List<(KnownColor, string)> GetGameTextTuple()
        {
            return ChessUtils.GetGameTextTuple(_board, _moveManager.State);
        }
    }
}
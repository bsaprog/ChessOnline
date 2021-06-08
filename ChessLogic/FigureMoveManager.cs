using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace ChessLogic
{
    internal class FigureMoveManager
    {
        private readonly ChessBoard _board;
        private readonly ChessGameState _state;
        private readonly Stack<FigureMove> _history;
        private readonly List<FigureMove> _moveVariants;

        internal FigureMoveManager(ChessBoard board, ChessGameState state)
        {
            _board = board;
            _state = state;
            _history = new Stack<FigureMove>();
            _moveVariants = new List<FigureMove>();
        }

        internal void RefreshMoveVariants()
        {
            _moveVariants.Clear();
            List<ChessBoardCell> cells = _board.GetCellsWithFigures();
            foreach (ChessBoardCell cell in cells)
            {
                _moveVariants.AddRange(GetMoveVariantsFromCell(cell));
            }

            /*
            List<FigureMove> forRemove = new List<FigureMove>();

            foreach (var variant in _moveVariants)
            {
                ChessBoard checkBoard = new ChessBoard(_board.ToString());
                FigureMoveManager checkMoveManager = new FigureMoveManager(checkBoard, _state);

                ChessBoardCell start = checkBoard.GetCellByPosition(variant.Start.Position);
                ChessBoardCell end = checkBoard.GetCellByPosition(variant.End.Position);

                end.SetFigure(start.Figure);
                start.SetFigure(null);

                checkMoveManager.RefreshMoveVariants(deep);

                if (checkMoveManager.KingIsUnderAttack(ChessUtils.InvertColor(_state.TurnOwner)))
                {
                    forRemove.Add(variant);
                }
            }

            _moveVariants.RemoveAll(v => forRemove.Contains(v));
            */

        }

        private bool KingIsUnderAttack(KnownColor color)
        {
            ChessBoardCell cell = _board.GetCellWithKing(color);
            return _moveVariants.Where(v => v.End == cell).Count() != 0;
        }

        private List<FigureMove> GetMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();

            if (startCell != null && startCell.Figure != null)
            {
                switch (startCell.Figure.Type)
                {
                    case FigureType.Pawn:
                        result.AddRange(GetPawnMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.Knight:
                        result.AddRange(GetKnightMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.Bishop:
                        result.AddRange(GetBishopMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.Rook:
                        result.AddRange(GetRockMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.Queen:
                        result.AddRange(GetBishopMoveVariantsFromCell(startCell));
                        result.AddRange(GetRockMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.King:
                        result.AddRange(GetKingMoveVariantsFromCell(startCell));
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        private List<FigureMove> GetMoveVariantsFromCells(List<ChessBoardCell> cells)
        {
            List<FigureMove> result = new List<FigureMove>();

            foreach (ChessBoardCell cell in cells)
            {
                result.AddRange(GetMoveVariantsFromCell(cell));
            }

            return result;
        }

        private List<FigureMove> GetPawnMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();

            int direction = startCell.Figure.Color == KnownColor.White ? 1 : -1;
            bool pawnIsOnStartPosition = startCell.Figure.Color == KnownColor.White ? startCell.Position.Y == 1 : startCell.Position.Y == 6;

            ChessBoardCell endCellFrontDirection = _board.GetCellByPosition(startCell.Position + new Vector2(0, direction));
            ChessBoardCell endCellPassantDirection = _board.GetCellByPosition(startCell.Position + new Vector2(0, 2 * direction));
            ChessBoardCell endCellFrontRightDirection = _board.GetCellByPosition(startCell.Position + new Vector2(1, direction));
            ChessBoardCell endCellFronLeftDirection = _board.GetCellByPosition(startCell.Position + new Vector2(-1, direction));

            if (endCellFrontDirection != null && endCellFrontDirection.Figure == null)
            {
                result.Add(new FigureMove(startCell, endCellFrontDirection));

                if (endCellPassantDirection != null && endCellPassantDirection.Figure == null && pawnIsOnStartPosition)
                {
                    result.Add(new FigureMove(startCell, endCellPassantDirection));
                }
            }

            if (endCellFrontRightDirection != null && endCellFrontDirection.Figure != null && endCellFrontDirection.Figure.Color != startCell.Figure.Color)
            {
                result.Add(new FigureMove(startCell, endCellFrontRightDirection));
            }

            if (endCellFronLeftDirection != null && endCellFronLeftDirection.Figure != null && endCellFronLeftDirection.Figure.Color != startCell.Figure.Color)
            {
                result.Add(new FigureMove(startCell, endCellFronLeftDirection));
            }

            return result;
        }

        private List<FigureMove> GetKnightMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();
            List<ChessBoardCell> endCells = new List<ChessBoardCell>()
            {
                _board.GetCellByPosition(startCell.Position + new Vector2(1, 2)),
                _board.GetCellByPosition(startCell.Position + new Vector2(2, 1)),
                _board.GetCellByPosition(startCell.Position + new Vector2(1, -2)),
                _board.GetCellByPosition(startCell.Position + new Vector2(2, -1)),
                _board.GetCellByPosition(startCell.Position + new Vector2(-1, 2)),
                _board.GetCellByPosition(startCell.Position + new Vector2(-2, 1)),
                _board.GetCellByPosition(startCell.Position + new Vector2(-1, -2)),
                _board.GetCellByPosition(startCell.Position + new Vector2(-2, -1))
            };

            foreach (ChessBoardCell endCell in endCells)
            {
                if (endCell != null)
                {
                    if (endCell.Figure == null)
                    {
                        result.Add(new FigureMove(startCell, endCell));
                    }
                    else if (endCell.Figure != null && endCell.Figure.Color != startCell.Figure.Color)
                    {
                        result.Add(new FigureMove(startCell, endCell));
                    }
                }
            }

            return result;
        }

        private List<FigureMove> GetBishopMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();
            List<Vector2> directions = new List<Vector2>
            {
                new Vector2(1, 1),
                new Vector2(1, -1),
                new Vector2(-1, 1),
                new Vector2(-1, -1)
            };

            foreach (Vector2 direction in directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    ChessBoardCell endCell = _board.GetCellByPosition(startCell.Position + direction * i);
                    if (endCell == null)
                    {
                        break;
                    }
                    else
                    {
                        if (endCell.Figure == null)
                        {
                            result.Add(new FigureMove(startCell, endCell));
                        }
                        else if (endCell.Figure.Color != startCell.Figure.Color)
                        {
                            result.Add(new FigureMove(startCell, endCell));
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }
        private List<FigureMove> GetRockMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();

            List<Vector2> directions = new List<Vector2>
            {
                new Vector2(1, 0),
                new Vector2(0, 1),
                new Vector2(-1, 0),
                new Vector2(0, -1)
            };

            foreach (Vector2 direction in directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    ChessBoardCell endCell = _board.GetCellByPosition(startCell.Position + direction * i);
                    if (endCell == null)
                    {
                        break;
                    }
                    else
                    {
                        if (endCell.Figure == null)
                        {
                            result.Add(new FigureMove(startCell, endCell));
                        }
                        else if (endCell.Figure.Color != startCell.Figure.Color)
                        {
                            result.Add(new FigureMove(startCell, endCell));
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }
        private List<FigureMove> GetKingMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();
            List<ChessBoardCell> endCells = new List<ChessBoardCell>()
            {
                _board.GetCellByPosition(startCell.Position + new Vector2(0, 1)),
                _board.GetCellByPosition(startCell.Position + new Vector2(1, 1)),
                _board.GetCellByPosition(startCell.Position + new Vector2(1, 0)),
                _board.GetCellByPosition(startCell.Position + new Vector2(1, -1)),
                _board.GetCellByPosition(startCell.Position + new Vector2(-0, -1)),
                _board.GetCellByPosition(startCell.Position + new Vector2(-1, -1)),
                _board.GetCellByPosition(startCell.Position + new Vector2(-1, 0)),
                _board.GetCellByPosition(startCell.Position + new Vector2(-1, 1))
            };

            foreach (ChessBoardCell endCell in endCells)
            {
                if (endCell != null)
                {
                    if (endCell.Figure == null)
                    {
                        result.Add(new FigureMove(startCell, endCell));
                    }
                    else if (endCell.Figure != null && endCell.Figure.Color != startCell.Figure.Color)
                    {
                        result.Add(new FigureMove(startCell, endCell));
                    }
                }
            }

            return result;
        }

        internal bool MakeMove(FigureMove move)
        {
            FigureMove variant = _moveVariants
                .Where(v => v.Start == move.Start && v.End == move.End)
                .FirstOrDefault();

            if(variant.Equals(new FigureMove())) 
            {
                return false;
            }

            int direction = _state.TurnOwner == KnownColor.White ? 1 : -1;
            Vector2 potpPosition;

            if (move.Start.Figure.Type == FigureType.Pawn && (Math.Abs((move.Start.Position - move.End.Position).Y) == 2))
            {
                potpPosition = move.Start.Position + new Vector2(0, 1 * direction);
            }
            else
            {
                potpPosition = new Vector2(-1);
            }

            if (move.Start.Figure.Type == FigureType.Pawn && move.End.Figure == null && move.Start.Position.X != move.End.Position.X)
            {
                _board.GetCellByPosition(move.End.Position + new Vector2(0, 1 * -1 * direction)).SetFigure(null);
            }

            var state = new ChessGameState(_state);
            state.PawnOnThePassant = new PawnOnThePassant(potpPosition, _state.TurnOwner);
            state.RuleOf50 = move.Start.Figure.Type == FigureType.Pawn || move.End.Figure != null ? 0 : _state.RuleOf50 + 1;
            state.Turn += _state.TurnOwner == KnownColor.Black ? 1 : 0;
            state.TurnOwner = ChessUtils.InvertColor(_state.TurnOwner);

            move.End.SetFigure(move.Start.Figure);
            move.Start.SetFigure(null);
 
            _history.Push(move);
            RefreshMoveVariants();
            return true;
        }

        internal bool MakeMove(string path)
        {
            ChessBoardCell start = _board.GetCellByAddress(path.Substring(0, 2));
            ChessBoardCell end = _board.GetCellByAddress(path.Substring(2, 2));

            FigureMove move = new FigureMove(start, end);

            return MakeMove(move);
        }

        internal void UndoLastMove()
        {
            if (_history.Count > 0)
            {
                FigureMove move = _history.Pop();
            }
        }
    }

    internal struct FigureMove
    {
        internal ChessBoardCell Start { get; private set; }
        internal ChessBoardCell End { get; private set; }
        internal FigureMove(ChessBoardCell start, ChessBoardCell end)
        {
            Start = start;
            End = end;
        }

        public override string ToString()
        {
            return $"from {Start.Address} to {End.Address}";
        }
    }

}

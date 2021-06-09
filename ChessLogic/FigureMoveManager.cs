using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace ChessLogic
{
    internal class FigureMoveManager
    {
        public ChessGameState State { get; private set; }
        private readonly ChessBoard _board;
        private readonly Stack<FigureMove> _history;
        private readonly List<FigureMove> _moveVariants;

        internal FigureMoveManager(ChessBoard board, ChessGameState state)
        {
            State = state;
            _board = board;
            _moveVariants = new List<FigureMove>();
            _history = new Stack<FigureMove>();

            RefreshMoveVariants();
        }

        internal bool MakeRandomMove()
        {
            if(_moveVariants.Count() == 0)
            {
                return false;
            }
            if(State.RuleOf50 == 50)
            {
                return false;
            }

            Random r = new Random();
            int id = r.Next(0, _moveVariants.Count() - 1);

            return MakeMove(_moveVariants[id]);
        }

        internal bool MakeMove(string path)
        {
            ChessBoardCell start = _board.GetCellByAddress(path.Substring(0, 2));
            ChessBoardCell end = _board.GetCellByAddress(path.Substring(2, 2));

            FigureMove move = new FigureMove(start, end);

            return MakeMove(move);
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

            int direction = State.TurnOwner == KnownColor.White ? 1 : -1;
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

            var state = new ChessGameState(State);
            state.PawnOnThePassant = new PawnOnThePassant(potpPosition, State.TurnOwner);
            state.RuleOf50 = move.Start.Figure.Type == FigureType.Pawn || move.End.Figure != null ? 0 : State.RuleOf50 + 1;
            state.Turn += State.TurnOwner == KnownColor.Black ? 1 : 0;
            state.TurnOwner = ChessUtils.InvertColor(State.TurnOwner);

            if(move.Start.Figure.Type == FigureType.King)
            {
                if(move.Start.Figure.Color == KnownColor.Black)
                {
                    state.BlackKingCastlingAvailable = false;
                    state.BlackQueenCastlingAvailable = false;
                }
                else if(move.Start.Figure.Color == KnownColor.White)
                {
                    state.WhiteKingCastlingAvailable = false;
                    state.WhiteQueenCastlingAvailable = false;
                }
            }
            else if(move.Start.Figure.Type == FigureType.Rook)
            {
                if(move.Start.Figure.Color == KnownColor.Black)
                {
                    if(move.Start.Position.X == 0)
                    {
                        state.BlackQueenCastlingAvailable = false;
                    }
                    else if(move.Start.Position.X == 7)
                    {
                        state.BlackKingCastlingAvailable = false;
                    }
                }
                else if (move.Start.Figure.Color == KnownColor.White)
                {
                    if (move.Start.Position.X == 0)
                    {
                        state.WhiteQueenCastlingAvailable = false;
                    }
                    else if (move.Start.Position.X == 7)
                    {
                        state.WhiteKingCastlingAvailable = false;
                    }
                }
            }

            State = state;

            move.End.SetFigure(move.Start.Figure);
            move.Start.SetFigure(null);

            if(move.End.Figure.Type == FigureType.Pawn && (move.End.Position.Y == 0 || move.End.Position.Y == 7)) {
                move.End.Figure.Transform(FigureType.Queen);
            }

            _history.Push(move);
            RefreshMoveVariants();
            return true;
        }

        internal void UndoLastMove()
        {
            if (_history.Count > 0)
            {
                FigureMove move = _history.Pop();
            }
        }

        private void RefreshMoveVariants()
        {
            List<FigureMove> currentBoardMoveVariants = GetMoveVariants(_board, State.TurnOwner);
            List<FigureMove> forRemove = new List<FigureMove>();

            foreach (var variant in currentBoardMoveVariants)
            {
                ChessBoard nextBoard = new ChessBoard(_board.ToString());

                ChessBoardCell start = nextBoard.GetCellByPosition(variant.Start.Position);
                ChessBoardCell end = nextBoard.GetCellByPosition(variant.End.Position);

                end.SetFigure(start.Figure);
                start.SetFigure(null);

                List<FigureMove> enemyMoveVariants = GetMoveVariants(nextBoard, ChessUtils.InvertColor(State.TurnOwner));
                if (KingIsUnderAttack(nextBoard, enemyMoveVariants, State.TurnOwner))
                {
                    forRemove.Add(variant);
                }
            }

            currentBoardMoveVariants.RemoveAll(v => forRemove.Contains(v));

            _moveVariants.Clear();
            _moveVariants.AddRange(currentBoardMoveVariants);
        }

        private bool KingIsUnderAttack(ChessBoard board, List<FigureMove> moveVariants, KnownColor color)
        {
            ChessBoardCell cell = board.GetCellWithKing(color);
            return moveVariants
                .Where(v => v.End == cell)
                .Count() != 0;
        }

        private List<FigureMove> GetMoveVariants(ChessBoard board, KnownColor color)
        {
            List<ChessBoardCell> cells = board.GetCellsWithFiguresOfColor(color);
            List<FigureMove> variants = new List<FigureMove>();

            foreach (ChessBoardCell cell in cells)
            {
                variants.AddRange(GetMoveVariantsFromCell(board, cell));
            }

            return variants;
        }

        private List<FigureMove> GetMoveVariantsFromCell(ChessBoard board, ChessBoardCell cell)
        {
            List<FigureMove> result = new List<FigureMove>();

            if (cell != null && cell.Figure != null)
            {
                switch (cell.Figure.Type)
                {
                    case FigureType.Pawn:
                        result.AddRange(GetPawnMoveVariantsFromCell(board, cell));
                        break;
                    case FigureType.Knight:
                        result.AddRange(GetKnightMoveVariantsFromCell(board, cell));
                        break;
                    case FigureType.Bishop:
                        result.AddRange(GetBishopMoveVariantsFromCell(board, cell));
                        break;
                    case FigureType.Rook:
                        result.AddRange(GetRockMoveVariantsFromCell(board, cell));
                        break;
                    case FigureType.Queen:
                        result.AddRange(GetBishopMoveVariantsFromCell(board, cell));
                        result.AddRange(GetRockMoveVariantsFromCell(board, cell));
                        break;
                    case FigureType.King:
                        result.AddRange(GetKingMoveVariantsFromCell(board, cell));
                        break;
                    default:
                        break;
                }
            }
            return result;
        }

        private List<FigureMove> GetMoveVariantsFromCells(ChessBoard board, List<ChessBoardCell> cells)
        {
            List<FigureMove> result = new List<FigureMove>();

            foreach (ChessBoardCell cell in cells)
            {
                result.AddRange(GetMoveVariantsFromCell(board, cell));
            }

            return result;
        }

        private List<FigureMove> GetPawnMoveVariantsFromCell(ChessBoard board, ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();

            int direction = startCell.Figure.Color == KnownColor.White ? 1 : -1;
            bool pawnIsOnStartPosition = startCell.Figure.Color == KnownColor.White ? startCell.Position.Y == 1 : startCell.Position.Y == 6;

            ChessBoardCell endCellFrontDirection = board.GetCellByPosition(startCell.Position + new Vector2(0, direction));
            ChessBoardCell endCellPassantDirection = board.GetCellByPosition(startCell.Position + new Vector2(0, 2 * direction));
            ChessBoardCell endCellFrontRightDirection = board.GetCellByPosition(startCell.Position + new Vector2(1, direction));
            ChessBoardCell endCellFronLeftDirection = board.GetCellByPosition(startCell.Position + new Vector2(-1, direction));

            if (endCellFrontDirection != null && endCellFrontDirection.Figure == null)
            {
                result.Add(new FigureMove(startCell, endCellFrontDirection));

                if (endCellPassantDirection != null && endCellPassantDirection.Figure == null && pawnIsOnStartPosition)
                {
                    result.Add(new FigureMove(startCell, endCellPassantDirection));
                }
            }

            if (endCellFrontRightDirection != null && endCellFrontRightDirection.Figure != null && endCellFrontRightDirection.Figure.Color != startCell.Figure.Color)
            {
                result.Add(new FigureMove(startCell, endCellFrontRightDirection));
            }

            if (endCellFronLeftDirection != null && endCellFronLeftDirection.Figure != null && endCellFronLeftDirection.Figure.Color != startCell.Figure.Color)
            {
                result.Add(new FigureMove(startCell, endCellFronLeftDirection));
            }

            return result;
        }

        private List<FigureMove> GetKnightMoveVariantsFromCell(ChessBoard board, ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();
            List<ChessBoardCell> endCells = new List<ChessBoardCell>()
            {
                board.GetCellByPosition(startCell.Position + new Vector2(1, 2)),
                board.GetCellByPosition(startCell.Position + new Vector2(2, 1)),
                board.GetCellByPosition(startCell.Position + new Vector2(1, -2)),
                board.GetCellByPosition(startCell.Position + new Vector2(2, -1)),
                board.GetCellByPosition(startCell.Position + new Vector2(-1, 2)),
                board.GetCellByPosition(startCell.Position + new Vector2(-2, 1)),
                board.GetCellByPosition(startCell.Position + new Vector2(-1, -2)),
                board.GetCellByPosition(startCell.Position + new Vector2(-2, -1))
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

        private List<FigureMove> GetBishopMoveVariantsFromCell(ChessBoard board, ChessBoardCell startCell)
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
                    ChessBoardCell endCell = board.GetCellByPosition(startCell.Position + direction * i);
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

        private List<FigureMove> GetRockMoveVariantsFromCell(ChessBoard board, ChessBoardCell startCell)
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
                    ChessBoardCell endCell = board.GetCellByPosition(startCell.Position + direction * i);
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

        private List<FigureMove> GetKingMoveVariantsFromCell(ChessBoard board, ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();
            List<ChessBoardCell> endCells = new List<ChessBoardCell>()
            {
                board.GetCellByPosition(startCell.Position + new Vector2(0, 1)),
                board.GetCellByPosition(startCell.Position + new Vector2(1, 1)),
                board.GetCellByPosition(startCell.Position + new Vector2(1, 0)),
                board.GetCellByPosition(startCell.Position + new Vector2(1, -1)),
                board.GetCellByPosition(startCell.Position + new Vector2(-0, -1)),
                board.GetCellByPosition(startCell.Position + new Vector2(-1, -1)),
                board.GetCellByPosition(startCell.Position + new Vector2(-1, 0)),
                board.GetCellByPosition(startCell.Position + new Vector2(-1, 1))
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
    }
}

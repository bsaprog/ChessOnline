using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace ChessLogic
{
    internal struct MoveVariant
    {
        internal ChessBoardCell Start { get; private set; }
        internal ChessBoardCell End { get; private set; }
        internal MoveVariant(ChessBoardCell start, ChessBoardCell end)
        {
            Start = start;
            End = end;

        }
        public override string ToString()
        {
            return $"from {Start.Address} to {End.Address}";
        }
    }
    
    internal struct FigureMove
    {
        internal ChessBoardCell Start { get; private set; }
        internal ChessBoardCell End { get; private set; }
        internal Figure MovingFigure { get; private set; }
        internal Figure RemovingFigure { get; private set; }

        internal FigureMove(ChessBoardCell start, ChessBoardCell end)
        {
            Start = start;
            End = end;
            MovingFigure = start.Figure;
            RemovingFigure = end.Figure;
        }
    }

    internal class FigureMoveManager
    {
        private readonly ChessBoard _board;
        private readonly ChessGameState _state;
        private readonly Stack<FigureMove> _history;
        private readonly List<MoveVariant> _moveVariants;

        internal FigureMoveManager(ChessBoard board, ChessGameState state)
        {
            _board = board;
            _state = state;
            _history = new Stack<FigureMove>();
            _moveVariants = new List<MoveVariant>();
        }
        internal void RefreshMoveVariants(int deep = 1)
        {
            deep -= 1;
            _moveVariants.Clear();
            List<ChessBoardCell> cells = _board.GetCellsWithFigures();
            foreach(ChessBoardCell cell in cells)
            {
                _moveVariants.AddRange(GetMoveVariantsFromCell(cell));
            }

            if(deep > 0)
            {
                List<MoveVariant> forRemove = new List<MoveVariant>();

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
            }
        }
        private bool KingIsUnderAttack(Color color)
        {
            ChessBoardCell cell = _board.GetCellWithKing(color);
            return _moveVariants.Where(v => v.End == cell).Count() != 0;
        }
        private List<MoveVariant> GetMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();

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
        private List<MoveVariant> GetMoveVariantsFromCells(List<ChessBoardCell> cells)
        {
            List<MoveVariant> result = new List<MoveVariant>();

            foreach (ChessBoardCell cell in cells)
            {
                result.AddRange(GetMoveVariantsFromCell(cell));
            }

            return result;
        }
        private List<MoveVariant> GetPawnMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();

            int direction = startCell.Figure.Color == Color.White ? 1 : -1;
            bool pawnIsOnStartPosition = startCell.Figure.Color == Color.White ? startCell.Position.Y == 1 : startCell.Position.Y == 6;

            ChessBoardCell endCellFrontDirection = _board.GetCellByPosition(startCell.Position + new Vector2(0, direction));
            ChessBoardCell endCellPassantDirection = _board.GetCellByPosition(startCell.Position + new Vector2(0, 2 * direction));
            ChessBoardCell endCellFrontRightDirection = _board.GetCellByPosition(startCell.Position + new Vector2(1, direction));
            ChessBoardCell endCellFronLeftDirection = _board.GetCellByPosition(startCell.Position + new Vector2(-1, direction));

            if (endCellFrontDirection != null && endCellFrontDirection.Figure == null)
            {
                result.Add(new MoveVariant(startCell, endCellFrontDirection));

                if (endCellPassantDirection != null && endCellPassantDirection.Figure == null && pawnIsOnStartPosition)
                {
                    result.Add(new MoveVariant(startCell, endCellPassantDirection));
                }
            }

            if (endCellFrontRightDirection != null && endCellFrontDirection.Figure != null && endCellFrontDirection.Figure.Color != startCell.Figure.Color)
            {
                result.Add(new MoveVariant(startCell, endCellFrontRightDirection));
            }

            if (endCellFronLeftDirection != null && endCellFronLeftDirection.Figure != null && endCellFronLeftDirection.Figure.Color != startCell.Figure.Color)
            {
                result.Add(new MoveVariant(startCell, endCellFronLeftDirection));
            }

            return result;
        }
        private List<MoveVariant> GetKnightMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();
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
                if (endCell != null) {
                    if(endCell.Figure == null)
                    {
                        result.Add(new MoveVariant(startCell, endCell));
                    }
                    else if (endCell.Figure != null && endCell.Figure.Color != startCell.Figure.Color)
                    {
                        result.Add(new MoveVariant(startCell, endCell));
                    }
                }
            }

            return result;
        }
        private List<MoveVariant> GetBishopMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();
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
                            result.Add(new MoveVariant(startCell, endCell));
                        }
                        else if (endCell.Figure.Color != startCell.Figure.Color)
                        {
                            result.Add(new MoveVariant(startCell, endCell));
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
        private List<MoveVariant> GetRockMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();

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
                            result.Add(new MoveVariant(startCell, endCell));
                        }
                        else if (endCell.Figure.Color != startCell.Figure.Color)
                        {
                            result.Add(new MoveVariant(startCell, endCell));
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
        private List<MoveVariant> GetKingMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();
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
                        result.Add(new MoveVariant(startCell, endCell));
                    }
                    else if (endCell.Figure != null && endCell.Figure.Color != startCell.Figure.Color)
                    {
                        result.Add(new MoveVariant(startCell, endCell));
                    }
                }
            }

            return result;
        }
        internal bool MakeMove(FigureMove move)
        {
            if(MoveIsReal(move) )
            {
                int direction = _state.TurnOwner == Color.White ? 1 : -1;
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

                _state.PawnOnThePassant = new PawnOnThePassant(potpPosition, _state.TurnOwner);
                _state.RuleOf50 = move.Start.Figure.Type == FigureType.Pawn || move.End.Figure != null ? 0 : _state.RuleOf50 + 1;
                _state.Turn += _state.TurnOwner == Color.Black ? 1 : 0;
                _state.TurnOwner = ChessUtils.InvertColor(_state.TurnOwner);


                move.Start.SetFigure(null);
                move.End.SetFigure(move.MovingFigure);

                // _state.UpdateState(false);

                _history.Push(move);
                RefreshMoveVariants(2);
                return true;
            }

            return false;
        }
        internal bool MakeMove(string path)
        {
            ChessBoardCell start = _board.GetCellByAddress(path.Substring(0, 2));
            ChessBoardCell end = _board.GetCellByAddress(path.Substring(2, 2));

            FigureMove move = new FigureMove(start, end);

            return MakeMove(move);
        }
        /*
       internal void UndoLastMove()
       {
           if (_history.Count > 0)
           {
               FigureMove move = _history.Pop();

               move.Start.SetFigure(move.MovingFigure);
               move.End.SetFigure(move.RemovingFigure);
           }
       }
         */
       private bool MoveIsReal(FigureMove move)
       {
            MoveVariant variant = _moveVariants
                .Where(v => v.Start == move.Start && v.End == move.End)
                .FirstOrDefault();


            return !variant.Equals(new MoveVariant());
       }
    }

    public class ChessGame
    {
        private readonly ChessBoard _board;
        private readonly ChessGameState _state;
        private readonly FigureMoveManager _moveManager;

        public ChessGame(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            _board = new ChessBoard(fen);
            _state = new ChessGameState(fen);
            _moveManager = new FigureMoveManager(_board, _state);


            _moveManager.RefreshMoveVariants(2);

        }
        public Figure GetFigureAt(Vector2 position)
        {
            ChessBoardCell cell = _board.GetCellByPosition(position);
            return cell.Figure;
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
        public bool MakeMove(string path)
        {
            return _moveManager.MakeMove(path);
        }
    }

    internal struct PawnOnThePassant
    {
        internal Vector2 HitPosition { get; private set; }
        internal Vector2 PawnPosition { get; private set; }
        internal Color OwnerColor { get; private set; }

        internal PawnOnThePassant(string adres, Color ownerColor)
        {
            int direction = ownerColor == Color.White ? 1 : -1;

            OwnerColor = ownerColor;
            HitPosition = ChessUtils.GetPositionFromAddress(adres);
            PawnPosition = HitPosition + new Vector2(0, 1 * direction);
        }
        internal PawnOnThePassant(Vector2 position, Color ownerColor)
        {
            int direction = ownerColor == Color.White ? 1 : -1;

            OwnerColor = ownerColor;
            HitPosition = position;
            PawnPosition = HitPosition + new Vector2(0, 1 * direction);
        }
    }

    internal class ChessGameState
    {
        internal Color TurnOwner;
        internal bool WhiteKingCastlingAvailable = false;
        internal bool WhiteQueenCastlingAvailable = false;
        internal bool BlackKingCastlingAvailable = false;
        internal bool BlackQueenCastlingAvailable = false;
        internal PawnOnThePassant PawnOnThePassant;
        internal int RuleOf50;
        internal int Turn;

        internal ChessGameState(string fen)
        {
            string[] fenParts = fen.Split(" ");

            foreach (char c in fenParts[2])
            {
                switch (c)
                {
                    case 'K':
                        WhiteKingCastlingAvailable = true;
                        break;
                    case 'Q':
                        WhiteQueenCastlingAvailable = true;
                        break;
                    case 'k':
                        BlackKingCastlingAvailable = true;
                        break;
                    case 'q':
                        BlackQueenCastlingAvailable = true;
                        break;
                    default:
                        break;
                }
            }

            TurnOwner = fenParts[1] == "w" ? Color.White : Color.Black;
            PawnOnThePassant = new PawnOnThePassant(fenParts[3], ChessUtils.InvertColor(TurnOwner) );
            RuleOf50 = Int32.Parse(fenParts[4]);
            Turn = Int32.Parse(fenParts[5]);
        }
    }

    public class Figure
    {
        public Color Color { get; private set; }
        public FigureType Type { get; private set; }
        public Figure(FigureType type, Color color)
        {
            Type = type;
            Color = color;
        }
        public override string ToString()
        {
            char c = (char)Type;
            c = Color == Color.White ? Char.ToLower(c) : char.ToUpper(c);
            return c.ToString();
        }
    }

    internal class ChessBoard
    {
        internal readonly Dictionary<Vector2, ChessBoardCell> Cells = new Dictionary<Vector2, ChessBoardCell>();
        internal ChessBoard(string fen)
        {
            fen = fen.Split(' ')[0];

            string data = fen;
            for (int i = 8; i > 1; i--)
            {
                data = data.Replace(i.ToString(), (i - 1).ToString() + "1");
            }

            string[] lines = data.Split('/');

            Cells = new Dictionary<Vector2, ChessBoardCell>();
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    ChessBoardCell cell = new ChessBoardCell(new Vector2(x, y));
                    Cells.Add(cell.Position, cell);

                    char symbol = lines[y][x];

                    if (!char.IsDigit(symbol))
                    {
                        FigureType type = (FigureType)Char.ToLower(symbol);
                        Color color = Char.IsUpper(symbol) ? Color.Black : Color.White;
                        cell.SetFigure(new Figure(type, color));
                    }
                }
            }
        }
        internal ChessBoardCell GetCellByPosition(Vector2 position)
        {
            ChessBoardCell result = null;

            if(Cells.ContainsKey(position))
            {
                result = Cells[position];
            }

            return result;
        }
        internal ChessBoardCell GetCellByAddress(string address)
        {
            Vector2 position = ChessUtils.GetPositionFromAddress(address);
            return GetCellByPosition(position);
        }
        internal ChessBoardCell GetCellWithKing(Color kingColor)
        {
            return Cells
                .Select(pair => pair.Value)
                .Where(cell => cell.Figure != null && cell.Figure.Type == FigureType.King && cell.Figure.Color == kingColor)
                .First();
        }
        internal List<ChessBoardCell> GetCellsWithFigures()
        {
            return Cells
                .Select(pair => pair.Value)
                .Where(cell => cell.Figure != null)
                .ToList<ChessBoardCell>();
        }
        public override string ToString()
        {
            string result = "";

            for (int y = 0; y < 8; y++)
            {
                if(y != 0)
                {
                    result += "/";
                }

                for (int x = 0; x < 8; x++)
                {
                    ChessBoardCell cell = Cells[new Vector2(x, y)];
                    result += cell.Figure == null ? "1" : cell.Figure.ToString();
                }
            }

            string template = "11111111";
            for (int i = 8; i > 1; i--)
            {
                result = result.Replace(template.Substring(0, i), i.ToString());
            }

            return result;
        }
    }

    internal class ChessBoardCell
    {
        internal Vector2 Position { get; private set; }
        internal string Address { get; private set; }
        internal Figure Figure { get; private set; }
        internal ChessBoardCell(Vector2 position)
        {
            Position = position;
            Address = ChessUtils.GetAddresFromPosition(position);
        }
        internal void SetFigure(Figure figure)
        {
            Figure = figure;
        }
        public override string ToString()
        {
            return $"{Address} {Figure}";
        }
    }

    internal static class ChessUtils
    {
        internal static string GetAddresFromPosition(Vector2 position)
        {
            string result = "";

            int x = (Int32)position.X;
            int y = (Int32)position.Y;

            if(x >= 0 && x < 8 && y >= 0 && y < 8)
            {
                string[] conformity = new string[8]
                {
                    "a",
                    "b",
                    "c",
                    "d",
                    "e",
                    "f",
                    "g",
                    "h"
                };
                result = conformity[x] + (y + 1).ToString();
            }

            return result;
        }
        internal static Vector2 GetPositionFromAddress(string address)
        {
            Vector2 result = new Vector2(-1);
            
            if(address.Length == 2)
            {
                char x = address[0];
                Int32.TryParse(address[1].ToString(), out int y);

                if (x >= 'a' && x <= 'h' && y >= 1 && y <= 8)
                {
                    Dictionary<char, int> conformity = new Dictionary<char, int>
                    {
                        { 'a', 0 },
                        { 'b', 1 },
                        { 'c', 2 },
                        { 'd', 3 },
                        { 'e', 4 },
                        { 'f', 5 },
                        { 'g', 6 },
                        { 'h', 7 }
                    };

                    result = new Vector2(conformity[x], y - 1);
                }
            }

            return result;
        }
        internal static Color InvertColor(Color color)
        {
            Color result;
            if(color == Color.Black)
            {
                result = Color.White;
            }
            else if(color == Color.White)
            {
                result = Color.Black;
            }
            else
            {
                result = color;
            }

            return result;
        }
    }

    public enum FigureType
    {
        None = '.',
        Pawn = 'p',
        Rook = 'r',
        Knight = 'n',
        Bishop = 'b',
        Queen = 'q',
        King = 'k'
    }

    public enum Color
    {
        None = 0,
        White,
        Black
    }

    public enum GameType
    {
        None = 0,
        Classic
    }
}
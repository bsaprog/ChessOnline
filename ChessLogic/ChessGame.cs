using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChessLogic
{

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

        internal FigureMoveManager(ChessBoard board, ChessGameState state)
        {
            _board = board;
            _state = state;
            _history = new Stack<FigureMove>();
        }

        internal void MakeMove(FigureMove move)
        {
            if(MoveIsReal(move) )
            {
                move.Start.SetFigure(null);
                move.End.SetFigure(move.MovingFigure);

               // _state.UpdateState(false);

                _history.Push(move);
            }
        }

        internal void MakeMove(string path)
        {
            ChessBoardCell start = _board.GetCellByAddress(path.Substring(0, 2));
            ChessBoardCell end = _board.GetCellByAddress(path.Substring(2, 2));

            FigureMove move = new FigureMove(start, end);

            MakeMove(move);
        }

        internal void UndoLastMove()
        {
            if (_history.Count > 0)
            {
                FigureMove move = _history.Pop();

                move.Start.SetFigure(move.MovingFigure);
                move.End.SetFigure(move.RemovingFigure);
            }
        }

        private bool MoveIsReal(FigureMove move)
        {
            List<FigureMove> AvailableMoves = GetAvailableMovesFromCell(move.Start);
            return AvailableMoves.Contains(move);
        }

        private List<FigureMove> GetAvailableMovesFromCell(ChessBoardCell startCell)
        {
            List<FigureMove> result = new List<FigureMove>();

            if(startCell != null)
            {
                Vector2 startPosition = startCell.Position;

                Figure figure = startCell.Figure;
                if(figure != null)
                {
                    int direction = figure.Color == Color.White ? 1 : -1;
                    bool pawnIsOnStartPosition = figure.Color == Color.White ? startPosition.Y == 1 : startPosition.Y == 6;

                    Vector2 frontDirection = new Vector2(0, direction);
                    Vector2 frontPassDirection = new Vector2(0, 2 * direction);
                    Vector2 frontRightDirection = new Vector2(1, direction);
                    Vector2 fronLeftDirection = new Vector2(-1, direction);

                    ChessBoardCell endCellFrontDirection = _board.GetCellByPosition(startPosition + frontDirection);
                    ChessBoardCell endCellFrontPassDirection = _board.GetCellByPosition(startPosition + frontPassDirection);
                    ChessBoardCell endCellFrontRightDirection = _board.GetCellByPosition(startPosition + frontRightDirection);
                    ChessBoardCell endCellFronLeftDirection = _board.GetCellByPosition(startPosition + fronLeftDirection);

                    if (endCellFrontDirection != null && endCellFrontDirection.Figure == null)
                    {
                        result.Add(new FigureMove(startCell, endCellFrontDirection));
                    }

                    if (endCellFrontPassDirection != null && endCellFrontPassDirection.Figure == null && pawnIsOnStartPosition)
                    {
                        result.Add(new FigureMove(startCell, endCellFrontPassDirection));
                    }

                    if (endCellFrontRightDirection != null && endCellFrontRightDirection.Figure != null && endCellFrontRightDirection.Figure.Color != figure.Color)
                    {
                        result.Add(new FigureMove(startCell, endCellFrontRightDirection));
                    }

                    if (endCellFronLeftDirection != null && endCellFronLeftDirection.Figure != null && endCellFronLeftDirection.Figure.Color != figure.Color)
                    {
                        result.Add(new FigureMove(startCell, endCellFronLeftDirection));
                    }
                }
            }

            return result;
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
        }
        public Figure GetFigureAt(Vector2 position)
        {
            ChessBoardCell cell = _board.GetCellByPosition(position);
            return cell.Figure;
        }
        public List<string> GetAvailableMoves()
        {
            List<string> result = new List<string>();

            foreach (KeyValuePair<Vector2, ChessBoardCell> pair in _board.Cells)
            {
                Figure figure = pair.Value.Figure;
                Vector2 position = pair.Key;

                if (figure != null)
                {
                    if(figure.Color != _state.TurnOwner)
                    {
                        continue;
                    }

                    result.AddRange(GetAvailableFigureMoves(figure, position));
                }
            }

            return result;
        }
        public List<string> GetAvailableFigureMoves(Figure figure, Vector2 startPosition)
        {
            List<string> result = new List<string>();

            switch (figure.Type)
            {
                case FigureType.Pawn:
                    result.AddRange(GetAvailablePawnMoves(figure, startPosition));
                    break;
                case FigureType.Knight:
                    result.AddRange(GetAvailableKnightMoves(figure, startPosition));
                    break;
                case FigureType.Bishop:
                    result.AddRange(GetAvailableBishopMoves(figure, startPosition));
                    break;
                case FigureType.Rook:
                    result.AddRange(GetAvailableRockMoves(figure, startPosition));
                    break;
                case FigureType.Queen:
                    result.AddRange(GetAvailableBishopMoves(figure, startPosition));
                    result.AddRange(GetAvailableRockMoves(figure, startPosition));
                    break;
                case FigureType.King:
                    result.AddRange(GetAvailableKingMoves(figure, startPosition));
                    break;
                default:
                    break;
            }

            return result;
        }
        private List<string> GetAvailablePawnMoves(Figure figure, Vector2 position)
        {
            List<string> result = new List<string>();

            int direction = figure.Color == Color.White ? 1 : -1;
            bool pawnIsOnStartPosition = figure.Color == Color.White ? position.Y == 1 : position.Y == 6;

            Vector2 frontDirection = new Vector2(0, direction);
            Vector2 frontPassDirection = new Vector2(0, 2 * direction);
            Vector2 frontRightDirection = new Vector2(1, direction);
            Vector2 fronLeftDirection = new Vector2(-1, direction);

            ChessBoardCell endPointFrontDirection = _board.GetCellByPosition(position + frontDirection);
            ChessBoardCell endPointFrontPassDirection = _board.GetCellByPosition(position + frontPassDirection);
            ChessBoardCell endPointFrontRightDirection = _board.GetCellByPosition(position + frontRightDirection);
            ChessBoardCell endPointFronLeftDirection = _board.GetCellByPosition(position + fronLeftDirection);

            ChessBoardCell pawnOnThePassatHitCell = _board.GetCellByPosition(_state.PawnOnThePassant.HitPosition);

            if (endPointFrontDirection != null && endPointFrontDirection.Figure == null)
            {
                result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPointFrontDirection.Position));
            }

            if (endPointFrontPassDirection != null && endPointFrontPassDirection.Figure == null && pawnIsOnStartPosition)
            {
                result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPointFrontPassDirection.Position));
            }

            if (endPointFrontRightDirection != null && endPointFrontRightDirection.Figure != null && endPointFrontRightDirection.Figure.Color != figure.Color)
            {
                result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPointFrontRightDirection.Position));
            }

            if (endPointFronLeftDirection != null && endPointFronLeftDirection.Figure != null && endPointFronLeftDirection.Figure.Color != figure.Color)
            {
                result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPointFronLeftDirection.Position));
            }

            if(endPointFrontRightDirection != null && pawnOnThePassatHitCell != null && endPointFrontRightDirection == pawnOnThePassatHitCell)
            {
                result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPointFrontRightDirection.Position));
            }

            if (endPointFronLeftDirection != null && pawnOnThePassatHitCell != null && endPointFronLeftDirection == pawnOnThePassatHitCell)
            {
                result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPointFronLeftDirection.Position));
            }

            return result;
        }
        private List<string> GetAvailableKnightMoves(Figure figure, Vector2 position)
        {
            List<string> result = new List<string>();
            List<Vector2> directions = new List<Vector2>();
            List<ChessBoardCell> endPoints = new List<ChessBoardCell>();

            directions.Add(new Vector2(1, 2));
            directions.Add(new Vector2(2, 1));
            directions.Add(new Vector2(1, -2));
            directions.Add(new Vector2(2, -1));
            directions.Add(new Vector2(-1, 2));
            directions.Add(new Vector2(-2, 1));
            directions.Add(new Vector2(-1, -2));
            directions.Add(new Vector2(-2, -1));

            foreach (Vector2 direction in directions)
            {
                endPoints.Add(_board.GetCellByPosition(position + direction));
            }

            foreach (ChessBoardCell endPoint in endPoints)
            {
                if (endPoint != null && (endPoint.Figure == null || endPoint.Figure.Color != figure.Color))
                {
                    result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPoint.Position));
                }
            }

            return result;
        }
        private List<string> GetAvailableBishopMoves(Figure figure, Vector2 position)
        {
            List<string> result = new List<string>();
            List<Vector2> directions = new List<Vector2>();

            directions.Add(new Vector2(1, 1));
            directions.Add(new Vector2(1, -1));
            directions.Add(new Vector2(-1, 1));
            directions.Add(new Vector2(-1, -1));

            foreach (Vector2 direction in directions) 
            {
                for (int i = 1; i < 8; i++)
                {
                    ChessBoardCell endPoint = _board.GetCellByPosition(position + direction * i);
                    if(endPoint == null)
                    {
                        break;
                    }
                    else
                    {
                        if(endPoint.Figure == null)
                        {
                            result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPoint.Position));
                        }
                        else if(endPoint.Figure.Color != figure.Color)
                        {
                            result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPoint.Position));
                            break;
                        }
                        else if(endPoint.Figure.Color == figure.Color)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }
        private List<string> GetAvailableRockMoves(Figure figure, Vector2 position)
        {
            List<string> result = new List<string>();
            List<Vector2> directions = new List<Vector2>();

            directions.Add(new Vector2(1, 0));
            directions.Add(new Vector2(0, 1));
            directions.Add(new Vector2(-1, 0));
            directions.Add(new Vector2(0, -1));

            foreach (Vector2 direction in directions)
            {
                for (int i = 1; i < 8; i++)
                {
                    ChessBoardCell endPoint = _board.GetCellByPosition(position + direction * i);
                    if (endPoint == null)
                    {
                        break;
                    }
                    else
                    {
                        if (endPoint.Figure == null)
                        {
                            result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPoint.Position));
                        }
                        else if (endPoint.Figure.Color != figure.Color)
                        {
                            result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPoint.Position));
                            break;
                        }
                        else if (endPoint.Figure.Color == figure.Color)
                        {
                            break;
                        }
                    }
                }
            }

            return result;
        }
        private List<string> GetAvailableKingMoves(Figure figure, Vector2 position)
        {
            List<string> result = new List<string>();
            List<Vector2> directions = new List<Vector2>();
            List<ChessBoardCell> endPoints = new List<ChessBoardCell>();

            directions.Add(new Vector2(0, 1));
            directions.Add(new Vector2(1, 1));
            directions.Add(new Vector2(1, 0));
            directions.Add(new Vector2(1, -1));
            directions.Add(new Vector2(-1, 0));
            directions.Add(new Vector2(-1, -1));
            directions.Add(new Vector2(-1, 0));
            directions.Add(new Vector2(1, -1));

            foreach (Vector2 direction in directions)
            {
                endPoints.Add(_board.GetCellByPosition(position + direction));
            }

            foreach (ChessBoardCell endPoint in endPoints)
            {
                if (endPoint != null && (endPoint.Figure == null || endPoint.Figure.Color != figure.Color))
                {
                    result.Add(ChessUtils.GetAddresFromPosition(position) + ChessUtils.GetAddresFromPosition(endPoint.Position));
                }
            }

            return result;
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
            List<string> AvailableMoves = GetAvailableMoves();

            if(AvailableMoves.Contains(path))
            {
                ChessBoardCell startCell = _board.GetCellByAddress(path.Substring(0, 2));
                ChessBoardCell endCell = _board.GetCellByAddress(path.Substring(2, 2));
                int direction = _state.TurnOwner == Color.White ? 1 : -1;
                Vector2 potpPosition;

                if(startCell.Figure.Type == FigureType.Pawn && (Math.Abs((startCell.Position - endCell.Position).Y) == 2) )
                {
                    potpPosition = startCell.Position + new Vector2(0, 1 * direction);
                }
                else
                {
                    potpPosition = new Vector2(-1);
                }

                if(startCell.Figure.Type == FigureType.Pawn && endCell.Figure == null && startCell.Position.X != endCell.Position.X)
                {
                    _board.GetCellByPosition(endCell.Position + new Vector2(0, 1 * -1 * direction)).SetFigure(null);
                }

                _state.PawnOnThePassant = new PawnOnThePassant(potpPosition, _state.TurnOwner);
                _state.RuleOf50 = startCell.Figure.Type == FigureType.Pawn || endCell.Figure != null ? 0 : _state.RuleOf50 + 1;
                _state.Turn += _state.TurnOwner == Color.Black ? 1 : 0;
                _state.TurnOwner = ChessUtils.InvertColor(_state.TurnOwner);

                endCell.SetFigure(startCell.Figure);
                startCell.SetFigure(null);

                return true;
            }

            return false;
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
    }

    internal class ChessBoard
    {
        internal readonly Dictionary<Vector2, ChessBoardCell> Cells = new Dictionary<Vector2, ChessBoardCell>();
        internal ChessBoard(string fen)
        {
            fen = fen.Split(' ')[0];
            string[] lines = fen.Split('/');

            Cells = new Dictionary<Vector2, ChessBoardCell>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ChessBoardCell cell = new ChessBoardCell(new Vector2(i, j));
                    Cells.Add(cell.Position, cell);
                }
            }

            for (int y = 0; y < lines.Length; y++)
            {
                int skipped = 0;

                for (int x = 0; x < 8; x++)
                {
                    char symbol = lines[y][x - skipped];
                    Color color = Char.IsUpper(symbol) ? Color.Black : Color.White;

                    if (Int32.TryParse(symbol.ToString(), out int num))
                    {
                        x += num - 1;
                        skipped += num - 1;
                    }
                    else
                    {
                        FigureType type = (FigureType)Char.ToLower(symbol);
                        ChessBoardCell cell = GetCellByPosition(new Vector2(x, y));
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
    }

    internal static class ChessUtils
    {
        internal static string GetAddresFromPosition(Vector2 position)
        {
            string result = "";

            int x = (Int32)position.X;
            int y = (Int32)position.Y;

            if(x >= 0 && x <= 7 && y >= 0 && y <= 7)
            {
                string[] conformity = new string[8];
                conformity[0] = "a";
                conformity[1] = "b";
                conformity[2] = "c";
                conformity[3] = "d";
                conformity[4] = "e";
                conformity[5] = "f";
                conformity[6] = "g";
                conformity[7] = "h";

                result = conformity[x] + (y + 1).ToString();
            }

            return result;
        }
        internal static Vector2 GetPositionFromAddress(string address)
        {
            Vector2 result = new Vector2(-1, -1);
            
            if(address.Length == 2)
            {
                char x = address[0];
                int y = -1;
                Int32.TryParse(address[1].ToString(), out y);

                if (x >= 'a' && x <= 'h' && y >= 1 && y <= 8)
                {
                    Dictionary<char, int> conformity = new Dictionary<char, int>();
                    conformity.Add('a', 0);
                    conformity.Add('b', 1);
                    conformity.Add('c', 2);
                    conformity.Add('d', 3);
                    conformity.Add('e', 4);
                    conformity.Add('f', 5);
                    conformity.Add('g', 6);
                    conformity.Add('h', 7);

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
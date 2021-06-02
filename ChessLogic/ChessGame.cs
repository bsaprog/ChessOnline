using System;
using System.Collections.Generic;
using System.Numerics;

namespace ChessLogic
{
    public class ChessGame
    {
        private readonly ChessBoard Board;
        public ChessGameState State;

        public ChessGame(string fen)
        {
            string defaultFen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
            fen = fen == "" ? defaultFen : fen;

            Board = new ChessBoard(fen);
            State = new ChessGameState(fen);
        }

        public Figure getFigureAt(int x, int y)
        {
            Vector2 position = new Vector2(x, y);
            ChessBoardCell cell = Board.GetCellByPosition(position);
            return cell.Figure;
        }

        public List<string> getAvailableMoves()
        {
            List<string> result = new List<string>();

            foreach (KeyValuePair<Vector2, ChessBoardCell> pair in Board.Cells)
            {
                Figure figure = pair.Value.Figure;
                Vector2 position = pair.Key;

                if (figure != null)
                {
                    if(figure.Color != State.TurnOwner)
                    {
                        continue;
                    }

                    switch (figure.Type)
                    {
                        case FigureType.Pawn:
                            result.AddRange(getAvailablePawnMoves(figure, position));
                            break;
                        case FigureType.Knight:
                            result.AddRange(getAvailableKnightMoves(figure, position));
                            break;
                        case FigureType.Bishop:
                            result.AddRange(getAvailableBishopMoves(figure, position));
                            break;
                        case FigureType.Rook:
                            result.AddRange(getAvailableRockMoves(figure, position));
                            break;
                        case FigureType.Queen:
                            result.AddRange(getAvailableBishopMoves(figure, position));
                            result.AddRange(getAvailableRockMoves(figure, position));
                            break;
                        case FigureType.King:
                            result.AddRange(getAvailableKingMoves(figure, position));
                            break;
                        default:
                            break;
                    }
                }
            }

            return result;
        }

        private List<string> getAvailablePawnMoves(Figure figure, Vector2 position)
        {
            //TODO: add enemy pawn on pass check.

            List<string> result = new List<string>();

            int direction = figure.Color == Color.White ? 1 : -1;
            bool pawnIsOnStartPosition = figure.Color == Color.White ? position.Y == 1 : position.Y == 6;

            Vector2 frontDirection = new Vector2(0, direction);
            Vector2 frontPassDirection = new Vector2(0, 2 * direction);
            Vector2 frontRightDirection = new Vector2(1, direction);
            Vector2 fronLeftDirection = new Vector2(-1, direction);

            ChessBoardCell endPointFrontDirection = Board.GetCellByPosition(position + frontDirection);
            ChessBoardCell endPointFrontPassDirection = Board.GetCellByPosition(position + frontPassDirection);
            ChessBoardCell endPointFrontRightDirection = Board.GetCellByPosition(position + frontRightDirection);
            ChessBoardCell endPointFronLeftDirection = Board.GetCellByPosition(position + fronLeftDirection);

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

            return result;
        }
        
        private List<string> getAvailableKnightMoves(Figure figure, Vector2 position)
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
                endPoints.Add(Board.GetCellByPosition(position + direction));
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
        
        private List<string> getAvailableBishopMoves(Figure figure, Vector2 position)
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
                    ChessBoardCell endPoint = Board.GetCellByPosition(position + direction * i);
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
       
        private List<string> getAvailableRockMoves(Figure figure, Vector2 position)
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
                    ChessBoardCell endPoint = Board.GetCellByPosition(position + direction * i);
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
        
        private List<string> getAvailableKingMoves(Figure figure, Vector2 position)
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
                endPoints.Add(Board.GetCellByPosition(position + direction));
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
        
        public bool MakeMove(string path)
        {
            List<string> AvailableMoves = getAvailableMoves();

            if(AvailableMoves.Contains(path))
            {
                ChessBoardCell startCell = Board.GetCellByAddress(path.Substring(0, 2));
                ChessBoardCell endCell = Board.GetCellByAddress(path.Substring(2, 2));

                bool reset50 = startCell.Figure.Type == FigureType.Pawn || endCell.Figure != null;

                endCell.SetFigure(startCell.Figure);
                startCell.SetFigure(null);

                State.updateState(reset50);

                return true;
            }

            return false;
        }
    }

    public class ChessGameState
    {
        public Color TurnOwner { get; private set; }
        public bool WhiteKingCastlingAvailable { get; private set; } = false;
        public bool WhiteQueenCastlingAvailable { get; private set; } = false;
        public bool BlackKingCastlingAvailable { get; private set; } = false;
        public bool BlackQueenCastlingAvailable { get; private set; } = false;
        public string PawnOnThePass { get; private set; }
        public int RuleOf50 { get; private set; }
        public int Turn { get; private set; }

        public ChessGameState(string fen)
        {
            string[] fenParts = fen.Split(" ");
            TurnOwner = fenParts[1] == "w" ? Color.White : Color.Black;

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

            PawnOnThePass = fenParts[3];
            RuleOf50 = Int32.Parse(fenParts[4]);
            Turn = Int32.Parse(fenParts[5]);
        }

        public void updateState(bool reset50)
        {
            RuleOf50 = reset50 ? 0 : RuleOf50 + 1;
            Turn += TurnOwner == Color.Black ? 1 : 0;
            TurnOwner = TurnOwner == Color.Black ? Color.White : Color.Black;
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
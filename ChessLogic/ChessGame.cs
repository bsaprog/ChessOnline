using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace ChessLogic
{
    public class ChessGame
    {
        private ChessBoard Board;

        public Color TurnOwner { get; private set; }
        public bool WhiteKingCastlingAvailable { get; private set; } = false;
        public bool WhiteQueenCastlingAvailable { get; private set; } = false;
        public bool BlackKingCastlingAvailable { get; private set; } = false;
        public bool BlackQueenCastlingAvailable { get; private set; } = false;
        public string PawnOnThePass { get; private set; }
        public int RuleOf50 { get; private set; }
        public int Turn { get; private set; }

        public ChessGame(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            InitWithFen(fen);
        }

        private void InitWithFen(string fen)
        {
            string[] fenParts = fen.Split(' ');

            Board = new ChessBoard();
            Board.InitWithFen(fenParts[0]);

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

        public Figure getFigureAt(int x, int y)
        {
            return Board.GetCellByPosition(new Vector2(x, y)).Figure;
        }
    }

    public class ChessBoard
    {
        private readonly Dictionary<Vector2, ChessBoardCell> Cells = new Dictionary<Vector2, ChessBoardCell>();

        public ChessBoard()
        {
            Cells = new Dictionary<Vector2, ChessBoardCell>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ChessBoardCell cell = new ChessBoardCell(new Vector2(i, j));
                    Cells.Add(cell.Position, cell);
                }
            }
        }

        public ChessBoardCell GetCellByAddress(string address)
        {
            return Cells[ChessUtils.GetPositionFromAddress(address)];
        }

        public ChessBoardCell GetCellByPosition(Vector2 position)
        {
            return Cells[position];
        }

        public void InitWithFen(string fen)
        {
            string[] lines = fen.Split('/');

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
                        cell.PlaceFigure(new Figure(type, color));
                    }
                }
            }
        }
    }
   
    public class ChessBoardCell
    {
        public Vector2 Position { get; private set; }
        public Color Color { get; private set; }
        public string Address { get; private set; }
        public Figure Figure { get; private set; }

        public ChessBoardCell(Vector2 position)
        {
            Position = position;
            Address = ChessUtils.GetAddresFromPosition(position);
        }

        public ChessBoardCell(string address)
        {
            Address = address;
            Position = ChessUtils.GetPositionFromAddress(address);
        }

        public void PlaceFigure(Figure figure)
        {
            Figure = figure;
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

    public static class ChessUtils
    {
        public static string GetAddresFromPosition(Vector2 position)
        {
            int x = (Int32)position.X;
            int y = (Int32)position.Y;

            string[] conformity = new string[8];
            conformity[0] = "a";
            conformity[1] = "b";
            conformity[2] = "c";
            conformity[3] = "d";
            conformity[4] = "e";
            conformity[5] = "f";
            conformity[6] = "g";
            conformity[7] = "h";

            return conformity[x] + (y + 1).ToString();
        }

        public static Vector2 GetPositionFromAddress(string address)
        {
            char x = address[0];
            char y = address[1];

            Dictionary<char, int> conformity = new Dictionary<char, int>();
            conformity.Add('a', 0);
            conformity.Add('b', 1);
            conformity.Add('c', 2);
            conformity.Add('d', 3);
            conformity.Add('e', 4);
            conformity.Add('f', 5);
            conformity.Add('g', 6);
            conformity.Add('h', 7);

            return new Vector2(conformity[x], y - 1);
        }
    }
}
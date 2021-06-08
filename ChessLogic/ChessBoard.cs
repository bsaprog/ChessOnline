using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;

namespace ChessLogic
{
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
                        KnownColor color = Char.IsUpper(symbol) ? KnownColor.Black : KnownColor.White;
                        cell.SetFigure(new Figure(type, color));
                    }
                }
            }
        }
        
        internal ChessBoardCell GetCellByPosition(Vector2 position)
        {
            ChessBoardCell result = null;

            if (Cells.ContainsKey(position))
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

        internal ChessBoardCell GetCellWithKing(KnownColor kingColor)
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
                .ToList();
        }
        
        public override string ToString()
        {
            string result = "";

            for (int y = 0; y < 8; y++)
            {
                if (y != 0)
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
}

using System;
using System.Drawing;

namespace ChessLogic
{
    public class Figure
    {
        public KnownColor Color { get; private set; }
        public FigureType Type { get; private set; }
        public Figure(FigureType type, KnownColor color)
        {
            Type = type;
            Color = color;
        }

        public override string ToString()
        {
            char c = (char)Type;
            c = Color == KnownColor.White ? Char.ToLower(c) : char.ToUpper(c);
            return c.ToString();
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
}

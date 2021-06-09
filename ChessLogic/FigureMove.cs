using System;
using System.Collections.Generic;
using System.Text;

namespace ChessLogic
{
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

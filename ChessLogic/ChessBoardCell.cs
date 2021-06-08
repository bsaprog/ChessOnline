using System.Numerics;

namespace ChessLogic
{
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
}

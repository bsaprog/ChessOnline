using System.Numerics;

namespace ChessLogic
{
    internal class ChessBoardCell
    {
        internal readonly Vector2 Position;
        internal readonly string Address;
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
            return $"{Address}";
        }
    }
}

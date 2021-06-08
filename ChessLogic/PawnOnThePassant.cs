using System.Drawing;
using System.Numerics;

namespace ChessLogic
{
    internal struct PawnOnThePassant
    {
        internal Vector2 HitPosition { get; private set; }
        internal Vector2 PawnPosition { get; private set; }
        internal KnownColor Color { get; private set; }

        internal PawnOnThePassant(string adres, KnownColor color)
        {
            int direction = color == KnownColor.White ? 1 : -1;

            Color = color;
            HitPosition = ChessUtils.GetPositionFromAddress(adres);
            PawnPosition = HitPosition + new Vector2(0, 1 * direction);
        }

        internal PawnOnThePassant(Vector2 position, KnownColor color)
        {
            int direction = color == KnownColor.White ? 1 : -1;

            Color = color;
            HitPosition = position;
            PawnPosition = HitPosition + new Vector2(0, 1 * direction);
        }
    }
}

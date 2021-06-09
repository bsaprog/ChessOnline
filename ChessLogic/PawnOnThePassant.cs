using System.Drawing;
using System.Numerics;

namespace ChessLogic
{
    internal struct PawnOnThePassant
    {
        internal Vector2 HitPosition { get; private set; }
        internal Vector2 PawnPosition { get; private set; }
        internal KnownColor Color { get; private set; }

        internal PawnOnThePassant(string address, KnownColor color)
        {
            Color = color;
            HitPosition = ChessUtils.GetPositionFromAddress(address);
            if (HitPosition == new Vector2(-1))
            {
                PawnPosition = HitPosition;
            }
            else
            {
                int direction = Color == KnownColor.White ? 1 : -1;
                PawnPosition = HitPosition + new Vector2(0, 1 * direction);
            }
        }

        internal PawnOnThePassant(Vector2 position, KnownColor color)
        {
            Color = color;
            HitPosition = position;
            if (HitPosition == new Vector2(-1))
            {
                PawnPosition = HitPosition;
            }
            else
            {
                int direction = Color == KnownColor.White ? 1 : -1;
                PawnPosition = HitPosition + new Vector2(0, 1 * direction);
            }
        }
    }
}

using System;
using System.Drawing;
using System.Numerics;

namespace ChessLogic
{
    internal struct ChessGameState
    {
        internal KnownColor TurnOwner;
        internal PawnOnThePassant PawnOnThePassant;
        internal bool WhiteKingCastlingAvailable;
        internal bool WhiteQueenCastlingAvailable;
        internal bool BlackKingCastlingAvailable;
        internal bool BlackQueenCastlingAvailable;
        internal int RuleOf50;
        internal int Turn;

        internal ChessGameState(string fen)
        {
            string[] fenParts = fen.Split(" ");

            WhiteKingCastlingAvailable = false;
            WhiteQueenCastlingAvailable = false;
            BlackKingCastlingAvailable = false;
            BlackQueenCastlingAvailable = false;

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

            TurnOwner = fenParts[1] == "w" ? KnownColor.White : KnownColor.Black;
            PawnOnThePassant = new PawnOnThePassant(fenParts[3], ChessUtils.InvertColor(TurnOwner));
            RuleOf50 = Int32.Parse(fenParts[4]);
            Turn = Int32.Parse(fenParts[5]);
        }

        internal ChessGameState(ChessGameState state)
        {
            TurnOwner = state.TurnOwner;
            PawnOnThePassant = state.PawnOnThePassant;
            WhiteKingCastlingAvailable = state.WhiteKingCastlingAvailable;
            WhiteQueenCastlingAvailable = state.WhiteQueenCastlingAvailable;
            BlackKingCastlingAvailable = state.BlackKingCastlingAvailable;
            BlackQueenCastlingAvailable = state.BlackQueenCastlingAvailable;
            RuleOf50 = state.RuleOf50;
            Turn = state.Turn;
        }

        public override string ToString()
        {

            string result = "";
            string castling = "";
            string passant;

            if(WhiteKingCastlingAvailable)
            {
                castling += "K";
            }
            if(WhiteQueenCastlingAvailable)
            {
                castling += "Q";
            }
            if(BlackKingCastlingAvailable)
            {
                castling += "k";
            }
            if(BlackQueenCastlingAvailable)
            {
                castling += "q";
            }
            if(castling == "")
            {
                castling = "-";
            }

            if(PawnOnThePassant.HitPosition == new Vector2(-1))
            {
                passant = "-";
            }
            else
            {
                passant = ChessUtils.GetAddresFromPosition(PawnOnThePassant.HitPosition);
            }

            result += TurnOwner == KnownColor.White ? "w" : "b";
            result += " ";
            result += castling;
            result += " ";
            result += passant;
            result += " ";
            result += RuleOf50;
            result += " ";
            result += Turn;

            return result;
        }
    }
}

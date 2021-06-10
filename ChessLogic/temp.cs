using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace ChessLogic
{

    internal abstract class BaseFigure
    {
        protected readonly List<Vector2> move_directions;

        internal FigureType FigureType { get; private set; }
        internal KnownColor Color { get; private set; }

        internal BaseFigure(FigureType type, KnownColor color)
        {
            move_directions = new List<Vector2>();
            FigureType = type;
            Color = color;
        }

        internal abstract List<Vector2> GetDirections();
    }

    internal class Pawn : BaseFigure
    {
        private readonly List<Vector2> attack_directions;

        internal Pawn(FigureType type, KnownColor color) : base(type, color)
        {
            attack_directions = new List<Vector2>();
        }

        internal override List<Vector2> GetDirections()
        {
            var result = new List<Vector2>();
            result.AddRange(attack_directions);
            result.AddRange(move_directions);

            return result;
        }
    }

    internal class MovementRules
    {
        internal int range;
        internal List<Vector2> directions;

        internal MovementRules(FigureType figureType)
        {
            directions = new List<Vector2>();

            switch (figureType)
            {
                case FigureType.Pawn:
                    range = 1;
                    directions.Add(new Vector2(0, 1));
                    directions.Add(new Vector2(0, 2));
                    directions.Add(new Vector2(1, 1));
                    directions.Add(new Vector2(-1, 1));
                    break;
                case FigureType.Knight:
                    range = 1;
                    directions.Add(new Vector2(1, 2));
                    directions.Add(new Vector2(2, 1));
                    directions.Add(new Vector2(1, -2));
                    directions.Add(new Vector2(2, -1));
                    directions.Add(new Vector2(-1, 2));
                    directions.Add(new Vector2(-2, 1));
                    directions.Add(new Vector2(-1, -2));
                    directions.Add(new Vector2(-2, -1));
                    break;
                case FigureType.Bishop:
                    range = 7;
                    directions.Add(new Vector2(1, 1));
                    directions.Add(new Vector2(1, -1));
                    directions.Add(new Vector2(-1, 1));
                    directions.Add(new Vector2(-1, -1));
                    break;
                case FigureType.Rook:
                    range = 7;
                    directions.Add(new Vector2(0, 1));
                    directions.Add(new Vector2(0, -1));
                    directions.Add(new Vector2(1, 0));
                    directions.Add(new Vector2(-1, 0));
                    break;
                case FigureType.Queen:
                    range = 7;
                    directions.Add(new Vector2(1, 1));
                    directions.Add(new Vector2(1, -1));
                    directions.Add(new Vector2(-1, 1));
                    directions.Add(new Vector2(-1, -1));
                    directions.Add(new Vector2(0, 1));
                    directions.Add(new Vector2(0, -1));
                    directions.Add(new Vector2(1, 0));
                    directions.Add(new Vector2(-1, 0));
                    break;
                case FigureType.King:
                    range = 1;
                    directions.Add(new Vector2(1, 1));
                    directions.Add(new Vector2(1, -1));
                    directions.Add(new Vector2(-1, 1));
                    directions.Add(new Vector2(-1, -1));
                    directions.Add(new Vector2(0, 1));
                    directions.Add(new Vector2(0, -1));
                    directions.Add(new Vector2(1, 0));
                    directions.Add(new Vector2(-1, 0));
                    break;
                default:
                    break;
            }
        }
    }

    internal class test
    {
        internal test()
        {
            var kingMovementRules = new MovementRules(FigureType.King);
        }
    }
}

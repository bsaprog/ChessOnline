using System;
using System.Collections.Generic;
using System.Numerics;
using System.Linq;

namespace ChessLogic
{
    internal struct MoveVariant
    {
        internal List<ChessBoardCell> Blockers { get; private set; }
        internal bool IsAvailable
        {
            get
            {
                if (Start.Figure != null && Start.Figure.Type == FigureType.Pawn && Start.Position.X != End.Position.X)
                {
                    return End.Figure != null && End.Figure.Color != Start.Figure.Color;
                }
                else
                {
                    return Blockers.Count == 0;
                }
            }
        }
        internal ChessBoardCell Start { get; private set; }
        internal ChessBoardCell End { get; private set; }

        internal MoveVariant(ChessBoardCell start, ChessBoardCell end)
        {
            Start = start;
            End = end;
            Blockers = new List<ChessBoardCell>();
        }
        internal void AddBlocker(ChessBoardCell cell)
        {
            Blockers.Add(cell);
        }
        internal void AddBlockersList(List<ChessBoardCell> blockers)
        {
            this.Blockers.AddRange(blockers);
        }
        public override string ToString()
        {
            return $"from {Start.Address} to {End.Address}";
        }
    }
    
    internal struct FigureMove
    {
        internal ChessBoardCell Start { get; private set; }
        internal ChessBoardCell End { get; private set; }
        internal Figure MovingFigure { get; private set; }
        internal Figure RemovingFigure { get; private set; }

        internal FigureMove(ChessBoardCell start, ChessBoardCell end)
        {
            Start = start;
            End = end;
            MovingFigure = start.Figure;
            RemovingFigure = end.Figure;
        }
    }

    internal class FigureMoveManager
    {
        private readonly ChessBoard _board;
        private readonly ChessGameState _state;
        private readonly Stack<FigureMove> _history;
        private readonly List<MoveVariant> _moveVariants;
        private readonly List<MoveVariant> _blockedMoveVariants;

        internal FigureMoveManager(ChessBoard board, ChessGameState state)
        {
            _board = board;
            _state = state;
            _history = new Stack<FigureMove>();
            _moveVariants = new List<MoveVariant>();
            _blockedMoveVariants = new List<MoveVariant>();

            RefreshMoveVariants();
            RefreshBlockedMoveVariants();
        }
        private void RefreshMoveVariants()
        {
            _moveVariants.Clear();
            List<ChessBoardCell> cells = _board.GetCellsWithFigures();
            foreach(ChessBoardCell cell in cells)
            {
                _moveVariants.AddRange(GetMoveVariantsFromCell(cell));
            }
        }
        private void RefreshBlockedMoveVariants()
        {
            _blockedMoveVariants.Clear();
            ChessBoardCell cellWithKing = _board.GetCellWithKing(_state.TurnOwner);
            
            List<MoveVariant> potentialAttacks = GetPotentialCellAttackMoveVariants(cellWithKing)
                .Where(move => move.Blockers.Count == 1)
                .ToList<MoveVariant>();

            foreach (MoveVariant attack in potentialAttacks)
            {
                List<Vector2> availableMovePositions = new List<Vector2>();

                Vector2 attackPosition = attack.Start.Position;
                Vector2 blockPosition = attack.Blockers[0].Position;
                Vector2 kingPosition = cellWithKing.Position;
                Vector2 deltaAttack = attackPosition - blockPosition;
                Vector2 deltaDefence = kingPosition - blockPosition;

                int attackCounter = (int)Math.Max(Math.Abs(deltaAttack.X), Math.Abs(deltaAttack.Y));
                int defenceCounter = (int)Math.Max(Math.Abs(deltaDefence.X), Math.Abs(deltaDefence.Y));

                Vector2 directionAttack = deltaAttack / attackCounter;
                Vector2 derectionDeffence = deltaDefence / defenceCounter;

                for (int i = attackCounter; i > 0; i --)
                {
                    availableMovePositions.Add(blockPosition + directionAttack * i);
                }
                for (int i = 0; i < defenceCounter; i++)
                {
                    availableMovePositions.Add(blockPosition + derectionDeffence * i);
                }

                List<MoveVariant> blockedMoveVariants = _moveVariants
                    .Where(move => move.Start.Position == blockPosition)
                    .Where(move => move.IsAvailable == true)
                    .Where(move => !availableMovePositions.Contains(move.End.Position))
                    .ToList<MoveVariant>();

                _blockedMoveVariants.AddRange(blockedMoveVariants);
            }
        }
        private void UpdateMoveVariantsAfterMove(ChessBoardCell start, ChessBoardCell end)
        {
            List<ChessBoardCell> cellsToUpdate = _moveVariants
                .Where(move => move.Start == start || move.End == start || move.Start == end || move.End == end)
                .Select(move => move.Start)
                .Distinct()
                .ToList<ChessBoardCell>();

            if(!cellsToUpdate.Contains(start))
            {
                cellsToUpdate.Add(start);
            }

            if(!cellsToUpdate.Contains(end))
            {
                cellsToUpdate.Add(end);
            }

            _moveVariants.RemoveAll(move => cellsToUpdate.Contains(move.Start));
            _moveVariants.AddRange(GetMoveVariantsFromCells(cellsToUpdate));
        }
        private bool KingIsUnderAttack(Color kingColor)
        {
            ChessBoardCell cellWithKing = _board.GetCellWithKing(kingColor);

            List<ChessBoardCell> potentialAttackers = GetPotentialCellAttackers(cellWithKing);
            List<MoveVariant> moveVariants = GetMoveVariantsFromCells(potentialAttackers);

            return moveVariants
                .Where(move => move.IsAvailable && move.End == cellWithKing)
                .Count() != 0;
        }
        private List<ChessBoardCell> GetPotentialCellAttackers(ChessBoardCell cell)
        {
            return GetPotentialCellAttackMoveVariants(cell)
                .Select(cell => cell.Start)
                .ToList<ChessBoardCell>();
        }
        private List<MoveVariant> GetPotentialCellAttackMoveVariants(ChessBoardCell cell)
        {
            return _moveVariants
                .Select(move => move)
                .Where(move => move.End == cell && move.Start.Figure.Color != cell.Figure.Color)
                .ToList<MoveVariant>();
        }
        private List<MoveVariant> GetMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();

            if (startCell != null && startCell.Figure != null)
            {
                switch (startCell.Figure.Type)
                {
                    case FigureType.Pawn:
                        result.AddRange(GetPawnMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.Knight:
                        result.AddRange(GetKnightMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.Bishop:
                        result.AddRange(GetBishopMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.Rook:
                        result.AddRange(GetRockMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.Queen:
                        result.AddRange(GetBishopMoveVariantsFromCell(startCell));
                        result.AddRange(GetRockMoveVariantsFromCell(startCell));
                        break;
                    case FigureType.King:
                        result.AddRange(GetKingMoveVariantsFromCell(startCell));
                        break;
                    default:
                        break;
                }
            }
            return result;
        }
        private List<MoveVariant> GetMoveVariantsFromCells(List<ChessBoardCell> cells)
        {
            List<MoveVariant> result = new List<MoveVariant>();

            foreach (ChessBoardCell cell in cells)
            {
                result.AddRange(GetMoveVariantsFromCell(cell));
            }

            return result;
        }
        private List<MoveVariant> GetPawnMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();

            Vector2 startPosition = startCell.Position;

            int direction = startCell.Figure.Color == Color.White ? 1 : -1;
            bool pawnIsOnStartPosition = startCell.Figure.Color == Color.White ? startPosition.Y == 1 : startPosition.Y == 6;

            ChessBoardCell endCellFrontDirection = _board.GetCellByPosition(startPosition + new Vector2(0, direction));
            ChessBoardCell endCellFrontPassDirection = _board.GetCellByPosition(startPosition + new Vector2(0, 2 * direction));
            ChessBoardCell endCellFrontRightDirection = _board.GetCellByPosition(startPosition + new Vector2(1, direction));
            ChessBoardCell endCellFronLeftDirection = _board.GetCellByPosition(startPosition + new Vector2(-1, direction));

            if (endCellFrontDirection != null)
            {
                MoveVariant moveVariant = new MoveVariant(startCell, endCellFrontDirection);
                if(endCellFrontDirection.Figure != null)
                {
                    moveVariant.AddBlocker(endCellFrontDirection);
                }

                result.Add(moveVariant);
            }

            if (endCellFrontPassDirection != null && pawnIsOnStartPosition)
            {
                MoveVariant moveVariant = new MoveVariant(startCell, endCellFrontPassDirection);
                if(endCellFrontDirection != null && endCellFrontDirection.Figure != null)
                {
                    moveVariant.AddBlocker(endCellFrontDirection);
                }

                if(endCellFrontPassDirection.Figure != null)
                {
                    moveVariant.AddBlocker(endCellFrontPassDirection);
                }

                result.Add(moveVariant);
            }

            if (endCellFrontRightDirection != null)
            {
                result.Add(new MoveVariant(startCell, endCellFrontRightDirection));
            }

            if (endCellFronLeftDirection != null)
            {
                result.Add(new MoveVariant(startCell, endCellFronLeftDirection));
            }

            return result;
        }
        private List<MoveVariant> GetKnightMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();

            List<Vector2> directions = new List<Vector2>();
            List<ChessBoardCell> endCells = new List<ChessBoardCell>();

            directions.Add(new Vector2(1, 2));
            directions.Add(new Vector2(2, 1));
            directions.Add(new Vector2(1, -2));
            directions.Add(new Vector2(2, -1));
            directions.Add(new Vector2(-1, 2));
            directions.Add(new Vector2(-2, 1));
            directions.Add(new Vector2(-1, -2));
            directions.Add(new Vector2(-2, -1));

            foreach (Vector2 direction in directions)
            {
                endCells.Add(_board.GetCellByPosition(startCell.Position + direction));
            }

            foreach (ChessBoardCell endCell in endCells)
            {
                if (endCell != null) {
                    MoveVariant moveVariant = new MoveVariant(startCell, endCell);
                    if (endCell.Figure != null && endCell.Figure.Color == startCell.Figure.Color)
                    {
                        moveVariant.AddBlocker(endCell);
                    }

                    result.Add(moveVariant);
                }
            }

            return result;
        }
        private List<MoveVariant> GetBishopMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();

            List<Vector2> directions = new List<Vector2>();

            directions.Add(new Vector2(1, 1));
            directions.Add(new Vector2(1, -1));
            directions.Add(new Vector2(-1, 1));
            directions.Add(new Vector2(-1, -1));

            foreach (Vector2 direction in directions)
            {
                List<ChessBoardCell> figuresOnThePath = new List<ChessBoardCell>();

                for (int i = 1; i < 8; i++)
                {
                    ChessBoardCell endCell = _board.GetCellByPosition(startCell.Position + direction * i);
                    if (endCell == null)
                    {
                        break;
                    }
                    else
                    {
                        MoveVariant moveVariant = new MoveVariant(startCell, endCell);
                        moveVariant.AddBlockersList(figuresOnThePath);

                        if (endCell.Figure != null)
                        {
                            if (endCell.Figure.Color == startCell.Figure.Color)
                            {
                                moveVariant.AddBlocker(endCell);
                            }

                            figuresOnThePath.Add(endCell);
                        }

                        result.Add(moveVariant);
                    }
                }
            }

            return result;
        }
        private List<MoveVariant> GetRockMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();

            List<Vector2> directions = new List<Vector2>();

            directions.Add(new Vector2(1, 0));
            directions.Add(new Vector2(0, 1));
            directions.Add(new Vector2(-1, 0));
            directions.Add(new Vector2(0, -1));

            foreach (Vector2 direction in directions)
            {
                List<ChessBoardCell> figuresOnThePath = new List<ChessBoardCell>();

                for (int i = 1; i < 8; i++)
                {
                    ChessBoardCell endCell = _board.GetCellByPosition(startCell.Position + direction * i);
                    if (endCell == null)
                    {
                        break;
                    }
                    else
                    {
                        MoveVariant moveVariant = new MoveVariant(startCell, endCell);
                        moveVariant.AddBlockersList(figuresOnThePath);

                        if (endCell.Figure != null)
                        {
                            if (endCell.Figure.Color == startCell.Figure.Color)
                            {
                                moveVariant.AddBlocker(endCell);
                            }

                            figuresOnThePath.Add(endCell);
                        }

                        result.Add(moveVariant);
                    }
                }
            }

            return result;
        }
        private List<MoveVariant> GetKingMoveVariantsFromCell(ChessBoardCell startCell)
        {
            List<MoveVariant> result = new List<MoveVariant>();

            List<Vector2> directions = new List<Vector2>();
            List<ChessBoardCell> endCells = new List<ChessBoardCell>();

            directions.Add(new Vector2(0, 1));
            directions.Add(new Vector2(1, 1));
            directions.Add(new Vector2(1, 0));
            directions.Add(new Vector2(1, -1));
            directions.Add(new Vector2(-1, 0));
            directions.Add(new Vector2(-1, -1));
            directions.Add(new Vector2(-1, 0));
            directions.Add(new Vector2(1, -1));

            foreach (Vector2 direction in directions)
            {
                endCells.Add(_board.GetCellByPosition(startCell.Position + direction));
            }

            foreach (ChessBoardCell endCell in endCells)
            {
                if (endCell != null) 
                {
                    MoveVariant moveVariant = new MoveVariant(startCell, endCell);

                    if (endCell.Figure != null && endCell.Figure.Color == startCell.Figure.Color)
                    {
                        moveVariant.AddBlocker(endCell);
                    }

                    result.Add(moveVariant);
                }
            }

            return result;
        }
        internal bool MakeMove(FigureMove move)
        {
            if(MoveIsReal(move) )
            {
                int direction = _state.TurnOwner == Color.White ? 1 : -1;
                Vector2 potpPosition;

                if (move.Start.Figure.Type == FigureType.Pawn && (Math.Abs((move.Start.Position - move.End.Position).Y) == 2))
                {
                    potpPosition = move.Start.Position + new Vector2(0, 1 * direction);
                }
                else
                {
                    potpPosition = new Vector2(-1);
                }

                if (move.Start.Figure.Type == FigureType.Pawn && move.End.Figure == null && move.Start.Position.X != move.End.Position.X)
                {
                    _board.GetCellByPosition(move.End.Position + new Vector2(0, 1 * -1 * direction)).SetFigure(null);
                }

                _state.PawnOnThePassant = new PawnOnThePassant(potpPosition, _state.TurnOwner);
                _state.RuleOf50 = move.Start.Figure.Type == FigureType.Pawn || move.End.Figure != null ? 0 : _state.RuleOf50 + 1;
                _state.Turn += _state.TurnOwner == Color.Black ? 1 : 0;
                _state.TurnOwner = ChessUtils.InvertColor(_state.TurnOwner);


                move.Start.SetFigure(null);
                move.End.SetFigure(move.MovingFigure);

                // _state.UpdateState(false);

                _history.Push(move);

                UpdateMoveVariantsAfterMove(move.Start, move.End);
                RefreshBlockedMoveVariants();

                return true;
            }

            return false;
        }

        internal bool MakeMove(string path)
        {
            ChessBoardCell start = _board.GetCellByAddress(path.Substring(0, 2));
            ChessBoardCell end = _board.GetCellByAddress(path.Substring(2, 2));

            FigureMove move = new FigureMove(start, end);

            return MakeMove(move);
        }
        /*
       internal void UndoLastMove()
       {
           if (_history.Count > 0)
           {
               FigureMove move = _history.Pop();

               move.Start.SetFigure(move.MovingFigure);
               move.End.SetFigure(move.RemovingFigure);
           }
       }
         */
       private bool MoveIsReal(FigureMove move)
       {
            MoveVariant variant = _moveVariants
                .Where(v => v.Start == move.Start)
                .Where(v => v.End == move.End)
                .Where(v => v.IsAvailable)
                .FirstOrDefault();

            return !variant.Equals(new MoveVariant()) && !_blockedMoveVariants.Contains(variant);
       }
    }

    public class ChessGame
    {
        private readonly ChessBoard _board;
        private readonly ChessGameState _state;
        private readonly FigureMoveManager _moveManager;

        public ChessGame(string fen = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1")
        {
            _board = new ChessBoard(fen);
            _state = new ChessGameState(fen);
            _moveManager = new FigureMoveManager(_board, _state);
        }
        public Figure GetFigureAt(Vector2 position)
        {
            ChessBoardCell cell = _board.GetCellByPosition(position);
            return cell.Figure;
        }
        public string GetStateString()
        {
            return
                $"Turn #{_state.Turn}. Turn owner: {_state.TurnOwner}. Rule of 50: {_state.RuleOf50}\n" +
                $"CastlingAvailable:\n" +
                $"  White King: {_state.WhiteKingCastlingAvailable}\n" +
                $"  White Queen: {_state.WhiteQueenCastlingAvailable}\n" +
                $"  Black King: {_state.BlackKingCastlingAvailable}\n" +
                $"  Black Queen: {_state.BlackQueenCastlingAvailable}\n" +
                $"Pawn on the pass: {ChessUtils.GetAddresFromPosition(_state.PawnOnThePassant.HitPosition)}";
        }
        public bool MakeMove(string path)
        {
            return _moveManager.MakeMove(path);
        }
    }

    internal struct PawnOnThePassant
    {
        internal Vector2 HitPosition { get; private set; }
        internal Vector2 PawnPosition { get; private set; }
        internal Color OwnerColor { get; private set; }

        internal PawnOnThePassant(string adres, Color ownerColor)
        {
            int direction = ownerColor == Color.White ? 1 : -1;

            OwnerColor = ownerColor;
            HitPosition = ChessUtils.GetPositionFromAddress(adres);
            PawnPosition = HitPosition + new Vector2(0, 1 * direction);
        }
        internal PawnOnThePassant(Vector2 position, Color ownerColor)
        {
            int direction = ownerColor == Color.White ? 1 : -1;

            OwnerColor = ownerColor;
            HitPosition = position;
            PawnPosition = HitPosition + new Vector2(0, 1 * direction);
        }
    }

    internal class ChessGameState
    {
        internal Color TurnOwner;
        internal bool WhiteKingCastlingAvailable = false;
        internal bool WhiteQueenCastlingAvailable = false;
        internal bool BlackKingCastlingAvailable = false;
        internal bool BlackQueenCastlingAvailable = false;
        internal PawnOnThePassant PawnOnThePassant;
        internal int RuleOf50;
        internal int Turn;

        internal ChessGameState(string fen)
        {
            string[] fenParts = fen.Split(" ");

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

            TurnOwner = fenParts[1] == "w" ? Color.White : Color.Black;
            PawnOnThePassant = new PawnOnThePassant(fenParts[3], ChessUtils.InvertColor(TurnOwner) );
            RuleOf50 = Int32.Parse(fenParts[4]);
            Turn = Int32.Parse(fenParts[5]);
        }
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
        public override string ToString()
        {
            return $"{Color.ToString()} {Type.ToString()}";
        }
    }

    internal class ChessBoard
    {
        internal readonly Dictionary<Vector2, ChessBoardCell> Cells = new Dictionary<Vector2, ChessBoardCell>();
        internal ChessBoard(string fen)
        {
            fen = fen.Split(' ')[0];
            string[] lines = fen.Split('/');

            Cells = new Dictionary<Vector2, ChessBoardCell>();
            for (int i = 0; i < 8; i++)
            {
                for (int j = 0; j < 8; j++)
                {
                    ChessBoardCell cell = new ChessBoardCell(new Vector2(i, j));
                    Cells.Add(cell.Position, cell);
                }
            }

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
                        cell.SetFigure(new Figure(type, color));
                    }
                }
            }
        }
        internal ChessBoardCell GetCellByPosition(Vector2 position)
        {
            ChessBoardCell result = null;

            if(Cells.ContainsKey(position))
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
        internal ChessBoardCell GetCellWithKing(Color kingColor)
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
                .ToList<ChessBoardCell>();
        }
    }

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
            return $"{Address} {Figure.ToString()}";
        }
    }

    internal static class ChessUtils
    {
        internal static string GetAddresFromPosition(Vector2 position)
        {
            string result = "";

            int x = (Int32)position.X;
            int y = (Int32)position.Y;

            if(x >= 0 && x <= 7 && y >= 0 && y <= 7)
            {
                string[] conformity = new string[8];
                conformity[0] = "a";
                conformity[1] = "b";
                conformity[2] = "c";
                conformity[3] = "d";
                conformity[4] = "e";
                conformity[5] = "f";
                conformity[6] = "g";
                conformity[7] = "h";

                result = conformity[x] + (y + 1).ToString();
            }

            return result;
        }
        internal static Vector2 GetPositionFromAddress(string address)
        {
            Vector2 result = new Vector2(-1, -1);
            
            if(address.Length == 2)
            {
                char x = address[0];
                int y = -1;
                Int32.TryParse(address[1].ToString(), out y);

                if (x >= 'a' && x <= 'h' && y >= 1 && y <= 8)
                {
                    Dictionary<char, int> conformity = new Dictionary<char, int>();
                    conformity.Add('a', 0);
                    conformity.Add('b', 1);
                    conformity.Add('c', 2);
                    conformity.Add('d', 3);
                    conformity.Add('e', 4);
                    conformity.Add('f', 5);
                    conformity.Add('g', 6);
                    conformity.Add('h', 7);

                    result = new Vector2(conformity[x], y - 1);
                }
            }

            return result;
        }
        internal static Color InvertColor(Color color)
        {
            Color result;
            if(color == Color.Black)
            {
                result = Color.White;
            }
            else if(color == Color.White)
            {
                result = Color.Black;
            }
            else
            {
                result = color;
            }

            return result;
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
}
namespace Sokoban;

/// <summary>
/// Lépés irány
/// </summary>
public class MoveDirection
{
    public int DRow { get; }
    public int DCol { get; }
    public string Name { get; }
    public string Arrow { get; }

    public MoveDirection(int dRow, int dCol, string name, string arrow)
    {
        DRow = dRow;
        DCol = dCol;
        Name = name;
        Arrow = arrow;
    }

    public static readonly MoveDirection Up = new(-1, 0, "fel", "↑");
    public static readonly MoveDirection Down = new(1, 0, "le", "↓");
    public static readonly MoveDirection Left = new(0, -1, "bal", "←");
    public static readonly MoveDirection Right = new(0, 1, "jobb", "→");

    public static readonly MoveDirection[] All = { Up, Down, Left, Right };
}

/// <summary>
/// AI megoldó lépés
/// </summary>
public class SolverMove
{
    public MoveDirection Direction { get; set; } = MoveDirection.Up;
    public bool Pushed { get; set; }
}

/// <summary>
/// Megoldás eredménye
/// </summary>
public class SolutionResult
{
    public bool Success { get; set; }
    public List<SolverMove> Moves { get; set; } = new();
    public int Iterations { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// AI Megoldó (BFS/A* algoritmus)
/// </summary>
public class AISolver
{
    private const int MaxIterations = 50000;

    /// <summary>
    /// Manhattan-távolság heurisztika
    /// </summary>
    public int CalculateHeuristic(SokobanGame game)
    {
        int totalDistance = 0;
        var goals = new List<(int Row, int Col)>();
        var boxes = new List<(int Row, int Col)>();

        for (int row = 0; row < game.Height; row++)
        {
            for (int col = 0; col < game.Width; col++)
            {
                char tile = game.GetTile(row, col);
                if (tile == Tiles.Goal || tile == Tiles.PlayerOnGoal)
                {
                    goals.Add((row, col));
                }
                if (tile == Tiles.Box)
                {
                    boxes.Add((row, col));
                }
            }
        }

        foreach (var box in boxes)
        {
            int minDist = int.MaxValue;
            foreach (var goal in goals)
            {
                int dist = Math.Abs(box.Row - goal.Row) + Math.Abs(box.Col - goal.Col);
                minDist = Math.Min(minDist, dist);
            }
            if (minDist != int.MaxValue)
                totalDistance += minDist;
        }

        return totalDistance;
    }

    /// <summary>
    /// Deadlock ellenőrzés
    /// </summary>
    public bool IsDeadlock(SokobanGame game, int row, int col)
    {
        if (game.IsGoal(row, col))
            return false;

        bool up = game.IsWall(row - 1, col);
        bool down = game.IsWall(row + 1, col);
        bool left = game.IsWall(row, col - 1);
        bool right = game.IsWall(row, col + 1);

        return (up && left) || (up && right) || (down && left) || (down && right);
    }

    /// <summary>
    /// Megoldás keresése BFS/A* algoritmussal
    /// </summary>
    public SolutionResult Solve(SokobanGame game)
    {
        if (game.IsSolved())
        {
            return new SolutionResult { Success = true };
        }

        var visited = new HashSet<string>();
        var queue = new PriorityQueue<(SokobanGame Game, List<SolverMove> Moves), int>();

        var initialGame = CloneGame(game);
        queue.Enqueue((initialGame, new List<SolverMove>()), CalculateHeuristic(initialGame));
        visited.Add(initialGame.GetStateKey());

        int iterations = 0;

        while (queue.Count > 0 && iterations < MaxIterations)
        {
            iterations++;

            var (currentGame, currentMoves) = queue.Dequeue();

            foreach (var dir in MoveDirection.All)
            {
                var newGame = CloneGame(currentGame);
                var result = newGame.Move(dir.DRow, dir.DCol);

                if (result.Success)
                {
                    string stateKey = newGame.GetStateKey();

                    if (!visited.Contains(stateKey))
                    {
                        // Deadlock ellenőrzés
                        if (result.Pushed)
                        {
                            int boxRow = currentGame.PlayerPosition.Row + dir.DRow * 2;
                            int boxCol = currentGame.PlayerPosition.Col + dir.DCol * 2;
                            if (IsDeadlock(newGame, boxRow, boxCol))
                            {
                                continue;
                            }
                        }

                        visited.Add(stateKey);

                        var newMoves = new List<SolverMove>(currentMoves)
                        {
                            new SolverMove { Direction = dir, Pushed = result.Pushed }
                        };

                        if (newGame.IsSolved())
                        {
                            return new SolutionResult
                            {
                                Success = true,
                                Moves = newMoves,
                                Iterations = iterations
                            };
                        }

                        int priority = newMoves.Count + CalculateHeuristic(newGame);
                        queue.Enqueue((newGame, newMoves), priority);
                    }
                }
            }
        }

        return new SolutionResult
        {
            Success = false,
            Iterations = iterations,
            Reason = iterations >= MaxIterations ? "timeout" : "unsolvable"
        };
    }

    /// <summary>
    /// Következő lépés megtalálása
    /// </summary>
    public (SolverMove? Move, int TotalMoves, int PushCount)? GetNextMove(SokobanGame game)
    {
        var solution = Solve(game);
        if (solution.Success && solution.Moves.Count > 0)
        {
            return (
                solution.Moves[0],
                solution.Moves.Count,
                solution.Moves.Count(m => m.Pushed)
            );
        }
        return null;
    }

    /// <summary>
    /// Játék klónozása
    /// </summary>
    private SokobanGame CloneGame(SokobanGame original)
    {
        var clone = new SokobanGame(original.CurrentLevel);
        // A térkép állapotát kell másolni
        var map = original.CloneMap();
        var field = typeof(SokobanGame).GetField("_map", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(clone, map);
        
        var playerRowField = typeof(SokobanGame).GetField("_playerRow", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var playerColField = typeof(SokobanGame).GetField("_playerCol", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        playerRowField?.SetValue(clone, original.PlayerPosition.Row);
        playerColField?.SetValue(clone, original.PlayerPosition.Col);
        
        return clone;
    }
}

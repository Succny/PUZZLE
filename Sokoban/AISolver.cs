namespace Sokoban;

// ============================================================================
// AI LAYER / MESTERSÉGES INTELLIGENCIA RÉTEG
// Ez a fájl az AI megoldó algoritmust tartalmazza.
// A szakdolgozatban hivatkozható: AI Solver réteg.
// ============================================================================

/// <summary>
/// [AI Layer]
/// Lépés irány reprezentáció az AI algoritmushoz.
/// </summary>
public class MoveDirection(int dRow, int dCol, string name, string arrow, string directionalSuffix)
{
    /// <summary>Sor irányú elmozdulás (-1: fel, 1: le, 0: nincs)</summary>
    public int DRow { get; } = dRow;
    /// <summary>Oszlop irányú elmozdulás (-1: bal, 1: jobb, 0: nincs)</summary>
    public int DCol { get; } = dCol;
    /// <summary>Magyar megnevezés</summary>
    public string Name { get; } = name;
    /// <summary>Nyíl karakter vizuális megjelenítéshez</summary>
    public string Arrow { get; } = arrow;
    /// <summary>Magyar irányhatározó rag (-ra/-fele)</summary>
    public string DirectionalSuffix { get; } = directionalSuffix;

    public static readonly MoveDirection Up = new(-1, 0, "fel", "↑", "fele");
    public static readonly MoveDirection Down = new(1, 0, "le", "↓", "fele");
    public static readonly MoveDirection Left = new(0, -1, "bal", "←", "ra");
    public static readonly MoveDirection Right = new(0, 1, "jobb", "→", "ra");

    public static readonly MoveDirection[] All = { Up, Down, Left, Right };
}

/// <summary>
/// [AI Layer]
/// AI megoldó lépés - egy lépés adatait tartalmazza.
/// </summary>
public class SolverMove
{
    /// <summary>A lépés iránya</summary>
    public MoveDirection Direction { get; set; } = MoveDirection.Up;
    /// <summary>Történt-e láda tolás a lépés során</summary>
    public bool Pushed { get; set; }
}

/// <summary>
/// [AI Layer]
/// Megoldás eredménye - az AI keresés kimenetelét tartalmazza.
/// </summary>
public class SolutionResult
{
    /// <summary>Sikerült-e megoldást találni</summary>
    public bool Success { get; set; }
    /// <summary>A megoldáshoz szükséges lépések listája</summary>
    public List<SolverMove> Moves { get; set; } = new();
    /// <summary>A keresés során végrehajtott iterációk száma</summary>
    public int Iterations { get; set; }
    /// <summary>Sikertelen keresés esetén az ok (pl. "timeout", "exhausted")</summary>
    public string? Reason { get; set; }

    /// <summary>
    /// Ellenőrzi, hogy a sikertelen keresés timeout miatt történt-e.
    /// </summary>
    public bool IsTimeout => !Success && Reason == "timeout";

    /// <summary>
    /// Ellenőrzi, hogy a sikertelen keresés azért történt-e, mert kimerült az állapottér.
    /// Ez valószínűleg azt jelenti, hogy nincs megoldás, de nem 100% biztos.
    /// </summary>
    public bool IsExhausted => !Success && Reason == "exhausted";
}

/// <summary>
/// [AI Layer]
/// AI Megoldó algoritmus A* kereséssel és Manhattan-távolság heurisztikával.
/// 
/// A szakdolgozatban hivatkozható:
/// - A* algoritmus implementáció Sokoban megoldásához
/// - Manhattan-távolság heurisztika
/// - Állapottér keresés visited halmazon
/// - Deadlock állapotok szűrése a keresés során
/// - Konfigurálható iteráció limit a futási idő korlátozásához
/// </summary>
public class AISolver
{
    private const int DefaultMaxIterations = 100000;

    public int MaxIterations { get; set; } = DefaultMaxIterations;

    public int LastIterationCount { get; private set; } = 0;

    // Cache for goal positions to avoid repeated scanning
    private List<(int Row, int Col)>? _cachedGoals;
    private SokobanGame? _lastGameInstance;

    /// <summary>
    /// Alapértelmezett konstruktor 100000-es MaxIterations értékkel.
    /// </summary>
    public AISolver()
    {
    }

    /// <summary>
    /// Konstruktor egyedi MaxIterations értékkel.
    /// </summary>
    /// <param name="maxIterations">Maximális iterációszám (alapértelmezett: 100000)</param>
    /// <exception cref="ArgumentException">Ha maxIterations nem pozitív</exception>
    public AISolver(int maxIterations)
    {
        if (maxIterations <= 0)
            throw new ArgumentException("A maximális iterációszámnak pozitívnak kell lennie.", nameof(maxIterations));

        MaxIterations = maxIterations;
    }

    /// <summary>
    /// Manhattan-távolság heurisztika számítása.
    ///
    /// Az algoritmus minden nem célhelyen lévő ládához kiszámítja a legközelebbi
    /// cél Manhattan-távolságát, és ezek összegét adja vissza.
    ///
    /// FONTOS: A heurisztika NEM ellenőriz deadlockot - ezt a Solve metódus
    /// külön kezeli a lépések szűrésénél. Ez biztosítja, hogy a heurisztika
    /// admissible (megengedett) maradjon és ne zárjon ki túl sok állapotot.
    ///
    /// OPTIMALIZÁCIÓ: A célhelyek pozíciói cachelve vannak, mivel ezek nem
    /// változnak a játék során. Csak a ládák pozícióit kell újra kiszámolni.
    ///
    /// A szakdolgozatban hivatkozható: heurisztika tervezés A* kereséshez,
    /// admissible heurisztika tulajdonságok.
    /// </summary>
    /// <param name="game">Az aktuális játékállapot</param>
    /// <returns>A heurisztika értéke (alacsonyabb = jobb)</returns>
    /// <exception cref="ArgumentNullException">Ha game null</exception>
    public int CalculateHeuristic(SokobanGame game)
    {
        ArgumentNullException.ThrowIfNull(game);

        // Cache goal positions since they never change during solving
        if (_cachedGoals == null || _lastGameInstance != game)
        {
            _cachedGoals = [];
            _lastGameInstance = game;

            for (int row = 0; row < game.Height; row++)
            {
                for (int col = 0; col < game.Width; col++)
                {
                    char tile = game.GetTile(row, col);
                    if (tile == Tiles.Goal || tile == Tiles.PlayerOnGoal || tile == Tiles.BoxOnGoal)
                    {
                        _cachedGoals.Add((row, col));
                    }
                }
            }
        }

        int totalDistance = 0;
        var boxes = new List<(int Row, int Col)>(_cachedGoals.Count);

        // Only scan for boxes (goals are cached)
        for (int row = 0; row < game.Height; row++)
        {
            for (int col = 0; col < game.Width; col++)
            {
                char tile = game.GetTile(row, col);
                if (tile == Tiles.Box || tile == Tiles.BoxOnGoal)
                {
                    boxes.Add((row, col));
                }
            }
        }

        foreach (var box in boxes)
        {
            int minDist = int.MaxValue;
            foreach (var goal in _cachedGoals)
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
    /// Deadlock (zsákutca) ellenőrzés.
    /// 
    /// Ellenőrzi, hogy egy adott pozíción lévő láda deadlock állapotban van-e,
    /// azaz geometriailag lehetetlen-e célhelyre mozgatni.
    /// 
    /// FONTOS: Csak akkor jelent deadlockot, ha biztosan menthetetlen az állapot.
    /// Ha bizonytalan, inkább NEM tekinti deadlocknak, hogy ne zárjon ki
    /// megoldható állapotokat.
    /// 
    /// MEGJEGYZÉS: A hívó felelőssége ellenőrizni, hogy az adott pozíción 
    /// valóban láda van-e. A Solve metódus ezt biztosítja a push detektálással.
    /// 
    /// A szakdolgozatban hivatkozható: AI keresés optimalizálás deadlock felismeréssel.
    /// </summary>
    /// <param name="game">Az aktuális játékállapot</param>
    /// <param name="row">A láda sor pozíciója</param>
    /// <param name="col">A láda oszlop pozíciója</param>
    /// <returns>True, ha a láda biztosan deadlock állapotban van</returns>
    public static bool IsDeadlock(SokobanGame game, int row, int col)
    {
        // Használjuk a SokobanGame konzervatív deadlock ellenőrzését
        return game.CheckDeadlock(row, col);
    }

    /// <summary>
    /// Megoldás keresése A* algoritmussal.
    ///
    /// Az algoritmus prioritásos sorban tárolja a vizsgálandó állapotokat,
    /// ahol a prioritás = eddigi lépések száma + heurisztika (f = g + h).
    ///
    /// Optimalizációk:
    /// - Visited halmaz a már vizsgált állapotok kiszűrésére
    /// - Deadlock állapotok korai szűrése
    /// - Konfigurálható iteráció limit
    ///
    /// A szakdolgozatban hivatkozható: A* algoritmus implementáció,
    /// állapottér keresés, prioritásos sor alkalmazása.
    /// </summary>
    /// <param name="game">A kiinduló játékállapot</param>
    /// <returns>A megoldás eredménye (siker/kudarc, lépések, iterációk)</returns>
    /// <exception cref="ArgumentNullException">Ha game null</exception>
    public SolutionResult Solve(SokobanGame game)
    {
        ArgumentNullException.ThrowIfNull(game);

        LastIterationCount = 0;

        if (game.IsSolved())
        {
            return new SolutionResult { Success = true };
        }

        var visited = new HashSet<string>();
        var queue = new PriorityQueue<(SokobanGame Game, List<SolverMove> Moves), int>();

        var initialGame = game.Clone();
        queue.Enqueue((initialGame, (List<SolverMove>)[]), CalculateHeuristic(initialGame));
        visited.Add(initialGame.GetStateKey());

        int iterations = 0;

        while (queue.Count > 0 && iterations < MaxIterations)
        {
            iterations++;

            var (currentGame, currentMoves) = queue.Dequeue();

            foreach (var dir in MoveDirection.All)
            {
                var newGame = currentGame.Clone();
                var result = newGame.Move(dir.DRow, dir.DCol);

                if (result.Success)
                {
                    string stateKey = newGame.GetStateKey();

                    if (!visited.Contains(stateKey))
                    {
                        // Deadlock ellenőrzés - csak ha láda tolás történt
                        if (result.Pushed)
                        {
                            // A láda új pozíciója: az új játékos pozíció + 1 * irány
                            int boxRow = newGame.PlayerPosition.Row + dir.DRow;
                            int boxCol = newGame.PlayerPosition.Col + dir.DCol;
                            if (IsDeadlock(newGame, boxRow, boxCol))
                            {
                                continue;
                            }
                        }

                        visited.Add(stateKey);

                        List<SolverMove> newMoves = [..currentMoves, new SolverMove { Direction = dir, Pushed = result.Pushed }];

                        if (newGame.IsSolved())
                        {
                            LastIterationCount = iterations;
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

        LastIterationCount = iterations;
        string reason = iterations >= MaxIterations ? "timeout" : "exhausted";

        return new SolutionResult
        {
            Success = false,
            Iterations = iterations,
            Reason = reason
        };
    }

    /// <summary>
    /// Következő lépés megtalálása.
    /// 
    /// Megoldja a játékot és visszaadja az első lépést a megoldásból.
    /// Hasznos a hint rendszer számára, hogy egy lépést javasoljon.
    /// 
    /// A szakdolgozatban hivatkozható: AI-alapú lépésjavaslat generálás,
    /// ember-AI együttműködés a Sokoban megoldásában.
    /// </summary>
    /// <param name="game">Az aktuális játékállapot</param>
    /// <returns>A következő lépés adatai (irány, összes hátralévő lépés, tolások száma), vagy null ha nincs megoldás</returns>
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
    /// Következő lépés megtalálása részletes eredménnyel.
    /// 
    /// Megoldja a játékot és visszaadja az első lépést a megoldásból,
    /// valamint a teljes megoldás eredményét (beleértve a timeout/exhausted információt).
    /// </summary>
    /// <param name="game">Az aktuális játékállapot</param>
    /// <returns>Tuple: (következő lépés info vagy null, megoldás eredmény)</returns>
    public ((SolverMove? Move, int TotalMoves, int PushCount)? NextMove, SolutionResult Solution) GetNextMoveWithDetails(SokobanGame game)
    {
        var solution = Solve(game);
        if (solution.Success && solution.Moves.Count > 0)
        {
            return (
                (solution.Moves[0], solution.Moves.Count, solution.Moves.Count(m => m.Pushed)),
                solution
            );
        }
        return (null, solution);
    }

}

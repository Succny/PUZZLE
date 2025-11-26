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
public class MoveDirection
{
    /// <summary>Sor irányú elmozdulás (-1: fel, 1: le, 0: nincs)</summary>
    public int DRow { get; }
    /// <summary>Oszlop irányú elmozdulás (-1: bal, 1: jobb, 0: nincs)</summary>
    public int DCol { get; }
    /// <summary>Magyar megnevezés</summary>
    public string Name { get; }
    /// <summary>Nyíl karakter vizuális megjelenítéshez</summary>
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
    /// <summary>
    /// Büntetés érték deadlock állapotokhoz a heurisztikában.
    /// Nagyon nagy érték, ami biztosítja, hogy a deadlock állapotok
    /// ne kerüljenek a prioritási sor elejére.
    /// </summary>
    private const int DeadlockPenalty = int.MaxValue / 2;

    /// <summary>
    /// Alapértelmezett maximális iterációszám.
    /// </summary>
    private const int DefaultMaxIterations = 100000;

    /// <summary>
    /// Maximális iterációszám a keresés során.
    /// A futási idő korlátozására szolgál, hogy elkerüljük a túl hosszú számításokat.
    /// Alapértelmezett érték: 100000.
    /// 
    /// A szakdolgozatban hivatkozható: számítási komplexitás korlátozása.
    /// </summary>
    public int MaxIterations { get; set; } = DefaultMaxIterations;

    /// <summary>
    /// Debug mód engedélyezése - kiírja az iterációk számát a keresés végén.
    /// </summary>
    public bool DebugMode { get; set; } = false;

    /// <summary>
    /// Az utolsó keresés során elért iterációk száma.
    /// </summary>
    public int LastIterationCount { get; private set; } = 0;

    /// <summary>
    /// Az utolsó keresés leállási oka.
    /// </summary>
    public string? LastSearchResult { get; private set; }

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
    public AISolver(int maxIterations)
    {
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
    /// A szakdolgozatban hivatkozható: heurisztika tervezés A* kereséshez,
    /// admissible heurisztika tulajdonságok.
    /// </summary>
    /// <param name="game">Az aktuális játékállapot</param>
    /// <returns>A heurisztika értéke (alacsonyabb = jobb)</returns>
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
    /// Deadlock (zsákutca) ellenőrzés.
    /// 
    /// Ellenőrzi, hogy egy adott pozíción lévő láda deadlock állapotban van-e,
    /// azaz geometriailag lehetetlen-e célhelyre mozgatni.
    /// 
    /// FONTOS: Csak akkor jelent deadlockot, ha biztosan menthetetlen az állapot.
    /// Ha bizonytalan, inkább NEM tekinti deadlocknak, hogy ne zárjon ki
    /// megoldható állapotokat.
    /// 
    /// A szakdolgozatban hivatkozható: AI keresés optimalizálás deadlock felismeréssel.
    /// </summary>
    /// <param name="game">Az aktuális játékállapot</param>
    /// <param name="row">A láda sor pozíciója</param>
    /// <param name="col">A láda oszlop pozíciója</param>
    /// <returns>True, ha a láda biztosan deadlock állapotban van</returns>
    public bool IsDeadlock(SokobanGame game, int row, int col)
    {
        // Ellenőrizzük, hogy valóban van-e láda az adott pozíción
        if (!game.IsBox(row, col))
        {
            return false;
        }

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
    public SolutionResult Solve(SokobanGame game)
    {
        LastIterationCount = 0;
        LastSearchResult = null;

        if (game.IsSolved())
        {
            LastSearchResult = "already_solved";
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
                        // Deadlock ellenőrzés - csak ha láda tolás történt
                        if (result.Pushed)
                        {
                            // A láda új pozíciója: játékos eredeti pozíció + 2 * irány
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
                            LastIterationCount = iterations;
                            LastSearchResult = "success";
                            if (DebugMode)
                            {
                                Console.WriteLine($"[DEBUG] Solver: megoldás találva {iterations} iteráció után");
                            }
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
        LastSearchResult = reason;

        if (DebugMode)
        {
            Console.WriteLine($"[DEBUG] Solver: keresés leállt - {reason}, {iterations} iteráció, {visited.Count} állapot vizsgálva");
        }

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

    /// <summary>
    /// Játék klónozása a keresés során.
    /// Minden új állapot független másolat, hogy a keresés ne módosítsa az eredetit.
    /// </summary>
    private SokobanGame CloneGame(SokobanGame original)
    {
        return original.Clone();
    }
}

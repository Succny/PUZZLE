namespace Sokoban;

/// <summary>
/// Mozgatás eredménye
/// </summary>
public class MoveResult
{
    public bool Success { get; set; }
    public bool Pushed { get; set; }
    public bool Solved { get; set; }
    public bool Deadlock { get; set; }
    public string? Reason { get; set; }
}

/// <summary>
/// Játékállapot mentése undo-hoz
/// </summary>
public class GameState
{
    public char[,] Map { get; }
    public int PlayerRow { get; }
    public int PlayerCol { get; }
    public int Moves { get; }
    public int Pushes { get; }

    public GameState(char[,] map, int playerRow, int playerCol, int moves, int pushes)
    {
        int height = map.GetLength(0);
        int width = map.GetLength(1);
        Map = new char[height, width];
        Array.Copy(map, Map, map.Length);
        PlayerRow = playerRow;
        PlayerCol = playerCol;
        Moves = moves;
        Pushes = pushes;
    }
}

/// <summary>
/// Sokoban játék logika
/// </summary>
public class SokobanGame
{
    private char[,] _map;
    private char[,] _originalMap;
    private int _playerRow;
    private int _playerCol;
    private readonly Stack<GameState> _history;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Moves { get; private set; }
    public int Pushes { get; private set; }
    public Level CurrentLevel { get; private set; }

    public SokobanGame(Level level)
    {
        CurrentLevel = level;
        _history = new Stack<GameState>();
        _map = new char[0, 0];
        _originalMap = new char[0, 0];
        Initialize(level);
    }

    /// <summary>
    /// Játék inicializálása
    /// </summary>
    private void Initialize(Level level)
    {
        Height = level.Map.Length;
        Width = level.Map.Max(row => row.Length);
        
        _map = new char[Height, Width];
        _originalMap = new char[Height, Width];

        for (int row = 0; row < Height; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                char tile = col < level.Map[row].Length ? level.Map[row][col] : ' ';
                _map[row, col] = tile;
                _originalMap[row, col] = tile;

                if (tile == Tiles.Player || tile == Tiles.PlayerOnGoal)
                {
                    _playerRow = row;
                    _playerCol = col;
                }
            }
        }

        Moves = 0;
        Pushes = 0;
        _history.Clear();
    }

    /// <summary>
    /// Pálya újraindítása
    /// </summary>
    public void Restart()
    {
        Initialize(CurrentLevel);
    }

    /// <summary>
    /// Elem lekérése adott pozíción
    /// </summary>
    public char GetTile(int row, int col)
    {
        if (row < 0 || row >= Height || col < 0 || col >= Width)
            return Tiles.Wall;
        return _map[row, col];
    }

    /// <summary>
    /// Ellenőrzi, hogy a pozíció fal-e
    /// </summary>
    public bool IsWall(int row, int col) => GetTile(row, col) == Tiles.Wall;

    /// <summary>
    /// Ellenőrzi, hogy a pozíción láda van-e
    /// </summary>
    public bool IsBox(int row, int col)
    {
        var tile = GetTile(row, col);
        return tile == Tiles.Box || tile == Tiles.BoxOnGoal;
    }

    /// <summary>
    /// Ellenőrzi, hogy a pozíció célhely-e
    /// </summary>
    public bool IsGoal(int row, int col)
    {
        var tile = GetTile(row, col);
        return tile == Tiles.Goal || tile == Tiles.PlayerOnGoal || tile == Tiles.BoxOnGoal;
    }

    /// <summary>
    /// Játékos pozíciója
    /// </summary>
    public (int Row, int Col) PlayerPosition => (_playerRow, _playerCol);

    /// <summary>
    /// Játékos mozgatása
    /// </summary>
    public MoveResult Move(int dRow, int dCol)
    {
        int newRow = _playerRow + dRow;
        int newCol = _playerCol + dCol;

        // Fal ellenőrzése
        if (IsWall(newRow, newCol))
        {
            return new MoveResult { Success = false, Reason = "wall" };
        }

        // Láda ellenőrzése
        if (IsBox(newRow, newCol))
        {
            int boxNewRow = newRow + dRow;
            int boxNewCol = newCol + dCol;

            // Láda mögötti pozíció ellenőrzése
            if (IsWall(boxNewRow, boxNewCol) || IsBox(boxNewRow, boxNewCol))
            {
                return new MoveResult { Success = false, Reason = "blocked" };
            }

            // Állapot mentése undo-hoz
            SaveState();

            // Láda mozgatása
            MoveBox(newRow, newCol, boxNewRow, boxNewCol);
            
            // Játékos mozgatása
            MovePlayer(newRow, newCol);

            Moves++;
            Pushes++;

            bool deadlock = CheckDeadlock(boxNewRow, boxNewCol);

            return new MoveResult
            {
                Success = true,
                Pushed = true,
                Solved = IsSolved(),
                Deadlock = deadlock
            };
        }

        // Szabad mozgás
        SaveState();
        MovePlayer(newRow, newCol);
        Moves++;

        return new MoveResult { Success = true, Pushed = false, Solved = IsSolved() };
    }

    /// <summary>
    /// Játékos mozgatása térképen
    /// </summary>
    private void MovePlayer(int newRow, int newCol)
    {
        // Régi pozíció frissítése
        _map[_playerRow, _playerCol] = _map[_playerRow, _playerCol] == Tiles.PlayerOnGoal
            ? Tiles.Goal
            : Tiles.Floor;

        // Új pozíció beállítása
        _map[newRow, newCol] = _map[newRow, newCol] == Tiles.Goal
            ? Tiles.PlayerOnGoal
            : Tiles.Player;

        _playerRow = newRow;
        _playerCol = newCol;
    }

    /// <summary>
    /// Láda mozgatása térképen
    /// </summary>
    private void MoveBox(int fromRow, int fromCol, int toRow, int toCol)
    {
        // Eredeti pozíció frissítése
        _map[fromRow, fromCol] = _map[fromRow, fromCol] == Tiles.BoxOnGoal
            ? Tiles.Goal
            : Tiles.Floor;

        // Új pozíció beállítása
        _map[toRow, toCol] = _map[toRow, toCol] == Tiles.Goal
            ? Tiles.BoxOnGoal
            : Tiles.Box;
    }

    /// <summary>
    /// Állapot mentése undo-hoz
    /// </summary>
    private void SaveState()
    {
        _history.Push(new GameState(_map, _playerRow, _playerCol, Moves, Pushes));

        // Maximum 1000 lépés tárolása
        if (_history.Count > 1000)
        {
            var tempList = _history.ToList();
            tempList.RemoveAt(tempList.Count - 1);
            _history.Clear();
            foreach (var state in tempList.AsEnumerable().Reverse())
            {
                _history.Push(state);
            }
        }
    }

    /// <summary>
    /// Visszalépés (undo)
    /// </summary>
    public bool Undo()
    {
        if (_history.Count == 0)
            return false;

        var state = _history.Pop();
        Array.Copy(state.Map, _map, _map.Length);
        _playerRow = state.PlayerRow;
        _playerCol = state.PlayerCol;
        Moves = state.Moves;
        Pushes = state.Pushes;
        return true;
    }

    /// <summary>
    /// Ellenőrzi, hogy a játék megoldott-e
    /// </summary>
    public bool IsSolved()
    {
        for (int row = 0; row < Height; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                // Ha van láda, ami nincs célhelyen
                if (_map[row, col] == Tiles.Box)
                    return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Deadlock (zsákutca) ellenőrzés
    /// </summary>
    public bool CheckDeadlock(int boxRow, int boxCol)
    {
        // Ha a láda célhelyen van, nem deadlock
        if (IsGoal(boxRow, boxCol))
            return false;

        // Sarok deadlock: ha a láda sarokba szorult
        bool up = IsWall(boxRow - 1, boxCol);
        bool down = IsWall(boxRow + 1, boxCol);
        bool left = IsWall(boxRow, boxCol - 1);
        bool right = IsWall(boxRow, boxCol + 1);

        // Sarok pozíciók
        if ((up && left) || (up && right) || (down && left) || (down && right))
            return true;

        return false;
    }

    /// <summary>
    /// Ládák száma
    /// </summary>
    public int BoxCount
    {
        get
        {
            int count = 0;
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (_map[row, col] == Tiles.Box || _map[row, col] == Tiles.BoxOnGoal)
                        count++;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// Célhelyen lévő ládák száma
    /// </summary>
    public int BoxesOnGoalCount
    {
        get
        {
            int count = 0;
            for (int row = 0; row < Height; row++)
            {
                for (int col = 0; col < Width; col++)
                {
                    if (_map[row, col] == Tiles.BoxOnGoal)
                        count++;
                }
            }
            return count;
        }
    }

    /// <summary>
    /// Térkép klónozása
    /// </summary>
    public char[,] CloneMap()
    {
        var clone = new char[Height, Width];
        Array.Copy(_map, clone, _map.Length);
        return clone;
    }

    /// <summary>
    /// Állapot kulcs generálása (AI-hoz)
    /// </summary>
    public string GetStateKey()
    {
        var boxes = new List<string>();
        for (int row = 0; row < Height; row++)
        {
            for (int col = 0; col < Width; col++)
            {
                if (_map[row, col] == Tiles.Box || _map[row, col] == Tiles.BoxOnGoal)
                {
                    boxes.Add($"{row},{col}");
                }
            }
        }
        boxes.Sort();
        return $"{_playerRow},{_playerCol}|{string.Join("|", boxes)}";
    }
}

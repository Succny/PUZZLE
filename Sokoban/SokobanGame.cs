namespace Sokoban;

// ============================================================================
// CORE LAYER / JÁTÉKMOTOR RÉTEG
// Ez a fájl a Sokoban játékmotor alapvető osztályait tartalmazza.
// A szakdolgozatban hivatkozható: Core játéklogika réteg.
// ============================================================================

/// <summary>
/// [Core Layer]
/// Mozgatás eredménye - a játékos lépésének kimenetelét írja le.
/// </summary>
public class MoveResult
{
    /// <summary>Sikeres volt-e a lépés</summary>
    public bool Success { get; set; }
    /// <summary>Történt-e láda tolás</summary>
    public bool Pushed { get; set; }
    /// <summary>Megoldódott-e a pálya</summary>
    public bool Solved { get; set; }
    /// <summary>Deadlock állapot keletkezett-e</summary>
    public bool Deadlock { get; set; }
    /// <summary>A sikertelen lépés oka (pl. "wall", "blocked")</summary>
    public string? Reason { get; set; }
}

/// <summary>
/// [Core Layer]
/// Játékállapot mentése undo funkcióhoz.
/// Tárolja a térkép, játékos pozíció, és statisztikák pillanatképét.
/// </summary>
public class GameState
{
    /// <summary>A térkép másolata</summary>
    public char[,] Map { get; }
    /// <summary>Játékos sor pozíciója</summary>
    public int PlayerRow { get; }
    /// <summary>Játékos oszlop pozíciója</summary>
    public int PlayerCol { get; }
    /// <summary>Aktuális lépésszám</summary>
    public int Moves { get; }
    /// <summary>Aktuális tolásszám</summary>
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
/// [Core Layer]
/// Sokoban játék fő logikája.
/// Felelős a játékállapot kezeléséért, mozgásokért, undo funkcióért és deadlock detektálásért.
/// 
/// A szakdolgozatban hivatkozható:
/// - Játékállapot reprezentáció (2D char tömb)
/// - Lépés validáció és végrehajtás
/// - Undo stack kezelése (max ~1000 lépés)
/// - Deadlock detektálás (sarok és fal-vonal deadlock)
/// </summary>
public class SokobanGame
{
    /// <summary>
    /// Maximális undo történet mérete.
    /// A memória korlátozására szolgál, ~1000 visszalépés tárolása.
    /// </summary>
    private const int MaxHistorySize = 1000;

    private char[,] _map;
    private char[,] _originalMap;
    private int _playerRow;
    private int _playerCol;
    private readonly LinkedList<GameState> _history;
    
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int Moves { get; private set; }
    public int Pushes { get; private set; }
    public Level CurrentLevel { get; private set; }

    public SokobanGame(Level level)
    {
        CurrentLevel = level;
        _history = new LinkedList<GameState>();
        _map = new char[0, 0];
        _originalMap = new char[0, 0];
        Initialize(level);
    }

    /// <summary>
    /// Privát konstruktor klónozáshoz
    /// </summary>
    private SokobanGame(SokobanGame other)
    {
        CurrentLevel = other.CurrentLevel;
        _history = new LinkedList<GameState>();
        Width = other.Width;
        Height = other.Height;
        Moves = other.Moves;
        Pushes = other.Pushes;
        _playerRow = other._playerRow;
        _playerCol = other._playerCol;
        
        _map = new char[Height, Width];
        _originalMap = new char[Height, Width];
        Array.Copy(other._map, _map, other._map.Length);
        Array.Copy(other._originalMap, _originalMap, other._originalMap.Length);
    }

    /// <summary>
    /// Játék klónozása (AI solver-hez)
    /// </summary>
    public SokobanGame Clone()
    {
        return new SokobanGame(this);
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
    /// Pálya betöltése
    /// </summary>
    public void LoadLevel(Level level)
    {
        CurrentLevel = level;
        _history.Clear();
        Initialize(level);
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
        _history.AddLast(new GameState(_map, _playerRow, _playerCol, Moves, Pushes));
        TrimHistory();
    }

    /// <summary>
    /// Undo történet ritkítása a memória korlátozásához.
    /// Maximum ~1000 lépés tárolása a visszalépési lehetőségek biztosításához.
    /// 
    /// A szakdolgozatban hivatkozható: undo stack memória-menedzsment.
    /// </summary>
    private void TrimHistory()
    {
        if (_history.Count > MaxHistorySize)
        {
            _history.RemoveFirst();
        }
    }

    /// <summary>
    /// Visszalépés (undo).
    /// Visszaállítja az előző mentett állapotot, ha van elérhető történet.
    /// </summary>
    /// <returns>True, ha sikerült visszalépni; False, ha nincs elérhető történet</returns>
    public bool Undo()
    {
        if (_history.Count == 0)
            return false;

        var state = _history.Last!.Value;
        _history.RemoveLast();

        if (state.Map.Length != _map.Length)
        {
            // Biztonsági ellenőrzés: ha a térkép mérete nem egyezik, ne állítsuk vissza
            return false;
        }

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
    /// Deadlock (zsákutca) ellenőrzés.
    /// Kombinálja a sarok és fal-vonal deadlock detektálást.
    /// 
    /// FONTOS: A logika konzervatív - csak akkor jelent deadlockot, ha biztosan
    /// menthetetlen az állapot. Ha bizonytalan, inkább NEM tekinti deadlocknak.
    /// 
    /// A szakdolgozatban hivatkozható: deadlock típusok elemzése Sokoban játékban.
    /// </summary>
    /// <param name="boxRow">A láda sor pozíciója</param>
    /// <param name="boxCol">A láda oszlop pozíciója</param>
    /// <returns>True, ha a láda biztosan deadlock állapotban van</returns>
    public bool CheckDeadlock(int boxRow, int boxCol)
    {
        // Ha a láda célhelyen van, nem deadlock
        if (IsGoal(boxRow, boxCol))
            return false;

        // Sarok deadlock ellenőrzése - ez a legbiztosabb deadlock típus
        if (IsCornerDeadlock(boxRow, boxCol))
            return true;

        // Fal-vonal deadlock ellenőrzése - konzervatív megközelítéssel
        if (IsWallLineDeadlock(boxRow, boxCol))
            return true;

        return false;
    }

    /// <summary>
    /// Sarok deadlock ellenőrzése.
    /// Egy láda sarokba szorult, ha két szomszédos oldalon (pl. fel és bal) fal van,
    /// és a láda nincs célhelyen.
    /// 
    /// Ez a legbiztosabb deadlock típus - ha egy láda sarokba kerül és nincs célhelyen,
    /// akkor biztosan nem mozdítható ki.
    /// 
    /// A szakdolgozatban hivatkozható: klasszikus sarok-deadlock felismerés.
    /// </summary>
    /// <param name="boxRow">A láda sor pozíciója</param>
    /// <param name="boxCol">A láda oszlop pozíciója</param>
    /// <returns>True, ha a láda sarok-deadlock állapotban van</returns>
    public bool IsCornerDeadlock(int boxRow, int boxCol)
    {
        bool up = IsWall(boxRow - 1, boxCol);
        bool down = IsWall(boxRow + 1, boxCol);
        bool left = IsWall(boxRow, boxCol - 1);
        bool right = IsWall(boxRow, boxCol + 1);

        // Sarok pozíciók: két szomszédos oldalon fal
        return (up && left) || (up && right) || (down && left) || (down && right);
    }

    /// <summary>
    /// Fal-vonal deadlock ellenőrzése.
    /// Ha egy láda olyan falvonal mentén áll (vízszintes vagy függőleges),
    /// ahol a vonalon sehol nincs cél (Goal), akkor az deadlock.
    /// 
    /// FONTOS: A logika konzervatív - csak akkor jelent deadlockot, ha biztosan
    /// nem mozdítható a láda a falvonal mentén, és nincs cél a vonalon.
    /// 
    /// A szakdolgozatban hivatkozható: fal-menti deadlock felismerés,
    /// amely a sarok-deadlock kiterjesztése az egész falvonalra.
    /// </summary>
    /// <param name="boxRow">A láda sor pozíciója</param>
    /// <param name="boxCol">A láda oszlop pozíciója</param>
    /// <returns>True, ha a láda biztosan fal-vonal deadlock állapotban van</returns>
    public bool IsWallLineDeadlock(int boxRow, int boxCol)
    {
        // Ellenőrizzük vízszintes fal-vonalat (fel vagy le irányban van fal)
        bool upWall = IsWall(boxRow - 1, boxCol);
        bool downWall = IsWall(boxRow + 1, boxCol);

        // Csak akkor vizsgálunk vízszintes fal-vonalat, ha pontosan EGY oldalon van fal
        // (XOR: ha mindkét oldalon fal van, az sarok-deadlock, amit az IsCornerDeadlock kezel)
        if (upWall ^ downWall)
        {
            // Vizsgáljuk a vízszintes vonalat
            if (IsWallLineDeadlockHorizontal(boxRow, boxCol, upWall ? -1 : 1))
                return true;
        }

        // Ellenőrizzük függőleges fal-vonalat (bal vagy jobb irányban van fal)
        bool leftWall = IsWall(boxRow, boxCol - 1);
        bool rightWall = IsWall(boxRow, boxCol + 1);

        // Csak akkor vizsgálunk függőleges fal-vonalat, ha pontosan EGY oldalon van fal
        // (XOR: ha mindkét oldalon fal van, az sarok-deadlock, amit az IsCornerDeadlock kezel)
        if (leftWall ^ rightWall)
        {
            // Vizsgáljuk a függőleges vonalat
            if (IsWallLineDeadlockVertical(boxRow, boxCol, leftWall ? -1 : 1))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Vízszintes fal-vonal deadlock segédfüggvény.
    /// Ellenőrzi, hogy a láda vízszintes vonalán van-e cél.
    /// 
    /// FONTOS: Konzervatív megközelítés - csak akkor deadlock, ha:
    /// 1. A teljes vízszintes szegmens mindkét végén fal van
    /// 2. A fal folyamatosan fut a szegmens alatt/felett
    /// 3. A szegmensen nincs célhely
    /// </summary>
    private bool IsWallLineDeadlockHorizontal(int boxRow, int boxCol, int wallDirection)
    {
        // Balra keresés - a fal folyamatosan fut a szegmens mellett
        int leftEnd = boxCol;
        while (leftEnd > 0 && !IsWall(boxRow, leftEnd - 1) && IsWall(boxRow + wallDirection, leftEnd - 1))
        {
            leftEnd--;
        }
        // Csak akkor blokkolva a bal oldal, ha fal van az úton
        bool leftBlocked = IsWall(boxRow, leftEnd - 1);

        // Jobbra keresés
        int rightEnd = boxCol;
        while (rightEnd < Width - 1 && !IsWall(boxRow, rightEnd + 1) && IsWall(boxRow + wallDirection, rightEnd + 1))
        {
            rightEnd++;
        }
        bool rightBlocked = IsWall(boxRow, rightEnd + 1);

        // Ha mindkét végén blokkolva van, ellenőrizzük, van-e cél a vonalon
        if (leftBlocked && rightBlocked)
        {
            // Ellenőrizzük, hogy a fal folyamatos-e a teljes szegmens mellett
            for (int col = leftEnd; col <= rightEnd; col++)
            {
                if (!IsWall(boxRow + wallDirection, col))
                {
                    // A fal megszakad - nem biztos deadlock
                    return false;
                }
                if (IsGoal(boxRow, col))
                {
                    return false; // Van cél a vonalon, nem deadlock
                }
            }
            return true; // Nincs cél a vonalon és a fal folyamatos, deadlock
        }

        return false;
    }

    /// <summary>
    /// Függőleges fal-vonal deadlock segédfüggvény.
    /// Ellenőrzi, hogy a láda függőleges vonalán van-e cél.
    /// 
    /// FONTOS: Konzervatív megközelítés - csak akkor deadlock, ha:
    /// 1. A teljes függőleges szegmens mindkét végén fal van
    /// 2. A fal folyamatosan fut a szegmens mellett
    /// 3. A szegmensen nincs célhely
    /// </summary>
    private bool IsWallLineDeadlockVertical(int boxRow, int boxCol, int wallDirection)
    {
        // Felfelé keresés
        int topEnd = boxRow;
        while (topEnd > 0 && !IsWall(topEnd - 1, boxCol) && IsWall(topEnd - 1, boxCol + wallDirection))
        {
            topEnd--;
        }
        bool topBlocked = IsWall(topEnd - 1, boxCol);

        // Lefelé keresés
        int bottomEnd = boxRow;
        while (bottomEnd < Height - 1 && !IsWall(bottomEnd + 1, boxCol) && IsWall(bottomEnd + 1, boxCol + wallDirection))
        {
            bottomEnd++;
        }
        bool bottomBlocked = IsWall(bottomEnd + 1, boxCol);

        // Ha mindkét végén blokkolva van, ellenőrizzük, van-e cél a vonalon
        if (topBlocked && bottomBlocked)
        {
            // Ellenőrizzük, hogy a fal folyamatos-e a teljes szegmens mellett
            for (int row = topEnd; row <= bottomEnd; row++)
            {
                if (!IsWall(row, boxCol + wallDirection))
                {
                    // A fal megszakad - nem biztos deadlock
                    return false;
                }
                if (IsGoal(row, boxCol))
                {
                    return false; // Van cél a vonalon, nem deadlock
                }
            }
            return true; // Nincs cél a vonalon és a fal folyamatos, deadlock
        }

        return false;
    }

    /// <summary>
    /// Hibamegelőzés: előrejelzi, hogy egy adott irányú lépés deadlockot okozna-e.
    /// Ellenőrzi a push céljának sarok-deadlock státuszát a lépés végrehajtása nélkül.
    /// 
    /// FONTOS: Csak sarok-deadlockot jelez előre. Fal-vonal deadlock előrejelzéséhez
    /// a szomszédos pozíciók is megváltoznának, ezért azt nem vizsgáljuk előre.
    /// 
    /// A szakdolgozatban hivatkozható: proaktív hibamegelőzés (hibamegelőzés),
    /// amely a játékos döntése ELŐTT figyelmezteti a rossz lépésre.
    /// </summary>
    /// <param name="dRow">Sor irányú elmozdulás (-1: fel, 1: le)</param>
    /// <param name="dCol">Oszlop irányú elmozdulás (-1: bal, 1: jobb)</param>
    /// <returns>True, ha a lépés végrehajtása sarok-deadlockot okozna</returns>
    public bool PredictDeadlock(int dRow, int dCol)
    {
        int newRow = _playerRow + dRow;
        int newCol = _playerCol + dCol;

        // Csak akkor releváns, ha a célmezőn láda van
        if (!IsBox(newRow, newCol))
            return false;

        int boxNewRow = newRow + dRow;
        int boxNewCol = newCol + dCol;

        // Ha a tolás nem érvényes (fal vagy láda blokkolja), nem deadlock
        if (IsWall(boxNewRow, boxNewCol) || IsBox(boxNewRow, boxNewCol))
            return false;

        // Ha célhelyre kerülne, nem deadlock
        if (IsGoal(boxNewRow, boxNewCol))
            return false;

        // Sarok-deadlock előrejelzés a célpozícióban
        return IsCornerDeadlock(boxNewRow, boxNewCol);
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

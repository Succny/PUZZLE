namespace Sokoban;

// ============================================================================
// UI LAYER / PREZENTÁCIÓS RÉTEG
// Ez a fájl a konzolos felhasználói felületet tartalmazza.
// A szakdolgozatban hivatkozható: UI/Prezentáció réteg.
// ============================================================================

/// <summary>
/// [UI Layer]
/// Konzol alapú felhasználói felület.
/// 
/// Felelősségek:
/// - Játéktér vizuális megjelenítése
/// - Felhasználói bemenet kezelése
/// - AI üzenetek megjelenítése
/// - Játék statisztikák mutatása
/// 
/// A szakdolgozatban hivatkozható:
/// - Konzol UI implementáció
/// - Színes karakter-alapú megjelenítés
/// - Input kezelés és játékvezérlés
/// </summary>
public class ConsoleUI
{
    /// <summary>
    /// A teljes UI keretrendszer szélessége (fix érték a konzisztens megjelenítéshez)
    /// </summary>
    internal const int UiWidth = 65;

    /// <summary>
    /// A játéktér belső szélessége (keret és padding nélkül)
    /// </summary>
    private const int GameAreaWidth = 61;

    /// <summary>
    /// Minimális játéktér magasság (ha a pálya kisebb, üres sorokkal töltjük ki)
    /// </summary>
    private const int MinGameAreaHeight = 10;

    private readonly SokobanGame _game;
    private readonly HintSystem _hintSystem;
    private readonly AISolver _solver;
    private int _currentLevelIndex;
    private string _lastMessage;
    private DateTime _startTime;
    private bool _running;
    
    /// <summary>
    /// Az AI által legutóbb javasolt lépés iránya (F gombhoz).
    /// Null, ha nincs érvényes javasolt lépés.
    /// </summary>
    private MoveDirection? _lastAISuggestedMove;
    
    /// <summary>
    /// AI által végrehajtott (Follow AI) lépések száma - kooperáció metrika.
    /// </summary>
    private int _aiAssistedMoves;

    // Színek a konzolhoz
    private static readonly ConsoleColor WallColor = ConsoleColor.DarkGray;
    private static readonly ConsoleColor BoxColor = ConsoleColor.Yellow;
    private static readonly ConsoleColor BoxOnGoalColor = ConsoleColor.Green;
    private static readonly ConsoleColor GoalColor = ConsoleColor.Red;
    private static readonly ConsoleColor PlayerColor = ConsoleColor.Cyan;
    private static readonly ConsoleColor HintColor = ConsoleColor.Magenta;

    public ConsoleUI()
    {
        _currentLevelIndex = 0;
        _solver = new AISolver();
        _game = new SokobanGame(Levels.AllLevels[_currentLevelIndex]);
        _hintSystem = new HintSystem(_solver);
        _lastMessage = _hintSystem.GetWelcomeMessage(Levels.AllLevels[_currentLevelIndex], _currentLevelIndex + 1);
        _startTime = DateTime.Now;
        _running = true;
    }

    /// <summary>
    /// Játék futtatása - fő ciklus.
    /// Renderelés csak az indulásnál és minden billentyűleütés után történik,
    /// így nem árasztja el a terminált felesleges kiírásokkal.
    /// </summary>
    public void Run()
    {
        Console.CursorVisible = false;
        Console.Clear();

        Render(); // Kezdeti megjelenítés

        while (_running)
        {
            HandleInput(); // Blokkolva vár billentyűleütésre
            if (_running)
                Render(); // Csak akkor renderel, ha még fut a játék
        }

        Console.CursorVisible = true;
    }

    #region Rendering Methods

    /// <summary>
    /// Képernyő renderelése - összeállítja a teljes UI-t.
    /// A metódus kisebb, jól elkülöníthető részekre van bontva a könnyebb olvashatóság érdekében.
    /// </summary>
    private void Render()
    {
        // Kurzor pozíció visszaállítása a stabil rendereléshez
        // Ez megakadályozza a "ugrálást" PowerShell/Windows Terminal alatt
        ConsoleSizing.ResetCursorForRender();

        RenderHeader();
        RenderLevelSelector();
        RenderGameArea();
        RenderStats();
        RenderAIPanel();
        RenderMessagePanel();
        RenderControls();
    }

    /// <summary>
    /// Fejléc renderelése - cím és logó.
    /// </summary>
    private void RenderHeader()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔═══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║               📦 SOKOBAN - AI Hint Rendszerrel                ║");
        Console.WriteLine("║         Kooperatív Puzzle Játék (C# Verzió)                   ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
        Console.ResetColor();
    }

    /// <summary>
    /// Pálya választó sáv renderelése.
    /// </summary>
    private void RenderLevelSelector()
    {
        Console.Write("║  Pályák: ");
        for (int i = 0; i < Levels.AllLevels.Length; i++)
        {
            if (i == _currentLevelIndex)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"[{i + 1}] ");
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.Write($" {i + 1}  ");
            }
        }
        Console.ResetColor();
        Console.WriteLine("                              ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
        Console.ResetColor();
    }

    /// <summary>
    /// Játéktér renderelése - térkép és csempék.
    /// </summary>
    private void RenderGameArea()
    {
        RenderGame();
    }

    /// <summary>
    /// Statisztikák panel renderelése.
    /// </summary>
    private void RenderStats()
    {
        TimeSpan elapsed = DateTime.Now - _startTime;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
        Console.ResetColor();
        Console.Write("║  ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"Lépések: {_game.Moves,3}");
        Console.ResetColor();
        Console.Write("  │  ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"Tolások: {_game.Pushes,3}");
        Console.ResetColor();
        Console.Write("  │  ");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"Idő: {elapsed:mm\\:ss}");
        Console.ResetColor();
        Console.Write("  │  ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Ládák: {_game.BoxesOnGoalCount}/{_game.BoxCount}");
        Console.ResetColor();
        Console.Write("  │  ");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write($"AI: {_aiAssistedMoves,2}");
        Console.ResetColor();
        Console.WriteLine("  ║");
    }

    /// <summary>
    /// AI asszisztens fejléc renderelése.
    /// Mutatja az aktuálisan javasolt lépést, ha van.
    /// </summary>
    private void RenderAIPanel()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
        if (_lastAISuggestedMove != null)
        {
            Console.Write("║  🤖 AI Asszisztens  [F: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{_lastAISuggestedMove!.Arrow} Kövesd az AI-t");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("]                       ║");
        }
        else
        {
            Console.WriteLine("║  🤖 AI Asszisztens  [H: Kérj segítséget]                     ║");
        }
        Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
        Console.ResetColor();
    }

    /// <summary>
    /// AI üzenetek panel renderelése.
    /// </summary>
    private void RenderMessagePanel()
    {
        var messageLines = _lastMessage.Split('\n');
        for (int i = 0; i < 4; i++)
        {
            Console.Write("║  ");
            Console.ForegroundColor = HintColor;
            string line = i < messageLines.Length ? messageLines[i] : "";
            // Csonkítás, hogy ne törje meg a keretet
            if (line.Length > GameAreaWidth)
                line = line[..GameAreaWidth];
            Console.Write(line.PadRight(GameAreaWidth));
            Console.ResetColor();
            Console.WriteLine("║");
        }
    }

    /// <summary>
    /// Irányítás panel renderelése.
    /// </summary>
    private void RenderControls()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  🎮 Irányítás                                                 ║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
        Console.ResetColor();
        Console.WriteLine("║  ↑↓←→/WASD: Mozgás  │  H: Segítség  │  N: Elemzés           ║");
        Console.WriteLine("║  F: Kövesd AI-t     │  1-5: Pálya választás  │  Q: Kilépés   ║");
        Console.WriteLine("║  U/Backspace: Visszalépés  │  R: Újraindítás                 ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    /// <summary>
    /// Játéktér (térkép) renderelése.
    /// </summary>
    private void RenderGame()
    {
        // A játéktér szélessége karakterekben (minden csempe 2 karakter széles)
        int gameWidth = _game.Width * 2;
        
        // Középre igazítás - biztosítjuk, hogy a padding nem negatív
        int padding = Math.Max(0, (GameAreaWidth - gameWidth) / 2);
        int rightPadding = Math.Max(0, GameAreaWidth - padding - gameWidth);

        for (int row = 0; row < _game.Height; row++)
        {
            Console.Write("║  ");
            Console.Write(new string(' ', padding));

            for (int col = 0; col < _game.Width; col++)
            {
                char tile = _game.GetTile(row, col);
                RenderTile(tile);
            }

            Console.Write(new string(' ', rightPadding));
            Console.WriteLine("║");
        }

        // Üres sorok kitöltése a minimális magasságig
        for (int i = _game.Height; i < MinGameAreaHeight; i++)
        {
            Console.Write("║  ");
            Console.Write(new string(' ', GameAreaWidth));
            Console.WriteLine("║");
        }
    }

    /// <summary>
    /// Egy csempe (tile) renderelése színekkel.
    /// </summary>
    private void RenderTile(char tile)
    {
        switch (tile)
        {
            case Tiles.Wall:
                Console.ForegroundColor = WallColor;
                Console.BackgroundColor = WallColor;
                Console.Write("██");
                break;

            case Tiles.Floor:
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("  ");
                break;

            case Tiles.Goal:
                Console.ForegroundColor = GoalColor;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("··");
                break;

            case Tiles.Box:
                Console.ForegroundColor = BoxColor;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("[]");
                break;

            case Tiles.BoxOnGoal:
                Console.ForegroundColor = BoxOnGoalColor;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("▣▣");
                break;

            case Tiles.Player:
                Console.ForegroundColor = PlayerColor;
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("@");
                Console.Write(" ");
                break;

            case Tiles.PlayerOnGoal:
                Console.ForegroundColor = PlayerColor;
                Console.BackgroundColor = ConsoleColor.DarkRed;
                Console.Write("@ ");
                break;

            default:
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.Write("  ");
                break;
        }

        Console.ResetColor();
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// Bemenet kezelése - billentyűleütések feldolgozása.
    /// Blokkolva vár, amíg a játékos le nem nyom egy billentyűt, így
    /// nem okoz felesleges terminálkiírásokat.
    /// A bemenetek logikailag csoportosítva:
    /// - Mozgás (nyilak, WASD)
    /// - Undo (U, Backspace)
    /// - Hint (H, N)
    /// - Játékvezérlés (R, 1-5, Q, Esc)
    /// </summary>
    private void HandleInput()
    {
        var key = Console.ReadKey(true);

        // Mozgás kezelése
        if (HandleMovementInput(key))
            return;

        // Undo kezelése
        if (HandleUndoInput(key))
            return;

        // Hint kezelése
        if (HandleHintInput(key))
            return;

        // Játékvezérlés kezelése
        HandleGameControlInput(key);
    }

    /// <summary>
    /// Mozgás billentyűk kezelése.
    /// </summary>
    /// <returns>True, ha mozgás történt</returns>
    private bool HandleMovementInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.UpArrow:
            case ConsoleKey.W:
                MakeMove(-1, 0);
                return true;

            case ConsoleKey.DownArrow:
            case ConsoleKey.S:
                MakeMove(1, 0);
                return true;

            case ConsoleKey.LeftArrow:
            case ConsoleKey.A:
                MakeMove(0, -1);
                return true;

            case ConsoleKey.RightArrow:
            case ConsoleKey.D:
                MakeMove(0, 1);
                return true;
        }
        return false;
    }

    /// <summary>
    /// Undo billentyűk kezelése.
    /// </summary>
    /// <returns>True, ha undo történt</returns>
    private bool HandleUndoInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.U:
            case ConsoleKey.Backspace:
                if (_game.Undo())
                {
                    _lastMessage = "↩️ Visszalépés sikeres!";
                }
                else
                {
                    _lastMessage = "Nincs több visszalépési lehetőség.";
                }
                return true;
        }
        return false;
    }

    /// <summary>
    /// Hint billentyűk kezelése.
    /// H = Help (segítség) - következő lépés javaslata
    /// N = iNfo/aNalízis - állapot elemzés
    /// F = Follow AI - AI javasolt lépés végrehajtása
    /// </summary>
    /// <returns>True, ha hint kérés történt</returns>
    private bool HandleHintInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.H:
                // H = Help - következő lépés javaslata (segítség)
                _lastMessage = _hintSystem.GenerateHint(_game, _currentLevelIndex);
                // Az AI javasolt lépés mentése az F gombhoz
                UpdateLastAISuggestedMove();
                return true;

            case ConsoleKey.N:
                // N = iNfo/aNalízis - állapot elemzés
                _lastMessage = _hintSystem.GenerateDetailedHint(_game, _currentLevelIndex);
                return true;

            case ConsoleKey.F:
                // F = Follow AI - az AI által javasolt lépés végrehajtása
                return HandleFollowAI();
        }
        return false;
    }

    /// <summary>
    /// Az AI javasolt lépésének frissítése az AISolver segítségével.
    /// Menti a következő optimális lépés irányát az F gombhoz.
    /// </summary>
    private void UpdateLastAISuggestedMove()
    {
        if (_game.IsSolved())
        {
            _lastAISuggestedMove = null;
            return;
        }
        var nextMove = _solver.GetNextMove(_game);
        _lastAISuggestedMove = nextMove?.Move?.Direction;
    }

    /// <summary>
    /// AI által javasolt lépés végrehajtása (F gomb - Follow AI).
    /// Ha van érvényes AI javaslat, végrehajtja azt és frissíti az AI-asszisztált lépések számát.
    /// Ez az ember-AI kooperáció közvetlen megvalósítása: a játékos delegálja
    /// a lépést az AI-nak, de ő dönti el, mikor teszi ezt.
    /// </summary>
    /// <returns>True, ha a billentyű le lett kezelve</returns>
    private bool HandleFollowAI()
    {
        if (_lastAISuggestedMove == null)
        {
            // Nincs előző hint - megpróbáljuk most kiszámolni
            UpdateLastAISuggestedMove();
        }

        if (_lastAISuggestedMove != null)
        {
            var dir = _lastAISuggestedMove!;
            MakeMove(dir.DRow, dir.DCol);
            _aiAssistedMoves++;
            // Sikeres AI lépés után frissítjük a javaslatot
            UpdateLastAISuggestedMove();
            if (!_lastMessage.StartsWith("🎉") && !_lastMessage.StartsWith("🏆"))
            {
                _lastMessage = $"🤖 AI lépett: {dir.Name}{dir.DirectionalSuffix} ({dir.Arrow})\n" +
                               $"(AI által végrehajtott lépések: {_aiAssistedMoves})";
            }
        }
        else
        {
            _lastMessage = "🤖 Az AI jelenleg nem talál javaslatot.\nNyomj H-t a segítségért!";
        }
        return true;
    }

    /// <summary>
    /// Játékvezérlés billentyűk kezelése (restart, pályaváltás, kilépés).
    /// </summary>
    private void HandleGameControlInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            // Újraindítás
            case ConsoleKey.R:
                _game.Restart();
                _hintSystem.Reset();
                _aiAssistedMoves = 0;
                _lastAISuggestedMove = null;
                _startTime = DateTime.Now;
                _lastMessage = "🔄 Pálya újraindítva!";
                break;

            // Pálya választás
            case ConsoleKey.D1:
            case ConsoleKey.NumPad1:
                LoadLevel(0);
                break;

            case ConsoleKey.D2:
            case ConsoleKey.NumPad2:
                LoadLevel(1);
                break;

            case ConsoleKey.D3:
            case ConsoleKey.NumPad3:
                LoadLevel(2);
                break;

            case ConsoleKey.D4:
            case ConsoleKey.NumPad4:
                LoadLevel(3);
                break;

            case ConsoleKey.D5:
            case ConsoleKey.NumPad5:
                LoadLevel(4);
                break;

            // Kilépés
            case ConsoleKey.Q:
            case ConsoleKey.Escape:
                _running = false;
                Console.Clear();
                Console.WriteLine("Köszönjük, hogy játszottál! 👋");
                break;
        }
    }

    #endregion

    #region Game Actions

    /// <summary>
    /// Lépés végrehajtása.
    /// </summary>
    private void MakeMove(int dRow, int dCol)
    {
        var result = _game.Move(dRow, dCol);

        if (result.Success)
        {
            var response = _hintSystem.GetMoveResponse(result, _game);
            if (response != null)
            {
                _lastMessage = response;
            }

            if (result.Solved)
            {
                TimeSpan elapsed = DateTime.Now - _startTime;
                // Következő pálya számának meghatározása
                int nextLevelNum = _currentLevelIndex + 2; // +2 mert 1-indexelt a megjelenítés
                string nextLevelHint = _currentLevelIndex < Levels.AllLevels.Length - 1
                    ? $"Nyomj '{nextLevelNum}'-t a következő pályához!"
                    : "🏆 Ez volt az utolsó pálya!";
                
                _lastMessage = $"🎉 GRATULÁLOK! Pálya teljesítve!\n" +
                               $"Lépések: {_game.Moves}  │  Tolások: {_game.Pushes}  │  Idő: {elapsed:mm\\:ss}\n" +
                               $"AI-asszisztált lépések: {_aiAssistedMoves}  │  {nextLevelHint}";
            }
        }
        // Minden saját lépés után töröljük az AI javaslatot (elavult lett)
        _lastAISuggestedMove = null;
    }

    /// <summary>
    /// Pálya betöltése index alapján.
    /// </summary>
    private void LoadLevel(int index)
    {
        if (index >= 0 && index < Levels.AllLevels.Length)
        {
            _currentLevelIndex = index;
            var level = Levels.AllLevels[index];
            
            _game.LoadLevel(level);
            _hintSystem.Reset();
            _aiAssistedMoves = 0;
            _lastAISuggestedMove = null;
            _startTime = DateTime.Now;
            _lastMessage = _hintSystem.GetWelcomeMessage(level, index + 1);
        }
    }

    #endregion
}

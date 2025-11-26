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
    private readonly SokobanGame _game;
    private readonly HintSystem _hintSystem;
    private int _currentLevelIndex;
    private string _lastMessage;
    private DateTime _startTime;
    private bool _running;

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
        var solver = new AISolver();
        _game = new SokobanGame(Levels.AllLevels[_currentLevelIndex]);
        _hintSystem = new HintSystem(solver);
        _lastMessage = _hintSystem.GetWelcomeMessage(Levels.AllLevels[_currentLevelIndex], _currentLevelIndex + 1);
        _startTime = DateTime.Now;
        _running = true;
    }

    /// <summary>
    /// Játék futtatása - fő ciklus.
    /// </summary>
    public void Run()
    {
        Console.CursorVisible = false;
        Console.Clear();

        while (_running)
        {
            Render();
            HandleInput();
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
        Console.SetCursorPosition(0, 0);

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
        Console.WriteLine("    ║");
    }

    /// <summary>
    /// AI asszisztens fejléc renderelése.
    /// </summary>
    private void RenderAIPanel()
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╠═══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║  🤖 AI Asszisztens                                            ║");
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
            Console.Write(line.PadRight(61));
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
        Console.WriteLine("║  ↑↓←→/WASD: Mozgás  │  H: Hint  │  U: Visszalépés            ║");
        Console.WriteLine("║  R: Újraindítás     │  1-5: Pálya választás  │  Q: Kilépés   ║");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");
        Console.ResetColor();
    }

    /// <summary>
    /// Játéktér (térkép) renderelése.
    /// </summary>
    private void RenderGame()
    {
        // Középre igazítás
        int padding = (63 - _game.Width * 2) / 2;

        for (int row = 0; row < _game.Height; row++)
        {
            Console.Write("║  ");
            Console.Write(new string(' ', padding));

            for (int col = 0; col < _game.Width; col++)
            {
                char tile = _game.GetTile(row, col);
                RenderTile(tile);
            }

            Console.Write(new string(' ', 63 - padding - _game.Width * 2));
            Console.WriteLine("║");
        }

        // Üres sorok kitöltése
        for (int i = _game.Height; i < 10; i++)
        {
            Console.WriteLine("║                                                               ║");
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
    /// A bemenetek logikailag csoportosítva:
    /// - Mozgás (nyilak, WASD)
    /// - Undo (U, Backspace)
    /// - Hint (H, N)
    /// - Játékvezérlés (R, 1-5, Q, Esc)
    /// </summary>
    private void HandleInput()
    {
        if (!Console.KeyAvailable)
        {
            Thread.Sleep(50);
            return;
        }

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
    /// </summary>
    /// <returns>True, ha hint kérés történt</returns>
    private bool HandleHintInput(ConsoleKeyInfo key)
    {
        switch (key.Key)
        {
            case ConsoleKey.H:
                _lastMessage = _hintSystem.GenerateDetailedHint(_game);
                return true;

            case ConsoleKey.N:
                _lastMessage = _hintSystem.GenerateHint(_game);
                return true;
        }
        return false;
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
                _lastMessage = $"🎉 GRATULÁLOK! Pálya teljesítve!\n" +
                               $"Lépések: {_game.Moves}  │  Tolások: {_game.Pushes}  │  Idő: {elapsed:mm\\:ss}\n" +
                               $"Nyomj N-t a következő pályához!";
            }
        }
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
            _startTime = DateTime.Now;
            _lastMessage = _hintSystem.GetWelcomeMessage(level, index + 1);
        }
    }

    #endregion
}

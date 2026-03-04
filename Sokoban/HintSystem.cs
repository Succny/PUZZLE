namespace Sokoban;

// ============================================================================
// AI LAYER / MESTERSÉGES INTELLIGENCIA RÉTEG
// Ez a fájl a hint (segítség) rendszert tartalmazza.
// A szakdolgozatban hivatkozható: AI Hint réteg.
// ============================================================================

/// <summary>
/// [AI Layer]
/// AI Hint Rendszer - segíti a játékost hint-ekkel és stratégiai tanácsokkal.
/// 
/// A rendszer az AISolver-t használja a megoldás kereséséhez,
/// és barátságos, magyar nyelvű visszajelzéseket ad a játékosnak.
/// 
/// A szakdolgozatban hivatkozható:
/// - Ember-AI együttműködés implementálása
/// - Játékos segítése intelligens tanácsokkal
/// - Hint statisztikák gyűjtése (hányszor kért segítséget a játékos)
/// - UX elemek: bátorítás, deadlock figyelmeztetés, stratégiai tippek
/// </summary>
public class HintSystem
{
    private readonly AISolver _solver;
    private int _hintCount;
    private int _movesSinceLastPush;

    /// <summary>
    /// Ennyi lépés ládatolás nélkül után jelezzük a játékosnak, hogy esetleg elakadt.
    /// </summary>
    private const int StuckMoveThreshold = 7;

    private static readonly Random _random = new();

    public HintSystem(AISolver solver)
    {
        _solver = solver;
        _hintCount = 0;
        _movesSinceLastPush = 0;
    }

    /// <summary>
    /// Véletlenszerű üzenet egy tömbből.
    /// </summary>
    private static string GetRandomMessage(string[] messages)
    {
        return messages[_random.Next(messages.Length)];
    }

    /// <summary>
    /// Hint generálása - megadja a következő ajánlott lépést (H gomb - Segítség).
    /// Növeli a hint számlálót a statisztikákhoz.
    /// 
    /// FONTOS: Ha a pálya már megoldott, jelzi a játékosnak és javasolja a következő pályát.
    /// Ha a solver nem talál megoldást, megkülönbözteti a timeout-ot
    /// a valódi deadlock-tól, és ennek megfelelő üzenetet ad.
    /// </summary>
    public string GenerateHint(SokobanGame game, int currentLevelIndex = -1)
    {
        _hintCount++;

        // Ellenőrizzük, hogy a pálya már megoldott-e
        if (game.IsSolved())
        {
            // Van-e következő pálya?
            if (currentLevelIndex >= 0 && currentLevelIndex < Levels.AllLevels.Length - 1)
            {
                int nextLevel = currentLevelIndex + 2; // +2 mert 1-indexelt a megjelenítés
                return $"🎉 Gratulálok! A pálya már teljesítve van!\n" +
                       $"Nyomd meg a '{nextLevel}' gombot a következő pályához,\n" +
                       $"vagy válassz egy másik pályát (1-{Levels.AllLevels.Length})!";
            }
            else if (currentLevelIndex == Levels.AllLevels.Length - 1)
            {
                return $"🏆 Fantasztikus! Az utolsó pályát is teljesítetted!\n" +
                       $"Válassz egy másik pályát (1-{Levels.AllLevels.Length}),\n" +
                       $"vagy nyomd meg 'R'-t az újrakezdéshez!";
            }
            else
            {
                return $"🎉 Gratulálok! A pálya már teljesítve van!\n" +
                       $"Válassz egy új pályát (1-{Levels.AllLevels.Length}) vagy nyomd meg 'R'-t!";
            }
        }

        var (nextMove, solution) = _solver.GetNextMoveWithDetails(game);

        if (nextMove == null)
        {
            // Különböztetjük meg a timeout-ot a kimerített állapottértől
            if (solution.IsTimeout)
            {
                return "🤔 Jelenleg nem találok megoldást (időkorlát).\n" +
                       "Próbálj visszalépni néhányat (U), vagy más stratégiát!\n" +
                       $"💡 Tipp: {GetRandomMessage(Messages.StrategyTips)}";
            }
            else if (solution.IsExhausted)
            {
                // Az állapottér kimerült - valószínűleg nincs megoldás
                return "⚠️ Úgy tűnik, ebből az állapotból nincs megoldás.\n" +
                       "Használd az 'U' billentyűt a visszalépéshez!\n" +
                       $"💡 Tipp: {GetRandomMessage(Messages.StrategyTips)}";
            }
            else
            {
                // Általános eset
                return "🤔 Nem találok megoldást innen.\n" +
                       "Próbálj visszalépni (U), vagy más utat keresni!";
            }
        }

        var (move, totalMoves, pushCount) = nextMove.Value;
        string action = move!.Pushed
            ? $"Told a ládát {move.Direction.Name}{move.Direction.DirectionalSuffix} ({move.Direction.Arrow})!"
            : $"Menj {move.Direction.Name}{move.Direction.DirectionalSuffix} ({move.Direction.Arrow})!";

        return $"{action}\n(Még {totalMoves} lépés, {pushCount} tolás a megoldásig)";
    }

    /// <summary>
    /// Részletes állapot elemzés (N gomb - iNfo/aNalízis).
    /// Átfogó információ a játék állapotáról.
    /// Nem növeli a hint számlálót, mert ez inkább elemzés, mint segítség.
    /// 
    /// FONTOS: Ha a pálya már megoldott, jelzi a játékosnak és javasolja a következő pályát.
    /// Különbözteti meg a timeout-ot a kimerített állapottértől,
    /// és ennek megfelelő üzenetet ad.
    /// </summary>
    public string GenerateDetailedHint(SokobanGame game, int currentLevelIndex = -1)
    {
        int boxesOnGoal = game.BoxesOnGoalCount;
        int totalBoxes = game.BoxCount;
        int progress = totalBoxes > 0 ? (boxesOnGoal * 100) / totalBoxes : 0;

        var lines = new List<string>
        {
            "📊 Állapot elemzés:",
            $"• {boxesOnGoal}/{totalBoxes} láda a célhelyen ({progress}%)",
            $"• Eddigi lépések: {game.Moves}",
            $"• Eddigi tolások: {game.Pushes}",
            ""
        };

        // Ellenőrizzük, hogy a pálya már megoldott-e
        if (game.IsSolved())
        {
            lines.Add("🎉 A pálya teljesítve van!");
            if (currentLevelIndex >= 0 && currentLevelIndex < Levels.AllLevels.Length - 1)
            {
                int nextLevel = currentLevelIndex + 2;
                lines.Add($"Nyomd meg a '{nextLevel}' gombot a következő pályához!");
            }
            else if (currentLevelIndex == Levels.AllLevels.Length - 1)
            {
                lines.Add("🏆 Ez volt az utolsó pálya! Gratulálok!");
            }
            return string.Join("\n", lines);
        }

        var solution = _solver.Solve(game);
        if (solution.Success)
        {
            lines.Add("✅ A pálya megoldható!");
            lines.Add($"Hátralévő lépések: ~{solution.Moves.Count}");
        }
        else
        {
            // Különböztetjük meg a timeout-ot a kimerített állapottértől
            if (solution.IsTimeout)
            {
                lines.Add("⏱️ A keresés időkorlátba ütközött.");
                lines.Add("Ez nem jelenti, hogy nincs megoldás!");
                lines.Add("Próbálj visszalépni (U), vagy folytasd a játékot.");
            }
            else if (solution.IsExhausted)
            {
                lines.Add("⚠️ Valószínűleg nincs megoldás ebből az állapotból.");
                lines.Add("Használd az 'U' billentyűt a visszalépéshez!");
            }
            else
            {
                lines.Add("🤔 Nem találok megoldást innen.");
                lines.Add("Próbálj visszalépni (U), vagy más utat keresni!");
            }
        }

        lines.Add("");
        lines.Add($"💡 Tipp: {GetRandomMessage(Messages.StrategyTips)}");

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Lépés utáni visszajelzés generálása.
    /// Bátorító üzenetek, deadlock figyelmeztetések, győzelmi gratulációk.
    /// Ha a játékos több lépést tesz ládatolás nélkül, „elakadt" üzenetet kap.
    /// </summary>
    public string? GetMoveResponse(MoveResult result, SokobanGame game)
    {
        if (result.Solved)
        {
            return GetRandomMessage(Messages.SolvedMessages);
        }

        if (result.Deadlock)
        {
            return GetRandomMessage(Messages.DeadlockMessages);
        }

        if (result.Pushed)
        {
            _movesSinceLastPush = 0;
            // Véletlenszerű bátorítás láda tolás után
            if (_random.Next(100) > 70)
            {
                return GetRandomMessage(Messages.Encouragements);
            }
        }
        else
        {
            _movesSinceLastPush++;
            if (_movesSinceLastPush >= StuckMoveThreshold)
            {
                _movesSinceLastPush = 0;
                return GetRandomMessage(Messages.StuckMessages);
            }
        }

        return null;
    }

    /// <summary>
    /// Hibamegelőzés: proaktív figyelmeztetés veszélyes lépések előtt.
    /// Ellenőrzi, hogy a játékos bármelyik irányban tolna-e ládát zsákutcába,
    /// és ha igen, figyelmezteti a lehetséges bajt MIELŐTT az megtörténne.
    /// 
    /// A szakdolgozatban hivatkozható: proaktív hibamegelőzés, az MI
    /// a játékos döntése előtt figyelmeztet a veszélyes lépésekre.
    /// </summary>
    /// <param name="game">Az aktuális játékállapot</param>
    /// <returns>Figyelmeztetés szövege, vagy null ha nincs veszélyes lépés</returns>
    public string? GetProactiveWarning(SokobanGame game)
    {
        var dangerousDirs = MoveDirection.All
            .Where(dir => game.PredictDeadlock(dir.DRow, dir.DCol))
            .Select(dir => $"{dir.Name} ({dir.Arrow})")
            .ToList();

        if (dangerousDirs.Count == 0)
            return null;

        string dirs = string.Join(" vagy ", dangerousDirs);
        return $"⚠️ Hibamegelőzés: Ha {dirs} tolod a ládát, zsákutcába kerül!\n" +
               $"Használd az 'U' billentyűt ha meggondolod magad.";
    }

    /// <summary>
    /// Üdvözlő üzenet új pálya betöltésekor.
    /// </summary>
    public string GetWelcomeMessage(Level level, int levelNum)
    {
        return $"🎮 {levelNum}. pálya: \"{level.Name}\" ({level.Difficulty})\n" +
               $"Told a ládákat ($) a célhelyekre (.)!\n" +
               $"💡 {GetRandomMessage(Messages.StrategyTips)}";
    }

    /// <summary>
    /// Hint statisztikák - hányszor kért a játékos segítséget.
    /// 
    /// A szakdolgozatban hivatkozható: játékos–AI együttműködés mérése.
    /// Minél több hintet kér a játékos, annál inkább támaszkodik az AI-ra.
    /// Ez az adat felhasználható a nehézségi szint és az AI hatékonyságának
    /// elemzéséhez.
    /// </summary>
    public int HintsUsed => _hintCount;

    /// <summary>
    /// Hint számláló és statisztikák resetelése.
    /// Hívandó új pálya betöltésekor vagy újraindításkor,
    /// hogy a statisztikák az aktuális pályára vonatkozzanak.
    /// </summary>
    public void Reset()
    {
        _hintCount = 0;
        _movesSinceLastPush = 0;
    }
}

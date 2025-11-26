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

    private static readonly Random _random = new();

    public HintSystem(AISolver solver)
    {
        _solver = solver;
        _hintCount = 0;
    }

    /// <summary>
    /// Véletlenszerű üzenet egy tömbből.
    /// </summary>
    private static string GetRandomMessage(string[] messages)
    {
        return messages[_random.Next(messages.Length)];
    }

    /// <summary>
    /// Hint generálása - megadja a következő ajánlott lépést.
    /// Növeli a hint számlálót a statisztikákhoz.
    /// </summary>
    public string GenerateHint(SokobanGame game)
    {
        _hintCount++;

        var nextMove = _solver.GetNextMove(game);

        if (nextMove == null)
        {
            return GetRandomMessage(Messages.DeadlockMessages) + "\nHasználd az 'U' billentyűt a visszalépéshez!";
        }

        var (move, totalMoves, pushCount) = nextMove.Value;
        string action = move!.Pushed
            ? $"Told a ládát {move.Direction.Name}ra ({move.Direction.Arrow})!"
            : $"Menj {move.Direction.Name}ra ({move.Direction.Arrow})!";

        return $"{action}\n(Még {totalMoves} lépés, {pushCount} tolás a megoldásig)";
    }

    /// <summary>
    /// Részletes állapot elemzés - átfogó információ a játék állapotáról.
    /// Nem növeli a hint számlálót, mert ez inkább elemzés, mint segítség.
    /// </summary>
    public string GenerateDetailedHint(SokobanGame game)
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

        var solution = _solver.Solve(game);
        if (solution.Success)
        {
            lines.Add("✅ A pálya megoldható!");
            lines.Add($"Hátralévő lépések: ~{solution.Moves.Count}");
        }
        else
        {
            lines.Add("⚠️ A pálya nem megoldható ebből az állapotból!");
            lines.Add("Használd az 'U' billentyűt a visszalépéshez!");
        }

        lines.Add("");
        lines.Add($"💡 Tipp: {GetRandomMessage(Messages.StrategyTips)}");

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Lépés utáni visszajelzés generálása.
    /// Bátorító üzenetek, deadlock figyelmeztetések, győzelmi gratulációk.
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

        // Véletlenszerű bátorítás láda tolás után
        if (result.Pushed && _random.Next(100) > 70)
        {
            return GetRandomMessage(Messages.Encouragements);
        }

        return null;
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
    }
}

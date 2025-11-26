namespace Sokoban;

/// <summary>
/// AI Hint Rendszer
/// Segíti a játékost hint-ekkel és stratégiai tanácsokkal
/// </summary>
public class HintSystem
{
    private readonly AISolver _solver;
    private int _hintCount;

    private static readonly string[] Encouragements = new[]
    {
        "Szuper, jó úton haladsz! 👍",
        "Remek lépés volt!",
        "Folytatsd így!",
        "Nagyon jól csinálod!",
        "Egy láda már a helyén! ✅"
    };

    private static readonly string[] StuckMessages = new[]
    {
        "Úgy látom, elakadtál. Kérj egy hint-et!",
        "Ne aggódj, a Sokoban nehéz játék. Segítek!",
        "Próbáld meg a 'H' billentyűt a hint-ért!"
    };

    private static readonly string[] DeadlockMessages = new[]
    {
        "⚠️ Vigyázz! Egy láda zsákutcába került!",
        "⚠️ Ez a láda már nem mozdítható a célhelyre!",
        "⚠️ Deadlock! Használd az 'U' billentyűt a visszalépéshez!"
    };

    private static readonly string[] SolvedMessages = new[]
    {
        "🎉 Fantasztikus! Teljesítetted a pályát!",
        "🏆 Gratulálok a győzelemhez!",
        "⭐ Kiváló munka!"
    };

    private static readonly string[] StrategyTips = new[]
    {
        "Először gondold végig, melyik ládát mozdítsd!",
        "A sarokban lévő ládákat nehéz kimozdítani.",
        "Próbáld a ládákat a fal mentén a célhelyek felé tolni.",
        "Néha vissza kell lépni, hogy előre juss.",
        "A ládák sorrendje is számít!",
        "Vigyázz, hogy ne told sarokba a ládát!"
    };

    private static readonly Random _random = new();

    public HintSystem(AISolver solver)
    {
        _solver = solver;
        _hintCount = 0;
    }

    /// <summary>
    /// Véletlenszerű üzenet egy tömbből
    /// </summary>
    private static string GetRandomMessage(string[] messages)
    {
        return messages[_random.Next(messages.Length)];
    }

    /// <summary>
    /// Hint generálása
    /// </summary>
    public string GenerateHint(SokobanGame game)
    {
        _hintCount++;

        var nextMove = _solver.GetNextMove(game);

        if (nextMove == null)
        {
            return GetRandomMessage(DeadlockMessages) + "\nHasználd az 'U' billentyűt a visszalépéshez!";
        }

        var (move, totalMoves, pushCount) = nextMove.Value;
        string action = move!.Pushed
            ? $"Told a ládát {move.Direction.Name}ra ({move.Direction.Arrow})!"
            : $"Menj {move.Direction.Name}ra ({move.Direction.Arrow})!";

        return $"{action}\n(Még {totalMoves} lépés, {pushCount} tolás a megoldásig)";
    }

    /// <summary>
    /// Részletes állapot elemzés
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
        lines.Add($"💡 Tipp: {GetRandomMessage(StrategyTips)}");

        return string.Join("\n", lines);
    }

    /// <summary>
    /// Lépés utáni visszajelzés
    /// </summary>
    public string? GetMoveResponse(MoveResult result, SokobanGame game)
    {
        if (result.Solved)
        {
            return GetRandomMessage(SolvedMessages);
        }

        if (result.Deadlock)
        {
            return GetRandomMessage(DeadlockMessages);
        }

        // Véletlenszerű bátorítás
        if (result.Pushed && _random.Next(100) > 70)
        {
            return GetRandomMessage(Encouragements);
        }

        return null;
    }

    /// <summary>
    /// Üdvözlő üzenet
    /// </summary>
    public string GetWelcomeMessage(Level level, int levelNum)
    {
        return $"🎮 {levelNum}. pálya: \"{level.Name}\" ({level.Difficulty})\n" +
               $"Told a ládákat ($) a célhelyekre (.)!\n" +
               $"💡 {GetRandomMessage(StrategyTips)}";
    }

    /// <summary>
    /// Hint statisztikák
    /// </summary>
    public int HintsUsed => _hintCount;

    /// <summary>
    /// Reset
    /// </summary>
    public void Reset()
    {
        _hintCount = 0;
    }
}

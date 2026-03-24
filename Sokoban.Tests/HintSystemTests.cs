using Xunit;

namespace Sokoban.Tests;

// ============================================================================
// UNIT TESTS / EGYSÉGTESZTEK
// Ez a fájl a HintSystem egységtesztjeit tartalmazza.
// A szakdolgozatban hivatkozható: AI hint rendszer tesztelés.
// ============================================================================

/// <summary>
/// HintSystem osztály egységtesztjei.
/// </summary>
public class HintSystemTests
{
    private static AISolver CreateSolver() => new AISolver();

    /// <summary>
    /// Teszt: HintSystem konstruktor null solver-rel kivételt dob.
    /// </summary>
    [Fact]
    public void Constructor_WithNullSolver_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new HintSystem(null!));
    }

    /// <summary>
    /// Egyszerű megoldható pálya
    /// </summary>
    private static Level CreateSolvableLevel() => new Level(
        "Solvable",
        "Test",
        new string[]
        {
            "#####",
            "#   #",
            "#@$.#",
            "#   #",
            "#####"
        });

    /// <summary>
    /// Pálya ahol a játékos egy lépéssel zsákutcába tolhat egy ládát
    /// </summary>
    private static Level CreateDangerousLevel() => new Level(
        "Dangerous",
        "Test",
        new string[]
        {
            "#####",
            "#  ##",
            "#@$ #",
            "#  .#",
            "#####"
        });

    /// <summary>
    /// Teszt: GetProactiveWarning - veszélyes lépés esetén figyelmeztet.
    /// </summary>
    [Fact]
    public void GetProactiveWarning_DangerousPush_ReturnsWarning()
    {
        var solver = CreateSolver();
        var hint = new HintSystem(solver);
        var game = new SokobanGame(CreateDangerousLevel());

        // (2,2) láda jobbra tolása -> (2,3) sarokba kerül (fent és jobbra is fal)
        var warning = hint.GetProactiveWarning(game);

        Assert.NotNull(warning);
        Assert.Contains("⚠️", warning);
        Assert.Contains("zsákutcába", warning);
    }

    /// <summary>
    /// Teszt: GetProactiveWarning - nincs veszélyes lépés esetén null.
    /// </summary>
    [Fact]
    public void GetProactiveWarning_NoDangerousMoves_ReturnsNull()
    {
        var solver = CreateSolver();
        var hint = new HintSystem(solver);
        var game = new SokobanGame(CreateSolvableLevel());

        // Az egyszerű pályán a láda biztonságosan tolható célra
        var warning = hint.GetProactiveWarning(game);

        // Nincs veszélyes lépés (a cél melletti sarok, de a cél irányában tolható)
        Assert.Null(warning);
    }

    /// <summary>
    /// Teszt: GenerateHint - megoldható pályán lépésjavaslatot ad.
    /// </summary>
    [Fact]
    public void GenerateHint_SolvableLevel_ReturnsHint()
    {
        var solver = CreateSolver();
        var hint = new HintSystem(solver);
        var game = new SokobanGame(CreateSolvableLevel());

        var result = hint.GenerateHint(game);

        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    /// <summary>
    /// Teszt: GenerateDetailedHint - állapot elemzést ad.
    /// </summary>
    [Fact]
    public void GenerateDetailedHint_SolvableLevel_ContainsStats()
    {
        var solver = CreateSolver();
        var hint = new HintSystem(solver);
        var game = new SokobanGame(CreateSolvableLevel());

        var result = hint.GenerateDetailedHint(game);

        Assert.Contains("láda", result);
        Assert.Contains("lépés", result);
    }

    /// <summary>
    /// Teszt: HintsUsed számláló növekszik minden GenerateHint hívásnál.
    /// </summary>
    [Fact]
    public void HintsUsed_IncrementsOnEachHintCall()
    {
        var solver = CreateSolver();
        var hint = new HintSystem(solver);
        var game = new SokobanGame(CreateSolvableLevel());

        Assert.Equal(0, hint.HintsUsed);
        hint.GenerateHint(game);
        Assert.Equal(1, hint.HintsUsed);
        hint.GenerateHint(game);
        Assert.Equal(2, hint.HintsUsed);
    }

    /// <summary>
    /// Teszt: Reset visszaállítja a számlálókat.
    /// </summary>
    [Fact]
    public void Reset_AfterHints_ResetsCounter()
    {
        var solver = CreateSolver();
        var hint = new HintSystem(solver);
        var game = new SokobanGame(CreateSolvableLevel());

        hint.GenerateHint(game);
        hint.GenerateHint(game);
        hint.Reset();

        Assert.Equal(0, hint.HintsUsed);
    }

    /// <summary>
    /// Teszt: GetMoveResponse - deadlock esetén figyelmeztetést ad.
    /// </summary>
    [Fact]
    public void GetMoveResponse_Deadlock_ReturnsWarning()
    {
        var solver = CreateSolver();
        var hint = new HintSystem(solver);
        var game = new SokobanGame(CreateSolvableLevel());

        var result = new MoveResult { Success = true, Deadlock = true };
        var response = hint.GetMoveResponse(result, game);

        Assert.NotNull(response);
        Assert.Contains("⚠️", response);
    }

    /// <summary>
    /// Teszt: GetMoveResponse - megoldott pálya esetén gratulál.
    /// </summary>
    [Fact]
    public void GetMoveResponse_Solved_ReturnsCongratulations()
    {
        var solver = CreateSolver();
        var hint = new HintSystem(solver);
        var game = new SokobanGame(CreateSolvableLevel());

        var result = new MoveResult { Success = true, Solved = true };
        var response = hint.GetMoveResponse(result, game);

        Assert.NotNull(response);
    }

    /// <summary>
    /// Teszt: GetWelcomeMessage - üdvözlő üzenetet ad.
    /// </summary>
    [Fact]
    public void GetWelcomeMessage_ReturnsMessage()
    {
        var solver = CreateSolver();
        var hint = new HintSystem(solver);
        var level = CreateSolvableLevel();

        var msg = hint.GetWelcomeMessage(level, 1);

        Assert.NotNull(msg);
        Assert.NotEmpty(msg);
        Assert.Contains("1.", msg);
    }
}

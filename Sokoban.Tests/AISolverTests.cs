using Xunit;

namespace Sokoban.Tests;

// ============================================================================
// UNIT TESTS / EGYSÉGTESZTEK
// Ez a fájl az AI Solver egységtesztjeit tartalmazza.
// A szakdolgozatban hivatkozható: AI algoritmus tesztelés.
// ============================================================================

/// <summary>
/// AISolver osztály egységtesztjei.
/// </summary>
public class AISolverTests
{
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
    /// Már megoldott pálya
    /// </summary>
    private static Level CreateSolvedLevel() => new Level(
        "Solved",
        "Test",
        new string[]
        {
            "#####",
            "#   #",
            "#@* #",
            "#   #",
            "#####"
        });
    
    /// <summary>
    /// Deadlock állapotú pálya (sarok)
    /// </summary>
    private static Level CreateDeadlockLevel() => new Level(
        "Deadlock",
        "Test",
        new string[]
        {
            "#####",
            "#$  #",
            "#@  #",
            "#  .#",
            "#####"
        });

    /// <summary>
    /// Teszt: megoldható pálya megoldása.
    /// </summary>
    [Fact]
    public void Solve_SolvableLevel_FindsSolution()
    {
        var solver = new AISolver();
        var game = new SokobanGame(CreateSolvableLevel());
        
        var result = solver.Solve(game);
        
        Assert.True(result.Success);
        Assert.NotEmpty(result.Moves);
    }
    
    /// <summary>
    /// Teszt: már megoldott pálya.
    /// </summary>
    [Fact]
    public void Solve_AlreadySolved_ReturnsEmptySolution()
    {
        var solver = new AISolver();
        var game = new SokobanGame(CreateSolvedLevel());
        
        var result = solver.Solve(game);
        
        Assert.True(result.Success);
        Assert.Empty(result.Moves);
    }
    
    /// <summary>
    /// Teszt: GetNextMove visszaadja az első lépést.
    /// </summary>
    [Fact]
    public void GetNextMove_SolvableLevel_ReturnsFirstMove()
    {
        var solver = new AISolver();
        var game = new SokobanGame(CreateSolvableLevel());
        
        var result = solver.GetNextMove(game);
        
        Assert.NotNull(result);
        Assert.NotNull(result.Value.Move);
    }
    
    /// <summary>
    /// Teszt: heurisztika számítása.
    /// </summary>
    [Fact]
    public void CalculateHeuristic_BoxNotOnGoal_ReturnsPositiveValue()
    {
        var solver = new AISolver();
        var game = new SokobanGame(CreateSolvableLevel());
        
        var heuristic = solver.CalculateHeuristic(game);
        
        Assert.True(heuristic > 0);
    }
    
    /// <summary>
    /// Teszt: heurisztika megoldott pályánál 0.
    /// </summary>
    [Fact]
    public void CalculateHeuristic_SolvedLevel_ReturnsZero()
    {
        var solver = new AISolver();
        var game = new SokobanGame(CreateSolvedLevel());
        
        var heuristic = solver.CalculateHeuristic(game);
        
        Assert.Equal(0, heuristic);
    }
    
    /// <summary>
    /// Teszt: deadlock felismerése.
    /// </summary>
    [Fact]
    public void IsDeadlock_CornerBox_ReturnsTrue()
    {
        var solver = new AISolver();
        var game = new SokobanGame(CreateDeadlockLevel());
        
        // A láda (1,1) pozíción van, ami sarok
        var isDeadlock = solver.IsDeadlock(game, 1, 1);
        
        Assert.True(isDeadlock);
    }
    
    /// <summary>
    /// Teszt: MaxIterations tiszteletben tartása.
    /// </summary>
    [Fact]
    public void Solve_RespectsMaxIterations()
    {
        var solver = new AISolver(maxIterations: 10);
        var game = new SokobanGame(CreateSolvableLevel());
        
        var result = solver.Solve(game);
        
        Assert.True(result.Iterations <= 10 || result.Success);
    }
    
    /// <summary>
    /// Teszt: iterációk száma naplózása.
    /// </summary>
    [Fact]
    public void Solve_TracksIterationCount()
    {
        var solver = new AISolver();
        var game = new SokobanGame(CreateSolvableLevel());
        
        solver.Solve(game);
        
        Assert.True(solver.LastIterationCount > 0);
    }
}

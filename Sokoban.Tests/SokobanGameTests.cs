using Xunit;

namespace Sokoban.Tests;

// ============================================================================
// UNIT TESTS / EGYSÉGTESZTEK
// Ez a fájl a játéklogika egységtesztjeit tartalmazza.
// A szakdolgozatban hivatkozható: Tesztvezérelt fejlesztés (TDD).
// ============================================================================

/// <summary>
/// SokobanGame osztály egységtesztjei.
/// </summary>
public class SokobanGameTests
{
    /// <summary>
    /// Egyszerű teszt pálya - egy láda, egy cél
    /// </summary>
    private static Level CreateSimpleLevel() => new Level(
        "Test Level",
        "Teszt",
        new string[]
        {
            "#####",
            "#   #",
            "#@$.#",
            "#   #",
            "#####"
        });

    /// <summary>
    /// Teszt: játék inicializálása helyes állapottal.
    /// </summary>
    [Fact]
    public void Initialize_SetsCorrectPlayerPosition()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        
        Assert.Equal((2, 1), game.PlayerPosition);
    }
    
    /// <summary>
    /// Teszt: lépésszámláló 0-ról indul.
    /// </summary>
    [Fact]
    public void Initialize_MovesStartAtZero()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        
        Assert.Equal(0, game.Moves);
        Assert.Equal(0, game.Pushes);
    }
    
    /// <summary>
    /// Teszt: játékos nem mehet falba.
    /// </summary>
    [Fact]
    public void Move_IntoWall_ReturnsFalse()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        
        var result = game.Move(0, -1); // Bal oldal (0,-1) - fal van ott
        
        Assert.False(result.Success);
        Assert.Equal("wall", result.Reason);
    }
    
    /// <summary>
    /// Teszt: szabad mezőre lépés sikeres.
    /// </summary>
    [Fact]
    public void Move_ToEmptySpace_Succeeds()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        
        var result = game.Move(-1, 0); // Fel - üres mező van ott (1,1)
        
        Assert.True(result.Success);
        Assert.False(result.Pushed);
        Assert.Equal(1, game.Moves);
    }
    
    /// <summary>
    /// Teszt: láda tolása.
    /// </summary>
    [Fact]
    public void Move_PushBox_SucceedsAndIncrementsPushes()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        
        var result = game.Move(0, 1); // Jobbra - láda van ott
        
        Assert.True(result.Success);
        Assert.True(result.Pushed);
        Assert.Equal(1, game.Moves);
        Assert.Equal(1, game.Pushes);
    }
    
    /// <summary>
    /// Teszt: láda célhelyre tolása megoldja a pályát.
    /// </summary>
    [Fact]
    public void Move_PushBoxToGoal_SolvesLevel()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        
        var result = game.Move(0, 1); // Láda a célhelyre kerül
        
        Assert.True(result.Solved);
        Assert.True(game.IsSolved());
    }
    
    /// <summary>
    /// Teszt: undo működik.
    /// </summary>
    [Fact]
    public void Undo_AfterMove_RestoresState()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        var originalPos = game.PlayerPosition;
        
        game.Move(-1, 0); // Fel (érvényes lépés)
        Assert.NotEqual(originalPos, game.PlayerPosition);
        
        var undoResult = game.Undo();
        
        Assert.True(undoResult);
        Assert.Equal(originalPos, game.PlayerPosition);
        Assert.Equal(0, game.Moves);
    }
    
    /// <summary>
    /// Teszt: undo üres történettel.
    /// </summary>
    [Fact]
    public void Undo_WithNoHistory_ReturnsFalse()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        
        var result = game.Undo();
        
        Assert.False(result);
    }
    
    /// <summary>
    /// Teszt: restart visszaállítja az eredeti állapotot.
    /// </summary>
    [Fact]
    public void Restart_ResetsToInitialState()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        var originalPos = game.PlayerPosition;
        
        game.Move(0, -1);
        game.Move(0, -1);
        
        game.Restart();
        
        Assert.Equal(originalPos, game.PlayerPosition);
        Assert.Equal(0, game.Moves);
        Assert.Equal(0, game.Pushes);
    }
    
    /// <summary>
    /// Teszt: sarok deadlock felismerése.
    /// </summary>
    [Fact]
    public void CheckDeadlock_CornerBox_ReturnsTrue()
    {
        var level = new Level("Corner Test", "Test", new string[]
        {
            "#####",
            "#$  #",
            "# @ #",
            "#  .#",
            "#####"
        });
        var game = new SokobanGame(level);
        
        // A láda sarokba tolása
        game.Move(-1, 0); // Fel
        game.Move(0, -1); // Bal - láda sarokba kerül
        
        // A (1,1) pozíció sarok
        Assert.True(game.IsCornerDeadlock(1, 1));
    }
    
    /// <summary>
    /// Teszt: láda célhelyen nem deadlock.
    /// </summary>
    [Fact]
    public void CheckDeadlock_BoxOnGoal_ReturnsFalse()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        
        game.Move(0, 1); // Láda célhelyre
        
        Assert.False(game.CheckDeadlock(2, 3));
    }
    
    /// <summary>
    /// Teszt: GetStateKey egyedi állapotokra különböző.
    /// </summary>
    [Fact]
    public void GetStateKey_DifferentStates_ReturnsDifferentKeys()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        var key1 = game.GetStateKey();
        
        game.Move(-1, 0); // Fel - érvényes lépés
        var key2 = game.GetStateKey();
        
        Assert.NotEqual(key1, key2);
    }
    
    /// <summary>
    /// Teszt: azonos állapot azonos kulcsot ad.
    /// </summary>
    [Fact]
    public void GetStateKey_SameState_ReturnsSameKey()
    {
        var game = new SokobanGame(CreateSimpleLevel());
        var key1 = game.GetStateKey();
        
        game.Move(-1, 0); // Fel - érvényes lépés
        game.Undo();
        var key2 = game.GetStateKey();
        
        Assert.Equal(key1, key2);
    }
}

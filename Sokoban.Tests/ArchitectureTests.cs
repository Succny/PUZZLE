using Xunit;

namespace Sokoban.Tests;

using Sokoban.Application;
using Sokoban.Domain;
using Sokoban.Infrastructure;

// ============================================================================
// UNIT TESTS / EGYSÉGTESZTEK
// Ez a fájl az architektúra komponensek tesztjeit tartalmazza.
// A szakdolgozatban hivatkozható: Szolgáltatás réteg tesztelés.
// ============================================================================

/// <summary>
/// CachedHintProvider tesztek.
/// </summary>
public class CachedHintProviderTests
{
    private static Level CreateSimpleLevel() => new Level(
        "Test",
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
    /// Teszt: cache hit növeli a számlálót.
    /// </summary>
    [Fact]
    public void GetHint_SameState_UsesCacheAndIncrementsCacheHit()
    {
        var inner = new DefaultHintProvider();
        var cached = new CachedHintProvider(inner);
        var game = new SokobanGame(CreateSimpleLevel());
        var options = new HintOptions();
        
        // Első hívás - cache miss
        cached.GetHint(game, options);
        Assert.Equal(0, cached.CacheHits);
        Assert.Equal(1, cached.CacheMisses);
        
        // Második hívás - cache hit
        cached.GetHint(game, options);
        Assert.Equal(1, cached.CacheHits);
        Assert.Equal(1, cached.CacheMisses);
    }
    
    /// <summary>
    /// Teszt: különböző állapotok nem használják a cache-t.
    /// </summary>
    [Fact]
    public void GetHint_DifferentStates_DoesNotUseCache()
    {
        var inner = new DefaultHintProvider();
        var cached = new CachedHintProvider(inner);
        var game = new SokobanGame(CreateSimpleLevel());
        var options = new HintOptions();
        
        cached.GetHint(game, options);
        
        game.Move(-1, 0); // Fel - érvényes lépés, változtassuk meg az állapotot
        cached.GetHint(game, options);
        
        Assert.Equal(0, cached.CacheHits);
        Assert.Equal(2, cached.CacheMisses);
    }
    
    /// <summary>
    /// Teszt: cache törölhető.
    /// </summary>
    [Fact]
    public void ClearCache_ResetsStatistics()
    {
        var inner = new DefaultHintProvider();
        var cached = new CachedHintProvider(inner);
        var game = new SokobanGame(CreateSimpleLevel());
        var options = new HintOptions();
        
        cached.GetHint(game, options);
        cached.GetHint(game, options);
        
        cached.ClearCache();
        
        Assert.Equal(0, cached.CacheHits);
        Assert.Equal(0, cached.CacheMisses);
        Assert.Equal(0, cached.CacheSize);
    }
}

/// <summary>
/// Lokalizáció tesztek.
/// </summary>
public class LocalizationTests
{
    /// <summary>
    /// Teszt: magyar szöveg lekérése.
    /// </summary>
    [Fact]
    public void Get_HungarianKey_ReturnsHungarianText()
    {
        var loc = new JsonLocalization("hu");
        
        var text = loc.Get("victory.congrats");
        
        Assert.Contains("Fantasztikus", text);
    }
    
    /// <summary>
    /// Teszt: angol szöveg lekérése.
    /// </summary>
    [Fact]
    public void Get_EnglishKey_ReturnsEnglishText()
    {
        var loc = new JsonLocalization("en");
        
        var text = loc.Get("victory.congrats");
        
        Assert.Contains("Fantastic", text);
    }
    
    /// <summary>
    /// Teszt: nyelv váltás.
    /// </summary>
    [Fact]
    public void SetLanguage_ChangesCurrentLanguage()
    {
        var loc = new JsonLocalization("hu");
        Assert.Equal("hu", loc.CurrentLanguage);
        
        loc.SetLanguage("en");
        
        Assert.Equal("en", loc.CurrentLanguage);
    }
    
    /// <summary>
    /// Teszt: formázási argumentumok.
    /// </summary>
    [Fact]
    public void Get_WithArguments_FormatsCorrectly()
    {
        var loc = new JsonLocalization("hu");
        
        var text = loc.Get("stats.moves", 42);
        
        Assert.Contains("42", text);
    }
    
    /// <summary>
    /// Teszt: hiányzó kulcs visszaadja a kulcsot.
    /// </summary>
    [Fact]
    public void Get_MissingKey_ReturnsKey()
    {
        var loc = new JsonLocalization("hu");
        
        var text = loc.Get("nonexistent.key");
        
        Assert.Equal("nonexistent.key", text);
    }
}

/// <summary>
/// Telemetria tesztek.
/// </summary>
public class TelemetryTests
{
    /// <summary>
    /// Teszt: esemény rögzítése.
    /// </summary>
    [Fact]
    public void Track_Event_RecordsEvent()
    {
        var client = new ConsoleTelemetryClient(enabled: false);
        
        client.Track("TestEvent", new Dictionary<string, object>
        {
            ["Key1"] = "Value1",
            ["Key2"] = 42
        });
        
        var events = client.GetEvents();
        Assert.Single(events);
        Assert.Equal("TestEvent", events[0].Name);
        Assert.Equal("Value1", events[0].Properties["Key1"]);
    }
    
    /// <summary>
    /// Teszt: események törlése.
    /// </summary>
    [Fact]
    public void Clear_RemovesAllEvents()
    {
        var client = new ConsoleTelemetryClient(enabled: false);
        client.Track("Event1");
        client.Track("Event2");
        
        client.Clear();
        
        Assert.Empty(client.GetEvents());
    }
}

/// <summary>
/// LevelLoader tesztek.
/// </summary>
public class LevelLoaderTests
{
    /// <summary>
    /// Teszt: összes pálya betöltése.
    /// </summary>
    [Fact]
    public void LoadAllLevels_ReturnsAllLevels()
    {
        var loader = new DefaultLevelLoader();
        
        var levels = loader.LoadAllLevels();
        
        Assert.Equal(Levels.AllLevels.Length, levels.Count);
    }
    
    /// <summary>
    /// Teszt: pálya betöltése index alapján.
    /// </summary>
    [Fact]
    public void LoadLevel_ValidIndex_ReturnsLevel()
    {
        var loader = new DefaultLevelLoader();
        
        var level = loader.LoadLevel(0);
        
        Assert.NotNull(level);
        Assert.Equal(Levels.AllLevels[0].Name, level.Name);
    }
    
    /// <summary>
    /// Teszt: érvénytelen index null-t ad.
    /// </summary>
    [Fact]
    public void LoadLevel_InvalidIndex_ReturnsNull()
    {
        var loader = new DefaultLevelLoader();
        
        var level = loader.LoadLevel(999);
        
        Assert.Null(level);
    }
    
    /// <summary>
    /// Teszt: pálya betöltése név alapján.
    /// </summary>
    [Fact]
    public void LoadLevelByName_ExistingName_ReturnsLevel()
    {
        var loader = new DefaultLevelLoader();
        var expectedName = Levels.AllLevels[0].Name;
        
        var level = loader.LoadLevelByName(expectedName);
        
        Assert.NotNull(level);
        Assert.Equal(expectedName, level.Name);
    }
}

/// <summary>
/// GameEvents tesztek.
/// </summary>
public class GameEventsTests
{
    /// <summary>
    /// Teszt: esemény kiváltása és kezelése.
    /// </summary>
    [Fact]
    public void RaiseLevelLoaded_TriggersEvent()
    {
        GameEvents.ClearAllSubscribers();
        
        bool eventFired = false;
        GameEvents.OnLevelLoaded += (args) => eventFired = true;
        
        GameEvents.RaiseLevelLoaded(new LevelLoadedEventArgs
        {
            Level = Levels.AllLevels[0],
            LevelIndex = 0
        });
        
        Assert.True(eventFired);
        
        GameEvents.ClearAllSubscribers();
    }
    
    /// <summary>
    /// Teszt: ClearAllSubscribers eltávolítja az összes listenert.
    /// </summary>
    [Fact]
    public void ClearAllSubscribers_RemovesAllListeners()
    {
        bool eventFired = false;
        GameEvents.OnLevelLoaded += (args) => eventFired = true;
        
        GameEvents.ClearAllSubscribers();
        
        GameEvents.RaiseLevelLoaded(new LevelLoadedEventArgs
        {
            Level = Levels.AllLevels[0],
            LevelIndex = 0
        });
        
        Assert.False(eventFired);
    }
}

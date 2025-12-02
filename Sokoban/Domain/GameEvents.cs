namespace Sokoban.Domain;

// ============================================================================
// DOMAIN LAYER / ESEMÉNYEK
// Ez a fájl a játék eseményeket tartalmazza a decoupled architektúrához.
// A szakdolgozatban hivatkozható: Event-driven architektúra.
// ============================================================================

/// <summary>
/// [Domain Layer]
/// Központi játék események.
/// Lehetővé teszi az AI rendszer és az UI réteg szétválasztását.
/// 
/// A szakdolgozatban hivatkozható: 
/// - Observer pattern alkalmazása játék eseményekre
/// - Decouple-olt architektúra az AI és UI között
/// </summary>
public static class GameEvents
{
    /// <summary>Pálya betöltésekor hívódik</summary>
    public static event Action<LevelLoadedEventArgs>? OnLevelLoaded;
    
    /// <summary>Lépés végrehajtásakor hívódik</summary>
    public static event Action<MoveAppliedEventArgs>? OnMoveApplied;
    
    /// <summary>Hint kérésekor hívódik</summary>
    public static event Action<HintRequestContext>? OnHintRequested;
    
    /// <summary>Hint megjelenítésekor hívódik</summary>
    public static event Action<HintRecommendation>? OnHintShown;
    
    /// <summary>Pálya teljesítésekor hívódik</summary>
    public static event Action<LevelCompletedEventArgs>? OnLevelCompleted;
    
    /// <summary>Deadlock észlelésekor hívódik</summary>
    public static event Action<DeadlockEventArgs>? OnDeadlockDetected;

    /// <summary>Pálya betöltés esemény kiváltása</summary>
    public static void RaiseLevelLoaded(LevelLoadedEventArgs args) => OnLevelLoaded?.Invoke(args);
    
    /// <summary>Lépés alkalmazás esemény kiváltása</summary>
    public static void RaiseMoveApplied(MoveAppliedEventArgs args) => OnMoveApplied?.Invoke(args);
    
    /// <summary>Hint kérés esemény kiváltása</summary>
    public static void RaiseHintRequested(HintRequestContext ctx) => OnHintRequested?.Invoke(ctx);
    
    /// <summary>Hint megjelenítés esemény kiváltása</summary>
    public static void RaiseHintShown(HintRecommendation rec) => OnHintShown?.Invoke(rec);
    
    /// <summary>Pálya teljesítés esemény kiváltása</summary>
    public static void RaiseLevelCompleted(LevelCompletedEventArgs args) => OnLevelCompleted?.Invoke(args);
    
    /// <summary>Deadlock észlelés esemény kiváltása</summary>
    public static void RaiseDeadlockDetected(DeadlockEventArgs args) => OnDeadlockDetected?.Invoke(args);
    
    /// <summary>Összes esemény listener eltávolítása (teszteléshez)</summary>
    public static void ClearAllSubscribers()
    {
        OnLevelLoaded = null;
        OnMoveApplied = null;
        OnHintRequested = null;
        OnHintShown = null;
        OnLevelCompleted = null;
        OnDeadlockDetected = null;
    }
}

/// <summary>
/// [Domain Layer]
/// Pálya betöltés esemény argumentumok.
/// </summary>
public class LevelLoadedEventArgs
{
    /// <summary>A betöltött pálya</summary>
    public required Level Level { get; init; }
    
    /// <summary>A pálya indexe</summary>
    public int LevelIndex { get; init; }
    
    /// <summary>Betöltés időpontja</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// [Domain Layer]
/// Lépés végrehajtás esemény argumentumok.
/// </summary>
public class MoveAppliedEventArgs
{
    /// <summary>A lépés iránya</summary>
    public required MoveDirection Direction { get; init; }
    
    /// <summary>A lépés eredménye</summary>
    public required MoveResult Result { get; init; }
    
    /// <summary>Az új állapot kulcsa</summary>
    public required string NewStateKey { get; init; }
    
    /// <summary>Lépés időpontja</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// [Domain Layer]
/// Pálya teljesítés esemény argumentumok.
/// </summary>
public class LevelCompletedEventArgs
{
    /// <summary>A teljesített pálya</summary>
    public required Level Level { get; init; }
    
    /// <summary>A pálya indexe</summary>
    public int LevelIndex { get; init; }
    
    /// <summary>Összes lépés száma</summary>
    public int TotalMoves { get; init; }
    
    /// <summary>Összes tolás száma</summary>
    public int TotalPushes { get; init; }
    
    /// <summary>Felhasznált hintek száma</summary>
    public int HintsUsed { get; init; }
    
    /// <summary>Eltelt idő</summary>
    public TimeSpan ElapsedTime { get; init; }
    
    /// <summary>Teljesítés időpontja</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// [Domain Layer]
/// Deadlock észlelés esemény argumentumok.
/// </summary>
public class DeadlockEventArgs
{
    /// <summary>A deadlock-ba került láda sor pozíciója</summary>
    public int BoxRow { get; init; }
    
    /// <summary>A deadlock-ba került láda oszlop pozíciója</summary>
    public int BoxCol { get; init; }
    
    /// <summary>A deadlock típusa</summary>
    public DeadlockType Type { get; init; }
    
    /// <summary>Észlelés időpontja</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// [Domain Layer]
/// Deadlock típusok.
/// </summary>
public enum DeadlockType
{
    /// <summary>Sarok deadlock - láda sarokba szorult</summary>
    Corner,
    
    /// <summary>Fal-vonal deadlock - láda fal mentén, cél nélkül</summary>
    WallLine,
    
    /// <summary>Freeze deadlock - több láda egymást blokkolja</summary>
    Freeze,
    
    /// <summary>Ismeretlen típusú deadlock</summary>
    Unknown
}

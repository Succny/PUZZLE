namespace Sokoban.Domain;

// ============================================================================
// DOMAIN LAYER / INTERFÉSZEK
// Ez a fájl az AI hint rendszer és más szolgáltatások interfészeit tartalmazza.
// A szakdolgozatban hivatkozható: Domain interfészek, Dependency Injection.
// ============================================================================

/// <summary>
/// [Domain Layer]
/// Hint ajánlás eredménye.
/// Tartalmazza a javasolt lépést és a minőség metrikákat.
/// </summary>
public class HintRecommendation
{
    /// <summary>A javasolt lépés iránya</summary>
    public MoveDirection Direction { get; init; } = MoveDirection.Up;
    
    /// <summary>Történik-e láda tolás a lépés során</summary>
    public bool IsPush { get; init; }
    
    /// <summary>Összesen hány lépés van hátra a megoldásig</summary>
    public int RemainingMoves { get; init; }
    
    /// <summary>Hány tolás van hátra a megoldásig</summary>
    public int RemainingPushes { get; init; }
    
    /// <summary>A hint minősége (0.0 - 1.0, ahol 1.0 a legjobb)</summary>
    public double Quality { get; init; } = 1.0;
    
    /// <summary>Opcionális magyarázat a hint-hez</summary>
    public string? Explanation { get; init; }
}

/// <summary>
/// [Domain Layer]
/// Hint kérés beállításai.
/// Konfigurálható paraméterek a hint generáláshoz.
/// </summary>
public class HintOptions
{
    /// <summary>Maximális számítási idő milliszekundumban</summary>
    public int MaxMilliseconds { get; init; } = 5000;
    
    /// <summary>Elvárt minimum minőség (0.0 - 1.0)</summary>
    public double TargetQuality { get; init; } = 0.8;
    
    /// <summary>Hint szint: Soft (irány), Medium (objektum), Hard (pontos lépés)</summary>
    public HintLevel Level { get; init; } = HintLevel.Hard;
    
    /// <summary>Cache használata engedélyezett-e</summary>
    public bool UseCache { get; init; } = true;
}

/// <summary>
/// [Domain Layer]
/// Hint részletességi szintek.
/// </summary>
public enum HintLevel
{
    /// <summary>Lágy hint - csak irány jelzése</summary>
    Soft,
    
    /// <summary>Közepes hint - melyik játékos/objektum érintett</summary>
    Medium,
    
    /// <summary>Kemény hint - pontos lépés megadása</summary>
    Hard
}

/// <summary>
/// [Domain Layer]
/// Állapot hash számításhoz.
/// </summary>
public readonly struct StateHash : IEquatable<StateHash>
{
    private readonly int _hash;
    
    private StateHash(int hash)
    {
        _hash = hash;
    }
    
    /// <summary>
    /// Állapot hash számítása a játékállapotból és beállításokból.
    /// </summary>
    public static StateHash Compute(SokobanGame state, HintOptions options)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + state.GetStateKey().GetHashCode();
            hash = hash * 31 + options.Level.GetHashCode();
            return new StateHash(hash);
        }
    }
    
    public bool Equals(StateHash other) => _hash == other._hash;
    public override bool Equals(object? obj) => obj is StateHash other && Equals(other);
    public override int GetHashCode() => _hash;
}

/// <summary>
/// [Domain Layer]
/// Hint szolgáltató interfész.
/// Minden hint algoritmus implementálja ezt az interfészt.
/// 
/// A szakdolgozatban hivatkozható: Strategy pattern alkalmazása hint algoritmusokra.
/// </summary>
public interface IHintProvider
{
    /// <summary>
    /// Hint ajánlás generálása az adott állapothoz.
    /// </summary>
    /// <param name="state">Az aktuális játékállapot</param>
    /// <param name="options">Hint beállítások</param>
    /// <returns>Hint ajánlás, vagy null ha nem találunk megfelelő lépést</returns>
    HintRecommendation? GetHint(SokobanGame state, HintOptions options);
}

/// <summary>
/// [Domain Layer]
/// Pálya betöltő interfész.
/// 
/// A szakdolgozatban hivatkozható: Repository pattern pálya betöltéshez.
/// </summary>
public interface ILevelLoader
{
    /// <summary>Összes elérhető pálya betöltése</summary>
    IReadOnlyList<Level> LoadAllLevels();
    
    /// <summary>Adott pálya betöltése index alapján</summary>
    Level? LoadLevel(int index);
    
    /// <summary>Pálya betöltése név alapján</summary>
    Level? LoadLevelByName(string name);
}

/// <summary>
/// [Domain Layer]
/// Állapot szerializáló interfész save/load funkcióhoz.
/// </summary>
public interface IStateSerializer
{
    /// <summary>Játékállapot mentése</summary>
    string Serialize(SokobanGame game);
    
    /// <summary>Játékállapot visszaállítása</summary>
    SokobanGame? Deserialize(string data, Level level);
}

/// <summary>
/// [Domain Layer]
/// Telemetria kliens interfész.
/// Események és metrikák gyűjtésére szolgál.
/// 
/// A szakdolgozatban hivatkozható: telemetria és analitika implementáció.
/// </summary>
public interface ITelemetryClient
{
    /// <summary>
    /// Esemény naplózása.
    /// </summary>
    /// <param name="eventName">Az esemény neve</param>
    /// <param name="properties">Az esemény tulajdonságai</param>
    void Track(string eventName, IDictionary<string, object>? properties = null);
}

/// <summary>
/// [Domain Layer]
/// Lokalizációs interfész.
/// Többnyelvű szövegek kezelésére szolgál.
/// 
/// A szakdolgozatban hivatkozható: lokalizációs rendszer implementáció.
/// </summary>
public interface ILocalization
{
    /// <summary>
    /// Lokalizált szöveg lekérése kulcs alapján.
    /// </summary>
    /// <param name="key">A szöveg kulcsa</param>
    /// <param name="args">Formázási argumentumok</param>
    /// <returns>A lokalizált szöveg</returns>
    string Get(string key, params object[] args);
    
    /// <summary>
    /// Aktuális nyelv kódja (pl. "hu", "en")
    /// </summary>
    string CurrentLanguage { get; }
    
    /// <summary>
    /// Nyelv váltása
    /// </summary>
    void SetLanguage(string languageCode);
}

/// <summary>
/// [Domain Layer]
/// Hint kérés kontextus telemetriához.
/// </summary>
public class HintRequestContext
{
    /// <summary>A játék állapota hint kéréskor</summary>
    public required string StateKey { get; init; }
    
    /// <summary>A pálya indexe</summary>
    public int LevelIndex { get; init; }
    
    /// <summary>Eddigi lépések száma</summary>
    public int MoveCount { get; init; }
    
    /// <summary>Hint kérések száma eddig</summary>
    public int HintCount { get; init; }
    
    /// <summary>A kérés időpontja</summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}

namespace Sokoban.Infrastructure;

using Sokoban.Domain;

// ============================================================================
// INFRASTRUCTURE LAYER / TELEMETRIA
// Ez a fájl a telemetria és analitika implementációt tartalmazza.
// A szakdolgozatban hivatkozható: Telemetria és játékanalitika.
// ============================================================================

/// <summary>
/// [Infrastructure Layer]
/// Konzol alapú telemetria kliens (debug célokra).
/// A valós alkalmazásban ezt le lehet cserélni pl. Application Insights-ra.
/// </summary>
public sealed class ConsoleTelemetryClient : ITelemetryClient
{
    private readonly bool _enabled;
    private readonly List<TelemetryEvent> _events = new();
    
    public ConsoleTelemetryClient(bool enabled = false)
    {
        _enabled = enabled;
    }
    
    /// <inheritdoc/>
    public void Track(string eventName, IDictionary<string, object>? properties = null)
    {
        var telemetryEvent = new TelemetryEvent
        {
            Name = eventName,
            Properties = properties ?? new Dictionary<string, object>(),
            Timestamp = DateTime.UtcNow
        };
        
        _events.Add(telemetryEvent);
        
        if (_enabled)
        {
            Console.WriteLine($"[Telemetry] {eventName}: {string.Join(", ", telemetryEvent.Properties.Select(p => $"{p.Key}={p.Value}"))}");
        }
    }
    
    /// <summary>
    /// Összes rögzített esemény lekérése (teszteléshez és analitikához).
    /// </summary>
    public IReadOnlyList<TelemetryEvent> GetEvents() => _events.AsReadOnly();
    
    /// <summary>
    /// Események törlése.
    /// </summary>
    public void Clear() => _events.Clear();
}

/// <summary>
/// [Infrastructure Layer]
/// Telemetria esemény adatmodell.
/// </summary>
public class TelemetryEvent
{
    /// <summary>Az esemény neve</summary>
    public required string Name { get; init; }
    
    /// <summary>Az esemény tulajdonságai</summary>
    public required IDictionary<string, object> Properties { get; init; }
    
    /// <summary>Az esemény időpontja</summary>
    public DateTime Timestamp { get; init; }
}

/// <summary>
/// [Infrastructure Layer]
/// Telemetria integráció a játék eseményekkel.
/// Automatikusan naplózza a játék eseményeket.
/// </summary>
public sealed class TelemetryIntegration : IDisposable
{
    private readonly ITelemetryClient _client;
    
    public TelemetryIntegration(ITelemetryClient client)
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        
        // Feliratkozás a játék eseményekre
        GameEvents.OnLevelLoaded += HandleLevelLoaded;
        GameEvents.OnMoveApplied += HandleMoveApplied;
        GameEvents.OnHintRequested += HandleHintRequested;
        GameEvents.OnHintShown += HandleHintShown;
        GameEvents.OnLevelCompleted += HandleLevelCompleted;
        GameEvents.OnDeadlockDetected += HandleDeadlockDetected;
    }
    
    private void HandleLevelLoaded(LevelLoadedEventArgs args)
    {
        _client.Track("LevelLoaded", new Dictionary<string, object>
        {
            ["LevelIndex"] = args.LevelIndex,
            ["LevelName"] = args.Level.Name,
            ["Difficulty"] = args.Level.Difficulty
        });
    }
    
    private void HandleMoveApplied(MoveAppliedEventArgs args)
    {
        _client.Track("MoveApplied", new Dictionary<string, object>
        {
            ["Direction"] = args.Direction.Name,
            ["Success"] = args.Result.Success,
            ["Pushed"] = args.Result.Pushed,
            ["Deadlock"] = args.Result.Deadlock
        });
    }
    
    private void HandleHintRequested(HintRequestContext ctx)
    {
        _client.Track("HintRequested", new Dictionary<string, object>
        {
            ["LevelIndex"] = ctx.LevelIndex,
            ["MoveCount"] = ctx.MoveCount,
            ["HintCount"] = ctx.HintCount
        });
    }
    
    private void HandleHintShown(HintRecommendation rec)
    {
        _client.Track("HintShown", new Dictionary<string, object>
        {
            ["Direction"] = rec.Direction.Name,
            ["IsPush"] = rec.IsPush,
            ["RemainingMoves"] = rec.RemainingMoves,
            ["Quality"] = rec.Quality
        });
    }
    
    private void HandleLevelCompleted(LevelCompletedEventArgs args)
    {
        _client.Track("LevelCompleted", new Dictionary<string, object>
        {
            ["LevelIndex"] = args.LevelIndex,
            ["LevelName"] = args.Level.Name,
            ["TotalMoves"] = args.TotalMoves,
            ["TotalPushes"] = args.TotalPushes,
            ["HintsUsed"] = args.HintsUsed,
            ["ElapsedSeconds"] = args.ElapsedTime.TotalSeconds
        });
    }
    
    private void HandleDeadlockDetected(DeadlockEventArgs args)
    {
        _client.Track("DeadlockDetected", new Dictionary<string, object>
        {
            ["BoxRow"] = args.BoxRow,
            ["BoxCol"] = args.BoxCol,
            ["Type"] = args.Type.ToString()
        });
    }
    
    public void Dispose()
    {
        // Leiratkozás az eseményekről
        GameEvents.OnLevelLoaded -= HandleLevelLoaded;
        GameEvents.OnMoveApplied -= HandleMoveApplied;
        GameEvents.OnHintRequested -= HandleHintRequested;
        GameEvents.OnHintShown -= HandleHintShown;
        GameEvents.OnLevelCompleted -= HandleLevelCompleted;
        GameEvents.OnDeadlockDetected -= HandleDeadlockDetected;
    }
}

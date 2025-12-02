namespace Sokoban.Application;

using Sokoban.Domain;

// ============================================================================
// APPLICATION LAYER / ASZINKRON HINT SZOLGÁLTATÁS
// Ez a fájl az aszinkron hint számítást tartalmazza időkerettel.
// A szakdolgozatban hivatkozható: Aszinkron feldolgozás és időkeret kezelés.
// ============================================================================

/// <summary>
/// [Application Layer]
/// Időkeret kezelő a számítási idő korlátozásához.
/// </summary>
public sealed class TimeBudget : IDisposable
{
    private readonly DateTime _startTime;
    private readonly int _maxMilliseconds;
    
    /// <summary>Időkeret létrehozása.</summary>
    /// <param name="maxMilliseconds">Maximális idő milliszekundumban</param>
    public TimeBudget(int maxMilliseconds)
    {
        _startTime = DateTime.UtcNow;
        _maxMilliseconds = maxMilliseconds;
    }
    
    /// <summary>Lejárt-e az időkeret</summary>
    public bool IsExpired => ElapsedMilliseconds >= _maxMilliseconds;
    
    /// <summary>Eltelt idő milliszekundumban</summary>
    public double ElapsedMilliseconds => (DateTime.UtcNow - _startTime).TotalMilliseconds;
    
    /// <summary>Hátralévő idő milliszekundumban</summary>
    public double RemainingMilliseconds => Math.Max(0, _maxMilliseconds - ElapsedMilliseconds);
    
    public void Dispose() { }
}

/// <summary>
/// [Application Layer]
/// Aszinkron Hint Szolgáltatás időkerettel és megszakítási támogatással.
/// 
/// A szakdolgozatban hivatkozható:
/// - Aszinkron programozás C#-ban (async/await)
/// - CancellationToken használata
/// - Időkeret (time budget) kezelés valós idejű alkalmazásokban
/// </summary>
public sealed class AsyncHintService
{
    private readonly IHintProvider _provider;
    
    public AsyncHintService(IHintProvider provider)
    {
        _provider = provider ?? throw new ArgumentNullException(nameof(provider));
    }
    
    /// <summary>
    /// Hint számítása aszinkron módon, időkerettel.
    /// </summary>
    /// <param name="state">Az aktuális játékállapot</param>
    /// <param name="options">Hint beállítások</param>
    /// <param name="ct">Megszakítási token</param>
    /// <returns>Hint ajánlás, vagy null ha nem sikerült időben megtalálni</returns>
    public async Task<HintRecommendation?> CalculateHintAsync(
        SokobanGame state,
        HintOptions options,
        CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            using var budget = new TimeBudget(options.MaxMilliseconds);
            
            while (!budget.IsExpired && !ct.IsCancellationRequested)
            {
                var hint = _provider.GetHint(state, options);
                
                if (hint != null && hint.Quality >= options.TargetQuality)
                {
                    return hint;
                }
                
                // Ha nem érjük el a cél minőséget, visszaadjuk amit találtunk
                if (hint != null)
                {
                    return hint;
                }
                
                // Nincs eredmény, kilépünk
                break;
            }
            
            return null;
        }, ct);
    }
    
    /// <summary>
    /// Hint számítása szinkron módon időkerettel (nem blokkoló alternatíva nélkül).
    /// </summary>
    public HintRecommendation? CalculateHint(SokobanGame state, HintOptions options)
    {
        using var budget = new TimeBudget(options.MaxMilliseconds);
        
        if (budget.IsExpired)
        {
            return null;
        }
        
        return _provider.GetHint(state, options);
    }
}

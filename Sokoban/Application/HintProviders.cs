namespace Sokoban.Application;

using Sokoban.Domain;

// ============================================================================
// APPLICATION LAYER / SZOLGÁLTATÁSOK
// Ez a fájl a cached hint provider implementációt tartalmazza.
// A szakdolgozatban hivatkozható: Application szolgáltatások réteg.
// ============================================================================

/// <summary>
/// [Application Layer]
/// Cached Hint Provider - memorizálja a hint eredményeket.
/// 
/// A szakdolgozatban hivatkozható:
/// - Memoization pattern alkalmazása hint cache-eléshez
/// - Teljesítmény optimalizálás cache-eléssel
/// </summary>
public sealed class CachedHintProvider : IHintProvider
{
    private readonly IHintProvider _inner;
    private readonly Dictionary<StateHash, HintRecommendation?> _cache;
    private readonly int _maxCacheSize;
    
    /// <summary>Cache találatok száma statisztikához</summary>
    public int CacheHits { get; private set; }
    
    /// <summary>Cache hiányok száma statisztikához</summary>
    public int CacheMisses { get; private set; }
    
    /// <summary>
    /// Cached Hint Provider létrehozása.
    /// </summary>
    /// <param name="inner">A belső hint provider, amit cache-elünk</param>
    /// <param name="maxCacheSize">Maximális cache méret (alapértelmezett: 1024)</param>
    public CachedHintProvider(IHintProvider inner, int maxCacheSize = 1024)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _cache = new Dictionary<StateHash, HintRecommendation?>(capacity: maxCacheSize);
        _maxCacheSize = maxCacheSize;
    }
    
    /// <inheritdoc/>
    public HintRecommendation? GetHint(SokobanGame state, HintOptions options)
    {
        if (!options.UseCache)
        {
            CacheMisses++;
            return _inner.GetHint(state, options);
        }
        
        var key = StateHash.Compute(state, options);
        
        if (_cache.TryGetValue(key, out var cached))
        {
            CacheHits++;
            return cached;
        }
        
        CacheMisses++;
        
        // Cache méret korlátozása - egyszerű stratégia: ha tele van, töröljük a felét
        if (_cache.Count >= _maxCacheSize)
        {
            var keysToRemove = _cache.Keys.Take(_maxCacheSize / 2).ToList();
            foreach (var k in keysToRemove)
            {
                _cache.Remove(k);
            }
        }
        
        var hint = _inner.GetHint(state, options);
        _cache[key] = hint;
        return hint;
    }
    
    /// <summary>
    /// Cache ürítése.
    /// </summary>
    public void ClearCache()
    {
        _cache.Clear();
        CacheHits = 0;
        CacheMisses = 0;
    }
    
    /// <summary>
    /// Cache méretének lekérése.
    /// </summary>
    public int CacheSize => _cache.Count;
}

/// <summary>
/// [Application Layer]
/// Alapértelmezett Hint Provider az AISolver-t használva.
/// 
/// A szakdolgozatban hivatkozható: Adapter pattern - AISolver becsomagolása IHintProvider interfészbe.
/// </summary>
public sealed class DefaultHintProvider : IHintProvider
{
    private readonly AISolver _solver;
    
    public DefaultHintProvider(AISolver? solver = null)
    {
        _solver = solver ?? new AISolver();
    }
    
    /// <inheritdoc/>
    public HintRecommendation? GetHint(SokobanGame state, HintOptions options)
    {
        var nextMove = _solver.GetNextMove(state);
        
        if (nextMove == null)
        {
            return null;
        }
        
        var (move, totalMoves, pushCount) = nextMove.Value;
        
        if (move == null)
        {
            return null;
        }
        
        string explanation = options.Level switch
        {
            HintLevel.Soft => $"Próbálj {move.Direction.Name} irányba menni",
            HintLevel.Medium => move.Pushed
                ? $"Told a ládát {move.Direction.Name}ra"
                : $"Menj {move.Direction.Name}ra",
            HintLevel.Hard => move.Pushed
                ? $"Told a ládát {move.Direction.Name}ra ({move.Direction.Arrow})! Még {totalMoves} lépés a megoldásig."
                : $"Menj {move.Direction.Name}ra ({move.Direction.Arrow})! Még {totalMoves} lépés a megoldásig.",
            _ => null
        };
        
        return new HintRecommendation
        {
            Direction = move.Direction,
            IsPush = move.Pushed,
            RemainingMoves = totalMoves,
            RemainingPushes = pushCount,
            Quality = 1.0,
            Explanation = explanation
        };
    }
}

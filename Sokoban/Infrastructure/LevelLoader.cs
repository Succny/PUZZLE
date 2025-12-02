namespace Sokoban.Infrastructure;

using Sokoban.Domain;

// ============================================================================
// INFRASTRUCTURE LAYER / PÁLYA BETÖLTÉS
// Ez a fájl a pálya betöltő implementációt tartalmazza.
// A szakdolgozatban hivatkozható: Repository pattern, adatbetöltés.
// ============================================================================

/// <summary>
/// [Infrastructure Layer]
/// Alapértelmezett pálya betöltő - a beépített pályákat szolgáltatja.
/// 
/// A szakdolgozatban hivatkozható:
/// - Repository pattern implementáció
/// - Pálya adatok központi kezelése
/// </summary>
public sealed class DefaultLevelLoader : ILevelLoader
{
    private readonly Level[] _levels;
    
    public DefaultLevelLoader()
    {
        _levels = Levels.AllLevels;
    }
    
    /// <inheritdoc/>
    public IReadOnlyList<Level> LoadAllLevels() => _levels;
    
    /// <inheritdoc/>
    public Level? LoadLevel(int index)
    {
        if (index >= 0 && index < _levels.Length)
        {
            return _levels[index];
        }
        return null;
    }
    
    /// <inheritdoc/>
    public Level? LoadLevelByName(string name)
    {
        return _levels.FirstOrDefault(l => 
            l.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// [Infrastructure Layer]
/// Egyszerű állapot szerializáló - mentés/betöltés támogatáshoz.
/// </summary>
public sealed class SimpleStateSerializer : IStateSerializer
{
    /// <inheritdoc/>
    public string Serialize(SokobanGame game)
    {
        var lines = new List<string>
        {
            $"MOVES:{game.Moves}",
            $"PUSHES:{game.Pushes}",
            $"LEVEL:{game.CurrentLevel.Name}",
            "MAP:"
        };
        
        var map = game.CloneMap();
        for (int row = 0; row < game.Height; row++)
        {
            var line = "";
            for (int col = 0; col < game.Width; col++)
            {
                line += map[row, col];
            }
            lines.Add(line);
        }
        
        return string.Join("\n", lines);
    }
    
    /// <inheritdoc/>
    public SokobanGame? Deserialize(string data, Level level)
    {
        // Egyszerűsített implementáció - újraindítja a pályát
        // A teljes mentés/betöltés további fejlesztést igényel
        return new SokobanGame(level);
    }
}

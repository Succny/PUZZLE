namespace Sokoban.Infrastructure;

using System.Text.Json;
using Sokoban.Domain;

// ============================================================================
// INFRASTRUCTURE LAYER / LOKALIZÁCIÓ
// Ez a fájl a lokalizációs rendszert tartalmazza.
// A szakdolgozatban hivatkozható: Többnyelvű támogatás, i18n.
// ============================================================================

/// <summary>
/// [Infrastructure Layer]
/// JSON alapú lokalizációs szolgáltatás.
/// Támogatja a magyar (hu) és angol (en) nyelveket.
/// 
/// A szakdolgozatban hivatkozható:
/// - Lokalizációs rendszer implementáció
/// - Kulcs-alapú szövegkezelés
/// </summary>
public sealed class JsonLocalization : ILocalization
{
    private readonly Dictionary<string, Dictionary<string, string>> _translations;
    private string _currentLanguage;
    
    /// <summary>Elérhető nyelvek</summary>
    public static readonly string[] SupportedLanguages = { "hu", "en" };
    
    /// <summary>Alapértelmezett nyelv</summary>
    public const string DefaultLanguage = "hu";
    
    public JsonLocalization(string language = DefaultLanguage)
    {
        _translations = new Dictionary<string, Dictionary<string, string>>();
        _currentLanguage = language;
        LoadDefaultTranslations();
    }
    
    /// <inheritdoc/>
    public string CurrentLanguage => _currentLanguage;
    
    /// <inheritdoc/>
    public void SetLanguage(string languageCode)
    {
        if (SupportedLanguages.Contains(languageCode))
        {
            _currentLanguage = languageCode;
        }
    }
    
    /// <inheritdoc/>
    public string Get(string key, params object[] args)
    {
        if (_translations.TryGetValue(_currentLanguage, out var langDict) &&
            langDict.TryGetValue(key, out var translation))
        {
            return args.Length > 0 ? string.Format(translation, args) : translation;
        }
        
        // Fallback az alapértelmezett nyelvre
        if (_currentLanguage != DefaultLanguage &&
            _translations.TryGetValue(DefaultLanguage, out var defaultDict) &&
            defaultDict.TryGetValue(key, out var defaultTranslation))
        {
            return args.Length > 0 ? string.Format(defaultTranslation, args) : defaultTranslation;
        }
        
        // Ha nincs fordítás, visszaadjuk a kulcsot
        return key;
    }
    
    /// <summary>
    /// Fordítások betöltése JSON stringből.
    /// </summary>
    public void LoadFromJson(string languageCode, string jsonContent)
    {
        var translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent);
        if (translations != null)
        {
            _translations[languageCode] = translations;
        }
    }
    
    /// <summary>
    /// Alapértelmezett fordítások betöltése (beépített).
    /// </summary>
    private void LoadDefaultTranslations()
    {
        // Magyar fordítások
        _translations["hu"] = new Dictionary<string, string>
        {
            // Hint üzenetek
            ["hint.move.direction"] = "Menj {0}ra ({1})!",
            ["hint.push.direction"] = "Told a ládát {0}ra ({1})!",
            ["hint.remaining"] = "Még {0} lépés, {1} tolás a megoldásig",
            ["hint.solved"] = "🎉 Gratulálok! A pálya már teljesítve van!",
            ["hint.solved.next"] = "Nyomd meg a '{0}' gombot a következő pályához!",
            ["hint.solved.last"] = "🏆 Ez volt az utolsó pálya! Gratulálok!",
            ["hint.no_solution.timeout"] = "🤔 Jelenleg nem találok megoldást (időkorlát).",
            ["hint.no_solution.exhausted"] = "⚠️ Úgy tűnik, ebből az állapotból nincs megoldás.",
            ["hint.try_undo"] = "Használd az 'U' billentyűt a visszalépéshez!",
            
            // Állapot elemzés
            ["analysis.header"] = "📊 Állapot elemzés:",
            ["analysis.boxes"] = "• {0}/{1} láda a célhelyen ({2}%)",
            ["analysis.moves"] = "• Eddigi lépések: {0}",
            ["analysis.pushes"] = "• Eddigi tolások: {0}",
            ["analysis.solvable"] = "✅ A pálya megoldható!",
            ["analysis.remaining"] = "Hátralévő lépések: ~{0}",
            ["analysis.timeout"] = "⏱️ A keresés időkorlátba ütközött.",
            ["analysis.exhausted"] = "⚠️ Valószínűleg nincs megoldás ebből az állapotból.",
            
            // Deadlock üzenetek
            ["deadlock.corner"] = "⚠️ Vigyázz! Egy láda sarokba szorult!",
            ["deadlock.wall"] = "⚠️ Ez a láda már nem mozdítható a célhelyre!",
            ["deadlock.general"] = "⚠️ Deadlock! Használd az 'U' billentyűt a visszalépéshez!",
            
            // Bátorítás
            ["encourage.good"] = "Szuper, jó úton haladsz! 👍",
            ["encourage.great"] = "Remek lépés volt!",
            ["encourage.continue"] = "Folytatsd így!",
            ["encourage.well_done"] = "Nagyon jól csinálod!",
            ["encourage.box_placed"] = "Egy láda már a helyén! ✅",
            
            // Győzelem
            ["victory.congrats"] = "🎉 Fantasztikus! Teljesítetted a pályát!",
            ["victory.trophy"] = "🏆 Gratulálok a győzelemhez!",
            ["victory.star"] = "⭐ Kiváló munka!",
            
            // Stratégiai tippek
            ["tip.think_first"] = "Először gondold végig, melyik ládát mozdítsd!",
            ["tip.corner_warning"] = "A sarokban lévő ládákat nehéz kimozdítani.",
            ["tip.wall_push"] = "Próbáld a ládákat a fal mentén a célhelyek felé tolni.",
            ["tip.step_back"] = "Néha vissza kell lépni, hogy előre juss.",
            ["tip.order_matters"] = "A ládák sorrendje is számít!",
            ["tip.avoid_corner"] = "Vigyázz, hogy ne told sarokba a ládát!",
            
            // UI elemek
            ["ui.level"] = "{0}. pálya: \"{1}\" ({2})",
            ["ui.instructions"] = "Told a ládákat ($) a célhelyekre (.)!",
            ["ui.undo_success"] = "↩️ Visszalépés sikeres!",
            ["ui.undo_failed"] = "Nincs több visszalépési lehetőség.",
            ["ui.restart"] = "🔄 Pálya újraindítva!",
            ["ui.goodbye"] = "Köszönjük, hogy játszottál! 👋",
            
            // Statisztikák
            ["stats.moves"] = "Lépések: {0}",
            ["stats.pushes"] = "Tolások: {0}",
            ["stats.time"] = "Idő: {0}",
            ["stats.boxes"] = "Ládák: {0}/{1}"
        };
        
        // Angol fordítások
        _translations["en"] = new Dictionary<string, string>
        {
            // Hint messages
            ["hint.move.direction"] = "Move {0} ({1})!",
            ["hint.push.direction"] = "Push the box {0} ({1})!",
            ["hint.remaining"] = "{0} moves, {1} pushes remaining to solve",
            ["hint.solved"] = "🎉 Congratulations! The level is already completed!",
            ["hint.solved.next"] = "Press '{0}' for the next level!",
            ["hint.solved.last"] = "🏆 This was the last level! Congratulations!",
            ["hint.no_solution.timeout"] = "🤔 Could not find a solution (timeout).",
            ["hint.no_solution.exhausted"] = "⚠️ It seems there's no solution from this state.",
            ["hint.try_undo"] = "Use the 'U' key to undo!",
            
            // State analysis
            ["analysis.header"] = "📊 State Analysis:",
            ["analysis.boxes"] = "• {0}/{1} boxes on goals ({2}%)",
            ["analysis.moves"] = "• Moves so far: {0}",
            ["analysis.pushes"] = "• Pushes so far: {0}",
            ["analysis.solvable"] = "✅ The level is solvable!",
            ["analysis.remaining"] = "Remaining moves: ~{0}",
            ["analysis.timeout"] = "⏱️ Search timed out.",
            ["analysis.exhausted"] = "⚠️ Probably no solution from this state.",
            
            // Deadlock messages
            ["deadlock.corner"] = "⚠️ Warning! A box is stuck in a corner!",
            ["deadlock.wall"] = "⚠️ This box can no longer reach a goal!",
            ["deadlock.general"] = "⚠️ Deadlock! Use 'U' to undo!",
            
            // Encouragement
            ["encourage.good"] = "Great, you're on the right track! 👍",
            ["encourage.great"] = "That was a great move!",
            ["encourage.continue"] = "Keep it up!",
            ["encourage.well_done"] = "You're doing great!",
            ["encourage.box_placed"] = "One box is in place! ✅",
            
            // Victory
            ["victory.congrats"] = "🎉 Fantastic! You completed the level!",
            ["victory.trophy"] = "🏆 Congratulations on your victory!",
            ["victory.star"] = "⭐ Excellent work!",
            
            // Strategy tips
            ["tip.think_first"] = "Think about which box to move first!",
            ["tip.corner_warning"] = "Boxes in corners are hard to move out.",
            ["tip.wall_push"] = "Try pushing boxes along walls toward goals.",
            ["tip.step_back"] = "Sometimes you need to step back to move forward.",
            ["tip.order_matters"] = "The order of boxes matters!",
            ["tip.avoid_corner"] = "Be careful not to push boxes into corners!",
            
            // UI elements
            ["ui.level"] = "Level {0}: \"{1}\" ({2})",
            ["ui.instructions"] = "Push the boxes ($) to the goals (.)!",
            ["ui.undo_success"] = "↩️ Undo successful!",
            ["ui.undo_failed"] = "No more undo available.",
            ["ui.restart"] = "🔄 Level restarted!",
            ["ui.goodbye"] = "Thanks for playing! 👋",
            
            // Statistics
            ["stats.moves"] = "Moves: {0}",
            ["stats.pushes"] = "Pushes: {0}",
            ["stats.time"] = "Time: {0}",
            ["stats.boxes"] = "Boxes: {0}/{1}"
        };
    }
}

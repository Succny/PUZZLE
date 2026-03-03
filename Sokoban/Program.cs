namespace Sokoban;

using System.Text;

// ============================================================================
// UI LAYER / PREZENTÁCIÓS RÉTEG - BELÉPÉSI PONT
// Ez a fájl az alkalmazás belépési pontja.
// A szakdolgozatban hivatkozható: Alkalmazás indítás, UI réteg.
// ============================================================================

/// <summary>
/// [UI Layer]
/// SOKOBAN - Kooperatív AI Puzzle Játék
/// BSc Szakdolgozat - Mesterséges Intelligencia és Ember Együttműködése
/// 
/// C# Console Application belépési pont.
/// Inicializálja a konzol beállításokat és indítja a játék UI-t.
/// </summary>
class Program
{
    /// <summary>
    /// UI keretrendszer fix magassága karaktercellákban.
    /// Összetevők:
    /// - Header: 4 sor
    /// - Level selector: 2 sor
    /// - Game area: 10 sor (minimum)
    /// - Stats: 2 sor
    /// - AI panel: 3 sor
    /// - Message panel: 4 sor
    /// - Controls: 7 sor
    /// Összesen: 32 sor
    /// </summary>
    private const int UiHeight = 32;

    static void Main(string[] args)
    {
        try
        {
            // UTF-8 kódolás beállítása az emoji és speciális karakterekhez
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "SOKOBAN - AI Hint Rendszerrel";

            // Fix méretű konzol beállítása a stabil megjelenítéshez
            // A ConsoleSizing osztály gondoskodik a buffer és ablak szinkronizálásáról
            ConsoleSizing.ApplyFixedSize(ConsoleUI.UiWidth, UiHeight);
        }
        catch
        {
            // Ha valami miatt nem sikerül (pl. IDE host, átirányított kimenet), 
            // ne álljon le az alkalmazás
        }

        try
        {
            var ui = new ConsoleUI();
            ui.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hiba történt: {ex.Message}");
            Console.WriteLine("Nyomj egy billentyűt a kilépéshez...");
            Console.ReadKey();
        }
    }
}

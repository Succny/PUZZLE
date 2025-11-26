namespace Sokoban;

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
    static void Main(string[] args)
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Title = "SOKOBAN - AI Hint Rendszerrel";

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

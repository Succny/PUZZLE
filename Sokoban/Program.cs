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
    /// Minimális konzol szélesség a UI megfelelő megjelenítéséhez
    /// </summary>
    private const int MinConsoleWidth = 80;

    /// <summary>
    /// Minimális konzol magasság a UI megfelelő megjelenítéséhez
    /// </summary>
    private const int MinConsoleHeight = 35;

    static void Main(string[] args)
    {
        try
        {
            // UTF-8 kódolás beállítása az emoji és speciális karakterekhez
            Console.OutputEncoding = Encoding.UTF8;
            Console.Title = "SOKOBAN - AI Hint Rendszerrel";

            // Konzol méret beállítása, ha lehetséges
            SetupConsoleSize();
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

    /// <summary>
    /// Konzol ablak méretének beállítása, ha a platform támogatja.
    /// </summary>
    private static void SetupConsoleSize()
    {
        try
        {
            // Ellenőrizzük, hogy van-e konzol ablak (nem átirányított kimenet)
            if (Console.IsOutputRedirected || Console.IsInputRedirected)
            {
                return;
            }

            // Platformfüggő méretbeállítás
            if (OperatingSystem.IsWindows())
            {
                SetupWindowsConsoleSize();
            }
            // Linux/macOS esetén általában nem kell méretet állítani,
            // a terminál emulátor kezeli
        }
        catch
        {
            // Ha nem sikerül, nem probléma - az alkalmazás fut tovább
        }
    }

    /// <summary>
    /// Windows-specifikus konzol méret beállítás.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void SetupWindowsConsoleSize()
    {
        try
        {
            // Csak Windows-on működik a SetWindowSize
            int availableWidth = Console.LargestWindowWidth;
            int availableHeight = Console.LargestWindowHeight;

            int targetWidth = Math.Min(MinConsoleWidth, availableWidth);
            int targetHeight = Math.Min(MinConsoleHeight, availableHeight);

            // Először a buffer méretet kell beállítani, ha szükséges
            if (Console.BufferWidth < targetWidth)
            {
                Console.BufferWidth = targetWidth;
            }
            if (Console.BufferHeight < targetHeight)
            {
                Console.BufferHeight = targetHeight;
            }

            // Ablak méret beállítása
            if (availableWidth >= MinConsoleWidth && availableHeight >= MinConsoleHeight)
            {
                Console.SetWindowSize(targetWidth, targetHeight);
            }
        }
        catch
        {
            // Windows Terminal és néhány konzol nem támogatja ezeket a műveleteket
        }
    }
}

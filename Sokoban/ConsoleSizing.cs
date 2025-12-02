using System;

namespace Sokoban;

// ============================================================================
// UI LAYER / PREZENTÁCIÓS RÉTEG - KONZOL MÉRETEZÉS
// Ez a fájl a konzol ablak/buffer méretezéséért felelős.
// A szakdolgozatban hivatkozható: Konzol stabilizáció.
// ============================================================================

/// <summary>
/// [UI Layer]
/// Konzol méretezés segédosztály.
/// 
/// Felelősségek:
/// - Konzol buffer és ablak méret szinkronizálása
/// - "Ugrálás" megakadályozása Windows Terminal/PowerShell alatt
/// - Fix méretű megjelenítés biztosítása
/// 
/// A szakdolgozatban hivatkozható:
/// - Konzol méret stabilizáció
/// - Buffer/ablak szinkronizáció
/// </summary>
public static class ConsoleSizing
{
    /// <summary>
    /// Fix méretű konzol beállítása a pályaméret és HUD alapján.
    /// 
    /// A metódus biztosítja, hogy:
    /// 1. A buffer méret pontosan megegyezik az ablak mérettel (nincs scrollbar)
    /// 2. A konzol ablak mérete megfelelő a játéktér megjelenítéséhez
    /// 3. A kurzor el van rejtve
    /// 
    /// FONTOS: Előbb a buffer méretet kell beállítani, aztán az ablak méretet,
    /// különben kivétel keletkezhet.
    /// </summary>
    /// <param name="playfieldWidth">A pálya szélessége karaktercellákban</param>
    /// <param name="playfieldHeight">A pálya magassága karaktercellákban</param>
    /// <param name="hudExtraWidth">HUD/keretek extra szélessége</param>
    /// <param name="hudExtraHeight">HUD/keretek extra magassága</param>
    /// <returns>True, ha sikerült beállítani a méretet</returns>
    public static bool ApplyFixedSize(int playfieldWidth, int playfieldHeight, int hudExtraWidth = 0, int hudExtraHeight = 0)
    {
        // Teljes terület: pálya + HUD/keretek ha vannak
        int totalWidth = Math.Max(1, playfieldWidth + hudExtraWidth);
        int totalHeight = Math.Max(1, playfieldHeight + hudExtraHeight);

        try
        {
            // Ellenőrizzük, hogy van-e konzol ablak (nem átirányított kimenet)
            if (Console.IsOutputRedirected || Console.IsInputRedirected)
            {
                return false;
            }

            // Platformfüggő méretbeállítás
            if (OperatingSystem.IsWindows())
            {
                return ApplyWindowsFixedSize(totalWidth, totalHeight);
            }

            // Linux/macOS esetén általában nem kell méretet állítani
            return true;
        }
        catch
        {
            // Ha nem sikerül, nem probléma - az alkalmazás fut tovább
            return false;
        }
    }

    /// <summary>
    /// Windows-specifikus fix méretű konzol beállítás.
    /// </summary>
    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static bool ApplyWindowsFixedSize(int totalWidth, int totalHeight)
    {
        try
        {
            // Lekérjük a maximálisan elérhető méretet
            int availableWidth = Console.LargestWindowWidth;
            int availableHeight = Console.LargestWindowHeight;

            // A célméret nem lehet nagyobb, mint az elérhető maximum
            int targetWidth = Math.Min(totalWidth, availableWidth);
            int targetHeight = Math.Min(totalHeight, availableHeight);

            // Biztonsági minimum
            targetWidth = Math.Max(1, targetWidth);
            targetHeight = Math.Max(1, targetHeight);

            // A buffernek legalább akkora kell lennie, mint az ablak
            // Előbb buffer -> aztán window, különben kivétel jöhet.
            // De ha az ablak nagyobb, mint a tervezett buffer, először zsugorítsuk az ablakot.
            try
            {
                // Előbb próbáljuk közvetlenül beállítani a buffer méretet
                Console.SetBufferSize(targetWidth, targetHeight);
            }
            catch
            {
                // Ha kisebb a window, előbb zsugorítsuk ablakot minimálisra, majd buffer, majd ablak
                int safeW = Math.Min(Console.WindowWidth, targetWidth);
                int safeH = Math.Min(Console.WindowHeight, targetHeight);
                safeW = Math.Max(1, safeW);
                safeH = Math.Max(1, safeH);
                
                Console.SetWindowSize(safeW, safeH);
                Console.SetBufferSize(targetWidth, targetHeight);
            }

            // Most az ablak mérete pontosan egyezzen a bufferrel
            Console.SetWindowSize(targetWidth, targetHeight);

            // Biztonsági beállítások a stabil rendereléshez
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);

            return true;
        }
        catch
        {
            // Windows Terminal és néhány konzol nem támogatja ezeket a műveleteket
            // Ilyenkor próbáljunk meg legalább a kurzort elrejteni
            try
            {
                Console.CursorVisible = false;
            }
            catch
            {
                // Ignoráljuk
            }
            return false;
        }
    }

    /// <summary>
    /// Újrainicializálja a kurzor pozícióját és elrejti azt.
    /// Hívd meg minden renderelés előtt a stabil megjelenítéshez.
    /// </summary>
    public static void ResetCursorForRender()
    {
        try
        {
            Console.CursorVisible = false;
            Console.SetCursorPosition(0, 0);
        }
        catch
        {
            // Ignoráljuk az esetleges hibákat
        }
    }
}

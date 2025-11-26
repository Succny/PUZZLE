namespace Sokoban;

/// <summary>
/// [UI Layer / Localization]
/// Statikus osztály a játékban használt üzenetek tárolására.
/// A szövegek központi kezelése megkönnyíti a lokalizációt és a karbantartást.
/// 
/// A szakdolgozatban hivatkozható: a játékos–AI interakció szöveges visszajelzéseinek
/// központi kezelése, amely lehetővé teszi a több nyelvre történő egyszerű kiterjesztést.
/// </summary>
public static class Messages
{
    /// <summary>
    /// Bátorító üzenetek sikeres lépés után.
    /// </summary>
    public static readonly string[] Encouragements = new[]
    {
        "Szuper, jó úton haladsz! 👍",
        "Remek lépés volt!",
        "Folytatsd így!",
        "Nagyon jól csinálod!",
        "Egy láda már a helyén! ✅"
    };

    /// <summary>
    /// Üzenetek, amikor a játékos elakadt.
    /// </summary>
    public static readonly string[] StuckMessages = new[]
    {
        "Úgy látom, elakadtál. Kérj egy hint-et!",
        "Ne aggódj, a Sokoban nehéz játék. Segítek!",
        "Próbáld meg a 'H' billentyűt a hint-ért!"
    };

    /// <summary>
    /// Figyelmeztetések deadlock (zsákutca) helyzetben.
    /// </summary>
    public static readonly string[] DeadlockMessages = new[]
    {
        "⚠️ Vigyázz! Egy láda zsákutcába került!",
        "⚠️ Ez a láda már nem mozdítható a célhelyre!",
        "⚠️ Deadlock! Használd az 'U' billentyűt a visszalépéshez!"
    };

    /// <summary>
    /// Gratulációs üzenetek pálya teljesítése után.
    /// </summary>
    public static readonly string[] SolvedMessages = new[]
    {
        "🎉 Fantasztikus! Teljesítetted a pályát!",
        "🏆 Gratulálok a győzelemhez!",
        "⭐ Kiváló munka!"
    };

    /// <summary>
    /// Stratégiai tippek a játékhoz.
    /// Az AI hint rendszer ezeket használja proaktív tanácsadásra.
    /// </summary>
    public static readonly string[] StrategyTips = new[]
    {
        "Először gondold végig, melyik ládát mozdítsd!",
        "A sarokban lévő ládákat nehéz kimozdítani.",
        "Próbáld a ládákat a fal mentén a célhelyek felé tolni.",
        "Néha vissza kell lépni, hogy előre juss.",
        "A ládák sorrendje is számít!",
        "Vigyázz, hogy ne told sarokba a ládát!"
    };
}

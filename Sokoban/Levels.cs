namespace Sokoban;

// ============================================================================
// CORE LAYER / JÁTÉKMOTOR RÉTEG
// Ez a fájl a pályadefiníciókat és csempe konstansokat tartalmazza.
// A szakdolgozatban hivatkozható: Core játéklogika réteg.
// ============================================================================

/// <summary>
/// [Core Layer]
/// Pálya elemek (csempék) konstansok.
/// A Sokoban pályák karakteres reprezentációjának alapja.
/// </summary>
public static class Tiles
{
    /// <summary>Fal - nem átjárható terület</summary>
    public const char Wall = '#';
    /// <summary>Padló - üres, járható terület</summary>
    public const char Floor = ' ';
    /// <summary>Célhely - ide kell tolni a ládákat</summary>
    public const char Goal = '.';
    /// <summary>Láda - a játékos által tolható objektum</summary>
    public const char Box = '$';
    /// <summary>Játékos - a raktáros karakter</summary>
    public const char Player = '@';
    /// <summary>Játékos célhelyen</summary>
    public const char PlayerOnGoal = '+';
    /// <summary>Láda célhelyen - cél állapot</summary>
    public const char BoxOnGoal = '*';
}

/// <summary>
/// [Core Layer]
/// Pálya definíció osztály.
/// Egy pályát ír le: név, nehézségi szint, és a térkép.
/// </summary>
public class Level(string name, string difficulty, string[] map)
{
    /// <summary>A pálya megnevezése</summary>
    public string Name { get; } = name;
    /// <summary>Nehézségi szint (pl. "Könnyű", "Közepes", "Nehéz")</summary>
    public string Difficulty { get; } = difficulty;
    /// <summary>A pálya karakteres reprezentációja (soronként)</summary>
    public string[] Map { get; } = map;
}

/// <summary>
/// [Core Layer]
/// Előre definiált pályák gyűjteménye.
/// A játékhoz tartozó beépített pályák tömbje.
/// </summary>
public static class Levels
{
    public static readonly Level[] AllLevels =
    [
        // Level 1 - Tutorial (nagyon könnyű)
        new Level("Első lépések", "Könnyű",
        [
            "#######",
            "#     #",
            "# .$@ #",
            "#     #",
            "#######"
        ]),

        // Level 2 - Két láda
        new Level("Kettős kihívás", "Könnyű",
        [
            "########",
            "#      #",
            "# $  $ #",
            "#  ##  #",
            "# .  . #",
            "#  @   #",
            "########"
        ]),

        // Level 3 - Közepes
        new Level("Szűk folyosó", "Közepes",
        [
            "  #####",
            "###   #",
            "# $ # #",
            "# # . #",
            "# @ $ #",
            "###.  #",
            "  #####"
        ]),

        // Level 4 - Nehezebb
        new Level("Labirintus", "Közepes",
        [
            "########",
            "#  #   #",
            "# $$ @ #",
            "# .#.  #",
            "#  # $ #",
            "#  . ###",
            "########"
        ]),

        // Level 5 - Kihívás (klasszikus megoldható pálya)
        new Level("Mester próba", "Nehéz",
        [
            "  #####",
            "###   #",
            "#.@$  #",
            "### $.#",
            "#.##$ #",
            "# # . ##",
            "#$  $$.#",
            "#   .  #",
            "########"
        ])
    ];
}

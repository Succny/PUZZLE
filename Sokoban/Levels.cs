namespace Sokoban;

/// <summary>
/// Pálya elemek konstansok
/// </summary>
public static class Tiles
{
    public const char Wall = '#';
    public const char Floor = ' ';
    public const char Goal = '.';
    public const char Box = '$';
    public const char Player = '@';
    public const char PlayerOnGoal = '+';
    public const char BoxOnGoal = '*';
}

/// <summary>
/// Pálya definíció
/// </summary>
public class Level
{
    public string Name { get; }
    public string Difficulty { get; }
    public string[] Map { get; }

    public Level(string name, string difficulty, string[] map)
    {
        Name = name;
        Difficulty = difficulty;
        Map = map;
    }
}

/// <summary>
/// Előre definiált pályák
/// </summary>
public static class Levels
{
    public static readonly Level[] AllLevels = new Level[]
    {
        // Level 1 - Tutorial (nagyon könnyű)
        new Level("Első lépések", "Könnyű", new string[]
        {
            "#######",
            "#     #",
            "# .$@ #",
            "#     #",
            "#######"
        }),

        // Level 2 - Két láda
        new Level("Kettős kihívás", "Könnyű", new string[]
        {
            "########",
            "#      #",
            "# $  $ #",
            "#  ##  #",
            "# .  . #",
            "#  @   #",
            "########"
        }),

        // Level 3 - Közepes
        new Level("Szűk folyosó", "Közepes", new string[]
        {
            "  #####",
            "###   #",
            "# $ # #",
            "# # . #",
            "# @ $ #",
            "###.  #",
            "  #####"
        }),

        // Level 4 - Nehezebb
        new Level("Labirintus", "Közepes", new string[]
        {
            "########",
            "#  #   #",
            "# $$ @ #",
            "# .#.  #",
            "#  # $ #",
            "#  . ###",
            "########"
        }),

        // Level 5 - Kihívás
        new Level("Mester próba", "Nehéz", new string[]
        {
            " ######",
            "##    #",
            "# $ $ #",
            "# #.# #",
            "# $ . #",
            "# .#  #",
            "#@ .$ #",
            "#######"
        })
    };
}

/**
 * SOKOBAN - Pályák (Levels)
 * 
 * Pálya formátum:
 * # - Fal
 * . - Célhely
 * $ - Láda
 * @ - Játékos
 * + - Játékos célhelyen
 * * - Láda célhelyen
 *   - Üres padló
 */

const LEVELS = [
    // Level 1 - Tutorial (nagyon könnyű)
    {
        name: "Első lépések",
        difficulty: "Könnyű",
        map: [
            "#######",
            "#     #",
            "# .$@ #",
            "#     #",
            "#######"
        ]
    },

    // Level 2 - Két láda
    {
        name: "Kettős kihívás",
        difficulty: "Könnyű",
        map: [
            "########",
            "#      #",
            "# $  $ #",
            "#  ##  #",
            "# .  . #",
            "#  @   #",
            "########"
        ]
    },

    // Level 3 - Közepes
    {
        name: "Szűk folyosó",
        difficulty: "Közepes",
        map: [
            "  #####",
            "###   #",
            "# $ # #",
            "# # . #",
            "# @ $ #",
            "###.  #",
            "  #####"
        ]
    },

    // Level 4 - Nehezebb
    {
        name: "Labirintus",
        difficulty: "Közepes",
        map: [
            "########",
            "#  #   #",
            "# $$ @ #",
            "# .#.  #",
            "#  # $ #",
            "#  . ###",
            "########"
        ]
    },

    // Level 5 - Kihívás
    {
        name: "Mester próba",
        difficulty: "Nehéz",
        map: [
            " ######",
            "##    #",
            "# $ $ #",
            "# #.# #",
            "# $ . #",
            "# .#  #",
            "#@ .$ #",
            "#######"
        ]
    }
];

// Pálya elemek konstansok
const TILES = {
    WALL: '#',
    FLOOR: ' ',
    GOAL: '.',
    BOX: '$',
    PLAYER: '@',
    PLAYER_ON_GOAL: '+',
    BOX_ON_GOAL: '*'
};

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { LEVELS, TILES };
}

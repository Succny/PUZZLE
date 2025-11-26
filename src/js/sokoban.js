/**
 * SOKOBAN - Játék Logika
 * 
 * Ez a modul kezeli a Sokoban alapvető logikáját:
 * - Játékállapot kezelés
 * - Játékos és láda mozgatás
 * - Megoldás ellenőrzés
 * - Undo funkció
 */

class Sokoban {
    /**
     * Új Sokoban játék létrehozása
     * @param {Object} level - A pálya objektum
     */
    constructor(level) {
        this.originalMap = level.map.map(row => row.split(''));
        this.map = null;
        this.playerPos = { row: 0, col: 0 };
        this.moves = 0;
        this.pushes = 0;
        this.history = [];
        this.width = 0;
        this.height = 0;
        this.init();
    }

    /**
     * Játék inicializálása
     */
    init() {
        // Mély másolat a térképről
        this.map = this.originalMap.map(row => [...row]);
        this.height = this.map.length;
        this.width = Math.max(...this.map.map(row => row.length));
        
        // Normalizálás: minden sor azonos hosszúságú legyen
        this.map = this.map.map(row => {
            while (row.length < this.width) {
                row.push(' ');
            }
            return row;
        });

        // Játékos pozíciójának megtalálása
        for (let row = 0; row < this.height; row++) {
            for (let col = 0; col < this.width; col++) {
                const tile = this.map[row][col];
                if (tile === TILES.PLAYER || tile === TILES.PLAYER_ON_GOAL) {
                    this.playerPos = { row, col };
                }
            }
        }

        this.moves = 0;
        this.pushes = 0;
        this.history = [];
    }

    /**
     * Pálya újraindítása
     */
    restart() {
        this.init();
    }

    /**
     * Elem lekérése adott pozíción
     * @param {number} row - Sor
     * @param {number} col - Oszlop
     * @returns {string}
     */
    getTile(row, col) {
        if (row < 0 || row >= this.height || col < 0 || col >= this.width) {
            return TILES.WALL;
        }
        return this.map[row][col];
    }

    /**
     * Ellenőrzi, hogy a pozíció fal-e
     * @param {number} row - Sor
     * @param {number} col - Oszlop
     * @returns {boolean}
     */
    isWall(row, col) {
        return this.getTile(row, col) === TILES.WALL;
    }

    /**
     * Ellenőrzi, hogy a pozíción láda van-e
     * @param {number} row - Sor
     * @param {number} col - Oszlop
     * @returns {boolean}
     */
    isBox(row, col) {
        const tile = this.getTile(row, col);
        return tile === TILES.BOX || tile === TILES.BOX_ON_GOAL;
    }

    /**
     * Ellenőrzi, hogy a pozíció célhely-e
     * @param {number} row - Sor
     * @param {number} col - Oszlop
     * @returns {boolean}
     */
    isGoal(row, col) {
        const tile = this.getTile(row, col);
        return tile === TILES.GOAL || tile === TILES.PLAYER_ON_GOAL || tile === TILES.BOX_ON_GOAL;
    }

    /**
     * Ellenőrzi, hogy a pozíció szabad-e (lehet oda lépni)
     * @param {number} row - Sor
     * @param {number} col - Oszlop
     * @returns {boolean}
     */
    isFree(row, col) {
        const tile = this.getTile(row, col);
        return tile === TILES.FLOOR || tile === TILES.GOAL;
    }

    /**
     * Játékos mozgatása
     * @param {number} dRow - Sor irány (-1, 0, 1)
     * @param {number} dCol - Oszlop irány (-1, 0, 1)
     * @returns {Object} - Mozgatás eredménye
     */
    move(dRow, dCol) {
        const newRow = this.playerPos.row + dRow;
        const newCol = this.playerPos.col + dCol;

        // Fal ellenőrzése
        if (this.isWall(newRow, newCol)) {
            return { success: false, reason: 'wall' };
        }

        // Láda ellenőrzése
        if (this.isBox(newRow, newCol)) {
            const boxNewRow = newRow + dRow;
            const boxNewCol = newCol + dCol;

            // Láda mögötti pozíció ellenőrzése
            if (this.isWall(boxNewRow, boxNewCol) || this.isBox(boxNewRow, boxNewCol)) {
                return { success: false, reason: 'blocked' };
            }

            // Állapot mentése undo-hoz
            this.saveState();

            // Láda mozgatása
            this.moveBox(newRow, newCol, boxNewRow, boxNewCol);
            
            // Játékos mozgatása
            this.movePlayer(newRow, newCol);

            this.moves++;
            this.pushes++;

            return { 
                success: true, 
                pushed: true, 
                solved: this.isSolved(),
                deadlock: this.checkDeadlock(boxNewRow, boxNewCol)
            };
        }

        // Szabad mozgás
        this.saveState();
        this.movePlayer(newRow, newCol);
        this.moves++;

        return { success: true, pushed: false, solved: this.isSolved() };
    }

    /**
     * Játékos mozgatása térképen
     * @param {number} newRow - Új sor
     * @param {number} newCol - Új oszlop
     */
    movePlayer(newRow, newCol) {
        const oldRow = this.playerPos.row;
        const oldCol = this.playerPos.col;

        // Régi pozíció frissítése
        if (this.map[oldRow][oldCol] === TILES.PLAYER_ON_GOAL) {
            this.map[oldRow][oldCol] = TILES.GOAL;
        } else {
            this.map[oldRow][oldCol] = TILES.FLOOR;
        }

        // Új pozíció beállítása
        if (this.map[newRow][newCol] === TILES.GOAL) {
            this.map[newRow][newCol] = TILES.PLAYER_ON_GOAL;
        } else {
            this.map[newRow][newCol] = TILES.PLAYER;
        }

        this.playerPos = { row: newRow, col: newCol };
    }

    /**
     * Láda mozgatása térképen
     * @param {number} fromRow - Eredeti sor
     * @param {number} fromCol - Eredeti oszlop
     * @param {number} toRow - Cél sor
     * @param {number} toCol - Cél oszlop
     */
    moveBox(fromRow, fromCol, toRow, toCol) {
        // Eredeti pozíció frissítése
        if (this.map[fromRow][fromCol] === TILES.BOX_ON_GOAL) {
            this.map[fromRow][fromCol] = TILES.GOAL;
        } else {
            this.map[fromRow][fromCol] = TILES.FLOOR;
        }

        // Új pozíció beállítása
        if (this.map[toRow][toCol] === TILES.GOAL) {
            this.map[toRow][toCol] = TILES.BOX_ON_GOAL;
        } else {
            this.map[toRow][toCol] = TILES.BOX;
        }
    }

    /**
     * Állapot mentése undo-hoz
     */
    saveState() {
        this.history.push({
            map: this.map.map(row => [...row]),
            playerPos: { ...this.playerPos },
            moves: this.moves,
            pushes: this.pushes
        });

        // Maximum 1000 lépés tárolása
        if (this.history.length > 1000) {
            this.history.shift();
        }
    }

    /**
     * Visszalépés (undo)
     * @returns {boolean} - Sikeres volt-e
     */
    undo() {
        if (this.history.length === 0) {
            return false;
        }

        const state = this.history.pop();
        this.map = state.map;
        this.playerPos = state.playerPos;
        this.moves = state.moves;
        this.pushes = state.pushes;
        return true;
    }

    /**
     * Ellenőrzi, hogy a játék megoldott-e
     * @returns {boolean}
     */
    isSolved() {
        for (let row = 0; row < this.height; row++) {
            for (let col = 0; col < this.width; col++) {
                // Ha van láda, ami nincs célhelyen
                if (this.map[row][col] === TILES.BOX) {
                    return false;
                }
            }
        }
        return true;
    }

    /**
     * Deadlock (zsákutca) ellenőrzés
     * @param {number} boxRow - Láda sora
     * @param {number} boxCol - Láda oszlopa
     * @returns {boolean}
     */
    checkDeadlock(boxRow, boxCol) {
        // Ha a láda célhelyen van, nem deadlock
        if (this.isGoal(boxRow, boxCol)) {
            return false;
        }

        // Sarok deadlock: ha a láda sarokba szorult
        const up = this.isWall(boxRow - 1, boxCol);
        const down = this.isWall(boxRow + 1, boxCol);
        const left = this.isWall(boxRow, boxCol - 1);
        const right = this.isWall(boxRow, boxCol + 1);

        // Sarok pozíciók
        if ((up && left) || (up && right) || (down && left) || (down && right)) {
            return true;
        }

        return false;
    }

    /**
     * Játékállapot lekérése (AI-hoz)
     * @returns {Object}
     */
    getState() {
        const boxes = [];
        const goals = [];

        for (let row = 0; row < this.height; row++) {
            for (let col = 0; col < this.width; col++) {
                const tile = this.map[row][col];
                if (tile === TILES.BOX || tile === TILES.BOX_ON_GOAL) {
                    boxes.push({ row, col, onGoal: tile === TILES.BOX_ON_GOAL });
                }
                if (tile === TILES.GOAL || tile === TILES.PLAYER_ON_GOAL || tile === TILES.BOX_ON_GOAL) {
                    goals.push({ row, col });
                }
            }
        }

        return {
            map: this.map.map(row => [...row]),
            playerPos: { ...this.playerPos },
            boxes,
            goals,
            width: this.width,
            height: this.height,
            moves: this.moves,
            pushes: this.pushes
        };
    }

    /**
     * Állapot beállítása (klónozáshoz)
     * @param {Object} state - Állapot objektum
     */
    setState(state) {
        this.map = state.map.map(row => [...row]);
        this.playerPos = { ...state.playerPos };
        this.width = state.width;
        this.height = state.height;
    }

    /**
     * Klón létrehozása
     * @returns {Sokoban}
     */
    clone() {
        const clone = Object.create(Sokoban.prototype);
        clone.map = this.map.map(row => [...row]);
        clone.playerPos = { ...this.playerPos };
        clone.width = this.width;
        clone.height = this.height;
        clone.moves = 0;
        clone.pushes = 0;
        clone.history = [];
        clone.originalMap = this.originalMap;
        return clone;
    }

    /**
     * Láda pozíciók lekérése (rendezett)
     * @returns {string}
     */
    getBoxesKey() {
        const boxes = [];
        for (let row = 0; row < this.height; row++) {
            for (let col = 0; col < this.width; col++) {
                const tile = this.map[row][col];
                if (tile === TILES.BOX || tile === TILES.BOX_ON_GOAL) {
                    boxes.push(`${row},${col}`);
                }
            }
        }
        return boxes.sort().join('|');
    }

    /**
     * Egyedi állapot kulcs generálása
     * @returns {string}
     */
    getStateKey() {
        return `${this.playerPos.row},${this.playerPos.col}|${this.getBoxesKey()}`;
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = Sokoban;
}

/**
 * PUZZLE - Csúszó Puzzle Logika
 * 
 * Ez a modul kezeli a puzzle alapvető logikáját:
 * - Puzzle állapot kezelés
 * - Csempe mozgatás
 * - Megoldás ellenőrzés
 */

class SlidingPuzzle {
    /**
     * Új puzzle létrehozása
     * @param {number} size - A puzzle mérete (3, 4, vagy 5)
     */
    constructor(size = 3) {
        this.size = size;
        this.tiles = [];
        this.emptyPos = { row: size - 1, col: size - 1 };
        this.moveCount = 0;
        this.init();
    }

    /**
     * Puzzle inicializálása megoldott állapotban
     */
    init() {
        this.tiles = [];
        let num = 1;
        for (let row = 0; row < this.size; row++) {
            this.tiles[row] = [];
            for (let col = 0; col < this.size; col++) {
                if (row === this.size - 1 && col === this.size - 1) {
                    this.tiles[row][col] = 0; // Üres hely
                } else {
                    this.tiles[row][col] = num++;
                }
            }
        }
        this.emptyPos = { row: this.size - 1, col: this.size - 1 };
        this.moveCount = 0;
    }

    /**
     * Puzzle keverése véletlenszerű érvényes lépésekkel
     * @param {number} moves - Keverési lépések száma
     */
    shuffle(moves = 100) {
        const directions = [
            { row: -1, col: 0 },  // fel
            { row: 1, col: 0 },   // le
            { row: 0, col: -1 },  // bal
            { row: 0, col: 1 }    // jobb
        ];

        let lastMove = null;

        for (let i = 0; i < moves; i++) {
            const validMoves = [];
            
            for (const dir of directions) {
                const newRow = this.emptyPos.row + dir.row;
                const newCol = this.emptyPos.col + dir.col;
                
                // Ellenőrizd, hogy érvényes-e a lépés és nem az előző ellentéte-e
                if (this.isValidPosition(newRow, newCol)) {
                    if (!lastMove || !(lastMove.row === -dir.row && lastMove.col === -dir.col)) {
                        validMoves.push({ row: newRow, col: newCol, dir: dir });
                    }
                }
            }

            if (validMoves.length > 0) {
                const move = validMoves[Math.floor(Math.random() * validMoves.length)];
                this.moveTile(move.row, move.col, false);
                lastMove = move.dir;
            }
        }

        this.moveCount = 0;
    }

    /**
     * Ellenőrzi, hogy a pozíció érvényes-e
     * @param {number} row - Sor index
     * @param {number} col - Oszlop index
     * @returns {boolean}
     */
    isValidPosition(row, col) {
        return row >= 0 && row < this.size && col >= 0 && col < this.size;
    }

    /**
     * Ellenőrzi, hogy a csempe mozgatható-e
     * @param {number} row - Sor index
     * @param {number} col - Oszlop index
     * @returns {boolean}
     */
    canMove(row, col) {
        // Csak akkor mozgatható, ha szomszédos az üres hellyel
        const rowDiff = Math.abs(row - this.emptyPos.row);
        const colDiff = Math.abs(col - this.emptyPos.col);
        return (rowDiff === 1 && colDiff === 0) || (rowDiff === 0 && colDiff === 1);
    }

    /**
     * Csempe mozgatása
     * @param {number} row - Sor index
     * @param {number} col - Oszlop index
     * @param {boolean} countMove - Számoljuk-e a lépést
     * @returns {boolean} - Sikeres volt-e a mozgatás
     */
    moveTile(row, col, countMove = true) {
        if (!this.canMove(row, col)) {
            return false;
        }

        // Csempék cseréje
        this.tiles[this.emptyPos.row][this.emptyPos.col] = this.tiles[row][col];
        this.tiles[row][col] = 0;
        this.emptyPos = { row, col };

        if (countMove) {
            this.moveCount++;
        }

        return true;
    }

    /**
     * Ellenőrzi, hogy a puzzle megoldott-e
     * @returns {boolean}
     */
    isSolved() {
        let expected = 1;
        for (let row = 0; row < this.size; row++) {
            for (let col = 0; col < this.size; col++) {
                if (row === this.size - 1 && col === this.size - 1) {
                    if (this.tiles[row][col] !== 0) return false;
                } else {
                    if (this.tiles[row][col] !== expected) return false;
                    expected++;
                }
            }
        }
        return true;
    }

    /**
     * Visszaadja a puzzle aktuális állapotát 1D tömbként
     * @returns {number[]}
     */
    getState() {
        const state = [];
        for (let row = 0; row < this.size; row++) {
            for (let col = 0; col < this.size; col++) {
                state.push(this.tiles[row][col]);
            }
        }
        return state;
    }

    /**
     * Beállítja a puzzle állapotát 1D tömbből
     * @param {number[]} state - Az új állapot
     */
    setState(state) {
        let idx = 0;
        for (let row = 0; row < this.size; row++) {
            for (let col = 0; col < this.size; col++) {
                this.tiles[row][col] = state[idx];
                if (state[idx] === 0) {
                    this.emptyPos = { row, col };
                }
                idx++;
            }
        }
    }

    /**
     * Visszaadja az adott pozíción lévő csempe értékét
     * @param {number} row - Sor
     * @param {number} col - Oszlop
     * @returns {number}
     */
    getTile(row, col) {
        return this.tiles[row][col];
    }

    /**
     * Visszaadja a csempe pozícióját
     * @param {number} value - A keresett csempe értéke
     * @returns {{row: number, col: number} | null}
     */
    findTile(value) {
        for (let row = 0; row < this.size; row++) {
            for (let col = 0; col < this.size; col++) {
                if (this.tiles[row][col] === value) {
                    return { row, col };
                }
            }
        }
        return null;
    }

    /**
     * Visszaadja, hogy a csempe a megfelelő pozícióban van-e
     * @param {number} row - Sor
     * @param {number} col - Oszlop
     * @returns {boolean}
     */
    isInCorrectPosition(row, col) {
        const value = this.tiles[row][col];
        if (value === 0) {
            return row === this.size - 1 && col === this.size - 1;
        }
        const expectedRow = Math.floor((value - 1) / this.size);
        const expectedCol = (value - 1) % this.size;
        return row === expectedRow && col === expectedCol;
    }

    /**
     * Klón létrehozása
     * @returns {SlidingPuzzle}
     */
    clone() {
        const clone = new SlidingPuzzle(this.size);
        clone.setState(this.getState());
        clone.moveCount = this.moveCount;
        return clone;
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SlidingPuzzle;
}

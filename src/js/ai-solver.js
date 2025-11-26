/**
 * PUZZLE - AI Megoldó (A* Search Algorithm)
 * 
 * Ez a modul az A* keresési algoritmust implementálja
 * a puzzle optimális megoldásának megtalálásához.
 */

class PuzzleSolver {
    constructor() {
        this.maxIterations = 100000; // Maximális iterációk korlátja
    }

    /**
     * Manhattan-távolság heurisztika számítása
     * @param {number[]} state - Puzzle állapot 1D tömbként
     * @param {number} size - Puzzle mérete
     * @returns {number} - Manhattan-távolság
     */
    manhattanDistance(state, size) {
        let distance = 0;
        for (let i = 0; i < state.length; i++) {
            const value = state[i];
            if (value !== 0) {
                const currentRow = Math.floor(i / size);
                const currentCol = i % size;
                const targetRow = Math.floor((value - 1) / size);
                const targetCol = (value - 1) % size;
                distance += Math.abs(currentRow - targetRow) + Math.abs(currentCol - targetCol);
            }
        }
        return distance;
    }

    /**
     * Lineáris konfliktus heurisztika (Manhattan + extra)
     * @param {number[]} state - Puzzle állapot
     * @param {number} size - Puzzle mérete
     * @returns {number}
     */
    linearConflict(state, size) {
        let conflicts = 0;

        // Sor konfliktusok
        for (let row = 0; row < size; row++) {
            for (let i = 0; i < size; i++) {
                for (let j = i + 1; j < size; j++) {
                    const posI = row * size + i;
                    const posJ = row * size + j;
                    const valI = state[posI];
                    const valJ = state[posJ];
                    
                    if (valI !== 0 && valJ !== 0) {
                        const targetRowI = Math.floor((valI - 1) / size);
                        const targetRowJ = Math.floor((valJ - 1) / size);
                        const targetColI = (valI - 1) % size;
                        const targetColJ = (valJ - 1) % size;
                        
                        if (targetRowI === row && targetRowJ === row && targetColI > targetColJ) {
                            conflicts++;
                        }
                    }
                }
            }
        }

        // Oszlop konfliktusok
        for (let col = 0; col < size; col++) {
            for (let i = 0; i < size; i++) {
                for (let j = i + 1; j < size; j++) {
                    const posI = i * size + col;
                    const posJ = j * size + col;
                    const valI = state[posI];
                    const valJ = state[posJ];
                    
                    if (valI !== 0 && valJ !== 0) {
                        const targetRowI = Math.floor((valI - 1) / size);
                        const targetRowJ = Math.floor((valJ - 1) / size);
                        const targetColI = (valI - 1) % size;
                        const targetColJ = (valJ - 1) % size;
                        
                        if (targetColI === col && targetColJ === col && targetRowI > targetRowJ) {
                            conflicts++;
                        }
                    }
                }
            }
        }

        return this.manhattanDistance(state, size) + 2 * conflicts;
    }

    /**
     * Üres hely pozíciójának megtalálása
     * @param {number[]} state - Puzzle állapot
     * @returns {number} - Üres hely indexe
     */
    findEmpty(state) {
        return state.indexOf(0);
    }

    /**
     * Lehetséges lépések generálása
     * @param {number[]} state - Aktuális állapot
     * @param {number} size - Puzzle mérete
     * @returns {Array} - Lehetséges új állapotok és a mozgatott csempe
     */
    getNeighbors(state, size) {
        const emptyIdx = this.findEmpty(state);
        const emptyRow = Math.floor(emptyIdx / size);
        const emptyCol = emptyIdx % size;
        const neighbors = [];

        const moves = [
            { dr: -1, dc: 0, name: 'up' },
            { dr: 1, dc: 0, name: 'down' },
            { dr: 0, dc: -1, name: 'left' },
            { dr: 0, dc: 1, name: 'right' }
        ];

        for (const move of moves) {
            const newRow = emptyRow + move.dr;
            const newCol = emptyCol + move.dc;

            if (newRow >= 0 && newRow < size && newCol >= 0 && newCol < size) {
                const newIdx = newRow * size + newCol;
                const newState = [...state];
                const movedTile = state[newIdx];
                
                // Csere
                newState[emptyIdx] = movedTile;
                newState[newIdx] = 0;

                neighbors.push({
                    state: newState,
                    move: move.name,
                    movedTile: movedTile,
                    tilePos: { row: newRow, col: newCol }
                });
            }
        }

        return neighbors;
    }

    /**
     * Állapot kulcs generálása hash-eléshez
     * @param {number[]} state - Puzzle állapot
     * @returns {string}
     */
    stateKey(state) {
        return state.join(',');
    }

    /**
     * A* algoritmus a megoldás megtalálásához
     * @param {number[]} startState - Kezdeti állapot
     * @param {number} size - Puzzle mérete
     * @returns {Object|null} - Megoldás vagy null
     */
    solve(startState, size) {
        // Célállapot generálása
        const goalState = [];
        for (let i = 1; i < size * size; i++) {
            goalState.push(i);
        }
        goalState.push(0);

        // Ellenőrzés: már megoldva van?
        if (this.stateKey(startState) === this.stateKey(goalState)) {
            return { moves: [], path: [startState] };
        }

        // Priority Queue implementáció (egyszerű tömb rendezéssel)
        const openSet = [];
        const closedSet = new Set();
        const gScore = new Map();
        const fScore = new Map();
        const cameFrom = new Map();
        const moveInfo = new Map();

        const startKey = this.stateKey(startState);
        gScore.set(startKey, 0);
        fScore.set(startKey, this.linearConflict(startState, size));
        
        openSet.push({
            state: startState,
            f: fScore.get(startKey)
        });

        let iterations = 0;

        while (openSet.length > 0 && iterations < this.maxIterations) {
            iterations++;

            // Legkisebb f értékű elem kiválasztása
            openSet.sort((a, b) => a.f - b.f);
            const current = openSet.shift();
            const currentKey = this.stateKey(current.state);

            // Célállapot elérve?
            if (currentKey === this.stateKey(goalState)) {
                return this.reconstructPath(current.state, cameFrom, moveInfo, startState);
            }

            closedSet.add(currentKey);

            // Szomszédok vizsgálata
            const neighbors = this.getNeighbors(current.state, size);

            for (const neighbor of neighbors) {
                const neighborKey = this.stateKey(neighbor.state);

                if (closedSet.has(neighborKey)) {
                    continue;
                }

                const tentativeG = gScore.get(currentKey) + 1;

                if (!gScore.has(neighborKey) || tentativeG < gScore.get(neighborKey)) {
                    cameFrom.set(neighborKey, currentKey);
                    moveInfo.set(neighborKey, {
                        move: neighbor.move,
                        movedTile: neighbor.movedTile,
                        tilePos: neighbor.tilePos
                    });
                    gScore.set(neighborKey, tentativeG);
                    const f = tentativeG + this.linearConflict(neighbor.state, size);
                    fScore.set(neighborKey, f);

                    // Hozzáadás ha még nincs benne
                    const existingIdx = openSet.findIndex(n => this.stateKey(n.state) === neighborKey);
                    if (existingIdx === -1) {
                        openSet.push({
                            state: neighbor.state,
                            f: f
                        });
                    }
                }
            }
        }

        // Nem találtunk megoldást (vagy túl sok iteráció)
        return null;
    }

    /**
     * Útvonal rekonstruálása
     * @param {number[]} goalState - Célállapot
     * @param {Map} cameFrom - Honnan érkeztünk
     * @param {Map} moveInfo - Lépés információk
     * @param {number[]} startState - Kezdeti állapot
     * @returns {Object}
     */
    reconstructPath(goalState, cameFrom, moveInfo, startState) {
        const path = [];
        const moves = [];
        let currentKey = this.stateKey(goalState);
        const startKey = this.stateKey(startState);

        while (currentKey !== startKey) {
            const state = currentKey.split(',').map(Number);
            path.unshift(state);
            moves.unshift(moveInfo.get(currentKey));
            currentKey = cameFrom.get(currentKey);
        }
        path.unshift(startState);

        return { moves, path };
    }

    /**
     * Csak az első lépés megtalálása (hint-hez)
     * @param {number[]} state - Aktuális állapot
     * @param {number} size - Puzzle mérete
     * @returns {Object|null}
     */
    getNextMove(state, size) {
        const solution = this.solve(state, size);
        if (solution && solution.moves.length > 0) {
            return {
                ...solution.moves[0],
                totalMoves: solution.moves.length
            };
        }
        return null;
    }

    /**
     * Megoldható-e a puzzle
     * @param {number[]} state - Puzzle állapot
     * @param {number} size - Puzzle mérete
     * @returns {boolean}
     */
    isSolvable(state, size) {
        let inversions = 0;
        const tilesWithoutEmpty = state.filter(t => t !== 0);

        for (let i = 0; i < tilesWithoutEmpty.length; i++) {
            for (let j = i + 1; j < tilesWithoutEmpty.length; j++) {
                if (tilesWithoutEmpty[i] > tilesWithoutEmpty[j]) {
                    inversions++;
                }
            }
        }

        if (size % 2 === 1) {
            // Páratlan méret: páros inverziószám kell
            return inversions % 2 === 0;
        } else {
            // Páros méret: inverziószám + üres sor paritása számít
            const emptyRow = Math.floor(state.indexOf(0) / size);
            const emptyRowFromBottom = size - emptyRow;
            return (inversions + emptyRowFromBottom) % 2 === 0;
        }
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = PuzzleSolver;
}

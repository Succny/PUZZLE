/**
 * SOKOBAN - AI Megoldó (BFS/A* Search Algorithm)
 * 
 * Ez a modul a keresési algoritmust implementálja
 * a Sokoban pályák megoldásának megtalálásához.
 */

class SokobanSolver {
    constructor() {
        this.maxIterations = 50000; // Maximális iterációk korlátja
        this.directions = [
            { dRow: -1, dCol: 0, name: 'up', arrow: '↑' },
            { dRow: 1, dCol: 0, name: 'down', arrow: '↓' },
            { dRow: 0, dCol: -1, name: 'left', arrow: '←' },
            { dRow: 0, dCol: 1, name: 'right', arrow: '→' }
        ];
    }

    /**
     * Heurisztika: összes láda Manhattan-távolsága a legközelebbi célhelytől
     * @param {Sokoban} game - Játék objektum
     * @returns {number}
     */
    heuristic(game) {
        const state = game.getState();
        let totalDistance = 0;

        for (const box of state.boxes) {
            let minDist = Infinity;
            for (const goal of state.goals) {
                const dist = Math.abs(box.row - goal.row) + Math.abs(box.col - goal.col);
                minDist = Math.min(minDist, dist);
            }
            totalDistance += minDist;
        }

        return totalDistance;
    }

    /**
     * Ellenőrzi, hogy a láda pozíció deadlock-e
     * @param {Sokoban} game - Játék objektum
     * @param {number} row - Sor
     * @param {number} col - Oszlop
     * @returns {boolean}
     */
    isDeadlock(game, row, col) {
        // Ha célhelyen van, nem deadlock
        if (game.isGoal(row, col)) {
            return false;
        }

        // Sarok deadlock
        const walls = {
            up: game.isWall(row - 1, col),
            down: game.isWall(row + 1, col),
            left: game.isWall(row, col - 1),
            right: game.isWall(row, col + 1)
        };

        if ((walls.up && walls.left) || (walls.up && walls.right) ||
            (walls.down && walls.left) || (walls.down && walls.right)) {
            return true;
        }

        return false;
    }

    /**
     * Ellenőrzi, hogy az állapot deadlock-e (bármely láda)
     * @param {Sokoban} game - Játék objektum
     * @returns {boolean}
     */
    hasDeadlock(game) {
        const state = game.getState();
        for (const box of state.boxes) {
            if (!box.onGoal && this.isDeadlock(game, box.row, box.col)) {
                return true;
            }
        }
        return false;
    }

    /**
     * BFS algoritmus a megoldás megtalálásához
     * @param {Sokoban} game - Játék objektum
     * @returns {Object|null} - Megoldás vagy null
     */
    solve(game) {
        if (game.isSolved()) {
            return { moves: [], success: true };
        }

        const visited = new Set();
        const queue = [{
            game: game.clone(),
            moves: [],
            cost: 0
        }];

        visited.add(game.getStateKey());

        let iterations = 0;

        while (queue.length > 0 && iterations < this.maxIterations) {
            iterations++;

            // A* prioritás: legkisebb f(n) = g(n) + h(n)
            queue.sort((a, b) => (a.cost + this.heuristic(a.game)) - (b.cost + this.heuristic(b.game)));

            const current = queue.shift();

            for (const dir of this.directions) {
                const newGame = current.game.clone();
                const result = newGame.move(dir.dRow, dir.dCol);

                if (result.success) {
                    const stateKey = newGame.getStateKey();

                    if (!visited.has(stateKey)) {
                        // Deadlock ellenőrzés
                        if (result.pushed && this.hasDeadlock(newGame)) {
                            continue; // Skip deadlock states
                        }

                        visited.add(stateKey);

                        const newMoves = [...current.moves, {
                            direction: dir.name,
                            arrow: dir.arrow,
                            dRow: dir.dRow,
                            dCol: dir.dCol,
                            pushed: result.pushed
                        }];

                        if (newGame.isSolved()) {
                            return {
                                moves: newMoves,
                                success: true,
                                iterations: iterations
                            };
                        }

                        queue.push({
                            game: newGame,
                            moves: newMoves,
                            cost: current.cost + 1
                        });
                    }
                }
            }
        }

        // Nem találtunk megoldást
        return {
            moves: [],
            success: false,
            iterations: iterations,
            reason: iterations >= this.maxIterations ? 'timeout' : 'unsolvable'
        };
    }

    /**
     * Csak a következő lépés megtalálása (hint-hez)
     * @param {Sokoban} game - Játék objektum
     * @returns {Object|null}
     */
    getNextMove(game) {
        const solution = this.solve(game);
        if (solution && solution.success && solution.moves.length > 0) {
            return {
                ...solution.moves[0],
                totalMoves: solution.moves.length,
                pushCount: solution.moves.filter(m => m.pushed).length
            };
        }
        return null;
    }

    /**
     * Ellenőrzi, hogy a pálya megoldható-e az aktuális állapotból
     * @param {Sokoban} game - Játék objektum
     * @returns {boolean}
     */
    isSolvable(game) {
        const solution = this.solve(game);
        return solution && solution.success;
    }

    /**
     * Megoldás hosszának becslése (gyors heurisztika)
     * @param {Sokoban} game - Játék objektum
     * @returns {number}
     */
    estimateSolutionLength(game) {
        return this.heuristic(game) * 2; // Durva becslés
    }

    /**
     * Deadlock pozíciók azonosítása a pályán
     * @param {Sokoban} game - Játék objektum
     * @returns {Array}
     */
    findDeadlockPositions(game) {
        const deadlocks = [];
        const state = game.getState();

        for (let row = 0; row < game.height; row++) {
            for (let col = 0; col < game.width; col++) {
                if (!game.isWall(row, col) && !game.isGoal(row, col)) {
                    if (this.isDeadlock(game, row, col)) {
                        deadlocks.push({ row, col });
                    }
                }
            }
        }

        return deadlocks;
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = SokobanSolver;
}

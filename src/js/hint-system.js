/**
 * PUZZLE - AI Hint Rendszer
 * 
 * Ez a modul felelős a játékos segítéséért:
 * - Hint generálás
 * - Stratégiai tanácsok
 * - Elakadás felismerés
 * - Motiváló üzenetek
 */

class HintSystem {
    constructor(solver) {
        this.solver = solver;
        this.lastHintTime = 0;
        this.hintCount = 0;
        this.idleTime = 0;
        this.lastMoveTime = Date.now();
        
        // Üzenetek kategóriánként
        this.messages = {
            greeting: [
                "Üdvözöllek! Készen állok segíteni. 🤖",
                "Szia! Kérj tőlem hint-et, ha elakadtál!",
                "Hello! Együtt megoldjuk ezt a puzzle-t!"
            ],
            encouragement: [
                "Szuper, jó úton haladsz! 👍",
                "Remek lépés volt!",
                "Folytatsd így, már közel vagy!",
                "Nagyon jól csinálod!"
            ],
            stuck: [
                "Úgy látom, egy ideje nem léptél. Segíthetek?",
                "Elakadtál? Kérj egy hint-et!",
                "Ne aggódj, néha nehéz ezeket a puzzle-okat. Segítek!"
            ],
            hint_used: [
                "Remélem, hasznos volt a tipp! 💡",
                "Most már tudod, mit kell tenni!",
                "Ezzel a lépéssel közelebb kerülsz a megoldáshoz!"
            ],
            almost_done: [
                "Már majdnem kész vagy! Csak pár lépés!",
                "A végéhez közelítesz, ne add fel!",
                "Utolsó lépések... 🎯"
            ],
            solved: [
                "🎉 Fantasztikus! Megoldottad!",
                "🏆 Gratulálok a győzelemhez!",
                "⭐ Kiváló munka! Készen állsz a következő kihívásra?"
            ]
        };

        // Stratégiai tanácsok
        this.strategies = {
            3: [
                "3×3-as puzzle-nál először az első sort rendezd!",
                "Ha az első sor kész, a második sorra koncentrálj.",
                "Az utolsó sorban csak a két alsó sarok marad."
            ],
            4: [
                "4×4-es puzzle-nál dolgozz rétegekben!",
                "Először az első két sort, aztán a maradékot.",
                "Ne feledd: a sarkokat nehezebb a helyükre tenni."
            ],
            5: [
                "5×5-ös puzzle: ez már igazi kihívás!",
                "Oszd részekre a problémát: felső rész, majd alsó.",
                "Légy türelmes, ez a méret több időt igényel."
            ]
        };
    }

    /**
     * Véletlenszerű üzenet kiválasztása egy kategóriából
     * @param {string} category - Üzenet kategória
     * @returns {string}
     */
    getRandomMessage(category) {
        const messages = this.messages[category];
        if (!messages || messages.length === 0) return "";
        return messages[Math.floor(Math.random() * messages.length)];
    }

    /**
     * Stratégiai tanács a puzzle mérete alapján
     * @param {number} size - Puzzle mérete
     * @returns {string}
     */
    getStrategyTip(size) {
        const tips = this.strategies[size] || this.strategies[3];
        return tips[Math.floor(Math.random() * tips.length)];
    }

    /**
     * Hint generálása az aktuális állapothoz
     * @param {SlidingPuzzle} puzzle - A puzzle objektum
     * @returns {Object} - Hint információk
     */
    generateHint(puzzle) {
        this.hintCount++;
        const state = puzzle.getState();
        const size = puzzle.size;

        // A* megoldó használata
        const nextMove = this.solver.getNextMove(state, size);

        if (!nextMove) {
            return {
                type: 'error',
                message: "Nem tudok megoldást találni. Próbálj új játékot!",
                tile: null,
                direction: null
            };
        }

        const directions = {
            'up': 'felfelé',
            'down': 'lefelé',
            'left': 'balra',
            'right': 'jobbra'
        };

        const hint = {
            type: 'move',
            message: `Mozgasd a ${nextMove.movedTile}-es csempét ${directions[nextMove.move]}! ` +
                     `(Még ${nextMove.totalMoves} lépés a megoldásig)`,
            tile: nextMove.movedTile,
            tilePos: nextMove.tilePos,
            direction: nextMove.move,
            remainingMoves: nextMove.totalMoves
        };

        return hint;
    }

    /**
     * Részletes stratégiai hint generálása
     * @param {SlidingPuzzle} puzzle - A puzzle objektum
     * @returns {Object}
     */
    generateDetailedHint(puzzle) {
        const state = puzzle.getState();
        const size = puzzle.size;
        
        // Hány csempe van a helyén?
        let correctCount = 0;
        for (let row = 0; row < size; row++) {
            for (let col = 0; col < size; col++) {
                if (puzzle.isInCorrectPosition(row, col)) {
                    correctCount++;
                }
            }
        }

        const totalTiles = size * size;
        const progress = Math.round((correctCount / totalTiles) * 100);

        // Manhattan távolság az egész puzzle-ra
        const distance = this.solver.manhattanDistance(state, size);

        let message = `📊 Állapot elemzés:\n`;
        message += `• ${correctCount}/${totalTiles} csempe van a helyén (${progress}%)\n`;
        message += `• Becsült távolság a megoldástól: ${distance}\n\n`;

        if (progress < 30) {
            message += `💡 Tipp: ${this.getStrategyTip(size)}`;
        } else if (progress < 70) {
            message += `💡 Jó úton haladsz! Koncentrálj a még rossz helyen lévő csempékre.`;
        } else {
            message += `💡 Már majdnem kész! Csak néhány csempét kell még mozgatni.`;
        }

        return {
            type: 'analysis',
            message: message,
            progress: progress,
            correctTiles: correctCount,
            distance: distance
        };
    }

    /**
     * Elakadás ellenőrzése és proaktív segítség
     * @param {number} idleSeconds - Inaktív másodpercek
     * @returns {string|null}
     */
    checkIdleState(idleSeconds) {
        if (idleSeconds > 30 && idleSeconds < 35) {
            return this.getRandomMessage('stuck');
        } else if (idleSeconds > 60) {
            return "Látom, egy ideje nem léptél. Kattints a 'Hint Kérése' gombra segítségért!";
        }
        return null;
    }

    /**
     * Lépés utáni visszajelzés
     * @param {SlidingPuzzle} puzzle - A puzzle objektum
     * @param {boolean} wasGoodMove - Jó lépés volt-e (közelebb a megoldáshoz)
     * @returns {string}
     */
    getMoveResponse(puzzle, wasGoodMove) {
        const state = puzzle.getState();
        const size = puzzle.size;
        const nextMove = this.solver.getNextMove(state, size);

        if (!nextMove) {
            // Megoldva
            return this.getRandomMessage('solved');
        }

        if (nextMove.totalMoves <= 3) {
            return this.getRandomMessage('almost_done');
        }

        if (wasGoodMove && Math.random() > 0.7) {
            return this.getRandomMessage('encouragement');
        }

        return null;
    }

    /**
     * Kezdő üzenet új játékhoz
     * @param {number} size - Puzzle mérete
     * @returns {string}
     */
    getWelcomeMessage(size) {
        const difficulty = {
            3: 'könnyű',
            4: 'közepes',
            5: 'nehéz'
        };

        return `🎮 Új ${size}×${size} (${difficulty[size]}) játék! ` +
               `Rendezd a számokat 1-től ${size*size - 1}-ig! ` +
               `${this.getStrategyTip(size)}`;
    }

    /**
     * Hint statisztikák lekérése
     * @returns {Object}
     */
    getStats() {
        return {
            hintsUsed: this.hintCount
        };
    }

    /**
     * Hint számlálók nullázása
     */
    reset() {
        this.hintCount = 0;
        this.lastHintTime = 0;
        this.lastMoveTime = Date.now();
    }
}

// Export for use in other modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = HintSystem;
}

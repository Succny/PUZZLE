/**
 * SOKOBAN - AI Hint Rendszer
 * 
 * Ez a modul felelős a játékos segítéséért:
 * - Hint generálás
 * - Stratégiai tanácsok
 * - Deadlock figyelmeztetés
 * - Motiváló üzenetek
 */

class HintSystem {
    constructor(solver) {
        this.solver = solver;
        this.lastHintTime = 0;
        this.hintCount = 0;
        this.lastMoveTime = Date.now();
        
        // Időküszöbök (másodpercben)
        this.IDLE_WARNING_THRESHOLD = 30;
        this.IDLE_CRITICAL_THRESHOLD = 60;
        
        // Üzenetek kategóriánként
        this.messages = {
            greeting: [
                "Üdvözöllek! Készen állok segíteni. 🤖",
                "Szia! Told a ládákat a célhelyekre!",
                "Hello! Együtt megoldjuk ezt a pályát!"
            ],
            encouragement: [
                "Szuper, jó úton haladsz! 👍",
                "Remek lépés volt!",
                "Folytatsd így!",
                "Nagyon jól csinálod!",
                "Egy láda már a helyén! ✅"
            ],
            stuck: [
                "Úgy látom, egy ideje nem léptél. Segíthetek?",
                "Elakadtál? Kérj egy hint-et!",
                "Ne aggódj, a Sokoban nehéz játék. Segítek!"
            ],
            deadlock: [
                "⚠️ Vigyázz! Egy láda zsákutcába került!",
                "⚠️ Ez a láda már nem mozdítható a célhelyre!",
                "⚠️ Deadlock! Használd a Vissza gombot!"
            ],
            hint_used: [
                "Remélem, hasznos volt a tipp! 💡",
                "Most már tudod, mit kell tenni!",
                "Ezzel közelebb kerülsz a megoldáshoz!"
            ],
            almost_done: [
                "Már majdnem kész vagy! Csak pár láda!",
                "A végéhez közelítesz, ne add fel!",
                "Utolsó lépések... 🎯"
            ],
            solved: [
                "🎉 Fantasztikus! Teljesítetted a pályát!",
                "🏆 Gratulálok a győzelemhez!",
                "⭐ Kiváló munka! Készen állsz a következő pályára?"
            ],
            undo_suggest: [
                "💡 Tipp: Használd a ↩️ Vissza gombot!",
                "Lépj vissza és próbálj más megközelítést!",
                "Az undo a barátod - ne félj használni!"
            ]
        };

        // Stratégiai tanácsok
        this.strategies = [
            "Először gondold végig, melyik ládát mozdítsd!",
            "A sarokban lévő ládákat nehéz kimozdítani.",
            "Próbáld a ládákat a fal mentén a célhelyek felé tolni.",
            "Néha vissza kell lépni, hogy előre juss.",
            "A ládák sorrendje is számít!",
            "Vigyázz, hogy ne told sarokba a ládát!",
            "Gondolkodj előre: mi lesz a következő lépés után?"
        ];
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
     * Véletlenszerű stratégiai tanács
     * @returns {string}
     */
    getStrategyTip() {
        return this.strategies[Math.floor(Math.random() * this.strategies.length)];
    }

    /**
     * Hint generálása az aktuális állapothoz
     * @param {Sokoban} game - A játék objektum
     * @returns {Object} - Hint információk
     */
    generateHint(game) {
        this.hintCount++;
        
        // Megoldó használata
        const nextMove = this.solver.getNextMove(game);

        if (!nextMove) {
            // Nem megoldható - valószínűleg deadlock
            return {
                type: 'deadlock',
                message: this.getRandomMessage('deadlock') + "\n" + this.getRandomMessage('undo_suggest'),
                direction: null
            };
        }

        const directionNames = {
            'up': 'felfelé (↑)',
            'down': 'lefelé (↓)',
            'left': 'balra (←)',
            'right': 'jobbra (→)'
        };

        const actionText = nextMove.pushed ? 
            `Told a ládát ${directionNames[nextMove.direction]}!` :
            `Menj ${directionNames[nextMove.direction]}!`;

        const hint = {
            type: 'move',
            message: `${actionText}\n(Még ${nextMove.totalMoves} lépés, ${nextMove.pushCount} tolás a megoldásig)`,
            direction: nextMove.direction,
            arrow: nextMove.arrow,
            dRow: nextMove.dRow,
            dCol: nextMove.dCol,
            pushed: nextMove.pushed,
            remainingMoves: nextMove.totalMoves
        };

        return hint;
    }

    /**
     * Részletes állapot elemzés
     * @param {Sokoban} game - A játék objektum
     * @returns {Object}
     */
    generateDetailedHint(game) {
        const state = game.getState();
        
        // Hány láda van célhelyen?
        const boxesOnGoal = state.boxes.filter(b => b.onGoal).length;
        const totalBoxes = state.boxes.length;
        const progress = Math.round((boxesOnGoal / totalBoxes) * 100);

        let message = `📊 Állapot elemzés:\n`;
        message += `• ${boxesOnGoal}/${totalBoxes} láda a célhelyen (${progress}%)\n`;
        message += `• Eddigi lépések: ${game.moves}\n`;
        message += `• Eddigi tolások: ${game.pushes}\n\n`;

        // Megoldhatóság ellenőrzése
        const solution = this.solver.solve(game);
        if (solution && solution.success) {
            message += `✅ A pálya megoldható!\n`;
            message += `Hátralévő lépések: ~${solution.moves.length}\n\n`;
        } else {
            message += `⚠️ A pálya nem megoldható ebből az állapotból!\n`;
            message += `Használd a Vissza gombot!\n\n`;
        }

        message += `💡 Tipp: ${this.getStrategyTip()}`;

        return {
            type: 'analysis',
            message: message,
            progress: progress,
            boxesOnGoal: boxesOnGoal,
            totalBoxes: totalBoxes,
            solvable: solution && solution.success
        };
    }

    /**
     * Deadlock figyelmeztetés generálása
     * @param {Sokoban} game - A játék objektum
     * @param {number} boxRow - Láda sora
     * @param {number} boxCol - Láda oszlopa
     * @returns {string|null}
     */
    getDeadlockWarning(game, boxRow, boxCol) {
        if (this.solver.isDeadlock(game, boxRow, boxCol)) {
            return this.getRandomMessage('deadlock');
        }
        return null;
    }

    /**
     * Elakadás ellenőrzése és proaktív segítség
     * @param {number} idleSeconds - Inaktív másodpercek
     * @returns {string|null}
     */
    checkIdleState(idleSeconds) {
        if (idleSeconds > this.IDLE_WARNING_THRESHOLD && idleSeconds < this.IDLE_WARNING_THRESHOLD + 5) {
            return this.getRandomMessage('stuck');
        } else if (idleSeconds > this.IDLE_CRITICAL_THRESHOLD) {
            return "Látom, elakadtál. Kattints a '💡 Hint Kérése' gombra, és megmutatom a következő lépést!";
        }
        return null;
    }

    /**
     * Lépés utáni visszajelzés
     * @param {Sokoban} game - A játék objektum
     * @param {Object} moveResult - A lépés eredménye
     * @returns {string|null}
     */
    getMoveResponse(game, moveResult) {
        if (moveResult.solved) {
            return this.getRandomMessage('solved');
        }

        if (moveResult.deadlock) {
            return this.getRandomMessage('deadlock');
        }

        const state = game.getState();
        const boxesOnGoal = state.boxes.filter(b => b.onGoal).length;
        const totalBoxes = state.boxes.length;

        if (boxesOnGoal === totalBoxes - 1) {
            return this.getRandomMessage('almost_done');
        }

        // Véletlenszerű bátorítás
        if (moveResult.pushed && Math.random() > 0.7) {
            return this.getRandomMessage('encouragement');
        }

        return null;
    }

    /**
     * Kezdő üzenet új pályához
     * @param {Object} level - Pálya objektum
     * @param {number} levelNum - Pálya száma
     * @returns {string}
     */
    getWelcomeMessage(level, levelNum) {
        return `🎮 ${levelNum}. pálya: "${level.name}" (${level.difficulty})\n` +
               `Told a ládákat (📦) a célhelyekre (🎯)!\n` +
               `${this.getStrategyTip()}`;
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

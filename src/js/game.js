/**
 * SOKOBAN - Fő Játék Vezérlő
 * 
 * Ez a modul összekapcsolja az összes komponenst:
 * - Sokoban játék logika
 * - AI megoldó
 * - Hint rendszer
 * - Felhasználói felület
 */

class SokobanGame {
    constructor() {
        this.game = null;
        this.solver = new SokobanSolver();
        this.hintSystem = new HintSystem(this.solver);
        this.currentLevel = 0;
        this.timer = null;
        this.seconds = 0;
        this.gameStarted = false;
        this.highlightDirection = null;
        this.lastMoveTime = Date.now();
        this.idleCheckInterval = null;

        this.initUI();
    }

    /**
     * UI inicializálása és eseménykezelők beállítása
     */
    initUI() {
        // DOM elemek
        this.gameGrid = document.getElementById('gameGrid');
        this.moveCountEl = document.getElementById('moveCount');
        this.pushCountEl = document.getElementById('pushCount');
        this.timerEl = document.getElementById('timer');
        this.aiMessageEl = document.getElementById('aiMessage');
        this.winModal = document.getElementById('winModal');
        this.finalMovesEl = document.getElementById('finalMoves');
        this.finalPushesEl = document.getElementById('finalPushes');
        this.finalTimeEl = document.getElementById('finalTime');

        // Gombok
        document.getElementById('restartBtn').addEventListener('click', () => this.restartLevel());
        document.getElementById('undoBtn').addEventListener('click', () => this.undo());
        document.getElementById('nextLevelBtn').addEventListener('click', () => this.nextLevel());
        document.getElementById('hintBtn').addEventListener('click', () => this.showHint());
        document.getElementById('showNextBtn').addEventListener('click', () => this.showNextMove());

        // Pálya választók
        document.querySelectorAll('.level-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                document.querySelectorAll('.level-btn').forEach(b => b.classList.remove('active'));
                e.target.classList.add('active');
                this.currentLevel = parseInt(e.target.dataset.level) - 1;
                this.loadLevel(this.currentLevel);
            });
        });

        // Billentyűzet támogatás
        document.addEventListener('keydown', (e) => this.handleKeyPress(e));

        // Első pálya betöltése
        this.loadLevel(0);
    }

    /**
     * Pálya betöltése
     * @param {number} levelIndex - Pálya index
     */
    loadLevel(levelIndex) {
        if (levelIndex < 0 || levelIndex >= LEVELS.length) {
            return;
        }

        this.currentLevel = levelIndex;
        const level = LEVELS[levelIndex];
        
        // Játék létrehozása
        this.game = new Sokoban(level);
        
        // Timer és számlálók reset
        this.stopTimer();
        this.seconds = 0;
        this.gameStarted = true;
        this.lastMoveTime = Date.now();
        this.highlightDirection = null;
        
        // Hint rendszer reset
        this.hintSystem.reset();

        // Pálya gomb frissítése
        document.querySelectorAll('.level-btn').forEach((btn, idx) => {
            btn.classList.toggle('active', idx === levelIndex);
        });

        // UI frissítése
        this.updateStats();
        this.renderGame();

        // Timer indítása
        this.startTimer();

        // Idle ellenőrzés
        this.startIdleCheck();

        // Üdvözlő üzenet
        this.showAIMessage(this.hintSystem.getWelcomeMessage(level, levelIndex + 1));
    }

    /**
     * Pálya újraindítása
     */
    restartLevel() {
        this.game.restart();
        this.stopTimer();
        this.seconds = 0;
        this.gameStarted = true;
        this.lastMoveTime = Date.now();
        this.highlightDirection = null;
        this.hintSystem.reset();
        
        this.updateStats();
        this.renderGame();
        this.startTimer();
        
        this.showAIMessage("🔄 Pálya újraindítva. Próbáld újra!");
    }

    /**
     * Következő pálya
     */
    nextLevel() {
        this.winModal.classList.add('hidden');
        if (this.currentLevel < LEVELS.length - 1) {
            this.loadLevel(this.currentLevel + 1);
        } else {
            this.showAIMessage("🎊 Gratulálok! Minden pályát teljesítettél!");
            this.loadLevel(0);
        }
    }

    /**
     * Játék megjelenítése
     */
    renderGame() {
        this.gameGrid.innerHTML = '';
        
        // Grid méret beállítása
        this.gameGrid.style.gridTemplateColumns = `repeat(${this.game.width}, 1fr)`;
        this.gameGrid.style.gridTemplateRows = `repeat(${this.game.height}, 1fr)`;

        for (let row = 0; row < this.game.height; row++) {
            for (let col = 0; col < this.game.width; col++) {
                const tile = this.game.getTile(row, col);
                const cell = document.createElement('div');
                cell.className = 'cell';
                cell.dataset.row = row;
                cell.dataset.col = col;

                // Alapértelmezett: padló
                let content = '';
                let classes = ['cell'];

                switch (tile) {
                    case TILES.WALL:
                        classes.push('wall');
                        content = '';
                        break;
                    case TILES.FLOOR:
                        classes.push('floor');
                        break;
                    case TILES.GOAL:
                        classes.push('floor', 'goal');
                        content = '🎯';
                        break;
                    case TILES.BOX:
                        classes.push('floor', 'box');
                        content = '📦';
                        break;
                    case TILES.BOX_ON_GOAL:
                        classes.push('floor', 'box-on-goal');
                        content = '✅';
                        break;
                    case TILES.PLAYER:
                        classes.push('floor', 'player');
                        content = '🧑';
                        break;
                    case TILES.PLAYER_ON_GOAL:
                        classes.push('floor', 'player', 'goal');
                        content = '🧑';
                        break;
                }

                cell.className = classes.join(' ');
                cell.textContent = content;

                // Irány kiemelés
                if (this.highlightDirection) {
                    const playerPos = this.game.playerPos;
                    const targetRow = playerPos.row + this.highlightDirection.dRow;
                    const targetCol = playerPos.col + this.highlightDirection.dCol;
                    
                    if (row === targetRow && col === targetCol) {
                        cell.classList.add('highlight');
                    }
                }

                this.gameGrid.appendChild(cell);
            }
        }
    }

    /**
     * Billentyűzet kezelés
     * @param {KeyboardEvent} e - Billentyűzet esemény
     */
    handleKeyPress(e) {
        if (!this.gameStarted) return;

        const keyToDirection = {
            'ArrowUp': { dRow: -1, dCol: 0 },
            'ArrowDown': { dRow: 1, dCol: 0 },
            'ArrowLeft': { dRow: 0, dCol: -1 },
            'ArrowRight': { dRow: 0, dCol: 1 },
            'w': { dRow: -1, dCol: 0 },
            'W': { dRow: -1, dCol: 0 },
            's': { dRow: 1, dCol: 0 },
            'S': { dRow: 1, dCol: 0 },
            'a': { dRow: 0, dCol: -1 },
            'A': { dRow: 0, dCol: -1 },
            'd': { dRow: 0, dCol: 1 },
            'D': { dRow: 0, dCol: 1 }
        };

        // Undo
        if (e.key === 'z' || e.key === 'Z' || e.key === 'Backspace') {
            e.preventDefault();
            this.undo();
            return;
        }

        // Restart
        if (e.key === 'r' || e.key === 'R') {
            e.preventDefault();
            this.restartLevel();
            return;
        }

        const dir = keyToDirection[e.key];
        if (dir) {
            e.preventDefault();
            this.makeMove(dir.dRow, dir.dCol);
        }
    }

    /**
     * Lépés végrehajtása
     * @param {number} dRow - Sor irány
     * @param {number} dCol - Oszlop irány
     */
    makeMove(dRow, dCol) {
        if (!this.gameStarted) return;

        const result = this.game.move(dRow, dCol);
        
        if (result.success) {
            this.lastMoveTime = Date.now();
            this.clearHighlight();

            // UI frissítés
            this.updateStats();
            this.renderGame();

            // AI visszajelzés
            const response = this.hintSystem.getMoveResponse(this.game, result);
            if (response) {
                this.showAIMessage(response);
            }

            // Győzelem ellenőrzés
            if (result.solved) {
                this.handleWin();
            }
        }
    }

    /**
     * Visszalépés (undo)
     */
    undo() {
        if (!this.gameStarted) return;

        if (this.game.undo()) {
            this.updateStats();
            this.renderGame();
            this.showAIMessage("↩️ Visszalépés sikeres!");
        } else {
            this.showAIMessage("Nincs több visszalépési lehetőség.");
        }
    }

    /**
     * Győzelem kezelése
     */
    handleWin() {
        this.gameStarted = false;
        this.stopTimer();
        this.stopIdleCheck();

        // Modal megjelenítése
        this.finalMovesEl.textContent = this.game.moves;
        this.finalPushesEl.textContent = this.game.pushes;
        this.finalTimeEl.textContent = this.formatTime(this.seconds);
        this.winModal.classList.remove('hidden');

        // AI gratulál
        this.showAIMessage(this.hintSystem.getRandomMessage('solved'));
    }

    /**
     * Hint megjelenítése
     */
    showHint() {
        if (!this.gameStarted) return;

        const hint = this.hintSystem.generateDetailedHint(this.game);
        this.showAIMessage(hint.message);
    }

    /**
     * Következő lépés megmutatása
     */
    showNextMove() {
        if (!this.gameStarted) return;

        const hint = this.hintSystem.generateHint(this.game);
        
        if (hint.type === 'move') {
            this.highlightDirection = {
                dRow: hint.dRow,
                dCol: hint.dCol
            };
            this.renderGame();
            this.showAIMessage(hint.message);

            // Kiemelés automatikus eltávolítása
            setTimeout(() => {
                this.clearHighlight();
                this.renderGame();
            }, 3000);
        } else {
            this.showAIMessage(hint.message);
        }
    }

    /**
     * Kiemelés törlése
     */
    clearHighlight() {
        this.highlightDirection = null;
    }

    /**
     * AI üzenet megjelenítése
     * @param {string} message - Üzenet szövege
     */
    showAIMessage(message) {
        this.aiMessageEl.textContent = message;
        
        // Animáció effekt
        this.aiMessageEl.style.opacity = '0';
        setTimeout(() => {
            this.aiMessageEl.style.opacity = '1';
        }, 50);
    }

    /**
     * Statisztikák frissítése
     */
    updateStats() {
        this.moveCountEl.textContent = this.game.moves;
        this.pushCountEl.textContent = this.game.pushes;
    }

    /**
     * Timer frissítése
     */
    updateTimer() {
        this.timerEl.textContent = this.formatTime(this.seconds);
    }

    /**
     * Idő formázása
     * @param {number} totalSeconds - Összes másodperc
     * @returns {string}
     */
    formatTime(totalSeconds) {
        const minutes = Math.floor(totalSeconds / 60);
        const seconds = totalSeconds % 60;
        return `${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }

    /**
     * Timer indítása
     */
    startTimer() {
        this.timer = setInterval(() => {
            this.seconds++;
            this.updateTimer();
        }, 1000);
    }

    /**
     * Timer leállítása
     */
    stopTimer() {
        if (this.timer) {
            clearInterval(this.timer);
            this.timer = null;
        }
    }

    /**
     * Idle ellenőrzés indítása
     */
    startIdleCheck() {
        this.stopIdleCheck();
        this.idleCheckInterval = setInterval(() => {
            if (!this.gameStarted) return;
            
            const idleSeconds = Math.floor((Date.now() - this.lastMoveTime) / 1000);
            const idleMessage = this.hintSystem.checkIdleState(idleSeconds);
            
            if (idleMessage) {
                this.showAIMessage(idleMessage);
            }
        }, 5000);
    }

    /**
     * Idle ellenőrzés leállítása
     */
    stopIdleCheck() {
        if (this.idleCheckInterval) {
            clearInterval(this.idleCheckInterval);
            this.idleCheckInterval = null;
        }
    }
}

// Játék indítása amikor a DOM betöltődött
document.addEventListener('DOMContentLoaded', () => {
    window.sokobanGame = new SokobanGame();
});

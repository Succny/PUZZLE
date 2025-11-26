/**
 * PUZZLE - Fő Játék Vezérlő
 * 
 * Ez a modul összekapcsolja az összes komponenst:
 * - Puzzle logika
 * - AI megoldó
 * - Hint rendszer
 * - Felhasználói felület
 */

class PuzzleGame {
    constructor() {
        this.puzzle = null;
        this.solver = new PuzzleSolver();
        this.hintSystem = new HintSystem(this.solver);
        this.size = 3;
        this.timer = null;
        this.seconds = 0;
        this.gameStarted = false;
        this.highlightedTile = null;
        this.lastMoveTime = Date.now();
        this.idleCheckInterval = null;
        this.previousDistance = 0;

        this.initUI();
    }

    /**
     * UI inicializálása és eseménykezelők beállítása
     */
    initUI() {
        // DOM elemek
        this.puzzleGrid = document.getElementById('puzzleGrid');
        this.moveCountEl = document.getElementById('moveCount');
        this.timerEl = document.getElementById('timer');
        this.optimalMovesEl = document.getElementById('optimalMoves');
        this.aiMessageEl = document.getElementById('aiMessage');
        this.winModal = document.getElementById('winModal');
        this.finalMovesEl = document.getElementById('finalMoves');
        this.finalTimeEl = document.getElementById('finalTime');

        // Gombok
        document.getElementById('newGameBtn').addEventListener('click', () => this.startNewGame());
        document.getElementById('playAgainBtn').addEventListener('click', () => {
            this.winModal.classList.add('hidden');
            this.startNewGame();
        });
        document.getElementById('hintBtn').addEventListener('click', () => this.showHint());
        document.getElementById('showNextBtn').addEventListener('click', () => this.showNextMove());

        // Nehézség választók
        document.querySelectorAll('.difficulty-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                document.querySelectorAll('.difficulty-btn').forEach(b => b.classList.remove('active'));
                e.target.classList.add('active');
                this.size = parseInt(e.target.dataset.size);
                this.startNewGame();
            });
        });

        // Billentyűzet támogatás
        document.addEventListener('keydown', (e) => this.handleKeyPress(e));

        // Kezdő játék indítása
        this.startNewGame();
    }

    /**
     * Új játék indítása
     */
    startNewGame() {
        // Puzzle létrehozása és keverése
        this.puzzle = new SlidingPuzzle(this.size);
        
        // Keverési lépésszám nehézség szerint
        const shuffleMoves = {
            3: 30,
            4: 80,
            5: 150
        };
        this.puzzle.shuffle(shuffleMoves[this.size]);

        // Timer és számlálók reset
        this.stopTimer();
        this.seconds = 0;
        this.gameStarted = true;
        this.lastMoveTime = Date.now();
        
        // Hint rendszer reset
        this.hintSystem.reset();

        // Kezdeti távolság mentése
        this.previousDistance = this.solver.manhattanDistance(this.puzzle.getState(), this.size);

        // UI frissítése
        this.updateMoveCount();
        this.updateTimer();
        this.updateOptimalMoves();
        this.renderPuzzle();

        // Timer indítása
        this.startTimer();

        // Idle ellenőrzés
        this.startIdleCheck();

        // Üdvözlő üzenet
        this.showAIMessage(this.hintSystem.getWelcomeMessage(this.size));
    }

    /**
     * Puzzle megjelenítése
     */
    renderPuzzle() {
        this.puzzleGrid.innerHTML = '';
        this.puzzleGrid.className = `puzzle-grid size-${this.size}`;

        for (let row = 0; row < this.size; row++) {
            for (let col = 0; col < this.size; col++) {
                const value = this.puzzle.getTile(row, col);
                const tile = document.createElement('div');
                tile.className = 'tile';
                tile.dataset.row = row;
                tile.dataset.col = col;
                tile.dataset.value = value;

                if (value === 0) {
                    tile.classList.add('empty');
                } else {
                    tile.textContent = value;
                    
                    // Helyes pozícióban lévő csempék kiemelése
                    if (this.puzzle.isInCorrectPosition(row, col)) {
                        tile.classList.add('correct');
                    }

                    // Kattintás esemény
                    tile.addEventListener('click', () => this.handleTileClick(row, col));
                }

                // Highlighted csempe
                if (this.highlightedTile === value && value !== 0) {
                    tile.classList.add('highlight');
                }

                this.puzzleGrid.appendChild(tile);
            }
        }
    }

    /**
     * Csempe kattintás kezelése
     * @param {number} row - Sor
     * @param {number} col - Oszlop
     */
    handleTileClick(row, col) {
        if (!this.gameStarted) return;

        // Korábbi távolság mentése
        const prevDistance = this.solver.manhattanDistance(this.puzzle.getState(), this.size);

        if (this.puzzle.moveTile(row, col)) {
            this.lastMoveTime = Date.now();
            this.clearHighlight();

            // Új távolság
            const newDistance = this.solver.manhattanDistance(this.puzzle.getState(), this.size);
            const wasGoodMove = newDistance < prevDistance;
            this.previousDistance = newDistance;

            // UI frissítés
            this.updateMoveCount();
            this.updateOptimalMoves();
            this.renderPuzzle();

            // AI visszajelzés (nem minden lépésnél)
            if (Math.random() > 0.6) {
                const response = this.hintSystem.getMoveResponse(this.puzzle, wasGoodMove);
                if (response) {
                    this.showAIMessage(response);
                }
            }

            // Győzelem ellenőrzés
            if (this.puzzle.isSolved()) {
                this.handleWin();
            }
        }
    }

    /**
     * Billentyűzet kezelés (nyilak)
     * @param {KeyboardEvent} e - Billentyűzet esemény
     */
    handleKeyPress(e) {
        if (!this.gameStarted) return;

        const keyToDirection = {
            'ArrowUp': { dr: 1, dc: 0 },
            'ArrowDown': { dr: -1, dc: 0 },
            'ArrowLeft': { dr: 0, dc: 1 },
            'ArrowRight': { dr: 0, dc: -1 }
        };

        const dir = keyToDirection[e.key];
        if (dir) {
            e.preventDefault();
            const newRow = this.puzzle.emptyPos.row + dir.dr;
            const newCol = this.puzzle.emptyPos.col + dir.dc;
            
            if (this.puzzle.isValidPosition(newRow, newCol)) {
                this.handleTileClick(newRow, newCol);
            }
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
        this.finalMovesEl.textContent = this.puzzle.moveCount;
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

        const hint = this.hintSystem.generateDetailedHint(this.puzzle);
        this.showAIMessage(hint.message);
    }

    /**
     * Következő lépés megmutatása (vizuálisan kiemelt csempe)
     */
    showNextMove() {
        if (!this.gameStarted) return;

        const hint = this.hintSystem.generateHint(this.puzzle);
        
        if (hint.type === 'move') {
            this.highlightedTile = hint.tile;
            this.renderPuzzle();
            this.showAIMessage(hint.message);

            // Kiemelés automatikus eltávolítása 3 másodperc után
            setTimeout(() => {
                this.clearHighlight();
                this.renderPuzzle();
            }, 3000);
        } else {
            this.showAIMessage(hint.message);
        }
    }

    /**
     * Kiemelés törlése
     */
    clearHighlight() {
        this.highlightedTile = null;
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
     * Lépésszámláló frissítése
     */
    updateMoveCount() {
        this.moveCountEl.textContent = this.puzzle.moveCount;
    }

    /**
     * Optimális lépésszám frissítése
     */
    updateOptimalMoves() {
        if (!this.puzzle.isSolved()) {
            const solution = this.solver.solve(this.puzzle.getState(), this.size);
            if (solution) {
                this.optimalMovesEl.textContent = solution.moves.length;
            } else {
                this.optimalMovesEl.textContent = '?';
            }
        } else {
            this.optimalMovesEl.textContent = '0';
        }
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
    window.puzzleGame = new PuzzleGame();
});

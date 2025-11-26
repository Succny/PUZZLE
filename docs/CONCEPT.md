# PUZZLE - Kooperatív Puzzle Játék Koncepció

## 1. Játék Áttekintése

A PUZZLE egy web-alapú kooperatív puzzle játék, ahol a játékos és a mesterséges intelligencia együttműködésével oldható meg a feladat. A játék fő célja, hogy bemutassa az ember-AI együttműködés lehetőségeit egy szórakoztató formában.

## 2. Játékmenet

### 2.1 Alapvető Játéktípus: Csúszó Puzzle (Sliding Puzzle)
- Egy n×n-es rács (pl. 3×3, 4×4, 5×5)
- Számozott csempék, amelyeket sorrendbe kell rakni
- Egy üres hely, amellyel a csempék mozgathatók
- A cél: a csempéket megfelelő sorrendbe rendezni

### 2.2 AI Együttműködési Elemek

#### Hint Rendszer
Az AI a következő módokon segíti a játékost:

1. **Vizuális Hint**: Kiemeli a következő lépéshez szükséges csempét
2. **Szöveges Hint**: Elmagyarázza a stratégiát
3. **Lépésszámláló**: Megmutatja a megoldáshoz szükséges minimális lépésszámot
4. **Nehézség Értékelés**: Az aktuális helyzet bonyolultságát elemzi

#### Proaktív Segítség
- Ha a játékos egy ideig nem lép, az AI felajánlja a segítséget
- Felismeri, ha a játékos rossz irányba halad
- Motiváló üzenetek küldése

## 3. Nehézségi Szintek

| Szint | Rács | Keverés | AI Segítség |
|-------|------|---------|-------------|
| Könnyű | 3×3 | 10 lépés | Folyamatos |
| Közepes | 4×4 | 30 lépés | Kérésre |
| Nehéz | 5×5 | 50 lépés | Korlátozott |

## 4. Technológiai Stack

- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **AI Algoritmus**: A* keresési algoritmus a puzzle megoldásához
- **Hint Generálás**: Állapot-elemzés és optimális útvonal számítás

## 5. AI Komponensek

### 5.1 Megoldó Algoritmus (A* Search)
- Manhattan-távolság heurisztika
- Optimális megoldás keresése
- Lépések sorozatának generálása

### 5.2 Hint Generáló Modul
- Aktuális állapot elemzése
- Következő optimális lépés meghatározása
- Stratégiai tanácsok generálása

### 5.3 Játékos Viselkedés Elemző
- Elakadás felismerése
- Mintázatok azonosítása
- Adaptív segítségnyújtás

## 6. Felhasználói Felület

```
+----------------------------------+
|        PUZZLE - AI HINT          |
+----------------------------------+
|   [3×3] [4×4] [5×5]    [Új Játék]|
+----------------------------------+
|                                  |
|     +---+---+---+                |
|     | 1 | 2 | 3 |                |
|     +---+---+---+                |
|     | 4 | 5 | 6 |                |
|     +---+---+---+                |
|     | 7 | 8 |   |                |
|     +---+---+---+                |
|                                  |
+----------------------------------+
|  Lépések: 0    Idő: 00:00        |
+----------------------------------+
|  [🤖 Hint Kérése]                |
|                                  |
|  AI: "Próbáld a 8-as csempét     |
|       jobbra mozgatni!"          |
+----------------------------------+
```

## 7. Megvalósítási Terv

### Fázis 1: Alapok
- [x] Projekt struktúra
- [ ] Puzzle rács megjelenítése
- [ ] Csempe mozgatás logika

### Fázis 2: AI Integráció
- [ ] A* algoritmus implementálása
- [ ] Hint generálás
- [ ] Állapot elemzés

### Fázis 3: UI/UX
- [ ] Vonzó dizájn
- [ ] Animációk
- [ ] Reszponzív megjelenés

### Fázis 4: Továbbfejlesztések
- [ ] Többféle puzzle típus
- [ ] Eredmények mentése
- [ ] Hangeffektek

## 8. Tudományos Háttér

A projekt a következő területeket érinti:
- **Mesterséges Intelligencia**: Keresési algoritmusok, heurisztikák
- **Ember-Gép Interakció (HCI)**: Felhasználói élmény, segítségnyújtás
- **Játékfejlesztés**: Game design, UX patterns

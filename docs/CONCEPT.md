# SOKOBAN - Kooperatív AI Puzzle Játék Koncepció

## 1. Játék Áttekintése

A SOKOBAN egy web-alapú kooperatív logikai játék, ahol a játékos és a mesterséges intelligencia együttműködésével oldható meg a feladat. A játék fő célja, hogy bemutassa az ember-AI együttműködés lehetőségeit egy klasszikus, de kihívást jelentő játékban.

## 2. Játékmenet

### 2.1 Alapvető Játéktípus: Sokoban
- A játékos egy raktárost (📦 munkás) irányít
- Ládákat (📦) kell a célhelyekre (🎯) tolni
- Csak **tolni** lehet a ládákat, húzni nem
- Egyszerre csak egy ládát lehet mozgatni
- A cél: minden ládát a megfelelő célhelyre juttatni

### 2.2 Játékelemek

| Elem | Szimbólum | Leírás |
|------|-----------|--------|
| Fal | 🧱 | Átjárhatatlan akadály |
| Padló | ⬜ | Szabad terület |
| Játékos | 🧑 | A raktáros, akit irányítunk |
| Láda | 📦 | Tolandó objektum |
| Célhely | 🎯 | Ide kell a ládákat juttatni |
| Láda célhelyen | ✅ | Helyesen elhelyezett láda |

### 2.3 AI Együttműködési Elemek

#### Hint Rendszer
Az AI a következő módokon segíti a játékost:

1. **Vizuális Hint**: Kiemeli a következő lépés irányát
2. **Láda Kiemelés**: Megmutatja, melyik ládát kell mozgatni
3. **Deadlock Figyelmeztetés**: Jelzi, ha zsákutcába kerültünk
4. **Lépésszám Előrejelzés**: Megmutatja a hátralévő lépések számát
5. **Visszalépés Javaslat**: Felajánlja az undo-t, ha elrontottuk

#### Proaktív Segítség
- Ha a játékos egy ideig nem lép, az AI felajánlja a segítséget
- Felismeri a deadlock (zsákutca) helyzeteket
- Motiváló üzenetek és bátorítás

## 3. Nehézségi Szintek

| Szint | Pálya Méret | Ládák | AI Segítség |
|-------|-------------|-------|-------------|
| Könnyű | 7×7 | 1-2 | Folyamatos |
| Közepes | 9×9 | 3-4 | Kérésre |
| Nehéz | 11×11 | 5+ | Korlátozott |

## 4. Technológiai Stack

- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **AI Algoritmus**: BFS/A* keresés a megoldáshoz
- **Deadlock Detektálás**: Sarok és vonal deadlock felismerés
- **Hint Generálás**: Állapot-elemzés és optimális útvonal számítás

## 5. AI Komponensek

### 5.1 Megoldó Algoritmus
- BFS (Breadth-First Search) kisebb pályákhoz
- A* keresés Manhattan-távolság heurisztikával
- Állapottér kezelés és duplikáció szűrés

### 5.2 Deadlock Detektálás
- **Sarok deadlock**: Láda sarokba szorul
- **Vonal deadlock**: Láda fal mellett ragad
- **Freeze deadlock**: Ládák egymást blokkolják

### 5.3 Hint Generáló Modul
- Aktuális állapot elemzése
- Következő optimális lépés meghatározása
- Stratégiai tanácsok generálása

### 5.4 Játékos Viselkedés Elemző
- Elakadás felismerése
- Hibás lépések azonosítása
- Adaptív segítségnyújtás

## 6. Felhasználói Felület

```
+----------------------------------+
|      SOKOBAN - AI ASSZISZTENS    |
+----------------------------------+
| [1] [2] [3] [4] [5]   [Újra] [↩] |
+----------------------------------+
|                                  |
|   🧱🧱🧱🧱🧱🧱🧱                 |
|   🧱⬜⬜🎯⬜⬜🧱                 |
|   🧱⬜📦⬜📦⬜🧱                 |
|   🧱⬜⬜🧑⬜⬜🧱                 |
|   🧱⬜⬜🎯⬜⬜🧱                 |
|   🧱🧱🧱🧱🧱🧱🧱                 |
|                                  |
+----------------------------------+
|  Lépések: 0    Tolások: 0        |
+----------------------------------+
|  [🤖 Hint Kérése] [👀 Megoldás]  |
|                                  |
|  AI: "Told a felső ládát lefelé  |
|       a célhely felé!"           |
+----------------------------------+
```

## 7. Megvalósítási Terv

### Fázis 1: Alapok
- [x] Projekt struktúra
- [x] Sokoban pálya megjelenítése
- [x] Játékos és láda mozgatás logika
- [x] Pályák (levels) létrehozása

### Fázis 2: AI Integráció
- [x] Megoldó algoritmus implementálása
- [x] Deadlock detektálás
- [x] Hint generálás

### Fázis 3: UI/UX
- [x] Vonzó dizájn
- [x] Animációk
- [x] Reszponzív megjelenés

### Fázis 4: Továbbfejlesztések
- [ ] Több pálya
- [ ] Eredmények mentése
- [ ] Pálya szerkesztő

## 8. Tudományos Háttér

A projekt a következő területeket érinti:
- **Mesterséges Intelligencia**: Keresési algoritmusok, heurisztikák, deadlock detektálás
- **Ember-Gép Interakció (HCI)**: Felhasználói élmény, intelligens segítségnyújtás
- **Játékfejlesztés**: Game design, UX patterns
- **Kombinatorikus Optimalizálás**: NP-nehéz problémák

## 9. Miért Sokoban?

A Sokoban különösen alkalmas az ember-AI együttműködés bemutatására:
1. **NP-nehéz probléma**: Az AI segítség valódi értéket ad
2. **Visszafordíthatatlan lépések**: A deadlock detektálás fontos
3. **Stratégiai gondolkodás**: Komplex tervezést igényel
4. **Klasszikus játék**: Jól ismert és népszerű

# 📦 SOKOBAN - Kooperatív AI Puzzle Játék

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Kooperatív Sokoban puzzle játék mesterséges intelligencia hint-rendszerrel.

## 📋 Projekt Leírás

Ez a projekt egy BSc szakdolgozat keretében készült, amely a **mesterséges intelligencia és ember együttműködésén alapuló játékfejlesztést** mutatja be.

A Sokoban egy klasszikus logikai játék, ahol a játékosnak ládákat kell a célhelyekre tolnia. Ez a verzió egy AI asszisztenst tartalmaz, amely segíti a játékost hint-ekkel és stratégiai tanácsokkal.

### Főbb Jellemzők

- 🎮 **Klasszikus Sokoban**: Told a ládákat a célhelyekre
- 🤖 **AI Asszisztens**: Intelligens hint-rendszer segít, ha elakadsz
- ⚠️ **Deadlock Detektálás**: Figyelmeztet, ha zsákutcába kerültél
- 📊 **Állapot Elemzés**: A játék folyamatosan elemzi a helyzetet
- ↩️ **Undo Funkció**: Bármikor visszaléphetsz
- 📱 **Reszponzív Design**: Működik asztali gépen és mobilon is

## 🚀 Indítás

### Egyszerű Indítás

Nyisd meg a `src/index.html` fájlt bármelyik modern böngészőben:

```bash
# Linux/Mac
open src/index.html

# Windows
start src/index.html

# Vagy használj Live Server-t VS Code-ban
```

### Helyi Szerver (opcionális)

```bash
# Python 3
cd src
python -m http.server 8000

# Node.js
npx serve src

# Majd nyisd meg: http://localhost:8000
```

## 🎮 Játékmenet

### Szabályok
- Irányítsd a raktárost (🧑) a pályán
- Told a ládákat (📦) a célhelyekre (🎯)
- Csak **tolni** lehet a ládákat, húzni nem
- Egyszerre csak egy ládát lehet mozgatni
- Ha minden láda célhelyen van (✅), nyertél!

### Irányítás
- ⬆️⬇️⬅️➡️ **Nyílbillentyűk** vagy **WASD**: Mozgás
- **Z** vagy **Backspace**: Visszalépés (Undo)
- **R**: Pálya újraindítása

## 🤖 AI Funkciók

### Hint Rendszer
- **💡 Hint Kérése**: Állapot elemzés és stratégiai tanácsok
- **👀 Következő lépés**: Vizuálisan kiemeli a javasolt irányt

### AI Képességek
- **Megoldó Algoritmus**: BFS/A* keresés az optimális megoldáshoz
- **Deadlock Detektálás**: Felismeri a zsákutca helyzeteket
- **Proaktív Segítség**: Ha elakadsz, felajánlja a segítséget

## 📁 Projekt Struktúra

```
PUZZLE/
├── src/
│   ├── index.html          # Fő HTML oldal
│   ├── css/
│   │   └── style.css       # Stílusok
│   └── js/
│       ├── levels.js       # Pályák definíciói
│       ├── sokoban.js      # Sokoban játék logika
│       ├── ai-solver.js    # AI megoldó algoritmus
│       ├── hint-system.js  # Hint rendszer
│       └── game.js         # Játék vezérlő
├── docs/
│   └── CONCEPT.md          # Részletes koncepció
├── README.md               # Ez a fájl
└── .gitignore              # Git ignore szabályok
```

## 🧠 Technológiai Háttér

### Használt Technológiák
- **HTML5**: Szemantikus markup
- **CSS3**: Modern stílusok, animációk, Grid layout
- **JavaScript (ES6+)**: Játék logika és AI

### AI Algoritmusok
- **BFS (Breadth-First Search)**: Megoldás keresése
- **A* keresés**: Heurisztikus optimalizálás
- **Manhattan-távolság**: Heurisztika a célállapottól való távolsághoz
- **Deadlock detektálás**: Sarok és vonal deadlock felismerés

## 📚 Dokumentáció

Részletes dokumentáció a `docs/` mappában:
- [CONCEPT.md](docs/CONCEPT.md) - Játék koncepció és tervezési dokumentum

## 🎓 Tudományos Háttér

A projekt a következő területeket érinti:
- **Mesterséges Intelligencia**: Keresési algoritmusok, heurisztikák
- **Ember-Gép Interakció (HCI)**: Felhasználói élmény, intelligens segítségnyújtás
- **Játékfejlesztés**: Game design, UX patterns
- **Kombinatorikus Optimalizálás**: NP-nehéz problémák

### Miért Sokoban?

A Sokoban különösen alkalmas az ember-AI együttműködés bemutatására:
1. **NP-nehéz probléma**: Az AI segítség valódi értéket ad
2. **Visszafordíthatatlan lépések**: A deadlock detektálás fontos
3. **Stratégiai gondolkodás**: Komplex tervezést igényel
4. **Klasszikus játék**: Jól ismert és népszerű

## 📝 Licensz

MIT License - Szabadon használható és módosítható.

## 👨‍💻 Szerző

BSc Szakdolgozat - Mesterséges Intelligencia és Ember Együttműködése

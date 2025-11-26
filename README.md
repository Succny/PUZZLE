# 🧩 PUZZLE - Kooperatív AI Puzzle Játék

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

Kooperatív csúszó puzzle játék mesterséges intelligencia hint-rendszerrel.

## 📋 Projekt Leírás

Ez a projekt egy BSc szakdolgozat keretében készült, amely a **mesterséges intelligencia és ember együttműködésén alapuló játékfejlesztést** mutatja be.

### Főbb Jellemzők

- 🎮 **Klasszikus Csúszó Puzzle**: Rendezd a számokat 1-től n²-1-ig
- 🤖 **AI Asszisztens**: Intelligens hint-rendszer segít, ha elakadsz
- 📊 **Állapot Elemzés**: A játék folyamatosan elemzi a helyzetet
- 🎯 **Optimális Megoldás**: A* algoritmus számítja ki a legrövidebb utat
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

1. **Válassz nehézségi szintet**: 3×3 (könnyű), 4×4 (közepes), 5×5 (nehéz)
2. **Kattints az "Új Játék" gombra** a puzzle keveréséhez
3. **Mozgasd a csempéket** kattintással vagy nyílbillentyűkkel
4. **Kérj hint-et** az AI-tól, ha elakadtál
5. **Cél**: Rendezd a számokat sorrendbe!

## 🤖 AI Funkciók

### Hint Rendszer
- **💡 Hint Kérése**: Általános stratégiai tanácsok és állapot elemzés
- **👀 Mutasd a következő lépést**: Vizuálisan kiemeli a javasolt csempét

### A* Megoldó Algoritmus
- Manhattan-távolság + lineáris konfliktus heurisztika
- Optimális megoldás keresése
- Lépések számának előrejelzése

### Proaktív Segítség
- Felismeri, ha a játékos elakadt
- Automatikus motivációs üzenetek
- Visszajelzés a lépésekről

## 📁 Projekt Struktúra

```
PUZZLE/
├── src/
│   ├── index.html          # Fő HTML oldal
│   ├── css/
│   │   └── style.css       # Stílusok
│   └── js/
│       ├── puzzle.js       # Puzzle logika
│       ├── ai-solver.js    # A* megoldó algoritmus
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
- **A* (A-star) keresés**: Optimális útvonal keresése
- **Manhattan-távolság**: Heurisztika a célállapottól való távolsághoz
- **Lineáris konfliktus**: Továbbfejlesztett heurisztika

## 📚 Dokumentáció

Részletes dokumentáció a `docs/` mappában:
- [CONCEPT.md](docs/CONCEPT.md) - Játék koncepció és tervezési dokumentum

## 🎓 Tudományos Háttér

A projekt a következő területeket érinti:
- **Mesterséges Intelligencia**: Keresési algoritmusok, heurisztikák
- **Ember-Gép Interakció (HCI)**: Felhasználói élmény, segítségnyújtás
- **Játékfejlesztés**: Game design, UX patterns

## 📝 Licensz

MIT License - Szabadon használható és módosítható.

## 👨‍💻 Szerző

BSc Szakdolgozat - Mesterséges Intelligencia és Ember Együttműködése

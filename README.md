# 📦 SOKOBAN - Kooperatív AI Puzzle Játék

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)

Kooperatív Sokoban puzzle játék mesterséges intelligencia hint-rendszerrel.

## 📋 Projekt Leírás

Ez a projekt egy BSc szakdolgozat keretében készült, amely a **mesterséges intelligencia és ember együttműködésén alapuló játékfejlesztést** mutatja be.

A Sokoban egy klasszikus logikai játék, ahol a játékosnak ládákat kell a célhelyekre tolnia. Ez a verzió egy AI asszisztenst tartalmaz, amely segíti a játékost hint-ekkel és stratégiai tanácsokkal.

### Főbb Jellemzők

- 🎮 **Klasszikus Sokoban**: Told a ládákat a célhelyekre
- 🤖 **AI Asszisztens**: Intelligens hint-rendszer segít, ha elakadsz
- ⚠️ **Deadlock Detektálás**: Figyelmeztet, ha zsákutcába kerültél
- ↩️ **Undo Funkció**: Bármikor visszaléphetsz
- 📊 **Állapot Elemzés**: A játék folyamatosan elemzi a helyzetet
- 🖥️ **Konzol Felület**: Tiszta, áttekinthető konzol UI

## 🚀 Indítás

### Követelmények

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) vagy újabb

### Futtatás

```bash
# Klónozás
git clone https://github.com/Succny/PUZZLE.git
cd PUZZLE

# Futtatás
dotnet run --project Sokoban
```

### Build

```bash
# Release build
dotnet build -c Release

# Futtatható fájl létrehozása
dotnet publish -c Release -r win-x64 --self-contained
```

## 🎮 Játékmenet

### Szabályok
- Irányítsd a raktárost (@) a pályán
- Told a ládákat ([]) a célhelyekre (..)
- Csak **tolni** lehet a ládákat, húzni nem
- Egyszerre csak egy ládát lehet mozgatni
- Ha minden láda célhelyen van (▣▣), nyertél!

### Irányítás

| Billentyű | Funkció |
|-----------|---------|
| ↑ ↓ ← → / WASD | Mozgás |
| H | Részletes hint kérése |
| N | Következő lépés megmutatása |
| U / Backspace | Visszalépés (Undo) |
| R | Pálya újraindítása |
| 1-5 | Pálya választás |
| Q / Esc | Kilépés |

## 🤖 AI Funkciók

### Hint Rendszer
- **H - Részletes Hint**: Állapot elemzés, megoldhatóság ellenőrzése
- **N - Következő lépés**: Megmutatja az optimális következő lépést

### AI Képességek
- **BFS/A* Megoldó Algoritmus**: Megtalálja az optimális megoldást
- **Manhattan-távolság Heurisztika**: Hatékony keresés
- **Deadlock Detektálás**: Felismeri a zsákutca helyzeteket
- **Proaktív Segítség**: Stratégiai tanácsok

## 📁 Projekt Struktúra

```
PUZZLE/
├── Sokoban/
│   ├── Program.cs          # Belépési pont
│   ├── ConsoleUI.cs        # Konzol felhasználói felület
│   ├── SokobanGame.cs      # Játék logika
│   ├── AISolver.cs         # AI megoldó algoritmus
│   ├── HintSystem.cs       # Hint rendszer
│   ├── Levels.cs           # Pályák definíciói
│   └── Sokoban.csproj      # Projekt fájl
├── Sokoban.sln             # Solution fájl
├── README.md               # Ez a fájl
└── .gitignore              # Git ignore szabályok
```

## 🧠 Technológiai Háttér

### Használt Technológiák
- **C# 12**: Modern C# nyelvi funkciók
- **.NET 8.0**: Cross-platform futtatókörnyezet
- **Console Application**: Konzol alapú felület

### AI Algoritmusok
- **BFS (Breadth-First Search)**: Megoldás keresése
- **A* keresés**: Heurisztikus optimalizálás
- **Manhattan-távolság**: Heurisztika a célállapottól való távolsághoz
- **Deadlock detektálás**: Sarok és vonal deadlock felismerés

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

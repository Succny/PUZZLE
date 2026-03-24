# 📦 SOKOBAN - Kooperatív AI Puzzle Játék

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![Build and Test](https://github.com/Succny/PUZZLE/actions/workflows/dotnet.yml/badge.svg)](https://github.com/Succny/PUZZLE/actions/workflows/dotnet.yml)

Kooperatív Sokoban puzzle játék mesterséges intelligencia hint-rendszerrel.

## 📋 Projekt Leírás

Ez a projekt egy BSc szakdolgozat keretében készült, amely a **mesterséges intelligencia és ember együttműködésén alapuló játékfejlesztést** mutatja be.

A Sokoban egy klasszikus logikai játék, ahol a játékosnak ládákat kell a célhelyekre tolnia. Ez a verzió egy AI asszisztenst tartalmaz, amely segíti a játékost hint-ekkel és stratégiai tanácsokkal.

### Főbb Jellemzők

- 🎮 **Klasszikus Sokoban**: Told a ládákat a célhelyekre
- 🤖 **AI Asszisztens**: Intelligens hint-rendszer segít, ha elakadsz
- ⚠️ **Deadlock Detektálás**: Figyelmeztet, ha zsákutcába kerültél (sarok és fal-vonal deadlock)
- ↩️ **Undo Funkció**: Bármikor visszaléphetsz (max. 1000 lépés)
- 📊 **Állapot Elemzés**: A játék folyamatosan elemzi a helyzetet
- 🖥️ **Konzol Felület**: Tiszta, áttekinthető konzol UI

## 🏗️ Architektúra

A projekt rétegezett felépítésű, ahol minden fájl egyértelműen egy-egy réteghez tartozik:

### Core Réteg (Játékmotor)
- **SokobanGame**: Játéklogika, állapotkezelés, mozgások
- **Levels**: Pályadefiníciók és szintek
- **Tiles**: Csempe konstansok (fal, padló, láda, cél, játékos)
- **MoveResult, GameState**: Eredmény és állapot osztályok
- Undo stack kezelése (TrimHistory)
- Deadlock detektálás (IsCornerDeadlock, IsWallLineDeadlock)

### AI Réteg
- **AISolver**: A* algoritmussal megoldás keresés
  - Manhattan-távolság heurisztika
  - Visited állapottér (HashSet)
  - Konfigurálható MaxIterations paraméter
  - Deadlock állapotok szűrése
- **HintSystem**: Játékos segítése
  - Stratégiai tippek
  - Lépésjavaslatok
  - Állapotelemzés
  - Hint statisztikák (HintsUsed)
- **Messages**: Lokalizálható üzenetek

### UI Réteg (Prezentáció)
- **ConsoleUI**: Konzol megjelenítés és input kezelés
  - Render metódusok: RenderHeader, RenderLevelSelector, RenderGameArea, RenderStats, RenderAIPanel, RenderMessagePanel, RenderControls
  - Input handler metódusok: HandleMovementInput, HandleUndoInput, HandleHintInput, HandleGameControlInput
- **ConsoleSizing**: Konzol ablak/buffer méretezés és stabilizáció
- **Program**: Alkalmazás belépési pont

## 🚀 Indítás

### Követelmények

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) vagy újabb

### Futtatás Visual Studio-ból

1. Nyisd meg a `Sokoban.sln` fájlt Visual Studio 2022-ben (vagy újabb verzióban)
2. Állítsd be a `Sokoban` projektet startup projektnek
3. Nyomj F5-öt a futtatáshoz

### Futtatás parancssorból

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
| H | Segítség - következő lépés megmutatása |
| N | Állapot elemzés - részletes információ |
| F | AI javasolt lépésének végrehajtása (Follow AI) |
| U / Backspace | Visszalépés (Undo) |
| R | Pálya újraindítása |
| 1-5 | Pálya választás |
| Q / Esc | Kilépés |

## 🤖 AI Funkciók

### Hint Rendszer
- **H - Segítség**: Megmutatja az optimális következő lépést, vagy jelzi ha a pálya teljesítve van
- **N - Állapot Elemzés**: Részletes információ a játék állapotáról, megoldhatóság ellenőrzése
- **F - Follow AI**: Az utolsó javasolt AI lépés végrehajtása

### AI Képességek
- **A* Megoldó Algoritmus**: Megtalálja az optimális megoldást
- **Manhattan-távolság Heurisztika**: Hatékony keresés
- **Deadlock Detektálás**: Felismeri a zsákutca helyzeteket
  - Sarok deadlock (corner deadlock)
  - Fal-vonal deadlock (wall line deadlock)
- **Proaktív Segítség**: Stratégiai tanácsok

### Hint Statisztikák

A HintSystem osztály számolja, hányszor kért a játékos segítséget (`HintsUsed` property), a UI pedig követi az AI által végrehajtott lépések számát (`AI` statisztika). Ezek az adatok felhasználhatók a szakdolgozatban a játékos–AI együttműködés elemzéséhez:
- Milyen gyakran támaszkodik a játékos az AI-ra?
- Mely pályákon kér több segítséget?
- Hogyan változik a hint-használat a gyakorlással?

## 📁 Projekt Struktúra

```
PUZZLE/
├── .github/
│   └── workflows/
│       └── dotnet.yml          # CI/CD workflow
├── Sokoban/
│   ├── Program.cs              # [UI] Belépési pont
│   ├── ConsoleUI.cs            # [UI] Konzol felhasználói felület
│   ├── ConsoleSizing.cs        # [UI] Konzol ablak/buffer méretezés
│   ├── SokobanGame.cs          # [Core] Játék logika
│   ├── Levels.cs               # [Core] Pályák és csempék definíciói
│   ├── AISolver.cs             # [AI] AI megoldó algoritmus
│   ├── HintSystem.cs           # [AI] Hint rendszer
│   ├── Messages.cs             # [AI] Lokalizálható üzenetek
│   └── Sokoban.csproj          # Projekt fájl
├── Sokoban.Tests/              # Egységtesztek
│   ├── SokobanGameTests.cs     # Játéklogika tesztek
│   ├── AISolverTests.cs        # AI algoritmus tesztek
│   ├── HintSystemTests.cs      # Hint rendszer tesztek
│   └── Sokoban.Tests.csproj    # Teszt projekt fájl
├── Sokoban.sln                 # Solution fájl
├── README.md                   # Ez a fájl
└── .gitignore                  # Git ignore szabályok
```

## 🧪 Tesztelés

A projekt xUnit alapú egységteszteket tartalmaz:

```bash
# Tesztek futtatása
dotnet test

# Tesztek részletes kimenettel
dotnet test --verbosity normal

# Coverage riport generálása
dotnet test --collect:"XPlat Code Coverage"
```

## 🔄 CI/CD

A projekt GitHub Actions-t használ folyamatos integrációhoz:
- Automatikus build minden push és pull request esetén
- Egységtesztek futtatása
- Teszt és coverage riportok

## 🧠 Technológiai Háttér

### Használt Technológiák
- **C# 12**: Modern C# nyelvi funkciók
- **.NET 10.0**: Cross-platform futtatókörnyezet
- **xUnit**: Egységtesztelési keretrendszer
- **Console Application**: Konzol alapú felület

### Design Patternek
- **Strategy Pattern**: HintSystem különböző hint stratégiái
- **Template Method**: AISolver keresési algoritmus felépítése

### AI Algoritmusok
- **A* keresés**: Heurisztikus optimalizálás prioritásos sorral
- **Manhattan-távolság**: Heurisztika a célállapottól való távolsághoz
- **Deadlock detektálás**: 
  - Sarok deadlock: láda sarokba szorult (két szomszédos oldalon fal)
  - Fal-vonal deadlock: láda fal mentén, ahol nincs cél a vonalon

## 🎓 Tudományos Háttér

A projekt a következő területeket érinti:
- **Mesterséges Intelligencia**: Keresési algoritmusok, heurisztikák
- **Ember-Gép Interakció (HCI)**: Felhasználói élmény, intelligens segítségnyújtás
- **Játékfejlesztés**: Game design, UX patterns
- **Kombinatorikus Optimalizálás**: NP-nehéz problémák
- **Szoftverarchitektúra**: Rétegezett architektúra, SOLID elvek

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

# Conway's Game of Life – Konzept & erwartete Klassenstruktur

Die Tests in `GameOfLife.Tests/` sind **Scaffolding-Tests** (Test-First): Sie beschreiben das
erwartete Verhalten, bevor die Implementierung existiert. Dieses Dokument beschreibt, welche
Klassen und Member die Tests voraussetzen. Code ist bewusst nicht enthalten.

Reines C# (.NET 8), keine Unity-Abhängigkeit.

## Projektstruktur

```
GameOfLife/
├── GameOfLife/                 ← Klassenbibliothek (noch anzulegen)
│   └── GameOfLife.csproj
└── GameOfLife.Tests/           ← NUnit-Tests (vorhanden)
    ├── GameOfLife.Tests.csproj   (referenziert ../GameOfLife/GameOfLife.csproj)
    ├── BoardTestHelper.cs
    ├── CellRuleTests.cs          3. Single-Cell-Tests
    ├── GameStepTests.cs          1. Spielzug-Tests
    └── GridViewTests.cs          2. UI-Tests
```

Bibliothek anlegen und Tests ausführen:

```bash
dotnet new classlib -n GameOfLife -o GameOfLife/GameOfLife -f net8.0
dotnet test GameOfLife/GameOfLife.Tests
```

Alle Klassen liegen im Namespace **`GameOfLife`** und sind `public`.

## Die Regeln (Conway)

| # | Ausgangszustand | Lebende Nachbarn | Nächste Generation |
|---|-----------------|------------------|--------------------|
| 1 | lebend          | < 2              | stirbt (Unterbevölkerung) |
| 2 | lebend          | 2 oder 3         | überlebt |
| 3 | lebend          | > 3              | stirbt (Überbevölkerung) |
| 4 | tot             | genau 3          | wird lebendig (Reproduktion) |

In allen anderen Fällen bleibt eine tote Zelle tot.

## Architektur

```
GameController ──► Board.Step()        (Logik)
       │
       └─────────► GridView.Refresh()  (Darstellung)

Board.Step() ──► GameRules.NextState() für jede Zelle
```

Logik (`GameRules`, `Board`) und Darstellung (`GridView`) sind getrennt; der `GameController`
verbindet beide.

## Klassen

### `GameRules` (statische Klasse)

Reine Regel-Logik für **eine** Zelle, kennt kein Board.

| Member | Beschreibung |
|--------|--------------|
| `bool NextState(bool isAlive, int liveNeighbours)` | Zustand der Zelle in der nächsten Generation gemäß den vier Regeln. |

### `Board`

Das Spielfeld als Raster aus lebenden/toten Zellen.

| Member | Beschreibung |
|--------|--------------|
| `Board(int width, int height)` | Erstellt ein Feld; **alle Zellen sind anfangs tot**. |
| `int Width { get; }` | Breite (Anzahl Spalten, x). |
| `int Height { get; }` | Höhe (Anzahl Zeilen, y). |
| `bool IsAlive(int x, int y)` | Ist die Zelle lebendig? |
| `void SetAlive(int x, int y, bool alive)` | Setzt den Zustand einer Zelle. |
| `int CountLiveNeighbours(int x, int y)` | Anzahl lebender Nachbarn in der 8er-Nachbarschaft. |
| `void Step()` | Führt einen Spielzug aus. |

Erwartetes Verhalten:

- **Koordinaten:** `x` = Spalte, `y` = Zeile, Ursprung `(0,0)` oben links. x und y dürfen nicht vertauscht werden.
- **Nachbarzählung:** alle 8 umliegenden Zellen (inkl. Diagonalen); die Zelle selbst zählt nicht.
- **Ränder:** kein Wrap-around. Zellen außerhalb des Feldes gelten als tot.
- **`Step()`:** alle Zellen werden **gleichzeitig** auf Basis des alten Zustands berechnet
  (d. h. nicht Zelle für Zelle „in place“ überschreiben). Breite und Höhe bleiben unverändert.

### `GridView`

Unity-freie Darstellung des Boards als Zeichenraster. Das Grid ist ein **eigener Zwischenspeicher**:
Es zeigt den Board-Zustand vom letzten `Initialize`/`Refresh`, nicht live.

| Member | Beschreibung |
|--------|--------------|
| `GridView(char aliveSymbol, char deadSymbol)` | Legt fest, mit welchen Zeichen lebende/tote Zellen dargestellt werden. |
| `Board Board { get; }` | Das dargestellte Board (gesetzt durch `Initialize`). |
| `void Initialize(Board board)` | Verbindet das View mit dem Board und übernimmt dessen aktuellen Zustand. |
| `void Refresh()` | Überträgt den aktuellen Board-Zustand in die Darstellung. |
| `char GetCellSymbol(int x, int y)` | Aktuell angezeigtes Zeichen der Zelle. |
| `string Render()` | Gesamtes Grid als Text. |

Format von `Render()`: eine Textzeile pro Board-Zeile (y), je `Width` Zeichen,
Zeilen mit `\n` getrennt, **kein** abschließendes `\n`. Beispiel (`'X'` lebend, `'.'` tot):

```
.....
..X..
..X..
..X..
.....
```

### `GameController`

Verbindet Logik und Darstellung.

| Member | Beschreibung |
|--------|--------------|
| `GameController(GridView gridView)` | Erhält das (bereits initialisierte) View. |
| `Board Board { get; }` | Liefert das Board des Views (dieselbe Instanz). |
| `void NextGeneration()` | Genau ein `Board.Step()`, danach `GridView.Refresh()`. |

## Was die Tests prüfen

### 1. Spielzug-Tests (`GameStepTests`)

- Jede der vier Regeln in einem echten Spielzug
- Leeres Feld bleibt leer
- Block (Still Life) bleibt unverändert
- Blinker wechselt zwischen waagrecht und senkrecht
- Gleichzeitige Berechnung aller Zellen
- Glider wandert nach 4 Generationen um eine Zelle diagonal
- Feldgröße bleibt gleich, Ränder werden nicht umgebrochen

### 2. UI-Tests (`GridViewTests`)

- Nach `Initialize` zeigt das Grid den Startzustand in Board-Größe
- Nach `NextGeneration` hat sich das Grid geändert und zeigt den neuen Zustand
- Gestorbene, geborene und überlebende Zellen zeigen das richtige Symbol
- Grid stimmt über mehrere Generationen Zelle für Zelle mit dem Board überein
- `NextGeneration` führt genau einen Schritt aus
- Bei stabilem Muster bleibt das Grid gleich
- `Refresh` übernimmt manuelle Board-Änderungen
- `GameController.Board` ist dieselbe Instanz wie das Board des Views

### 3. Single-Cell-Tests (`CellRuleTests`)

- `NextState` für alle Nachbarzahlen 0–8, für lebende und tote Zellen
- Neues Board ist komplett tot; `SetAlive`/`IsAlive` ändern nur die Zielzelle
- Nachbarzählung: alle 8 Richtungen, ohne die Zelle selbst, ohne entferntere Zellen,
  Zellen außerhalb gelten als tot, kein Wrap-around

## Test-Hilfsmittel

`BoardTestHelper` baut Boards aus lesbaren Text-Mustern und vergleicht sie:
`'X'` = lebend, `'.'` = tot, jede Zeichenkette ist eine Zeile (y), jedes Zeichen eine Spalte (x).
Er nutzt nur `Board(width, height)`, `SetAlive`, `IsAlive`, `Width` und `Height`.

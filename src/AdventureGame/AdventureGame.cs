namespace AdventureGame;

public class AdventureGame
{
    // ── Input keys ───────────────────────────────────────────────
    public readonly string GO_NORTH   = "W";
    public readonly string GO_SOUTH   = "S";
    public readonly string GO_EAST    = "D";
    public readonly string GO_WEST    = "A";
    public readonly string GET_LAMP   = "L";
    public readonly string GET_KEY    = "K";
    public readonly string OPEN_CHEST = "O";
    public readonly string QUIT       = "Q";

    // ── Grid dimensions ──────────────────────────────────────────
    public const int ROWS = 3;
    public const int COLS = 4;

    // ── Dungeon & player state ───────────────────────────────────
    private Adventurer adventurer;
    private Room[,]    dungeon;
    private int  aRow, aCol;
    private bool isChestOpen;
    private bool hasPlayerQuit;
    private bool isAdventureAlive;
    private string lastDirection;

    // ── Grue state ───────────────────────────────────────────────
    public static bool GrueActive = false;
    public static int  GrueRow    = -1;
    public static int  GrueCol    = -1;
    private bool grueCaught = false;

    // ── Fixed room coordinates ───────────────────────────────────
    // Exit  → top-left   [0,0]
    // Start → bot-right  [2,3]
    // Chest → mid-left   [1,0]
    // Key   → top-right  [0,3]
    // Lamp  → mid-center [1,2]
    // Grue spawns at     [0,2]  (opposite side from exit)
    private const int EXIT_ROW  = 0, EXIT_COL  = 0;
    private const int START_ROW = 2, START_COL = 3;
    private const int CHEST_ROW = 1, CHEST_COL = 0;
    private const int KEY_ROW   = 0, KEY_COL   = 3;
    private const int LAMP_ROW  = 1, LAMP_COL  = 2;
    private const int GRUE_SPAWN_ROW = 0, GRUE_SPAWN_COL = 2;

    public AdventureGame() { }

    // ════════════════════════════════════════════════════════════
    //  GAME LOOP
    // ════════════════════════════════════════════════════════════
    public void Start()
    {
        Init();
        ShowGameStartScreen();

        string input;
        do
        {
            ShowScene();
            do
            {
                ShowInputOptions();
                input = GetInput();
            }
            while (!IsValidInput(input));

            ProcessInput(input);
            UpdateGameState();
        }
        while (!IsGameOver());

        ShowGameOverScreen();
    }

    // ════════════════════════════════════════════════════════════
    //  INIT — 4×3 dungeon
    //
    //   [0,0]EXIT ─── [0,1] ─── [0,2] ─── [0,3]KEY
    //     │                       │           │
    //   [1,0]CHEST ─ [1,1] ─── [1,2]LAMP ─ [1,3]
    //     │            │                      │
    //   [2,0] ─────  [2,1] ─── [2,2] ─────  [2,3]START
    //
    // ════════════════════════════════════════════════════════════
    private void Init()
    {
        adventurer       = new Adventurer();
        isChestOpen      = false;
        hasPlayerQuit    = false;
        isAdventureAlive = true;
        lastDirection    = string.Empty;
        GrueActive       = false;
        GrueRow          = -1;
        GrueCol          = -1;
        grueCaught       = false;

        dungeon = new Room[ROWS, COLS];
        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
                dungeon[r, c] = new Room();

        // ── Room names ────────────────────────────────────────────
        dungeon[0,0].SetDescription("Entrada");    // EXIT
        dungeon[0,1].SetDescription("Armería");
        dungeon[0,2].SetDescription("Biblioteca");
        dungeon[0,3].SetDescription("Torreón");
        dungeon[1,0].SetDescription("Cripta");     // CHEST
        dungeon[1,1].SetDescription("Capilla");
        dungeon[1,2].SetDescription("Bodega");     // LAMP
        dungeon[1,3].SetDescription("Mazmorra");
        dungeon[2,0].SetDescription("Catacumba");
        dungeon[2,1].SetDescription("Sótano");
        dungeon[2,2].SetDescription("Cisterna");
        dungeon[2,3].SetDescription("Cámara");     // START

        // ── Lit rooms ────────────────────────────────────────────
        dungeon[EXIT_ROW,  EXIT_COL ].SetLit(true);   // exit always lit
        dungeon[0,1]                  .SetLit(true);
        dungeon[1,1]                  .SetLit(true);
        dungeon[START_ROW, START_COL].SetLit(true);   // start lit

        // ── Items ─────────────────────────────────────────────────
        dungeon[KEY_ROW,   KEY_COL  ].SetKey  (true);
        dungeon[LAMP_ROW,  LAMP_COL ].SetLamp (true);
        dungeon[CHEST_ROW, CHEST_COL].SetChest(true);

        // ── Mark exit ─────────────────────────────────────────────
        dungeon[EXIT_ROW, EXIT_COL].SetExit(true);

        // ── Horizontal connections (E/W) ──────────────────────────
        ConnectEW(0,0, 0,1);
        ConnectEW(0,1, 0,2);
        ConnectEW(0,2, 0,3);
        // row 1: gap between [1,0] and [1,1] intentional — forces detour
        ConnectEW(1,1, 1,2);
        ConnectEW(1,2, 1,3);
        ConnectEW(2,0, 2,1);
        ConnectEW(2,1, 2,2);
        ConnectEW(2,2, 2,3);

        // ── Vertical connections (N/S) ────────────────────────────
        ConnectNS(0,0, 1,0);
        ConnectNS(0,2, 1,2);
        ConnectNS(0,3, 1,3);
        ConnectNS(1,0, 2,0);
        ConnectNS(1,1, 2,1);
        ConnectNS(1,3, 2,3);

        // ── Player start ──────────────────────────────────────────
        aRow = START_ROW;
        aCol = START_COL;
    }

    // Connect helpers
    private void ConnectEW(int r1, int c1, int r2, int c2)
    {
        dungeon[r1, c1].SetEast(true);
        dungeon[r2, c2].SetWest(true);
    }
    private void ConnectNS(int r1, int c1, int r2, int c2)
    {
        dungeon[r1, c1].SetSouth(true);
        dungeon[r2, c2].SetNorth(true);
    }

    // ════════════════════════════════════════════════════════════
    //  START SCREEN
    // ════════════════════════════════════════════════════════════
    private void ShowGameStartScreen()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine(@"
  ╔══════════════════════════════════════════════════╗
  ║       ⚔   BIENVENIDO AL CALABOZO   ⚔            ║
  ╠══════════════════════════════════════════════════╣
  ║                                                  ║
  ║   OBJETIVO:                                      ║
  ║   1. Encuentra la 🪔 Lámpara  (Bodega)           ║
  ║   2. Consigue la  🗝  Llave    (Torreón)          ║
  ║   3. Abre el      📦 Cofre    (Cripta)            ║
  ║   4. ¡Escapa por la  🚪 Entrada antes            ║
  ║      de que el Grue te atrape!                   ║
  ║                                                  ║
  ║   El Grue duerme hasta que abres el cofre.       ║
  ║   Cuando despierte, buscará el camino más        ║
  ║   corto hacia ti en cada turno.                  ║
  ║                                                  ║
  ║   En la oscuridad, un Grue estático te mata      ║
  ║   si te mueves sin lámpara.                      ║
  ║                                                  ║
  ╠══════════════════════════════════════════════════╣
  ║  W=Norte  S=Sur  D=Este  A=Oeste                 ║
  ║  L=Lámpara  K=Llave  O=Abrir cofre  Q=Salir      ║
  ╚══════════════════════════════════════════════════╝
");
        Console.ResetColor();
        Console.Write("  Presiona ENTER para comenzar...");
        Console.ReadLine();
        Console.Clear();
    }

    // ════════════════════════════════════════════════════════════
    //  SHOW SCENE
    // ════════════════════════════════════════════════════════════
    private void ShowScene()
    {
        Console.Clear();
        DungeonMap.Show(dungeon, aRow, aCol, adventurer.HasLamp(), adventurer.HasKey(),
                        GrueActive, GrueRow, GrueCol);

        var r = dungeon[aRow, aCol];
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.Write("  ► ");
        Console.ResetColor();

        bool canSee = adventurer.HasLamp() || r.IsLit();
        if (canSee)
        {
            Console.Write(r.GetDescription());
            if (r.IsExit())
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("  🚪 ¡SALIDA!");
                Console.ResetColor();
            }
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("Oscuridad total... no ves nada.");
            Console.ResetColor();
        }
        Console.WriteLine();

        if (GrueActive && GrueRow == aRow && GrueCol == aCol)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("\n  ⚠  ¡¡ EL GRUE ESTÁ EN ESTA SALA !! ¡Muévete o mueres!");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════
    //  INPUT
    // ════════════════════════════════════════════════════════════
    private void ShowInputOptions()
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("  ┌──────────────────────────────────────────────────────┐");
        Console.WriteLine($"  │  [{GO_NORTH}] Norte  [{GO_EAST}] Este  [{GET_LAMP}] Lámpara  [{OPEN_CHEST}] Abrir cofre  │");
        Console.WriteLine($"  │  [{GO_SOUTH}] Sur    [{GO_WEST}] Oeste [{GET_KEY}] Llave                        │");
        Console.WriteLine($"  │                                       [{QUIT}] Salir      │");
        Console.WriteLine("  └──────────────────────────────────────────────────────┘");
        Console.ResetColor();
        Console.Write("  > ");
    }

    private string GetInput() => Console.ReadLine()!.ToUpper().Trim();

    private bool IsValidInput(string input)
    {
        string[] valid = { GO_NORTH, GO_SOUTH, GO_EAST, GO_WEST, GET_LAMP, GET_KEY, OPEN_CHEST, QUIT };
        if (!valid.Contains(input))
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("  ✗ Entrada inválida. Intenta de nuevo.");
            Console.ResetColor();
            return false;
        }
        return true;
    }

    // ════════════════════════════════════════════════════════════
    //  PROCESS INPUT
    // ════════════════════════════════════════════════════════════
    private void ProcessInput(string input)
    {
        Room r = dungeon[aRow, aCol];

        if      (input == GO_NORTH)   GoNorth(r);
        else if (input == GO_SOUTH)   GoSouth(r);
        else if (input == GO_EAST)    GoEast(r);
        else if (input == GO_WEST)    GoWest(r);
        else if (input == GET_LAMP)   GetLamp(r);
        else if (input == GET_KEY)    GetKey(r);
        else if (input == OPEN_CHEST) OpenChest(r);
        else                          Quit();
    }

    // ════════════════════════════════════════════════════════════
    //  UPDATE — called every turn after input
    // ════════════════════════════════════════════════════════════
    private void UpdateGameState()
    {
        if (hasPlayerQuit) return;

        // Grue only starts hunting AFTER the chest is opened
        if (GrueActive && isChestOpen)
            MoveGrueBFS();
    }

    // ════════════════════════════════════════════════════════════
    //  GAME OVER CHECK
    // ════════════════════════════════════════════════════════════
    private bool IsGameOver()
    {
        if (hasPlayerQuit || grueCaught)
            return true;

        // Win: chest opened AND player is at exit
        if (isChestOpen && aRow == EXIT_ROW && aCol == EXIT_COL)
            return true;

        // Grue catches player — only dangerous after chest is opened
        if (isChestOpen && GrueActive && GrueRow == aRow && GrueCol == aCol)
        {
            DungeonMap.PlayGrueCatchAnimation();
            grueCaught = true;
            return true;
        }

        return false;
    }

    // ════════════════════════════════════════════════════════════
    //  GAME OVER SCREEN
    // ════════════════════════════════════════════════════════════
    private void ShowGameOverScreen()
    {
        Console.Clear();

        if (isChestOpen && aRow == EXIT_ROW && aCol == EXIT_COL)
        {
            DungeonMap.PlayVictoryAnimation();
            Thread.Sleep(500);
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════╗
  ║   🏆   ¡¡ VICTORIA !!   🏆                   ║
  ║                                              ║
  ║   ¡Encontraste el tesoro y escapaste         ║
  ║   del Grue! Eres un verdadero aventurero.    ║
  ╚══════════════════════════════════════════════╝
");
        }
        else if (grueCaught)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════╗
  ║   💀   DEVORADO POR EL GRUE   💀             ║
  ║                                              ║
  ║   El Grue calculó cada paso.                 ║
  ║   Tu aventura terminó entre sus fauces.      ║
  ╚══════════════════════════════════════════════╝
");
        }
        else if (!isAdventureAlive)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════╗
  ║   💀   GAME OVER   💀                        ║
  ║                                              ║
  ║   La oscuridad te consumió.                  ║
  ║   El Grue siempre gana en las sombras.       ║
  ╚══════════════════════════════════════════════╝
");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine(@"
  ╔══════════════════════════════════════════════╗
  ║   Abandonaste el calabozo...                 ║
  ║   El Grue te dejó ir... esta vez.            ║
  ╚══════════════════════════════════════════════╝
");
        }

        Console.ResetColor();
        Console.WriteLine("  Presiona ENTER para salir...");
        Console.ReadLine();
    }

    // ════════════════════════════════════════════════════════════
    //  MOVEMENT
    // ════════════════════════════════════════════════════════════
    private void GoNorth(Room r)
    {
        if (r.HasNorth()) { aRow--; lastDirection = GO_SOUTH; }
        else Err("¡No puedes ir al norte!");
    }
    private void GoSouth(Room r)
    {
        if (r.HasSouth()) { aRow++; lastDirection = GO_NORTH; }
        else Err("¡No puedes ir al sur!");
    }
    private void GoEast(Room r)
    {
        if (r.HasEast()) { aCol++; lastDirection = GO_WEST; }
        else Err("¡No puedes ir al este!");
    }
    private void GoWest(Room r)
    {
        if (r.HasWest()) { aCol--; lastDirection = GO_EAST; }
        else Err("¡No puedes ir al oeste!");
    }

    // ════════════════════════════════════════════════════════════
    //  ITEMS
    // ════════════════════════════════════════════════════════════
    private void GetLamp(Room r)
    {
        if (r.HasLamp())
        {
            Ok("¡Tomaste la lámpara! Ahora puedes ver en la oscuridad.");
            adventurer.SetLamp(true);
            r.SetLamp(false);
        }
        else Err("No hay lámpara aquí.");
    }

    private void GetKey(Room r)
    {
        if (r.HasKey())
        {
            Ok("¡Tomaste la llave!");
            adventurer.SetKey(true);
            r.SetKey(false);
        }
        else Err("No hay llave aquí.");
    }

    private void OpenChest(Room r)
    {
        if (!r.HasChest()) { Err("No hay cofre aquí."); return; }
        if (!adventurer.HasKey()) { Err("¡Necesitas la llave para abrir el cofre!"); return; }

        isChestOpen = true;

        // Grue spawns at [0,2] — across the dungeon from the chest
        GrueRow    = GRUE_SPAWN_ROW;
        GrueCol    = GRUE_SPAWN_COL;
        GrueActive = true;

        DungeonMap.PlayGrueChaseIntro();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n  🏆 ¡Tomaste el tesoro del cofre!");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ⚠  ¡El Grue despertó y viene a por ti! ¡Escapa por la Entrada!");
        Console.ResetColor();
        Thread.Sleep(2000);
    }

    private void Quit()
    {
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("  Abandonaste el juego.");
        Console.ResetColor();
        hasPlayerQuit = true;
    }

    // ════════════════════════════════════════════════════════════
    //  GRUE AI — BFS shortest path through valid doors
    // ════════════════════════════════════════════════════════════
    private void MoveGrueBFS()
    {
        // BFS from grue position to player position
        // Returns the first step the grue should take
        var prev = new (int row, int col)[ROWS, COLS];
        for (int r = 0; r < ROWS; r++)
            for (int c = 0; c < COLS; c++)
                prev[r, c] = (-1, -1);

        var visited = new bool[ROWS, COLS];
        var queue   = new Queue<(int, int)>();

        queue.Enqueue((GrueRow, GrueCol));
        visited[GrueRow, GrueCol] = true;

        bool found = false;
        while (queue.Count > 0 && !found)
        {
            var (cr, cc) = queue.Dequeue();

            // Explore neighbours through valid doors
            (int dr, int dc, bool has)[] moves =
            {
                (-1, 0, dungeon[cr, cc].HasNorth()),
                ( 1, 0, dungeon[cr, cc].HasSouth()),
                ( 0, 1, dungeon[cr, cc].HasEast()),
                ( 0,-1, dungeon[cr, cc].HasWest()),
            };

            foreach (var (dr, dc, hasDoor) in moves)
            {
                if (!hasDoor) continue;
                int nr = cr + dr, nc = cc + dc;
                if (nr < 0 || nr >= ROWS || nc < 0 || nc >= COLS) continue;
                if (visited[nr, nc]) continue;

                visited[nr, nc] = true;
                prev[nr, nc] = (cr, cc);
                queue.Enqueue((nr, nc));

                if (nr == aRow && nc == aCol) { found = true; break; }
            }
        }

        if (!found) return; // no path (shouldn't happen with connected dungeon)

        // Trace back from player to find grue's next step
        int stepR = aRow, stepC = aCol;
        while (prev[stepR, stepC] != (GrueRow, GrueCol))
        {
            var p = prev[stepR, stepC];
            if (p == (-1, -1)) return; // no path
            stepR = p.row; stepC = p.col;
        }

        GrueRow = stepR;
        GrueCol = stepC;
    }

    // ════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════
    private static void Err(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("  ✗ " + msg);
        Console.ResetColor();
    }

    private static void Ok(string msg)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("  ✓ " + msg);
        Console.ResetColor();
    }
}
namespace AdventureGame;

public static class DungeonMap
{
    // ── Colors ───────────────────────────────────────────────────
    private static readonly ConsoleColor CLit    = ConsoleColor.Yellow;
    private static readonly ConsoleColor CDark   = ConsoleColor.DarkGray;
    private static readonly ConsoleColor CPlayer = ConsoleColor.Cyan;
    private static readonly ConsoleColor CGrue   = ConsoleColor.Red;
    private static readonly ConsoleColor CDoor   = ConsoleColor.DarkYellow;
    private static readonly ConsoleColor CItem   = ConsoleColor.Green;
    private static readonly ConsoleColor CExit   = ConsoleColor.Green;
    private static readonly ConsoleColor CTitle  = ConsoleColor.White;
    private static readonly ConsoleColor CDim    = ConsoleColor.DarkGray;
    private static readonly ConsoleColor CWarn   = ConsoleColor.Red;

    // ── Room cell size (inner, excluding border chars) ───────────
    private const int CW = 14;   // cell inner width
    private const int CH = 7;    // cell inner height

    // ════════════════════════════════════════════════════════════
    //  MAIN ENTRY
    // ════════════════════════════════════════════════════════════
    public static void Show(Room[,] dungeon, int pRow, int pCol,
                            bool hasLamp, bool hasKey,
                            bool grueActive, int grueRow, int grueCol)
    {
        const string pad = "  ";
        int rows = dungeon.GetLength(0);
        int cols = dungeon.GetLength(1);

        PrintHeader(pad, cols);
        Console.WriteLine();

        for (int r = 0; r < rows; r++)
        {
            PrintGridRow(dungeon, r, pRow, pCol, hasLamp,
                         grueActive, grueRow, grueCol, cols, pad);

            if (r < rows - 1)
                PrintRowConnector(dungeon, r, cols, pad);
        }

        Console.WriteLine();
        PrintInventory(hasLamp, hasKey, pad, cols);
        PrintLegend(pad);

        if (grueActive)
        {
            int dist = Math.Abs(grueRow - pRow) + Math.Abs(grueCol - pCol);
            PrintGrueAlert(pad, cols, dist);
        }

        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════
    //  HEADER
    // ════════════════════════════════════════════════════════════
    private static void PrintHeader(string pad, int cols)
    {
        int w = TotalWidth(cols);
        W(pad); Wln("╔" + Rep('═', w) + "╗", CDoor);
        W(pad); W("║", CDoor);
        W(Ctr("⚔  MAPA DEL CALABOZO  ⚔", w), CTitle);
        Wln("║", CDoor);
        W(pad); Wln("╚" + Rep('═', w) + "╝", CDoor);
    }

    // Total width for header/inventory: borders + cells + connectors
    private static int TotalWidth(int cols) => (CW + 2) * cols + (cols - 1);

    // ════════════════════════════════════════════════════════════
    //  ONE ROW OF ROOMS
    // ════════════════════════════════════════════════════════════
    private static void PrintGridRow(Room[,] d, int row,
                                     int pRow, int pCol, bool hasLamp,
                                     bool grueActive, int grueRow, int grueCol,
                                     int cols, string pad)
    {
        bool[] vis   = new bool[cols];
        bool[] isPl  = new bool[cols];
        bool[] isGr  = new bool[cols];
        string[][] cellLines = new string[cols][];

        for (int c = 0; c < cols; c++)
        {
            vis[c]  = d[row, c].IsLit() || hasLamp;
            isPl[c] = pRow == row && pCol == c;
            isGr[c] = grueActive && grueRow == row && grueCol == c;
            cellLines[c] = MakeCellLines(d[row, c], isPl[c], isGr[c], vis[c]);
        }

        // ── Top border ────────────────────────────────────────────
        W(pad);
        for (int c = 0; c < cols; c++)
        {
            ConsoleColor bc = vis[c] ? CLit : CDark;
            char leftCorner  = c == 0 ? '┌' : '┬';
            bool nd = d[row, c].HasNorth();
            // left corner + optional N door indicator
            W(leftCorner.ToString() + (nd ? "╨" : "─"), bc);
            W(Rep('─', CW - 2), bc);
            W((nd ? "╨" : "─") + "┐", bc);
            if (c < cols - 1) W("─", CDim);
        }
        Console.WriteLine();

        // ── Content lines ─────────────────────────────────────────
        for (int line = 0; line < CH; line++)
        {
            W(pad);
            for (int c = 0; c < cols; c++)
            {
                ConsoleColor bc = vis[c] ? CLit : CDark;

                if (c == 0)
                {
                    W("│", bc);
                }
                else
                {
                    // door connector between col c-1 and col c
                    bool eastPrev = d[row, c - 1].HasEast() && line == CH / 2;
                    bool westCur  = d[row, c].HasWest()     && line == CH / 2;
                    if (eastPrev && westCur)
                        W("╪", CDoor);
                    else
                    {
                        ConsoleColor sep = vis[c - 1] || vis[c] ? CLit : CDark;
                        W("│", sep);
                    }
                }

                PrintCellLine(cellLines[c][line], isPl[c], isGr[c], vis[c]);

                // Right wall only for last column
                if (c == cols - 1)
                    W("│", vis[c] ? CLit : CDark);
            }
            Console.WriteLine();
        }

        // ── Bottom border ─────────────────────────────────────────
        W(pad);
        for (int c = 0; c < cols; c++)
        {
            ConsoleColor bc = vis[c] ? CLit : CDark;
            char leftCorner  = c == 0 ? '└' : '┴';
            bool sd = d[row, c].HasSouth();
            W(leftCorner.ToString() + (sd ? "╥" : "─"), bc);
            W(Rep('─', CW - 2), bc);
            W((sd ? "╥" : "─") + "┘", bc);
            if (c < cols - 1) W("─", CDim);
        }
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════
    //  VERTICAL CONNECTORS BETWEEN ROWS
    // ════════════════════════════════════════════════════════════
    private static void PrintRowConnector(Room[,] d, int topRow, int cols, string pad)
    {
        int passCenter = (CW + 2) / 2;

        for (int line = 0; line < 2; line++)
        {
            W(pad);
            for (int c = 0; c < cols; c++)
            {
                W(Rep(' ', passCenter));
                bool hasSouth = d[topRow, c].HasSouth();
                W(hasSouth ? "║" : " ", hasSouth ? CDoor : CDim);
                W(Rep(' ', CW + 2 - passCenter - 1));
                if (c < cols - 1) W(" ", CDim);
            }
            Console.WriteLine();
        }
    }

    // ════════════════════════════════════════════════════════════
    //  CELL LINE BUILDER
    // ════════════════════════════════════════════════════════════
    private static string[] MakeCellLines(Room room, bool isP, bool isG, bool vis)
    {
        var lines = new string[CH];
        for (int i = 0; i < CH; i++) lines[i] = Rep(' ', CW);

        // Dark unoccupied room
        if (!vis && !isP)
        {
            lines[1] = Ctr("▓▓▓▓▓▓▓▓", CW);
            lines[2] = Ctr("▓OSCURO▓", CW);
            lines[3] = Ctr("▓▓▓▓▓▓▓▓", CW);
            lines[6] = Ctr("[" + Truncate(room.GetDescription(), CW - 2) + "]", CW);
            return lines;
        }

        // Room name
        lines[0] = Ctr(Truncate(room.GetDescription(), CW), CW);

        // Exit marker
        if (room.IsExit())
            lines[1] = Ctr("[E]SALIDA", CW);

        // Characters
        if (isP && isG)
        {
            lines[2] = Ctr("[P]O[G]>)'", CW);
            lines[3] = Ctr("[W]PELIGRO", CW);
        }
        else if (isP)
        {
            lines[2] = Ctr("[P]  O  ", CW);
            lines[3] = Ctr("[P] /|\\ ", CW);
            lines[4] = Ctr("[P] / \\ ", CW);
        }
        else if (isG)
        {
            lines[2] = Ctr("[G]>))'>", CW);
            lines[3] = Ctr("[G]/GRUE", CW);
            lines[4] = Ctr("[G]{____}", CW);
        }

        // Items
        var items = new System.Collections.Generic.List<string>();
        if (room.HasLamp())  items.Add("[I]L");
        if (room.HasKey())   items.Add("[I]K");
        if (room.HasChest()) items.Add("[I]C");
        if (items.Count > 0)
            lines[5] = Ctr(string.Join(" ", items), CW);

        // Light state
        lines[6] = Ctr(room.IsLit() ? "[S]☀ lit" : "[S]☾ dark", CW);

        return lines;
    }

    // ════════════════════════════════════════════════════════════
    //  CELL LINE PRINTER
    // ════════════════════════════════════════════════════════════
    private static void PrintCellLine(string line, bool isP, bool isG, bool vis)
    {
        ConsoleColor def = vis ? CLit : CDark;
        Console.ForegroundColor = def;

        int i = 0;
        while (i < line.Length)
        {
            if (line[i] == '[')
            {
                int close = line.IndexOf(']', i);
                if (close > i)
                {
                    string tag = line.Substring(i + 1, close - i - 1);
                    Console.ForegroundColor = tag switch
                    {
                        "P" => CPlayer,
                        "G" => CGrue,
                        "I" => CItem,
                        "S" => vis ? CLit : CDark,
                        "E" => CExit,
                        "W" => CWarn,
                        _   => def,
                    };
                    i = close + 1;
                    continue;
                }
            }
            Console.Write(line[i]);
            Console.ForegroundColor = def;
            i++;
        }
        Console.ResetColor();
    }

    // ════════════════════════════════════════════════════════════
    //  INVENTORY
    // ════════════════════════════════════════════════════════════
    private static void PrintInventory(bool hasLamp, bool hasKey, string pad, int cols)
    {
        int w = TotalWidth(cols);
        W(pad); Wln("┌─── Inventario " + Rep('─', w - 16) + "┐", CDoor);
        W(pad); W("│  ", CDoor);
        W("Lámpara: ", CTitle);
        if (hasLamp) W("🪔 EQUIPADA        ", CItem);
        else         W("─ no obtenida      ", CDim);
        W("  Llave: ", CTitle);
        if (hasKey) W("🗝 EQUIPADA    ", CItem);
        else        W("─ no obtenida  ", CDim);
        Wln("│", CDoor);
        W(pad); Wln("└" + Rep('─', w) + "┘", CDoor);
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════
    //  LEGEND
    // ════════════════════════════════════════════════════════════
    private static void PrintLegend(string pad)
    {
        W(pad);
        W(" O ", CPlayer);  W("=Tú  ", CTitle);
        W(">)'> ", CGrue);  W("=Grue  ", CTitle);
        W("SALIDA ", CExit); W("=Exit  ", CTitle);
        W("L ", CItem);     W("=Lámpara  ", CTitle);
        W("K ", CItem);     W("=Llave  ", CTitle);
        W("C ", CItem);     W("=Cofre  ", CTitle);
        W("╪", CDoor);      W("=Puerta", CTitle);
        Console.WriteLine();
    }

    // ════════════════════════════════════════════════════════════
    //  GRUE ALERT
    // ════════════════════════════════════════════════════════════
    private static void PrintGrueAlert(string pad, int cols, int dist)
    {
        int w = TotalWidth(cols);
        bool blink = (DateTime.Now.Millisecond / 350) % 2 == 0;
        ConsoleColor c = blink ? ConsoleColor.Red : ConsoleColor.DarkRed;

        string distMsg = dist switch {
            0 => "¡¡¡ ESTÁ EN TU SALA — CORRE !!!",
            1 => "¡Sala adyacente! Sientes su aliento...",
            2 => "Dos salas. Se acerca...",
            _ => "Oyes sus pasos a lo lejos.",
        };

        int barLen = 20;
        int filled = Math.Clamp(barLen - dist * 5, 0, barLen);
        ConsoleColor barCol = dist == 0 ? ConsoleColor.Red
                            : dist == 1 ? ConsoleColor.DarkYellow
                            : ConsoleColor.DarkGreen;

        Console.WriteLine();
        W(pad); Wln("╔" + Rep('═', w) + "╗", c);
        W(pad); W("║", c); W(Ctr("⚠  ¡EL GRUE TE PERSIGUE CON BFS!  ⚠", w), c); Wln("║", c);
        W(pad); W("║", c); W(Ctr(distMsg, w), ConsoleColor.Yellow); Wln("║", c);
        W(pad); W("║  PELIGRO [", c);
        W(Rep('█', filled), barCol);
        W(Rep('░', barLen - filled), CDim);
        W("] ", c);
        W(Rep(' ', Math.Max(0, w - 14 - barLen)), c);
        Wln("║", c);
        W(pad); Wln("╚" + Rep('═', w) + "╝", c);
    }

    // ════════════════════════════════════════════════════════════
    //  CUTSCENES
    // ════════════════════════════════════════════════════════════
    public static void PlayGrueChaseIntro()
    {
        ConsoleColor[] pulse = {
            ConsoleColor.DarkRed, ConsoleColor.Red, ConsoleColor.Red,
            ConsoleColor.DarkRed, ConsoleColor.Red, ConsoleColor.Black,
        };
        string[] art = {
            "",
            "   ╔══════════════════════════════════════════════════════╗",
            "   ║                                                      ║",
            "   ║     >)))'->)))'->)))'->)))'->)))'->)))'->)))'->     ║",
            "   ║                                                      ║",
            "   ║        G  R  U  E     D  E  S  P  I  E  R  T  A    ║",
            "   ║                                                      ║",
            "   ║   El Grue dormia en el cofre. Lo despertaste.       ║",
            "   ║   Ahora calculara BFS para encontrarte.             ║",
            "   ║                                                      ║",
            "   ║        ESCAPA POR LA ENTRADA ANTES DE QUE           ║",
            "   ║              TE ENCUENTRE!                          ║",
            "   ║                                                      ║",
            "   ╚══════════════════════════════════════════════════════╝",
            "",
        };

        for (int rep = 0; rep < 4; rep++)
        {
            foreach (var col in pulse)
            {
                Console.Clear();
                Console.ForegroundColor = col == ConsoleColor.Black ? ConsoleColor.DarkRed : col;
                foreach (var ln in art) Console.WriteLine(ln);
                Console.ResetColor();
                Thread.Sleep(160);
            }
        }

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"
            >)))'->)))'->)))'->)))'->)))'->
           /                               \
          |   .    .                        |
          |       ___                       |
          |      /   \     G R U E         |
          |      \___/                     |
           \      ~~~                     /
            '>)))'->)))'->)))'->)))'->))'

         !! TE VIO !!   !! CORRE AHORA !!
");
        Console.ResetColor();
        Thread.Sleep(2200);
    }

    public static void PlayGrueCatchAnimation()
    {
        string[][] frames = {
            new[]{ "   O                    >)))'->)))'-> ", "   Aventurero                    Grue" },
            new[]{ "   O            >)))'->)))'->         ", "   Aventurero        Grue"             },
            new[]{ "   O    >)))'->)))'->                 ", "   Aventurero  Grue"                   },
            new[]{ "   >)))'->  O  *CHOMP*                ", "   (OM NOM NOM NOM)"                   },
            new[]{ "   >)))'->     *CRUNCH*               ", "   (GLUP...)"                          },
        };

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine("\n\n   El Grue calculo el camino perfecto...\n");
        Thread.Sleep(700);

        foreach (var frame in frames)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("   " + frame[0] + new string(' ', 30));
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.Write("   " + frame[1] + new string(' ', 30));
            Console.WriteLine();
            Thread.Sleep(550);
            Console.SetCursorPosition(0, Console.CursorTop - 2);
        }

        Console.WriteLine("\n\n");
        Thread.Sleep(400);
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(@"
   ████████████████████████████████████████████████
   ██                                            ██
   ██   >))))'>   *CHOMP*   *CRUNCH*  *GLUP*    ██
   ██                                            ██
   ██   El Grue calculo cada sala.               ██
   ██   No habia escape posible.                 ██
   ██                                            ██
   ████████████████████████████████████████████████
");
        Console.ResetColor();
        Thread.Sleep(2500);
    }

    public static void PlayVictoryAnimation()
    {
        string[] frames = {
            "  Aventurero <<<<<<<<<<<<<<<<<<<<<<<<<<  >))'> ",
            "  Aventurero <<<<<<<<<<<<<<<<<<  >))'> ",
            "  Aventurero <<<<<<<<<<  >))'> ",
            "  Aventurero <<<<  >))'> ",
            "  [SALIDA]<< Aventurero   >))'> (roooar!)",
            "  [SALIDA]*SLAM*        >))'>...            ",
            "  [SALIDA]              (silencio...)       ",
        };

        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("\n  !! CORRE !! El Grue calculo tu ruta...\n");
        Thread.Sleep(900);

        int startLine = Console.CursorTop;
        foreach (var frame in frames)
        {
            Console.SetCursorPosition(0, startLine);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(frame + new string(' ', 10));
            Console.WriteLine();
            Thread.Sleep(650);
        }
        Console.ResetColor();
        Thread.Sleep(700);
    }

    // ════════════════════════════════════════════════════════════
    //  HELPERS
    // ════════════════════════════════════════════════════════════
    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..max];

    private static string Ctr(string s, int w)
    {
        // Strip color tags for width measurement
        string clean = System.Text.RegularExpressions.Regex.Replace(s, @"\[[A-Z]\]", "");
        int tagLen = s.Length - clean.Length;
        int visLen = clean.Length;
        if (visLen >= w) return s;
        int pad = w - visLen, left = pad / 2;
        return new string(' ', left) + s + new string(' ', pad - left);
    }

    private static string Rep(char c, int n) => n > 0 ? new string(c, n) : "";

    private static void W(string s, ConsoleColor col = ConsoleColor.Gray)
    { Console.ForegroundColor = col; Console.Write(s); }

    private static void Wln(string s, ConsoleColor col = ConsoleColor.Gray)
    { Console.ForegroundColor = col; Console.WriteLine(s); }
}
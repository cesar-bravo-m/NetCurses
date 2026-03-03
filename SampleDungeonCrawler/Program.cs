using BCyT.NetCurses;

const int C_PLAYER   = 1;
const int C_WALL     = 2;
const int C_FLOOR    = 3;
const int C_MONSTER  = 4;
const int C_ITEM     = 5;
const int C_STAIRS   = 6;
const int C_HUD      = 7;
const int C_MSG      = 8;
const int C_DEAD     = 9;
const int C_GOLD     = 10;

const char T_WALL  = '#';
const char T_FLOOR = '.';
const char T_STAIR = '>';
const char T_NONE  = ' ';

const int MAP_W = 78;
const int MAP_H = 20;
const int HUD_Y = 21;
const int MSG_Y = 23;

var rng = new Random();
int playerX = 0, playerY = 0;
int playerHp = 30, playerMaxHp = 30;
int playerAtk = 5, playerDef = 2;
int playerGold = 0;
int dungeonLevel = 1;
string message = "Welcome to the dungeon! Use arrow keys to move, bump to attack.";
bool gameOver = false;

var map = new char[MAP_H, MAP_W];
var monsters = new List<Monster>();
var items = new List<Item>();

(char glyph, string name, int hp, int atk, int def)[] monsterTemplates =
[
    ('r', "Rat",      4, 1, 0),
    ('g', "Goblin",   8, 3, 1),
    ('s', "Skeleton", 12, 4, 2),
    ('o', "Orc",      16, 5, 3),
    ('T', "Troll",    24, 7, 4),
];

void GenerateLevel()
{
    monsters.Clear();
    items.Clear();

    // Walls
    for (int y = 0; y < MAP_H; y++)
        for (int x = 0; x < MAP_W; x++)
            map[y, x] = T_NONE;

    // Rooms
    var rooms = new List<(int x, int y, int w, int h)>();
    int roomCount = rng.Next(5, 9);

    for (int attempt = 0; attempt < 200 && rooms.Count < roomCount; attempt++)
    {
        int rw = rng.Next(4, 12);
        int rh = rng.Next(3, 7);
        int rx = rng.Next(1, MAP_W - rw - 1);
        int ry = rng.Next(1, MAP_H - rh - 1);

        bool overlap = false;
        foreach (var (ox, oy, ow, oh) in rooms)
        {
            if (rx - 1 < ox + ow && rx + rw + 1 > ox &&
                ry - 1 < oy + oh && ry + rh + 1 > oy)
            {
                overlap = true;
                break;
            }
        }
        if (overlap) continue;

        for (int y = ry; y < ry + rh; y++)
            for (int x = rx; x < rx + rw; x++)
                map[y, x] = T_FLOOR;

        for (int y = ry - 1; y <= ry + rh; y++)
            for (int x = rx - 1; x <= rx + rw; x++)
                if (map[y, x] == T_NONE)
                    map[y, x] = T_WALL;

        rooms.Add((rx, ry, rw, rh));
    }

    for (int i = 1; i < rooms.Count; i++)
    {
        int cx1 = rooms[i - 1].x + rooms[i - 1].w / 2;
        int cy1 = rooms[i - 1].y + rooms[i - 1].h / 2;
        int cx2 = rooms[i].x + rooms[i].w / 2;
        int cy2 = rooms[i].y + rooms[i].h / 2;

        if (rng.Next(2) == 0)
        {
            CarveCorridor(cx1, cy1, cx2, cy1);
            CarveCorridor(cx2, cy1, cx2, cy2);
        }
        else
        {
            CarveCorridor(cx1, cy1, cx1, cy2);
            CarveCorridor(cx1, cy2, cx2, cy2);
        }
    }

    playerX = rooms[0].x + rooms[0].w / 2;
    playerY = rooms[0].y + rooms[0].h / 2;

    var lastRoom = rooms[^1];
    int sx = lastRoom.x + lastRoom.w / 2;
    int sy = lastRoom.y + lastRoom.h / 2;
    map[sy, sx] = T_STAIR;

    int monsterCount = dungeonLevel + rng.Next(3, 6);
    for (int i = 0; i < monsterCount; i++)
    {
        int roomIdx = rng.Next(1, rooms.Count);
        var room = rooms[roomIdx];
        int mx = rng.Next(room.x, room.x + room.w);
        int my = rng.Next(room.y, room.y + room.h);
        if (mx == playerX && my == playerY) continue;

        int maxTier = Math.Min(dungeonLevel, monsterTemplates.Length);
        int tier = rng.Next(0, maxTier);
        var t = monsterTemplates[tier];
        int scale = 1 + (dungeonLevel - 1) / 3;
        monsters.Add(new Monster(mx, my, t.glyph, t.name,
            t.hp * scale, t.hp * scale, t.atk * scale, t.def * scale));
    }

    int itemCount = rng.Next(2, 5);
    for (int i = 0; i < itemCount; i++)
    {
        int roomIdx = rng.Next(0, rooms.Count);
        var room = rooms[roomIdx];
        int ix = rng.Next(room.x, room.x + room.w);
        int iy = rng.Next(room.y, room.y + room.h);
        if (ix == playerX && iy == playerY) continue;

        if (rng.Next(3) == 0)
            items.Add(new Item(ix, iy, '!', "Health Potion", ItemKind.Heal, 8 + dungeonLevel * 2));
        else
            items.Add(new Item(ix, iy, '$', "Gold", ItemKind.Gold, rng.Next(5, 15) + dungeonLevel * 3));
    }
}

void CarveCorridor(int x1, int y1, int x2, int y2)
{
    int dx = Math.Sign(x2 - x1);
    int dy = Math.Sign(y2 - y1);
    int cx = x1, cy = y1;

    while (cx != x2 || cy != y2)
    {
        if (cx >= 0 && cx < MAP_W && cy >= 0 && cy < MAP_H)
        {
            if (map[cy, cx] == T_NONE || map[cy, cx] == T_WALL)
                map[cy, cx] = T_FLOOR;
            for (int wy = cy - 1; wy <= cy + 1; wy++)
                for (int wx = cx - 1; wx <= cx + 1; wx++)
                    if (wy >= 0 && wy < MAP_H && wx >= 0 && wx < MAP_W && map[wy, wx] == T_NONE)
                        map[wy, wx] = T_WALL;
        }
        if (cx != x2) cx += dx;
        else if (cy != y2) cy += dy;
    }
    if (cx >= 0 && cx < MAP_W && cy >= 0 && cy < MAP_H)
    {
        if (map[cy, cx] == T_NONE || map[cy, cx] == T_WALL)
            map[cy, cx] = T_FLOOR;
    }
}

void Draw()
{
    NetCurses.Erase();

    for (int y = 0; y < MAP_H; y++)
    {
        for (int x = 0; x < MAP_W; x++)
        {
            char tile = map[y, x];
            if (tile == T_NONE) continue;

            uint color = tile switch
            {
                T_WALL  => NetCurses.ColorPair(C_WALL),
                T_FLOOR => NetCurses.ColorPair(C_FLOOR),
                T_STAIR => NetCurses.ColorPair(C_STAIRS),
                _       => 0
            };
            NetCurses.AttributeSet(color);
            NetCurses.MvAddChar(y, x, tile);
        }
    }

    foreach (var item in items)
    {
        uint color = item.Kind == ItemKind.Gold
            ? NetCurses.ColorPair(C_GOLD)
            : NetCurses.ColorPair(C_ITEM);
        NetCurses.AttributeSet(color | Attrs.Bold);
        NetCurses.MvAddChar(item.Y, item.X, item.Glyph);
    }

    NetCurses.AttributeSet(NetCurses.ColorPair(C_MONSTER) | Attrs.Bold);
    foreach (var m in monsters)
        NetCurses.MvAddChar(m.Y, m.X, m.Glyph);

    NetCurses.AttributeSet(NetCurses.ColorPair(C_PLAYER) | Attrs.Bold);
    NetCurses.MvAddChar(playerY, playerX, '@');

    NetCurses.AttributeSet(NetCurses.ColorPair(C_HUD));
    string hud = $" HP: {playerHp}/{playerMaxHp}  ATK: {playerAtk}  DEF: {playerDef}  Gold: {playerGold}  Depth: {dungeonLevel} ";
    NetCurses.MvAddString(HUD_Y, 0, new string('─', MAP_W));
    NetCurses.MvAddString(HUD_Y + 1, 0, hud.PadRight(MAP_W));

    NetCurses.AttributeSet(NetCurses.ColorPair(C_MSG));
    string displayMsg = message.Length > MAP_W ? message[..MAP_W] : message;
    NetCurses.MvAddString(MSG_Y, 0, displayMsg.PadRight(MAP_W));

    NetCurses.Refresh();
}

void MovePlayer(int dx, int dy)
{
    int nx = playerX + dx;
    int ny = playerY + dy;

    if (nx < 0 || nx >= MAP_W || ny < 0 || ny >= MAP_H) return;
    if (map[ny, nx] == T_WALL || map[ny, nx] == T_NONE) return;

    for (int i = 0; i < monsters.Count; i++)
    {
        var m = monsters[i];
        if (m.X == nx && m.Y == ny)
        {
            int dmg = Math.Max(1, playerAtk - m.Def);
            var updated = m with { Hp = m.Hp - dmg };
            message = $"You hit the {m.Name} for {dmg} damage!";

            if (updated.Hp <= 0)
            {
                monsters.RemoveAt(i);
                message += $" The {m.Name} dies!";
            }
            else
            {
                monsters[i] = updated;
            }
            return;
        }
    }

    playerX = nx;
    playerY = ny;

    for (int i = items.Count - 1; i >= 0; i--)
    {
        var item = items[i];
        if (item.X == playerX && item.Y == playerY)
        {
            items.RemoveAt(i);
            switch (item.Kind)
            {
                case ItemKind.Heal:
                    int healed = Math.Min(item.Value, playerMaxHp - playerHp);
                    playerHp += healed;
                    message = $"You drink a {item.Name}. Restored {healed} HP.";
                    break;
                case ItemKind.Gold:
                    playerGold += item.Value;
                    message = $"You picked up {item.Value} gold!";
                    break;
            }
        }
    }

    if (map[playerY, playerX] == T_STAIR)
    {
        dungeonLevel++;
        playerMaxHp += 2;
        playerHp = playerMaxHp;
        playerAtk += 1;
        playerDef += 1;
        message = $"You descend to depth {dungeonLevel}. You feel stronger!";
        GenerateLevel();
    }
}

void MonstersTurn()
{
    for (int i = 0; i < monsters.Count; i++)
    {
        var m = monsters[i];

        int dist = Math.Abs(m.X - playerX) + Math.Abs(m.Y - playerY);
        if (dist > 8) continue;

        int dx = Math.Sign(playerX - m.X);
        int dy = Math.Sign(playerY - m.Y);

        if (Math.Abs(m.X - playerX) <= 1 && Math.Abs(m.Y - playerY) <= 1 && dist > 0)
        {
            int dmg = Math.Max(1, m.Atk - playerDef);
            playerHp -= dmg;
            message = $"The {m.Name} hits you for {dmg} damage!";

            if (playerHp <= 0)
            {
                playerHp = 0;
                gameOver = true;
                message = "You have died! Press 'q' to quit or 'r' to restart.";
            }
            continue;
        }

        int nx = m.X + dx;
        int ny = m.Y + dy;

        if (CanMonsterMoveTo(nx, ny, i))
        {
            monsters[i] = m with { X = nx, Y = ny };
        }
        else if (CanMonsterMoveTo(m.X + dx, m.Y, i))
        {
            monsters[i] = m with { X = m.X + dx };
        }
        else if (CanMonsterMoveTo(m.X, m.Y + dy, i))
        {
            monsters[i] = m with { Y = m.Y + dy };
        }
    }
}

bool CanMonsterMoveTo(int x, int y, int selfIdx)
{
    if (x < 0 || x >= MAP_W || y < 0 || y >= MAP_H) return false;
    if (map[y, x] != T_FLOOR) return false;
    if (x == playerX && y == playerY) return false;
    for (int i = 0; i < monsters.Count; i++)
        if (i != selfIdx && monsters[i].X == x && monsters[i].Y == y) return false;
    return true;
}

try
{
    NetCurses.InitScreen();
    NetCurses.StartColor();
    NetCurses.CBreak();
    NetCurses.NoEcho();
    NetCurses.CursorSet(0);
    NetCurses.StdScr.Keypad(true);

    NetCurses.InitPair(C_PLAYER,  Colors.Yellow,  Colors.Black);
    NetCurses.InitPair(C_WALL,    Colors.White,   Colors.Black);
    NetCurses.InitPair(C_FLOOR,   Colors.White,   Colors.Black);
    NetCurses.InitPair(C_MONSTER, Colors.Red,     Colors.Black);
    NetCurses.InitPair(C_ITEM,    Colors.Magenta, Colors.Black);
    NetCurses.InitPair(C_STAIRS,  Colors.Cyan,    Colors.Black);
    NetCurses.InitPair(C_HUD,     Colors.Cyan,    Colors.Black);
    NetCurses.InitPair(C_MSG,     Colors.Green,   Colors.Black);
    NetCurses.InitPair(C_DEAD,    Colors.Red,     Colors.Black);
    NetCurses.InitPair(C_GOLD,    Colors.Yellow,  Colors.Black);

    GenerateLevel();

    while (true)
    {
        Draw();

        int ch = NetCurses.GetChar();

        if (ch == 'q' || ch == 'Q')
            break;

        if (gameOver)
        {
            if (ch == 'r' || ch == 'R')
            {
                playerHp = 30; playerMaxHp = 30;
                playerAtk = 5; playerDef = 2;
                playerGold = 0; dungeonLevel = 1;
                gameOver = false;
                message = "A new adventure begins!";
                GenerateLevel();
            }
            continue;
        }

        message = "";

        switch (ch)
        {
            case Key.Up:    MovePlayer(0, -1);  break;
            case Key.Down:  MovePlayer(0, 1);   break;
            case Key.Left:  MovePlayer(-1, 0);  break;
            case Key.Right: MovePlayer(1, 0);   break;

            case 'k': MovePlayer(0, -1);  break;
            case 'j': MovePlayer(0, 1);   break;
            case 'h': MovePlayer(-1, 0);  break;
            case 'l': MovePlayer(1, 0);   break;

            case 'y': MovePlayer(-1, -1); break;
            case 'u': MovePlayer(1, -1);  break;
            case 'b': MovePlayer(-1, 1);  break;
            case 'n': MovePlayer(1, 1);   break;

            case '.': message = "You wait."; break;
            default:
                message = "Arrow keys/hjklyubn to move, '.' to wait, 'q' to quit.";
                continue;
        }

        if (!gameOver)
            MonstersTurn();
    }
}
finally
{
    NetCurses.EndWin();
}

record struct Monster(int X, int Y, char Glyph, string Name, int Hp, int MaxHp, int Atk, int Def);
record struct Item(int X, int Y, char Glyph, string Name, ItemKind Kind, int Value);
enum ItemKind { Heal, Gold }

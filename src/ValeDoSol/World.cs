using static ValeDoSol.Ray;
using static ValeDoSol.Logic;
namespace ValeDoSol;

public readonly record struct RenderItem(double y, int type, int index);
public static class World
{
    static double Mod(double x, double y) => x % y;
    public static Color color_hex(long c) { return new Color((byte)(c >> 16), (byte)(c >> 8), (byte)c, 255); }
    static void r(double x, double y, double w, double h, long color) { DrawRectangle((int)Math.Round(x), (int)Math.Round(y), (int)w, (int)h, color_hex(color)); }
    static void label(string s, int x, int y, int size, long color) { int w = MeasureText(s, size); DrawText(s, x - w / 2 + 1, y + 1, size, new Color(41, 70, 56, 85)); DrawText(s, x - w / 2, y, size, color_hex(color)); }
    static void tree(double x, double y) { r(x - 28, y + 6, 64, 12, 0x86a56d); r(x - 7, y - 29, 15, 46, 0x856247); r(x - 3, y - 23, 5, 36, 0xa38056); r(x - 35, y - 65, 70, 38, 0x49744a); r(x - 43, y - 52, 86, 24, 0x49744a); r(x - 29, y - 85, 58, 50, 0x5e8650); r(x - 34, y - 67, 64, 24, 0x5e8650); r(x - 19, y - 93, 35, 16, 0x759557); r(x - 23, y - 79, 23, 9, 0x81a25e); r(x - 29, y - 53, 15, 6, 0x749754); r(x + 17, y - 48, 17, 9, 0x3e6947); }
    static void house(Building b)
    {
        int x = b.x * 32, y = b.y * 32, w = b.w * 32, h = b.h * 32; r(x - 9, y + h - 5, w + 24, 17, 0x8ca171); r(x, y + 45, w, h - 45, 0xe0c592);
        for (int k = 55; k < h; k += 15) r(x + 5, y + k, w - 10, 2, 0xcab080);
        r(x + 6, y + 49, 6, h - 49, 0xc0a67a); r(x + w - 12, y + 49, 6, h - 49, 0xc0a67a); r(x - 9, y + 42, w + 18, 10, 0x745641);
        for (int i = 0; i < 8; i++) { Color col = color_hex(b.roof); if (i % 2 == 0) { col.r = (byte)(col.r + 12); col.g = (byte)(col.g + 12); col.b = (byte)(col.b + 12); } DrawRectangle(x - 9 + i * 9, y + 39 - i * 7, w + 18 - i * 18, 8, col); }
        r(x + w - 45, y - 7, 15, 42, 0xa78465); r(x + w - 49, y - 10, 23, 6, 0xc9ad8a);
        for (int i = 0; i < 2; i++) { int xx = i != 0 ? x + w - 55 : x + 24; r(xx, y + 70, 30, 34, 0x8a7555); r(xx + 4, y + 74, 22, 25, 0x8db0ad); r(xx + 14, y + 74, 3, 25, 0xf5deb2); r(xx + 4, y + 85, 22, 3, 0xf5deb2); r(xx - 3, y + 104, 36, 6, 0xb49467); r(xx, y + 103, 6, 4, 0x68854d); r(xx + 18, y + 101, 5, 6, 0xd78c8e); }
        int dx = x + w / 2 - 17; r(dx, y + h - 48, 34, 48, 0x785d43); r(dx + 4, y + h - 44, 26, 44, 0xa17e55); r(dx + 24, y + h - 24, 3, 3, 0xefd995); r(dx - 9, y + h, 52, 7, 0xb79e76); r(dx - 14, y + h + 7, 62, 5, 0xd0b88c);
        r(x + w / 2 - 68, y + 50, 136, 18, 0x5b634a); label(b.name, x + w / 2, y + 54, 10, 0xfbf3d7);
        if (b.type == 1) for (int k = 0; k < 8; k++) r(x + 8 + k * (w - 16) / 8, y + 47, (w - 16) / 8, 11, k % 2 != 0 ? 0xeee0b1 : 0x849a72);
        if (b.type == 3) for (int k = 0; k < 5; k++) { r(x + 14 + k * 28, y + h - 13, 16, 13, 0xa47956); r(x + 17 + k * 28, y + h - 24, 9, 12, 0x6f954e); r(x + 14 + k * 28, y + h - 26, 15, 5, 0xd59ca6); }
    }
    static void farmer(double x, double y, uint coat, uint hair, string? name, double time, bool player)
    {
        double bob = time > 0 ? Math.Sin(time * 12) * 2 : 0; r(x - 12, y + 10, 25, 7, 0x83a06d); r(x - 7, y + 4, 6, 12 + (time > 0 ? Math.Sin(time * 12) * 2 : 0), 0x4d6260); r(x + 3, y + 4, 6, 12 - (time > 0 ? Math.Sin(time * 12) * 2 : 0), 0x4d6260);
        r(x - 10, y - 12 + bob, 22, 21, coat); r(x - 14, y - 9 + bob, 5, 15, 0xe8bb92); r(x + 12, y - 9 + bob, 5, 15, 0xe8bb92); r(x - 8, y - 28 + bob, 18, 18, 0xeac29a); r(x - 9, y - 29 + bob, 20, 7, hair); r(x - 9, y - 25 + bob, 4, 12, hair); r(x + 5, y - 20 + bob, 2, 3, 0x3d4c40);
        if (player) { r(x - 15, y - 31 + bob, 32, 6, 0xe1bf77); r(x - 10, y - 40 + bob, 22, 10, 0xebce8b); r(x - 10, y - 32 + bob, 22, 3, 0xaf8d56); r(x - 5, y - 10 + bob, 13, 18, 0x77989b); }
        if (name != null) { r(x - 28, y - 54, 56, 17, 0x476349); label(name, (int)x, (int)y - 50, 10, 0xfbf3d7); }
    }
    static void crop(double x, double y, Plot p, double time)
    {
        if (p.crop < 0) return; Crop cr = CROPS[p.crop]; bool ripe = p.growth >= cr.days; int h = 5 + (p.growth < cr.days ? p.growth : cr.days) * 4;
        r(x - 1, y - h, 3, h, 0x50763e); r(x - 10, y - h + 2, 9, 4, 0x789a50); r(x + 2, y - h - 1, 9, 4, 0x5b8a45); r(x - 7, y - h - 2, 4, 4, 0x8cad5e);
        if (ripe) { r(x - 5, y - 5, 11, 10, cr.color); r(x - 2, y + 5, 5, 3, cr.color); if (p.crop == 2) { r(x - 10, y - 8, 7, 8, cr.color); r(x + 5, y - 7, 7, 9, cr.color); r(x + 7, y - 5, 2, 2, 0xffe5b1); } if (Math.Sin(time * 3 + x) > .5f) { r(x + 12, y - 22, 2, 7, 0xfff1b4); r(x + 10, y - 20, 6, 2, 0xfff1b4); } }
    }
    static void pond_draw(Scene scene, double time)
    {
        int x = (scene == FARM ? 26 : 25) * 32, y = 512, w = (scene == FARM ? 6 : 7) * 32, h = 128;
        r(x - 8, y + 8, w + 16, h - 16, 0x6e986a); r(x + 10, y - 7, w - 20, h + 14, 0x6e986a); r(x, y + 5, w, h - 10, 0x93bbae); r(x + 8, y, w - 16, h, 0x93bbae); r(x + 9, y + 9, w - 18, h - 18, 0x70a2a2);
        for (int i = 0; i < 18; i++) { double xx = x + 14 + (i * 47) % (w - 35) + Math.Sin(time + i) * 3; int yy = y + 15 + (i * 29) % (h - 30); r(xx, yy, 12, 2, 0xb9d1bc); }
        r(x + 32, y + 28, 18, 8, 0x548c61); r(x + 36, y + 23, 14, 7, 0x74a36c); r(x + 42, y + 24, 4, 4, 0xefd3ad);
        for (int i = 0; i < 5; i++) { r(x - 8 + i * 17, y + h - 10, 3, 16, 0x658c55); r(x - 10 + i * 17, y + h - 13, 5, 7, 0xa68a51); }
        if (scene == VILLAGE) { r(x + 35, y - 18, 88, 25, 0xaf9266); for (int j = 0; j < 6; j++) r(x + 36 + j * 15, y - 18, 2, 25, 0x87704e); r(x + 38, y - 24, 5, 33, 0x8e724c); r(x + 116, y - 24, 5, 33, 0x8e724c); }
    }
    public static void world_draw(Game g)
    {
        State s = g.s; Scene scene = s.scene; double time = g.elapsed; r(0, 0, 1120, 704, scene == FARM ? 0x9db77b : 0xa5bb83);
        for (int i = 0; i < 520; i++) { int x = (i * 157 + 19) % 1120, y = (i * 83 + 37) % 704; r(x, y, 3, i % 5 == 0 ? 5 : 2, i % 3 != 0 ? 0x91ad70 : 0xb2c58a); if (i % 19 == 0) r(x - 2, y - 1, 6, 3, i % 2 != 0 ? 0xe6dfb1 : 0xd6a7a1); }
        r(0, 345, 1120, 68, 0xc8b488); r(0, 351, 1120, 55, 0xdac69a);
        if (scene == FARM) { r(397, 0, 66, 704, 0xc8b488); r(403, 0, 54, 704, 0xdac69a); r(159, 221, 52, 150, 0xdac69a); }
        else { r(512, 190, 78, 514, 0xc8b488); r(518, 190, 66, 514, 0xdac69a); r(206, 245, 53, 120, 0xdac69a); r(833, 245, 53, 120, 0xdac69a); r(760, 375, 54, 168, 0xdac69a); r(497, 403, 115, 150, 0xdac69a); }
        for (int i = 0; i < 64; i++) r((i * 127) % 1120, 355 + (i * 13) % 40, 6, 2, 0xcbb689);
        for (int i = 0; i < 35; i++) { r(i * 32, 0, 31, 22, 0x668855); r(i * 32 + 4, 0, 23, 14, 0x7a985e); if (i < 12 || i > 15) r(i * 32, 689, 31, 15, 0x6c8d57); }
        pond_draw(scene, time);
        if (scene == FARM)
        {
            house(FARM_BUILDINGS[0]); r(529, 234, 283, 234, 0xb29b6d); r(535, 240, 271, 222, 0x8f7653);
            for (int i = 0; i < 48; i++) { Plot p = s.plots[i]; Point at = plot_position(i); double x = at.x, y = at.y; r(x, y, 30, 30, p.tilled ? (p.watered ? 0x78684d : 0xa78456) : 0xa6b77b); if (p.tilled) { for (int k = 0; k < 3; k++) r(x + 3, y + 6 + k * 8, 24, 2, p.watered ? 0x6a5c44 : 0x937349); if (p.watered) { r(x + 3, y + 3, 2, 2, 0xa0b4a7); r(x + 23, y + 21, 2, 2, 0x9bafa4); } } else r(x + 7, y + 11, 3, 5, 0x8a9f65); crop(x + 15, y + 20, p, time); if (g.hover == i) DrawRectangleLinesEx(new Rectangle(x, y, 30, 30), 2, color_hex(0xfff1b2)); }
            for (int i = 0; i < 11; i++) { int x = 514 + i * 29; for (int k = 0; k < 2; k++) { int y = k != 0 ? 476 : 215; r(x, y + 8, 29, 4, 0xc4a777); r(x, y + 19, 29, 4, 0xb49a6e); r(x, y + 2, 5, 29, 0xd8bc85); r(x - 1, y, 7, 5, 0xead09a); } }
            r(318, 280, 5, 31, 0x8d7454); r(307, 269, 28, 18, 0xb67d5b); r(310, 272, 21, 11, 0xca9970); r(333, 264, 3, 14, 0xe4c478); r(336, 264, 9, 5, 0xd3a65d); r(1017, 315, 5, 33, 0x95764c); r(988, 309, 62, 23, 0xd9bf87); label("VILA >", 1018, 316, 10, 0x6e6245);
        }
        else
        {
            for (int i = 0; i < 3; i++) house(VILLAGE_BUILDINGS[i]); r(455, 417, 196, 126, 0xb4b19b); for (int i = 0; i < 9; i++) r(460 + (i % 3) * 63, 421 + (i / 3) * 39, 58, 34, 0xc6c3a9);
            r(502, 450, 105, 56, 0x929c91); r(509, 444, 91, 54, 0xcbd1b7); r(516, 451, 77, 37, 0x8bb6b0); r(545, 426, 18, 47, 0xb8c3b1); r(538, 421, 32, 9, 0xd9dcc4); r(548, 415, 12, 8, 0xdfdfc9); r(550, 407 + Math.Sin(time * 3) * 2, 7, 12, 0xbddbd0);
            for (int k = 0; k < 2; k++) { int x = k != 0 ? 650 : 439; r(x, 449, 13, 48, 0xab835e); r(x - 2, 443, 17, 7, 0xc59b6b); r(x - 1, 432, 15, 11, 0x789757); r(x, 428, 7, 7, 0xe7c49a); }
            for (int i = 0; i < 3; i++) { int x = i == 1 ? 673 : 350, y = i == 2 ? 540 : 440; r(x, y, 53, 9, 0xab8961); r(x, y + 14, 53, 7, 0xc6a474); r(x + 4, y + 20, 5, 10, 0x816b4c); r(x + 44, y + 20, 5, 10, 0x816b4c); }
            r(338, 293, 5, 42, 0x94744d); r(390, 293, 5, 42, 0x94744d); r(329, 271, 74, 47, 0x9b7952); r(334, 276, 64, 36, 0xd5bd87); r(341, 282, 15, 19, 0xf3e8c3); r(363, 281, 25, 24, 0xe9ddb4); label("< FAZENDA", 67, 373, 10, 0x7b6b4b);
        }
        for (int i = 0; i < 6; i++) { Resource res = RESOURCES[i]; if (res.scene != scene || (s.gathered & (1u << i)) != 0) continue; int x = res.x * 32 + 16, y = res.y * 32 + 16; if (res.type == 0) { r(x - 14, y - 6, 28, 14, 0x8d714c); r(x - 16, y - 7, 10, 16, 0xc4a36e); r(x - 13, y - 4, 4, 10, 0x967849); r(x + 1, y - 5, 3, 12, 0xa58759); } else { r(x - 13, y - 3, 26, 13, 0x929c8d); r(x - 9, y - 9, 17, 11, 0xb2baa5); r(x - 7, y - 7, 8, 3, 0xccd0b7); } }
        // Sort trees and characters by their feet, so walking behind a tree is natural.
        RenderItem[] items = new RenderItem[24]; int length = 0;
        int[][] trees = scene == FARM ? FARM_TREES : VILLAGE_TREES; int count = scene == FARM ? 16 : 12;
        for (int i = 0; i < count; i++) items[length++] = new RenderItem(trees[i][1] * 32 + 20, 0, i);
        if (scene == VILLAGE) for (int i = 0; i < 4; i++) items[length++] = new RenderItem(g.npc_position[i].y, 1, i);
        items[length++] = new RenderItem(s.player.y, 2, 0);
        for (int i = 1; i < length; i++) { RenderItem a = items[i]; int j = i - 1; while (j >= 0 && items[j].y > a.y) { items[j + 1] = items[j]; j--; } items[j + 1] = a; }
        for (int i = 0; i < length; i++) { RenderItem a = items[i]; if (a.type == 0) tree(trees[a.index][0] * 32 + 16, trees[a.index][1] * 32 + 20); else if (a.type == 1) { int n = a.index; farmer(g.npc_position[n].x, g.npc_position[n].y, NPCS[n].coat, NPCS[n].hair, NPCS[n].name, g.npc_at[n] < g.npc_length[n] ? time : 0, false); } else farmer(s.player.x, s.player.y, 0xbc9a67, 0x765b42, null, g.moving ? time : 0, true); }
        if (g.path_at < g.path_length) { Point p = g.path[g.path_length - 1]; DrawEllipseLines((int)p.x, (int)p.y + 10, 9, 4, new Color(255, 244, 190, 170)); }
        for (int i = 0; i < 3; i++) { double x = 190 + i * 280 + Math.Sin(time * .3f + i) * 60, y = 85 + Math.Cos(time * .5f + i) * 25; r(x, y, 6, 2, 0xf5ecc7); r(x - 4, y - 3 + Math.Sin(time * 8) * 2, 5, 2, 0xf5ecc7); r(x + 6, y - 3 + Math.Sin(time * 8) * 2, 5, 2, 0xf5ecc7); }
        double evening = Math.Max(0, (s.minute - 1020) / 400.0f); if (evening > 0) DrawRectangle(0, 0, 1120, 704, new Color(32, 42, 64, (byte)(Math.Min(.42f, evening) * 255)));
        if (raining(s)) { DrawRectangle(0, 0, 1120, 704, new Color(94, 124, 140, 40)); for (int i = 0; i < 100; i++) { double x = Mod(i * 149 + time * 70, 1120), y = Mod(i * 83 + time * 370, 704); DrawRectangle((int)x, (int)y, 1, 9, new Color(215, 230, 223, 120)); } }
        if (g.fishing != 0) { Point p = s.player; Point a = new(p.x + 14, p.y - 14), b = new(p.x + 38, p.y - 35), c = new(p.x + 73, p.y + 30); DrawLineEx(a, b, 2, color_hex(0xd9c69c)); DrawLineEx(b, c, 1, color_hex(0xe1d6b0)); r(c.x - 3, c.y, 6, 5, g.fishing == 2 ? 0xf2ca79 : 0xe8c4aa); if (g.fishing == 2) { r(p.x - 60, p.y - 80, 120, 25, 0xf2d391); label("AGORA! [ESPAÇO]", (int)p.x, (int)p.y - 72, 10, 0x5a6041); } }
    }

}

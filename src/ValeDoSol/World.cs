using static ValeDoSol.Ray;
using static ValeDoSol.Logic;
using static ValeDoSol.GameLogic;
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
    static void farmer(double x, double y, uint coat, uint hair, string? name, double time, bool player, int facing = 0)
    {
        double step = time > 0 ? Math.Sin(time * 11) : 0, bob = time > 0 ? Math.Abs(step) * 1.5 : 0;
        DrawEllipse(x, y + 12, 14, 5, new Color(43, 69, 30, 75));
        r(x - 8, y + 5, 7, 10 + step * 3, 0x53676c); r(x + 2, y + 5, 7, 10 - step * 3, 0x53676c); r(x - 9, y + 13 + step * 3, 9, 4, 0x554334); r(x + 2, y + 13 - step * 3, 9, 4, 0x554334);
        r(x - 11, y - 12 + bob, 23, 22, coat); r(x - 14, y - 9 + bob + step * 2, 5, 15, 0xe8bb92); r(x + 12, y - 9 + bob - step * 2, 5, 15, 0xe8bb92);
        r(x - 8, y - 28 + bob, 18, 18, 0xeac29a); r(x - 9, y - 30 + bob, 20, 7, hair); r(x - 9, y - 25 + bob, 4, 12, hair);
        if (facing == 3) { r(x - 8, y - 27 + bob, 18, 14, hair); r(x - 8, y - 10 + bob, 19, 12, 0x94724f); } else { r(x + (facing == 1 ? -6 : 5), y - 20 + bob, 2, 3, 0x353e35); if (facing == 0) r(x - 4, y - 20 + bob, 2, 3, 0x353e35); r(x - 1, y - 13 + bob, 5, 1, 0xc18b6d); }
        if (player) { r(x - 16, y - 31 + bob, 33, 6, 0xc99d55); r(x - 11, y - 40 + bob, 23, 10, 0xe8c77b); r(x - 9, y - 39 + bob, 18, 3, 0xf8dfa0); r(x - 11, y - 32 + bob, 23, 3, 0xac7a43); if (facing != 3) { r(x - 6, y - 11 + bob, 14, 19, 0x638d9a); r(x - 6, y - 10 + bob, 3, 8, 0xa9b6a0); r(x + 5, y - 10 + bob, 3, 8, 0xa9b6a0); r(x - 2, y + 1 + bob, 7, 4, 0x87a7ad); } else { r(x - 8, y - 10 + bob, 18, 18, 0xa48254); r(x - 5, y - 7 + bob, 12, 10, 0xc5a16a); } }
        if (name != null) label(name, (int)x, (int)y - 55, 10, 0xfff2ce);
    }
    static void crop(double x, double y, Plot p, double time)
    {
        if (p.crop < 0) return; Crop cr = CROPS[p.crop]; bool ripe = p.growth >= cr.days; int h = 5 + (p.growth < cr.days ? p.growth : cr.days) * 4;
        r(x - 1, y - h, 3, h, 0x50763e); r(x - 10, y - h + 2, 9, 4, 0x789a50); r(x + 2, y - h - 1, 9, 4, 0x5b8a45); r(x - 7, y - h - 2, 4, 4, 0x8cad5e);
        if (ripe) { r(x - 5, y - 5, 11, 10, cr.color); r(x - 2, y + 5, 5, 3, cr.color); if (p.crop == 2) { r(x - 10, y - 8, 7, 8, cr.color); r(x + 5, y - 7, 7, 9, cr.color); r(x + 7, y - 5, 2, 2, 0xffe5b1); } if (Math.Sin(time * 3 + x) > .5f) { r(x + 12, y - 22, 2, 7, 0xfff1b4); r(x + 10, y - 20, 6, 2, 0xfff1b4); } }
    }
    static void pond_draw(Rectangle pond, double time, bool dock = false)
    {
        int x = (int)pond.x, y = (int)pond.y, w = (int)pond.width, h = (int)pond.height;
        r(x - 8, y + 8, w + 16, h - 16, 0x526d3b); r(x + 10, y - 7, w - 20, h + 14, 0x526d3b); r(x, y + 5, w, h - 10, 0x6baaa4); r(x + 8, y, w - 16, h, 0x6baaa4); r(x + 9, y + 9, w - 18, h - 18, 0x438f9d);
        for (int i = 0; i < w * h / 1200; i++) { double xx = x + 14 + i * 47 % (w - 35) + Math.Sin(time + i) * 3, yy = y + 15 + i * 29 % (h - 30); r(xx, yy, 12, 2, 0x96cac0); }
        r(x + 32, y + 28, 18, 8, 0x3b8054); r(x + 36, y + 23, 14, 7, 0x69a364); r(x + 42, y + 24, 4, 4, 0xf3c6b3);
        for (int i = 0; i < w / 23; i++) { r(x + 4 + i * 23, y + h - 10, 3, 16, 0x557d38); r(x + 2 + i * 23, y + h - 13, 5, 7, 0x9a7942); }
        if (dock) { r(x + 35, y - 18, 88, 25, 0xb08b54); for (int j = 0; j < 6; j++) r(x + 36 + j * 15, y - 18, 2, 25, 0x7b5b3c); r(x + 38, y - 24, 5, 33, 0x8e673e); r(x + 116, y - 24, 5, 33, 0x8e673e); }
    }
    static void interior(Game g)
    {
        bool home = g.s.scene == HOME; Point size = world_size(g.s.scene); int w = (int)size.x, h = (int)size.y;
        r(0, 0, w, h, 0x4c3023); r(24, 52, w - 48, h - 76, 0xb4814c);
        for (int y = 64; y < h - 24; y += 24) for (int x = 32; x < w - 32; x += 64) { r(x + (y / 24 % 2) * 12, y, 62, 22, (x + y) % 3 == 0 ? 0xbc8c57 : 0xb3804e); r(x + 3, y + 3, 48, 1, 0xca9b66); }
        r(24, 24, w - 48, 56, 0xe4bd80); r(24, 64, w - 48, 12, 0x805034); r(16, 16, 16, h - 32, 0x67442c); r(w - 32, 16, 16, h - 32, 0x67442c);
        for (int x = 40; x < w - 40; x += 32) { r(x, 29, 2, 32, 0xcda56e); r(x, 72, 27, 3, 0xa57648); }
        int door = home ? 320 : 352; r(32, h - 32, w - 64, 16, 0x66412c); r(door - 32, h - 36, 64, 28, 0xe1b26b); r(door - 36, h - 44, 72, 8, 0x97633b);
        // Window, daylight and curtains.
        r(256, 24, 96, 48, 0x765034); r(262, 29, 84, 35, 0x8dc6ce); r(302, 29, 4, 35, 0xefe0aa); r(262, 44, 84, 3, 0xefe0aa); r(250, 24, 10, 51, 0xbc705b); r(348, 24, 10, 51, 0xbc705b);
        DrawRectangle(263, 80, 86, 140, new Color(255, 234, 152, 22));
        if (home)
        {
            r(214, 220, 156, 90, 0x985447); r(220, 226, 144, 78, 0xb7755a); r(228, 233, 128, 63, 0xc78d66); r(240, 244, 104, 40, 0xb97756);
            r(432, 96, 112, 132, 0x765036); r(440, 105, 96, 116, 0xe3c28d); r(447, 110, 82, 28, 0xf6e7be); r(441, 144, 94, 74, 0xa95648); for (int y = 148; y < 216; y += 16) r(445, y, 86, 2, 0xc8755a); r(476, 146, 4, 68, 0xe5b380); r(433, 222, 14, 13, 0x533822); r(530, 222, 13, 13, 0x533822);
            r(112, 112, 96, 56, 0x8b5a32); r(117, 112, 86, 44, 0xbe8d54); r(120, 158, 8, 17, 0x70472b); r(192, 158, 8, 17, 0x70472b); r(142, 122, 24, 17, 0xe6c990); r(174, 119, 14, 16, 0x74985b);
            r(48, 48, 96, 64, 0x716049); r(56, 61, 80, 40, 0x59513f); r(70, 74, 48, 29, 0x322d29); r(79, 91, 27, 10, 0xe59f4c); r(88, 82, 14, 18, 0xf4c96a); r(42, 106, 108, 9, 0x9c8761);
            r(512, 288, 64, 48, 0x986a40); r(516, 293, 56, 37, 0xb58b51); r(519, 303, 50, 3, 0x795332); r(546, 300, 5, 8, 0xe0bb6f);
            r(562, 94, 18, 25, 0x9a6543); r(556, 83, 30, 15, 0x6d954e); r(566, 72, 10, 17, 0x83aa5c);
            farmer(g.s.player.x, g.s.player.y, 0xbc9a67, 0x765b42, null, g.moving ? g.elapsed : 0, true, g.facing);
        }
        else
        {
            for (int side = 0; side < 2; side++) { int x = side == 0 ? 48 : 560; for (int y = 104; y < 340; y += 64) { r(x, y, 96, 48, 0x855b37); r(x + 3, y + 3, 90, 36, 0xc6995b); r(x, y + 39, 96, 8, 0x65452e); for (int k = 0; k < 4; k++) { r(x + 10 + k * 20, y + 8, 12, 24, k % 2 == 0 ? 0x9aa05c : 0xcb8d55); r(x + 12 + k * 20, y + 10, 8, 7, 0xe6d19a); } } }
            farmer(352, 184, NPCS[0].coat, NPCS[0].hair, null, 0, false);
            r(224, 208, 256, 48, 0x815330); r(220, 202, 264, 15, 0xc99b5b); r(224, 250, 256, 8, 0x684329); for (int x = 232; x < 470; x += 32) r(x, 221, 2, 24, 0x996b3d); r(413, 183, 39, 21, 0x6b7152); r(418, 175, 29, 13, 0xc5c09a);
            r(302, 382, 100, 47, 0x9d6247); r(308, 387, 88, 37, 0xc08b5b);
            farmer(g.s.player.x, g.s.player.y, 0xbc9a67, 0x765b42, null, g.moving ? g.elapsed : 0, true, g.facing);
        }
    }
    public static void world_draw(Game g)
    {
        State s = g.s; Scene scene = s.scene; double time = g.elapsed;
        if (scene is HOME or SHOP) { interior(g); return; }
        r(0, 0, WORLD_W, WORLD_H, 0x7daf53);
        int minx = Math.Max(0, (int)(g.camera.x - UI_W / (2 * CAMERA_ZOOM) - 100) / 32), maxx = Math.Min(WORLD_W / 32, (int)(g.camera.x + UI_W / (2 * CAMERA_ZOOM) + 100) / 32 + 1), miny = Math.Max(0, (int)(g.camera.y - UI_H / (2 * CAMERA_ZOOM) - 100) / 32), maxy = Math.Min(WORLD_H / 32, (int)(g.camera.y + UI_H / (2 * CAMERA_ZOOM) + 100) / 32 + 1);
        for (int y = miny; y < maxy; y++) for (int x = minx; x < maxx; x++) { uint n = (uint)(x * 73856093 ^ y * 19349663); r(x * 32, y * 32, 32, 32, n % 4 == 0 ? 0x7fb354 : n % 4 == 1 ? 0x83b757 : 0x7baf50); r(x * 32 + n % 20 + 3, y * 32 + n % 23 + 3, 2, 4, 0x6a9e43); if (n % 5 == 0) { r(x * 32 + 11, y * 32 + 21, 5, 2, 0x9ac66b); r(x * 32 + 13, y * 32 + 18, 2, 5, 0x9ac66b); } if (n % 19 == 0) { r(x * 32 + 19, y * 32 + 11, 4, 4, n % 2 == 0 ? 0xf0d18c : 0xdab0ba); r(x * 32 + 20, y * 32 + 15, 2, 5, 0x5a903c); } }
        r(0, 344, WORLD_W, 72, 0xb18d50); r(0, 350, WORLD_W, 60, 0xd9b774);
        if (scene == FARM) { r(397, 0, 66, WORLD_H, 0xb18d50); r(403, 0, 54, WORLD_H, 0xd9b774); r(159, 221, 52, 145, 0xd9b774); }
        else { r(512, 180, 78, WORLD_H - 180, 0xb18d50); r(518, 180, 66, WORLD_H - 180, 0xd9b774); r(206, 245, 53, 120, 0xd9b774); r(833, 245, 53, 120, 0xd9b774); r(760, 375, 54, 168, 0xd9b774); }
        r(1024, 382, 64, 650, 0xb18d50); r(1030, 382, 52, 650, 0xd9b774); r(1050, 992, 426, 48, 0xd9b774);
        for (int i = 0; i < 180; i++) r(i * 127 % WORLD_W, 355 + i * 13 % 43, 5, 2, 0xc8a364);
        foreach (var pond in water_bodies(scene)) pond_draw(pond, time, scene == VILLAGE || pond.x > 1300);
        if (scene == FARM)
        {
            house(FARM_BUILDINGS[0]); r(529, 234, 283, 234, 0xa9864f); r(535, 240, 271, 222, 0x89643e);
            for (int i = 0; i < 48; i++) { Plot p = s.plots[i]; Point at = plot_position(i); double x = at.x, y = at.y; r(x, y, 30, 30, p.tilled ? (p.watered ? 0x715238 : 0xa87945) : 0x829b4b); if (p.tilled) { for (int k = 0; k < 3; k++) r(x + 3, y + 6 + k * 8, 24, 2, p.watered ? 0x614832 : 0x8c603b); if (p.watered) { r(x + 3, y + 3, 2, 2, 0x8b9f91); r(x + 23, y + 21, 2, 2, 0x8b9f91); } } else r(x + 7, y + 11, 3, 5, 0x648b39); crop(x + 15, y + 20, p, time); if (g.hover == i && Math.Abs(s.player.x - (x + 16)) < 78 && Math.Abs(s.player.y - (y + 16)) < 78) DrawRectangleLinesEx(new(x, y, 30, 30), 1, color_hex(0xffe5a4)); }
            for (int i = 0; i < 11; i++) { int x = 514 + i * 29; for (int k = 0; k < 2; k++) { int y = k == 1 ? 476 : 215; r(x, y + 8, 29, 4, 0xbc9255); r(x, y + 19, 29, 4, 0xa77d46); r(x, y + 2, 5, 29, 0xd3ad6e); r(x - 1, y, 7, 5, 0xe7c483); } }
            r(318, 280, 5, 31, 0x8d6339); r(307, 269, 28, 18, 0xb3704a); r(310, 272, 21, 11, 0xcf9661); r(333, 264, 3, 14, 0xe4c478); r(336, 264, 9, 5, 0xd3a65d);
            for (int i = 0; i < 8; i++) { r(88 + i * 23, 250, 16, 12, 0x9c6a3c); r(91 + i * 23, 240, 10, 13, 0x558d3f); r(88 + i * 23, 237, 16, 7, i % 2 == 0 ? 0xe4b96c : 0xd8989d); }
            r(WORLD_W - 120, 325, 6, 33, 0x835c35); r(WORLD_W - 150, 309, 73, 25, 0xd4aa65); label("VILA >", WORLD_W - 114, 317, 10, 0x614626);
            for (int i = 0; i < 4; i++) { double x = 273 + Math.Sin(time * .5 + i) * 4, y = 62 - i * 14 - time * 3 % 14; DrawRectangle((int)x, (int)y, 12 + i * 3, 7, new Color(221, 213, 184, (byte)(75 - i * 14))); }
        }
        else
        {
            foreach (var b in VILLAGE_BUILDINGS) house(b); r(455, 417, 196, 126, 0xb3a283); for (int i = 0; i < 9; i++) r(460 + i % 3 * 63, 421 + i / 3 * 39, 58, 34, 0xcebb94);
            r(502, 450, 105, 56, 0x8b8e78); r(509, 444, 91, 54, 0xd0c8a7); r(516, 451, 77, 37, 0x70b1b3); r(545, 426, 18, 47, 0xaaa98d); r(538, 421, 32, 9, 0xd9d3b7); r(548, 415, 12, 8, 0xe7dfbf); r(550, 407 + Math.Sin(time * 3) * 2, 7, 12, 0xbddbd0);
            for (int i = 0; i < 3; i++) { int x = i == 1 ? 673 : 350, y = i == 2 ? 540 : 440; r(x, y, 53, 9, 0xa57b46); r(x, y + 14, 53, 7, 0xc6985b); r(x + 4, y + 20, 5, 10, 0x755435); r(x + 44, y + 20, 5, 10, 0x755435); }
            r(338, 293, 5, 42, 0x94744d); r(390, 293, 5, 42, 0x94744d); r(329, 271, 74, 47, 0x9b7952); r(334, 276, 64, 36, 0xd5bd87); r(341, 282, 15, 19, 0xf3e8c3); r(363, 281, 25, 24, 0xe9ddb4); label("< FAZENDA", 77, 321, 10, 0x66482a);
        }
        for (int i = 0; i < 6; i++) { var res = RESOURCES[i]; if (res.scene != scene || (s.gathered & (1u << i)) != 0) continue; int x = res.x * 32 + 16, y = res.y * 32 + 16; if (res.type == 0) { r(x - 14, y - 6, 28, 14, 0x805731); r(x - 16, y - 7, 10, 16, 0xc59a5d); r(x - 13, y - 4, 4, 10, 0x967849); r(x + 1, y - 5, 3, 12, 0xa58759); } else { r(x - 13, y - 3, 26, 13, 0x899581); r(x - 9, y - 9, 17, 11, 0xb6bca0); r(x - 7, y - 7, 8, 3, 0xd0d1b7); } }
        var trees = scene_trees(scene); List<RenderItem> items = new(); for (int i = 0; i < trees.Length; i++) if (Math.Abs(trees[i][0] * 32 - g.camera.x) < UI_W / (2 * CAMERA_ZOOM) + 120 && Math.Abs(trees[i][1] * 32 - g.camera.y) < UI_H / (2 * CAMERA_ZOOM) + 120) items.Add(new(trees[i][1] * 32 + 20, 0, i));
        if (scene == VILLAGE) for (int i = 0; i < 4; i++) items.Add(new(g.npc_position[i].y, 1, i)); items.Add(new(s.player.y, 2, 0)); items.Sort((a, b) => a.y.CompareTo(b.y));
        foreach (var item in items) { if (item.type == 0) tree(trees[item.index][0] * 32 + 16, trees[item.index][1] * 32 + 20); else if (item.type == 1) { int n = item.index; bool near = Math.Abs(g.npc_position[n].x - s.player.x) < 90 && Math.Abs(g.npc_position[n].y - s.player.y) < 90; farmer(g.npc_position[n].x, g.npc_position[n].y, NPCS[n].coat, NPCS[n].hair, near ? NPCS[n].name : null, g.npc_at[n] < g.npc_length[n] ? time : 0, false); } else farmer(s.player.x, s.player.y, 0xbc9a67, 0x765b42, null, g.moving ? time : 0, true, g.facing); }
        if (g.action_timer > 0) { double phase = 1 - g.action_timer / .32f; Point a = s.player, p = g.action_target; if (g.tool == 2) { for (int i = 0; i < 5; i++) r(p.x - 13 + i * 6, p.y - 16 + phase * 18 + (i % 2) * 3, 2, 4, 0x8ccbd4); } else { DrawLineEx(new(a.x + 9, a.y - 12), new(p.x - 8, p.y - 10 - Math.Sin(phase * Math.PI) * 18), 3, color_hex(0x9a6838)); r(p.x - 14, p.y - 12 - Math.Sin(phase * Math.PI) * 18, 14, 5, g.tool == 0 ? 0x8da3a1 : 0xe3c783); } for (int i = 0; i < 4; i++) r(p.x - 12 + i * 8, p.y + phase * 5, 2, 2, 0xcba365); }
        double evening = Math.Max(0, (s.minute - 1020) / 400f); if (evening > 0) DrawRectangle(0, 0, WORLD_W, WORLD_H, new Color(28, 32, 66, (byte)(Math.Min(.52, evening) * 255)));
        if (raining(s)) { DrawRectangle(0, 0, WORLD_W, WORLD_H, new Color(65, 99, 125, 43)); for (int i = 0; i < 190; i++) { double x = g.camera.x - UI_W / (2 * CAMERA_ZOOM) + Mod(i * 149 + time * 70, UI_W / 2 + 80), y = g.camera.y - UI_H / (2 * CAMERA_ZOOM) + Mod(i * 83 + time * 370, UI_H / 2 + 80); DrawRectangle(x, y, 1, 9, new Color(201, 227, 224, 125)); } }
        if (g.fishing != 0) { Point p = s.player; Point a = new(p.x + 14, p.y - 14), b = new(p.x + 38, p.y - 35), c = new(p.x + 73, p.y + 30); DrawLineEx(a, b, 2, color_hex(0xd9c69c)); DrawLineEx(b, c, 1, color_hex(0xe1d6b0)); r(c.x - 3, c.y, 6, 5, g.fishing == 2 ? 0xf2ca79 : 0xe8c4aa); }
    }
}

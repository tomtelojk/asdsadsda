using static ValeDoSol.Ray;
using static ValeDoSol.World;
namespace ValeDoSol;

/// <summary>Village landmarks share their foot points with the collision model.</summary>
public static class TownArt
{
    static void R(double x, double y, double width, double height, long color) => DrawRectangle(x, y, width, height, color_hex(color));
    static void Text(string value, double x, double y, int size, long color) => DrawText(value, x, y, size, color_hex(color));
    static void Triangle(Point a, Point b, Point c, Color color)
    {
        float cross = (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        if (cross > 0) DrawTriangle(c, b, a, color); else DrawTriangle(a, b, c, color);
    }

    public static void Ground(Game g)
    {
        Paving(new(416, 416, 320, 224), 0xb5ac89, 0x9a9476);
        Paving(new(1152, 896, 384, 64), 0xc4b998, 0xaaa184);
        Paving(new(1280, 944, 96, 56), 0xc4b998, 0xaaa184);
        // A small compass rose in the civic forecourt.
        int x = 1464, y = 928;
        DrawCircle(x, y, 20, color_hex(0xaaa184)); DrawCircle(x, y, 16, color_hex(0xd6cbab));
        Triangle(new(x, y - 15), new(x - 4, y + 3), new(x + 4, y + 3), color_hex(0x647c73));
        Triangle(new(x, y + 14), new(x + 4, y - 2), new(x - 4, y - 2), color_hex(0xa78356));
        foreach (var prop in VillageProps.All)
        {
            if (prop.kind != VillageKind.Lamp) continue;
            Rectangle f = prop.footprint;
            R(f.x - 7, f.y - 7, f.width + 14, f.height + 14, 0xb5ac89);
            R(f.x - 5, f.y - 5, f.width + 10, f.height + 10, 0xd0c19c);
        }
    }
    static void Paving(Rectangle area, long baseColor, long grout)
    {
        R(area.x - 3, area.y - 3, area.width + 6, area.height + 6, grout);
        R(area.x, area.y, area.width, area.height, baseColor);
        for (int row = 0; row < area.height / 12 + 1; row++)
            for (int col = 0; col < area.width / 24 + 1; col++)
            {
                double x = area.x + col * 24 - row % 2 * 12, y = area.y + row * 12;
                double left = Math.Max(x, area.x), right = Math.Min(x + 22, area.x + area.width);
                if (right > left && y + 10 <= area.y + area.height)
                {
                    long shade = (row + col) % 5 == 0 ? 0xc5bc9b : (row + col) % 3 == 0 ? 0xaaa487 : baseColor;
                    R(left, y, right - left, 10, shade); R(left, y, right - left, 1, 0xd7ccaa);
                }
            }
    }

    public static void DrawProp(Game g, VillageProp prop)
    {
        Rectangle b = prop.visual, f = prop.footprint;
        double x = b.x, y = b.y, w = b.width, h = b.height, bottom = f.y + f.height;
        DrawEllipse(f.x + f.width / 2, bottom, Math.Max(8, f.width * .55), 4, new Color(32, 39, 27, 42));
        switch (prop.kind)
        {
            case VillageKind.Bench:
                R(x + 4, bottom - 5, 5, 7, 0x453e30); R(x + w - 9, bottom - 5, 5, 7, 0x453e30);
                R(x + 5, y + 4, 3, h - 5, 0x52564a); R(x + w - 8, y + 4, 3, h - 5, 0x52564a);
                for (int i = 0; i < 3; i++) { R(x, y + i * 5, w, 4, 0x8f633d); R(x + 1, y + i * 5, w - 2, 1, 0xc29a61); }
                R(x - 2, bottom - 9, w + 4, 5, 0x765035); R(x - 1, bottom - 10, w + 2, 3, 0xc49b5e);
                R(x + 2, bottom - 15, 2, 7, 0x51564a); R(x + w - 4, bottom - 15, 2, 7, 0x51564a);
                break;
            case VillageKind.Planter:
                R(x, bottom - 17, w, 16, 0x73553b); R(x + 2, bottom - 16, w - 4, 11, 0xc7aa78);
                R(x - 2, bottom - 19, w + 4, 5, 0xe1c592); R(x + 2, bottom - 19, w - 4, 2, 0x5c4c31);
                for (int i = 0; i < Math.Max(2, (int)w / 12); i++)
                {
                    double px = x + 7 + i * 12, py = bottom - 24 - i % 3 * 3;
                    R(px, py, 2, bottom - 17 - py, 0x3f6738); R(px - 4, py + 5, 5, 3, 0x7d9d4a); R(px + 2, py + 2, 5, 3, 0x5e873e);
                    Flower(px, py, i % 3 == 0 ? 0xdf8c9b : i % 3 == 1 ? 0xf2d584 : 0xebdfbb);
                }
                break;
            case VillageKind.Lamp:
                double cx = f.x + f.width / 2;
                bool lit = g.s.minute >= 1020 || Logic.raining(g.s);
                if (lit) { DrawCircle(cx, y + 19, 30, new Color(255, 211, 118, 18)); DrawCircle(cx, y + 19, 19, new Color(255, 211, 118, 26)); }
                R(cx - 6, bottom - 4, 12, 5, 0x454b40); R(cx - 3, y + 26, 6, bottom - y - 28, 0x374b43); R(cx - 1, y + 29, 2, bottom - y - 34, 0x68806b);
                R(cx - 10, y + 8, 20, 24, 0x34483f); R(cx - 7, y + 12, 14, 16, lit ? 0xf5d28c : 0xa6c0b0);
                R(cx - 1, y + 10, 2, 21, 0x5f6950); R(cx - 9, y + 27, 18, 4, 0x3e5146);
                Triangle(new(cx - 12, y + 9), new(cx, y), new(cx + 12, y + 9), color_hex(0x456858));
                R(cx - 2, y - 3, 4, 6, 0xb99456); R(cx - 7, y + 12, 2, 12, 0xe8e0b7);
                break;
            case VillageKind.NoticeBoard:
                R(x + 7, y + 15, 6, h - 15, 0x745336); R(x + w - 13, y + 15, 6, h - 15, 0x745336);
                R(x + 3, y + 7, w - 6, h - 22, 0x65472f); R(x + 7, y + 11, w - 14, h - 30, 0xb49362);
                R(x + 12, y + 15, 20, 24, 0xf0dfb1); R(x + 36, y + 17, w - 48, 19, 0xe5c994);
                R(x + 14, y + 19, 15, 2, 0x9f8458); R(x + 14, y + 24, 12, 1, 0xab946f); R(x + 14, y + 28, 14, 1, 0xab946f);
                R(x + 39, y + 21, Math.Max(4, w - 55), 2, 0xa17b50); R(x + 39, y + 27, Math.Max(3, w - 59), 1, 0xa17b50);
                Triangle(new(x - 3, y + 9), new(x + w / 2, y - 8), new(x + w + 3, y + 9), color_hex(0x496b54));
                R(x - 3, y + 7, w + 6, 4, 0x2f4e40); R(x + 10, y + 14, 2, 2, 0x8f5c3a); R(x + 38, y + 16, 2, 2, 0x8f5c3a);
                break;
            case VillageKind.Well:
                DrawEllipse(x + w / 2, bottom - 10, w / 2, 14, color_hex(0x6d7263)); DrawEllipse(x + w / 2, bottom - 15, w / 2, 13, color_hex(0xbab69b));
                DrawEllipse(x + w / 2, bottom - 18, w / 2 - 7, 7, color_hex(0x44584c));
                R(x + 5, y + 18, 5, h - 22, 0x795c3b); R(x + w - 10, y + 18, 5, h - 22, 0x795c3b);
                Triangle(new(x - 7, y + 22), new(x + w / 2, y), new(x + w + 7, y + 22), color_hex(0x4f7b60));
                R(x - 7, y + 20, w + 14, 4, 0x35533f); R(x + w / 2, y + 22, 1, 27, 0xd6bd83); R(x + w / 2 - 5, y + 46, 10, 9, 0x96734d);
                break;
            case VillageKind.MarketStall:
                R(x + 5, y + 18, 5, h - 18, 0x705137); R(x + w - 10, y + 18, 5, h - 18, 0x705137);
                R(x + 2, bottom - 24, w - 4, 22, 0x885d3c); R(x, bottom - 28, w, 6, 0xd1ab72);
                for (int i = 0; i < (int)w / 16; i++) { R(x - 4 + i * 16, y + 7, 16, 17, i % 2 == 0 ? 0x779760 : 0xe6cf98); R(x - 4 + i * 16, y + 24, 16, 5, i % 2 == 0 ? 0x506c45 : 0xc8ac76); }
                for (int i = 0; i < 5; i++) { int px = (int)x + 12 + i * 16; DrawCircle(px, bottom - 32, 5, color_hex(i % 2 == 0 ? 0xc96c52 : 0xe1ac57)); R(px - 1, bottom - 39, 2, 4, 0x51733e); }
                break;
            case VillageKind.Sign:
                R(x + w / 2 - 3, y + 8, 6, h - 8, 0x705438); R(x, y, w, 21, 0x5c442d); R(x + 2, y + 2, w - 4, 17, 0xc1a16a);
                Text("PREFEITURA", x + (w - MeasureText("PREFEITURA", 7)) / 2, y + 7, 7, 0x5a402a);
                break;
        }
    }
    static void Flower(double x, double y, long color)
    {
        R(x - 3, y - 1, 7, 3, color); R(x - 1, y - 3, 3, 7, color); R(x - 1, y - 1, 3, 3, 0xc99544);
    }

    public static void Hall(Building b, bool night)
    {
        int x = b.x * 32, y = b.y * 32, w = b.w * 32, h = b.h * 32, cx = x + w / 2;
        DrawRectangle(x + 5, y + h - 2, w + 5, 15, new Color(31, 37, 27, 52));
        R(x + 5, y + 60, w - 10, h - 60, 0xd6c7a0);
        for (int row = 0; row < (h - 64) / 12; row++)
            for (int col = 0; col < w / 24; col++)
            {
                int bx = x + 8 + col * 24 - row % 2 * 12, by = y + 65 + row * 12;
                int left = Math.Max(x + 8, bx), right = Math.Min(x + w - 8, bx + 22);
                if (right > left) { R(left, by, right - left, 10, (row + col) % 5 == 0 ? 0xcbb98f : 0xe2d5b2); R(left, by, right - left, 1, 0xf0e3c0); }
            }
        R(x + 3, y + h - 14, w - 6, 14, 0x8b8c77); R(x + 7, y + h - 12, w - 14, 3, 0xbdb99e);
        R(x - 8, y + 65, w + 16, 11, 0x3b5146);
        for (int row = 0; row < 72; row += 4)
        {
            int inset = (72 - row) / 2;
            R(x - 9 + inset, y - 4 + row, w + 18 - inset * 2, 4, row % 12 == 0 ? 0x567868 : row % 12 == 4 ? 0x3e6153 : 0x476e5c);
            for (int col = 0; col < w / 20; col++) { int px = x + col * 20 + row % 8; if (px > x - 9 + inset && px < x + w + 9 - inset) R(px, y - 3 + row, 1, 3, 0x2e5245); }
        }
        R(x + 27, y - 8, w - 54, 4, 0xc3a06b);
        foreach (int wx in new[] { x + 25, x + w - 57 })
        {
            R(wx - 4, y + 91, 40, 54, 0x8b7a57); R(wx - 2, y + 93, 36, 50, 0xe9dab8);
            if (night) R(wx + 3, y + 100, 26, 36, 0xf5d394);
            Scenery.AtFoot("window", wx + 16, y + 140);
            FlowerBox(wx - 2, y + 146, 36);
        }
        // Clock tower and the brass sun emblem distinguish the civic building.
        R(cx - 30, y - 55, 60, 110, 0x8f7854); R(cx - 27, y - 55, 54, 106, 0xe4d6af); R(cx - 22, y - 51, 44, 98, 0xc9bc94);
        R(cx - 34, y - 59, 68, 7, 0xdac89a); R(cx - 32, y + 46, 64, 7, 0xb59a6c);
        Triangle(new(cx - 38, y - 59), new(cx, y - 92), new(cx + 38, y - 59), color_hex(0x365a48));
        Triangle(new(cx - 29, y - 61), new(cx, y - 84), new(cx + 29, y - 61), color_hex(0x527d5f));
        R(cx - 2, y - 100, 4, 15, 0xc5a45f); DrawCircle(cx, y - 102, 4, color_hex(0xe4c57a));
        DrawCircle(cx, y - 21, 22, color_hex(0x8b744d)); DrawCircle(cx, y - 21, 19, color_hex(0xf0dfb4)); DrawCircle(cx, y - 21, 17, color_hex(0xf9ecc9));
        for (int i = 0; i < 12; i++) { double a = i * Math.PI / 6; R(cx - .5 + Math.Sin(a) * 14, y - 22 + Math.Cos(a) * 14, 2, 2, 0x8e7954); }
        DrawLineEx(new(cx, y - 21), new(cx, y - 32), 2, color_hex(0x5b543d)); DrawLineEx(new(cx, y - 21), new(cx + 8, y - 17), 2, color_hex(0x5b543d)); DrawCircle(cx, y - 21, 2, color_hex(0xa4864b));
        R(cx - 40, y + 22, 80, 31, 0xe6d6ac); R(cx - 37, y + 25, 74, 25, 0x466957);
        DrawCircle(cx, y + 36, 7, color_hex(0xe2bc68)); R(cx - 2, y + 35, 4, 11, 0xecd18d);
        DrawLineEx(new(cx - 12, y + 31), new(cx - 6, y + 45), 2, color_hex(0x9aba73)); DrawLineEx(new(cx + 12, y + 31), new(cx + 6, y + 45), 2, color_hex(0x9aba73));
        // Entry portico has columns, a distinct pediment and a full door sprite.
        R(cx - 63, y + 113, 126, h - 113, 0xbaa886); R(cx - 58, y + 121, 116, h - 127, 0xcdbd99);
        Scenery.AtFoot("door_alt", cx, y + h);
        foreach (int column in new[] { cx - 56, cx + 44 })
        {
            R(column - 3, y + 128, 18, 7, 0xf0e0b6); R(column, y + 135, 12, h - 144, 0xd9cca8); R(column + 2, y + 137, 3, h - 150, 0xf1e5c3); R(column + 9, y + 136, 3, h - 149, 0xb39f77); R(column - 4, y + h - 11, 20, 11, 0xa7a28a);
        }
        Triangle(new(cx - 76, y + 128), new(cx, y + 91), new(cx + 76, y + 128), color_hex(0x3d5e4d));
        Triangle(new(cx - 62, y + 126), new(cx, y + 99), new(cx + 62, y + 126), color_hex(0x6d8c69));
        R(cx - 76, y + 128, 152, 6, 0xdac99f); R(cx - 58, y + 139, 116, 18, 0x456650);
        Text("PREFEITURA", cx - MeasureText("PREFEITURA", 10) / 2, y + 143, 10, 0xffedbd);
        foreach (int side in new[] { -1, 1 })
        {
            int fx = cx + side * 95;
            R(fx - 1, y + 107, 2, 56, 0xb99756); R(fx - 10, y + 113, 20, 40, 0x3b7463); R(fx - 7, y + 115, 14, 28, 0x508a71);
            Triangle(new(fx - 10, y + 153), new(fx, y + 163), new(fx + 10, y + 153), color_hex(0x3b7463)); Flower(fx, y + 129, 0xe5c47d);
        }
        R(cx - 51, y + h, 102, 6, 0xc8b998); R(cx - 62, y + h + 6, 124, 7, 0xa4a48d); R(cx - 72, y + h + 13, 144, 5, 0xd3c6a6);
    }
    static void FlowerBox(double x, double y, double width)
    {
        R(x, y + 9, width, 11, 0x9a744d); R(x - 2, y + 7, width + 4, 4, 0xc6a272);
        for (int i = 0; i < width / 9; i++) { double px = x + 4 + i * 9; R(px, y, 2, 10, 0x547238); Flower(px, y - i % 2 * 3, i % 2 == 0 ? 0xe4a2aa : 0xf0d996); }
    }
}

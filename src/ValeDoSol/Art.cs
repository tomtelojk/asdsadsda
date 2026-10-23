using System.Runtime.InteropServices;
using static ValeDoSol.Ray;
namespace ValeDoSol;

// Original, editable pixel art. Each frame owns a padded 128px cell; the alpha
// gutters prevent neighbouring frames, labels or background pixels leaking in.
public static class Art
{
    public const int Cell = 128, Columns = 16, Rows = 12;
    public const int Tree = 144, Bush = 145, Flowers = 146, Rock = 147, Log = 148, Fence = 149, Crop = 150, Portrait = 176;
    static Texture atlas;
    public static bool Ready => atlas.id != 0;
    public static readonly uint[] Coats = [0x386b46, 0x416a42, 0x537b91, 0x8b72a8, 0x6b8648, 0x913f50, 0x708354, 0x398a87, 0x8b6a4b];
    public static readonly uint[] Hairs = [0x5a3529, 0xa94b29, 0x5b3f30, 0x302f38, 0x74503a, 0xc3bab0, 0x343034, 0x47302d, 0xaba090];
    static readonly uint[] Skins = [0xe9b887, 0xefbd91, 0xd9a076, 0xae7653, 0xe4b68b, 0xe1b092, 0xd6a37b, 0xb87c56, 0xdab38e];
    public static uint Shade(uint c, int change) => (uint)(Math.Clamp((int)(c >> 16) + change, 0, 255) << 16 | Math.Clamp((int)(c >> 8 & 255) + change, 0, 255) << 8 | Math.Clamp((int)(c & 255) + change, 0, 255));
    public sealed class Canvas(int width, int height)
    {
        public readonly byte[] Pixels = new byte[width * height * 4];
        public int Width => width;
        public int Height => height;
        public int X, Y;
        public void Pixel(int x, int y, uint color, byte alpha = 255) { if (x < 2 || y < 2 || x >= 126 || y >= 126) return; x += X; y += Y; if (x < 0 || y < 0 || x >= width || y >= height) return; int n = (y * width + x) * 4; Pixels[n] = (byte)(color >> 16); Pixels[n + 1] = (byte)(color >> 8); Pixels[n + 2] = (byte)color; Pixels[n + 3] = alpha; }
        public void Glint(int x, int y, uint c) { if (Pixels[((Y + y) * width + X + x) * 4 + 3] != 0) Pixel(x, y, c); }
        public void Rect(int x, int y, int w, int h, uint c) { for (int dy = 0; dy < h; dy++) for (int dx = 0; dx < w; dx++) Pixel(x + dx, y + dy, c); }
        public void Ellipse(int x, int y, int rx, int ry, uint c) { for (int dy = -ry; dy <= ry; dy++) for (int dx = -rx; dx <= rx; dx++) if ((double)dx * dx / (rx * rx) + (double)dy * dy / (ry * ry) <= 1) Pixel(x + dx, y + dy, c); }
        public void Shape(int x, int y, int rx, int ry, uint c, uint edge = 0x49352c) { Ellipse(x, y, rx, ry, edge); for (int dy = -ry + 1; dy < ry; dy++) for (int dx = -rx + 1; dx < rx; dx++) { double nx = (double)dx / Math.Max(1, rx - 1), ny = (double)dy / Math.Max(1, ry - 1); if (nx * nx + ny * ny > 1) continue; int light = (int)Math.Round((-nx * 9 - ny * 8 - nx * nx * 5) / 4) * 4; Pixel(x + dx, y + dy - 1, Shade(c, light)); } }
        public void Line(int x, int y, int x2, int y2, uint c, int thick = 1) { int steps = Math.Max(Math.Abs(x2 - x), Math.Abs(y2 - y)); for (int i = 0; i <= steps; i++) Rect(x + (steps == 0 ? 0 : (x2 - x) * i / steps), y + (steps == 0 ? 0 : (y2 - y) * i / steps), thick, thick, c); }
        public void Leaf(int x, int y, int rx, int ry, uint c) { Shape(x, y, rx, ry, c, 0x315431); Ellipse(x - rx / 4, y - ry / 3, Math.Max(1, rx / 2), Math.Max(1, ry / 3), Shade(c, 19)); }
        public void Flower(int x, int y, uint c) { Ellipse(x, y - 2, 2, 2, c); Ellipse(x - 2, y, 2, 2, c); Ellipse(x + 2, y, 2, 2, c); Ellipse(x, y + 2, 2, 2, c); Rect(x, y, 2, 2, 0xf5c45e); }
    }
    static void CharacterBase(Canvas p, int who, int face, int frame)
    {
        uint skin = Skins[who], hair = Hairs[who], coat = Coats[who], dark = Shade(coat, -27), light = Shade(coat, 25);
        bool woman = who is 1 or 3 or 5 or 7, back = face == 3, side = face is 1 or 2;
        int look = face == 1 ? -1 : face == 2 ? 1 : 0, step = frame switch { 1 => 3, 3 => -3, _ => 0 }, bob = frame is 1 or 3 ? -1 : 0;
        if (woman) { p.Shape(64, 72 + bob, 21, 29, Shade(hair, -14)); p.Ellipse(47, 70 + bob, 8, 20, hair); p.Ellipse(80, 72 + bob, 8, 21, hair); }
        p.Shape(57, 104 + step, 6, 9, dark); p.Shape(71, 104 - step, 6, 9, dark);
        p.Shape(55 + look, 110 + step, 8, 5, 0x69452b); p.Shape(72 + look, 110 - step, 8, 5, 0x69452b); p.Rect(50 + look, 107 + step, 7, 2, 0xaa7950); p.Rect(67 + look, 107 - step, 7, 2, 0xaa7950);
        p.Shape(64, 89 + bob, woman ? 15 : 14, 17, coat); p.Ellipse(61, 86 + bob, 10, 12, light); p.Rect(69, 83 + bob, 6, 17, dark);
        if (woman) { p.Shape(64, 100 + bob, 18, 6, coat); p.Line(51, 99 + bob, 76, 99 + bob, light, 2); }
        p.Shape(46, 87 + step + bob, 5, 10, 0xf0dec0); p.Shape(81, 87 - step + bob, 5, 10, 0xf0dec0);
        p.Shape(45, 96 + step + bob, 4, 6, skin); p.Shape(82, 96 - step + bob, 4, 6, skin);
        p.Rect(60, 75 + bob, 9, 7, Shade(skin, -19));
        p.Shape(64 + look * 2, 63 + bob, side ? 13 : 16, 18, skin); p.Ellipse(60 + look * 2, 58 + bob, 10, 10, Shade(skin, 13));
        p.Shape(47, 63 + bob, 3, 5, skin); p.Shape(81, 63 + bob, 3, 5, skin);
        p.Ellipse(64, 48 + bob, 17, 10, hair); p.Ellipse(49, 55 + bob, 6, 11, hair); p.Ellipse(78, 54 + bob, 6, 10, hair);
        for (int i = 0; i < 5; i++) p.Ellipse(51 + i * 6, 48 + (i % 2) * 3 + bob, 6, 6, i % 2 == 0 ? Shade(hair, 13) : hair);
        if (back) { p.Ellipse(64, 62 + bob, 16, 16, hair); p.Ellipse(59, 56 + bob, 9, 10, Shade(hair, 12)); }
        else
        {
            foreach (int eye in side ? new[] { 64 + look * 7 } : new[] { 57, 72 }) { p.Rect(eye - 2, 59 + bob, 6, 7, 0xfff0d3); p.Rect(eye + look, 60 + bob, 3, 6, who is 0 or 1 ? 0x32634au : 0x493d38u); p.Pixel(eye + look, 60 + bob, 0xffffff); p.Line(eye - 2, 57 + bob, eye + 3, 57 + bob, hair); }
            p.Line(64 + look * 5, 66 + bob, 65 + look * 5, 68 + bob, Shade(skin, -28)); p.Line(61 + look * 4, 73 + bob, 67 + look * 4, 73 + bob, 0x9c6248);
            if (woman) { p.Ellipse(53, 69 + bob, 3, 1, 0xdd937e); p.Ellipse(76, 69 + bob, 3, 1, 0xdd937e); }
            if (who is 2 or 8) { p.Ellipse(64, 73 + bob, 11, who == 2 ? 7 : 3, hair); p.Rect(60, 71 + bob, 8, 2, skin); }
            if (who is 4 or 8) { p.Rect(51, 59 + bob, 11, 9, 0x584c3d); p.Rect(52, 60 + bob, 9, 6, 0xb0c5af); p.Rect(68, 59 + bob, 11, 9, 0x584c3d); p.Rect(69, 60 + bob, 9, 6, 0xb0c5af); p.Line(62, 62 + bob, 68, 62 + bob, 0x584c3d); }
        }
        if (who == 0)
        {
            // Short-haired masculine farmer, rolled sleeves, green overalls.
            p.Shape(64, 47 + bob, 26, 6, 0xc68f36); p.Shape(64, 39 + bob, 18, 12, 0xe6b857); p.Ellipse(60, 35 + bob, 12, 6, 0xf5d27b); p.Rect(47, 43 + bob, 34, 4, 0x43734a); p.Line(43, 48 + bob, 84, 48 + bob, 0xf8d67b, 2);
            p.Rect(54, 82 + bob, 5, 17, dark); p.Rect(70, 82 + bob, 5, 17, dark); if (!back) { p.Rect(55, 87 + bob, 19, 13, coat); p.Rect(60, 90 + bob, 10, 7, light); p.Rect(55, 85 + bob, 3, 3, 0xe1bd56); p.Rect(71, 85 + bob, 3, 3, 0xe1bd56); }
        }
        if (who == 1) { p.Shape(78, 42 + bob, 9, 9, hair); p.Flower(80, 49 + bob, 0xffedc5); p.Rect(58, 83 + bob, 13, 12, 0xe4b963); }
        if (who == 2) { p.Rect(54, 84 + bob, 22, 20, 0x9a7148); p.Rect(59, 89 + bob, 11, 8, 0xc39860); }
        if (who == 3) { p.Shape(64, 43 + bob, 18, 6, 0x786e9a); p.Line(48, 46 + bob, 79, 46 + bob, 0xae9dbd, 2); }
        if (who == 4) { p.Line(55, 79 + bob, 63, 89 + bob, 0xe7d4a3, 3); p.Line(73, 79 + bob, 64, 89 + bob, 0xe7d4a3, 3); }
        if (who == 5) { p.Rect(54, 85 + bob, 21, 18, 0xf0dec0); p.Flower(76, 48 + bob, 0xdb9eaa); }
        if (who == 6) { p.Rect(62, 84 + bob, 4, 16, 0x737f72); p.Line(57, 79 + bob, 62, 90 + bob, 0xf7eed6, 2); p.Rect(72, 88 + bob, 5, 2, 0xaf5350); p.Rect(74, 86 + bob, 2, 6, 0xaf5350); }
        if (who == 7) { for (int i = 0; i < 5; i++) p.Shape(48 + i * 7, 45 + Math.Abs(2 - i) * 4 + bob, 6, 6, i % 2 == 0 ? hair : Shade(hair, 14)); p.Rect(58, 83 + bob, 13, 3, 0xd7a64e); }
        if (who == 8) { p.Rect(61, 81 + bob, 6, 16, 0xcba85e); p.Rect(64, 81 + bob, 2, 16, 0xe6ca8e); }
    }
    static void Character(Canvas p, int who, int face, int frame)
    {
        if (who == 0) { CharacterBase(p, who, face, frame); return; }
        uint skin = Skins[who], hair = Hairs[who], coat = Coats[who];
        bool back = face == 3, side = face is 1 or 2;
        int look = face == 1 ? -1 : face == 2 ? 1 : 0;
        int step = frame switch { 1 => 3, 3 => -3, _ => 0 }, bob = frame is 1 or 3 ? -1 : 0;
        int shoulders = who == 2 ? 19 : who == 4 ? 13 : who == 8 ? 14 : 16;
        uint trouser = who is 1 or 5 ? Shade(coat, -28) : who == 3 ? 0x5d6842u : 0x514a3bu;
        uint sleeve = who is 3 or 4 or 8 ? 0xf0dfbd : who == 1 ? 0x315f3d : coat;
        int hy = 59 + bob;

        // Different rear silhouettes remain recognizable from every direction.
        if (who == 1) { p.Shape(63, 69 + bob, 22, 27, Shade(hair, -18)); p.Shape(78, 42 + bob, 11, 11, hair); }
        if (who == 3) p.Shape(64, 61 + bob, 22, 21, Shade(hair, -14));
        if (who == 5) { p.Shape(65, 67 + bob, 20, 23, Shade(hair, -18)); p.Shape(76, 39 + bob, 12, 11, hair); }
        if (who == 7)
        {
            p.Shape(64, 61 + bob, 28, 29, Shade(hair, -17));
            for (int i = 0; i < 9; i++) p.Shape(43 + i % 2 * 42, 46 + i * 5 + bob, 7, 7, i % 2 == 0 ? hair : Shade(hair, 9));
        }
        p.Shape(57, 103 + step, 6, 10, trouser); p.Shape(71, 103 - step, 6, 10, trouser);
        p.Line(55, 99 + step, 55, 108 + step, Shade(trouser, 15), 2);
        p.Line(69, 99 - step, 69, 108 - step, Shade(trouser, 15), 2);
        p.Shape(55 + look, 111 + step, who == 2 ? 9 : 8, 5, 0x563c2e);
        p.Shape(72 + look, 111 - step, who == 2 ? 9 : 8, 5, 0x563c2e);
        p.Line(49 + look, 110 + step, 57 + look, 109 + step, 0xa37b50, 2);
        p.Line(66 + look, 110 - step, 74 + look, 109 - step, 0xa37b50, 2);
        p.Line(48 + look, 115 + step, 61 + look, 115 + step, 0x332d26);
        p.Line(65 + look, 115 - step, 78 + look, 115 - step, 0x332d26);
        p.Shape(64, 89 + bob, shoulders, 17, coat);
        p.Ellipse(58, 85 + bob, shoulders - 6, 10, Shade(coat, 17));
        p.Line(78, 87 + bob, 76, 100 + bob, Shade(coat, -24), 2);
        p.Line(51, 101 + bob, 76, 101 + bob, Shade(coat, -29), 2);
        p.Shape(45, 86 + step + bob, 6, 10, sleeve);
        p.Shape(82, 86 - step + bob, 6, 10, sleeve);
        p.Line(42, 87 + step + bob, 49, 87 + step + bob, Shade(sleeve, 23), 2);
        p.Line(79, 87 - step + bob, 86, 87 - step + bob, Shade(sleeve, 23), 2);
        p.Shape(45, 97 + step + bob, 4, 5, skin);
        p.Shape(82, 97 - step + bob, 4, 5, skin);
        p.Pixel(43, 95 + step + bob, Shade(skin, 17));
        p.Pixel(80, 95 - step + bob, Shade(skin, 17));
        p.Rect(59, 73 + bob, 11, 8, Shade(skin, -17));

        // Collars, seams, aprons and accessories match the illustrated portraits.
        if (who == 1)
        {
            p.Shape(64, 99 + bob, 17, 7, 0x315f3d);
            if (!back) { p.Rect(54, 84 + bob, 21, 18, 0xc79c3e); p.Rect(54, 80 + bob, 3, 12, 0xe4b65c); p.Rect(72, 80 + bob, 3, 12, 0xe4b65c); p.Rect(59, 90 + bob, 11, 7, 0xe1b55b); p.Line(54, 100 + bob, 75, 100 + bob, 0xa67b36); }
        }
        if (who == 2)
        {
            p.Line(52, 81 + bob, 76, 81 + bob, 0x83a5ba, 2);
            p.Rect(52, 85 + bob, 25, 19, 0x996b3d);
            p.Line(55, 80 + bob, 57, 90 + bob, 0x67472e, 3); p.Line(72, 80 + bob, 72, 90 + bob, 0x67472e, 3);
            p.Rect(58, 92 + bob, 12, 8, 0xb5834b); p.Line(59, 93 + bob, 69, 93 + bob, 0xd1a260);
            if (!back) { p.Rect(55, 85 + bob, 3, 3, 0xe0b34f); p.Rect(72, 85 + bob, 3, 3, 0xe0b34f); }
        }
        if (who is 3 or 4 or 6 or 8)
        {
            if (!back)
            {
                p.Line(56, 78 + bob, 62, 85 + bob, 0xf5e8cf, 3); p.Line(72, 78 + bob, 66, 85 + bob, 0xf5e8cf, 3);
                p.Rect(63, 85 + bob, 3, 16, 0xe2d4b5);
                for (int yy = 88; yy < 102; yy += 5) p.Rect(64, yy + bob, 1, 1, 0x8b704e);
                p.Line(53, 87 + bob, 59, 87 + bob, Shade(coat, -26)); p.Line(69, 87 + bob, 75, 87 + bob, Shade(coat, -26));
                if (who == 6) { p.Line(57, 83 + bob, 59, 80 + bob, 0x355932); p.Pixel(55, 82 + bob, 0x608343); p.Pixel(59, 81 + bob, 0x608343); }
                if (who == 8) { p.Rect(63, 83 + bob, 3, 13, 0xcaa452); p.Rect(64, 84 + bob, 1, 11, 0xf0cb79); }
            }
            p.Line(51, 99 + bob, 59, 99 + bob, Shade(coat, 16)); p.Line(69, 99 + bob, 76, 99 + bob, Shade(coat, 16));
        }
        if (who == 5)
        {
            p.Shape(64, 100 + bob, 18, 6, coat);
            p.Rect(54, 84 + bob, 22, 19, 0xefdec0); p.Line(54, 82 + bob, 56, 91 + bob, 0xffebcc, 3); p.Line(74, 82 + bob, 73, 91 + bob, 0xffebcc, 3);
            p.Line(58, 90 + bob, 70, 90 + bob, 0xc79557); p.Pixel(59, 88 + bob, 0x78904b); p.Pixel(65, 89 + bob, 0x78904b);
            p.Line(55, 101 + bob, 75, 101 + bob, 0xc6b698);
        }
        if (who == 7)
        {
            p.Shape(64, 99 + bob, 18, 6, coat);
            if (!back) { p.Line(54, 81 + bob, 62, 88 + bob, 0xe9c379, 2); p.Line(74, 81 + bob, 66, 88 + bob, 0xe9c379, 2); p.Line(64, 87 + bob, 64, 99 + bob, Shade(coat, -32)); for (int yy = 89; yy < 99; yy += 4) { p.Line(61, yy + bob, 66, yy + 3 + bob, 0xd9b065); p.Line(66, yy + bob, 61, yy + 3 + bob, 0xd9b065); } }
        }

        p.Shape(64 + look * 2, hy, side ? 14 : 17, who == 2 ? 21 : 20, skin);
        p.Ellipse(58 + look * 2, hy - 5, side ? 10 : 12, 12, Shade(skin, 15));
        p.Shape(46 + look, hy + 2, 3, 5, skin); p.Shape(82 + look, hy + 2, 3, 5, skin);
        p.Line(48 + look, hy + 1, 48 + look, hy + 4, Shade(skin, -21));
        p.Line(80 + look, hy + 1, 80 + look, hy + 4, Shade(skin, -21));
        if (back) p.Shape(64, hy - 2, 18, 20, hair);
        else
        {
            uint iris = who is 1 or 4 ? 0x386a41u : who == 3 ? 0x704a72u : 0x674d36u;
            foreach (int ex in side ? new[] { 64 + look * 8 } : new[] { 56, 72 })
            {
                p.Line(ex - 3, hy - 7, ex + 3, hy - 8, Shade(hair, -12), 2);
                p.Shape(ex, hy, 4, 5, 0xfff0d0);
                p.Rect(ex + look - 1, hy - 3, 4, 8, iris); p.Rect(ex + look, hy - 2, 2, 6, 0x252d28);
                p.Rect(ex + look - 1, hy - 3, 2, 2, 0xffffff); p.Pixel(ex + look + 2, hy + 3, Shade(iris, 31));
                p.Line(ex - 3, hy - 4, ex + 3, hy - 4, 0x49362c);
            }
            p.Line(64 + look * 5, hy + 3, 65 + look * 5, hy + 7, Shade(skin, -25));
            p.Line(61 + look * 4, hy + 12, 67 + look * 4, hy + 11, 0x975d45);
            p.Line(63 + look * 4, hy + 13, 66 + look * 4, hy + 13, Shade(skin, 19));
            if (who is 1 or 3 or 5 or 7) { p.Line(51, hy + 7, 55, hy + 7, Shade(skin, -7)); p.Line(74, hy + 7, 78, hy + 7, Shade(skin, -7)); }
            if (who == 2)
            {
                p.Shape(65 + look, hy + 14, side ? 12 : 15, 10, hair);
                p.Ellipse(61 + look, hy + 18, 8, 5, Shade(hair, 15)); p.Line(58 + look, hy + 10, 69 + look, hy + 10, skin, 2);
                p.Line(62 + look, hy + 11, 67 + look, hy + 11, 0x633d2b);
            }
            if (who == 6) { p.Line(55 + look, hy + 13, 60 + look, hy + 18, hair); p.Line(61 + look, hy + 18, 69 + look, hy + 16, hair); p.Line(61 + look, hy + 9, 69 + look, hy + 9, hair); }
            if (who == 8) { p.Shape(64 + look, hy + 10, 10, 3, hair); p.Line(63 + look, hy + 15, 68 + look, hy + 14, Shade(hair, 14), 2); }
            if (who is 4 or 8)
            {
                foreach (int ex in side ? new[] { 64 + look * 8 } : new[] { 56, 72 })
                { p.Line(ex - 5, hy - 4, ex + 5, hy - 4, 0x615139); p.Line(ex - 5, hy - 4, ex - 5, hy + 4, 0x615139); p.Line(ex + 5, hy - 4, ex + 5, hy + 4, 0x615139); p.Line(ex - 4, hy + 5, ex + 4, hy + 5, 0x615139); }
                p.Line(61, hy - 1, 67, hy - 1, 0x615139);
            }
        }
        p.Shape(64, hy - 17, side ? 17 : 19, 11, hair);
        for (int i = 0; i < 6; i++)
        {
            int hx = 49 + i * 6, yy = hy - 15 + i % 2 * 3;
            p.Shape(hx, yy, 6, 6, i % 2 == 0 ? Shade(hair, 13) : hair);
            p.Line(hx - 2, yy - 3, hx + 1, yy - 4, Shade(hair, 28));
        }
        p.Ellipse(47, hy - 6, 5, 10, hair); p.Ellipse(81, hy - 7, 5, 10, hair);
        if (back) p.Ellipse(62, hy - 2, 16, 14, hair);
        if (who == 1) { p.Shape(79, hy - 23, 10, 10, hair); p.Line(74, hy - 27, 80, hy - 29, Shade(hair, 30), 2); p.Flower(83, hy - 17, 0xffedca); p.Ellipse(48, hy + 7, 5, 13, hair); p.Line(46, hy + 2, 47, hy + 15, Shade(hair, 20), 2); }
        if (who == 3) { p.Shape(64, hy - 24, 22, 9, 0x9380b2); p.Shape(68 + look * 4, hy - 17, 22, 4, 0x70628c); p.Line(49, hy - 28, 73, hy - 30, 0xb6a1c8, 2); p.Rect(80, hy - 21, 3, 3, 0xd8b56b); }
        if (who == 5) { p.Shape(76, hy - 24, 12, 10, hair); p.Line(69, hy - 29, 78, hy - 31, Shade(hair, 26), 2); p.Line(76, hy - 23, 84, hy - 21, Shade(hair, -24)); }
        if (who == 7)
        {
            for (int i = 0; i < 7; i++) { int hx = 43 + i * 7, yy = hy - 19 + Math.Abs(3 - i) * 3; p.Shape(hx, yy, 7, 7, i % 2 == 0 ? hair : Shade(hair, 11)); p.Line(hx - 2, yy - 3, hx + 1, yy - 4, Shade(hair, 26)); }
            p.Rect(43, hy + 10, 2, 5, 0xe8bd5b); p.Rect(84, hy + 10, 2, 5, 0xe8bd5b);
        }
    }
    static void PortraitArt(Canvas p, int who)
    {
        uint skin = Skins[who], hair = Hairs[who], coat = Coats[who]; bool woman = who is 1 or 3 or 5 or 7;
        if (woman) p.Shape(64, 60, 42, 49, Shade(hair, -14));
        p.Shape(64, 105, 45, 18, coat); p.Ellipse(54, 102, 28, 14, Shade(coat, 22)); p.Rect(56, 84, 18, 22, Shade(skin, -14));
        p.Shape(64, 60, 30, 34, skin); p.Ellipse(56, 49, 19, 21, Shade(skin, 15)); p.Shape(32, 61, 5, 8, skin); p.Shape(96, 61, 5, 8, skin);
        p.Ellipse(64, 32, 32, 19, hair); p.Ellipse(36, 48, 9, 22, hair); p.Ellipse(92, 46, 9, 20, hair);
        for (int i = 0; i < 7; i++) p.Ellipse(40 + i * 8, 31 + i % 2 * 6, 10, 11, i % 2 == 0 ? Shade(hair, 15) : hair);
        foreach (int x in new[] { 49, 79 }) { p.Line(x - 6, 48, x + 5, 47, hair, 2); p.Shape(x, 58, 7, 9, 0xfff3d9); p.Ellipse(x + 1, 59, 4, 8, who is 0 or 1 ? 0x397452u : 0x604b35u); p.Rect(x, 56, 4, 8, 0x303a30); p.Rect(x - 1, 53, 3, 3, 0xffffff); }
        p.Line(64, 63, 62, 72, Shade(skin, -32), 2); p.Line(60, 73, 66, 73, Shade(skin, -24)); p.Line(56, 81, 71, 81, 0xa65e4a, 2); p.Line(59, 84, 68, 84, Shade(skin, 19));
        if (woman) { p.Ellipse(43, 74, 6, 2, 0xdb947d); p.Ellipse(85, 74, 6, 2, 0xdb947d); }
        if (who is 2 or 8) { p.Ellipse(64, 85, 23, who == 2 ? 12 : 5, hair); p.Rect(56, 80, 15, 3, skin); }
        if (who is 4 or 8) { p.Rect(39, 49, 22, 18, 0x675641); p.Rect(41, 51, 18, 13, 0xabc2a8); p.Rect(69, 49, 22, 18, 0x675641); p.Rect(71, 51, 18, 13, 0xabc2a8); p.Line(61, 55, 69, 55, 0x675641, 2); }
        if (who == 0) { p.Shape(64, 32, 49, 9, 0xb88537); p.Shape(64, 20, 34, 16, 0xe9bf64); p.Ellipse(56, 14, 24, 8, 0xf6d583); p.Rect(32, 27, 64, 6, 0x3b7146); p.Line(21, 34, 107, 34, 0xf7d78e, 2); p.Rect(39, 102, 9, 19, 0x2d5b3e); p.Rect(81, 102, 9, 19, 0x2d5b3e); }
        if (who == 1) { p.Shape(89, 19, 17, 17, hair); p.Flower(97, 36, 0xffefd4); p.Flower(100, 30, 0xffefd4); p.Rect(49, 102, 30, 18, 0xe6bd69); }
        if (who == 3) { p.Shape(64, 18, 35, 9, 0x8a7eaa); p.Line(34, 23, 95, 23, 0xb6a2ca, 3); }
        if (who == 5) { p.Flower(94, 37, 0xe3a0ac); p.Rect(43, 105, 43, 15, 0xefdfc4); }
        if (who == 7) for (int i = 0; i < 7; i++) p.Shape(37 + i * 9, 23 + Math.Abs(3 - i) * 5, 10, 10, i % 2 == 0 ? hair : Shade(hair, 16));
    }
    public static Canvas CreateAtlas()
    {
        Canvas p = new(Columns * Cell, Rows * Cell);
        void At(int cell) { p.X = cell % Columns * Cell; p.Y = cell / Columns * Cell; }
        for (int who = 0; who < 9; who++) for (int facing = 0; facing < 4; facing++) for (int frame = 0; frame < 4; frame++) { At(who * 16 + facing * 4 + frame); Character(p, who, facing, frame); }
        At(Tree); p.Shape(65, 115, 12, 10, 0x674733); p.Rect(59, 63, 13, 54, 0x674733); p.Rect(61, 68, 5, 45, 0xa17541); p.Line(63, 102, 46, 85, 0x714e30, 6); p.Line(67, 91, 83, 72, 0x714e30, 5);
        foreach (var (x, y, rx, ry, c) in new (int, int, int, int, uint)[] { (39, 66, 25, 23, 0x315f36), (85, 64, 27, 25, 0x315f36), (61, 47, 39, 34, 0x497b39), (35, 44, 20, 23, 0x5b8e3f), (87, 41, 23, 24, 0x5d903e), (63, 25, 28, 23, 0x78a349), (47, 24, 16, 14, 0x94b654) }) p.Shape(x, y, rx, ry, c, 0x344c2d);
        for (int i = 0; i < 125; i++) { int x = 22 + i * 23 % 83, y = 14 + i * 31 % 64; if ((x - 64) * (x - 64) + (y - 45) * (y - 45) < 1390) { for (int dx = 0; dx < 2 + i % 3; dx++) p.Glint(x + dx, y, i % 3 == 0 ? 0x98b254u : i % 3 == 1 ? 0x6f9943u : 0x3f6b36u); } }
        At(Bush); p.Leaf(43, 100, 19, 19, 0x537f3a); p.Leaf(67, 91, 25, 24, 0x648f3c); p.Leaf(86, 103, 17, 17, 0x4e7937); p.Flower(53, 89, 0xf0c1b0); p.Flower(83, 103, 0xe3a5ab); p.Flower(67, 107, 0xffe6b4);
        At(Flowers); for (int i = 0; i < 8; i++) { int x = 29 + i * 17 % 68, y = 92 + i * 7 % 27; p.Line(x, y, x + 3, y - 15, 0x447036, 2); p.Flower(x + 3, y - 16, i % 2 == 0 ? 0xffecc0u : 0xe6a2a9u); }
        At(Rock); p.Shape(65, 106, 27, 16, 0x686b64); p.Shape(58, 97, 22, 20, 0x929384); p.Ellipse(53, 90, 13, 8, 0xbabea1); p.Line(75, 87, 70, 104, 0x60675c, 2); p.Rect(48, 108, 7, 3, 0x5b7743);
        At(Log); p.Shape(65, 105, 31, 12, 0x6c432b); p.Line(44, 98, 85, 99, 0xa87842, 3); p.Line(50, 109, 88, 111, 0x8e5b32, 2); p.Shape(38, 104, 10, 13, 0xc49a59); p.Shape(38, 104, 7, 9, 0x815a38); p.Ellipse(38, 104, 4, 6, 0xc9a46c);
        At(Fence); p.Rect(19, 87, 92, 7, 0x60442c); p.Rect(19, 103, 92, 6, 0x60442c); p.Rect(19, 87, 92, 4, 0xc39755); p.Rect(19, 103, 92, 3, 0xab7b43); foreach (int x in new[] { 27, 95 }) { p.Rect(x, 76, 8, 45, 0x67472f); p.Rect(x + 1, 76, 5, 42, 0xc9a062); p.Shape(x + 4, 75, 5, 3, 0xe3bc79); }
        for (int crop = 0; crop < 3; crop++) for (int stage = 0; stage < 3; stage++)
        {
            At(Crop + crop * 3 + stage); int h = stage == 0 ? 10 : stage == 1 ? 21 : 29; p.Line(64, 116, 64, 116 - h, 0x38652f, 3); p.Leaf(56, 112 - h / 2, 9 + stage * 2, 4 + stage, 0x5f973b); p.Leaf(73, 107 - h / 2, 9 + stage * 2, 5 + stage, 0x71a342); if (stage > 0) { p.Leaf(62, 111 - h, 5, 10 + stage, 0x79a749); p.Line(62, 101 - h, 63, 109 - h, 0xb1c86b); }
            if (stage == 2) { if (crop == 0) { p.Shape(64, 114, 10, 11, 0xdf802f); p.Line(60, 109, 67, 109, 0xf4b14e, 2); p.Line(66, 115, 72, 115, 0xa95529, 2); } else if (crop == 1) { p.Shape(64, 113, 13, 10, 0xe9d4ad); p.Ellipse(64, 108, 11, 5, 0xa77f9d); p.Rect(63, 121, 3, 3, 0xf0ddba); } else { foreach (int x in new[] { 54, 68, 79 }) { p.Shape(x, 113 - x % 3 * 4, 6, 7, 0xc9504e); p.Rect(x - 2, 109 - x % 3 * 4, 2, 2, 0xffd38a); } } }
        }
        for (int who = 0; who < 9; who++) { At(Portrait + who); PortraitArt(p, who); }
        return p;
    }
    public static void Init()
    {
        Canvas canvas = CreateAtlas(); GCHandle pin = GCHandle.Alloc(canvas.Pixels, GCHandleType.Pinned);
        try { Image image = new() { data = pin.AddrOfPinnedObject(), width = canvas.Width, height = canvas.Height, mipmaps = 1, format = 7 }; atlas = LoadTextureFromImage(image); SetTextureFilter(atlas, TEXTURE_FILTER_POINT); }
        finally { pin.Free(); }
        if (!Ready) throw new InvalidOperationException("Não foi possível criar a folha de sprites.");
        Portraits.Init();
    }
    public static void Unload() { Portraits.Unload(); if (Ready) { UnloadTexture(atlas); atlas = default; } }
    public static void Draw(int id, double x, double y, double scale = 1, Color? tint = null)
    {
        DrawTexturePro(atlas, new(id % Columns * Cell, id / Columns * Cell, Cell, Cell), new(x - 64 * scale, y - 116 * scale, Cell * scale, Cell * scale), new(), 0, tint ?? WHITE);
    }
    public static void Person(int npc, double x, double y, int face, double time = 0) { int frame = time <= 0 ? 0 : (int)(time * 8) % 4; Draw((npc + 1) * 16 + face * 4 + frame, Math.Round(x * 2) / 2, Math.Round(y * 2) / 2, .5); }
    public static void PortraitDraw(int npc, double x, double y, double size)
    {
        if (npc >= 0) { Portraits.Draw(npc, new(x, y, size, size)); return; }
        DrawTexturePro(atlas, new(Portrait % Columns * Cell, Portrait / Columns * Cell, Cell, Cell), new(x, y, size, size), new(), 0, WHITE);
    }
    public static bool Export(string path) { Image image = LoadImageFromTexture(atlas); bool ok = ExportImage(image, path); UnloadImage(image); return ok; }
}

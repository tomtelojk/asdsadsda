using static ValeDoSol.Ray;
using static ValeDoSol.World;
namespace ValeDoSol;

// Original LPC PNGs remain unmodified. Source rectangles describe tiles explicitly;
// furniture uses the same destination bounds as the physics blueprint in Map.cs.
public static class Scenery
{
    static readonly Dictionary<string, Texture> textures = [];
    public static void Init()
    {
        foreach (string name in new[] { "house", "inside", "country", "kitchen", "cabinets", "grass", "dirt", "tree", "trunk", "water", "rock", "chests", "barrel" })
        {
            using var stream = typeof(Scenery).Assembly.GetManifestResourceStream("ValeDoSol.Lpc." + name + ".png") ?? throw new InvalidOperationException("Textura ausente: " + name);
            using MemoryStream memory = new(); stream.CopyTo(memory); byte[] bytes = memory.ToArray(); Image image = LoadImageFromMemory(".png", bytes, bytes.Length);
            Texture texture = LoadTextureFromImage(image); UnloadImage(image); if (texture.id == 0) throw new InvalidOperationException("Textura inválida: " + name); SetTextureFilter(texture, TEXTURE_FILTER_POINT); textures[name] = texture;
        }
    }
    public static void Unload() { foreach (Texture texture in textures.Values) UnloadTexture(texture); textures.Clear(); }
    public static void Draw(string name, Rectangle source, Rectangle destination, Color? tint = null) => DrawTexturePro(textures[name], source, destination, new(), 0, tint ?? WHITE);
    public static void Tile(string name, Rectangle source, Rectangle destination, Color? tint = null)
    {
        for (float y = destination.y; y < destination.y + destination.height; y += source.height) for (float x = destination.x; x < destination.x + destination.width; x += source.width) { float w = Math.Min(source.width, destination.x + destination.width - x), h = Math.Min(source.height, destination.y + destination.height - y); Draw(name, new(source.x, source.y, w, h), new(x, y, w, h), tint); }
    }
    public static void House(Building b, bool night)
    {
        int x = b.x * 32, y = b.y * 32, w = b.w * 32, h = b.h * 32;
        DrawRectangle(x + 8, y + h - 5, w, 17, new Color(44, 38, 28, 50));
        Tile("house", new(129, 16, 30, 16), new(x + 8, y + 70, w - 16, h - 70));
        DrawRectangle(x + 8, y + h - 11, w - 16, 11, color_hex(0x8b8268)); Tile("house", new(1, 1, 14, 14), new(x + 8, y + h - 10, w - 16, 10));
        foreach (int beam in new[] { x + 6, x + w - 14 }) { DrawRectangle(beam, y + 65, 8, h - 65, color_hex(0x57412d)); DrawRectangle(beam + 2, y + 65, 2, h - 69, color_hex(0xa27d4c)); }
        DrawRectangle(x - 9, y + 72, w + 18, 11, color_hex(0x4b3b2c));
        Color tint = b.type switch { 2 => new(185, 210, 238), 3 => new(223, 177, 210), 5 => new(171, 221, 183), _ => new(255, 184, 137) };
        // Equal roof dimensions for every building. The tile strips follow a
        // trapezoid rather than painting a rectangular roof through its borders.
        for (int row = 0; row < 80; row += 4)
        {
            int inset = (80 - row) / 2, start = x - 10 + inset, end = x + w + 10 - inset;
            for (int xx = start; xx < end; xx += 24) Draw("house", new(100, 100 + row % 24, Math.Min(24, end - xx), 4), new(xx, y - 4 + row, Math.Min(24, end - xx), 4), tint);
        }
        DrawLineEx(new(x + 30, y - 6), new(x - 11, y + 77), 3, color_hex(0x45382d)); DrawLineEx(new(x + w - 30, y - 6), new(x + w + 11, y + 77), 3, color_hex(0x45382d)); DrawRectangle(x + 29, y - 8, w - 58, 4, color_hex(0xc59a70));
        Draw("house", new(200, 0, 24, 56), new(x + w - 58, y - 20, 24, 56));
        for (int i = 0; i < 2; i++) { int wx = i == 0 ? x + 24 : x + w - 64; if (night) DrawRectangle(wx, y + 100, 40, 42, new Color(255, 204, 113, 175)); Draw("house", new(256, 16, 32, 32), new(wx, y + 96, 40, 42)); DrawRectangle(wx - 3, y + 138, 46, 7, color_hex(0x9b6b41)); for (int k = 0; k < 3; k++) { Draw("grass", new(0, 0, 32, 32), new(wx + k * 12 - 2, y + 123, 20, 18)); DrawCircle(wx + 8 + k * 12, y + 127, 2, color_hex(k % 2 == 0 ? 0xe2a0aa : 0xf3d68b)); } }
        int dx = x + w / 2 - 18; Draw("house", new(97, 0, 30, 48), new(dx, y + h - 59, 36, 59)); DrawCircle(dx + 27, y + h - 27, 2, color_hex(0xf3d389));
        DrawRectangle(dx - 12, y + h, 60, 8, color_hex(0x83735a)); DrawRectangle(dx - 18, y + h + 8, 72, 6, color_hex(0xbda27a));
        if (b.type is 1 or 4) { for (int i = 0; i < 10; i++) { DrawRectangle(x + 11 + i * 20, y + 84, 20, 15, color_hex(i % 2 == 0 ? (b.type == 4 ? 0xc9934b : 0x5a8c61) : 0xf0d7a2)); DrawCircle(x + 21 + i * 20, y + 99, 10, color_hex(i % 2 == 0 ? (b.type == 4 ? 0xc9934b : 0x5a8c61) : 0xf0d7a2)); } }
        DrawRectangle(x + w / 2 - 69, y + 57, 138, 20, color_hex(0x493b2a)); DrawRectangle(x + w / 2 - 66, y + 59, 132, 16, color_hex(0x80794e));
        DrawText(b.name, x + w / 2 - MeasureText(b.name, 10) / 2, y + 63, 10, color_hex(0xffecc5));
        if (b.type == 2) { Draw("barrel", new(0, 0, 32, 48), new(x + w - 42, y + h - 22, 28, 42)); Art.Draw(Art.Log, x + 32, y + h + 9, .42); }
    }
    public static void Tree(double x, double y, int variant, Color tint)
    {
        if (variant % 4 == 0) Draw("tree", new(0, 104, 96, 120), new(x - 44, y - 108, 88, 110), tint);
        else { Draw("trunk", new(32, 0, 32, 80), new(x - 16, y - 72, 32, 80), tint); Draw("tree", new(variant % 2 * 96, 0, 96, 96), new(x - 48, y - 112, 96, 96), tint); }
    }
    public static void Furniture(Prop prop, double time)
    {
        Rectangle b = prop.bounds; double x = b.x, y = b.y, w = b.width, h = b.height;
        if (prop.kind == Furnishing.Rug) { DrawRectangle(x, y, w, h, color_hex(0x8e5659)); DrawRectangleLinesEx(new(x + 5, y + 5, w - 10, h - 10), 2, color_hex(0xc6a075)); for (int i = 10; i < w - 10; i += 16) DrawRectangle(x + i, y + h / 2, 5, 3, color_hex(0xd2ad7b)); return; }
        DrawRectangle(x + 3, y + h - 4, w, 10, new Color(29, 24, 22, 45));
        switch (prop.kind)
        {
            case Furnishing.Bed: Draw("country", new(0, 0, 64, 128), b); break;
            case Furnishing.Table: Draw("cabinets", new(0, 272, 96, 144), b); Draw("kitchen", new(32, 32, 32, 32), new(x + w / 2 - 12, y + 5, 24, 24)); break;
            case Furnishing.Books: for (int i = 0; i < (int)(w / 28); i++) Draw("cabinets", new(32, 0, 32, 80), new(x + i * w / (int)(w / 28), y, w / (int)(w / 28), h)); break;
            case Furnishing.Shelf: Draw("cabinets", new(64, 0, 32, 80), b); break;
            case Furnishing.Chest: Draw("chests", new(0, 0, 32, 32), b); break;
            case Furnishing.Kitchen: Draw("kitchen", new(0, 96, 64, 32), b); break;
            case Furnishing.Fireplace: Draw("kitchen", new(32, 96, 32, 32), b); Draw("kitchen", new(8, 0, 16, 64), new(x + w * .42, y - 38, w * .2, 40)); DrawEllipse(x + w * .5, y + h * .79, 9 + Math.Sin(time * 7) * 2, 5, color_hex(0xf3b756)); break;
            case Furnishing.Counter: Tile("cabinets", new(96, 102, 32, 64), b); DrawRectangle(x, y, w, 6, color_hex(0xcdb085)); break;
            case Furnishing.Bench: Tile("cabinets", new(128, 102, 32, 64), b); for (int i = 0; i < 3; i++) { DrawLine(x + 24 + i * 37, y + 4, x + 32 + i * 37, y + 18, color_hex(0x745238)); DrawRectangle(x + 27 + i * 37, y + 3, 14, 5, color_hex(0x9ca495)); } break;
            case Furnishing.Crates: for (int i = 0; i < w / 32; i++) Draw("barrel", new(0, 0, 32, 48), new(x + i * 32, y + h - 48, 32, 48)); break;
            case Furnishing.Plant: Draw("grass", new(0, 0, 32, 32), b); break;
        }
    }
}

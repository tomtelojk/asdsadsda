using static ValeDoSol.Ray;
using static ValeDoSol.World;
namespace ValeDoSol;

// Original LPC PNGs remain unmodified. Complete objects use named, audited
// source rectangles and one uniform scale; only repeatable material tiles crop.
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
    public static void Draw(string name, Rectangle source, Rectangle destination, Color? tint = null)
    {
        Texture texture = textures[name];
        if (source.x < 0 || source.y < 0 || source.width <= 0 || source.height <= 0 || source.x + source.width > texture.width || source.y + source.height > texture.height)
            throw new ArgumentOutOfRangeException(nameof(source), "Recorte fora da textura: " + name);
        DrawTexturePro(texture, source, destination, new(), 0, tint ?? WHITE);
    }
    public static void Sprite(string name, Rectangle area, Color? tint = null)
    {
        SpriteDefinition sprite = SpriteCatalog.Get(name);
        Draw(sprite.Texture, sprite.Source, SpriteCatalog.PreserveAspect(area, sprite), tint);
    }
    public static void AtFoot(string name, double x, double y, float scale = 1, Color? tint = null)
    {
        SpriteDefinition sprite = SpriteCatalog.Get(name);
        Draw(sprite.Texture, sprite.Source, SpriteCatalog.AtFoot(new(Math.Round(x), Math.Round(y)), sprite, scale), tint);
    }
    public static void Tile(string name, Rectangle source, Rectangle destination, Color? tint = null)
    {
        for (float y = destination.y; y < destination.y + destination.height; y += source.height)
            for (float x = destination.x; x < destination.x + destination.width; x += source.width)
            {
                float w = Math.Min(source.width, destination.x + destination.width - x), h = Math.Min(source.height, destination.y + destination.height - y);
                Draw(name, new(source.x, source.y, w, h), new(x, y, w, h), tint);
            }
    }
    static void FlowerBox(int x, int y, int variant)
    {
        DrawRectangle(x - 2, y + 12, 36, 9, color_hex(0x70492e)); DrawRectangle(x, y + 13, 32, 6, color_hex(0xb47b47));
        DrawRectangle(x + 2, y + 14, 28, 1, color_hex(0xd1a16b));
        for (int k = 0; k < 3; k++)
        {
            int px = x + 4 + k * 11, py = y + k % 2 * 3;
            DrawRectangle(px + 2, py + 3, 2, 10, color_hex(0x456d36));
            DrawRectangle(px - 1, py + 7, 4, 2, color_hex(0x7c9d47)); DrawRectangle(px + 4, py + 5, 4, 2, color_hex(0x63883d));
            Color petal = color_hex((k + variant) % 2 == 0 ? 0xe8adbd : 0xf1d789);
            DrawRectangle(px, py, 6, 4, petal); DrawRectangle(px + 1, py - 1, 4, 6, petal); DrawRectangle(px + 2, py + 1, 2, 2, color_hex(0xc48b40));
        }
    }
    public static void House(Building b, bool night)
    {
        if (b.type == 7) { TownArt.Hall(b, night); return; }
        int x = b.x * 32, y = b.y * 32, w = b.w * 32, h = b.h * 32;
        DrawRectangle(x + 8, y + h - 5, w, 17, new Color(44, 38, 28, 50));
        Tile("house", new(129, 16, 30, 16), new(x + 8, y + 70, w - 16, h - 70));
        DrawRectangle(x + 8, y + h - 11, w - 16, 11, color_hex(0x6b6757));
        for (int fx = x + 10; fx < x + w - 10; fx += 18)
        {
            int fw = Math.Min(16, x + w - 10 - fx);
            DrawRectangle(fx, y + h - 9, fw, 7, color_hex(0xa6a18a)); DrawRectangle(fx + 1, y + h - 9, Math.Max(1, fw - 2), 2, color_hex(0xc5bda1));
        }
        foreach (int beam in new[] { x + 6, x + w - 14 })
        {
            DrawRectangle(beam, y + 65, 8, h - 65, color_hex(0x57412d)); DrawRectangle(beam + 2, y + 65, 2, h - 69, color_hex(0xa27d4c));
        }
        DrawRectangle(x - 9, y + 72, w + 18, 11, color_hex(0x4b3b2c));
        Color tint = b.type switch { 2 => new(185, 210, 238), 3 => new(223, 177, 210), 5 => new(171, 221, 183), _ => new(255, 184, 137) };
        // Roof pieces are tiled at native size. The trapezoid follows the same
        // footprint in every house; no complete object is squeezed into it.
        for (int row = 0; row < 80; row += 4)
        {
            int inset = (80 - row) / 2, start = x - 10 + inset, end = x + w + 10 - inset;
            for (int xx = start; xx < end; xx += 24)
            {
                int width = Math.Min(24, end - xx);
                Draw("house", new(104, 104 + row % 24, width, 4), new(xx, y - 4 + row, width, 4), tint);
            }
        }
        DrawLineEx(new(x + 30, y - 6), new(x - 11, y + 77), 3, color_hex(0x45382d)); DrawLineEx(new(x + w - 30, y - 6), new(x + w + 11, y + 77), 3, color_hex(0x45382d)); DrawRectangle(x + 29, y - 8, w - 58, 4, color_hex(0xc59a70));
        AtFoot("chimney", x + w - 46, y + 25);
        for (int i = 0; i < 2; i++)
        {
            int wx = i == 0 ? x + 28 : x + w - 60, wy = y + 98;
            if (night) DrawRectangle(wx + 3, wy + 5, 26, 34, new Color(255, 204, 113, 175));
            AtFoot("window", wx + 16, wy + 44);
            FlowerBox(wx, wy + 45, b.type + i);
        }
        int dx = x + w / 2;
        AtFoot(b.type is 2 or 5 ? "door_alt" : "door", dx, y + h);
        DrawRectangle(dx - 28, y + h, 56, 8, color_hex(0x83735a)); DrawRectangle(dx - 34, y + h + 8, 68, 6, color_hex(0xbda27a));
        if (b.type is 1 or 4)
        {
            long stripe = b.type == 4 ? 0xc9934b : 0x5a8c61;
            for (int i = 0; i < 10; i++)
            {
                DrawRectangle(x + 12 + i * 20, y + 84, 20, 12, color_hex(i % 2 == 0 ? stripe : 0xf0d7a2));
                DrawRectangle(x + 12 + i * 20, y + 96, 20, 4, color_hex(i % 2 == 0 ? 0x385e43 : 0xc9af7f));
            }
        }
        DrawRectangle(x + w / 2 - 69, y + 57, 138, 20, color_hex(0x493b2a)); DrawRectangle(x + w / 2 - 66, y + 59, 132, 16, color_hex(0x80794e));
        DrawText(b.name, x + w / 2 - MeasureText(b.name, 10) / 2, y + 63, 10, color_hex(0xffecc5));
        if (b.type == 2) AtFoot("barrel", x + w - 28, y + h + 5);
    }
    public static void Tree(double x, double y, int variant, Color tint)
    {
        DrawEllipse(x, y + 4, 25, 7, new Color(30, 49, 25, tint.a / 6));
        if (variant % 4 == 0) AtFoot(variant % 8 == 0 ? "pine_a" : "pine_b", x, y + 4, tint: tint);
        else
        {
            AtFoot(variant % 2 == 0 ? "trunk_a" : "trunk_b", x, y + 4, tint: tint);
            AtFoot(variant % 2 == 0 ? "tree_a" : "tree_b", x, y - 24, tint: tint);
        }
    }
    static void Columns(string spriteName, Rectangle bounds)
    {
        SpriteDefinition sprite = SpriteCatalog.Get(spriteName);
        for (float x = bounds.x; x + sprite.Source.width <= bounds.x + bounds.width; x += sprite.Source.width)
            AtFoot(spriteName, x + sprite.FootX, bounds.y + bounds.height);
    }
    public static void Furniture(Prop prop, double time)
    {
        Rectangle b = prop.bounds; double x = b.x, y = b.y, w = b.width, h = b.height;
        if (prop.kind == Furnishing.Rug)
        {
            DrawRectangle(x, y, w, h, color_hex(0x8e5659)); DrawRectangleLinesEx(new(x + 5, y + 5, w - 10, h - 10), 2, color_hex(0xc6a075));
            for (int i = 10; i < w - 10; i += 16) DrawRectangle(x + i, y + h / 2, 5, 3, color_hex(0xd2ad7b)); return;
        }
        DrawRectangle(x + 3, y + h - 3, w, 5, new Color(29, 24, 22, 45));
        switch (prop.kind)
        {
            case Furnishing.Bed: Sprite("bed", b); break;
            case Furnishing.Table:
                Sprite("table_horizontal", b);
                DrawRectangle(x + w / 2 - 7, y + h * .24, 12, 9, color_hex(0xeadbb8)); DrawRectangle(x + w / 2 - 5, y + h * .24 + 2, 8, 4, color_hex(0x8b6549));
                DrawRectangleLines(x + w / 2 + 5, y + h * .24 + 2, 5, 5, color_hex(0xd7c59c));
                break;
            case Furnishing.Books: Columns("bookshelf", b); break;
            case Furnishing.Shelf: Columns("shelf_jars", b); break;
            case Furnishing.Chest: Sprite("chest_closed", b); break;
            case Furnishing.Kitchen:
                int kitchenScale = w >= 128 ? 2 : 1;
                AtFoot("stove", x + 16 * kitchenScale, y + h, kitchenScale);
                AtFoot("stove_fire", x + 48 * kitchenScale, y + h, kitchenScale);
                AtFoot("cooking_utensils", x + w / 2, y - 5);
                break;
            case Furnishing.Fireplace:
                Sprite("stove_fire", b);
                // A native chimney on the rear wall, rather than a stretched
                // crop from a different kitchen appliance.
                AtFoot("chimney", x + w / 2, y + 3);
                DrawRectangle(x + w / 2 - 5, y + h - 13, 10, 3, color_hex(Math.Sin(time * 7) > 0 ? 0xf3b756 : 0xdc8b3e));
                break;
            case Furnishing.Counter:
                // The cabinet module is complete at y101..169. Repetition keeps
                // doors the same width and height as the original artwork.
                Columns("counter", b);
                DrawRectangle(x, y, w, 4, color_hex(0xcdb085));
                break;
            case Furnishing.Bench:
                Columns("table_horizontal", b);
                for (int i = 0; i < (int)(w / 64); i++) AtFoot("cooking_utensils", x + 32 + i * 64, y + 21);
                break;
            case Furnishing.Crates:
                for (float cx = b.x; cx + 28 <= b.x + b.width; cx += 32) AtFoot("barrel", cx + 14, y + h);
                break;
            case Furnishing.Plant: Columns("plant", b); break;
        }
    }
}

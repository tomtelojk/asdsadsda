namespace ValeDoSol;

/// <summary>A complete sprite rectangle and its base, in source-local pixels.</summary>
public sealed record SpriteDefinition
{
    public string Texture { get; }
    public Rectangle Source { get; }
    public int FootX { get; }
    public int FootY { get; }

    public SpriteDefinition(string texture, Rectangle source, int footX, int footY)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(texture);
        if (!float.IsFinite(source.x) || !float.IsFinite(source.y) || !float.IsFinite(source.width) || !float.IsFinite(source.height) || source.x < 0 || source.y < 0 || source.width <= 0 || source.height <= 0)
            throw new ArgumentOutOfRangeException(nameof(source), "O recorte precisa ter origem válida e dimensões positivas.");
        if (footX < 0 || footY < 0 || footX > source.width || footY > source.height)
            throw new ArgumentOutOfRangeException(nameof(footX), "A base deve ficar dentro do recorte.");
        Texture = texture;
        Source = source;
        FootX = footX;
        FootY = footY;
    }
}

/// <summary>Verified source bounds. No graphics context is needed to use or test this catalog.</summary>
public static class SpriteCatalog
{
    static SpriteDefinition S(string texture, int x, int y, int width, int height) => new(texture, new(x, y, width, height), width / 2, height);

    public static readonly IReadOnlyDictionary<string, SpriteDefinition> Sprites = new Dictionary<string, SpriteDefinition>(StringComparer.Ordinal)
    {
        ["bed"] = S("country", 8, 15, 48, 81),
        ["chair_front"] = S("country", 67, 0, 26, 32),
        ["chair_front_alt"] = S("country", 99, 16, 26, 32),
        ["stool"] = S("country", 67, 40, 26, 24),
        ["bench_stool"] = S("country", 131, 24, 26, 24),
        ["chair_left"] = S("country", 101, 80, 22, 32),
        ["chair_right"] = S("country", 133, 80, 22, 32),
        ["table_horizontal"] = S("cabinets", 96, 268, 64, 46),
        ["table_vertical"] = S("cabinets", 102, 320, 52, 90),
        ["table_large"] = S("cabinets", 0, 268, 96, 148),
        ["bookshelf"] = S("cabinets", 32, 5, 32, 69),
        ["bookshelf_alt"] = S("cabinets", 64, 5, 32, 69),
        ["shelf"] = S("cabinets", 0, 5, 32, 69),
        ["shelf_jars"] = S("cabinets", 96, 5, 32, 69),
        ["counter"] = S("cabinets", 96, 101, 32, 69),
        ["cabinet_tall"] = S("cabinets", 160, 224, 32, 64),
        ["cabinet"] = S("cabinets", 160, 300, 32, 46),
        ["barrel"] = S("barrel", 2, 10, 28, 38),
        ["barrel_alt"] = S("barrel", 34, 26, 28, 38),
        ["barrel_group"] = S("barrel", 64, 2, 48, 61),
        ["chest_closed"] = S("chests", 0, 72, 32, 24),
        ["chest_closed_alt"] = S("chests", 32, 72, 32, 24),
        ["tree_a"] = S("tree", 1, 0, 94, 80),
        ["tree_b"] = S("tree", 97, 16, 94, 80),
        ["pine_a"] = S("tree", 4, 101, 85, 91),
        ["pine_b"] = S("tree", 102, 117, 85, 91),
        ["trunk_a"] = S("trunk", 16, 0, 64, 73),
        ["trunk_b"] = S("trunk", 115, 0, 59, 71),
        ["door"] = S("house", 96, 0, 32, 48),
        ["door_alt"] = S("house", 160, 0, 32, 48),
        ["window"] = S("house", 256, 10, 32, 44),
        ["window_small"] = S("house", 226, 0, 28, 32),
        ["chimney"] = S("house", 199, 11, 22, 45),
        ["stove"] = S("kitchen", 0, 96, 32, 27),
        ["stove_fire"] = S("kitchen", 32, 96, 32, 27),
        ["cooking_utensils"] = S("kitchen", 35, 7, 29, 20),
        ["bucket"] = S("kitchen", 32, 32, 32, 28),
        ["plant"] = S("grass", 0, 0, 32, 32),
        ["grass_tuft"] = S("grass", 0, 0, 32, 32)
    };

    public static readonly IReadOnlyDictionary<string, (int Width, int Height)> TextureSizes = new Dictionary<string, (int, int)>(StringComparer.Ordinal)
    {
        ["country"] = (160, 128), ["cabinets"] = (192, 416), ["barrel"] = (128, 64),
        ["chests"] = (64, 96), ["tree"] = (192, 224), ["trunk"] = (192, 96),
        ["house"] = (288, 224), ["kitchen"] = (64, 128), ["grass"] = (96, 192)
    };

    public static SpriteDefinition Get(string name) => Sprites.TryGetValue(name, out var sprite) ? sprite : throw new KeyNotFoundException("Sprite desconhecido: " + name);

    /// <summary>Fits the complete image, centered horizontally and aligned at its base.</summary>
    /// <remarks>Integer enlargement is preferred. Areas smaller than the original require uniform fractional reduction; they never stretch one axis independently.</remarks>
    public static Rectangle PreserveAspect(Rectangle area, SpriteDefinition sprite, bool integer = true)
    {
        ArgumentNullException.ThrowIfNull(sprite);
        if (!float.IsFinite(area.x) || !float.IsFinite(area.y) || !float.IsFinite(area.width) || !float.IsFinite(area.height) || area.width <= 0 || area.height <= 0)
            throw new ArgumentOutOfRangeException(nameof(area), "A área precisa ter dimensões positivas e coordenadas finitas.");
        float scale = Math.Min(area.width / sprite.Source.width, area.height / sprite.Source.height);
        if (integer && scale >= 1) scale = MathF.Floor(scale);
        float width = sprite.Source.width * scale, height = sprite.Source.height * scale;
        return new(area.x + (area.width - width) / 2, area.y + area.height - height, width, height);
    }

    /// <summary>Places a sprite at a world foot point with one uniform scale.</summary>
    public static Rectangle AtFoot(Point foot, SpriteDefinition sprite, float scale = 1)
    {
        ArgumentNullException.ThrowIfNull(sprite);
        if (!float.IsFinite(foot.x) || !float.IsFinite(foot.y) || !float.IsFinite(scale) || scale <= 0)
            throw new ArgumentOutOfRangeException(nameof(scale));
        return new(foot.x - sprite.FootX * scale, foot.y - sprite.FootY * scale, sprite.Source.width * scale, sprite.Source.height * scale);
    }

    public static bool ValidSources() => Sprites.Values.All(sprite => TextureSizes.TryGetValue(sprite.Texture, out var size) && sprite.Source.x + sprite.Source.width <= size.Width && sprite.Source.y + sprite.Source.height <= size.Height);
}

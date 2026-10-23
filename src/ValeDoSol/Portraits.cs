using static ValeDoSol.Ray;
namespace ValeDoSol;

public readonly record struct PortraitDefinition(string Name, string Resource, int Width, int Height)
{
    // Each villager has their own complete PNG. No atlas cell can clip a hairstyle
    // or sample a neighbour when the portrait is reduced for the dialogue panel.
    public Rectangle Source => new(0, 0, Width, Height);
}

public static class Portraits
{
    public const int Count = 8;
    public static readonly IReadOnlyList<PortraitDefinition> Definitions = new PortraitDefinition[]
    {
        new("Lia", "ValeDoSol.Characters.lia.png", 1254, 1254),
        new("Bento", "ValeDoSol.Characters.bento.png", 1254, 1254),
        new("Iara", "ValeDoSol.Characters.iara.png", 1254, 1254),
        new("Theo", "ValeDoSol.Characters.theo.png", 1254, 1254),
        new("Dora", "ValeDoSol.Characters.dora.png", 1254, 1254),
        new("Miguel", "ValeDoSol.Characters.miguel.png", 1254, 1254),
        new("Ceci", "ValeDoSol.Characters.ceci.png", 1254, 1254),
        new("Otávio", "ValeDoSol.Characters.otavio.png", 1254, 1254)
    };
    static readonly Texture[] textures = new Texture[Count];
    public static bool Ready => textures.All(texture => texture.id != 0);

    public static Rectangle Fit(int npc, Rectangle area)
    {
        if (npc < 0 || npc >= Count) throw new ArgumentOutOfRangeException(nameof(npc));
        if (!float.IsFinite(area.width) || !float.IsFinite(area.height) || area.width <= 0 || area.height <= 0)
            throw new ArgumentOutOfRangeException(nameof(area));
        PortraitDefinition portrait = Definitions[npc];
        float scale = Math.Min(area.width / portrait.Width, area.height / portrait.Height);
        float width = portrait.Width * scale, height = portrait.Height * scale;
        return new(area.x + (area.width - width) / 2, area.y + area.height - height, width, height);
    }
    public static void Init()
    {
        try
        {
            for (int npc = 0; npc < Count; npc++)
            {
                PortraitDefinition portrait = Definitions[npc];
                using var stream = typeof(Portraits).Assembly.GetManifestResourceStream(portrait.Resource)
                    ?? throw new InvalidOperationException("Retrato ausente: " + portrait.Name);
                using MemoryStream data = new(); stream.CopyTo(data); byte[] bytes = data.ToArray();
                Image image = LoadImageFromMemory(".png", bytes, bytes.Length);
                if (image.data == 0) throw new InvalidOperationException("Retrato inválido: " + portrait.Name);
                try
                {
                    if (image.width != portrait.Width || image.height != portrait.Height)
                        throw new InvalidOperationException("Dimensões inesperadas no retrato: " + portrait.Name);
                    textures[npc] = LoadTextureFromImage(image);
                }
                finally { UnloadImage(image); }
                if (textures[npc].id == 0) throw new InvalidOperationException("Não foi possível carregar: " + portrait.Name);
                // Portrait illustrations need smooth reduction. World pixel sprites
                // deliberately retain point filtering in Art instead.
                SetTextureFilter(textures[npc], 1);
            }
        }
        catch { Unload(); throw; }
    }
    public static void Draw(int npc, Rectangle area)
    {
        if (npc < 0 || npc >= Count || textures[npc].id == 0) return;
        DrawTexturePro(textures[npc], Definitions[npc].Source, Fit(npc, area), new(), 0, WHITE);
    }
    public static void Unload()
    {
        for (int i = 0; i < textures.Length; i++)
            if (textures[i].id != 0) { UnloadTexture(textures[i]); textures[i] = default; }
    }
}

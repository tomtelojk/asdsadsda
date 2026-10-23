namespace ValeDoSol;

public enum VillageKind { Bench, Planter, Lamp, NoticeBoard, Well, MarketStall, Sign }

/// <summary>The art may rise above the base; movement always uses the same physical base.</summary>
public sealed record VillageProp(VillageKind kind, Rectangle footprint, Rectangle visual, string description);

public static class VillageProps
{
    static VillageProp P(VillageKind kind, int x, int y, int width, int height, int artWidth, int artHeight, string description) =>
        new(kind, new(x, y, width, height), new(x + (width - artWidth) / 2, y + height - artHeight, artWidth, artHeight), description);

    public static readonly VillageProp[] All =
    [
        P(VillageKind.Bench, 360, 480, 48, 12, 60, 42, "Um banco de madeira restaurado por Bento. Dá para ouvir a fonte daqui."),
        P(VillageKind.Bench, 748, 456, 48, 12, 60, 42, "O banco da praça guarda marcas das conversas depois da feira."),
        P(VillageKind.Bench, 424, 680, 64, 12, 76, 42, "Um lugar tranquilo para observar a horta comunitária."),
        P(VillageKind.Bench, 1152, 1048, 64, 12, 76, 42, "Um banco novo para as reuniões que continuam do lado de fora da prefeitura."),
        P(VillageKind.Planter, 680, 256, 48, 12, 56, 42, "A pequena horta de ervas de Miguel fica entre as duas casas."),
        P(VillageKind.Planter, 360, 624, 40, 24, 48, 52, "Dora e Miguel cuidam juntos destas flores."),
        P(VillageKind.Planter, 748, 640, 40, 24, 48, 52, "Flores de cores diferentes dividem a mesma floreira."),
        P(VillageKind.Planter, 1144, 736, 40, 24, 48, 52, "As ervas da entrada perfumam o caminho da prefeitura."),
        P(VillageKind.Planter, 1496, 736, 40, 24, 48, 52, "Mudas trocadas pelos moradores na última reunião."),
        P(VillageKind.Planter, 1248, 928, 40, 24, 48, 52, "Uma floreira de boas-vindas, com espaço livre até a porta."),
        P(VillageKind.Planter, 1424, 928, 40, 24, 48, 52, "Uma pequena placa diz: 'Uma vila floresce quando todos cuidam'."),
        P(VillageKind.Lamp, 390, 432, 12, 10, 36, 82, "O lampião da praça acende quando a tarde vai embora."),
        P(VillageKind.Lamp, 750, 432, 12, 10, 36, 82, "Um lampião ilumina o caminho de volta para casa."),
        P(VillageKind.Lamp, 984, 672, 12, 10, 36, 82, "A luz marca a curva entre a horta e a rua da prefeitura."),
        P(VillageKind.Lamp, 1144, 940, 12, 10, 36, 82, "Uma luz acolhedora diante da prefeitura."),
        P(VillageKind.Lamp, 1496, 940, 12, 10, 36, 82, "O lampião acompanha quem volta das reuniões da vila."),
        P(VillageKind.NoticeBoard, 1176, 448, 48, 12, 64, 56, "Quadro da vila: notícias, encontros e os lugares dos moradores."),
        P(VillageKind.Well, 1600, 720, 48, 32, 64, 82, "Um poço de pedra preservado desde os primeiros jardins do vale."),
        P(VillageKind.MarketStall, 1664, 432, 80, 32, 104, 100, "Uma banca de frutas e geleias feita com a ajuda de toda a vila."),
        P(VillageKind.Sign, 1800, 448, 24, 10, 56, 50, "Prefeitura e jardins: siga pela rua ao sul. Biblioteca: estrada ao norte.")
    ];

    public static Point NoticePoint => new(1200, 488);
    public static IEnumerable<Rectangle> Solids => All.Select(p => p.footprint);
    public static IEnumerable<Rectangle> Clearances => All.Select(p => new Rectangle(p.visual.x - 8, p.visual.y - 8, p.visual.width + 16, p.visual.height + 16));
}

using static ValeDoSol.Logic;
namespace ValeDoSol;

public enum Furnishing { Bed, Table, Fireplace, Chest, Shelf, Kitchen, Counter, Bench, Books, Plant, Rug, Crates }
public record Prop(Furnishing kind, Rectangle bounds, bool solid = true, string description = "");
public record Place(Scene scene, Building building)
{
    public Point Door => new((building.x + building.w / 2f) * 32, (building.y + building.h) * 32 + 16);
}
public static class Map
{
    public static bool Indoors(Scene scene) => scene >= Scene.HOME;
    public static readonly Place[] Village = [new(Scene.SHOP, new(4, 3, 7, 6, 1, "Armazém da Lia", 0xa9694b)), new(Scene.WORKSHOP, new(23, 3, 7, 6, 2, "Oficina do Bento", 0x667991)), new(Scene.GREENHOUSE, new(14, 2, 7, 6, 3, "Casa das flores", 0x89768e)), new(Scene.BAKERY, new(33, 3, 7, 6, 4, "Padaria da Dora", 0xb67a4f)), new(Scene.LIBRARY, new(43, 3, 7, 6, 5, "Casa dos livros", 0x647c73)), new(Scene.COTTAGE, new(6, 24, 7, 6, 6, "Casa do lago", 0x9a7257)), new(Scene.TOWNHALL, new(37, 21, 9, 7, 7, "Prefeitura da vila", 0x426f77))];
    public static readonly Place FarmHouse = new(Scene.HOME, new(2, 2, 7, 6, 0, "Sua casa", 0xae6c48));
    public static IEnumerable<Place> Places(Scene scene) => scene == Scene.FARM ? [FarmHouse] : scene == Scene.VILLAGE ? Village : [];
    public static Place PlaceFor(Scene scene) => scene == Scene.HOME ? FarmHouse : Village.Single(p => p.scene == scene);
    public static string Name(Scene scene) => scene switch { Scene.FARM => "Fazenda Girassol", Scene.VILLAGE => "Vila das Flores", _ => PlaceFor(scene).building.name };
    public static Point Size(Scene scene) => scene == Scene.HOME ? new(640, 480) : Indoors(scene) ? new(704, 512) : new(WORLD_W, WORLD_H);
    public static Point Exit(Scene scene) { Point size = Size(scene); return new(size.x / 2, size.y - 48); }
    public static Point Arrival(Scene scene) { Point exit = Exit(scene); return new(exit.x, exit.y - 48); }
    public static Point NoticePoint => new(352, 208);
    public static Point BedApproach(Scene scene) { var b = Props(scene).First(p => p.kind == Furnishing.Bed).bounds; return new(b.x - 20, b.y + b.height - 24); }
    public static Point ServicePoint(Scene scene) { var b = Props(scene).First(p => p.kind == Furnishing.Counter).bounds; return new(b.x + b.width / 2, b.y + b.height + 20); }
    public static Rectangle Footprint(Building b) => new(b.x * 32, b.y * 32 + 32, b.w * 32, b.h * 32 - 32);
    public static Rectangle VisualBounds(Building b) => new(b.x * 32 - 16, b.y * 32 - (b.type == 7 ? 108 : 24), b.w * 32 + 32, b.h * 32 + (b.type == 7 ? 132 : 48));
    public static Rectangle[] Roads(Scene scene)
    {
        List<Rectangle> roads = [new(0, 344, WORLD_W, 80)];
        if (scene == Scene.FARM) { roads.Add(new(384, 0, 96, WORLD_H)); roads.Add(new(144, 240, 64, 144)); roads.Add(new(672, 424, 32, 256)); }
        else { roads.Add(new(512, 208, 96, 1024)); roads.Add(new(256, 384, 96, 640)); roads.Add(new(416, 416, 320, 224)); }
        roads.Add(new(1024, 384, 96, 848)); roads.Add(new(304, 960, 1248, 80));
        foreach (Place place in Places(scene))
        {
            float junction = place.Door.y < 424 ? 424 : 1000;
            roads.Add(new(place.Door.x - 32, place.Door.y - 16, 64, Math.Max(32, junction - place.Door.y + 16)));
        }
        return roads.ToArray();
    }
    public static bool Touch(Rectangle a, Rectangle b) => a.x < b.x + b.width && a.x + a.width > b.x && a.y < b.y + b.height && a.y + a.height > b.y;
    public static Rectangle Inflated(Rectangle r, int amount) => new(r.x - amount, r.y - amount, r.width + amount * 2, r.height + amount * 2);
    public static readonly Rectangle Fountain = new(512, 480, 96, 64);
    public static readonly Rectangle CommunityBed = new(736, 672, 240, 160);
    static readonly Dictionary<Scene, Prop[]> rooms = BuildRooms();
    public static Prop[] Props(Scene scene) => rooms.GetValueOrDefault(scene, []);
    static Dictionary<Scene, Prop[]> BuildRooms()
    {
        // Match collision rectangles to complete artwork, anchored at its original base.
        Prop P(Furnishing k, int x, int y, int w, int h, string description = "", bool solid = true)
        {
            int nw = w, nh = h;
            switch (k)
            {
                case Furnishing.Bed: nw = 48; nh = 81; break;
                case Furnishing.Table: int scale = w >= 144 ? 2 : 1; nw = 64 * scale; nh = 46 * scale; break;
                case Furnishing.Fireplace: nw = 64; nh = 54; break;
                case Furnishing.Chest: nw = 64; nh = 48; break;
                case Furnishing.Shelf: case Furnishing.Books: case Furnishing.Counter: nw = Math.Max(32, w / 32 * 32); nh = 69; break;
                case Furnishing.Kitchen: nw = w >= 128 ? 128 : 64; nh = nw / 64 * 27; break;
                case Furnishing.Bench: nw = Math.Max(64, w / 64 * 64); nh = 46; break;
                case Furnishing.Crates: nw = Math.Max(1, w / 32) * 32 - 4; nh = 38; break;
                case Furnishing.Plant: nw = Math.Max(32, w / 32 * 32); nh = 32; break;
            }
            return new(k, new(x + (w - nw) / 2, y + h - nh, nw, nh), solid, description);
        }
        return new()
        {
            [Scene.HOME] = [P(Furnishing.Rug, 224, 240, 160, 96, solid: false), P(Furnishing.Bed, 432, 96, 112, 128, "Uma cama quentinha. E para descansar ao lado."), P(Furnishing.Table, 112, 128, 96, 64, "Uma xícara de chá e seu caderno da fazenda."), P(Furnishing.Fireplace, 48, 80, 96, 48, "O fogo estala baixinho. Sua casa está aquecida."), P(Furnishing.Chest, 512, 304, 64, 40, "Seu baú de lembranças. O progresso está salvo."), P(Furnishing.Shelf, 208, 88, 96, 32, "Livros de cultivo e cartas de quem morou aqui antes."), P(Furnishing.Kitchen, 48, 304, 112, 48, "Uma cozinha pequena, mas pronta para as suas colheitas."), P(Furnishing.Plant, 560, 96, 32, 32, "As flores procuram a luz da janela.")],
            [Scene.SHOP] = [P(Furnishing.Counter, 240, 224, 224, 40, "O balcão da Lia."), P(Furnishing.Shelf, 48, 112, 96, 112, "Sementes organizadas em envelopes feitos à mão."), P(Furnishing.Shelf, 560, 112, 96, 112, "Potes de geleia, grãos e um café recém-passado."), P(Furnishing.Crates, 80, 304, 80, 64, "Caixas de produtos dos moradores do vale."), P(Furnishing.Table, 528, 320, 96, 64, "A mesa de café de quem passa pelo armazém."), P(Furnishing.Rug, 288, 368, 128, 64, solid: false)],
            [Scene.WORKSHOP] = [P(Furnishing.Bench, 64, 128, 192, 56, "Bento deixou aqui o desenho do palco de Ceci."), P(Furnishing.Bench, 448, 128, 176, 56, "Ferramentas bem cuidadas, tábuas e um barco em miniatura."), P(Furnishing.Crates, 64, 304, 112, 64, "Madeiras separadas por tipo e tamanho."), P(Furnishing.Table, 448, 304, 112, 64, "Duas canecas: uma para Bento, outra para as visitas."), P(Furnishing.Chest, 304, 96, 80, 40, "Uma coleção surpreendentemente organizada de pregos.")],
            [Scene.BAKERY] = [P(Furnishing.Fireplace, 48, 96, 128, 64, "Um cheiro de pão assado toma conta da padaria."), P(Furnishing.Counter, 272, 208, 272, 48, "Pães, broas e biscoitos da Dora."), P(Furnishing.Kitchen, 448, 96, 176, 48, "Farinha sobre a bancada e a receita da família."), P(Furnishing.Table, 80, 320, 112, 64, "Uma mesa para dividir pão e conversa."), P(Furnishing.Table, 480, 336, 112, 64, "Dora sempre deixa uma cadeira para quem chega."), P(Furnishing.Shelf, 224, 96, 128, 40, "Potes de mel, açúcar e frutas do vale.")],
            [Scene.LIBRARY] = [P(Furnishing.Books, 48, 96, 112, 56, "Mapas, receitas e histórias reunidos por Otávio."), P(Furnishing.Books, 224, 96, 112, 56, "Cartas antigas de famílias da vila."), P(Furnishing.Books, 400, 96, 112, 56, "Poemas que Ceci quer levar para a feira."), P(Furnishing.Books, 560, 224, 96, 64, "Cadernos sobre as plantas de cada estação."), P(Furnishing.Table, 240, 304, 176, 80, "O caderno aberto espera a próxima história."), P(Furnishing.Rug, 208, 272, 240, 144, solid: false), P(Furnishing.Plant, 48, 336, 48, 48, "Uma samambaia quase tão antiga quanto os livros.")],
            [Scene.GREENHOUSE] = [P(Furnishing.Table, 64, 128, 160, 56, "Theo está catalogando sementes da região."), P(Furnishing.Plant, 272, 112, 64, 48, "Mudas de flores que gostam de sombra."), P(Furnishing.Plant, 400, 112, 64, 48, "Ervas cuidadas por Miguel."), P(Furnishing.Plant, 560, 112, 64, 48, "Flores vermelhas voltadas para a janela."), P(Furnishing.Table, 448, 304, 160, 64, "Lupas, etiquetas e um caderno cheio de desenhos."), P(Furnishing.Shelf, 64, 304, 112, 48, "Chás e pequenas descobertas dos dois amigos.")],
            [Scene.TOWNHALL] = [P(Furnishing.Rug, 240, 336, 224, 96, solid: false), P(Furnishing.Books, 288, 112, 128, 69, "O quadro da prefeitura reúne notícias, projetos e horários dos moradores. Aproxime-se para consultar."), P(Furnishing.Counter, 64, 224, 160, 69, "Balcão de atendimento: ninguém precisa enfrentar uma pilha de papéis para ser ouvido."), P(Furnishing.Books, 48, 112, 128, 69, "Atas das reuniões, mapas das primeiras trilhas e registros da Fazenda Girassol."), P(Furnishing.Books, 528, 112, 128, 69, "A pasta da horta comunitária guarda desenhos de Aurora e anotações de cada morador."), P(Furnishing.Table, 432, 304, 160, 92, "A mesa das reuniões tem espaço para ideias, café e gente que chega pela primeira vez."), P(Furnishing.Bench, 64, 368, 128, 46, "Bancos feitos por Bento para as tardes de conversa."), P(Furnishing.Plant, 608, 352, 32, 32, "Miguel deixou uma muda para lembrar que todo projeto começa pequeno.")],
            [Scene.COTTAGE] = [P(Furnishing.Bed, 64, 112, 96, 128, "A cama fica perto da janela que dá para o lago."), P(Furnishing.Books, 240, 96, 112, 48, "Partituras de Ceci e os diários de pesca de Iara."), P(Furnishing.Table, 448, 128, 144, 64, "Um verso inacabado e uma caixa de iscas."), P(Furnishing.Kitchen, 48, 336, 144, 48, "A cozinha das duas amigas."), P(Furnishing.Rug, 256, 256, 160, 96, solid: false), P(Furnishing.Chest, 544, 336, 64, 40, "Lembranças de viagens e apresentações antigas.")]
        };
    }
    static readonly Dictionary<Scene, int[][]> trees = new() { [Scene.FARM] = Forest(Scene.FARM), [Scene.VILLAGE] = Forest(Scene.VILLAGE) };
    public static int[][] Trees(Scene scene) => trees.GetValueOrDefault(scene, []);
    static int[][] Forest(Scene scene)
    {
        List<int[]> result = []; Rectangle[] roads = Roads(scene);
        for (int i = 0; i < 400; i++)
        {
            int x = i == 0 ? 576 : i == 1 ? 736 : 48 + i * 239 % 1952, y = i < 2 ? 112 : 112 + i * 337 % 1200; x = x / 32 * 32 + 16; y = y / 32 * 32 + 16;
            Rectangle canopy = new(x - 48, y - 104, 96, 116);
            if (roads.Any(r => Touch(canopy, Inflated(r, 12))) || Places(scene).Any(p => Touch(canopy, Inflated(VisualBounds(p.building), 16))) || water_bodies(scene).Any(r => Touch(canopy, Inflated(r, 16)))) continue;
            if (scene == Scene.FARM && Touch(canopy, Inflated(Garden.Bounds, 20))) continue;
            if (scene == Scene.VILLAGE && (Touch(canopy, Inflated(CommunityBed, 20)) || VillageProps.Clearances.Any(r => Touch(canopy, r)))) continue;
            if (result.Any(t => Math.Abs(x - (t[0] * 32 + 16)) < 64 && Math.Abs(y - (t[1] * 32 + 16)) < 72)) continue;
            result.Add([(x - 16) / 32, (y - 16) / 32]);
        }
        return result.ToArray();
    }
    public static IEnumerable<Rectangle> Solids(Scene scene)
    {
        if (Indoors(scene)) { foreach (Prop p in Props(scene)) if (p.solid) yield return p.bounds; yield break; }
        foreach (Place p in Places(scene)) yield return Footprint(p.building);
        foreach (Rectangle r in water_bodies(scene)) yield return r;
        if (scene == Scene.FARM) foreach (Rectangle r in Garden.Solids) yield return r;
        if (scene == Scene.VILLAGE)
        {
            yield return Fountain;
            yield return CommunityBed;
            foreach (Rectangle r in VillageProps.Solids) yield return r;
        }
        foreach (int[] t in Trees(scene)) yield return new(t[0] * 32 + 6, t[1] * 32 + 8, 20, 14);
    }
    public static Scene Outside(Scene room) => room == Scene.HOME ? Scene.FARM : Scene.VILLAGE;
    public static (Point at, Scene next, Point arrival) Portal(Scene from, Scene goal)
    {
        if (Indoors(from)) { Scene next = Outside(from); return (Exit(from), next, PlaceFor(from).Door); }
        if (from == Scene.FARM && goal != Scene.HOME) return (new(WORLD_W - 20, 384), Scene.VILLAGE, new(48, 384));
        if (from == Scene.VILLAGE && goal is Scene.FARM or Scene.HOME) return (new(20, 384), Scene.FARM, new(WORLD_W - 48, 384));
        return (PlaceFor(goal).Door, goal, Arrival(goal));
    }
}

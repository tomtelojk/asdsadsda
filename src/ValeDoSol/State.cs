using System.Text.Json;
using System.Text.Json.Serialization;
namespace ValeDoSol;

public struct Plot { public bool tilled, watered; public int crop, growth; }
public enum Scene { FARM, VILLAGE, HOME, SHOP, WORKSHOP, BAKERY, LIBRARY, GREENHOUSE, COTTAGE, TOWNHALL }
public class State
{
    public int edition = 8;
    [JsonRequired] public int version = 3, day = 1, minute = 480, coins = 120, energy = 100, max_energy = 100;
    [JsonRequired] public int[] seeds = [10, 5, 0], inventory = [0, 0, 0];
    public int wood, stone, fish, harvested, selected, watering_level = 1;
    public Scene scene;
    [JsonRequired] public Point player = new(400, 288);
    [JsonRequired] public Plot[] plots = Enumerable.Range(0, 48).Select(i => new Plot { tilled = i < 16, crop = i < 3 ? 0 : -1, growth = i < 3 ? 1 : 0 }).ToArray();
    [JsonRequired] public int[] friendship = new int[Social.Count], talked_day = new int[Social.Count], gifted_day = new int[Social.Count];
    public int[] conversations = new int[Social.Count];
    public Point[] npc_positions = new Point[Social.Count];
    public Scene[] npc_scenes = Enumerable.Repeat(Scene.VILLAGE, Social.Count).ToArray();
    public int npc_day = 1;
    public bool npc_saved;
    public int story_stage, story_day;
    public uint story_flags;
    public bool sharp_pixels = true;
    public int[] npc_last_line = Enumerable.Repeat(-1, Social.Count).ToArray();
    public int[] npc_last_topic = Enumerable.Repeat(-1, Social.Count).ToArray();
    public uint met, claimed, gathered, tutorial, rng = 4871;
    public bool rod;
}
public record Crop(string name, int days, int price, int sell, uint color);
public record Villager(string name, string role, string first, string[] later);
public record Building(int x, int y, int w, int h, int type, string name, uint roof);
public record Resource(Scene scene, int x, int y, int type);
[JsonSourceGenerationOptions(IncludeFields = true, WriteIndented = true)]
[JsonSerializable(typeof(State))]
internal partial class SaveContext : JsonSerializerContext { }
public static class Logic
{
    public const int WORLD_W = 2048, WORLD_H = 1408, MAX_PATH = 3000;
    public const Scene FARM = Scene.FARM, VILLAGE = Scene.VILLAGE, HOME = Scene.HOME, SHOP = Scene.SHOP;
    public static readonly Crop[] CROPS = [new("Cenoura", 3, 15, 14, 0xef9c42), new("Nabo", 2, 10, 9, 0xeee1bc), new("Morango", 4, 25, 23, 0xdc6c79)];
    public static readonly Villager[] NPCS = [
    new("Lia","Dona do armazém","Você deve ser o novo fazendeiro da Fazenda Girassol! Eu sou a Lia. A vila precisava desse seu jeito de recomeçar.",["Minha avó dizia: uma semente é uma promessa pequena. Regue as suas todos os dias, combinado?","Sempre tem espaço para uma conversa e um café por aqui. Como vai a sua horta?"]),
 new("Bento","Carpinteiro","Bento. Carpinteiro, colecionador de pregos e péssimo contador de piadas. Se quiser melhorar seu regador, é comigo.",["Traga 8 madeiras e 60 moedas. Seu regador de cobre vai alcançar o canteiro escolhido e os quatro vizinhos.","Uma boa ferramenta dura anos. Uma amizade bem cuidada dura ainda mais."]),
 new("Iara","Pescadora","O lago tem mais histórias do que peixes. Tome, uma vara antiga minha! Perto da água, aperte F e espere a fisgada.",["Quando aparecer AGORA, aperte Espaço. Se puxar antes ou depois, o peixe leva a isca e deixa a história.","Dias de chuva são os meus favoritos. O mundo fica quietinho e os peixes saem para conversar."]),
 new("Theo","Botânico","Sou Theo! Estou estudando o solo do vale. Três cenouras da sua horta seriam perfeitas para minha pesquisa.",["Pode levar o tempo que precisar. Cada coisa cresce no seu próprio ritmo por aqui.","Quando chove, deixe o regador em casa. A natureza também gosta de ajudar."]),
 new("Dora","Padeira","Sou Dora, da banca de pães. Você é o fazendeiro novo! Venha cedo qualquer dia; pão quentinho ajuda a enfrentar até uma horta teimosa.",["Aprendi a fazer pão com meu pai. A receita era simples: acordar cedo e nunca economizar no carinho.","Quero fazer uma torta com frutas de todas as estações. Primeiro preciso convencer Lia a me emprestar as formas."]),
 new("Miguel","Cuidador da horta de ervas","Miguel. Cuido das ervas do vale e ajudo quem precisa de uma pausa. Se me encontrar agachado num canteiro, pode falar: eu sempre perco a noção da hora.",["Minha avó tratava o jardim como parte da família. Fiquei com os livros dela e a mania de conversar com as plantas.","Theo e eu queremos organizar uma pequena horta comunitária. Um lugar para aprender e dividir o que cresce."]),
 new("Ceci","Musicista","Ei! Sou Ceci. Se escutar alguém repetindo os mesmos três acordes, provavelmente sou eu tentando terminar uma música. Você chegou numa boa hora para a vila.",["Vim passar uma semana aqui e fiquei. O silêncio do vale me deu vontade de escrever músicas de novo.","Estou juntando coragem para tocar na feira. Bento faz o palco, Dora traz os pães e Otávio escolhe um poema."]),
 new("Otávio","Guardião das histórias","Bem-vindo, rapaz. Sou Otávio. Guardo livros, mapas e algumas histórias que ficaram sem dono. Sua fazenda já apareceu em muitas delas.",["Quando eu era pequeno, a estrada até sua fazenda parecia enorme. Algumas distâncias diminuem quando a gente ganha companhia.","Se encontrar uma carta antiga ou um nome riscado numa árvore, me conte. A história do vale está espalhada por aí."])];
    public static readonly Resource[] RESOURCES = [new(FARM, 10, 15, 0), new(FARM, 7, 13, 0), new(FARM, 16, 18, 0), new(VILLAGE, 21, 18, 0), new(FARM, 15, 15, 1), new(FARM, 30, 12, 1)];
    static bool In(int n, int min, int max) => n >= min && n <= max;
    public static bool state_valid(State? s)
    {
        if (s is null || s.version != 3 || s.edition != 8 || !In(s.day, 1, 100000) || !In(s.minute, 360, 1440) || !In(s.coins, 0, 1_000_000_000) || !In(s.max_energy, 100, 200) || !In(s.energy, 0, s.max_energy) || !Enum.IsDefined(s.scene)) return false;
        if (!float.IsFinite(s.player.x) || !float.IsFinite(s.player.y) || s.player.x < 16 || s.player.x > world_size(s.scene).x - 16 || s.player.y < 32 || s.player.y > world_size(s.scene).y - 24) return false;
        if (!Story.IsValid(s) || s.npc_last_line?.Length != Social.Count || s.npc_last_topic?.Length != Social.Count || s.npc_last_line.Any(n => !In(n, -1, 9999)) || s.npc_last_topic.Any(n => !In(n, -1, 9999))) return false;
        if (s.npc_positions?.Length != Social.Count || s.npc_scenes?.Length != Social.Count || s.npc_day < 1 || s.npc_day > s.day) return false;
        if (s.npc_saved && s.npc_positions.Where((p, i) => !Enum.IsDefined(s.npc_scenes[i]) || !float.IsFinite(p.x) || !float.IsFinite(p.y) || p.x < 12 || p.y < 12 || p.x > Map.Size(s.npc_scenes[i]).x || p.y > Map.Size(s.npc_scenes[i]).y).Any()) return false;
        if (s.seeds?.Length != 3 || s.inventory?.Length != 3 || s.plots?.Length != 48 || s.friendship?.Length != Social.Count || s.talked_day?.Length != Social.Count || s.gifted_day?.Length != Social.Count || s.conversations?.Length != Social.Count) return false;
        if (s.seeds.Any(n => !In(n, 0, 10_000_000)) || s.inventory.Any(n => !In(n, 0, 10_000_000)) || s.plots.Any(p => !In(p.crop, -1, 2) || !In(p.growth, 0, 100000))) return false;
        if (s.friendship.Any(n => !In(n, 0, 5)) || s.talked_day.Any(n => !In(n, 0, s.day)) || s.gifted_day.Any(n => !In(n, 0, s.day))) return false;
        return new[] { s.wood, s.stone, s.fish, s.harvested }.All(n => In(n, 0, 10_000_000)) && In(s.selected, 0, 2) && In(s.watering_level, 1, 2) && s.met < (1u << Social.Count) && s.claimed < 8 && s.gathered < 64 && s.tutorial < 8 && s.conversations.All(n => In(n, 0, 10_000_000)) && s.rng > 0 && s.rng < 2147483647;
    }
    public static bool state_save(State s, string path)
    {
        if (!state_valid(s) || string.IsNullOrWhiteSpace(path)) return false;
        try { string tmp = path + ".tmp"; File.WriteAllText(tmp, JsonSerializer.Serialize(s, SaveContext.Default.State)); File.Move(tmp, path, true); return true; } catch (IOException) { return false; } catch (UnauthorizedAccessException) { return false; }
    }
    public static bool state_load(out State result, string path)
    {
        result = new();
        try
        {
            if (new FileInfo(path).Length > 200000) return false;
            string raw = File.ReadAllText(path);
            var s = JsonSerializer.Deserialize(raw, SaveContext.Default.State);
            using var meta = JsonDocument.Parse(raw);
            if (s is null) return false;
            if (!meta.RootElement.TryGetProperty("edition", out _)) s.edition = 3;
            if (s.edition is < 3 or > 8) return false;
            if (s.edition is 3 or 4 && s.friendship?.Length == 4 && s.talked_day?.Length == 4 && s.gifted_day?.Length == 4)
            {
                Array.Resize(ref s.friendship, Social.Count);
                Array.Resize(ref s.talked_day, Social.Count);
                Array.Resize(ref s.gifted_day, Social.Count);
                s.conversations = new int[Social.Count];
            }
            s.edition = 8;
            if (!state_valid(s)) return false;
            result = s;
            return true;
        }
        catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException or NotSupportedException) { return false; }
    }
    public static bool raining(State s) => s.day % 4 == 0;
    public static int next_day(State s) { int grown = 0; foreach (ref Plot p in s.plots.AsSpan()) { if (p.crop >= 0 && (p.watered || raining(s))) { p.growth = Math.Min(CROPS[p.crop].days, p.growth + 1); grown++; } p.watered = false; } s.day = Math.Min(100000, s.day + 1); s.minute = 480; s.energy = s.max_energy; s.gathered = 0; if (raining(s)) foreach (ref Plot p in s.plots.AsSpan()) if (p.tilled) p.watered = true; return grown; }
    public static string farm_action(State s, int index, int tool, out bool ok)
    {
        ok = false; if (!In(index, 0, 47)) return "Escolha um canteiro."; ref Plot p = ref s.plots[index]; if (s.energy < 3) return "Hora de descansar. Sua casa está esperando.";
        switch (tool)
        {
            case 0: if (p.tilled) return "Terra pronta! Escolha sementes com 2."; p.tilled = true; s.tutorial |= 1; break;
            case 1: if (!p.tilled) return "Prepare a terra com a enxada (1)."; if (p.crop >= 0) return "Já existe uma plantinha aqui."; if (s.seeds[s.selected] == 0) return "Sem sementes. Visite o armazém da Lia."; p.crop = s.selected; p.growth = 0; p.watered = raining(s); s.seeds[s.selected]--; s.tutorial |= 2; break;
            case 2: if (!p.tilled) return "Prepare a terra antes de regar."; if (p.watered) return "Esse canteiro já recebeu água hoje."; p.watered = true; s.tutorial |= 4; if (s.watering_level == 2) foreach (int n in new[] { index - 8, index + 8, index % 8 > 0 ? index - 1 : -1, index % 8 < 7 ? index + 1 : -1 }) if (In(n, 0, 47) && s.plots[n].tilled) s.plots[n].watered = true; break;
            case 3: if (p.crop < 0) return "Não há nada para colher aqui."; if (p.growth < CROPS[p.crop].days) return "Ainda está crescendo. Regue e volte outro dia."; if (s.inventory[p.crop] >= 10000000) return "A mochila está cheia. Venda sua colheita."; s.inventory[p.crop]++; s.harvested = Math.Min(10000000, s.harvested + 1); p.crop = -1; p.growth = 0; break;
            default: return "Selecione uma ferramenta para cultivar.";
        }
        s.energy -= 3; ok = true; return new[] { "Terra preparada. Um novo começo!", "Semente plantada. Agora é só regar!", "Água fresquinha! Sua planta agradece.", "Colheita guardada. Venda para Lia na vila." }[tool];
    }
    public static bool buy_seeds(State s, int c) { if (!In(c, 0, 2) || s.coins < CROPS[c].price || s.seeds[c] > 9999995) return false; s.coins -= CROPS[c].price; s.seeds[c] += 5; return true; }
    public static int sell_all(State s) { long total = (long)s.fish * 18; for (int i = 0; i < 3; i++) total += (long)s.inventory[i] * CROPS[i].sell; if (total + s.coins > 1000000000) return -1; s.fish = 0; Array.Clear(s.inventory); s.coins += (int)total; return (int)total; }
    public static void talk_to(State s, int npc) { if (!In(npc, 0, Social.Count - 1)) return; s.met |= 1u << npc; if (s.talked_day[npc] != s.day) { s.talked_day[npc] = s.day; s.friendship[npc] = Math.Min(5, s.friendship[npc] + 1); } if (npc == 2) s.rod = true; }
    public static bool give_gift(State s, int npc) { if (!In(npc, 0, Social.Count - 1) || s.gifted_day[npc] == s.day) return false; int i = Array.FindIndex(s.inventory, n => n > 0); if (i < 0) return false; s.inventory[i]--; s.gifted_day[npc] = s.day; s.friendship[npc] = Math.Min(5, s.friendship[npc] + 1); return true; }
    public static bool upgrade_can(State s) { if (s.watering_level == 2 || s.wood < 8 || s.coins < 60) return false; s.wood -= 8; s.coins -= 60; s.watering_level = 2; return true; }
    public static bool claim_quest(State s, int q) { if (!In(q, 0, 2) || (s.claimed & (1u << q)) != 0) return false; int reward = new[] { 40, 60, 75 }[q]; if ((q == 0 && (s.met & 15) != 15) || (q == 1 && s.harvested < 5) || (q == 2 && s.inventory[0] < 3) || s.coins > 1000000000 - reward) return false; if (q == 2) s.inventory[0] -= 3; s.claimed |= 1u << q; s.coins += reward; return true; }
    public static float random_unit(State s) { s.rng = (uint)((ulong)s.rng * 16807u % 2147483647u); return (s.rng - 1) / 2147483646f; }
    public static Point plot_position(int i) => Garden.PlotPosition(i);
    public static int plot_at(float x, float y) => Garden.PlotAt(x, y);

    public static Point world_size(Scene scene) => Map.Size(scene);
    public static Rectangle[] water_bodies(Scene scene) => scene is FARM or VILLAGE ? [new((scene == FARM ? 26 : 25) * 32, 512, (scene == FARM ? 6 : 7) * 32, 128), new(1408, 1088, 384, 192)] : [];
    public static Rectangle[] interior_solids(Scene scene) => Map.Props(scene).Where(p => p.solid).Select(p => p.bounds).ToArray();
    public static int[][] scene_trees(Scene scene) => Map.Trees(scene);
    public static bool blocked(Scene scene, float x, float y) => Physics.Blocked(scene, x, y);
    public static int find_path(Scene scene, Point start, Point goal, Point[] output) => Physics.FindPath(scene, start, goal, output);
}

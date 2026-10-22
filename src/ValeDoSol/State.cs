using System.Text.Json;
using System.Text.Json.Serialization;
namespace ValeDoSol;

public struct Plot { public bool tilled, watered; public int crop, growth; }
public enum Scene { FARM, VILLAGE, HOME, SHOP }
public class State
{
    public int edition = 4;
    [JsonRequired] public int version = 3, day = 1, minute = 480, coins = 120, energy = 100, max_energy = 100;
    [JsonRequired] public int[] seeds = [10, 5, 0], inventory = [0, 0, 0];
    public int wood, stone, fish, harvested, selected, watering_level = 1;
    public Scene scene;
    [JsonRequired] public Point player = new(400, 288);
    [JsonRequired] public Plot[] plots = Enumerable.Range(0, 48).Select(i => new Plot { tilled = i < 16, crop = i < 3 ? 0 : -1, growth = i < 3 ? 1 : 0 }).ToArray();
    [JsonRequired] public int[] friendship = new int[4], talked_day = new int[4], gifted_day = new int[4];
    public uint met, claimed, gathered, tutorial, rng = 4871;
    public bool rod;
}
public record Crop(string name, int days, int price, int sell, uint color);
public record Villager(string name, string role, string first, string[] later, uint coat, uint hair, Point morning, Point afternoon);
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
    new("Lia","Dona do armazém","Você deve ser a nova pessoa da Fazenda Girassol! Eu sou a Lia. A vila precisava desse seu jeito de recomeçar.",["Minha avó dizia: uma semente é uma promessa pequena. Regue as suas todos os dias, combinado?","Sempre tem espaço para uma conversa e um café por aqui. Como vai a sua horta?"],0xd69a70,0x694b3d,new(336,336),new(432,400)),
 new("Bento","Carpinteiro","Bento. Carpinteiro, colecionador de pregos e péssimo contador de piadas. Se quiser melhorar seu regador, é comigo.",["Traga 8 madeiras e 60 moedas. Seu regador de cobre vai alcançar o canteiro escolhido e os quatro vizinhos.","Uma boa ferramenta dura anos. Uma amizade bem cuidada dura ainda mais."],0x819ca1,0x655e50,new(784,336),new(688,464)),
 new("Iara","Pescadora","O lago tem mais histórias do que peixes. Tome, uma vara antiga minha! Perto da água, aperte F e espere a fisgada.",["Quando aparecer AGORA, aperte Espaço. Se puxar antes ou depois, o peixe leva a isca e deixa a história.","Dias de chuva são os meus favoritos. O mundo fica quietinho e os peixes saem para conversar."],0xa28bb0,0x3e4e47,new(912,496),new(784,496)),
 new("Theo","Botânico","Sou Theo! Estou estudando o solo do vale. Três cenouras da sua horta seriam perfeitas para minha pesquisa.",["Pode levar o tempo que precisar. Cada coisa cresce no seu próprio ritmo por aqui.","Quando chove, deixe o regador em casa. A natureza também gosta de ajudar."],0x89a76b,0x9d714f,new(528,528),new(272,528))];
    public static readonly Building[] FARM_BUILDINGS = [new(2, 2, 7, 5, 0, "Sua casa", 0xb97958)];
    public static readonly Building[] VILLAGE_BUILDINGS = [new(4, 3, 7, 5, 1, "Armazém da Lia", 0xbb785a), new(23, 3, 7, 5, 2, "Oficina do Bento", 0x728b9a), new(14, 2, 6, 5, 3, "Casa das flores", 0xa88a9b)];
    public static readonly Resource[] RESOURCES = [new(FARM, 10, 15, 0), new(FARM, 7, 13, 0), new(FARM, 16, 18, 0), new(VILLAGE, 21, 18, 0), new(FARM, 15, 15, 1), new(FARM, 30, 12, 1)];
    public static readonly int[][] FARM_TREES = [[1, 3], [11, 2], [14, 2], [17, 2], [20, 2], [23, 2], [27, 2], [31, 3], [33, 5], [2, 11], [10, 10], [2, 18], [6, 19], [10, 18], [32, 19], [22, 20]];
    public static readonly int[][] VILLAGE_TREES = [[1, 4], [2, 2], [12, 2], [22, 2], [32, 3], [33, 7], [2, 12], [4, 18], [8, 20], [19, 20], [33, 20], [21, 7]];
    static bool In(int n, int min, int max) => n >= min && n <= max;
    public static bool state_valid(State? s)
    {
        if (s is null || s.version != 3 || !In(s.day, 1, 100000) || !In(s.minute, 360, 1440) || !In(s.coins, 0, 1_000_000_000) || !In(s.max_energy, 100, 200) || !In(s.energy, 0, s.max_energy) || !Enum.IsDefined(s.scene)) return false;
        if (!float.IsFinite(s.player.x) || !float.IsFinite(s.player.y) || s.player.x < 16 || s.player.x > world_size(s.scene).x - 16 || s.player.y < 32 || s.player.y > world_size(s.scene).y - 24) return false;
        if (s.seeds?.Length != 3 || s.inventory?.Length != 3 || s.plots?.Length != 48 || s.friendship?.Length != 4 || s.talked_day?.Length != 4 || s.gifted_day?.Length != 4) return false;
        if (s.seeds.Any(n => !In(n, 0, 10_000_000)) || s.inventory.Any(n => !In(n, 0, 10_000_000)) || s.plots.Any(p => !In(p.crop, -1, 2) || !In(p.growth, 0, 100000))) return false;
        if (s.friendship.Any(n => !In(n, 0, 5)) || s.talked_day.Any(n => !In(n, 0, s.day)) || s.gifted_day.Any(n => !In(n, 0, s.day))) return false;
        return new[] { s.wood, s.stone, s.fish, s.harvested }.All(n => In(n, 0, 10_000_000)) && In(s.selected, 0, 2) && In(s.watering_level, 1, 2) && s.met < 16 && s.claimed < 8 && s.gathered < 64 && s.tutorial < 8 && s.rng > 0 && s.rng < 2147483647;
    }
    public static bool state_save(State s, string path)
    {
        if (!state_valid(s) || string.IsNullOrWhiteSpace(path)) return false;
        try { string tmp = path + ".tmp"; File.WriteAllText(tmp, JsonSerializer.Serialize(s, SaveContext.Default.State)); File.Move(tmp, path, true); return true; } catch (IOException) { return false; } catch (UnauthorizedAccessException) { return false; }
    }
    public static bool state_load(out State result, string path)
    {
        result = new(); try { if (new FileInfo(path).Length > 200000) return false; var s = JsonSerializer.Deserialize(File.ReadAllText(path), SaveContext.Default.State); if (!state_valid(s)) return false; result = s!; return true; } catch (Exception e) when (e is IOException or JsonException or UnauthorizedAccessException or NotSupportedException) { return false; }
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
    public static void talk_to(State s, int npc) { if (!In(npc, 0, 3)) return; s.met |= 1u << npc; if (s.talked_day[npc] != s.day) { s.talked_day[npc] = s.day; s.friendship[npc] = Math.Min(5, s.friendship[npc] + 1); } if (npc == 2) s.rod = true; }
    public static bool give_gift(State s, int npc) { if (!In(npc, 0, 3) || s.gifted_day[npc] == s.day) return false; int i = Array.FindIndex(s.inventory, n => n > 0); if (i < 0) return false; s.inventory[i]--; s.gifted_day[npc] = s.day; s.friendship[npc] = Math.Min(5, s.friendship[npc] + 1); return true; }
    public static bool upgrade_can(State s) { if (s.watering_level == 2 || s.wood < 8 || s.coins < 60) return false; s.wood -= 8; s.coins -= 60; s.watering_level = 2; return true; }
    public static bool claim_quest(State s, int q) { if (!In(q, 0, 2) || (s.claimed & (1u << q)) != 0) return false; int reward = new[] { 40, 60, 75 }[q]; if ((q == 0 && s.met != 15) || (q == 1 && s.harvested < 5) || (q == 2 && s.inventory[0] < 3) || s.coins > 1000000000 - reward) return false; if (q == 2) s.inventory[0] -= 3; s.claimed |= 1u << q; s.coins += reward; return true; }
    public static float random_unit(State s) { s.rng = (uint)((ulong)s.rng * 16807u % 2147483647u); return (s.rng - 1) / 2147483646f; }
    public static Point plot_position(int i) => new(544 + i % 8 * 32, 256 + i / 8 * 32);
    public static int plot_at(float x, float y) { int c = (int)MathF.Floor(x / 32) - 17, r = (int)MathF.Floor(y / 32) - 8; return c >= 0 && c < 8 && r >= 0 && r < 6 ? r * 8 + c : -1; }

    public static Point world_size(Scene scene) => scene switch { Scene.HOME => new(640, 480), Scene.SHOP => new(704, 512), _ => new(WORLD_W, WORLD_H) };
    public static Rectangle[] water_bodies(Scene scene) => scene is FARM or VILLAGE ? [new((scene == FARM ? 26 : 25) * 32, 512, (scene == FARM ? 6 : 7) * 32, 128), new(1408, 896, 384, 256)] : [];
    public static Rectangle[] interior_solids(Scene scene) => scene switch
    {
        HOME => [new(432, 96, 112, 128), new(112, 112, 96, 64), new(48, 48, 96, 64), new(512, 288, 64, 48)],
        SHOP => [new(224, 208, 256, 48), new(48, 96, 96, 256), new(560, 96, 96, 256)],
        _ => []
    };
    static readonly int[][] FarmForest = BuildForest(FARM), VillageForest = BuildForest(VILLAGE);
    static int[][] BuildForest(Scene scene) { var trees = (scene == FARM ? FARM_TREES : VILLAGE_TREES).ToList(); for (int i = 0; i < 150; i++) { int x = (96 + i * 239 % 1856) / 32 * 32 + 16, y = (64 + i * 337 % 1248) / 32 * 32 + 20; if (x < 1120 && y < 736 || y > 316 && y < 440 || Math.Abs(x - 432) < 80 || Math.Abs(x - 1056) < 64) continue; if (water_bodies(scene).Any(w => x > w.x - 70 && x < w.x + w.width + 70 && y > w.y - 20 && y < w.y + w.height + 90)) continue; trees.Add([(x - 16) / 32, (y - 20) / 32]); } return trees.ToArray(); }
    public static int[][] scene_trees(Scene scene) => scene == FARM ? FarmForest : scene == VILLAGE ? VillageForest : [];
    public static bool blocked(Scene scene, float x, float y)
    {
        Point size = world_size(scene); if (!float.IsFinite(x) || !float.IsFinite(y) || x < 16 || x > size.x - 16 || y < 32 || y > size.y - 24) return true;
        if (scene is HOME or SHOP) { if (x < 40 || x > size.x - 40 || y < 80 || y > size.y - 32) return true; return interior_solids(scene).Any(r => x >= r.x && x < r.x + r.width && y >= r.y && y < r.y + r.height); }
        int tx = (int)(x / 32), ty = (int)(y / 32); foreach (var b in scene == FARM ? FARM_BUILDINGS : VILLAGE_BUILDINGS) if (tx >= b.x && tx < b.x + b.w && ty >= b.y + 2 && ty < b.y + b.h) return true;
        if (water_bodies(scene).Any(r => x >= r.x && x < r.x + r.width && y >= r.y && y < r.y + r.height)) return true;
        if (scene == VILLAGE && tx >= 15 && tx <= 18 && ty >= 14 && ty <= 15) return true;
        return scene_trees(scene).Any(t => Math.Abs(x - (t[0] * 32 + 16)) < 14 && Math.Abs(y - (t[1] * 32 + 20)) < 13);
    }
    public static int find_path(Scene scene, Point start, Point goal, Point[] output)
    {
        if (blocked(scene, start.x, start.y) || blocked(scene, goal.x, goal.y)) return 0;
        Point size = world_size(scene); int cols = (int)size.x / 32, rows = (int)size.y / 32;
        int sx = (int)(start.x / 32), sy = (int)(start.y / 32), gx = (int)(goal.x / 32), gy = (int)(goal.y / 32);
        if (!In(sx, 0, cols - 1) || !In(gx, 0, cols - 1) || !In(sy, 0, rows - 1) || !In(gy, 0, rows - 1)) return 0;
        int[] parent = Enumerable.Repeat(-1, cols * rows).ToArray(); Queue<int> queue = new(); int first = sy * cols + sx, last = gy * cols + gx; parent[first] = first; queue.Enqueue(first);
        while (queue.TryDequeue(out int node)) { if (node == last) break; int x = node % cols, y = node / cols; foreach (var (dx, dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) { int nx = x + dx, ny = y + dy; if (!In(nx, 0, cols - 1) || !In(ny, 0, rows - 1)) continue; int next = ny * cols + nx; if (parent[next] >= 0 || blocked(scene, nx * 32 + 16, ny * 32 + 16)) continue; parent[next] = node; queue.Enqueue(next); } }
        if (parent[last] < 0) return 0; int count = 0; for (int n = last; n != first; n = parent[n]) { if (count >= output.Length) return 0; output[count++] = new(n % cols * 32 + 16, n / cols * 32 + 16); }
        Array.Reverse(output, 0, count); return count;
    }
}

using static ValeDoSol.Logic;
namespace ValeDoSol;

public static class SelfTests
{
    static int count;
    static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); count++; }
    public static int Run()
    {
        string dir = Path.Combine(Path.GetTempPath(), "vale-csharp-tests-" + Guid.NewGuid()); Directory.CreateDirectory(dir);
        try
        {
            State s = new(); Check(state_valid(s), "novo jogo válido"); farm_action(s, 5, 1, out bool ok); Check(ok && s.seeds[0] == 9, "plantio consome semente");
            for (int i = 0; i < 3; i++) { farm_action(s, 5, 2, out ok); Check(ok, "rega diária"); next_day(s); }
            Check(s.plots[5].growth == 3, "crescimento após três dias"); farm_action(s, 5, 3, out ok); Check(ok && s.inventory[0] == 1 && s.coins == 120, "colheita no inventário"); Check(sell_all(s) == 14 && s.coins == 134, "venda para Lia");
            s = new(); s.day = 3; s.plots[4].crop = 0; next_day(s); Check(s.plots[4].growth == 0 && s.plots[4].watered, "chuva sem crescimento retroativo"); next_day(s); Check(s.plots[4].growth == 1, "crescimento na chuva");
            s = new(); s.seeds[0] = 0; int energy = s.energy; farm_action(s, 5, 1, out ok); Check(!ok && s.energy == energy, "sem sementes não perde energia"); s.energy = 2; farm_action(s, 20, 0, out ok); Check(!ok && !s.plots[20].tilled, "sem energia não altera terreno"); s.coins = 0; Check(!buy_seeds(s, 0), "loja não aceita saldo negativo");
            s = new(); talk_to(s, 0); talk_to(s, 0); Check(s.friendship[0] == 1, "amizade uma vez por dia"); s.inventory[0] = 2; Check(give_gift(s, 0) && !give_gift(s, 0) && s.inventory[0] == 1, "presente consome vegetal uma vez ao dia"); talk_to(s, 2); Check(s.rod, "Iara entrega vara");
            foreach (int npc in new[] { 0, 1, 2, 3 }) talk_to(s, npc); Check(claim_quest(s, 0) && !claim_quest(s, 0), "missão recompensa uma vez"); s.inventory[0] = 3; Check(claim_quest(s, 2) && s.inventory[0] == 0, "entrega consome três cenouras"); s.wood = 8; s.coins = 100; Check(upgrade_can(s) && s.wood == 0 && s.coins == 40 && !upgrade_can(s), "melhoria do regador"); for (int i = 0; i < 48; i++) s.plots[i].tilled = true; farm_action(s, 5, 2, out ok); Check(ok && s.plots[4].watered && s.plots[6].watered && s.plots[13].watered && !s.plots[47].watered, "regador alcança vizinhos sem índices inválidos");
            Point[] path = new Point[MAX_PATH]; foreach (Scene scene in new[] { FARM, VILLAGE }) { int n = find_path(scene, scene == FARM ? new(464, 400) : new(80, 384), new(1040, 384), path); Check(n > 0 && path.Take(n).All(p => !blocked(scene, p.x, p.y)), "navegação respeita obstáculos"); }
            Check(find_path(FARM, new(464, 400), new(880, 560), path) == 0, "não caminha sobre o lago"); for (int i = 0; i < 48; i++) { var p = plot_position(i); Check(plot_at(p.x + 16, p.y + 16) == i, "índices dos canteiros"); }
            string save = Path.Combine(dir, "save.json"); Check(state_save(s, save) && state_load(out State loaded, save) && loaded.coins == s.coins && loaded.plots[4].watered, "save e carregamento completos"); File.WriteAllText(save, "{\"version\":3,\"seeds\":null}"); Check(!state_load(out _, save), "save inválido rejeitado"); File.WriteAllText(save, "{}"); Check(!state_load(out _, save), "save sem campos obrigatórios rejeitado"); File.WriteAllText(save, "not a save"); Check(!state_load(out _, save), "save truncado rejeitado"); s.player.x = float.NaN; Check(!state_valid(s) && !state_save(s, save), "coordenada inválida rejeitada");
            Console.WriteLine($"PASS: {count} verificações C# de cultivo, clima, loja, amizade, missões, navegação e saves."); return 0;
        }
        catch (Exception e) { Console.Error.WriteLine("FAIL: " + e.Message); return 1; }
        finally { Directory.Delete(dir, true); }
    }
}

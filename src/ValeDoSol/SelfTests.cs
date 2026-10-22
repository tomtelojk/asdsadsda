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
            State expanded = new(); expanded.player = new(1800, 1200); Check(state_valid(expanded), "save permite posição no mapa ampliado");
            foreach (Scene scene in new[] { FARM, VILLAGE, HOME, SHOP }) { Game cameraGame = new() { s = new State { scene = scene, player = scene is HOME ? new(320, 280) : scene is SHOP ? new(352, 280) : new(400, 288) } }; cameraGame.camera = GameLogic.camera_target(cameraGame.s); foreach (Point point in new[] { cameraGame.s.player, new Point(64, 96), new Point(512, 320) }) { Point projected = GameLogic.world_to_screen(cameraGame, point), restored = GameLogic.screen_to_world(cameraGame, projected); Check(Math.Abs(restored.x - point.x) < .001 && Math.Abs(restored.y - point.y) < .001, "câmera e clique usam transformações inversas"); } Check(state_valid(cameraGame.s), "save válido em cada área"); }
            Check(blocked(HOME, 480, 160) && !blocked(HOME, 416, 208), "colisão da cama e acesso ao lado"); Check(blocked(SHOP, 352, 224) && !blocked(SHOP, 352, 276), "balcão sólido e atendimento acessível");
            Check(find_path(FARM, new(432, 384), new(1968, 384), path) > 0, "estrada conecta fazenda ampliada à saída");
            Game farm = new() { s = new State { player = new(400, 288) }, tool = 1 }; int seed = farm.s.seeds[0]; GameLogic.farm_near(farm, 8); Check(farm.s.seeds[0] == seed && farm.s.energy == 100, "ferramentas não atuam longe do personagem"); farm.s.player = new(560, 304); GameLogic.farm_near(farm, 8); Check(farm.s.seeds[0] == seed - 1 && farm.s.plots[8].crop == 0, "plantio ao alcance do personagem");
            Game indoors = new() { s = new State { player = new(176, 246) } }; GameLogic.game_interact(indoors); Check(indoors.s.scene == HOME, "porta da casa leva ao interior"); indoors.s.player = new(416, 208); GameLogic.game_interact(indoors); Check(indoors.dialog == Dialog.D_REST, "cama permite descansar"); GameLogic.game_rest(indoors); Check(indoors.s.day == 2 && indoors.s.scene == HOME && indoors.s.energy == 100, "dormir começa o dia dentro de casa"); GameLogic.game_travel(indoors); Check(indoors.s.scene == FARM, "saída da casa retorna à fazenda");
            indoors.s.scene = VILLAGE; indoors.s.player = new(240, 278); GameLogic.game_interact(indoors); Check(indoors.s.scene == SHOP, "porta do armazém leva ao interior"); indoors.s.player = new(352, 276); GameLogic.game_interact(indoors); Check(indoors.dialog == Dialog.D_NPC && indoors.npc == 0, "Lia atende no balcão do armazém");
            string migrationDir = Path.Combine(dir, "migration"); Directory.CreateDirectory(migrationDir); string migrationSave = Path.Combine(migrationDir, "save.json"); state_save(new State { coins = 777 }, migrationSave); string original = File.ReadAllText(migrationSave).Replace("\"edition\": 4,", ""); File.WriteAllText(migrationSave, original); Game migrated = new(); GameLogic.game_init(migrated, migrationDir); Check(migrated.s.coins == 777 && migrated.save_ok, "edição anterior carregada sem perder moedas"); Check(File.ReadAllText(Path.Combine(migrationDir, "save-before-v4.json")) == original, "save anterior preservado em backup exato");
            string corruptDir = Path.Combine(dir, "corrupt"); Directory.CreateDirectory(corruptDir); File.WriteAllText(Path.Combine(corruptDir, "save.json"), "invalid"); Game corrupt = new(); GameLogic.game_init(corrupt, corruptDir); GameLogic.game_save(corrupt); Check(!corrupt.save_ok && File.ReadAllText(Path.Combine(corruptDir, "save.json")) == "invalid", "save corrompido não é sobrescrito");
            Console.WriteLine($"PASS: {count} verificações C# de cultivo, clima, loja, amizade, missões, navegação e saves."); return 0;
        }
        catch (Exception e) { Console.Error.WriteLine("FAIL: " + e.Message); return 1; }
        finally { Directory.Delete(dir, true); }
    }
}

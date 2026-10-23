using static ValeDoSol.Logic;
namespace ValeDoSol;

public static class SelfTests
{
    static int count;
    static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); count++; }
    public static int Run()
    {
        count = 0;
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
            foreach (Scene scene in Enum.GetValues<Scene>()) { Game cameraGame = new() { s = new State { scene = scene, player = scene is HOME ? new(320, 280) : scene is SHOP ? new(352, 280) : new(400, 288) } }; cameraGame.camera = GameLogic.camera_target(cameraGame.s); foreach (Point point in new[] { cameraGame.s.player, new Point(64, 96), new Point(512, 320) }) { Point projected = GameLogic.world_to_screen(cameraGame, point), restored = GameLogic.screen_to_world(cameraGame, projected); Check(Math.Abs(restored.x - point.x) < .001 && Math.Abs(restored.y - point.y) < .001, "câmera e clique usam transformações inversas"); } Check(state_valid(cameraGame.s), "save válido em cada área"); }
            Check(blocked(HOME, 480, 160) && !blocked(HOME, Map.BedApproach(HOME).x, Map.BedApproach(HOME).y), "colisão da cama e acesso ao lado"); Check(blocked(SHOP, 352, 224) && !blocked(SHOP, 352, 276), "balcão sólido e atendimento acessível");
            Check(find_path(FARM, new(432, 384), new(1968, 384), path) > 0, "estrada conecta fazenda ampliada à saída");
            Game farm = new() { s = new State { player = new(400, 288) }, tool = 1 }; int seed = farm.s.seeds[0]; GameLogic.farm_near(farm, 8); Check(farm.s.seeds[0] == seed && farm.s.energy == 100, "ferramentas não atuam longe do personagem"); farm.s.player = new(560, 304); GameLogic.farm_near(farm, 8); Check(farm.s.seeds[0] == seed - 1 && farm.s.plots[8].crop == 0, "plantio ao alcance do personagem");
            Game indoors = new() { s = new State { player = Map.FarmHouse.Door } }; GameLogic.game_interact(indoors); Check(indoors.s.scene == HOME, "porta da casa leva ao interior"); indoors.s.player = Map.BedApproach(HOME); GameLogic.game_interact(indoors); Check(indoors.dialog == Dialog.D_REST, "cama permite descansar"); GameLogic.game_rest(indoors); Check(indoors.s.day == 2 && indoors.s.scene == HOME && indoors.s.energy == 100, "dormir começa o dia dentro de casa"); GameLogic.game_travel(indoors); Check(indoors.s.scene == FARM, "saída da casa retorna à fazenda");
            GameLogic.reset_npcs(indoors); indoors.s.scene = VILLAGE; indoors.s.player = Map.PlaceFor(SHOP).Door; GameLogic.game_interact(indoors); Check(indoors.s.scene == SHOP, "porta do armazém leva ao interior"); indoors.s.player = Map.ServicePoint(SHOP); GameLogic.game_interact(indoors); Check(indoors.dialog == Dialog.D_NPC && indoors.npc == 0, "Lia atende no balcão do armazém");
            string migrationDir = Path.Combine(dir, "migration"); Directory.CreateDirectory(migrationDir); string migrationSave = Path.Combine(migrationDir, "save.json"); state_save(new State { coins = 777 }, migrationSave); string original = File.ReadAllText(migrationSave).Replace("\"edition\": 7,", ""); File.WriteAllText(migrationSave, original); Game migrated = new(); GameLogic.game_init(migrated, migrationDir); Check(migrated.s.coins == 777 && migrated.save_ok, "edição anterior carregada sem perder moedas"); Check(File.ReadAllText(Path.Combine(migrationDir, "save-before-v7.json")) == original, "save anterior preservado em backup exato");
            string corruptDir = Path.Combine(dir, "corrupt"); Directory.CreateDirectory(corruptDir); File.WriteAllText(Path.Combine(corruptDir, "save.json"), "invalid"); Game corrupt = new(); GameLogic.game_init(corrupt, corruptDir); GameLogic.game_save(corrupt); Check(!corrupt.save_ok && File.ReadAllText(Path.Combine(corruptDir, "save.json")) == "invalid", "save corrompido não é sobrescrito");

            // Published v4 saves have four social entries. Keep every original value.
            State old = new() { coins = 876, wood = 11, met = 9, friendship = [5, 2, 0, 3], talked_day = [1, 0, 0, 1], gifted_day = [0, 0, 0, 1] }; old.edition = 4;
            string legacyRaw = System.Text.Json.JsonSerializer.Serialize(old, SaveContext.Default.State); string legacyPath = Path.Combine(dir, "v4.json"); File.WriteAllText(legacyPath, legacyRaw);
            Check(state_load(out State upgraded, legacyPath) && upgraded.edition == 7 && upgraded.friendship.SequenceEqual(new[] { 5, 2, 0, 3, 0, 0, 0, 0 }) && upgraded.met == 9 && upgraded.coins == 876 && upgraded.wood == 11, "save v4 expande moradores preservando recursos e amizades");
            Check(upgraded.gifted_day[3] == 1 && upgraded.talked_day[0] == 1 && state_valid(upgraded), "migração preserva limites diários dos presentes e conversas");
            for (int i = 4; i < Social.Count; i++) { State social = new(); talk_to(social, i); social.inventory[1] = 1; Check((social.met & (1u << i)) != 0 && social.friendship[i] == 1 && give_gift(social, i) && social.friendship[i] == 2 && state_valid(social), "novos moradores têm amizade e presentes salvos"); }
            State quest = new() { met = 240 }; Check(!claim_quest(quest, 0), "novos moradores não concluem missão dos quatro moradores originais"); quest.met = 255; Check(claim_quest(quest, 0), "conhecer os oito moradores mantém a missão original disponível");
            foreach (Scene scene in new[] { FARM, VILLAGE })
            {
                foreach (int[] t in Map.Trees(scene)) { Rectangle canopy = new(t[0] * 32 + 16 - 48, t[1] * 32 + 16 - 104, 96, 116); Check(!Map.Roads(scene).Any(r => Map.Touch(canopy, Map.Inflated(r, 12))), "árvore e copa ficam fora de todas as estradas e acessos"); }
                foreach (Place place in Map.Places(scene)) { Check(!blocked(scene, place.Door.x, place.Door.y), "porta externa livre e alinhada ao desenho"); int length = find_path(scene, new(432, 384), place.Door, path); Check(length > 0 && path.Take(length).All(p => !blocked(scene, p.x, p.y)), "cada casa tem acesso pela estrada"); }
            }
            foreach (Scene room in Enum.GetValues<Scene>().Where(Map.Indoors))
            {
                Point entry = Map.Arrival(room), exit = Map.Exit(room); Check(!blocked(room, entry.x, entry.y) && !blocked(room, exit.x, exit.y) && find_path(room, entry, exit, path) > 0, "interior permite entrar e voltar à porta");
                foreach (Prop prop in Map.Props(room).Where(p => p.solid)) { Rectangle r = prop.bounds; Check(blocked(room, r.x + r.width / 2, r.y + r.height / 2), "móvel desenhado tem colisão na mesma posição"); Point approached = Physics.Nearest(room, new(r.x + r.width / 2, r.y + r.height + 20)); Check(find_path(room, entry, approached, path) > 0 || Physics.Distance(entry, approached) < 1, "móveis têm área de aproximação acessível"); }
                Game enter = new() { s = new() { scene = Map.Outside(room), player = Map.PlaceFor(room).Door } }; GameLogic.reset_npcs(enter); GameLogic.game_interact(enter); Check(enter.s.scene == room, "todas as portas levam ao interior correspondente"); GameLogic.game_travel(enter); Check(enter.s.scene == Map.Outside(room) && Physics.Distance(enter.s.player, Map.PlaceFor(room).Door) < 1, "saída retorna à porta correta");
            }
            Game overlap = new() { s = new State { player = new(400, 288) } }; overlap.npc_scene[0] = FARM; overlap.npc_position[0] = overlap.s.player; GameLogic.move_player(overlap, 5, 0); Check(overlap.s.player.x > 400, "jogador consegue se afastar de um morador sobreposto na chegada");
            Point rushed = Physics.Move(HOME, new(416, 160), new(240, 0)); Check(rushed.x <= Map.Props(HOME).First(p => p.kind == Furnishing.Bed).bounds.x - 6 && !blocked(HOME, rushed.x, rushed.y), "movimento rápido não atravessa a cama"); Point slide = Physics.Move(HOME, new(416, 160), new(160, 100)); Check(slide.y > 240 && !blocked(HOME, slide.x, slide.y), "movimento diagonal desliza pela borda do móvel");
            Point edge = Physics.Move(FARM, new(320, 300), new(-500, -140)); Check(!blocked(FARM, edge.x, edge.y) && edge.x >= 18, "paredes e limites respeitam a largura dos pés");
            string[] signatures = Enumerable.Range(0, Social.Count).Select(n => string.Join(";", Social.Schedule(n, new()).Select(a => $"{a.start}:{a.scene}:{a.at.x}:{a.at.y}"))).ToArray(); Check(signatures.Distinct().Count() == Social.Count, "os oito moradores têm agendas diferentes");
            foreach (int day in new[] { 1, 2, 4, 6, 7 }) for (int npc = 0; npc < Social.Count; npc++)
            {
                State calendar = new() { day = day }; var plan = Social.Schedule(npc, calendar); Check(plan.Length >= 5 && plan.Zip(plan.Skip(1)).All(pair => pair.First.start < pair.Second.start), "agenda pessoal tem vários horários ordenados");
                Scene here = plan[0].scene; Point previous = plan[0].at;
                foreach (Appointment a in plan)
                {
                    int hops = 0;
                    while (here != a.scene && hops++ < 5) { var portal = Map.Portal(here, a.scene); Check(Physics.Distance(previous, portal.at) < .1f || find_path(here, previous, portal.at, path) > 0, $"morador {npc}, dia {day}: trajeto até porta de {here} para {portal.next}"); here = portal.next; previous = portal.arrival; }
                    Check(here == a.scene && (Physics.Distance(previous, a.at) < .1f || find_path(here, previous, a.at, path) > 0), $"morador {npc}, dia {day}: compromisso {a.activity} acessível"); previous = a.at;
                }
            }
            Game routine = new(); GameLogic.reset_npcs(routine); float[] travel = new float[Social.Count]; HashSet<Scene>[] visited = Enumerable.Range(0, Social.Count).Select(_ => new HashSet<Scene>()).ToArray(); bool collisionFree = true, continuous = true;
            for (int frame = 0; frame < 6000; frame++)
            {
                if (frame % 8 == 0 && routine.s.minute < 1230) routine.s.minute++; routine.elapsed += .1f; Point[] before = routine.npc_position.ToArray(); Scene[] beforeScene = routine.npc_scene.ToArray(); GameLogic.update_npcs(routine, .1f);
                for (int npc = 0; npc < Social.Count; npc++) { visited[npc].Add(routine.npc_scene[npc]); collisionFree &= !blocked(routine.npc_scene[npc], routine.npc_position[npc].x, routine.npc_position[npc].y); if (beforeScene[npc] == routine.npc_scene[npc]) { float stepDistance = Physics.Distance(before[npc], routine.npc_position[npc]); continuous &= stepDistance < 18; travel[npc] += stepDistance; } }
            }
            Check(collisionFree && continuous, "dia simulado completo: sem colisões e sem teletransporte fora das portas");
            for (int npc = 0; npc < Social.Count; npc++) Check(travel[npc] > 350 && visited[npc].Count >= 2, $"{NPCS[npc].name} anda pelo mapa e visita mais de uma área");
            string positions = Path.Combine(dir, "positions.json"); Check(state_save(routine.s, positions), "posições e cenas dos moradores são salvas"); Check(state_load(out State persisted, positions), "posições dos moradores são carregadas"); Game resumed = new() { s = persisted }; Point[] exact = resumed.npc_position.ToArray(); Scene[] exactScenes = resumed.npc_scene.ToArray(); GameLogic.reset_npcs(resumed); Check(resumed.npc_position.Zip(exact).All(pair => Physics.Distance(pair.First, pair.Second) < .01f) && resumed.npc_scene.SequenceEqual(exactScenes), "reabrir partida não devolve moradores aos pontos iniciais");
            Game occupied = new() { s = new State { scene = VILLAGE, player = new(1120, 180) } }; GameLogic.safe_position(occupied); Check(!blocked(VILLAGE, occupied.s.player.x, occupied.s.player.y), "mapa atualizado não prende jogador do save anterior");
            for (int npc = 0; npc < Social.Count; npc++) { State social = new(); string one = Social.Greeting(social, npc, false), two = Social.Greeting(social, npc, false); Check(one != two && Social.Topic(social, npc, 0) != Social.Topic(social, npc, 1), "conversas variam e assuntos têm respostas próprias"); State wet = new() { day = 4 }; Check(Social.Greeting(wet, npc, false) != one, "diálogo reconhece chuva"); State close = new(); close.friendship[npc] = 4; close.conversations[npc] = 1; Check(Social.Greeting(close, npc, false) != two && Social.GiftReply(npc).Length > 40, "amizade e presentes têm falas pessoais"); }
            State remembered = new(); Social.Greeting(remembered, 7, false); Check(state_save(remembered, save) && state_load(out var recalled, save) && recalled.conversations[7] == 1, "variação de conversa persiste no save");
            Art.Canvas art = Art.CreateAtlas(); bool cleanGutters = true; for (int y = 0; y < art.Height; y++) for (int x = 0; x < art.Width; x++) if ((x % Art.Cell < 2 || x % Art.Cell >= 126 || y % Art.Cell < 2 || y % Art.Cell >= 126) && art.Pixels[(y * art.Width + x) * 4 + 3] != 0) cleanGutters = false; Check(cleanGutters, "atlas mantém margens transparentes em todas as células, sem vazamento de recortes");
            foreach (int cell in new[] { 0, 4, 8, 12, 16, 32, 64, 128, Art.Tree, Art.Crop + 8, Art.Portrait, Art.Portrait + 8 }) { bool visible = false; for (int y = 2; y < 126 && !visible; y++) for (int x = 2; x < 126; x++) if (art.Pixels[(((cell / Art.Columns) * Art.Cell + y) * art.Width + cell % Art.Columns * Art.Cell + x) * 4 + 3] != 0) visible = true; Check(visible, "direções do fazendeiro, moradores, cenário e retratos têm arte visível"); }
            for (int portrait = Art.Portrait; portrait < Art.Portrait + 9; portrait++)
            {
                bool baseClear = true;
                for (int y = 124; y < Art.Cell; y++) for (int x = 0; x < Art.Cell; x++)
                    baseClear &= art.Pixels[(((portrait / Art.Columns) * Art.Cell + y) * art.Width + portrait % Art.Columns * Art.Cell + x) * 4 + 3] == 0;
                Check(baseClear, "retratos mantêm os ombros inteiros antes da margem inferior do recorte");
            }
            StoryTests.Run(Check);
            RegressionTests.Run(Check);
            TextLayoutTests.Run(Check);
            Console.WriteLine($"PASS: {count} verificações C# de cultivo, história, rotinas, arte, física, interface e saves."); return 0;
        }
        catch (Exception e) { Console.Error.WriteLine("FAIL: " + e.Message); return 1; }
        finally { Directory.Delete(dir, true); }
    }
}

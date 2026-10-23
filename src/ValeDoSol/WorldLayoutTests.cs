using static ValeDoSol.Logic;
namespace ValeDoSol;

/// <summary>Reachability and collision regressions for the authored village and farm layout.</summary>
public static class WorldLayoutTests
{
    public static void Run(Action<bool, string> check)
    {
        CheckGarden(check);
        CheckVillage(check);
        CheckMigration(check);
    }

    static Point Center(Rectangle r) => new(r.x + r.width / 2, r.y + r.height / 2);
    static bool Contains(Rectangle outer, Rectangle inner) => inner.x >= outer.x && inner.y >= outer.y && inner.x + inner.width <= outer.x + outer.width && inner.y + inner.height <= outer.y + outer.height;

    static void CheckGarden(Action<bool, string> check)
    {
        Point[] route = new Point[MAX_PATH];
        check(Garden.Beds.Count == 8 && Garden.Beds.All(b => b.width == 64 && b.height == 96), "horta tem oito canteiros elevados com duas colunas e três linhas cada");
        check(Garden.Beds.All(b => Contains(Garden.Bounds, b)) && Garden.Beds.SelectMany((a, i) => Garden.Beds.Skip(i + 1).Select(b => Map.Touch(a, b))).All(touch => !touch), "canteiros cabem no jardim e não se sobrepõem");
        foreach (Rectangle bed in Garden.Beds)
        {
            Point middle = Center(bed);
            check(!Map.Roads(FARM).Any(road => Map.Touch(bed, road)) && !water_bodies(FARM).Any(water => Map.Touch(bed, water)), "canteiro de plantação fica completamente fora das estradas e da água");
            check(Physics.Blocked(FARM, middle.x, middle.y), "canteiro elevado impede atravessar as plantas pelo meio");
            Point beside = new(bed.x - 8, middle.y);
            Point rushed = Physics.Move(FARM, beside, new(bed.width + 40, 0));
            check(!Physics.Blocked(FARM, beside.x, beside.y) && !Physics.Blocked(FARM, rushed.x, rushed.y) && rushed.x <= bed.x - 6, "movimento rápido para na lateral física do canteiro");
        }
        foreach (Rectangle fence in Garden.Fences)
        {
            Point middle = Center(fence);
            bool horizontal = fence.width > fence.height;
            bool farEdge = horizontal ? fence.y > Garden.Bounds.y + Garden.Bounds.height / 2 : fence.x > Garden.Bounds.x + Garden.Bounds.width / 2;
            Point outside = horizontal ? new(middle.x, farEdge ? fence.y + fence.height + 12 : fence.y - 12) : new(farEdge ? fence.x + fence.width + 12 : fence.x - 12, middle.y);
            int direction = farEdge ? -1 : 1;
            Point rushed = Physics.Move(FARM, outside, horizontal ? new(0, direction * 48) : new(direction * 48, 0));
            bool remainedOutside = horizontal ? farEdge ? rushed.y >= fence.y + fence.height + 4 : rushed.y <= fence.y - 4 : farEdge ? rushed.x >= fence.x + fence.width + 6 : rushed.x <= fence.x - 6;
            check(Physics.Blocked(FARM, middle.x, middle.y) && !Physics.Blocked(FARM, outside.x, outside.y) && !Physics.Blocked(FARM, rushed.x, rushed.y) && remainedOutside, "cerca desenhada tem colisão e não permite atravessar com corrida");
        }
        foreach (Point gate in Garden.Gates)
        {
            Point outside, inside;
            if (gate.y < Garden.Bounds.y + 10) { outside = new(gate.x, gate.y - 24); inside = new(gate.x, gate.y + 24); }
            else if (gate.y > Garden.Bounds.y + Garden.Bounds.height - 10) { outside = new(gate.x, gate.y + 24); inside = new(gate.x, gate.y - 24); }
            else if (gate.x < Garden.Bounds.x + 10) { outside = new(gate.x - 24, gate.y); inside = new(gate.x + 24, gate.y); }
            else { outside = new(gate.x + 24, gate.y); inside = new(gate.x - 24, gate.y); }
            check(Physics.Clear(FARM, outside, inside) && Physics.Distance(Physics.Move(FARM, outside, new(inside.x - outside.x, inside.y - outside.y)), inside) < .1f, "portão real permite entrar e sair sem colisão invisível");
            check(Physics.FindPath(FARM, Map.FarmHouse.Door, inside, route) > 0, "entrada da horta é alcançável desde a casa");
        }
        foreach (int[] tree in Map.Trees(FARM))
        {
            Rectangle canopy = new(tree[0] * 32 + 16 - 48, tree[1] * 32 + 16 - 104, 96, 116);
            check(!Map.Touch(canopy, Garden.Bounds), "árvores e copas não invadem canteiros, cercas ou corredores da horta");
        }

        HashSet<(float x, float y)> positions = [];
        for (int index = 0; index < 48; index++)
        {
            Point tile = plot_position(index), center = new(tile.x + 16, tile.y + 16);
            check(positions.Add((tile.x, tile.y)) && plot_at(tile.x, tile.y) == index && plot_at(tile.x + 31.9f, tile.y + 31.9f) == index && Garden.Beds.Count(b => Contains(b, new(tile.x, tile.y, 32, 32))) == 1, "cada índice salvo corresponde exatamente a uma célula do novo canteiro");
            Point approached = Garden.Approach(index);
            int length = Physics.FindPath(FARM, Map.FarmHouse.Door, approached, route);
            check(Physics.Distance(approached, center) <= 76 && !Physics.Blocked(FARM, approached.x, approached.y) && length > 0 && SegmentsClear(FARM, Map.FarmHouse.Door, route.Take(length)), $"canteiro {index} é cultivável da lateral por um caminho que não atravessa as plantas");
            Game click = new() { s = new() { player = new(688, 816) } };
            GameLogic.walk_to(click, approached, new());
            check(click.path_length > 0 && Physics.Distance(click.path[click.path_length - 1], approached) < .001f && SegmentsClear(FARM, click.s.player, click.path.Take(click.path_length)), $"clique alcança o corredor lateral do canteiro {index} sem arredondar para uma planta sólida");
            Game game = new() { s = new State { player = approached, energy = 200, max_energy = 200 }, tool = 1, facing = index % 2 == 0 ? 2 : 1 };
            game.s.plots[index] = new Plot { tilled = true, crop = -1 };
            int seeds = game.s.seeds[0];
            GameLogic.use_tool(game);
            check(game.s.plots[index].crop == 0 && game.s.seeds[0] == seeds - 1 && game.s.energy == 197, $"semente é plantada no canteiro {index} sem precisar entrar na área sólida");
            for (int day = 0; day < CROPS[0].days; day++) { game.tool = 2; GameLogic.farm_near(game, index); next_day(game.s); }
            game.tool = 3; GameLogic.farm_near(game, index);
            check(game.s.inventory[0] == 1 && game.s.plots[index].crop == -1 && !Physics.Blocked(FARM, game.s.player.x, game.s.player.y), $"regar, crescer e colher o canteiro {index} continua possível da passagem");
        }
        foreach (Point corridor in new[] { new Point(592, 752), new Point(688, 816), new Point(784, 880), new Point(688, 696), new Point(688, 936) })
            check(plot_at(corridor.x, corridor.y) == -1 && !Physics.Blocked(FARM, corridor.x, corridor.y), "corredor da horta é caminhável e não aceita plantação");
        check(plot_at(float.NaN, 720) == -1 && plot_at(528, float.PositiveInfinity) == -1 && plot_at(-1, 720) == -1, "seleção de plantação rejeita coordenadas fora da horta");
        Game narrowClick = new() { s = new() { player = new(688, 816) } };
        Point exactClick = new(504.25f, 752.75f);
        GameLogic.walk_to(narrowClick, exactClick, new());
        check(narrowClick.path_length > 0 && Physics.Distance(narrowClick.path[narrowClick.path_length - 1], exactClick) < .001f && SegmentsClear(FARM, narrowClick.s.player, narrowClick.path.Take(narrowClick.path_length)), "clique preserva a posição exata numa passagem de dezesseis pixels");
        Game oldPosition = new() { s = new() { player = Center(Garden.Beds[0]) } };
        GameLogic.safe_position(oldPosition);
        check(!Physics.Blocked(FARM, oldPosition.s.player.x, oldPosition.s.player.y) && Physics.Distance(oldPosition.s.player, Center(Garden.Beds[0])) < 80, "posição antiga ocupada por um canteiro é corrigida para um corredor próximo");
    }

    static bool SegmentsClear(Scene scene, Point previous, IEnumerable<Point> points)
    {
        foreach (Point next in points) { if (!Physics.Clear(scene, previous, next)) return false; previous = next; }
        return true;
    }

    static void CheckVillage(Action<bool, string> check)
    {
        Point[] route = new Point[MAX_PATH];
        Place hall = Map.PlaceFor(Scene.TOWNHALL);
        check(hall.scene == Scene.TOWNHALL && Map.Village.Contains(hall) && Map.Indoors(Scene.TOWNHALL), "prefeitura é uma construção de verdade com interior próprio");
        check(!Physics.Blocked(VILLAGE, hall.Door.x, hall.Door.y) && Physics.FindPath(VILLAGE, new(80, 384), hall.Door, route) > 0, "estradas da vila conectam a entrada à prefeitura sem obstáculos");
        check(Map.Props(Scene.TOWNHALL).Any(p => p.kind == Furnishing.Table) && Map.Props(Scene.TOWNHALL).Any(p => p.kind == Furnishing.Counter) && Map.Props(Scene.TOWNHALL).Count(p => p.description.Length > 0) >= 4, "prefeitura possui mesa de reunião, atendimento e objetos com história");
        check(!Physics.Blocked(Scene.TOWNHALL, Map.NoticePoint.x, Map.NoticePoint.y) && Physics.FindPath(Scene.TOWNHALL, Map.Arrival(Scene.TOWNHALL), Map.NoticePoint, route) > 0, "quadro da prefeitura pode ser alcançado pelo interior sem atravessar móveis");
        foreach ((Scene scene, Point point) in new[] { (Scene.TOWNHALL, Map.NoticePoint), (VILLAGE, VillageProps.NoticePoint) })
        {
            Game board = new() { s = new State { scene = scene, player = point } };
            GameLogic.game_interact(board);
            check(board.dialog == Dialog.D_CIVIC && board.civic_page == 0 && board.map_scene == VILLAGE, "quadro próximo abre os avisos e locais da vila");
        }
        Game farBoard = new() { s = new() { scene = VILLAGE, player = new(VillageProps.NoticePoint.x, VillageProps.NoticePoint.y + 80) } };
        GameLogic.game_interact(farBoard);
        check(farBoard.dialog != Dialog.D_CIVIC, "interação do quadro não funciona de longe");
        Game farmMap = new() { path_length = 2, fishing = 1 };
        var beforeMap = (farmMap.s.player.x, farmMap.s.player.y, farmMap.s.minute, farmMap.s.coins, farmMap.s.story_stage);
        GameLogic.open_civic(farmMap, 1);
        check(farmMap.dialog == Dialog.D_CIVIC && farmMap.civic_page == 1 && farmMap.map_scene == FARM && farmMap.path_length == 0 && farmMap.fishing == 0 && beforeMap == (farmMap.s.player.x, farmMap.s.player.y, farmMap.s.minute, farmMap.s.coins, farmMap.s.story_stage), "mapa abre a área correta, pausa as ações e preserva progresso e posição");
        foreach (var prop in VillageProps.All)
        {
            Point middle = Center(prop.footprint);
            check(Contains(prop.visual, prop.footprint) && Physics.Blocked(VILLAGE, middle.x, middle.y), "objeto decorativo da vila usa uma base física dentro de seu desenho");
            check(!water_bodies(VILLAGE).Any(water => Map.Touch(prop.footprint, water)) && !Map.Places(VILLAGE).Any(place => Map.Touch(prop.footprint, Map.Footprint(place.building))), "bancos, postes e floreiras ficam fora das casas e da água");
            Point beside = Physics.Nearest(VILLAGE, new(middle.x, prop.footprint.y + prop.footprint.height + 16));
            check(Physics.FindPath(VILLAGE, new(80, 384), beside, route) > 0, "novos detalhes do cenário deixam uma área de aproximação acessível");
        }
        foreach (int[] tree in Map.Trees(VILLAGE))
        {
            Rectangle canopy = new(tree[0] * 32 + 16 - 48, tree[1] * 32 + 16 - 104, 96, 116);
            check(!Map.Places(VILLAGE).Any(place => Map.Touch(canopy, Map.VisualBounds(place.building))) && !VillageProps.Clearances.Any(clearance => Map.Touch(canopy, clearance)) && !Map.Touch(canopy, Map.CommunityBed), "copas deixam livres os desenhos completos das casas, torre da prefeitura e novos objetos da vila");
        }
        Point gardenCenter = Center(Map.CommunityBed);
        check(Physics.Blocked(VILLAGE, gardenCenter.x, gardenCenter.y), "canteiro comunitário da vila também possui física");
    }

    static void CheckMigration(Action<bool, string> check)
    {
        string file = Path.Combine(Path.GetTempPath(), "vale8-layout-" + Guid.NewGuid() + ".json");
        try
        {
            State old = new() { edition = 7, day = 12, coins = 931, wood = 12, seeds = [41, 12, 6], inventory = [7, 4, 2] };
            for (int i = 0; i < old.plots.Length; i++) old.plots[i] = new Plot { tilled = true, watered = i % 2 == 0, crop = i % 3, growth = i % 5 };
            File.WriteAllText(file, System.Text.Json.JsonSerializer.Serialize(old, SaveContext.Default.State));
            check(state_load(out State migrated, file) && migrated.edition == 8 && migrated.coins == 931 && migrated.inventory.SequenceEqual(old.inventory) && migrated.seeds.SequenceEqual(old.seeds) && migrated.plots.SequenceEqual(old.plots), "mover a horta preserva todas as 48 plantas, estágios e recursos dos saves da edição 7");
            State town = new() { scene = Scene.TOWNHALL, player = Map.Arrival(Scene.TOWNHALL), npc_saved = true };
            for (int i = 0; i < Social.Count; i++) { town.npc_scenes[i] = Scene.TOWNHALL; town.npc_positions[i] = Map.Arrival(Scene.TOWNHALL); }
            check(state_save(town, file) && state_load(out State returned, file) && returned.scene == Scene.TOWNHALL && returned.npc_scenes.All(scene => scene == Scene.TOWNHALL), "save preserva jogador e moradores no novo interior da prefeitura");
            old.edition = 9; File.WriteAllText(file, System.Text.Json.JsonSerializer.Serialize(old, SaveContext.Default.State));
            check(!state_load(out _, file), "edição futura desconhecida continua sendo rejeitada");
        }
        finally { File.Delete(file); }
    }
}

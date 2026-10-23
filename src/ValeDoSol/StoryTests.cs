using static ValeDoSol.Logic;
namespace ValeDoSol;

public static class StoryTests
{
    // The host test runner supplies its own counter/assertion reporting.
    public static void Run(Action<bool, string> check)
    {
        State s = new();
        check(Story.IsValid(s) && !Story.Complete(s), "história nova tem progresso válido e não começa concluída");
        for (int npc = 0; npc < NPCS.Length; npc++) talk_to(s, npc);
        int coins = s.coins;
        check(!Story.Current(s).Ready && !Story.TryAdvance(s, out _) && s.coins == coins && s.story_stage == 0, "conhecer os moradores não pula a leitura da carta");
        Story.ReadLetter(s); uint read = s.story_flags; Story.ReadLetter(s);
        check(s.story_flags == read && Story.Current(s).Ready, "reler a carta não concede progresso ou prêmios duplicados");
        check(Story.TryAdvance(s, out string arrival) && s.story_stage == 1 && s.coins == coins + 40 && arrival.Length > 40, "apresentações concluem o primeiro capítulo e dão a recompensa uma vez");

        // Actually grow and harvest the five starter carrots through farming rules.
        for (int i = 3; i < 5; i++) farm_action(s, i, 1, out _);
        for (int day = 0; day < 3; day++) { for (int i = 0; i < 5; i++) farm_action(s, i, 2, out _); next_day(s); }
        for (int i = 0; i < 5; i++) farm_action(s, i, 3, out _);
        int carrots = s.inventory[0];
        check(s.harvested == 5 && carrots == 5 && Story.Current(s).Ready, "cinco colheitas reais atendem ao capítulo da primeira página");
        check(Story.TryAdvance(s, out _) && s.story_stage == 2 && s.inventory[0] == carrots - 3, "Theo recebe exatamente três cenouras");

        s.wood = 12; s.stone = 6; int beforeEnergy = s.max_energy, beforeCoins = s.coins;
        check(Story.TryAdvance(s, out _) && s.story_stage == 3 && s.wood == 0 && s.stone == 0 && s.coins == beforeCoins - 80 && s.max_energy == beforeEnergy + 10, "reforma consome materiais e concede energia e sementes");
        check((s.story_flags & Story.GreenhouseRepaired) != 0 && Story.IsValid(s), "reforma tem um marco persistente para o cenário");

        for (int crop = 0; crop < 3; crop++) { s.selected = crop; farm_action(s, 8 + crop, 1, out _); }
        for (int npc = 0; npc < 3; npc++) talk_to(s, npc);
        check(Story.Current(s).Ready && Story.TryAdvance(s, out _) && s.story_stage == 4, "três culturas plantadas e amizades liberam o canteiro compartilhado");
        check((s.story_flags & Story.CommunityGarden) != 0, "canteiro comunitário tem seu próprio marco de progresso");

        for (int crop = 0; crop < 3; crop++) { s.selected = crop; farm_action(s, 11 + crop, 1, out _); }
        for (int day = 0; day < 4; day++) { for (int i = 8; i < 14; i++) farm_action(s, i, 2, out _); for (int npc = 0; npc < NPCS.Length; npc++) talk_to(s, npc); next_day(s); }
        for (int i = 8; i < 14; i++) farm_action(s, i, 3, out _);
        s.fish = 2; int[] food = s.inventory.ToArray();
        check(Story.Current(s).Ready && Story.TryAdvance(s, out _) && s.story_stage == 5 && s.fish == 0 && s.inventory.Where((n, i) => n != food[i] - 2).Any() == false, "preparativos entregam exatamente dois de cada vegetal e dois peixes");
        check((s.story_flags & Story.FestivalPrepared) != 0, "preparativos do encontro ficam registrados");
        s.scene = VILLAGE; s.minute = 1020;
        check(!Story.Current(s).Ready, "encontro não acontece no mesmo dia da entrega dos alimentos");
        next_day(s); s.minute = 1020;
        check(Story.Current(s).Ready && Story.TryAdvance(s, out string ending) && Story.Complete(s) && s.max_energy == 120 && ending.Length > 80, "campanha tem um desfecho alcançável após cultivo, amizade e um novo dia");
        var finished = (s.coins, s.max_energy, s.story_stage, s.story_flags); int[] finishedSeeds = s.seeds.ToArray();
        check(!Story.TryAdvance(s, out _) && finished == (s.coins, s.max_energy, s.story_stage, s.story_flags) && s.seeds.SequenceEqual(finishedSeeds), "repetir o desfecho não duplica moedas, sementes ou energia");
        check(state_valid(s) && Story.IsValid(s), "partida que concluiu a campanha mantém os limites do save");

        // Every gate is tested independently of the happy path.
        State gates = At(5); Array.Fill(gates.friendship, 2); gates.scene = VILLAGE; gates.minute = 1020;
        check(Story.Current(gates).Ready, "tarde do dia seguinte aceita o encontro");
        gates.story_day = gates.day; check(!Story.Current(gates).Ready, "encontro respeita o dia dos preparativos"); gates.story_day--;
        gates.minute = 1019; check(!Story.Current(gates).Ready, "encontro aguarda as 17h"); gates.minute = 1259; check(Story.Current(gates).Ready, "encontro permanece disponível antes das 21h"); gates.minute = 1260; check(!Story.Current(gates).Ready, "horário de encerramento do encontro é respeitado"); gates.minute = 1020;
        gates.scene = HOME; check(!Story.Current(gates).Ready, "encerramento precisa ocorrer na vila"); gates.scene = VILLAGE;
        gates.friendship[^1] = 1; check(!Story.Current(gates).Ready, "último morador também precisa ter sido convidado pela amizade");

        State missingFood = At(4); Array.Fill(missingFood.friendship, 3); missingFood.inventory = [2, 2, 1]; missingFood.fish = 2;
        var missingBefore = (missingFood.inventory[0], missingFood.inventory[1], missingFood.inventory[2], missingFood.fish, missingFood.coins, missingFood.story_flags);
        check(!Story.TryAdvance(missingFood, out _) && missingBefore == (missingFood.inventory[0], missingFood.inventory[1], missingFood.inventory[2], missingFood.fish, missingFood.coins, missingFood.story_flags), "entrega incompleta não consome parte dos alimentos");
        State noFriends = At(3); for (int crop = 0; crop < 3; crop++) noFriends.plots[crop].crop = crop;
        check(!Story.Current(noFriends).Ready, "plantas sozinhas não substituem o objetivo de amizade");
        Array.Fill(noFriends.friendship, 2); noFriends.plots[2].crop = 0;
        check(!Story.Current(noFriends).Ready, "três plantas da mesma espécie não contam como três culturas");

        State full = At(2); full.wood = 12; full.stone = 6; full.coins = 80; full.seeds[2] = 10_000_000;
        var materials = (full.wood, full.stone, full.coins, full.story_stage, full.story_flags);
        check(!Story.Current(full).Ready && !Story.TryAdvance(full, out _) && materials == (full.wood, full.stone, full.coins, full.story_stage, full.story_flags), "recompensa sem espaço não cobra materiais nem avança a história");
        State rich = new() { met = (1u << NPCS.Length) - 1, coins = 1_000_000_000 }; Story.ReadLetter(rich);
        check(!Story.TryAdvance(rich, out _) && rich.coins == 1_000_000_000 && rich.story_stage == 0, "recompensa não estoura o limite de moedas");
        rich.coins -= 40; check(Story.TryAdvance(rich, out _) && rich.coins == 1_000_000_000, "recompensa cabe exatamente no limite de moedas");
        State maxEnergy = At(2); maxEnergy.wood = 12; maxEnergy.stone = 6; maxEnergy.coins = 80; maxEnergy.max_energy = maxEnergy.energy = 200;
        check(Story.TryAdvance(maxEnergy, out _) && maxEnergy.max_energy == 200 && maxEnergy.energy == 200, "bônus de energia respeita o máximo já alcançado");

        State invalid = At(2); invalid.story_flags &= ~1u; check(!Story.IsValid(invalid) && !Story.Current(invalid).Ready && !Story.TryAdvance(invalid, out _), "capítulo sem os marcos anteriores é rejeitado");
        invalid = At(1); invalid.story_flags |= 128u; check(!Story.IsValid(invalid), "marco desconhecido no save é rejeitado");
        invalid = At(1); invalid.story_flags &= ~Story.LetterRead; check(!Story.IsValid(invalid), "capítulo avançado não perde o registro da carta");
        invalid = At(1); invalid.story_day = invalid.day + 1; check(!Story.IsValid(invalid), "data futura de capítulo é rejeitada");
        invalid = new() { story_stage = Story.ChapterCount + 1 }; check(!Story.IsValid(invalid), "capítulo fora do intervalo é rejeitado");

        for (int chapter = 0; chapter <= Story.ChapterCount; chapter++)
        {
            State cast = At(chapter); string[] lines = Enumerable.Range(0, NPCS.Length).Select(npc => Story.NpcLine(cast, npc)).ToArray();
            check(lines.Distinct().Count() == NPCS.Length && lines.All(line => line.Length is > 30 and < 260), "cada morador tem uma fala própria e legível em cada fase");
        }
        for (int npc = 0; npc < NPCS.Length; npc++) check(Enumerable.Range(0, Story.ChapterCount + 1).Select(chapter => Story.NpcLine(At(chapter), npc)).Distinct().Count() == Story.ChapterCount + 1, "morador reconhece cada avanço da história");
        check(Story.NpcLine(s, -1) == "" && Story.NpcLine(s, NPCS.Length) == "", "índices inválidos de morador não quebram a conversa");

        string path = Path.Combine(Path.GetTempPath(), "vale-story-" + Guid.NewGuid() + ".json");
        try
        {
            check(state_save(s, path), "campanha concluída pode ser salva");
            check(state_load(out State restored, path) && Story.IsValid(restored) && restored.story_stage == s.story_stage && restored.story_flags == s.story_flags && restored.story_day == s.story_day, "save restaura capítulo, carta e dia do encontro");
        }
        finally { File.Delete(path); }
    }
    static State At(int chapter) => new() { day = 10, story_stage = chapter, story_day = chapter == 0 ? 0 : 9, story_flags = ((1u << chapter) - 1) | Story.LetterRead };
}

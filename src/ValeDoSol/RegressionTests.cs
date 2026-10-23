using static ValeDoSol.Logic;
namespace ValeDoSol;

/// <summary>Pure logic regressions; these checks do not initialize a window or call Raylib.</summary>
public static class RegressionTests
{
    public static void Run(Action<bool, string> check)
    {
        Point start = new(432, 384), goal = new(464, 384);
        check(Physics.FindPath(FARM, start, goal, []) == 0,
            "buffer de navegação vazio retorna zero sem escrever fora dos limites");
        Point[] direct = new Point[1];
        check(Physics.FindPath(FARM, start, goal, direct) == 1 && Physics.Distance(direct[0], goal) < .001f,
            "buffer com uma posição aceita um trajeto direto");

        // Approach actual raised beds from their walkable aisles. Each keyboard
        // direction must use the tile in front, independent of a distant hover.
        int[] targets = [24, 9, 10, 16];
        Point[] positions = [new(528, 816), new(592, 752), new(592, 752), new(528, 816)];
        for (int facing = 0; facing < positions.Length; facing++)
        {
            int target = targets[facing];
            Game farm = new() { tool = 1, hover = 47, facing = facing };
            farm.s.player = positions[facing]; farm.s.plots[target].tilled = true;
            int seeds = farm.s.seeds[0], energy = farm.s.energy;
            GameLogic.use_tool(farm);
            check(!Physics.Blocked(FARM, farm.s.player.x, farm.s.player.y) && farm.s.plots[target].crop == 0 && farm.s.seeds[0] == seeds - 1 && farm.s.energy == energy - 3 && farm.s.plots[47].crop == -1,
                $"ferramenta pelo teclado alcança o canteiro à frente na direção {facing}, independentemente do mouse");
        }

        CheckStationaryPlayerCollision(check);
        CheckSaleLimits(check);
        CheckPresentation(check);
        CheckPortraits(check);
        Game doorway = new();
        doorway.s.scene = VILLAGE; doorway.s.player = new(848, 328);
        doorway.npc_scene[1] = VILLAGE; doorway.npc_position[1] = new(832, 328);
        GameLogic.game_interact(doorway);
        check(doorway.s.scene == Scene.WORKSHOP, "morador passando pela soleira não impede entrar na casa");

        State invalid = new() { conversations = new int[Social.Count - 1] };
        check(!state_valid(invalid), "save com memória de conversas incompleta é rejeitado");
        invalid = new() { npc_positions = new Point[Social.Count - 1] };
        check(!state_valid(invalid), "save com posições incompletas dos moradores é rejeitado");
        invalid = new() { npc_saved = true };
        invalid.npc_positions = Enumerable.Repeat(new Point(352, 192), Social.Count).ToArray();
        invalid.npc_positions[0] = new(float.NaN, 192);
        check(!state_valid(invalid), "save com posição não finita de morador é rejeitado");
    }

    static void CheckStationaryPlayerCollision(Action<bool, string> check)
    {
        Game village = new();
        village.s.scene = VILLAGE;
        village.s.player = new(432, 384);
        for (int i = 0; i < Social.Count; i++)
        {
            village.npc_scene[i] = SHOP;
            village.npc_position[i] = new(352, 192);
            village.npc_phase[i] = Social.Phase(i, village.s);
            village.npc_idle[i] = 100;
        }
        const int walker = 1;
        Point initial = new(408, 384);
        village.npc_scene[walker] = VILLAGE;
        village.npc_position[walker] = initial;
        village.npc_paths[walker][0] = new(472, 384);
        village.npc_length[walker] = 1;
        float closest = Physics.Distance(initial, village.s.player);
        bool walkable = true;
        for (int frame = 0; frame < 30; frame++)
        {
            village.elapsed += .05f;
            GameLogic.update_npcs(village, .05f);
            Point feet = village.npc_position[walker];
            closest = Math.Min(closest, Physics.Distance(feet, village.s.player));
            walkable &= !Physics.Blocked(village.npc_scene[walker], feet.x, feet.y);
        }
        check(closest >= 16.99f && Physics.Distance(village.s.player, new(432, 384)) < .001f,
            "morador respeita os pés do jogador parado sem deslocá-lo");
        check(walkable && Physics.Distance(initial, village.npc_position[walker]) > 5,
            "morador desvia do jogador sem ficar congelado ou atravessar cenário");
    }


    static void CheckPresentation(Action<bool, string> check)
    {
        check(SpriteCatalog.ValidSources(), "todos os recortes de sprites completos ficam dentro das texturas originais");
        foreach ((string texture, var size) in SpriteCatalog.TextureSizes)
        {
            using var stream = typeof(SpriteCatalog).Assembly.GetManifestResourceStream("ValeDoSol.Lpc." + texture + ".png");
            byte[] header = new byte[24]; int read = stream?.Read(header) ?? 0;
            check(read == 24 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }) &&
                System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(16, 4)) == size.Width &&
                System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(header.AsSpan(20, 4)) == size.Height,
                $"dimensões auditadas da textura {texture} correspondem ao PNG realmente distribuído");
        }
        foreach ((string name, SpriteDefinition sprite) in SpriteCatalog.Sprites)
        {
            Rectangle area = new(13, 29, sprite.Source.width * 2 + 9, sprite.Source.height * 2 + 19);
            Rectangle fitted = SpriteCatalog.PreserveAspect(area, sprite);
            check(Math.Abs(fitted.width / sprite.Source.width - 2) < .001f && Math.Abs(fitted.height / sprite.Source.height - 2) < .001f &&
                  fitted.x >= area.x && fitted.y >= area.y && fitted.x + fitted.width <= area.x + area.width && Math.Abs(fitted.y + fitted.height - area.y - area.height) < .001f,
                $"sprite {name} mantém proporção, escala inteira e base sem cortar partes da imagem");
            Point foot = new(176, 272); Rectangle atFoot = SpriteCatalog.AtFoot(foot, sprite, 2);
            check(Math.Abs(atFoot.x + sprite.FootX * 2 - foot.x) < .001f && Math.Abs(atFoot.y + sprite.FootY * 2 - foot.y) < .001f,
                $"sprite {name} conserva a mesma âncora dos pés ao ampliar");
        }
        SpriteDefinition bed = SpriteCatalog.Get("bed");
        Rectangle small = SpriteCatalog.PreserveAspect(new(0, 0, 24, 81), bed);
        check(Math.Abs(small.width - 24) < .001f && Math.Abs(small.height - 40.5f) < .001f,
            "área menor reduz uniformemente em vez de comprimir apenas a largura");
        foreach ((int width, int height, float sharpScale) in new[] { (1280, 720, 1f), (1920, 1080, 1f), (2560, 1440, 2f), (1921, 1081, 1f), (960, 540, .75f) })
        {
            Viewport viewport = Viewport.Fit(width, height, true);
            check(Math.Abs(viewport.Scale - sharpScale) < .001f && viewport.X >= 0 && viewport.Y >= 0 && viewport.Bounds.width + viewport.X <= width && viewport.Bounds.height + viewport.Y <= height,
                $"janela {width}x{height} respeita os pixels e os limites da imagem");
            foreach (Point logical in new[] { new Point(0, 0), new Point(640, 360), new Point(1279, 719) })
                check(Physics.Distance(logical, viewport.ToLogical(viewport.ToWindow(logical))) < .001f,
                    "renderização e ponteiro usam a mesma transformação de janela");
        }
        check(Math.Abs(Viewport.Fit(1920, 1080, false).Scale - 1.5f) < .001f,
            "modo preencher permite escala fracionária escolhida pelo jogador");
        Game camera = new() { camera = new(640.13, 360.88) }; Point snapped = GameLogic.render_camera(camera);
        check(Math.Abs(snapped.x * GameLogic.CAMERA_ZOOM - Math.Round(snapped.x * GameLogic.CAMERA_ZOOM)) < .001 &&
              Math.Abs(snapped.y * GameLogic.CAMERA_ZOOM - Math.Round(snapped.y * GameLogic.CAMERA_ZOOM)) < .001,
            "câmera de desenho alinha cada movimento à grade de pixels");
        FixedStepClock fast = new(), slow = new(), invalid = new(); int fastSteps = 0, slowSteps = 0;
        for (int frame = 0; frame < 600; frame++) fastSteps += fast.Advance(1.0 / 60);
        for (int frame = 0; frame < 200; frame++) slowSteps += slow.Advance(1.0 / 20);
        check(fastSteps == 600 && slowSteps == fastSteps,
            "dez segundos a vinte ou sessenta quadros avançam o mesmo tempo de simulação");
        check(invalid.Advance(double.NaN) == 0 && invalid.Advance(-1) == 0 && invalid.Advance(10) == FixedStepClock.MaxSteps,
            "relógio rejeita tempo inválido e limita trabalho acumulado após uma pausa do sistema");
    }

    static void CheckPortraits(Action<bool, string> check)
    {
        check(Portraits.Count == Social.Count && Portraits.Definitions.Count == Social.Count, "cada morador possui um retrato próprio na edição distribuída");
        HashSet<string> portraits = [];
        for (int npc = 0; npc < Social.Count; npc++)
        {
            PortraitDefinition portrait = Portraits.Definitions[npc];
            using var stream = typeof(Portraits).Assembly.GetManifestResourceStream(portrait.Resource);
            using MemoryStream data = new(); stream?.CopyTo(data); byte[] bytes = data.ToArray();
            bool png = bytes.Length >= 24 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
            check(png && portrait.Name == NPCS[npc].name && System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(16, 4)) == portrait.Width && System.Buffers.Binary.BinaryPrimitives.ReadInt32BigEndian(bytes.AsSpan(20, 4)) == portrait.Height,
                $"retrato de {NPCS[npc].name} está embutido com dimensões iguais ao arquivo PNG original");
            check(portraits.Add(Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))) && portrait.Source.x == 0 && portrait.Source.y == 0 && portrait.Source.width == portrait.Width && portrait.Source.height == portrait.Height,
                $"retrato de {NPCS[npc].name} usa uma imagem distinta e completa, sem recorte de cabelo ou ombros");
            foreach (Rectangle area in new[] { new Rectangle(13, 29, 136, 136), new Rectangle(17, 41, 136, 88), new Rectangle(23, 31, 68, 102) })
            {
                Rectangle fit = Portraits.Fit(npc, area);
                check(fit.x >= area.x && fit.y >= area.y && fit.x + fit.width <= area.x + area.width + .001f && Math.Abs(fit.y + fit.height - area.y - area.height) < .001f && Math.Abs(fit.width / portrait.Width - fit.height / portrait.Height) < .00001f,
                    "retrato mantém a proporção e a imagem inteira em diálogos, mochila e moradores");
            }
        }
        using var credits = typeof(Portraits).Assembly.GetManifestResourceStream("ValeDoSol.CharacterCredits.txt");
        check(credits is not null && credits.Length > 80, "créditos e procedência dos retratos acompanham o executável");
    }

    static void CheckSaleLimits(Action<bool, string> check)
    {
        State full = new() { coins = 999_999_999, inventory = [1, 2, 3], fish = 2 };
        int[] inventory = full.inventory.ToArray();
        int result = sell_all(full);
        check(result == -1 && full.coins == 999_999_999 && full.fish == 2 && full.inventory.SequenceEqual(inventory),
            "venda acima do limite preserva moedas, peixes e toda a colheita");
        State exact = new() { coins = 1_000_000_000 - CROPS[0].sell, inventory = [1, 0, 0] };
        check(sell_all(exact) == CROPS[0].sell && exact.coins == 1_000_000_000 && exact.inventory.All(n => n == 0),
            "venda que chega exatamente ao limite entrega o valor e consome os produtos");
    }
}

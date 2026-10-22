using System.Globalization;
using System.Text.RegularExpressions;
using static ValeDoSol.Ray;
using static ValeDoSol.Logic;
using static ValeDoSol.World;
using static ValeDoSol.GameLogic;
using static ValeDoSol.Dialog;
namespace ValeDoSol;

public static class Ui
{
    static string Fmt(string fmt, params object[] args) { int i = 0; return Regex.Replace(fmt, @"%0?(\d*)([ds])", m => { string value = Convert.ToString(args[i++], CultureInfo.InvariantCulture) ?? ""; return m.Groups[1].Value.Length > 0 ? value.PadLeft(int.Parse(m.Groups[1].Value), '0') : value; }); }
    static Color ink() { return color_hex(0x536348); }
    static void text(string s, int x, int y, int size, long color) { DrawText(s, x, y, size, color_hex(color)); }
    static void panel(Rectangle rect, long color) { DrawRectangleRounded(rect, .025f, 4, color_hex(color)); }
    static bool hit(Game g, Rectangle r) { return CheckCollisionPointRec(new Point(g.mouse.x, g.mouse.y), r); }
    static bool button(Game g, Rectangle r, string s, bool enabled, bool primary)
    {
        Color bg = color_hex(primary ? 0x69815a : 0xe8eddc), fg = primary ? color_hex(0xfcf4d8) : ink();
        if (enabled && hit(g, r)) { bg = color_hex(primary ? 0x5a724d : 0xdfe7d0); SetMouseCursor(MOUSE_CURSOR_POINTING_HAND); }
        if (!enabled) { fg = ColorAlpha(fg, .45f); bg = ColorAlpha(bg, .65f); }
        DrawRectangleRounded(r, .13f, 4, bg); DrawRectangleRoundedLinesEx(r, .13f, 4, 1, color_hex(primary ? 0x647d55 : 0xd2ddc2));
        int size = 16, w = MeasureText(s, size); DrawText(s, (int)(r.x + (r.width - w) / 2), (int)(r.y + (r.height - size) / 2), size, fg);
        return enabled && hit(g, r) && IsMouseButtonReleased(MOUSE_BUTTON_LEFT);
    }
    static int wrap(string s, int x, int y, int width, int size, int line, long color)
    {
        foreach (string paragraph in s.Split('\n')) { string current = ""; foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries)) { string candidate = current.Length == 0 ? word : current + " " + word; if (current.Length > 0 && MeasureText(candidate, size) > width) { text(current, x, y, size, color); y += line; current = word; } else current = candidate; } if (current.Length > 0) { text(current, x, y, size, color); y += line; } }
        return y;
    }
    static void flower(int x, int y, double scale) { Color c = color_hex(0xe7c582); for (int i = 0; i < 8; i++) { double a = i * 3.14159265f / 4; DrawRectangle((int)(x + Math.Cos(a) * 12 * scale - 3 * scale), (int)(y + Math.Sin(a) * 12 * scale - 3 * scale), (int)(6 * scale), (int)(6 * scale), c); } DrawCircle(x, y, 8 * scale, c); DrawCircle(x, y, 4 * scale, color_hex(0x526b47)); }
    static void heart(int x, int y, bool full) { Color c = color_hex(full ? 0xc48f88 : 0xd9ddce); DrawRectangle(x, y + 2, 5, 5, c); DrawRectangle(x + 7, y + 2, 5, 5, c); DrawRectangle(x + 2, y + 6, 9, 4, c); DrawRectangle(x + 5, y + 10, 3, 3, c); }
    public static void tool_icon(int tool, double x, double y, double scale)
    {
        Camera2D camera = new() { offset = new(x, y), target = new(0, 0), rotation = 0, zoom = (float)scale }; BeginMode2D(camera);
        if (tool == 0) { DrawLineEx(new Point(5, 28), new Point(24, 5), 5, color_hex(0xa78153)); DrawRectanglePro(new Rectangle(25, 6, 21, 7), new Point(10, 3), 30, color_hex(0x8b9d91)); }
        else if (tool == 1) { DrawRectangle(14, 8, 4, 24, color_hex(0x7c995b)); DrawEllipse(8, 13, 8, 4, color_hex(0xa4b96f)); DrawEllipse(23, 8, 8, 4, color_hex(0x8fac5e)); DrawCircle(18, 31, 5, color_hex(0xc8ae77)); }
        else if (tool == 2) { DrawRectangle(7, 13, 20, 18, color_hex(0x8ca8a0)); DrawRectangle(9, 10, 16, 4, color_hex(0xa4bfb3)); DrawRectangle(1, 15, 8, 4, color_hex(0x77988c)); DrawLineEx(new Point(27, 18), new Point(34, 9), 4, color_hex(0x7d9e94)); DrawRectangle(31, 7, 7, 4, color_hex(0xacc5bb)); DrawRectangleLines(10, 4, 13, 8, color_hex(0x7d9e94)); }
        else if (tool == 3) { DrawRectangle(4, 15, 28, 17, color_hex(0xc39a62)); DrawRectangle(2, 12, 32, 5, color_hex(0xdbb780)); DrawRectangleLines(9, 3, 18, 13, color_hex(0xa48051)); for (int i = 0; i < 4; i++) DrawRectangle(8 + i * 6, 17, 2, 13, color_hex(0xa88251)); }
        else { DrawRectangle(7, 7, 8, 21, color_hex(0xc9ae78)); DrawRectangle(20, 7, 8, 21, color_hex(0xc9ae78)); DrawRectangle(6, 26, 13, 7, color_hex(0x8f9a79)); DrawRectangle(19, 26, 13, 7, color_hex(0x8f9a79)); }
        EndMode2D();
    }
    static void crop_icon(int which, int x, int y)
    {
        Color leaf = color_hex(0x8da56a); DrawRectangle(x + 14, y + 1, 3, 14, leaf); DrawRectangle(x + 5, y + 3, 11, 4, leaf); DrawRectangle(x + 17, y, 10, 4, leaf);
        if (which == 0) DrawTriangle(new Point(x + 6, y + 12), new Point(x + 16, y + 34), new Point(x + 27, y + 12), color_hex(0xe9a05a));
        else if (which == 1) { DrawCircle(x + 16, y + 21, 11, color_hex(0xd4cfae)); DrawTriangle(new Point(x + 12, y + 27), new Point(x + 16, y + 36), new Point(x + 20, y + 27), color_hex(0xd4cfae)); }
        else { DrawCircle(x + 16, y + 20, 10, color_hex(0xd58a8c)); DrawRectangle(x + 12, y + 16, 2, 3, color_hex(0xf7dfad)); DrawRectangle(x + 20, y + 20, 2, 3, color_hex(0xf7dfad)); }
    }
    static void backup(Game g, bool restore)
    {
        string path = Path.Combine(g.save_dir, "backup.json");
        if (!restore) { game_toast(g, state_save(g.s, path) ? "Backup criado na pasta de saves: backup.json." : "Não foi possível criar o backup."); return; }
        if (!state_load(out State s, path)) { game_toast(g, "Não foi encontrado um backup válido na pasta de saves."); return; }
        g.s = s; g.path_length = 0; g.pending = new(); g.fishing = 0; for (int i = 0; i < 4; i++) { g.npc_phase[i] = s.minute >= 780 ? 1 : 0; g.npc_position[i] = s.minute >= 780 ? NPCS[i].afternoon : NPCS[i].morning; g.npc_length[i] = g.npc_at[i] = 0; }
        game_save(g); game_toast(g, "Backup restaurado. Bem-vindo de volta!");
    }
    static void sidebar(Game g)
    {
        bool active = g.dialog == D_NONE; State s = g.s; Rectangle side = new(1132, 72, 284, 862); panel(side, 0xf4f2e6);
        string[] tabs = ["Diário", "Mochila", "Vila"]; for (int i = 0; i < 3; i++) { Rectangle b = new(1146 + i * 83, 86, 81, 37); bool selected = g.tab == i; if (button(g, b, tabs[i], active, selected)) g.tab = i; }
        int x = 1154; string buf = "";
        if (g.tab == 0)
        {
            text("SEU DIÁRIO", x, 155, 13, 0x99a184); text("Pequenas", x, 186, 30, 0x536348); text("conquistas.", x, 219, 30, 0x536348); text("Um dia de cada vez.", x, 267, 15, 0x929a80);
            string[] names = ["Rostos do vale", "O fruto do trabalho", "A pesquisa de Theo"]; string[] desc = ["Conheça os quatro moradores.", "Colha 5 vegetais na fazenda.", "Entregue 3 cenouras a Theo."]; int[] counts = [0, s.harvested, s.inventory[0]]; int[] targets = [4, 5, 3]; int[] rewards = [40, 60, 75]; for (int i = 0; i < 4; i++) if ((s.met & (1u << i)) != 0) counts[0]++;
            for (int i = 0; i < 3; i++)
            {
                int y = 318 + i * 135; bool done = (s.claimed & (1u << i)) != 0; DrawLine(x, y - 16, 1394, y - 16, color_hex(0xe0e4d2)); text(names[i], x, y, 18, done ? 0x94a178 : 0x536348); wrap(desc[i], x, y + 30, 237, 14, 19, 0x929d80); DrawRectangle(x, y + 59, 239, 4, color_hex(0xe0e5d4)); double amount = Math.Min(1, (float)counts[i] / targets[i]); DrawRectangle(x, y + 59, (int)(239 * amount), 4, color_hex(0x9daf82)); buf = Fmt("%d/%d  +%d G", counts[i] > targets[i] ? targets[i] : counts[i], targets[i], rewards[i]); text(buf, x, y + 78, 13, 0x9d8b58);
                if (done) text("Concluída", 1310, y + 79, 13, 0x94a178); else if (i == 2) text("Fale com Theo", 1282, y + 79, 13, 0x9aa284); else if (button(g, new Rectangle(1293, y + 71, 101, 32), "Resgatar", active && counts[i] >= targets[i], false)) { if (claim_quest(s, i)) { game_toast(g, "Missão concluída! A vila agradece."); game_save(g); } }
            }
            panel(new Rectangle(x, 746, 239, 82), 0xe8eddb); wrap("Prepare, plante, regue e descanse. Cada pequena ação faz o vale crescer.", x + 13, 759, 212, 14, 20, 0x859570);
        }
        else if (g.tab == 1)
        {
            text("MOCHILA", x, 155, 13, 0x99a184); text("Tudo que", x, 186, 30, 0x536348); text("você cultiva.", x, 219, 30, 0x536348);
            for (int i = 0; i < 3; i++) { int y = 283 + i * 91; DrawLine(x, y - 11, 1394, y - 11, color_hex(0xe0e4d2)); crop_icon(i, x, y + 4); text(CROPS[i].name, x + 47, y + 4, 18, 0x536348); buf = Fmt("%d colhidos | %d sementes", s.inventory[i], s.seeds[i]); text(buf, x + 47, y + 30, 12, 0x94a083); if (button(g, new Rectangle(1287, y + 51, 107, 27), s.selected == i ? "Selecionado" : "Plantar", active, false)) { s.selected = i; g.tool = 1; game_save(g); } }
            string[] names = ["Madeira", "Pedra", "Peixes"]; int[] counts = [s.wood, s.stone, s.fish]; for (int i = 0; i < 3; i++) { int y = 578 + i * 49; DrawLine(x, y - 8, 1394, y - 8, color_hex(0xe0e4d2)); text(names[i], x, y + 4, 17, 0x68795a); buf = Fmt("%d", counts[i]); text(buf, 1350, y + 4, 18, 0x8b9c73); }
            panel(new Rectangle(x, 746, 239, 82), 0xe8eddb); text(s.watering_level == 2 ? "Regador de cobre" : "Regador simples", x + 13, 760, 16, 0x6d8056); wrap(s.rod ? "Vara de pesca da Iara: F perto do lago." : "Converse com Iara para ganhar uma vara de pesca.", x + 13, 787, 212, 13, 18, 0x859570);
        }
        else
        {
            text("VILA DAS FLORES", x, 155, 13, 0x99a184); text("Gente que", x, 186, 30, 0x536348); text("vira casa.", x, 219, 30, 0x536348); text("Amizades crescem todo dia.", x, 267, 14, 0x929a80);
            for (int i = 0; i < 4; i++) { int y = 324 + i * 96; DrawLine(x, y - 16, 1394, y - 16, color_hex(0xe0e4d2)); panel(new Rectangle(x, y, 37, 45), NPCS[i].coat); text((s.met & (1u << i)) != 0 ? NPCS[i].name : "???", x + 51, y, 19, 0x536348); text(NPCS[i].role, x + 51, y + 26, 13, 0x94a083); for (int k = 0; k < 5; k++) heart(x + 51 + k * 21, y + 49, k < s.friendship[i]); }
            panel(new Rectangle(x, 746, 239, 82), 0xe8eddb); wrap("Converse e dê um vegetal de presente. Os moradores passeiam pela vila à tarde.", x + 13, 759, 212, 14, 20, 0x859570);
        }
        DrawLine(x, 852, 1394, 852, color_hex(0xe0e4d2)); if (button(g, new Rectangle(x, 871, 116, 34), "Criar backup", active, false)) backup(g, false); if (button(g, new Rectangle(1280, 871, 114, 34), "Restaurar", active, false)) backup(g, true);
    }
    static void dialogs(Game g)
    {
        if (g.dialog == D_NONE) return; DrawRectangle(0, 0, UI_W, UI_H, new Color(26, 48, 39, 175)); State s = g.s;
        if (g.dialog == D_HELP)
        {
            panel(new Rectangle(410, 132, 620, 690), 0xf7f4e7); text("BEM-VINDO AO VALE", 446, 169, 14, 0x99a184); text("A sua vida no campo.", 446, 203, 32, 0x536348);
            string[] keys = ["WASD / setas", "1 - 5", "Clique", "E", "F + Espaço", "Tab", "F5", "F11", "Escape / H"]; string[] actions = ["Caminhar", "Escolher ferramenta", "Caminhar ou cultivar", "Conversar / recolher / abrir loja", "Pescar / puxar na hora certa", "Diário, mochila e vila", "Salvar agora", "Tela cheia", "Fechar / ajuda"]; for (int i = 0; i < 9; i++) { text(keys[i], 446, 271 + i * 29, 17, 0x899a78); text(actions[i], 650, 271 + i * 29, 17, 0x536348); }
            wrap("Prepare a terra, plante, regue e descanse. Colha com a cesta e venda para Lia na vila. Fale com Iara para ganhar uma vara, e com Bento para melhorar seu regador.", 446, 555, 548, 17, 25, 0x92997f);
            if (button(g, new Rectangle(446, 719, 262, 48), "Continuar", true, true)) g.dialog = D_NONE; if (button(g, new Rectangle(726, 719, 268, 48), "Salvar e sair", true, false)) { game_save(g); g.quit = true; }
        }
        else if (g.dialog == D_REST)
        {
            panel(new Rectangle(450, 300, 540, 310), 0xf7f4e7); text("LAR, DOCE LAR", 482, 336, 14, 0x99a184); text("Descansar por hoje?", 482, 371, 31, 0x536348); wrap("As plantas regadas vão crescer e sua energia será recuperada. O próximo dia começa às 8h.", 482, 426, 476, 19, 29, 0x92997f); if (button(g, new Rectangle(482, 535, 225, 43), "Ainda não", true, false)) g.dialog = D_NONE; if (button(g, new Rectangle(723, 535, 235, 43), "Bom descanso", true, true)) game_rest(g);
        }
        else if (g.dialog == D_NPC)
        {
            int n = g.npc; panel(new Rectangle(390, 240, 660, 455), 0xf7f4e7); panel(new Rectangle(424, 275, 68, 79), NPCS[n].coat); text(NPCS[n].name, 515, 296, 32, 0x536348); text(NPCS[n].role, 515, 275, 14, 0x99a184); for (int i = 0; i < 5; i++) heart(516 + i * 23, 337, i < s.friendship[n]); wrap(g.reply, 424, 390, 592, 21, 32, 0x6d7b5c);
            if (n == 0 && button(g, new Rectangle(424, 537, 592, 43), "Visitar o armazém", true, true)) g.dialog = D_SHOP;
            if (n == 1 && button(g, new Rectangle(424, 537, 592, 43), s.watering_level == 2 ? "Regador melhorado" : "Regador de cobre: 60 G + 8 madeiras", s.watering_level < 2, true)) { bool ok = upgrade_can(s); g.reply = ok ? "Prontinho! O regador agora molha o canteiro escolhido e seus quatro vizinhos. Cuide bem dele!" : "Preciso de 8 madeiras e 60 moedas. Recolha os troncos da fazenda com E e volte quando puder."; game_save(g); }
            if (n == 3 && (s.claimed & 4) == 0 && button(g, new Rectangle(424, 537, 592, 43), "Entregar 3 cenouras: +75 G", s.inventory[0] >= 3, true)) { if (claim_quest(s, 2)) { g.reply = "Essas cenouras vão ajudar muito minha pesquisa! Aqui estão suas 75 moedas. Obrigado por cuidar do vale."; game_save(g); } }
            if (n == 2) text("Vara de pesca recebida! F perto da água.", 424, 548, 18, 0x8a9b75);
            bool has_veg = s.inventory[0] + s.inventory[1] + s.inventory[2] > 0;
            if (button(g, new Rectangle(424, 605, 285, 43), "Dar um vegetal", has_veg && s.gifted_day[n] != s.day, false)) { if (give_gift(s, n)) { g.reply = "Para mim? Que carinho! Vou guardar esse presente com um sorriso. Obrigado por lembrar de mim!"; game_save(g); } }
            if (button(g, new Rectangle(727, 605, 289, 43), "Até mais!", true, false)) g.dialog = D_NONE;
        }
        else if (g.dialog == D_SHOP)
        {
            panel(new Rectangle(440, 175, 560, 610), 0xf7f4e7); text("O ARMAZÉM DA LIA", 475, 210, 14, 0x99a184); text("Sementes de um novo dia.", 475, 244, 28, 0x536348); text("Cada pacote tem 5 sementes.", 475, 290, 17, 0x92997f); string buf = ""; for (int i = 0; i < 3; i++) { int y = 344 + i * 91; DrawLine(475, y - 13, 965, y - 13, color_hex(0xe1e4d1)); crop_icon(i, 478, y + 3); text(CROPS[i].name, 530, y, 21, 0x536348); buf = Fmt("%d dias | %d sementes na mochila", CROPS[i].days, s.seeds[i]); text(buf, 530, y + 33, 14, 0x92997f); buf = Fmt("%d G", CROPS[i].price); if (button(g, new Rectangle(864, y + 2, 101, 44), buf, s.coins >= CROPS[i].price, true)) { buy_seeds(s, i); game_save(g); } }
            if (button(g, new Rectangle(475, 640, 490, 47), "Vender colheita e peixes", true, true)) { int value = sell_all(s); buf = Fmt(value >= 0 ? "Lia comprou sua colheita! +%d moedas." : "Limite de moedas atingido: %d.", value); game_toast(g, buf); game_save(g); }
            if (button(g, new Rectangle(475, 707, 490, 41), "Até a próxima, Lia!", true, false)) g.dialog = D_NONE;
        }
    }
    public static void game_draw(Game g)
    {
        State s = g.s; SetMouseCursor(MOUSE_CURSOR_DEFAULT); ClearBackground(color_hex(0x293f34)); string buf = "";
        flower(44, 36, 1); text("vale do sol", 73, 21, 32, 0xf2ead1); text("UMA PEQUENA VIDA NO CAMPO", 306, 30, 12, 0xa6b59a); text("C# + .NET | v3.0", 1030, 32, 13, 0xa1b196);
        bool active = g.dialog == D_NONE; if (button(g, new Rectangle(1216, 20, 111, 33), "Ajuda [H]", active, false)) game_modal(g, D_HELP); if (button(g, new Rectangle(1340, 20, 76, 33), "F11", active, false)) ToggleFullscreen();
        panel(new Rectangle(24, 72, 1092, 60), 0xf5f3e7); flower(56, 101, .6f); string[] seasons = ["Primavera", "Verão", "Outono", "Inverno"]; buf = Fmt("%s | Dia %d", seasons[((s.day - 1) / 28) % 4], (s.day - 1) % 28 + 1); text(buf, 81, 85, 22, 0x526246); buf = Fmt("%02d:%02d | Ano %d", s.minute / 60, s.minute % 60, (s.day - 1) / 112 + 1); text(buf, 82, 113, 12, 0x99a087);
        text(raining(s) ? "Chuva suave" : "Céu aberto", 690, 99, 15, 0x909975); DrawCircle(872, 104, 13, color_hex(0xe1c47b)); text("G", 867, 96, 16, 0x8c774a); buf = Fmt("%d", s.coins); text(buf, 895, 94, 24, 0xa08a4f); buf = Fmt("Energia %d/%d", s.energy, s.max_energy); text(buf, 982, 88, 12, 0x849575); DrawRectangle(982, 113, 111, 5, color_hex(0xdfe4d4)); DrawRectangle(982, 113, (int)(111.0f * s.energy / s.max_energy), 5, color_hex(0x91a574));
        BeginScissorMode(VIEW_X, VIEW_Y, VIEW_W, (int)VIEW_H); Camera2D world = new() { offset = new(VIEW_X, VIEW_Y), target = new(0, 0), rotation = 0, zoom = WORLD_SCALE }; BeginMode2D(world); Ray.SmoothText = false; world_draw(g); Ray.SmoothText = true; EndMode2D(); EndScissorMode();
        panel(new Rectangle(42, 149, 226, 46), 0xfaf6e5); DrawCircle(56, 167, 3, color_hex(0x91a770)); text(s.scene == FARM ? "Fazenda Girassol" : "Vila das Flores", 68, 158, 16, 0x53684b); text(s.scene == FARM ? "LAR, DOCE LAR" : "HISTÓRIAS PARA ENCONTRAR", 68, 180, 9, 0x9caa88);
        if (button(g, new Rectangle(928, 149, 166, 38), s.scene == FARM ? "Visitar a vila >" : "< Minha fazenda", active, true)) game_travel(g);
        if (g.toast_timer > 0) { int w = MeasureText(g.toast, 15) + 34; if (w > 1016) w = 1016; int x = VIEW_X + (VIEW_W - w) / 2; panel(new Rectangle(x, 775, w, 31), 0xfbf7e8); text(g.toast, x + 17, 783, 15, 0x61714e); }
        if (g.fishing != 0) { string message = g.fishing == 2 ? "AGORA! ESPAÇO para puxar!" : "Esperando a fisgada..."; int w = MeasureText(message, 20) + 44; panel(new Rectangle(VIEW_X + (VIEW_W - w) / 2, 214, w, 51), g.fishing == 2 ? 0xe8c57f : 0xf5ecd5); text(message, VIEW_X + (VIEW_W - w) / 2 + 22, 230, 20, 0x68734f); }
        panel(new Rectangle(24, 819, 1092, 81), 0xf5f3e7);
        string[] toolnames = ["Enxada", "Sementes", "Regador", "Cesta", "Explorar"]; string[] help = ["Prepare um novo canteiro", "Plante o que você quer colher", "Um pouco de carinho todo dia", "Colha e guarde na mochila", "Caminhe, converse e descubra"];
        for (int i = 0; i < 5; i++) { Rectangle b = new(43 + i * 61, 836, 52, 49); bool selected = g.tool == i; panel(b, selected ? 0xdfe8ce : 0xebeede); DrawRectangleRoundedLinesEx(b, .1f, 4, selected ? 2 : 1, color_hex(selected ? 0x859965 : 0xd9dfcd)); buf = Fmt("%d", i + 1); text(buf, (int)b.x + 4, (int)b.y + 3, 10, 0x8b9b75); tool_icon(i, b.x + 8, b.y + 8, .95f); if (active && hit(g, b)) { SetMouseCursor(MOUSE_CURSOR_POINTING_HAND); if (IsMouseButtonReleased(MOUSE_BUTTON_LEFT)) g.tool = i; } }
        text(g.tool == 1 ? CROPS[s.selected].name : toolnames[g.tool], 377, 843, 19, 0x5c6a4d); buf = Fmt("%d sementes | troque na Mochila", s.seeds[s.selected]); text(g.tool == 1 ? buf : help[g.tool], 377, 871, 13, 0x9aa287);
        if (button(g, new Rectangle(941, 840, 151, 43), "Descansar", active, false)) game_modal(g, D_REST);
        panel(new Rectangle(24, 901, 1092, 33), 0xf5f3e7); DrawCircle(43, 917, 3, color_hex(g.save_ok ? 0x99ad7a : 0xc48f88)); text(g.save_ok ? "Progresso salvo no seu computador" : "Save indisponível - consulte a pasta de dados", 54, 910, 12, 0x9ca28d); text("WASD andar  |  E conversar  |  F pescar", 750, 910, 12, 0x9ca28d);
        sidebar(g); text("VALE DO SOL / C# + RAYLIB", 28, 944, 11, 0x899d83); text("Um lugar para criar raízes.", 570, 943, 12, 0x899d83); text("Arte e personagens originais", 1185, 944, 10, 0x71886c); dialogs(g);
    }

}

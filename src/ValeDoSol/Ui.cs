using static ValeDoSol.Ray;
using static ValeDoSol.Logic;
using static ValeDoSol.World;
using static ValeDoSol.GameLogic;
using static ValeDoSol.Dialog;
namespace ValeDoSol;

public static partial class Ui
{
    public static readonly Rectangle Hotbar = new(371, 638, 538, 68);
    static readonly Rectangle BagButton = new(20, 650, 48, 48), MapButton = new(80, 650, 48, 48);
    static bool hit(Game g, Rectangle r) => CheckCollisionPointRec(g.mouse, r);
    public static bool hud_hit(Point p) => CheckCollisionPointRec(p, Hotbar) || CheckCollisionPointRec(p, BagButton) || CheckCollisionPointRec(p, MapButton) || CheckCollisionPointRec(p, new(1046, 12, 222, 170)) || CheckCollisionPointRec(p, new(16, 17, 320, 73));
    static void text(string s, double x, double y, int size, long color = 0x55371f) => DrawText(s, x, y, size, color_hex(color));
    static int wrap(string s, int x, int y, int width, int size, int line, long color = 0x55371f) { foreach (string paragraph in s.Split('\n')) { string current = ""; foreach (string word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries)) { string candidate = current.Length == 0 ? word : current + " " + word; if (current.Length > 0 && MeasureText(candidate, size) > width) { text(current, x, y, size, color); y += line; current = word; } else current = candidate; } if (current.Length > 0) { text(current, x, y, size, color); y += line; } } return y; }
    static void frame(Rectangle r, bool dark = false)
    {
        DrawRectangle(r.x + 4, r.y + 5, r.width, r.height, new Color(35, 24, 14, 95)); DrawRectangle(r.x - 4, r.y - 4, r.width + 8, r.height + 8, color_hex(0x42291b)); DrawRectangle(r.x, r.y, r.width, r.height, color_hex(0xab703d)); DrawRectangle(r.x + 5, r.y + 5, r.width - 10, r.height - 10, color_hex(dark ? 0x754b2e : 0xe9c48b));
        DrawRectangle(r.x + 6, r.y + 6, r.width - 12, 2, color_hex(dark ? 0x92623a : 0xf7dda5)); DrawRectangle(r.x + 6, r.y + r.height - 8, r.width - 12, 2, color_hex(0xb48048));
        foreach (int dx in new[] { 3, (int)r.width - 7 }) foreach (int dy in new[] { 3, (int)r.height - 7 }) DrawRectangle(r.x + dx, r.y + dy, 4, 4, color_hex(0xe3b66c));
    }
    static bool button(Game g, Rectangle r, string label, bool enabled = true, bool chosen = false)
    {
        bool over = enabled && hit(g, r); DrawRectangle(r.x, r.y, r.width, r.height, color_hex(0x684226)); DrawRectangle(r.x + 2, r.y + 2, r.width - 4, r.height - 4, color_hex(chosen ? 0xc28e4d : over ? 0xf1d49a : 0xd8af74)); DrawRectangle(r.x + 3, r.y + 3, r.width - 6, 2, color_hex(0xf5dba2));
        Color tint = color_hex(enabled ? 0x55371f : 0xa88a61); int size = 18; DrawText(label, r.x + (r.width - MeasureText(label, size)) / 2, r.y + (r.height - size) / 2, size, tint);
        if (over) SetMouseCursor(MOUSE_CURSOR_POINTING_HAND); return over && (g.dialog == D_NONE || g.dialog_age > .16f) && IsMouseButtonReleased(MOUSE_BUTTON_LEFT);
    }
    static void heart(int x, int y, bool full) { Color c = color_hex(full ? 0xb9524a : 0xb8a17b); DrawRectangle(x, y + 2, 5, 5, c); DrawRectangle(x + 7, y + 2, 5, 5, c); DrawRectangle(x + 2, y + 6, 9, 4, c); DrawRectangle(x + 5, y + 10, 3, 3, c); }
    static void portrait(int npc, int x, int y, float scale = 2) => Art.PortraitDraw(npc, x, y, 47 * scale);
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
        g.s = s; g.path_length = 0; g.pending = new(); g.fishing = 0; safe_position(g); reset_npcs(g);
        g.camera = camera_target(g.s); g.location_timer = 4; game_save(g); game_toast(g, "Backup restaurado. Bem-vindo de volta!");
    }
    static void hud(Game g)
    {
        State s = g.s;
        if (g.dialog == D_NONE) { var chapter = Story.Current(s); frame(new(20, 21, 312, 65)); text("Caderno de Aurora [J]", 33, 31, 16, 0x89623a); text(chapter.Title, 33, 56, 16); if (hit(g, new(20, 21, 312, 65)) && IsMouseButtonReleased(MOUSE_BUTTON_LEFT)) { g.tab = 1; game_modal(g, D_MENU); } }
        frame(new(1050, 16, 214, 112)); string[] season = ["Primavera", "Verão", "Outono", "Inverno"], week = ["Seg", "Ter", "Qua", "Qui", "Sex", "Sáb", "Dom"];
        text($"{week[(s.day - 1) % 7]}, {(s.day - 1) % 28 + 1} de {season[((s.day - 1) / 28) % 4]}", 1065, 29, 18);
        DrawRectangle(1062, 57, 190, 2, color_hex(0xc6975c)); text($"{s.minute / 60:00}:{s.minute % 60:00}", 1115, 71, 29);
        if (raining(s)) { for (int i = 0; i < 4; i++) DrawLineEx(new(1073 + i * 7, 74), new(1069 + i * 7, 90), 2, color_hex(0x6597b2)); } else { DrawCircle(1081, 84, 11, color_hex(0xd2993e)); for (int i = 0; i < 8; i++) { double a = i * Math.PI / 4; DrawRectangle(1080 + Math.Cos(a) * 17, 83 + Math.Sin(a) * 17, 3, 3, color_hex(0xd2993e)); } }
        frame(new(1050, 138, 214, 40)); DrawCircle(1076, 158, 10, color_hex(0xd4a24d)); text("G", 1071, 150, 16, 0x8a5b2c); text(s.coins.ToString(), 1100, 148, 23);
        frame(new(1232, 250, 24, 174), true); int h = (int)(158f * s.energy / s.max_energy); DrawRectangle(1239, 258, 10, 158, color_hex(0x4e3c25)); DrawRectangle(1239, 416 - h, 10, h, color_hex(s.energy < 25 ? 0xc58c47 : 0x91ac56)); text("E", 1236, 229, 16, 0xffedc4);
        frame(Hotbar); int active = active_slot(g);
        for (int i = 0; i < 9; i++)
        {
            Rectangle r = new(383 + i * 57, 648, 49, 48); DrawRectangle(r.x, r.y, r.width, r.height, color_hex(0x94633c)); DrawRectangle(r.x + 2, r.y + 2, r.width - 4, r.height - 4, color_hex(i == active ? 0xf0d59c : 0xcda36d)); if (i == active) DrawRectangleLinesEx(new(r.x - 2, r.y - 2, r.width + 4, r.height + 4), 2, color_hex(0xffecaf));
            text((i + 1).ToString(), r.x + 3, r.y + 1, 10, 0x795635);
            if (i < 5) tool_icon(i, r.x + 8, r.y + 9, .88); else if (i < 8) { crop_icon(i - 5, (int)r.x + 9, (int)r.y + 7); text(s.seeds[i - 5].ToString(), r.x + 29, r.y + 33, 12); } else { DrawLineEx(new(r.x + 8, r.y + 37), new(r.x + 27, r.y + 8), 3, color_hex(0x866337)); DrawLineEx(new(r.x + 27, r.y + 8), new(r.x + 39, r.y + 31), 1, color_hex(0xf3dfb6)); DrawCircle(r.x + 39, r.y + 32, 3, color_hex(s.rod ? 0xba6260 : 0xa88e6b)); }
            if (g.dialog == D_NONE && hit(g, r)) { SetMouseCursor(MOUSE_CURSOR_POINTING_HAND); if (IsMouseButtonReleased(MOUSE_BUTTON_LEFT)) select_slot(g, i); }
        }
        frame(BagButton); tool_icon(3, 27, 658, .9); text("I", 58, 679, 13);
        if (g.dialog == D_NONE && hit(g, BagButton) && IsMouseButtonReleased(MOUSE_BUTTON_LEFT)) { g.tab = 0; game_modal(g, D_MENU); }
        frame(MapButton); DrawRectangle(88, 660, 31, 26, color_hex(0x8fa66b)); DrawLine(99, 661, 99, 685, color_hex(0xd8cc9a)); DrawLine(108, 661, 108, 685, color_hex(0xd8cc9a)); DrawLine(89, 674, 118, 674, color_hex(0xd8cc9a)); text("M", 113, 680, 12);
        if (g.dialog == D_NONE && hit(g, MapButton) && IsMouseButtonReleased(MOUSE_BUTTON_LEFT)) open_civic(g, 1);
        if (g.location_timer > 0) { string name = Map.Name(s.scene); int width = MeasureText(name, 20) + 44; frame(new((UI_W - width) / 2, 21, width, 40)); text(name, (UI_W - width) / 2 + 22, 30, 20); }
        if (g.dialog == D_NONE)
        {
            string hint = interaction_hint(g); if (hint.Length > 0) { int width = MeasureText(hint, 17) + 30; DrawRectangle((UI_W - width) / 2, 594, width, 29, new Color(47, 38, 24, 194)); text(hint, (UI_W - width) / 2 + 15, 600, 17, 0xffedc3); }
            if (g.toast_timer > 0) { var lines = TextLayout.Wrap(g.toast, 930, v => MeasureText(v, 17)); int width = Math.Min(970, lines.Max(v => MeasureText(v, 17)) + 34), height = lines.Count * 23 + 16, top = 584 - height; frame(new((UI_W - width) / 2, top, width, height)); int y = top + 8; foreach (var line in lines) { text(line, (UI_W - width) / 2 + 17, y, 17); y += 23; } }
        }
        if (g.fishing != 0) { string prompt = g.fishing == 2 ? "AGORA! Aperte ESPAÇO" : "Esperando a fisgada..."; int w = MeasureText(prompt, 24) + 44; frame(new((UI_W - w) / 2, 140, w, 51)); text(prompt, (UI_W - w) / 2 + 22, 153, 24, g.fishing == 2 ? 0x9e4f32 : 0x55371f); }
    }
    static void inventory(Game g)
    {
        State s = g.s; frame(g.tab == 2 ? new(180, 70, 920, 556) : g.tab == 1 ? new(190, 86, 900, 540) : new(280, 90, 720, 536)); string[] tabs = ["Mochila [I]", "Diário [J]", "Moradores [R]"];
        for (int i = 0; i < 3; i++) if (button(g, new(300 + i * 216, 108, 208, 38), tabs[i], true, g.tab == i)) g.tab = i;
        if (button(g, new(956, 108, 26, 30), "X")) { g.dialog = D_NONE; return; }
        if (g.tab == 0)
        {
            portrait(-1, 310, 186, 1.7f); text("Fazendeiro", 304, 289, 20); text($"Dia {s.day}", 323, 319, 17); for (int i = 0; i < 5; i++) { Rectangle r = new(458 + i * 93, 183, 76, 66); frame(r); tool_icon(i, r.x + 18, r.y + 16, 1.15); if (hit(g, r) && g.dialog_age > .16 && IsMouseButtonReleased(MOUSE_BUTTON_LEFT)) { g.tool = i; g.dialog = D_NONE; } }
            text("Sementes", 458, 273, 21); for (int i = 0; i < 3; i++) { int x = 458 + i * 166; frame(new(x, 308, 150, 104)); crop_icon(i, x + 13, 324); text(CROPS[i].name, x + 53, 321, 18); text($"x{s.seeds[i]}", x + 56, 350, 21); if (button(g, new(x + 10, 378, 130, 27), "Equipar")) { s.selected = i; g.tool = 1; g.dialog = D_NONE; game_save(g); } }
            string[] names = ["Madeira", "Pedra", "Peixes"]; int[] values = [s.wood, s.stone, s.fish]; for (int i = 0; i < 3; i++) { int x = 308 + i * 219; frame(new(x, 446, 202, 68)); text(names[i], x + 14, 459, 18); text($"x{values[i]}", x + 137, 473, 22); }
            text($"Colheita: {s.inventory[0]} cenouras | {s.inventory[1]} nabos | {s.inventory[2]} morangos", 310, 531, 17);
            if (button(g, new(307, 569, 201, 33), "Criar backup")) backup(g, false); if (button(g, new(519, 569, 201, 33), "Restaurar backup")) backup(g, true); text(s.watering_level == 2 ? "Regador de cobre" : "Regador simples", 745, 576, 17);
        }
        else if (g.tab == 1)
        {
            if (g.journal_page == 0) { draw_journal(g, 190, 86); return; }
            if (button(g, new(746, 151, 223, 25), "Voltar à história")) { g.journal_page = 0; return; }
            string[] names = ["Rostos do vale", "O fruto do trabalho", "A pesquisa de Theo"], desc = ["Conheça Lia, Bento, Iara e Theo.", "Colha 5 vegetais na fazenda.", "Entregue 3 cenouras para Theo."]; int[] counts = [System.Numerics.BitOperations.PopCount(s.met & 15), s.harvested, s.inventory[0]], targets = [4, 5, 3], reward = [40, 60, 75];
            for (int i = 0; i < 3; i++) { int y = 177 + i * 132; frame(new(308, y, 664, 112)); text(names[i], 326, y + 13, 23); text(desc[i], 326, y + 45, 18); DrawRectangle(326, y + 78, 400, 6, color_hex(0xb28d5c)); DrawRectangle(326, y + 78, 400 * Math.Min(1, (double)counts[i] / targets[i]), 6, color_hex(0x849958)); text($"{Math.Min(counts[i], targets[i])}/{targets[i]}  +{reward[i]} G", 746, y + 18, 17); bool done = (s.claimed & (1u << i)) != 0; if (done) text("Concluída", 794, y + 68, 18); else if (i == 2) text("Fale com Theo", 794, y + 68, 17); else if (button(g, new(790, y + 61, 160, 33), "Receber", counts[i] >= targets[i])) { if (claim_quest(s, i)) { game_save(g); game_toast(g, "Missão concluída!"); } } }
        }
        else
        {
            for (int i = 0; i < Social.Count; i++)
            {
                int x = 201 + i % 2 * 449, y = 164 + i / 2 * 108; bool met = (s.met & (1u << i)) != 0;
                frame(new(x, y, 427, 98)); portrait(i, x + 8, y + 8, 1.6f); text(met ? NPCS[i].name : "???", x + 94, y + 7, 22); text(NPCS[i].role, x + 94, y + 34, 15);
                for (int k = 0; k < 5; k++) heart(x + 94 + k * 20, y + 58, k < s.friendship[i]);
                text(met ? Social.Now(i, s).activity : "Ainda não se conheceram", x + 94, y + 79, 13, 0x846044);
            }
            text("Chuva e fins de semana mudam as rotinas.", 211, 604, 16);
            if (button(g, new(757, 594, 316, 30), "Ver retratos ampliados")) game_modal(g, D_GALLERY);

        }
    }
    static void dialogue(Game g)
    {
        State s = g.s; int n = g.npc; frame(new(86, 396, 1108, 298)); frame(new(96, 409, 224, 244)); portrait(n, 100, 415, 4.5f); text(NPCS[n].name, 110, 665, 23); text(NPCS[n].role, 342, 415, 16, 0x927047); text(Social.Now(n, s).activity, 342, 438, 14, 0x927047);
        int letters = Math.Clamp((int)(Math.Max(0, g.dialog_age - .1f) * 55), 0, g.reply.Length); wrap(g.reply[..letters], 342, 468, 825, 21, 29);
        string[] topics = ["Seu trabalho [Q]", "Algo pessoal [T]", "Seu dia [Y]", "O caderno [U]"]; int[] keys = [81, 84, 89, 85];
        for (int i = 0; i < 4; i++) if (button(g, new(342 + i * 207, 590, 199, 33), topics[i]) || (g.dialog_age > .2f && IsKeyPressed(keys[i]))) { g.reply = i == 3 ? Story.NpcLine(s, n) : Social.Topic(s, n, i); g.dialog_age = 0; game_save(g); }

        bool storyAction = Story.Current(s).ContactNpc == n && can_story_handoff(g);
        if (storyAction && button(g, new(342, 640, 230, 34), "Continuar história")) { advance_story(g); return; }
        if (!storyAction && n == 0 && button(g, new(342, 640, 230, 34), "Comprar e vender", g.s.scene == SHOP && g.npc_scene[0] == SHOP)) game_modal(g, D_SHOP);
        if (!storyAction && n == 1 && button(g, new(342, 640, 230, 34), s.watering_level == 2 ? "Regador melhorado" : "Regador: 60 G + 8 madeiras", s.watering_level < 2)) { g.reply = upgrade_can(s) ? "Prontinho! Agora seu regador molha o canteiro escolhido e seus quatro vizinhos." : "Preciso de 8 madeiras e 60 moedas. Recolha os troncos pela fazenda e volte quando puder."; g.dialog_age = 0; game_save(g); }
        if (!storyAction && n == 3 && (s.claimed & 4) == 0 && button(g, new(342, 640, 230, 34), "Entregar 3 cenouras", s.inventory[0] >= 3)) { claim_quest(s, 2); g.reply = "Perfeitas! Muito obrigado por ajudar na minha pesquisa. Aqui estão suas 75 moedas."; g.dialog_age = 0; game_save(g); }
        if (!storyAction && n == 2) text("Sua vara está na tecla 9.", 342, 649, 17, 0x887045); if (!storyAction && n >= 4) text("Converse pelos assuntos acima.", 342, 649, 17, 0x887045);
        if (button(g, new(585, 640, 245, 34), "Dar um presente", s.inventory.Any(x => x > 0) && s.gifted_day[n] != s.day)) { int crop = Array.FindIndex(s.inventory, count => count > 0); give_gift(s, n); g.reply = Social.GiftReply(s, n, crop); g.dialog_age = 0; game_save(g); }
        if (button(g, new(847, 640, 324, 34), "Até mais! [E]")) g.dialog = D_NONE;
        if (g.dialog_age > .2f && (IsKeyPressed(KEY_E) || IsKeyPressed(KEY_ENTER) || IsKeyPressed(KEY_SPACE))) { if (letters < g.reply.Length) g.dialog_age = g.reply.Length / 55f + 1; else g.dialog = D_NONE; }
    }
    static void dialogs(Game g)
    {
        if (g.dialog == D_NONE) return; if (g.dialog != D_NPC) DrawRectangle(0, 0, UI_W, UI_H, new Color(20, 25, 18, 125));
        if (g.dialog == D_GALLERY) { draw_gallery(g); return; }
        if (g.dialog == D_CIVIC) { draw_civic(g); return; }
        if (g.dialog == D_TITLE) { draw_title(g); return; }
        if (g.dialog == D_LETTER) { draw_letter(g); return; }
        if (g.dialog == D_MENU) { inventory(g); return; }
        if (g.dialog == D_NPC) { dialogue(g); return; }
        if (g.dialog == D_PAUSE) { frame(new(450, 122, 380, 480)); text("Vale do Sol", 510, 157, 39); if (button(g, new(490, 225, 300, 50), "Continuar")) g.dialog = D_NONE; if (button(g, new(490, 290, 300, 50), "Controles")) game_modal(g, D_HELP); if (button(g, new(490, 355, 300, 50), g.s.sharp_pixels ? "Imagem: nítida" : "Imagem: preencher")) { g.s.sharp_pixels = !g.s.sharp_pixels; game_save(g); } if (button(g, new(490, 420, 300, 50), "Salvar e sair")) { game_save(g); g.quit = true; } text("Esc para voltar", 561, 548, 17); }
        else if (g.dialog == D_HELP) { frame(new(273, 60, 734, 594)); text("Como viver no vale", 315, 91, 32); string[] keys = ["WASD / setas", "Shift", "1-5 / 6-8 / 9", "Clique / Espaço", "Botão direito", "E / F", "I / J / R", "F5 / F11"], actions = ["Caminhar", "Correr", "Ferramentas / sementes / vara", "Usar a ferramenta perto de você", "Caminhar até um ponto", "Interagir / pescar", "Mochila / diário / moradores", "Salvar / tela cheia"]; for (int i = 0; i < keys.Length; i++) { text(keys[i], 315, 153 + i * 39, 19, 0x927047); text(actions[i], 552, 153 + i * 39, 19); } wrap("A horta fica ao sul da estrada. Cultive pelos corredores; os canteiros elevados são sólidos. Use M para consultar o mapa. Visite a prefeitura e o quadro de avisos para conhecer os lugares da vila.", 315, 483, 645, 19, 28); if (button(g, new(450, 594, 380, 38), "Voltar ao jogo")) g.dialog = D_NONE; }
        else if (g.dialog == D_REST) { frame(new(380, 245, 520, 230)); text("Dormir por hoje?", 421, 276, 32); wrap("As plantas regadas vão crescer e sua energia será recuperada. Amanhã é um novo começo.", 421, 330, 435, 19, 27); if (button(g, new(415, 417, 210, 38), "Ainda não")) g.dialog = D_NONE; if (button(g, new(645, 417, 220, 38), "Sim, boa noite")) game_rest(g); }
        else if (g.dialog == D_SHOP)
        {
            frame(new(330, 81, 620, 558)); text("Armazém da Lia", 372, 113, 32); text($"Suas moedas: {g.s.coins} G", 680, 124, 18); for (int i = 0; i < 3; i++) { int y = 184 + i * 102; frame(new(360, y, 560, 87)); crop_icon(i, 378, y + 21); text(CROPS[i].name, 434, y + 12, 24); text($"{CROPS[i].days} dias | {g.s.seeds[i]} sementes na mochila", 434, y + 49, 17); if (button(g, new(753, y + 22, 145, 40), $"5 por {CROPS[i].price} G", g.s.coins >= CROPS[i].price)) { buy_seeds(g.s, i); game_save(g); } }
            if (button(g, new(361, 516, 559, 42), "Vender colheita e peixes")) { int value = sell_all(g.s); game_save(g); game_toast(g, value < 0 ? "Gaste algumas moedas antes de vender. Sua colheita foi preservada." : value == 0 ? "Sua mochila ainda não tem colheitas ou peixes para vender." : $"Colheita vendida! +{value} moedas."); }
            if (button(g, new(361, 580, 559, 36), "Até a próxima, Lia!")) g.dialog = D_NONE;
        }
    }
    public static void game_draw(Game g)
    {
        SetMouseCursor(MOUSE_CURSOR_DEFAULT); ClearBackground(color_hex(0x20291a)); Camera2D camera = new() { offset = new(UI_W / 2, UI_H / 2), target = render_camera(g), rotation = 0, zoom = CAMERA_ZOOM }; BeginMode2D(camera); Ray.SmoothText = false; world_draw(g); Ray.SmoothText = true; EndMode2D(); if (g.dialog != D_TITLE) hud(g); dialogs(g);
        if (g.fade_timer > 0) DrawRectangle(0, 0, UI_W, UI_H, new Color(15, 19, 12, (int)(255 * g.fade_timer / .3f)));
    }
}

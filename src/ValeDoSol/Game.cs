using static ValeDoSol.Ray;
using static ValeDoSol.Logic;
using static ValeDoSol.Dialog;
namespace ValeDoSol;

public enum Dialog { D_NONE, D_HELP, D_NPC, D_SHOP, D_REST, D_MENU, D_PAUSE }
public readonly record struct PendingAction(int type = 0, int index = 0, int tool = 0, Point target = default);
public class Game
{
    public State s = new();
    public int tool, tab, hover = -1, npc = -1, facing;
    public Dialog dialog;
    public Point mouse, camera, action_target;
    public Point[] path = new Point[MAX_PATH], npc_position = new Point[4];
    public Point[][] npc_paths = Enumerable.Range(0, 4).Select(_ => new Point[MAX_PATH]).ToArray();
    public int path_length, path_at;
    public int[] npc_phase = new int[4], npc_length = new int[4], npc_at = new int[4];
    public PendingAction pending;
    public float elapsed, clock_tick, save_tick, toast_timer, dialog_age, location_timer = 4, fade_timer, action_timer;
    public bool moving, save_ok, quit, ignore_focus;
    public int fishing;
    public float fish_timer, fish_wait;
    public string toast = "", reply = "", save_path = "", save_dir = "";
}
public static class GameLogic
{
    public const int UI_W = 1280, UI_H = 720;
    public const float CAMERA_ZOOM = 1.5f;
    public static Point camera_target(State s) { Point size = world_size(s.scene); float hw = UI_W / (2 * CAMERA_ZOOM), hh = UI_H / (2 * CAMERA_ZOOM); return new(size.x < 2 * hw ? size.x / 2 : Math.Clamp(s.player.x, hw, size.x - hw), size.y < 2 * hh ? size.y / 2 : Math.Clamp(s.player.y, hh, size.y - hh)); }
    public static Point screen_to_world(Game g, Point p) => new((p.x - UI_W / 2f) / CAMERA_ZOOM + g.camera.x, (p.y - UI_H / 2f) / CAMERA_ZOOM + g.camera.y);
    public static Point world_to_screen(Game g, Point p) => new((p.x - g.camera.x) * CAMERA_ZOOM + UI_W / 2f, (p.y - g.camera.y) * CAMERA_ZOOM + UI_H / 2f);
    public static void select_slot(Game g, int slot) { if (slot < 5) g.tool = slot; else if (slot < 8) { g.tool = 1; g.s.selected = slot - 5; } else g.tool = 5; }
    public static int active_slot(Game g) => g.tool == 5 ? 8 : g.tool == 1 ? 5 + g.s.selected : g.tool;
    public static void game_scene(Game g, Scene scene, Point at) { g.s.scene = scene; g.s.player = at; g.path_length = 0; g.pending = new(); g.fishing = 0; g.dialog = D_NONE; g.camera = camera_target(g.s); g.location_timer = 4; g.fade_timer = .3f; game_save(g); }

    static float distance(Point a, Point b) => MathF.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
    static void reset_npcs(Game g) { for (int i = 0; i < 4; i++) { int phase = g.s.minute >= 780 ? 1 : 0; g.npc_phase[i] = phase; g.npc_position[i] = phase == 1 ? NPCS[i].afternoon : NPCS[i].morning; g.npc_length[i] = g.npc_at[i] = 0; } }
    public static void game_toast(Game g, string message) { g.toast = message; g.toast_timer = 7; }
    public static void game_save(Game g) { g.save_ok = state_save(g.s, g.save_path); }
    public static void game_init(Game g, string dir)
    {
        g.save_dir = dir; g.save_path = Path.Combine(dir, "save.json"); bool exists = false, loaded = false;
        try { Directory.CreateDirectory(dir); exists = File.Exists(g.save_path); if (state_load(out var state, g.save_path)) { g.s = state; loaded = true; using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(g.save_path)); string backup = Path.Combine(dir, "save-before-v4.json"); if (!document.RootElement.TryGetProperty("edition", out _) && !File.Exists(backup)) File.Copy(g.save_path, backup); } } catch (Exception e) when (e is IOException or UnauthorizedAccessException) { g.save_path = ""; }
        reset_npcs(g); g.camera = camera_target(g.s);
        if (exists && !loaded) { g.save_path = ""; g.save_ok = false; game_toast(g, "Save inválido preservado. Inicie com outro diretório de save para criar uma nova partida."); }
        else { game_save(g); game_toast(g, loaded ? "Bem-vindo de volta! O vale sentiu sua falta." : "Um novo começo. Aperte H para aprender a jogar."); }
    }
    public static void game_modal(Game g, Dialog d) { g.dialog = d; g.dialog_age = 0; g.path_length = 0; g.pending = new(); g.fishing = 0; }
    public static void game_travel(Game g) { if (g.s.scene == HOME) { game_scene(g, FARM, new(176, 272)); return; } if (g.s.scene == SHOP) { game_scene(g, VILLAGE, new(240, 288)); return; } game_scene(g, g.s.scene == FARM ? VILLAGE : FARM, g.s.scene == FARM ? new(80, 384) : new(WORLD_W - 80, 384)); }
    public static void game_rest(Game g) { int grown = next_day(g.s); game_scene(g, HOME, new(320, 280)); g.clock_tick = 0; reset_npcs(g); game_toast(g, $"Bom dia! {grown} plantas cresceram. " + (raining(g.s) ? "A chuva está regando a horta." : "Um novo dia começa.")); }
    public static void game_converse(Game g, int npc) { if (npc < 0 || npc > 3) return; bool first = (g.s.met & (1u << npc)) == 0; talk_to(g.s, npc); g.npc = npc; g.reply = first ? NPCS[npc].first : NPCS[npc].later[g.s.day % 2]; game_modal(g, D_NPC); game_save(g); }
    static PendingAction closest(Game g)
    {
        PendingAction result = new(); float best = 76; Point p = g.s.player;
        void Consider(PendingAction a) { float d = distance(p, a.target); if (d < best) { best = d; result = a; } }
        if (g.s.scene == HOME) { Consider(new(7, 0, 0, new(416, 208))); Consider(new(8, 0, 0, new(320, 432))); return result; }
        if (g.s.scene == SHOP) { Consider(new(1, 0, 0, new(352, 276))); Consider(new(9, 0, 0, new(352, 464))); return result; }
        if (g.s.scene == VILLAGE) for (int i = 0; i < 4; i++) Consider(new(1, i, 0, g.npc_position[i]));
        for (int i = 0; i < 6; i++) { var r = RESOURCES[i]; if (r.scene != g.s.scene || (g.s.gathered & (1u << i)) != 0) continue; Consider(new(2, i, 0, new(r.x * 32 + 16, r.y * 32 + 16))); }
        var buildings = g.s.scene == FARM ? FARM_BUILDINGS : VILLAGE_BUILDINGS;
        for (int i = 0; i < buildings.Length; i++) { var b = buildings[i]; if (b.type > 2) continue; Consider(new(b.type + 3, i, 0, new((b.x + b.w / 2f) * 32, (b.y + b.h) * 32 + 22))); }
        return result;
    }
    static void perform(Game g, PendingAction a)
    {
        if (a.type == 1) { game_converse(g, a.index); return; }
        if (a.type == 3) { game_scene(g, HOME, new(320, 352)); return; }
        if (a.type == 7) { game_modal(g, D_REST); return; }
        if (a.type is 8 or 9) { game_travel(g); return; }
        if (a.type == 4) { game_scene(g, SHOP, new(352, 400)); return; }
        if (a.type == 5) { game_converse(g, 1); return; }
        if (a.type != 2 || (g.s.gathered & (1u << a.index)) != 0) return;
        if (g.s.energy < 5) { game_toast(g, "Você precisa descansar para recuperar energia."); return; }
        if (g.s.wood > 9999997 || g.s.stone > 9999998) return;
        g.s.energy -= 5; g.s.gathered |= 1u << a.index; if (RESOURCES[a.index].type == 0) { g.s.wood += 3; game_toast(g, "+3 madeiras! Leve para Bento melhorar seu regador."); } else { g.s.stone += 2; if (g.s.coins <= 999999997) g.s.coins += 3; game_toast(g, "+2 pedras e 3 moedas encontradas entre elas."); }
        game_save(g);
    }
    public static void game_interact(Game g) { var a = closest(g); if (a.type != 0) perform(g, a); else game_toast(g, "Aproxime-se de um morador, de uma porta ou de um tronco."); }
    static bool near_pond(Game g) => water_bodies(g.s.scene).Any(w => distance(g.s.player, new(Math.Clamp(g.s.player.x, w.x, w.x + w.width), Math.Clamp(g.s.player.y, w.y, w.y + w.height))) < 67);
    public static string interaction_hint(Game g) { var a = closest(g); return a.type switch { 1 => "E  Conversar com " + NPCS[a.index].name, 2 => RESOURCES[a.index].type == 0 ? "E  Recolher madeira" : "E  Recolher pedra", 3 => "E  Entrar em casa", 4 => "E  Entrar no armazém", 5 => "E  Falar com Bento", 7 => "E  Dormir", 8 or 9 => "E  Sair", _ => g.s.rod && near_pond(g) ? "F  Pescar" : "" }; }
    public static void farm_near(Game g, int index) { if (index < 0) return; Point p = plot_position(index); p.x += 16; p.y += 16; if (distance(g.s.player, p) > 76) { game_toast(g, "Chegue mais perto do canteiro para usar a ferramenta."); return; } string message = farm_action(g.s, index, g.tool, out bool ok); if (ok) { g.action_timer = .32f; g.action_target = p; game_save(g); } game_toast(g, message); }
    public static void use_tool(Game g) { if (g.tool == 5) { game_fish(g); return; } if (g.tool == 4) { game_interact(g); return; } if (g.s.scene != FARM) return; int index = g.hover; if (index < 0) { Point p = g.s.player; Point[] directions = [new(0, 32), new(-32, 0), new(32, 0), new(0, -32)]; p.x += directions[g.facing].x; p.y += directions[g.facing].y; index = plot_at(p.x, p.y); } farm_near(g, index); }

    public static void game_fish(Game g)
    {
        if (g.fishing != 0) { g.fishing = 0; return; }
        if (!g.s.rod) { game_toast(g, "Converse com Iara na vila para ganhar uma vara."); return; }
        if (!near_pond(g)) { game_toast(g, "Aproxime-se da margem do lago para pescar."); return; }
        if (g.s.energy < 5) { game_toast(g, "Você precisa descansar antes de pescar."); return; }
        g.s.energy -= 5; g.fishing = 1; g.fish_timer = 0; g.fish_wait = 2 + random_unit(g.s) * 3; g.path_length = 0; g.pending = new(); game_save(g);
    }
    public static void game_catch(Game g) { if (g.fishing == 0) return; if (g.fishing == 2) { g.s.fish = Math.Min(10000000, g.s.fish + 1); game_toast(g, "Um peixe fresquinho! Venda por 18 G para Lia."); } else game_toast(g, "Muito cedo! Espere aparecer AGORA antes de puxar."); g.fishing = 0; game_save(g); }
    static void finish_action(Game g) { var a = g.pending; g.pending = new(); if (a.type == 0) return; if (a.type == 6) { var p = plot_position(a.index); p.x += 16; p.y += 16; if (distance(g.s.player, p) < 72) { game_toast(g, farm_action(g.s, a.index, a.tool, out _)); game_save(g); } } else if (distance(g.s.player, a.target) < 90) perform(g, a); }
    static void walk_to(Game g, Point target, PendingAction action)
    {
        g.fishing = 0; Point goal = new(); bool found = false; float best = float.MaxValue;
        for (int radius = 0; radius < 4 && !found; radius++) for (int x = -radius; x <= radius; x++) for (int y = -radius; y <= radius; y++) { if (Math.Abs(x) + Math.Abs(y) > radius) continue; Point candidate = new(((int)MathF.Floor(target.x / 32) + x) * 32 + 16, ((int)MathF.Floor(target.y / 32) + y) * 32 + 16); if (blocked(g.s.scene, candidate.x, candidate.y)) continue; float d = distance(g.s.player, candidate); if (d < best) { best = d; goal = candidate; found = true; } }
        if (!found) { game_toast(g, "Não dá para caminhar até esse lugar."); return; }
        g.path_length = find_path(g.s.scene, g.s.player, goal, g.path); g.path_at = 0; g.pending = action; if (g.path_length == 0) { if (distance(g.s.player, goal) < 46) finish_action(g); else { g.pending = new(); game_toast(g, "Caminho bloqueado. Tente chegar pelo outro lado."); } }
    }
    static void world_click(Game g, Point p)
    {
        if (g.s.scene == VILLAGE) for (int i = 0; i < 4; i++) { Point at = g.npc_position[i]; if (distance(new(at.x, at.y - 12), p) < 30) { if (distance(g.s.player, at) < 90) game_converse(g, i); else game_toast(g, "Aproxime-se para conversar."); return; } }
        if (g.s.scene is HOME or SHOP) { game_interact(g); return; }
        for (int i = 0; i < 6; i++) { var r = RESOURCES[i]; Point at = new(r.x * 32 + 16, r.y * 32 + 16); if (r.scene == g.s.scene && (g.s.gathered & (1u << i)) == 0 && distance(at, p) < 22) { if (distance(g.s.player, at) < 76) perform(g, new(2, i, 0, at)); else game_toast(g, "Chegue mais perto para recolher."); return; } }
        int index = g.s.scene == FARM ? plot_at(p.x, p.y) : -1; if (index >= 0 && g.tool < 4) { farm_near(g, index); return; }
        if (g.tool == 5) { game_fish(g); return; }
        if (g.tool == 4) walk_to(g, p, new());
    }
    static void move_player(Game g, float dx, float dy) { if (!blocked(g.s.scene, g.s.player.x + dx, g.s.player.y)) g.s.player.x += dx; if (!blocked(g.s.scene, g.s.player.x, g.s.player.y + dy)) g.s.player.y += dy; }
    static void update_npcs(Game g, float dt) { if (g.s.scene != VILLAGE) return; for (int i = 0; i < 4; i++) { int phase = g.s.minute >= 780 ? 1 : 0; if (phase != g.npc_phase[i]) { g.npc_phase[i] = phase; g.npc_length[i] = find_path(VILLAGE, g.npc_position[i], phase == 1 ? NPCS[i].afternoon : NPCS[i].morning, g.npc_paths[i]); g.npc_at[i] = 0; } if (g.npc_at[i] >= g.npc_length[i]) continue; Point goal = g.npc_paths[i][g.npc_at[i]], p = g.npc_position[i]; float d = distance(p, goal), step = 35 * dt; if (d <= step) { g.npc_position[i] = goal; g.npc_at[i]++; } else { g.npc_position[i].x += (goal.x - p.x) / d * step; g.npc_position[i].y += (goal.y - p.y) / d * step; } } }
    public static void game_update(Game g, float dt, Point mouse)
    {
        g.mouse = mouse; g.elapsed += dt; g.toast_timer = Math.Max(0, g.toast_timer - dt); g.action_timer = Math.Max(0, g.action_timer - dt); g.location_timer = Math.Max(0, g.location_timer - dt); g.fade_timer = Math.Max(0, g.fade_timer - dt); g.moving = false;
        if (g.dialog != D_NONE) g.dialog_age += dt;
        if (IsKeyPressed(KEY_F11)) ToggleFullscreen();
        if (IsKeyPressed(KEY_ESCAPE)) { if (g.dialog != D_NONE) g.dialog = D_NONE; else game_modal(g, D_PAUSE); g.fishing = 0; return; }
        int menu = IsKeyPressed(KEY_I) || IsKeyPressed(KEY_TAB) ? 0 : IsKeyPressed(KEY_J) ? 1 : IsKeyPressed(KEY_R) ? 2 : -1;
        if (menu >= 0) { if (g.dialog == D_MENU && g.tab == menu) g.dialog = D_NONE; else { g.tab = menu; game_modal(g, D_MENU); } return; }
        if (g.dialog != D_NONE || (!g.ignore_focus && !IsWindowFocused())) return;
        if (IsKeyPressed(KEY_H)) { game_modal(g, D_HELP); return; }
        for (int i = 0; i < 9; i++) if (IsKeyPressed(KEY_ONE + i)) select_slot(g, i);
        if (IsKeyPressed(KEY_E) && g.fishing == 0) game_interact(g); if (g.dialog != D_NONE) return;
        if (IsKeyPressed(KEY_F)) game_fish(g);
        if (IsKeyPressed(KEY_SPACE)) { if (g.fishing != 0) game_catch(g); else use_tool(g); }
        if (IsKeyPressed(KEY_F5)) { game_save(g); game_toast(g, g.save_ok ? "Progresso salvo." : "Não foi possível salvar."); }
        float dx = ((IsKeyDown(KEY_D) || IsKeyDown(KEY_RIGHT)) ? 1 : 0) - ((IsKeyDown(KEY_A) || IsKeyDown(KEY_LEFT)) ? 1 : 0), dy = ((IsKeyDown(KEY_S) || IsKeyDown(KEY_DOWN)) ? 1 : 0) - ((IsKeyDown(KEY_W) || IsKeyDown(KEY_UP)) ? 1 : 0);
        float speed = IsKeyDown(KEY_LEFT_SHIFT) ? 220 : 150;
        if (dx != 0 || dy != 0) { g.facing = dx < 0 ? 1 : dx > 0 ? 2 : dy < 0 ? 3 : 0; g.path_length = 0; g.pending = new(); g.fishing = 0; float norm = MathF.Sqrt(dx * dx + dy * dy); move_player(g, dx / norm * speed * dt, dy / norm * speed * dt); g.moving = true; }
        else if (g.path_at < g.path_length) { Point goal = g.path[g.path_at]; float d = distance(g.s.player, goal), step = speed * dt; if (d <= step) { g.s.player = goal; g.path_at++; if (g.path_at == g.path_length) finish_action(g); } else { float x = goal.x - g.s.player.x, y = goal.y - g.s.player.y; g.facing = Math.Abs(x) > Math.Abs(y) ? (x < 0 ? 1 : 2) : (y < 0 ? 3 : 0); move_player(g, x / d * step, y / d * step); g.moving = true; } }
        if (g.dialog != D_NONE) return;
        if ((g.s.scene == FARM && g.s.player.x > WORLD_W - 28 && Math.Abs(g.s.player.y - 384) < 44) || (g.s.scene == VILLAGE && g.s.player.x < 28 && Math.Abs(g.s.player.y - 384) < 44) || (g.s.scene == HOME && g.s.player.y > 432 && Math.Abs(g.s.player.x - 320) < 48) || (g.s.scene == SHOP && g.s.player.y > 464 && Math.Abs(g.s.player.x - 352) < 48)) game_travel(g);
        Point target = camera_target(g.s); float blend = Math.Min(1, dt * 10); g.camera.x += (target.x - g.camera.x) * blend; g.camera.y += (target.y - g.camera.y) * blend;
        Point wp = screen_to_world(g, mouse); g.hover = g.s.scene == FARM ? plot_at(wp.x, wp.y) : -1;
        if (mouse.x >= 0 && mouse.x < UI_W && mouse.y >= 0 && mouse.y < UI_H && !Ui.hud_hit(mouse))
        {
            if (IsMouseButtonReleased(MOUSE_BUTTON_LEFT)) { if (g.fishing != 0) game_catch(g); else world_click(g, wp); }
            if (IsMouseButtonReleased(MOUSE_BUTTON_RIGHT)) walk_to(g, wp, new());
        }
        update_npcs(g, dt);
        if (g.fishing != 0) { g.fish_timer += dt; if (g.fishing == 1 && g.fish_timer >= g.fish_wait) { g.fishing = 2; g.fish_timer = 0; } else if (g.fishing == 2 && g.fish_timer > 1.8f) { g.fishing = 0; game_toast(g, "O peixe escapou. Tente de novo com F."); } }
        g.clock_tick += dt; if (g.clock_tick >= .8f) { g.clock_tick -= .8f; g.s.minute++; if (g.s.minute >= 1320) game_rest(g); }
        g.save_tick += dt; if (g.save_tick >= 5) { game_save(g); g.save_tick = 0; }
    }
}

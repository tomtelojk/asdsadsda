using static ValeDoSol.Ray;
using static ValeDoSol.Logic;
using static ValeDoSol.Dialog;
namespace ValeDoSol;

public enum Dialog { D_NONE, D_HELP, D_NPC, D_SHOP, D_REST }
public readonly record struct PendingAction(int type = 0, int index = 0, int tool = 0, Point target = default);
public class Game
{
    public State s = new();
    public int tool, tab, hover = -1, npc = -1;
    public Dialog dialog;
    public Point mouse;
    public Point[] path = new Point[MAX_PATH], npc_position = new Point[4];
    public Point[][] npc_paths = Enumerable.Range(0, 4).Select(_ => new Point[MAX_PATH]).ToArray();
    public int path_length, path_at;
    public int[] npc_phase = new int[4], npc_length = new int[4], npc_at = new int[4];
    public PendingAction pending;
    public float elapsed, clock_tick, save_tick, toast_timer;
    public bool moving, save_ok, quit, ignore_focus;
    public int fishing;
    public float fish_timer, fish_wait;
    public string toast = "", reply = "", save_path = "", save_dir = "";
}
public static class GameLogic
{
    public const int UI_W = 1440, UI_H = 960, VIEW_X = 24, VIEW_Y = 132, VIEW_W = 1092;
    public const float VIEW_H = 686.4f, WORLD_SCALE = 1092f / 1120;
    static float distance(Point a, Point b) => MathF.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
    static void reset_npcs(Game g) { for (int i = 0; i < 4; i++) { int phase = g.s.minute >= 780 ? 1 : 0; g.npc_phase[i] = phase; g.npc_position[i] = phase == 1 ? NPCS[i].afternoon : NPCS[i].morning; g.npc_length[i] = g.npc_at[i] = 0; } }
    public static void game_toast(Game g, string message) { g.toast = message; g.toast_timer = 7; }
    public static void game_save(Game g) { g.save_ok = state_save(g.s, g.save_path); }
    public static void game_init(Game g, string dir)
    {
        g.save_dir = dir; g.save_path = Path.Combine(dir, "save.json"); bool exists = false, loaded = false;
        try { Directory.CreateDirectory(dir); exists = File.Exists(g.save_path); if (state_load(out var state, g.save_path)) { g.s = state; loaded = true; } } catch (Exception e) when (e is IOException or UnauthorizedAccessException) { g.save_path = ""; }
        reset_npcs(g);
        if (exists && !loaded) { g.save_path = ""; g.save_ok = false; game_toast(g, "Save inválido preservado. Inicie com outro diretório de save para criar uma nova partida."); }
        else { game_save(g); game_toast(g, loaded ? "Bem-vindo de volta! O vale sentiu sua falta." : "Um novo começo. Aperte H para aprender a jogar."); }
    }
    public static void game_modal(Game g, Dialog d) { g.dialog = d; g.path_length = 0; g.pending = new(); g.fishing = 0; }
    public static void game_travel(Game g) { g.s.scene = g.s.scene == FARM ? VILLAGE : FARM; g.s.player = g.s.scene == VILLAGE ? new(80, 384) : new(1040, 384); g.path_length = 0; g.pending = new(); g.hover = -1; g.fishing = 0; game_save(g); game_toast(g, g.s.scene == VILLAGE ? "Vila das Flores. Aproxime-se de alguém e aperte E." : "Lar, doce lar. Sua fazenda estava esperando."); }
    public static void game_rest(Game g) { int grown = next_day(g.s); g.s.scene = FARM; g.s.player = new(208, 288); g.path_length = 0; g.pending = new(); g.fishing = 0; g.dialog = D_NONE; g.clock_tick = 0; reset_npcs(g); game_toast(g, $"Bom dia! {grown} plantas cresceram. " + (raining(g.s) ? "A chuva está regando sua horta." : "Um novo dia está começando.")); game_save(g); }
    public static void game_converse(Game g, int npc) { if (npc < 0 || npc > 3) return; bool first = (g.s.met & (1u << npc)) == 0; talk_to(g.s, npc); g.npc = npc; g.reply = first ? NPCS[npc].first : NPCS[npc].later[g.s.day % 2]; game_modal(g, D_NPC); game_save(g); }
    static PendingAction closest(Game g)
    {
        PendingAction result = new(); float best = 76; Point p = g.s.player;
        void Consider(PendingAction a) { float d = distance(p, a.target); if (d < best) { best = d; result = a; } }
        if (g.s.scene == VILLAGE) for (int i = 0; i < 4; i++) Consider(new(1, i, 0, g.npc_position[i]));
        for (int i = 0; i < 6; i++) { var r = RESOURCES[i]; if (r.scene != g.s.scene || (g.s.gathered & (1u << i)) != 0) continue; Consider(new(2, i, 0, new(r.x * 32 + 16, r.y * 32 + 16))); }
        var buildings = g.s.scene == FARM ? FARM_BUILDINGS : VILLAGE_BUILDINGS;
        for (int i = 0; i < buildings.Length; i++) { var b = buildings[i]; if (b.type > 2) continue; Consider(new(b.type + 3, i, 0, new((b.x + b.w / 2f) * 32, (b.y + b.h) * 32 + 22))); }
        return result;
    }
    static void perform(Game g, PendingAction a)
    {
        if (a.type == 1) { game_converse(g, a.index); return; }
        if (a.type == 3) { game_modal(g, D_REST); return; }
        if (a.type == 4) { game_modal(g, D_SHOP); return; }
        if (a.type == 5) { game_converse(g, 1); return; }
        if (a.type != 2 || (g.s.gathered & (1u << a.index)) != 0) return;
        if (g.s.energy < 5) { game_toast(g, "Você precisa descansar para recuperar energia."); return; }
        if (g.s.wood > 9999997 || g.s.stone > 9999998) return;
        g.s.energy -= 5; g.s.gathered |= 1u << a.index; if (RESOURCES[a.index].type == 0) { g.s.wood += 3; game_toast(g, "+3 madeiras! Leve para Bento melhorar seu regador."); } else { g.s.stone += 2; if (g.s.coins <= 999999997) g.s.coins += 3; game_toast(g, "+2 pedras e 3 moedas encontradas entre elas."); }
        game_save(g);
    }
    public static void game_interact(Game g) { var a = closest(g); if (a.type != 0) perform(g, a); else game_toast(g, "Aproxime-se de um morador, de uma porta ou de um tronco."); }
    static bool near_pond(Game g) { float minx = (g.s.scene == FARM ? 26 : 25) * 32; var p = g.s.player; float x = Math.Clamp(p.x, minx, 1024), y = Math.Clamp(p.y, 512, 640); return distance(p, new(x, y)) < 67; }
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
        if (g.s.scene == VILLAGE) for (int i = 0; i < 4; i++) { Point at = g.npc_position[i]; if (distance(new(at.x, at.y - 12), p) < 30) { walk_to(g, at, new(1, i, 0, at)); return; } }
        for (int i = 0; i < 6; i++) { var r = RESOURCES[i]; Point at = new(r.x * 32 + 16, r.y * 32 + 16); if (r.scene == g.s.scene && (g.s.gathered & (1u << i)) == 0 && distance(at, p) < 22) { walk_to(g, at, new(2, i, 0, at)); return; } }
        var bs = g.s.scene == FARM ? FARM_BUILDINGS : VILLAGE_BUILDINGS; for (int i = 0; i < bs.Length; i++) { var b = bs[i]; if (b.type <= 2 && CheckCollisionPointRec(p, new(b.x * 32, b.y * 32, b.w * 32, b.h * 32))) { Point at = new((b.x + b.w / 2f) * 32, (b.y + b.h) * 32 + 22); walk_to(g, at, new(b.type + 3, i, 0, at)); return; } }
        int index = g.s.scene == FARM ? plot_at(p.x, p.y) : -1; if (index >= 0 && g.tool < 4) walk_to(g, p, new(6, index, g.tool, p)); else walk_to(g, p, new());
    }
    static void move_player(Game g, float dx, float dy) { if (!blocked(g.s.scene, g.s.player.x + dx, g.s.player.y)) g.s.player.x += dx; if (!blocked(g.s.scene, g.s.player.x, g.s.player.y + dy)) g.s.player.y += dy; }
    static void update_npcs(Game g, float dt) { if (g.s.scene != VILLAGE) return; for (int i = 0; i < 4; i++) { int phase = g.s.minute >= 780 ? 1 : 0; if (phase != g.npc_phase[i]) { g.npc_phase[i] = phase; g.npc_length[i] = find_path(VILLAGE, g.npc_position[i], phase == 1 ? NPCS[i].afternoon : NPCS[i].morning, g.npc_paths[i]); g.npc_at[i] = 0; } if (g.npc_at[i] >= g.npc_length[i]) continue; Point goal = g.npc_paths[i][g.npc_at[i]], p = g.npc_position[i]; float d = distance(p, goal), step = 35 * dt; if (d <= step) { g.npc_position[i] = goal; g.npc_at[i]++; } else { g.npc_position[i].x += (goal.x - p.x) / d * step; g.npc_position[i].y += (goal.y - p.y) / d * step; } } }
    public static void game_update(Game g, float dt, Point mouse)
    {
        g.mouse = mouse; g.elapsed += dt; g.toast_timer = Math.Max(0, g.toast_timer - dt); g.moving = false;
        if (IsKeyPressed(KEY_F11)) ToggleFullscreen(); if (IsKeyPressed(KEY_ESCAPE)) { if (g.dialog != D_NONE) g.dialog = D_NONE; else game_modal(g, D_HELP); g.fishing = 0; return; }
        if (g.dialog != D_NONE || (!g.ignore_focus && !IsWindowFocused())) return; if (IsKeyPressed(KEY_H)) { game_modal(g, D_HELP); return; }
        for (int i = 0; i < 5; i++) if (IsKeyPressed(KEY_ONE + i)) g.tool = i;
        if (IsKeyPressed(KEY_TAB)) g.tab = (g.tab + 1) % 3;
        if (IsKeyPressed(KEY_E) && g.fishing == 0) game_interact(g); if (g.dialog != D_NONE) return;
        if (IsKeyPressed(KEY_F)) game_fish(g); if (IsKeyPressed(KEY_SPACE)) game_catch(g);
        if (IsKeyPressed(KEY_F5)) { game_save(g); game_toast(g, g.save_ok ? "Progresso salvo no seu computador." : "Não foi possível salvar. Verifique o diretório de save."); }
        float dx = ((IsKeyDown(KEY_D) || IsKeyDown(KEY_RIGHT)) ? 1 : 0) - ((IsKeyDown(KEY_A) || IsKeyDown(KEY_LEFT)) ? 1 : 0), dy = ((IsKeyDown(KEY_S) || IsKeyDown(KEY_DOWN)) ? 1 : 0) - ((IsKeyDown(KEY_W) || IsKeyDown(KEY_UP)) ? 1 : 0);
        if (dx != 0 || dy != 0) { g.path_length = 0; g.pending = new(); g.fishing = 0; float norm = MathF.Sqrt(dx * dx + dy * dy); move_player(g, dx / norm * 145 * dt, dy / norm * 145 * dt); g.moving = true; }
        else if (g.path_at < g.path_length) { Point goal = g.path[g.path_at]; float d = distance(g.s.player, goal), step = 145 * dt; if (d <= step) { g.s.player = goal; g.path_at++; if (g.path_at == g.path_length) finish_action(g); } else { move_player(g, (goal.x - g.s.player.x) / d * step, (goal.y - g.s.player.y) / d * step); g.moving = true; } }
        if (g.dialog != D_NONE) return; if ((g.s.scene == FARM && g.s.player.x > 1090 && Math.Abs(g.s.player.y - 384) < 44) || (g.s.scene == VILLAGE && g.s.player.x < 26 && Math.Abs(g.s.player.y - 384) < 44)) game_travel(g);
        Point wp = new((mouse.x - VIEW_X) / WORLD_SCALE, (mouse.y - VIEW_Y) / WORLD_SCALE); bool inside = CheckCollisionPointRec(mouse, new(VIEW_X, VIEW_Y, VIEW_W, VIEW_H)); g.hover = inside && g.s.scene == FARM ? plot_at(wp.x, wp.y) : -1;
        if (inside && IsMouseButtonReleased(MOUSE_BUTTON_LEFT) && !CheckCollisionPointRec(mouse, new(928, 149, 166, 38))) { if (g.fishing == 2) game_catch(g); else world_click(g, wp); }
        update_npcs(g, dt); if (g.fishing != 0) { g.fish_timer += dt; if (g.fishing == 1 && g.fish_timer >= g.fish_wait) { g.fishing = 2; g.fish_timer = 0; } else if (g.fishing == 2 && g.fish_timer > 1.8f) { g.fishing = 0; game_toast(g, "Quase! O peixe escapou. Tente de novo com F."); } }
        g.clock_tick += dt; if (g.clock_tick >= .8f) { g.clock_tick -= .8f; g.s.minute++; if (g.s.minute >= 1320) game_rest(g); }
        g.save_tick += dt; if (g.save_tick >= 5) { game_save(g); g.save_tick = 0; }
    }
}

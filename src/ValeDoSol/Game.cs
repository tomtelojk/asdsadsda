using static ValeDoSol.Ray;
using static ValeDoSol.Logic;
using static ValeDoSol.Dialog;
namespace ValeDoSol;

public enum Dialog { D_NONE, D_HELP, D_NPC, D_SHOP, D_REST, D_MENU, D_PAUSE, D_TITLE, D_LETTER, D_CIVIC, D_GALLERY }
public readonly record struct PendingAction(int type = 0, int index = 0, int tool = 0, Point target = default);
public class Game
{
    public State s = new();
    public int tool, tab, hover = -1, npc = -1, facing;
    public Dialog dialog;
    public Point mouse, camera, action_target, velocity;
    public Point[] path = new Point[MAX_PATH];
    public Point[] npc_position => s.npc_positions;
    public Point[][] npc_paths = Enumerable.Range(0, Social.Count).Select(_ => new Point[MAX_PATH]).ToArray();
    public int path_length, path_at;
    public int[] npc_phase = new int[Social.Count], npc_length = new int[Social.Count], npc_at = new int[Social.Count];
    public Scene[] npc_scene => s.npc_scenes;
    public float[] npc_idle = new float[Social.Count];
    public int[] npc_facing = new int[Social.Count], npc_task_cycle = new int[Social.Count];
    public bool[] npc_arrived = new bool[Social.Count], npc_moving = new bool[Social.Count];
    public Appointment[] npc_appointment = new Appointment[Social.Count];
    public NpcPose[] npc_pose = new NpcPose[Social.Count];
    public string[] npc_task = Enumerable.Repeat("", Social.Count).ToArray(), npc_bubble = Enumerable.Repeat("", Social.Count).ToArray();
    public float[] npc_bubble_time = new float[Social.Count], npc_bubble_cooldown = new float[Social.Count];
    public PendingAction pending;
    public float elapsed, clock_tick, save_tick, toast_timer, dialog_age, location_timer = 4, fade_timer, action_timer;
    public int letter_page, journal_page, civic_page;
    public Scene map_scene = Scene.VILLAGE;
    public string letter_title = "", letter_body = "";
    public bool moving, save_ok, quit, ignore_focus;
    public int fishing;
    public float fish_timer, fish_wait;
    public string toast = "", reply = "", save_path = "", save_dir = "";
}
public static class GameLogic
{
    public const int UI_W = 1280, UI_H = 720;
    public const float CAMERA_ZOOM = 2f;
    public static Point render_camera(Game g) => new(Math.Round(g.camera.x * CAMERA_ZOOM) / CAMERA_ZOOM, Math.Round(g.camera.y * CAMERA_ZOOM) / CAMERA_ZOOM);
    public static Point camera_target(State s) { Point size = world_size(s.scene); float hw = UI_W / (2 * CAMERA_ZOOM), hh = UI_H / (2 * CAMERA_ZOOM); return new(size.x < 2 * hw ? size.x / 2 : Math.Clamp(s.player.x, hw, size.x - hw), size.y < 2 * hh ? size.y / 2 : Math.Clamp(s.player.y, hh, size.y - hh)); }
    public static Point screen_to_world(Game g, Point p) => new((p.x - UI_W / 2f) / CAMERA_ZOOM + render_camera(g).x, (p.y - UI_H / 2f) / CAMERA_ZOOM + render_camera(g).y);
    public static Point world_to_screen(Game g, Point p) => new((p.x - render_camera(g).x) * CAMERA_ZOOM + UI_W / 2f, (p.y - render_camera(g).y) * CAMERA_ZOOM + UI_H / 2f);
    public static void select_slot(Game g, int slot) { if (slot < 5) g.tool = slot; else if (slot < 8) { g.tool = 1; g.s.selected = slot - 5; } else g.tool = 5; }
    public static int active_slot(Game g) => g.tool == 5 ? 8 : g.tool == 1 ? 5 + g.s.selected : g.tool;
    public static void game_scene(Game g, Scene scene, Point at) { g.s.scene = scene; g.s.player = at; g.path_length = 0; g.pending = new(); g.fishing = 0; g.velocity = new(); g.dialog = D_NONE; g.camera = camera_target(g.s); g.location_timer = 4; g.fade_timer = .3f; game_save(g); }

    static float distance(Point a, Point b) => MathF.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
    public static void reset_npcs(Game g)
    {
        bool keep = g.s.npc_saved && g.s.npc_day == g.s.day;
        for (int i = 0; i < Social.Count; i++)
        {
            Appointment a = Social.Now(i, g.s); g.npc_phase[i] = -1;
            if (!keep) { g.npc_scene[i] = a.scene; g.npc_position[i] = Physics.Nearest(a.scene, a.at); }
            else g.npc_position[i] = Physics.Nearest(g.npc_scene[i], g.npc_position[i]);
            g.npc_length[i] = g.npc_at[i] = g.npc_task_cycle[i] = 0; g.npc_facing[i] = i % 4;
            g.npc_idle[i] = 0; g.npc_arrived[i] = g.npc_moving[i] = false; g.npc_appointment[i] = a; g.npc_pose[i] = NpcPose.Idle; g.npc_task[i] = a.activity;
            g.npc_bubble_time[i] = 0; g.npc_bubble_cooldown[i] = i * 2;
        }
        g.s.npc_saved = true; g.s.npc_day = g.s.day;
    }
    public static void safe_position(Game g) => g.s.player = Physics.Nearest(g.s.scene, g.s.player);
    public static void open_letter(Game g, string title, string body)
    {
        g.letter_title = title; g.letter_body = body; g.letter_page = 0;
        game_modal(g, D_LETTER);
    }
    public static void read_aurora(Game g)
    {
        Story.ReadLetter(g.s); game_save(g);
        open_letter(g, "Uma carta de Aurora", Story.IntroLetter);
    }
    public static void begin_journey(Game g)
    {
        if ((g.s.story_flags & Story.LetterRead) == 0) read_aurora(g);
        else g.dialog = D_NONE;
    }
    public static bool can_story_handoff(Game g)
    {
        var chapter = Story.Current(g.s);
        if (!chapter.Ready) return false;
        if (chapter.Stage == 5) return true; // The last page is read at the town square.
        int contact = chapter.ContactNpc;
        return g.npc_scene[contact] == g.s.scene && distance(g.s.player, g.npc_position[contact]) < 110;
    }
    public static void advance_story(Game g)
    {
        if (!can_story_handoff(g)) { game_toast(g, "Encontre " + Story.Current(g.s).Contact + " depois de completar o objetivo do diário."); return; }
        if (!Story.TryAdvance(g.s, out string message)) { game_toast(g, message); return; }
        game_save(g); open_letter(g, Story.Complete(g.s) ? "A próxima página é sua" : "Uma nova página", message + "\n\n" + (Story.Complete(g.s) ? Story.Ending : Story.Current(g.s).Objective));
    }
    public static void open_civic(Game g, int page = 0)
    {
        g.civic_page = page; g.map_scene = g.s.scene == FARM ? FARM : VILLAGE;
        game_modal(g, D_CIVIC);
    }
    public static void game_toast(Game g, string message) { g.toast = message; g.toast_timer = 7; }
    public static void game_save(Game g) { g.save_ok = state_save(g.s, g.save_path); }
    public static void game_init(Game g, string dir)
    {
        g.save_dir = dir; g.save_path = Path.Combine(dir, "save.json"); bool exists = false, loaded = false;
        try { Directory.CreateDirectory(dir); exists = File.Exists(g.save_path); if (state_load(out var state, g.save_path)) { g.s = state; loaded = true; using var document = System.Text.Json.JsonDocument.Parse(File.ReadAllText(g.save_path)); string backup = Path.Combine(dir, "save-before-v8.json"); if ((!document.RootElement.TryGetProperty("edition", out var oldEdition) || oldEdition.GetInt32() < 8) && !File.Exists(backup)) File.Copy(g.save_path, backup); } } catch (Exception e) when (e is IOException or UnauthorizedAccessException) { g.save_path = ""; }
        safe_position(g); reset_npcs(g); g.camera = camera_target(g.s);
        if (exists && !loaded) { g.save_path = ""; g.save_ok = false; game_toast(g, "Save inválido preservado. Inicie com outro diretório de save para criar uma nova partida."); }
        else { game_save(g); game_toast(g, loaded ? "Bem-vindo de volta! O vale sentiu sua falta." : "Sua horta fica ao sul da estrada. Use M para ver o mapa e H para os controles."); }
    }
    public static void game_modal(Game g, Dialog d) { g.dialog = d; g.dialog_age = 0; g.path_length = 0; g.pending = new(); g.fishing = 0; }
    public static void game_travel(Game g)
    {
        if (Map.Indoors(g.s.scene)) { Scene room = g.s.scene; game_scene(g, Map.Outside(room), Map.PlaceFor(room).Door); return; }
        game_scene(g, g.s.scene == FARM ? VILLAGE : FARM, g.s.scene == FARM ? new(48, 384) : new(WORLD_W - 48, 384));
    }
    public static void game_rest(Game g) { int grown = next_day(g.s); g.s.npc_saved = false; game_scene(g, HOME, new(320, 280)); g.clock_tick = 0; reset_npcs(g); game_toast(g, $"Bom dia! {grown} plantas cresceram. " + (raining(g.s) ? "A chuva está regando a horta." : "Um novo dia começa.")); }
    public static void game_converse(Game g, int npc) { if (npc < 0 || npc >= Social.Count) return; bool first = (g.s.met & (1u << npc)) == 0; talk_to(g.s, npc); g.npc = npc; g.reply = Social.Greeting(g.s, npc, first); Point delta = new(g.s.player.x - g.npc_position[npc].x, g.s.player.y - g.npc_position[npc].y); g.npc_facing[npc] = Math.Abs(delta.x) > Math.Abs(delta.y) ? (delta.x < 0 ? 1 : 2) : delta.y < 0 ? 3 : 0; game_modal(g, D_NPC); game_save(g); }
    static PendingAction closest(Game g)
    {
        PendingAction result = new(); float best = 72; Point p = g.s.player;
        // A resident passing the doorstep must not prevent entering the house.
        if (!Map.Indoors(g.s.scene)) foreach (Place place in Map.Places(g.s.scene))
            if (Math.Abs(p.x - place.Door.x) < 25 && p.y >= place.Door.y - 6 && p.y <= place.Door.y + 36)
                return new(3, (int)place.scene, 0, place.Door);
        void Consider(PendingAction a) { float d = distance(p, a.target); if (d < best) { best = d; result = a; } }
        for (int i = 0; i < Social.Count; i++) if (g.npc_scene[i] == g.s.scene) Consider(new(1, i, 0, g.npc_position[i]));
        if (g.s.scene == Scene.TOWNHALL) Consider(new(11, 0, 0, Map.NoticePoint));
        if (g.s.scene == VILLAGE) Consider(new(11, 0, 0, VillageProps.NoticePoint));
        if (g.s.scene == SHOP && g.npc_scene[0] == SHOP) Consider(new(1, 0, 0, Map.ServicePoint(SHOP)));
        if (Map.Indoors(g.s.scene))
        {
            if (g.s.scene == HOME) Consider(new(7, 0, 0, Map.BedApproach(HOME)));
            Consider(new(8, 0, 0, Map.Exit(g.s.scene)));
            if (result.type != 0) return result;
            var props = Map.Props(g.s.scene); for (int i = 0; i < props.Length; i++) { var r = props[i].bounds; if (props[i].description.Length == 0) continue; Consider(new(10, i, 0, new(Math.Clamp(p.x, r.x - 8, r.x + r.width + 8), Math.Clamp(p.y, r.y - 8, r.y + r.height + 8)))); }
            return result;
        }
        for (int i = 0; i < RESOURCES.Length; i++) { var r = RESOURCES[i]; if (r.scene != g.s.scene || (g.s.gathered & (1u << i)) != 0) continue; Consider(new(2, i, 0, new(r.x * 32 + 16, r.y * 32 + 16))); }
        foreach (Place place in Map.Places(g.s.scene)) Consider(new(3, (int)place.scene, 0, place.Door));
        if (g.s.scene == VILLAGE) for (int i = 0; i < VillageProps.All.Length; i++)
        {
            var prop = VillageProps.All[i]; if (prop.kind == VillageKind.NoticeBoard) continue;
            Rectangle b = prop.footprint;
            Consider(new(12, i, 0, new(Math.Clamp(p.x, b.x - 8, b.x + b.width + 8), Math.Clamp(p.y, b.y - 8, b.y + b.height + 8))));
        }
        return result;
    }
    static void perform(Game g, PendingAction a)
    {
        if (a.type == 12) { game_toast(g, VillageProps.All[a.index].description); return; }
        if (a.type == 11) { open_civic(g); return; }
        if (a.type == 1) { game_converse(g, a.index); return; }
        if (a.type == 3) { Scene room = (Scene)a.index; game_scene(g, room, Map.Arrival(room)); return; }
        if (a.type == 7) { game_modal(g, D_REST); return; }
        if (a.type == 8) { game_travel(g); return; }
        if (a.type == 10) { game_toast(g, Map.Props(g.s.scene)[a.index].description); game_save(g); return; }
        if (a.type != 2 || (g.s.gathered & (1u << a.index)) != 0) return;
        if (g.s.energy < 5) { game_toast(g, "Você precisa descansar para recuperar energia."); return; }
        if (g.s.wood > 9999997 || g.s.stone > 9999998) return;
        g.s.energy -= 5; g.s.gathered |= 1u << a.index; if (RESOURCES[a.index].type == 0) { g.s.wood += 3; game_toast(g, "+3 madeiras! Leve para Bento melhorar seu regador."); } else { g.s.stone += 2; if (g.s.coins <= 999999997) g.s.coins += 3; game_toast(g, "+2 pedras e 3 moedas encontradas entre elas."); }
        game_save(g);
    }
    public static void game_interact(Game g) { var a = closest(g); if (a.type != 0) perform(g, a); else game_toast(g, g.s.scene == SHOP ? "Lia está fora do balcão. O armazém atende das 8h às 17h; nos fins de semana, até 13h." : "Aproxime-se de um morador, de uma porta ou de um tronco."); }
    static bool near_pond(Game g) => water_bodies(g.s.scene).Any(w => distance(g.s.player, new(Math.Clamp(g.s.player.x, w.x, w.x + w.width), Math.Clamp(g.s.player.y, w.y, w.y + w.height))) < 67);
    public static string interaction_hint(Game g) { var a = closest(g); return a.type switch { 1 => "E  Conversar com " + NPCS[a.index].name, 2 => "E  Recolher " + (RESOURCES[a.index].type == 0 ? "madeira" : "pedra"), 3 => "E  Entrar: " + Map.Name((Scene)a.index), 7 => "E  Dormir", 8 => "E  Sair", 10 or 12 => "E  Examinar", 11 => "E  Consultar quadro da vila", _ => g.s.rod && near_pond(g) ? "F  Pescar" : "" }; }
    public static void farm_near(Game g, int index) { if (index < 0) return; Point p = plot_position(index); p.x += 16; p.y += 16; if (distance(g.s.player, p) > 76) { game_toast(g, "Chegue mais perto do canteiro para usar a ferramenta."); return; } string message = farm_action(g.s, index, g.tool, out bool ok); if (ok) { g.action_timer = .32f; g.action_target = p; game_save(g); } game_toast(g, message); }
    public static void use_tool(Game g) { if (g.tool == 5) { game_fish(g); return; } if (g.tool == 4) { game_interact(g); return; } if (g.s.scene != FARM) return; int index; { Point p = g.s.player; Point[] directions = [new(0, 32), new(-32, 0), new(32, 0), new(0, -32)]; p.x += directions[g.facing].x; p.y += directions[g.facing].y; index = plot_at(p.x, p.y); } farm_near(g, index); }

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
    public static void walk_to(Game g, Point target, PendingAction action)
    {
        g.fishing = 0;
        // The collision/navigation grid has 16px cells. Preserve free clicks exactly,
        // including the narrow outer aisles, instead of rounding them to 32px plots.
        Point goal = Physics.Nearest(g.s.scene, target);
        g.path_length = find_path(g.s.scene, g.s.player, goal, g.path);
        g.path_at = 0; g.pending = action;
        if (g.path_length == 0)
        {
            if (distance(g.s.player, goal) < 2) finish_action(g);
            else { g.pending = new(); game_toast(g, "Caminho bloqueado. Tente chegar pelo outro lado."); }
        }
    }
    static void world_click(Game g, Point p)
    {
        for (int i = 0; i < Social.Count; i++) { if (g.npc_scene[i] != g.s.scene) continue; Point at = g.npc_position[i]; if (distance(new(at.x, at.y - 12), p) < 30) { if (distance(g.s.player, at) < 90) game_converse(g, i); else game_toast(g, "Aproxime-se para conversar."); return; } }
        if (Map.Indoors(g.s.scene)) { game_interact(g); return; }
        for (int i = 0; i < 6; i++) { var r = RESOURCES[i]; Point at = new(r.x * 32 + 16, r.y * 32 + 16); if (r.scene == g.s.scene && (g.s.gathered & (1u << i)) == 0 && distance(at, p) < 22) { if (distance(g.s.player, at) < 76) perform(g, new(2, i, 0, at)); else game_toast(g, "Chegue mais perto para recolher."); return; } }
        int index = g.s.scene == FARM ? plot_at(p.x, p.y) : -1; if (index >= 0 && g.tool < 4) { farm_near(g, index); return; }
        if (g.tool == 5) { game_fish(g); return; }
        if (g.tool == 4) walk_to(g, p, new());
    }
    public static void move_player(Game g, float dx, float dy)
    {
        Point next = Physics.Move(g.s.scene, g.s.player, new(dx, dy));
        for (int i = 0; i < Social.Count; i++) if (g.npc_scene[i] == g.s.scene && distance(next, g.npc_position[i]) < 17)
        {
            Point npc = g.npc_position[i]; if (distance(next, npc) > distance(g.s.player, npc)) continue; float d = Math.Max(.01f, distance(next, npc)); Point push = new((npc.x - next.x) / d * 2, (npc.y - next.y) / d * 2); Point moved = Physics.Move(g.s.scene, npc, push);
            if (distance(moved, next) < 14) return; g.npc_position[i] = moved;
        }
        g.s.player = next;
    }
    static void npc_route(Game g, int i, Appointment activity)
    {
        Scene here = g.npc_scene[i];
        NpcTaskStep task = NpcBehavior.Step(g.s, i, activity, g.npc_task_cycle[i]);
        Point target = here == activity.scene ? task.at : Map.Portal(here, activity.scene).at;
        g.npc_task[i] = here == activity.scene ? task.label : "A caminho: " + activity.activity;
        g.npc_pose[i] = NpcPose.Idle;
        g.npc_arrived[i] = false;
        target = Physics.Nearest(here, target);
        g.npc_length[i] = find_path(here, g.npc_position[i], target, g.npc_paths[i]);
        g.npc_at[i] = 0;
    }
    static void npc_arrival(Game g, int i, Appointment activity)
    {
        NpcTaskStep task = NpcBehavior.Step(g.s, i, activity, g.npc_task_cycle[i]);
        g.npc_arrived[i] = true; g.npc_idle[i] = task.pause;
        g.npc_facing[i] = task.facing; g.npc_pose[i] = task.pose; g.npc_task[i] = task.label;
        if (g.npc_scene[i] == g.s.scene && distance(g.npc_position[i], g.s.player) < 160 && g.npc_bubble_cooldown[i] <= 0)
        {
            g.npc_bubble[i] = NpcBehavior.SmallTalk(g.s, i); g.npc_bubble_time[i] = 3.8f;
            g.npc_bubble_cooldown[i] = 22 + i * 2;
        }
    }
    static bool safe_from_player(Game g, int i, Point from, Point to) => g.npc_scene[i] != g.s.scene || distance(to, g.s.player) >= 17 || distance(to, g.s.player) > distance(from, g.s.player);
    public static void update_npcs(Game g, float dt)
    {
        for (int i = 0; i < Social.Count; i++)
        {
            g.npc_moving[i] = false;
            g.npc_bubble_time[i] = Math.Max(0, g.npc_bubble_time[i] - dt);
            g.npc_bubble_cooldown[i] = Math.Max(0, g.npc_bubble_cooldown[i] - dt);
            Appointment activity = Social.Now(i, g.s); int phase = Social.Phase(i, g.s);
            // A changed story can replace an appointment without changing its array index.
            if (phase != g.npc_phase[i] || g.npc_appointment[i].activity != null && g.npc_appointment[i] != activity)
            {
                g.npc_phase[i] = phase; g.npc_task_cycle[i] = 0; npc_route(g, i, activity);
            }
            g.npc_appointment[i] = activity;
            if (g.npc_scene[i] == g.s.scene && g.npc_bubble_cooldown[i] <= 0)
            {
                string greeting = NpcBehavior.ProximityGreeting(g.s, i, g.s.scene, g.s.player, g.npc_position[i]);
                if (greeting.Length > 0) { g.npc_bubble[i] = greeting; g.npc_bubble_time[i] = 3.8f; g.npc_bubble_cooldown[i] = 35; }
            }
            if (g.npc_at[i] >= g.npc_length[i])
            {
                if (g.npc_scene[i] != activity.scene)
                {
                    var portal = Map.Portal(g.npc_scene[i], activity.scene);
                    if (distance(g.npc_position[i], portal.at) < 10) { g.npc_scene[i] = portal.next; g.npc_position[i] = Physics.Nearest(portal.next, portal.arrival); }
                    npc_route(g, i, activity); continue;
                }
                if (!g.npc_arrived[i]) { npc_arrival(g, i, activity); continue; }
                g.npc_idle[i] -= dt;
                if (g.npc_idle[i] > 0) continue;
                g.npc_task_cycle[i]++; npc_route(g, i, activity); continue;
            }
            g.npc_pose[i] = NpcPose.Idle;
            Point goal = g.npc_paths[i][g.npc_at[i]], from = g.npc_position[i]; float d = distance(from, goal), step = Math.Min(d, (58 + i % 4 * 6) * dt);
            if (d < .15f) { g.npc_at[i]++; continue; }
            Point delta = new((goal.x - from.x) / d * step, (goal.y - from.y) / d * step); Point next = Physics.Move(g.npc_scene[i], from, delta);
            g.npc_facing[i] = NpcBehavior.FaceToward(from, goal);
            if (!safe_from_player(g, i, from, next))
            {
                Point side = new(-delta.y * 1.8f, delta.x * 1.8f);
                Point candidate = Physics.Move(g.npc_scene[i], from, side);
                if (safe_from_player(g, i, from, candidate)) next = candidate;
                else { candidate = Physics.Move(g.npc_scene[i], from, new(-side.x, -side.y)); next = safe_from_player(g, i, from, candidate) ? candidate : from; }
            }
            g.npc_moving[i] = distance(next, from) > .01f;
            g.npc_position[i] = next; if (distance(next, goal) < 1) g.npc_at[i]++;
            // Replan against changed static geometry; keep the same task instead of skipping it.
            if (distance(next, from) < .01f && dt > 0) npc_route(g, i, activity);
        }
        // Separate residents while respecting both the scenery and the player's feet.
        for (int i = 0; i < Social.Count; i++) for (int j = i + 1; j < Social.Count; j++) if (g.npc_scene[i] == g.npc_scene[j])
        {
            Point a = g.npc_position[i], b = g.npc_position[j]; float d = distance(a, b);
            if (d <= 0 || d >= 14) continue;
            Point push = new((b.x - a.x) / d * (14 - d) * .5f, (b.y - a.y) / d * (14 - d) * .5f);
            Point movedB = Physics.Move(g.npc_scene[j], b, push), movedA = Physics.Move(g.npc_scene[i], a, new(-push.x, -push.y));
            if (safe_from_player(g, j, b, movedB)) g.npc_position[j] = movedB;
            if (safe_from_player(g, i, a, movedA)) g.npc_position[i] = movedA;
        }
    }
    public static void game_update(Game g, float dt, Point mouse, bool input_edges = true)
    {
        g.mouse = mouse; g.elapsed += dt; g.toast_timer = Math.Max(0, g.toast_timer - dt); g.action_timer = Math.Max(0, g.action_timer - dt); g.location_timer = Math.Max(0, g.location_timer - dt); g.fade_timer = Math.Max(0, g.fade_timer - dt); g.moving = false;
        if (g.dialog != D_NONE) g.dialog_age += dt;
        if ((input_edges && IsKeyPressed(KEY_F11))) ToggleFullscreen();
        if (g.dialog is D_TITLE or D_LETTER)
        {
            int key = input_edges ? IsKeyPressed(KEY_ENTER) ? KEY_ENTER : IsKeyPressed(KEY_RIGHT) ? KEY_RIGHT : IsKeyPressed(KEY_LEFT) ? KEY_LEFT : IsKeyPressed(KEY_ESCAPE) ? KEY_ESCAPE : IsKeyPressed(KEY_SPACE) ? KEY_SPACE : 0 : 0;
            if (g.dialog == D_TITLE) Ui.handle_title(g, key); else Ui.handle_letter(g, key);
            return;
        }
        if ((input_edges && IsKeyPressed(KEY_ESCAPE))) { if (g.dialog != D_NONE) g.dialog = D_NONE; else game_modal(g, D_PAUSE); g.fishing = 0; return; }
        if (input_edges && IsKeyPressed(KEY_M)) { if (g.dialog == D_CIVIC) g.dialog = D_NONE; else open_civic(g, 1); return; }
        int menu = (input_edges && IsKeyPressed(KEY_I)) || (input_edges && IsKeyPressed(KEY_TAB)) ? 0 : (input_edges && IsKeyPressed(KEY_J)) ? 1 : (input_edges && IsKeyPressed(KEY_R)) ? 2 : -1;
        if (menu >= 0) { if (g.dialog == D_MENU && g.tab == menu) g.dialog = D_NONE; else { g.tab = menu; game_modal(g, D_MENU); } return; }
        if (g.dialog != D_NONE || (!g.ignore_focus && !IsWindowFocused())) return;
        if ((input_edges && IsKeyPressed(KEY_H))) { game_modal(g, D_HELP); return; }
        for (int i = 0; i < 9; i++) if ((input_edges && IsKeyPressed(KEY_ONE + i))) select_slot(g, i);
        if ((input_edges && IsKeyPressed(KEY_E)) && g.fishing == 0) game_interact(g); if (g.dialog != D_NONE) return;
        if ((input_edges && IsKeyPressed(KEY_F))) game_fish(g);
        if ((input_edges && IsKeyPressed(KEY_SPACE))) { if (g.fishing != 0) game_catch(g); else use_tool(g); }
        if ((input_edges && IsKeyPressed(KEY_F5))) { game_save(g); game_toast(g, g.save_ok ? "Progresso salvo." : "Não foi possível salvar."); }
        float dx = ((IsKeyDown(KEY_D) || IsKeyDown(KEY_RIGHT)) ? 1 : 0) - ((IsKeyDown(KEY_A) || IsKeyDown(KEY_LEFT)) ? 1 : 0), dy = ((IsKeyDown(KEY_S) || IsKeyDown(KEY_DOWN)) ? 1 : 0) - ((IsKeyDown(KEY_W) || IsKeyDown(KEY_UP)) ? 1 : 0);
        float speed = IsKeyDown(KEY_LEFT_SHIFT) ? 220 : 150;
        if (dx != 0 || dy != 0) { g.facing = dx < 0 ? 1 : dx > 0 ? 2 : dy < 0 ? 3 : 0; g.path_length = 0; g.pending = new(); g.fishing = 0; float norm = MathF.Sqrt(dx * dx + dy * dy); Point old = g.s.player; float blendSpeed = Math.Min(1, dt * 16); g.velocity.x += (dx / norm * speed - g.velocity.x) * blendSpeed; g.velocity.y += (dy / norm * speed - g.velocity.y) * blendSpeed; move_player(g, g.velocity.x * dt, g.velocity.y * dt); g.moving = distance(old, g.s.player) > .01f; }
        else if (g.path_at < g.path_length) { Point goal = g.path[g.path_at]; float d = distance(g.s.player, goal), step = speed * dt; if (d <= step && Physics.Clear(g.s.scene, g.s.player, goal)) { move_player(g, goal.x - g.s.player.x, goal.y - g.s.player.y); if (distance(g.s.player, goal) < 1) g.path_at++; if (g.path_at == g.path_length) finish_action(g); } else { float x = goal.x - g.s.player.x, y = goal.y - g.s.player.y; g.facing = Math.Abs(x) > Math.Abs(y) ? (x < 0 ? 1 : 2) : (y < 0 ? 3 : 0); Point before = g.s.player; move_player(g, x / d * step, y / d * step); g.moving = distance(before, g.s.player) > .01f; } }
        if (g.dialog != D_NONE) return;
        if (dx == 0 && dy == 0 && g.path_at >= g.path_length) g.velocity = new();
        if ((g.s.scene == FARM && g.s.player.x > WORLD_W - 28 && Math.Abs(g.s.player.y - 384) < 40) || (g.s.scene == VILLAGE && g.s.player.x < 28 && Math.Abs(g.s.player.y - 384) < 40) || (Map.Indoors(g.s.scene) && g.s.player.y > Map.Exit(g.s.scene).y && Math.Abs(g.s.player.x - Map.Exit(g.s.scene).x) < 32)) game_travel(g);
        Point target = camera_target(g.s); float blend = Math.Min(1, dt * 10); g.camera.x += (target.x - g.camera.x) * blend; g.camera.y += (target.y - g.camera.y) * blend;
        Point wp = screen_to_world(g, mouse); g.hover = g.s.scene == FARM ? plot_at(wp.x, wp.y) : -1;
        if (mouse.x >= 0 && mouse.x < UI_W && mouse.y >= 0 && mouse.y < UI_H && !Ui.hud_hit(mouse))
        {
            if ((input_edges && IsMouseButtonReleased(MOUSE_BUTTON_LEFT))) { if (g.fishing != 0) game_catch(g); else world_click(g, wp); }
            if ((input_edges && IsMouseButtonReleased(MOUSE_BUTTON_RIGHT))) walk_to(g, wp, new());
        }
        update_npcs(g, dt);
        if (g.fishing != 0) { g.fish_timer += dt; if (g.fishing == 1 && g.fish_timer >= g.fish_wait) { g.fishing = 2; g.fish_timer = 0; } else if (g.fishing == 2 && g.fish_timer > 1.8f) { g.fishing = 0; game_toast(g, "O peixe escapou. Tente de novo com F."); } }
        g.clock_tick += dt; if (g.clock_tick >= .8f) { g.clock_tick -= .8f; g.s.minute++; if (g.s.minute >= 1320) game_rest(g); }
        g.save_tick += dt; if (g.save_tick >= 5) { game_save(g); g.save_tick = 0; }
    }
}

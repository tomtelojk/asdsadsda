using static ValeDoSol.Ray;
using static ValeDoSol.Logic;
using static ValeDoSol.GameLogic;
namespace ValeDoSol;

public readonly record struct RenderItem(double y, int type, int index);
public static class World
{
    public static Color color_hex(long c) => new((byte)(c >> 16), (byte)(c >> 8), (byte)c, 255);
    static void R(double x, double y, double w, double h, long c) => DrawRectangle(x, y, w, h, color_hex(c));
    static void Label(string s, double x, double y) { int w = MeasureText(s, 10); DrawRectangle(x - w / 2 - 4, y - 2, w + 8, 15, new Color(39, 33, 23, 165)); DrawText(s, x - w / 2, y, 10, color_hex(0xffedc6)); }
    static bool Visible(Game g, double x, double y, int margin = 128) => Math.Abs(x - g.camera.x) < UI_W / (2 * CAMERA_ZOOM) + margin && Math.Abs(y - g.camera.y) < UI_H / (2 * CAMERA_ZOOM) + margin;
    static void Bubble(Game g, string message, Point p)
    {
        List<string> lines = []; string current = "";
        foreach (string word in message.Split(' ', StringSplitOptions.RemoveEmptyEntries))
        {
            string candidate = current.Length == 0 ? word : current + " " + word;
            if (current.Length > 0 && MeasureText(candidate, 9) > 158) { lines.Add(current); current = word; }
            else current = candidate;
        }
        if (current.Length > 0) lines.Add(current);
        int width = Math.Max(32, lines.Max(line => MeasureText(line, 9)) + 12), height = lines.Count * 12 + 10;
        Point camera = render_camera(g);
        double left = camera.x - UI_W / (2 * CAMERA_ZOOM), top = camera.y - UI_H / (2 * CAMERA_ZOOM);
        double x = Math.Round(Math.Clamp(p.x - width / 2f, left + 6, left + UI_W / CAMERA_ZOOM - width - 6));
        double y = Math.Round(Math.Max(top + 6, p.y - 68 - height));
        DrawRectangle(x - 1, y - 1, width + 2, height + 2, color_hex(0x68472e));
        DrawRectangle(x, y, width, height, color_hex(0xffebc0));
        DrawTriangle(new(p.x - 4, y + height), new(p.x, y + height + 5), new(p.x + 4, y + height), color_hex(0xffebc0));
        for (int i = 0; i < lines.Count; i++) DrawText(lines[i], x + 6, y + 5 + i * 12, 9, color_hex(0x55371f));
    }
    static void TaskProp(Game g, int npc, Point p)
    {
        double x = Math.Round(p.x), y = Math.Round(p.y); NpcPose pose = g.npc_pose[npc];
        if (pose == NpcPose.Fish)
        {
            int side = g.npc_facing[npc] == 1 ? -1 : 1;
            DrawLineEx(new(x + side * 8, y - 17), new(x + side * 27, y - 42), 2, color_hex(0xa78350));
            DrawLine(x + side * 27, y - 42, x + side * 44, y + 10, color_hex(0xe2cf9e));
            DrawCircle(x + side * 44, y + 10, 2, color_hex(0xc36951));
        }
        else if (pose == NpcPose.Read || pose == NpcPose.Inspect)
        {
            R(x - 10, y - 21, 20, 12, 0x755039); R(x - 9, y - 20, 8, 9, 0xe8dab4); R(x + 1, y - 20, 8, 9, 0xf1e4bc);
            R(x - 7, y - 18, 4, 1, 0xb49f7c); R(x + 3, y - 16, 4, 1, 0xb49f7c);
        }
        else if (pose == NpcPose.Carry)
        {
            R(x - 11, y - 23, 22, 15, 0x936641); R(x - 10, y - 22, 20, 3, 0xc79b60);
            R(x - 7, y - 17, 3, 8, 0xb78850); R(x + 4, y - 17, 3, 8, 0xb78850);
        }
        else if (pose == NpcPose.Work)
        {
            double lift = Math.Sin(g.elapsed * 4 + npc) * 4;
            DrawLineEx(new(x + 9, y - 14), new(x + 15, y - 30 + lift), 2, color_hex(0x997348));
            R(x + 10, y - 34 + lift, 12, 5, 0x8b9b9c);
        }
    }
    static void Person(Game g, int npc)
    {
        Point p = npc < 0 ? g.s.player : g.npc_position[npc]; int face = npc < 0 ? g.facing : g.npc_facing[npc]; bool moving = npc < 0 ? g.moving : g.npc_moving[npc];
        DrawEllipse(p.x, p.y + 1, 13, 4, new Color(29, 38, 25, 76)); Art.Person(npc, p.x, p.y, face, moving ? g.elapsed + (npc + 1) * .23 : 0);
        if (npc < 0) return;
        if (!moving) TaskProp(g, npc, p);
        if (g.dialog == Dialog.D_NONE && g.npc_bubble_time[npc] > 0) Bubble(g, g.npc_bubble[npc], p);
        else if (Physics.Distance(p, g.s.player) < 90)
        {
            Label(NPCS[npc].name, p.x, p.y - 63);
            if (!moving && g.npc_task[npc].Length > 0) Label(g.npc_task[npc], p.x, p.y + 11);
        }
    }
    static void Pond(Rectangle b, double time)
    {
        R(b.x - 5, b.y - 5, b.width + 10, b.height + 10, 0x556e3b); R(b.x - 2, b.y - 2, b.width + 4, b.height + 4, 0xa99568);
        Scenery.Tile("water", new(32, 160, 32, 32), b); DrawRectangle(b.x, b.y, b.width, b.height, new Color(66, 122, 141, 75));
        for (int i = 0; i < b.width * b.height / 1100; i++) { double x = b.x + 12 + i * 47 % ((int)b.width - 28) + Math.Sin(time * .8 + i) * 4, y = b.y + 14 + i * 37 % ((int)b.height - 24); R(x, y, 9, 1, 0x92c4bc); }
        for (int i = 0; i < b.width / 28; i++) { R(b.x + i * 28 + 9, b.y + b.height - 4, 2, 11, 0x4d7137); R(b.x + i * 28 + 7, b.y + b.height - 8, 5, 7, 0x927147); }
        R(b.x + 44, b.y - 16, 84, 23, 0x775939); for (int i = 0; i < 6; i++) { R(b.x + 46 + i * 14, b.y - 16, 12, 20, 0xbb965f); R(b.x + 48 + i * 14, b.y - 13, 8, 1, 0xd9b678); }
    }
    static void GardenFence(Rectangle fence)
    {
        // Render the same footprint that blocks feet; posts are visual height.
        bool horizontal = fence.width > fence.height;
        R(fence.x, fence.y, fence.width, fence.height, 0x62452e);
        R(fence.x + 1, fence.y + 1, fence.width - 2, fence.height - 3, 0xba8b51);
        R(fence.x + 1, fence.y + 1, fence.width - 2, 1, 0xe1bb7c);
        if (horizontal)
            for (int x = (int)fence.x; x < fence.x + fence.width; x += 32)
            {
                R(x, fence.y - 5, 6, 13, 0x735035); R(x + 1, fence.y - 5, 4, 11, 0xd1a469);
                R(x + 2, fence.y - 5, 2, 2, 0xf0ce91);
            }
        else
            for (int y = (int)fence.y; y < fence.y + fence.height; y += 32)
            {
                R(fence.x, y - 5, 8, 13, 0x735035); R(fence.x + 1, y - 5, 6, 11, 0xd1a469);
                R(fence.x + 2, y - 5, 3, 2, 0xf0ce91);
            }
    }
    static void Farm(Game g)
    {
        Rectangle garden = Garden.Bounds;
        R(garden.x - 4, garden.y - 3, garden.width + 8, garden.height + 8, 0x536f3c);
        Scenery.Tile("dirt", new(32, 160, 32, 32), garden);
        foreach (Rectangle path in Garden.Paths)
        {
            R(path.x, path.y, path.width, path.height, 0xb39b65);
            // Small, repeatable pebbles keep the aisles readable beside the soil.
            for (int x = (int)path.x + 5; x < path.x + path.width; x += 23)
                for (int y = (int)path.y + 5; y < path.y + path.height; y += 29)
                    R(x, y, 2, 1, 0xcbb784);
        }
        foreach (Rectangle bed in Garden.Beds)
        {
            R(bed.x - 2, bed.y - 2, bed.width + 4, bed.height + 5, 0x59402b);
            R(bed.x, bed.y, bed.width, bed.height, 0x8d633d);
        }
        for (int i = 0; i < g.s.plots.Length; i++)
        {
            Plot p = g.s.plots[i]; Point at = plot_position(i);
            long soil = p.tilled ? p.watered ? 0x715238 : 0xa57948 : 0x8d764b;
            R(at.x, at.y, 32, 32, soil);
            if (p.tilled)
                for (int k = 0; k < 3; k++)
                {
                    R(at.x + 2, at.y + 5 + k * 9, 28, 2, p.watered ? 0x594332 : 0x835e39);
                    R(at.x + 3, at.y + 7 + k * 9, 25, 1, p.watered ? 0x836044 : 0xc19658);
                }
            else
                for (int k = 0; k < 4; k++) R(at.x + 4 + k * 7, at.y + 6 + k * 11 % 21, 2, 2, 0xaf9564);
            if (p.crop >= 0)
            {
                int stage = p.growth == 0 ? 0 : p.growth >= CROPS[p.crop].days ? 2 : 1;
                Art.Draw(Art.Crop + p.crop * 3 + stage, at.x + 16, at.y + 29, .5);
            }
            if (g.hover == i && Physics.Distance(g.s.player, new(at.x + 16, at.y + 16)) < 78)
                DrawRectangleLinesEx(new(at.x + 1, at.y + 1, 30, 30), 1, color_hex(0xffe6b1));
        }
        foreach (Rectangle bed in Garden.Beds)
        {
            // Raised timber edge belongs to the bed's solid rectangle.
            R(bed.x, bed.y, bed.width, 2, 0xd6ad70);
            R(bed.x, bed.y, 2, bed.height, 0xb8864d);
            R(bed.x + bed.width - 2, bed.y, 2, bed.height, 0x775035);
            R(bed.x, bed.y + bed.height - 4, bed.width, 4, 0x775035);
            R(bed.x + 2, bed.y + bed.height - 4, bed.width - 4, 1, 0xb98851);
            foreach (int corner in new[] { 4, (int)bed.width - 6 }) R(bed.x + corner, bed.y + bed.height - 2, 2, 1, 0xd4b17b);
        }
        foreach (Rectangle fence in Garden.Fences) GardenFence(fence);
        Label("Horta Girassol - acesso pelos portões", garden.x + garden.width / 2, garden.y - 24);
        R(320, 286, 5, 33, 0x735539); R(304, 275, 34, 21, 0x995c41);
        R(307, 279, 28, 13, 0xc08a57); Label("VILA >", WORLD_W - 94, 328);
    }
    static void Square(Game g)
    {
        TownArt.Ground(g);
        for (int i = 0; i < 6; i++) { int x = 400 + i * 64; DrawLine(x, 328, x + 64, 334, color_hex(0x8b7655)); DrawTriangle(new(x + 10, 329), new(x + 23, 343), new(x + 36, 332), color_hex(i % 2 == 0 ? 0xc77861 : 0xd2b56e)); }
        Label("< FAZENDA", 94, 328);
    }
    static void CommunityGarden(Game g)
    {
        Rectangle b = Map.CommunityBed; bool restored = (g.s.story_flags & Story.CommunityGarden) != 0;
        R(b.x - 8, b.y - 8, b.width + 16, b.height + 16, 0xb79a62);
        R(b.x, b.y, b.width, b.height, restored ? 0x87613f : 0x8e8653);
        for (int row = 0; row < 3; row++) for (int col = 0; col < 6; col++)
        {
            int x = (int)b.x + 20 + col * 40, y = (int)b.y + 25 + row * 44;
            R(x - 15, y - 6, 30, 21, restored ? 0x674b31 : 0x79764a);
            if (restored) Art.Draw(Art.Crop + col % 3 * 3 + 2, x, y + 15, .5);
            else { R(x - 3, y - 3, 2, 10, 0x9e9760); R(x - 6, y - 3, 8, 2, 0xb1a56e); }
        }
        Label(restored ? "Horta de muitas mãos" : "A antiga horta de Aurora", b.x + b.width / 2, b.y - 14);
        if ((g.s.story_flags & Story.FestivalFinished) != 0)
            for (int i = 0; i < 7; i++) { int x = (int)b.x + i * 36; DrawLine(x, b.y - 42, x + 36, b.y - 42, color_hex(0x73543a)); DrawTriangle(new(x + 8, b.y - 42), new(x + 16, b.y - 29), new(x + 24, b.y - 42), color_hex(i % 2 == 0 ? 0xc77861 : 0xd9bf77)); }
    }
    static void Fountain(Game g)
    {
        Rectangle b = Map.Fountain; double cx = b.x + b.width / 2, cy = b.y + b.height / 2;
        DrawEllipse(cx + 3, cy + 13, 52, 25, new Color(32, 40, 29, 46));
        DrawEllipse(cx, cy + 5, 49, 27, color_hex(0x727765));
        DrawEllipse(cx, cy - 1, 49, 26, color_hex(0xc1baa0));
        DrawEllipse(cx, cy - 4, 43, 21, color_hex(0xe0d4b4));
        DrawEllipse(cx, cy - 4, 36, 16, color_hex(0x527f83));
        DrawEllipse(cx, cy - 7, 33, 12, color_hex(0x7cb6b2));
        for (int i = 0; i < 7; i++)
        {
            double x = cx - 38 + i * 12, y = cy + 16 + (i is 0 or 6 ? -7 : i is 1 or 5 ? -2 : 2);
            R(x, y, 9, 5, i % 2 == 0 ? 0x8f947f : 0xa5a58c);
            R(x, y, 9, 1, 0xd1c6a6);
        }
        // Native-size stone tiers sit inside the physical basin, sorted at its base.
        R(cx - 7, cy - 38, 14, 32, 0x8e9a84); R(cx - 5, cy - 38, 5, 29, 0xc8cbb0);
        DrawEllipse(cx, cy - 35, 19, 6, color_hex(0xa2ae93));
        DrawEllipse(cx, cy - 38, 20, 6, color_hex(0xddd8b9));
        DrawEllipse(cx, cy - 39, 14, 3, color_hex(0x78aaa6));
        R(cx - 3, cy - 53, 6, 16, 0xc1c8a8); R(cx - 2, cy - 52, 2, 14, 0xe7e1c0);
        DrawCircle(cx, cy - 54, 5, color_hex(0xd5cd9f));
        foreach (int side in new[] { -1, 1 })
        {
            Point last = new(cx + side * 13, cy - 37);
            for (int step = 1; step <= 5; step++)
            {
                Point next = new(cx + side * (13 + step * 3), cy - 37 + step * step * 1.2);
                DrawLineEx(last, next, 1, color_hex(0xb7ddcf)); last = next;
            }
        }
        for (int i = 0; i < 5; i++)
        {
            double px = cx - 27 + i * 13, py = cy - 7 + Math.Sin(g.elapsed * 2 + i) * 3;
            R(px, py, 6, 1, 0xd0e5ca);
        }
    }
    static void Interior(Game g)
    {
        Scene scene = g.s.scene; Point size = Map.Size(scene); int w = (int)size.x, h = (int)size.y;
        R(16, 16, w - 32, h - 32, 0x3c3028); Scenery.Tile("inside", new(0, 128, 32, 32), new(32, 80, w - 64, h - 112));
        Color tint = scene switch { Scene.BAKERY => new(255, 228, 199), Scene.LIBRARY => new(221, 230, 206), Scene.WORKSHOP => new(228, 217, 199), _ => WHITE };
        Scenery.Tile("inside", new(8, 16, 64, 48), new(32, 24, w - 64, 64), tint); R(32, 83, w - 64, 5, 0xd2c0a0); R(32, 88, w - 64, 5, 0x5e4a36);
        R(24, 20, 8, h - 40, 0x5a4431); R(w - 32, 20, 8, h - 40, 0x5a4431); R(32, h - 32, w - 64, 8, 0x654a32);
        for (int i = 0; i < 2; i++) { int x = 200 + i * 192; R(x - 5, 22, 74, 59, 0x5d4935); R(x, 26, 64, 48, g.s.minute >= 1080 ? 0x5a6e87 : 0x9bc0bf); R(x + 29, 26, 5, 48, 0xe1d6b1); R(x, 47, 64, 4, 0xe1d6b1); R(x - 6, 24, 9, 53, scene == Scene.BAKERY ? 0xb77e5d : 0x8b6f73); R(x + 61, 24, 9, 53, scene == Scene.BAKERY ? 0xb77e5d : 0x8b6f73); if (g.s.minute < 1080) DrawRectangle(x, 96, 64, 120, new Color(255, 227, 154, 15)); }
        if (scene == Scene.GREENHOUSE && (g.s.story_flags & Story.GreenhouseRepaired) != 0) { Label("As bancadas de Aurora voltaram a receber mudas", w / 2, 97); for (int i = 0; i < 6; i++) { R(240 + i * 32, 62, 18, 9, 0xa66e46); Art.Draw(Art.Crop + i % 3 * 3 + 1, 249 + i * 32, 68, .5); } }
        Point exit = Map.Exit(scene); R(exit.x - 30, h - 36, 60, 17, 0xb79966); R(exit.x - 36, h - 41, 72, 5, 0x735439);
        Prop[] props = Map.Props(scene); foreach (Prop prop in props.Where(p => p.kind == Furnishing.Rug)) Scenery.Furniture(prop, g.elapsed);
        List<RenderItem> items = []; for (int i = 0; i < props.Length; i++) if (props[i].kind != Furnishing.Rug) items.Add(new(props[i].bounds.y + props[i].bounds.height, 0, i)); for (int i = 0; i < Social.Count; i++) if (g.npc_scene[i] == scene) items.Add(new(g.npc_position[i].y, 1, i)); items.Add(new(g.s.player.y, 2, -1));
        foreach (RenderItem item in items.OrderBy(i => i.y)) if (item.type == 0) Scenery.Furniture(props[item.index], g.elapsed); else Person(g, item.type == 2 ? -1 : item.index);
    }
    public static void world_draw(Game g)
    {
        State s = g.s; Scene scene = s.scene; double time = g.elapsed;
        if (Map.Indoors(scene)) { Interior(g); return; }
        int minx = Math.Max(0, (int)(g.camera.x - 520) / 32), maxx = Math.Min(WORLD_W / 32, (int)(g.camera.x + 520) / 32 + 1), miny = Math.Max(0, (int)(g.camera.y - 360) / 32), maxy = Math.Min(WORLD_H / 32, (int)(g.camera.y + 360) / 32 + 1);
        R(0, 0, WORLD_W, WORLD_H, 0x75a044);
        for (int y = miny; y < maxy; y++) for (int x = minx; x < maxx; x++) { uint hash = (uint)(x * 73856093 ^ y * 19349663); Scenery.Draw("grass", new(hash % 3 * 32, 160, 32, 32), new(x * 32, y * 32, 32, 32)); }
        foreach (Rectangle road in Map.Roads(scene)) { if (road.x > g.camera.x + 560 || road.x + road.width < g.camera.x - 560 || road.y > g.camera.y + 400 || road.y + road.height < g.camera.y - 400) continue; Rectangle view = new(Math.Max(road.x, minx * 32), Math.Max(road.y, miny * 32), Math.Min(road.x + road.width, maxx * 32) - Math.Max(road.x, minx * 32), Math.Min(road.y + road.height, maxy * 32) - Math.Max(road.y, miny * 32)); if (view.width > 0 && view.height > 0) Scenery.Tile("dirt", new(32, 160, 32, 32), view); }
        foreach (Rectangle pond in water_bodies(scene)) if (Visible(g, pond.x + pond.width / 2, pond.y + pond.height / 2, 256)) Pond(pond, time);
        if (scene == FARM) Farm(g); else { Square(g); CommunityGarden(g); }
        for (int i = 0; i < RESOURCES.Length; i++) { Resource res = RESOURCES[i]; if (res.scene == scene && (s.gathered & (1u << i)) == 0) Art.Draw(res.type == 0 ? Art.Log : Art.Rock, res.x * 32 + 16, res.y * 32 + 16, .58); }
        for (int i = 0; i < 90; i++) { int x = 80 + i * 227 % 1880, y = 176 + i * 191 % 1120; if (Visible(g, x, y) && !blocked(scene, x, y) && (scene != FARM || plot_at(x, y) < 0) && !Map.Roads(scene).Any(r => Map.Touch(new(x - 12, y - 12, 24, 24), r))) Art.Draw(i % 4 == 0 ? Art.Bush : Art.Flowers, x, y, i % 4 == 0 ? .55 : .4); }
        int[][] trees = Map.Trees(scene); List<RenderItem> items = []; for (int i = 0; i < trees.Length; i++) if (Visible(g, trees[i][0] * 32 + 16, trees[i][1] * 32 + 16)) items.Add(new(trees[i][1] * 32 + 16, 0, i));
        if (scene == VILLAGE)
        {
            items.Add(new(Map.Fountain.y + Map.Fountain.height, 4, 0));
            for (int i = 0; i < VillageProps.All.Length; i++)
            {
                Rectangle f = VillageProps.All[i].footprint;
                if (Visible(g, f.x + f.width / 2, f.y + f.height / 2)) items.Add(new(f.y + f.height, 5, i));
            }
        }
        Place[] places = Map.Places(scene).ToArray(); for (int i = 0; i < places.Length; i++) if (Visible(g, places[i].Door.x, places[i].Door.y, places[i].building.type == 7 ? 360 : 256)) items.Add(new(places[i].Door.y - 16, 3, i));
        for (int i = 0; i < Social.Count; i++) if (g.npc_scene[i] == scene && Visible(g, g.npc_position[i].x, g.npc_position[i].y)) items.Add(new(g.npc_position[i].y, 1, i)); items.Add(new(s.player.y, 2, -1));
        foreach (RenderItem item in items.OrderBy(i => i.y)) { if (item.type == 0) { int x = trees[item.index][0] * 32 + 16, y = trees[item.index][1] * 32 + 16; bool obscures = Math.Abs(x - s.player.x) < 45 && s.player.y < y && s.player.y > y - 112; Scenery.Tree(x, y, item.index, new Color(255, 255, 255, obscures ? 100 : 255)); } else if (item.type == 5) TownArt.DrawProp(g, VillageProps.All[item.index]); else if (item.type == 4) Fountain(g); else if (item.type == 3) Scenery.House(places[item.index].building, s.minute >= 1080); else Person(g, item.type == 2 ? -1 : item.index); }
        if (g.action_timer > 0) { double phase = 1 - g.action_timer / .32; Point p = g.action_target; if (g.tool == 2) for (int i = 0; i < 5; i++) R(p.x - 12 + i * 6, p.y - 20 + phase * 24 + i % 2 * 3, 2, 4, 0x8dcbd2); else { DrawLineEx(new(s.player.x + 9, s.player.y - 18), new(p.x - 8, p.y - 10 - Math.Sin(phase * Math.PI) * 18), 3, color_hex(0x8e633c)); R(p.x - 14, p.y - 12 - Math.Sin(phase * Math.PI) * 18, 14, 5, 0xa4b1a4); } }
        float evening = Math.Clamp((s.minute - 1020) / 550f, 0, .5f); if (evening > 0) DrawRectangle(0, 0, WORLD_W, WORLD_H, new Color(28, 32, 60, (int)(255 * evening)));
        if (raining(s)) { DrawRectangle(0, 0, WORLD_W, WORLD_H, new Color(70, 92, 117, 42)); for (int i = 0; i < 150; i++) DrawRectangle(g.camera.x - 480 + (i * 149 + time * 70) % 960, g.camera.y - 280 + (i * 83 + time * 360) % 560, 1, 8, new Color(190, 218, 224, 130)); }
        if (g.fishing != 0) { Point p = s.player; DrawLineEx(new(p.x + 12, p.y - 20), new(p.x + 40, p.y - 46), 2, color_hex(0xc4a06c)); DrawLine(p.x + 40, p.y - 46, p.x + 66, p.y + 16, color_hex(0xe4cf9c)); DrawCircle(p.x + 66, p.y + 16, 3, color_hex(g.fishing == 2 ? 0xf1d18a : 0xd8997b)); }
    }
}

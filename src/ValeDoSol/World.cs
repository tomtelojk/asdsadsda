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
    static void Person(Game g, int npc)
    {
        Point p = npc < 0 ? g.s.player : g.npc_position[npc]; int face = npc < 0 ? g.facing : g.npc_facing[npc]; bool moving = npc < 0 ? g.moving : g.npc_at[npc] < g.npc_length[npc];
        DrawEllipse(p.x, p.y + 1, 13, 4, new Color(29, 38, 25, 76)); Art.Person(npc, p.x, p.y, face, moving ? g.elapsed + (npc + 1) * .23 : 0);
        if (npc >= 0 && Physics.Distance(p, g.s.player) < 90) Label(NPCS[npc].name, p.x, p.y - 65);
        if (npc == 2 && !moving && g.s.scene == VILLAGE && p.y > 448) { DrawLineEx(new(p.x + 8, p.y - 17), new(p.x + 27, p.y - 42), 2, color_hex(0xa78350)); DrawLine(p.x + 27, p.y - 42, p.x + 44, p.y + 10, color_hex(0xe2cf9e)); }
    }
    static void Pond(Rectangle b, double time)
    {
        R(b.x - 5, b.y - 5, b.width + 10, b.height + 10, 0x556e3b); R(b.x - 2, b.y - 2, b.width + 4, b.height + 4, 0xa99568);
        Scenery.Tile("water", new(32, 160, 32, 32), b); DrawRectangle(b.x, b.y, b.width, b.height, new Color(66, 122, 141, 75));
        for (int i = 0; i < b.width * b.height / 1100; i++) { double x = b.x + 12 + i * 47 % ((int)b.width - 28) + Math.Sin(time * .8 + i) * 4, y = b.y + 14 + i * 37 % ((int)b.height - 24); R(x, y, 9, 1, 0x92c4bc); }
        for (int i = 0; i < b.width / 28; i++) { R(b.x + i * 28 + 9, b.y + b.height - 4, 2, 11, 0x4d7137); R(b.x + i * 28 + 7, b.y + b.height - 8, 5, 7, 0x927147); }
        R(b.x + 44, b.y - 16, 84, 23, 0x775939); for (int i = 0; i < 6; i++) { R(b.x + 46 + i * 14, b.y - 16, 12, 20, 0xbb965f); R(b.x + 48 + i * 14, b.y - 13, 8, 1, 0xd9b678); }
    }
    static void Farm(Game g)
    {
        R(532, 244, 276, 212, 0x745131);
        for (int i = 0; i < 48; i++) { Plot p = g.s.plots[i]; Point at = plot_position(i); R(at.x, at.y, 32, 32, p.tilled ? (p.watered ? 0x715238 : 0xa57948) : 0x769747); if (!p.tilled) Scenery.Draw("grass", new(32, 160, 32, 32), new(at.x, at.y, 32, 32)); if (p.tilled) for (int k = 0; k < 3; k++) { R(at.x + 2, at.y + 5 + k * 9, 28, 2, p.watered ? 0x594332 : 0x835e39); R(at.x + 3, at.y + 7 + k * 9, 25, 1, p.watered ? 0x836044 : 0xc19658); } if (p.crop >= 0) { int stage = p.growth == 0 ? 0 : p.growth >= CROPS[p.crop].days ? 2 : 1; Art.Draw(Art.Crop + p.crop * 3 + stage, at.x + 16, at.y + 29, .62); } if (g.hover == i && Physics.Distance(g.s.player, new(at.x + 16, at.y + 16)) < 78) DrawRectangleLinesEx(new(at.x + 1, at.y + 1, 30, 30), 1, color_hex(0xffe6b1)); }
        for (int i = 0; i < 5; i++) { Art.Draw(Art.Fence, 550 + i * 60, 236, .65); Art.Draw(Art.Fence, 550 + i * 60, 488, .65); }
        R(320, 286, 5, 33, 0x735539); R(304, 275, 34, 21, 0x995c41); R(307, 279, 28, 13, 0xc08a57); Label("VILA >", WORLD_W - 94, 328);
    }
    static void Square(Game g)
    {
        Scenery.Tile("dirt", new(32, 160, 32, 32), new(432, 432, 288, 192));
        Rectangle f = Map.Fountain; R(f.x - 5, f.y, f.width + 10, f.height + 6, 0x696d61); R(f.x, f.y - 5, f.width, f.height, 0xc5c3a6); R(f.x + 8, f.y + 4, f.width - 16, f.height - 14, 0x669ca7); R(f.x + 42, f.y - 27, 13, 48, 0xa7ad99); R(f.x + 35, f.y - 28, 27, 6, 0xe1ddbf); for (int i = 0; i < 4; i++) R(f.x + 44 + i * 2, f.y - 21 + Math.Sin(g.elapsed * 3 + i) * 4, 2, 10, 0xb9d5d0);
        foreach (Point p in new[] { new Point(432, 576), new Point(656, 576), new Point(432, 448) }) { R(p.x - 28, p.y - 14, 56, 8, 0x7b5738); R(p.x - 28, p.y - 4, 56, 5, 0xb58b50); R(p.x - 24, p.y + 2, 5, 10, 0x62482f); R(p.x + 19, p.y + 2, 5, 10, 0x62482f); }
        for (int i = 0; i < 6; i++) { int x = 400 + i * 64; DrawLine(x, 328, x + 64, 334, color_hex(0x8b7655)); DrawTriangle(new(x + 10, 329), new(x + 23, 343), new(x + 36, 332), color_hex(i % 2 == 0 ? 0xc77861 : 0xd2b56e)); }
        for (int i = 0; i < 7; i++) { R(680 + i * 15, 248, 13, 19, 0x876341); Art.Draw(Art.Crop + 1, 686 + i * 15, 265, .35); }
        Label("< FAZENDA", 94, 328);
    }
    static void Interior(Game g)
    {
        Scene scene = g.s.scene; Point size = Map.Size(scene); int w = (int)size.x, h = (int)size.y;
        R(16, 16, w - 32, h - 32, 0x3c3028); Scenery.Tile("inside", new(0, 128, 32, 32), new(32, 80, w - 64, h - 112));
        Color tint = scene switch { Scene.BAKERY => new(255, 228, 199), Scene.LIBRARY => new(221, 230, 206), Scene.WORKSHOP => new(228, 217, 199), _ => WHITE };
        Scenery.Tile("inside", new(8, 16, 64, 48), new(32, 24, w - 64, 64), tint); R(32, 83, w - 64, 5, 0xd2c0a0); R(32, 88, w - 64, 5, 0x5e4a36);
        R(24, 20, 8, h - 40, 0x5a4431); R(w - 32, 20, 8, h - 40, 0x5a4431); R(32, h - 32, w - 64, 8, 0x654a32);
        for (int i = 0; i < 2; i++) { int x = 200 + i * 192; R(x - 5, 22, 74, 59, 0x5d4935); R(x, 26, 64, 48, g.s.minute >= 1080 ? 0x5a6e87 : 0x9bc0bf); R(x + 29, 26, 5, 48, 0xe1d6b1); R(x, 47, 64, 4, 0xe1d6b1); R(x - 6, 24, 9, 53, scene == Scene.BAKERY ? 0xb77e5d : 0x8b6f73); R(x + 61, 24, 9, 53, scene == Scene.BAKERY ? 0xb77e5d : 0x8b6f73); if (g.s.minute < 1080) DrawRectangle(x, 96, 64, 120, new Color(255, 227, 154, 15)); }
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
        if (scene == FARM) Farm(g); else Square(g);
        for (int i = 0; i < RESOURCES.Length; i++) { Resource res = RESOURCES[i]; if (res.scene == scene && (s.gathered & (1u << i)) == 0) Art.Draw(res.type == 0 ? Art.Log : Art.Rock, res.x * 32 + 16, res.y * 32 + 16, .58); }
        for (int i = 0; i < 90; i++) { int x = 80 + i * 227 % 1880, y = 176 + i * 191 % 1120; if (Visible(g, x, y) && !blocked(scene, x, y) && (scene != FARM || plot_at(x, y) < 0) && !Map.Roads(scene).Any(r => Map.Touch(new(x - 12, y - 12, 24, 24), r))) Art.Draw(i % 4 == 0 ? Art.Bush : Art.Flowers, x, y, i % 4 == 0 ? .55 : .4); }
        int[][] trees = Map.Trees(scene); List<RenderItem> items = []; for (int i = 0; i < trees.Length; i++) if (Visible(g, trees[i][0] * 32 + 16, trees[i][1] * 32 + 16)) items.Add(new(trees[i][1] * 32 + 16, 0, i));
        Place[] places = Map.Places(scene).ToArray(); for (int i = 0; i < places.Length; i++) if (Visible(g, places[i].Door.x, places[i].Door.y, 256)) items.Add(new(places[i].Door.y - 16, 3, i));
        for (int i = 0; i < Social.Count; i++) if (g.npc_scene[i] == scene && Visible(g, g.npc_position[i].x, g.npc_position[i].y)) items.Add(new(g.npc_position[i].y, 1, i)); items.Add(new(s.player.y, 2, -1));
        foreach (RenderItem item in items.OrderBy(i => i.y)) { if (item.type == 0) { int x = trees[item.index][0] * 32 + 16, y = trees[item.index][1] * 32 + 16; bool obscures = Math.Abs(x - s.player.x) < 45 && s.player.y < y && s.player.y > y - 112; Scenery.Tree(x, y, item.index, new Color(255, 255, 255, obscures ? 100 : 255)); } else if (item.type == 3) Scenery.House(places[item.index].building, s.minute >= 1080); else Person(g, item.type == 2 ? -1 : item.index); }
        if (g.action_timer > 0) { double phase = 1 - g.action_timer / .32; Point p = g.action_target; if (g.tool == 2) for (int i = 0; i < 5; i++) R(p.x - 12 + i * 6, p.y - 20 + phase * 24 + i % 2 * 3, 2, 4, 0x8dcbd2); else { DrawLineEx(new(s.player.x + 9, s.player.y - 18), new(p.x - 8, p.y - 10 - Math.Sin(phase * Math.PI) * 18), 3, color_hex(0x8e633c)); R(p.x - 14, p.y - 12 - Math.Sin(phase * Math.PI) * 18, 14, 5, 0xa4b1a4); } }
        float evening = Math.Clamp((s.minute - 1020) / 550f, 0, .5f); if (evening > 0) DrawRectangle(0, 0, WORLD_W, WORLD_H, new Color(28, 32, 60, (int)(255 * evening)));
        if (raining(s)) { DrawRectangle(0, 0, WORLD_W, WORLD_H, new Color(70, 92, 117, 42)); for (int i = 0; i < 150; i++) DrawRectangle(g.camera.x - 480 + (i * 149 + time * 70) % 960, g.camera.y - 280 + (i * 83 + time * 360) % 560, 1, 8, new Color(190, 218, 224, 130)); }
        if (g.fishing != 0) { Point p = s.player; DrawLineEx(new(p.x + 12, p.y - 20), new(p.x + 40, p.y - 46), 2, color_hex(0xc4a06c)); DrawLine(p.x + 40, p.y - 46, p.x + 66, p.y + 16, color_hex(0xe4cf9c)); DrawCircle(p.x + 66, p.y + 16, 3, color_hex(g.fishing == 2 ? 0xf1d18a : 0xd8997b)); }
    }
}

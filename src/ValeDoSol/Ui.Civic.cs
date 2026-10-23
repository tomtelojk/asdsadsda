using static ValeDoSol.Ray;
using static ValeDoSol.Logic;
using static ValeDoSol.World;
using static ValeDoSol.GameLogic;
using static ValeDoSol.Dialog;
namespace ValeDoSol;

public static partial class Ui
{
    static void civic_fill(Rectangle r, Color c) => DrawRectangle(r.x, r.y, r.width, r.height, c);
    static void civic_text(string value, int x, ref int y, int width, int size = 18, long color = 0x55371f)
    {
        foreach (string line in TextLayout.Wrap(value, width, v => MeasureText(v, size))) { text(line, x, y, size, color); y += size + 7; }
    }
    static void civic_notices(Game g)
    {
        var chapter = Story.Current(g.s);
        frame(new(164, 214, 606, 338));
        text("O vale é cuidado por quem vive aqui", 184, 233, 23);
        int y = 275;
        civic_text("A prefeitura guarda os registros da vila e recebe os planos dos moradores. Otávio organiza os arquivos nas manhãs de segunda, quarta e sexta. Dora e Ceci também passam por aqui para preparar os encontros.", 185, ref y, 559);
        y += 18; text("No caderno de Aurora", 185, y, 20, 0x89623a); y += 34;
        civic_text(chapter.Title + ": " + chapter.Objective, 185, ref y, 559);
        y += 12; civic_text(chapter.Progress, 185, ref y, 559, 16, 0x6b713c);
        frame(new(792, 214, 323, 338));
        text("Anotações da vila", 811, 235, 22);
        y = 277;
        civic_text("Horta da fazenda: ao sul da estrada, pelos portões. Trabalhe pelas laterais dos canteiros.", 812, ref y, 280, 17);
        y += 20; civic_text("Armazém: Lia atende até 17h nos dias úteis e até 13h no fim de semana.", 812, ref y, 280, 17);
        y += 20; civic_text("A prefeitura e as casas podem ser visitadas. Respeite os móveis, os jardins e os caminhos.", 812, ref y, 280, 17);
        if (button(g, new(164, 576, 453, 37), "Abrir o caderno de Aurora [J]")) { g.tab = 1; game_modal(g, D_MENU); }
        if (button(g, new(638, 576, 477, 37), "Ver lugares e caminhos")) g.civic_page = 1;
    }
    static void civic_map(Game g)
    {
        Scene scene = g.map_scene; float scale = .275f; Point origin = new(177, 224);
        Rectangle mini(Rectangle r) => new(origin.x + r.x * scale, origin.y + r.y * scale, r.width * scale, r.height * scale);
        Point at(Point p) => new(origin.x + p.x * scale, origin.y + p.y * scale);
        DrawRectangle(171, 218, WORLD_W * scale + 12, WORLD_H * scale + 12, color_hex(0x593e28));
        DrawRectangle(origin.x, origin.y, WORLD_W * scale, WORLD_H * scale, color_hex(0x73965b));
        foreach (Rectangle r in Map.Roads(scene)) civic_fill(mini(r), color_hex(0xc1aa77));
        foreach (Rectangle r in water_bodies(scene)) civic_fill(mini(r), color_hex(0x669cb1));
        foreach (int[] tree in Map.Trees(scene)) { Point p = at(new(tree[0] * 32 + 16, tree[1] * 32 + 16)); DrawCircle(p.x, p.y - 5, 5, color_hex(0x486e3f)); }
        if (scene == FARM)
        {
            civic_fill(mini(Garden.Bounds), color_hex(0xd6bc87));
            foreach (Rectangle bed in Garden.Beds) civic_fill(mini(bed), color_hex(0x785333));
        }
        else
        {
            civic_fill(mini(Map.Fountain), color_hex(0x98c0c0));
            civic_fill(mini(Map.CommunityBed), color_hex(0xa19b50));
            foreach (var prop in VillageProps.All) civic_fill(mini(prop.footprint), color_hex(0x624f32));
        }
        Place[] places = Map.Places(scene).ToArray();
        int index = 0;
        foreach (Place place in places)
        {
            Building b = place.building; Rectangle r = mini(new(b.x * 32, b.y * 32, b.w * 32, b.h * 32));
            DrawRectangle(r.x + 2, r.y + 3, r.width, r.height, color_hex(0x53613c)); civic_fill(r, color_hex(b.type == 7 ? 0xd9b977 : 0xc28964));
            Point door = at(place.Door); DrawRectangle(door.x - 3, door.y - 3, 6, 6, color_hex(0xffeac0));
            DrawText((++index).ToString(), r.x + r.width / 2 - 5, r.y + r.height / 2 - 8, 16, color_hex(0x4e3523));
        }
        for (int i = 0; i < Social.Count; i++) if (g.npc_scene[i] == scene)
        {
            Point p = at(g.npc_position[i]); DrawCircle(p.x, p.y, 4, color_hex(0x493c30)); DrawCircle(p.x, p.y, 2, color_hex(Art.Coats[i + 1]));
            if (Physics.Distance(g.mouse, p) < 9) { string name = NPCS[i].name; DrawRectangle(p.x + 6, p.y - 21, MeasureText(name, 16) + 12, 23, color_hex(0xffebbd)); text(name, p.x + 12, p.y - 18, 16); }
        }
        if (g.s.scene == scene)
        {
            Point player = at(g.s.player); DrawCircle(player.x, player.y, 6, color_hex(0xffecc3)); DrawCircle(player.x, player.y, 3, color_hex(0x9a4b3e));
        }
        int x = 776, y = 239;
        text(scene == FARM ? "Fazenda Girassol" : "Vila das Flores", x, y, 24); y += 45;
        for (int i = 0; i < places.Length; i++) { text($"{i + 1}. {places[i].building.name}", x, y, 17); y += 27; }
        y += 13; civic_text(scene == FARM ? "A horta fica abaixo da estrada. Os corredores permitem cuidar de todas as plantas sem subir nos canteiros." : "Prefeitura: sudeste da praça.", x, ref y, 333, 17);
        y += 16; civic_text("Ponto claro: você. Pontos coloridos: moradores nesta área. Passe o mouse para ver seus nomes.", x, ref y, 333, 16, 0x89623a);
        if (button(g, new(177, 177, 263, 29), "Fazenda", true, scene == FARM)) g.map_scene = FARM;
        if (button(g, new(456, 177, 284, 29), "Vila", true, scene == VILLAGE)) g.map_scene = VILLAGE;
    }
    static void civic_neighbors(Game g)
    {
        for (int i = 0; i < Social.Count; i++)
        {
            int x = 165 + i % 2 * 486, y = 218 + i / 2 * 95;
            frame(new(x, y, 465, 85)); portrait(i, x + 7, y + 5, 1.45f);
            text(NPCS[i].name, x + 84, y + 7, 22);
            string place = Map.Name(g.npc_scene[i]);
            text(place, x + 84, y + 35, 15, 0x775437);
            string activity = g.npc_task[i].Length == 0 ? Social.Now(i, g.s).activity : g.npc_task[i];
            var lines = TextLayout.Wrap(activity, 365, v => MeasureText(v, 13));
            text(lines[0], x + 84, y + 60, 13, 0x89623a);
        }
    }
    public static void draw_civic(Game g)
    {
        frame(new(135, 66, 1010, 588)); text("Prefeitura · Vila das Flores", 164, 91, 30);
        if (button(g, new(1090, 90, 27, 30), "X")) { g.dialog = D_NONE; return; }
        string[] tabs = ["Avisos", "Mapa do vale [M]", "Moradores agora"];
        for (int i = 0; i < tabs.Length; i++) if (button(g, new(164 + i * 321, 135, 308, 34), tabs[i], true, g.civic_page == i)) g.civic_page = i;
        if (g.civic_page == 0) civic_notices(g);
        else if (g.civic_page == 1) civic_map(g);
        else civic_neighbors(g);
        text("O tempo pausa enquanto você consulta o quadro. Esc para voltar.", 164, 627, 15, 0x89623a);
    }
}

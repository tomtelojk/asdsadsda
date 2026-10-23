using static ValeDoSol.Ray;
using static ValeDoSol.GameLogic;
using static ValeDoSol.Dialog;
namespace ValeDoSol;

public static partial class Ui
{
    public static void draw_gallery(Game g)
    {
        frame(new(66, 24, 1148, 672));
        text("Rostos do vale", 94, 44, 31);
        if (button(g, new(1154, 44, 32, 32), "X")) { g.tab = 2; game_modal(g, D_MENU); return; }
        for (int npc = 0; npc < Social.Count; npc++)
        {
            int x = 91 + npc % 4 * 275, y = 93 + npc / 4 * 281;
            frame(new(x, y, 255, 267));
            Portraits.Draw(npc, new(x + 13, y + 3, 229, 229));
            string name = Logic.NPCS[npc].name;
            text(name, x + (255 - MeasureText(name, 21)) / 2, y + 235, 21);
        }
        text("Cada pessoa tem uma história. Conheça os moradores pelo vale. Esc para voltar ao jogo.", 94, 667, 17);
    }
}

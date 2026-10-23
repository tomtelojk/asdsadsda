using static ValeDoSol.Ray;
using static ValeDoSol.World;
using static ValeDoSol.GameLogic;
using static ValeDoSol.Dialog;
namespace ValeDoSol;

public static partial class Ui
{
    const int LetterWidth = 768, LetterLines = 14;
    static IReadOnlyList<IReadOnlyList<string>> letter_pages(Game g) => TextLayout.Pages(g.letter_body, LetterWidth, LetterLines, value => MeasureText(value, 20));
    static int measured_lines(string value, int x, int y, int width, int size, int lineHeight, long color = 0x55371f)
    {
        foreach (string line in TextLayout.Wrap(value, width, v => MeasureText(v, size))) { text(line, x, y, size, color); y += lineHeight; }
        return y;
    }
    public static void handle_title(Game g, int key)
    {
        if (g.dialog != D_TITLE || g.dialog_age < .16f) return;
        if (key is KEY_ENTER or KEY_SPACE) begin_journey(g);
        else if (key == KEY_ESCAPE) g.quit = true;
    }
    public static void handle_letter(Game g, int key)
    {
        if (g.dialog != D_LETTER || g.dialog_age < .16f) return;
        int count = letter_pages(g).Count;
        if (key == KEY_ESCAPE) { g.dialog = D_NONE; return; }
        if (key == KEY_LEFT) g.letter_page = Math.Max(0, g.letter_page - 1);
        else if (key == KEY_RIGHT) g.letter_page = Math.Min(count - 1, g.letter_page + 1);
        else if (key is KEY_ENTER or KEY_SPACE) { if (g.letter_page + 1 < count) g.letter_page++; else g.dialog = D_NONE; }
    }
    public static void draw_title(Game g)
    {
        DrawRectangle(0, 0, UI_W, UI_H, new Color(24, 38, 26, 105));
        frame(new(323, 128, 634, 465));
        text("Vale do Sol", 640 - MeasureText("Vale do Sol", 58) / 2, 181, 58);
        text(Story.Title, 640 - MeasureText(Story.Title, 25) / 2, 260, 25, 0x88613a);
        measured_lines("Uma fazenda para recomeçar. Uma vila para conhecer.\nUm caderno que ainda tem páginas em branco.", 378, 319, 530, 19, 29);
        bool continuing = (g.s.story_flags & Story.LetterRead) != 0;
        if (button(g, new(432, 424, 416, 52), continuing ? "Continuar jornada [Enter]" : "Começar jornada [Enter]")) { begin_journey(g); return; }
        if (button(g, new(432, 492, 198, 36), "Controles")) game_modal(g, D_HELP);
        if (button(g, new(650, 492, 198, 36), "Sair")) g.quit = true;
        text("Seu progresso é salvo automaticamente.", 640 - MeasureText("Seu progresso é salvo automaticamente.", 16) / 2, 557, 16, 0x846044);
    }
    public static void draw_letter(Game g)
    {
        frame(new(210, 65, 860, 590));
        text(g.letter_title, 253, 97, 29);
        DrawRectangle(252, 143, 776, 2, color_hex(0xb78a54));
        var pages = letter_pages(g);
        g.letter_page = Math.Clamp(g.letter_page, 0, pages.Count - 1);
        int y = 167;
        foreach (string line in pages[g.letter_page]) { text(line, 255, y, 20); y += 27; }
        text($"Página {g.letter_page + 1} de {pages.Count}", 564, 570, 17, 0x88613a);
        if (button(g, new(250, 602, 202, 32), "Anterior [<]", g.letter_page > 0)) g.letter_page--;
        if (button(g, new(468, 602, 304, 32), g.letter_page + 1 < pages.Count ? "Próxima [Enter / >]" : "Guardar a carta [Enter]")) { if (g.letter_page + 1 < pages.Count) g.letter_page++; else g.dialog = D_NONE; }
        if (button(g, new(788, 602, 242, 32), "Fechar [Esc]")) g.dialog = D_NONE;
    }
    public static void draw_journal(Game g, int x, int y)
    {
        StoryProgress chapter = Story.Current(g.s); bool complete = Story.Complete(g.s);
        text(Story.Title, x + 28, y + 71, 20, 0x89623a);
        if (button(g, new(x + 660, y + 65, 208, 30), "Pedidos da vila")) { g.journal_page = 1; return; }
        text(complete ? chapter.Title : $"{chapter.Stage + 1}/{Story.ChapterCount}  {chapter.Title}", x + 28, y + 112, 27);
        int top = measured_lines(chapter.Summary, x + 30, y + 154, 834, 18, 24);
        top += 12; text("Seu próximo passo", x + 30, top, 19, 0x89623a);
        top = measured_lines(chapter.Objective, x + 30, top + 29, 830, 19, 24);
        top += 13;
        top = measured_lines(chapter.Progress, x + 30, top, 832, 17, 23, chapter.Ready ? 0x506f36 : 0x715032);
        if (!complete)
        {
            top += 12;
            string contact = chapter.Stage == 5 ? "Encontro na praça da Vila das Flores" : $"{chapter.Contact}: {Map.Name(Social.Now(chapter.ContactNpc, g.s).scene)} | {Social.Now(chapter.ContactNpc, g.s).activity}.";
            top = measured_lines(contact, x + 30, top, 830, 17, 22, 0x715032);
            measured_lines("Recompensa: " + chapter.Reward, x + 30, top + 8, 830, 16, 20, 0x89623a);
        }
        else measured_lines(chapter.Reward, x + 30, top + 14, 830, 17, 22, 0x715032);
        int footer = y + 484;
        if (button(g, new(x + 30, footer, 250, 36), "Reler carta de Aurora")) { read_aurora(g); return; }
        if (complete)
        {
            if (button(g, new(x + 300, footer, 568, 36), "Reler o desfecho")) open_letter(g, "A próxima página é sua", Story.Ending);
        }
        else if (button(g, new(x + 300, footer, 568, 36), chapter.Ready && !can_story_handoff(g) ? $"Aproxime-se de {chapter.Contact}" : "Continuar história", can_story_handoff(g))) advance_story(g);
    }
}

namespace ValeDoSol;

public static class TextLayoutTests
{
    public static void Run(Action<bool, string> check)
    {
        int Width(string s) => System.Globalization.StringInfo.ParseCombiningCharacters(s).Length;
        var lines = TextLayout.Wrap("A casa das flores voltou a receber visitas.", 14, Width);
        check(lines.All(s => Width(s) <= 14) && string.Join(" ", lines) == "A casa das flores voltou a receber visitas.", "quebra medida preserva palavras da carta");
        var paragraphs = TextLayout.Wrap("Uma carta.\r\n\r\nCom amor,\nAurora", 24, Width);
        check(paragraphs.SequenceEqual(new[] { "Uma carta.", "", "Com amor,", "Aurora" }), "carta conserva parágrafos e normaliza quebras");
        var longWord = TextLayout.Wrap("Paralelepípedo", 4, Width);
        check(longWord.All(s => Width(s) <= 4) && string.Concat(longWord) == "Paralelepípedo", "palavra longa não ultrapassa a moldura");
        var unicode = TextLayout.Wrap("cafe\u0301😀cafe\u0301", 3, Width);
        check(string.Concat(unicode) == "cafe\u0301😀cafe\u0301" && unicode.All(s => Width(s) <= 3), "quebra conserva acentos combinados e caracteres completos");
        var pages = TextLayout.Pages(Story.IntroLetter, 74, 14, Width);
        check(pages.Count >= 1 && pages.All(p => p.Count is > 0 and <= 14 && p.All(l => Width(l) <= 74)), "carta de Aurora cabe em páginas limitadas");
        var ending = TextLayout.Pages(Story.Ending, 64, 8, Width);
        check(ending.Count > 1 && ending.All(p => p.Count <= 8), "desfecho longo é paginado sem cortes");
        check(string.Join("", ending.SelectMany(p => p)).Replace(" ", "") == Story.Ending.Replace("\n", "").Replace(" ", ""), "paginação conserva todo o desfecho");
        check(TextLayout.Wrap("", 1, Width).Count == 1 && TextLayout.Pages("\n\n", 2, 3, Width).Count == 1, "texto vazio tem página segura");
        var exact = TextLayout.Pages("um\ndois\ntrês\nquatro", 10, 2, Width);
        check(exact.Count == 2 && exact[0].Count == 2 && exact[1][1] == "quatro", "limite exato de linhas não cria página vazia");
        bool invalidWidth = false, invalidPage = false;
        try { TextLayout.Wrap("x", 0, Width); } catch (ArgumentOutOfRangeException) { invalidWidth = true; }
        try { TextLayout.Pages("x", 2, 0, Width); } catch (ArgumentOutOfRangeException) { invalidPage = true; }
        check(invalidWidth && invalidPage, "limites inválidos são rejeitados");
    }
}

using System.Globalization;
namespace ValeDoSol;

/// <summary>Shared, measured layout for letters, journals and notices.</summary>
public static class TextLayout
{
    public static IReadOnlyList<string> Wrap(string value, int width, Func<string, int> measure)
    {
        ArgumentNullException.ThrowIfNull(measure);
        if (width <= 0) throw new ArgumentOutOfRangeException(nameof(width));
        List<string> lines = [];
        foreach (string paragraph in (value ?? "").Replace("\r\n", "\n").Replace('\r', '\n').Split('\n'))
        {
            string current = "";
            string[] words = paragraph.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) { lines.Add(""); continue; }
            foreach (string word in words)
            {
                string candidate = current.Length == 0 ? word : current + " " + word;
                if (measure(candidate) <= width) { current = candidate; continue; }
                if (current.Length > 0) { lines.Add(current); current = ""; }
                if (measure(word) <= width) { current = word; continue; }
                // A long name or unbroken word must not run through the frame.
                var elements = StringInfo.GetTextElementEnumerator(word);
                while (elements.MoveNext())
                {
                    string element = elements.GetTextElement();
                    if (current.Length > 0 && measure(current + element) > width) { lines.Add(current); current = ""; }
                    current += element;
                }
            }
            if (current.Length > 0) lines.Add(current);
        }
        return lines.Count > 0 ? lines : [""];
    }

    public static IReadOnlyList<IReadOnlyList<string>> Pages(string value, int width, int linesPerPage, Func<string, int> measure)
    {
        if (linesPerPage <= 0) throw new ArgumentOutOfRangeException(nameof(linesPerPage));
        var lines = Wrap(value, width, measure);
        List<IReadOnlyList<string>> pages = [];
        List<string> page = [];
        foreach (string line in lines)
        {
            if (page.Count == 0 && line.Length == 0) continue;
            page.Add(line);
            if (page.Count == linesPerPage) { pages.Add(page); page = []; }
        }
        if (page.Count > 0) pages.Add(page);
        if (pages.Count == 0) pages.Add([""]);
        return pages;
    }
}

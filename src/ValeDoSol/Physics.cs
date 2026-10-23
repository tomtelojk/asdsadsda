namespace ValeDoSol;

public static class Physics
{
    public static bool Blocked(Scene scene, float x, float y)
    {
        if (!float.IsFinite(x) || !float.IsFinite(y)) return true;
        Point size = Map.Size(scene); int inset = Map.Indoors(scene) ? 36 : 12, top = Map.Indoors(scene) ? 88 : 16;
        if (x < inset + 6 || x > size.x - inset - 6 || y < top + 4 || y > size.y - (Map.Indoors(scene) ? 28 : 12) - 4) return true;
        Rectangle feet = new(x - 6, y - 4, 12, 8);
        return Map.Solids(scene).Any(r => Map.Touch(feet, r));
    }
    public static Point Move(Scene scene, Point start, Point delta)
    {
        int steps = Math.Max(1, (int)MathF.Ceiling(Math.Max(Math.Abs(delta.x), Math.Abs(delta.y)) / 3));
        Point at = start; float dx = delta.x / steps, dy = delta.y / steps;
        for (int i = 0; i < steps; i++) { if (!Blocked(scene, at.x + dx, at.y)) at.x += dx; if (!Blocked(scene, at.x, at.y + dy)) at.y += dy; }
        return at;
    }
    public static float Distance(Point a, Point b) => MathF.Sqrt((a.x - b.x) * (a.x - b.x) + (a.y - b.y) * (a.y - b.y));
    public static bool Clear(Scene scene, Point from, Point to)
    {
        int n = Math.Max(1, (int)MathF.Ceiling(Distance(from, to) / 3));
        for (int i = 0; i <= n; i++) if (Blocked(scene, from.x + (to.x - from.x) * i / n, from.y + (to.y - from.y) * i / n)) return false;
        return true;
    }
    static readonly Dictionary<Scene, bool[]> navigation = [];
    static bool[] Grid(Scene scene)
    {
        if (navigation.TryGetValue(scene, out var grid)) return grid;
        Point size = Map.Size(scene); int cols = (int)size.x / 16, rows = (int)size.y / 16;
        grid = new bool[cols * rows]; for (int y = 0; y < rows; y++) for (int x = 0; x < cols; x++) grid[y * cols + x] = !Blocked(scene, x * 16 + 8, y * 16 + 8);
        navigation[scene] = grid; return grid;
    }
    public static Point Nearest(Scene scene, Point desired)
    {
        if (!Blocked(scene, desired.x, desired.y)) return desired;
        for (int r = 1; r < 64; r++) for (int y = -r; y <= r; y++) for (int x = -r; x <= r; x++) { if (Math.Abs(x) + Math.Abs(y) != r) continue; Point p = new((int)(desired.x / 16) * 16 + 8 + x * 16, (int)(desired.y / 16) * 16 + 8 + y * 16); if (!Blocked(scene, p.x, p.y)) return p; }
        throw new InvalidOperationException("Nenhuma posição livre no mapa.");
    }
    public static int FindPath(Scene scene, Point from, Point to, Point[] output)
    {
        if (output.Length == 0) return 0;
        if (Blocked(scene, from.x, from.y) || Blocked(scene, to.x, to.y)) return 0;
        if (Clear(scene, from, to)) { if (Distance(from, to) < .1f) return 0; output[0] = to; return 1; }
        Point size = Map.Size(scene); int cols = (int)size.x / 16, rows = (int)size.y / 16;
        int start = (int)(from.y / 16) * cols + (int)(from.x / 16), last = (int)(to.y / 16) * cols + (int)(to.x / 16); bool[] grid = Grid(scene);
        int[] cost = Enumerable.Repeat(int.MaxValue, grid.Length).ToArray(), parent = Enumerable.Repeat(-1, grid.Length).ToArray(); PriorityQueue<int, int> open = new(); cost[start] = 0; parent[start] = start; open.Enqueue(start, 0);
        while (open.TryDequeue(out int cell, out _))
        {
            if (cell == last) break; int x = cell % cols, y = cell / cols;
            foreach ((int dx, int dy) in new[] { (1, 0), (-1, 0), (0, 1), (0, -1) }) { int nx = x + dx, ny = y + dy; if (nx < 0 || ny < 0 || nx >= cols || ny >= rows) continue; int next = ny * cols + nx; if (!grid[next] && next != last || cost[next] <= cost[cell] + 1) continue; Point a = cell == start ? from : new(x * 16 + 8, y * 16 + 8), b = next == last ? to : new(nx * 16 + 8, ny * 16 + 8); if (!Clear(scene, a, b)) continue; cost[next] = cost[cell] + 1; parent[next] = cell; int h = Math.Abs(nx - (int)(to.x / 16)) + Math.Abs(ny - (int)(to.y / 16)); open.Enqueue(next, cost[next] + h); }
        }
        if (parent[last] < 0) return 0; List<Point> route = [to]; for (int n = last; n != start; n = parent[n]) { if (route.Count >= output.Length) return 0; if (n != last) route.Add(new(n % cols * 16 + 8, n / cols * 16 + 8)); }
        route.Reverse();
        int count = 0, at = 0; Point anchor = from; while (at < route.Count) { int far = at; while (far + 1 < route.Count && Clear(scene, anchor, route[far + 1])) far++; output[count++] = route[far]; anchor = route[far]; at = far + 1; }
        return count;
    }
}

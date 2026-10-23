namespace ValeDoSol;

/// <summary>
/// One layout for cultivation, drawing and collision. The 48 logical plot indices
/// remain stable across saves; only their world positions moved off the road.
/// Raised beds have two columns so every plant is reachable from a side aisle.
/// </summary>
public static class Garden
{
    const int OriginX = 512, OriginY = 704;
    const int BedColumns = 4, BedRows = 2, BedWidth = 64, BedHeight = 96;
    const int ColumnPitch = 96, RowPitch = 128;

    public static readonly Rectangle Bounds = new(488, 680, 400, 272);
    public static IReadOnlyList<Rectangle> Beds { get; } = Array.AsReadOnly(
        Enumerable.Range(0, BedColumns * BedRows)
            .Select(i => new Rectangle(OriginX + i % BedColumns * ColumnPitch,
                OriginY + i / BedColumns * RowPitch, BedWidth, BedHeight)).ToArray());

    // Gate openings are the same 32px gaps used by the internal aisles.
    public static IReadOnlyList<Rectangle> Fences { get; } = Array.AsReadOnly(new Rectangle[]
    {
        new(488, 680, 184, 8), new(704, 680, 184, 8),
        new(488, 944, 184, 8), new(704, 944, 184, 8),
        new(488, 688, 8, 112), new(488, 832, 8, 112),
        new(880, 688, 8, 112), new(880, 832, 8, 112)
    });
    public static IReadOnlyList<Point> Gates { get; } = Array.AsReadOnly(new Point[]
    {
        new(688, 684), new(688, 948), new(492, 816), new(884, 816)
    });
    public static IReadOnlyList<Rectangle> Paths { get; } = Array.AsReadOnly(new Rectangle[]
    {
        new(496, 688, 16, 256), new(576, 688, 32, 256),
        new(672, 680, 32, 272), new(768, 688, 32, 256), new(864, 688, 16, 256),
        new(496, 688, 384, 16), new(488, 800, 400, 32), new(496, 928, 384, 16)
    });
    public static IEnumerable<Rectangle> Solids => Beds.Concat(Fences);

    public static Point PlotPosition(int index)
    {
        if (index is < 0 or >= 48) throw new ArgumentOutOfRangeException(nameof(index));
        int column = index % 8, row = index / 8;
        return new(OriginX + column / 2 * ColumnPitch + column % 2 * 32,
            OriginY + row / 3 * RowPitch + row % 3 * 32);
    }
    /// <summary>A free standing point beside each plot, outside its raised bed.</summary>
    public static Point Approach(int index)
    {
        Point plot = PlotPosition(index);
        return new(plot.x + (index % 2 == 0 ? -8 : 40), plot.y + 16);
    }
    public static int PlotAt(float x, float y)
    {
        if (!float.IsFinite(x) || !float.IsFinite(y)) return -1;
        float localX = x - OriginX, localY = y - OriginY;
        if (localX < 0 || localY < 0 || localX >= 352 || localY >= 224) return -1;
        int bedColumn = (int)(localX / ColumnPitch), bedRow = (int)(localY / RowPitch);
        int withinX = (int)localX % ColumnPitch, withinY = (int)localY % RowPitch;
        if (withinX >= BedWidth || withinY >= BedHeight) return -1;
        int column = bedColumn * 2 + withinX / 32, row = bedRow * 3 + withinY / 32;
        return row * 8 + column;
    }
}

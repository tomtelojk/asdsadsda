namespace ValeDoSol;

/// <summary>The same fitted rectangle drives presentation and pointer coordinates.</summary>
public readonly record struct Viewport(float Scale, float X, float Y, int LogicalWidth, int LogicalHeight)
{
    public Rectangle Bounds => new(X, Y, LogicalWidth * Scale, LogicalHeight * Scale);

    public static Viewport Fit(int windowWidth, int windowHeight, bool sharpPixels, int logicalWidth = 1280, int logicalHeight = 720)
    {
        if (logicalWidth <= 0 || logicalHeight <= 0) throw new ArgumentOutOfRangeException(nameof(logicalWidth));
        float fit = Math.Min((float)Math.Max(1, windowWidth) / logicalWidth, (float)Math.Max(1, windowHeight) / logicalHeight);
        bool integerScale = sharpPixels && fit >= 1;
        float scale = integerScale ? MathF.Floor(fit) : fit;
        float x = (Math.Max(1, windowWidth) - logicalWidth * scale) / 2;
        float y = (Math.Max(1, windowHeight) - logicalHeight * scale) / 2;
        // Whole pixels preserve an exact source-to-screen mapping even in odd-sized windows.
        if (integerScale) { x = MathF.Floor(x); y = MathF.Floor(y); }
        return new(scale, x, y, logicalWidth, logicalHeight);
    }

    public Point ToLogical(Point windowPoint) => new((windowPoint.x - X) / Scale, (windowPoint.y - Y) / Scale);
    public Point ToWindow(Point logicalPoint) => new(X + logicalPoint.x * Scale, Y + logicalPoint.y * Scale);
}

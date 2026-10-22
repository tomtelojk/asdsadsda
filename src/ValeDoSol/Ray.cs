using System.Runtime.InteropServices;
namespace ValeDoSol;

[StructLayout(LayoutKind.Sequential)] public struct Point(double x, double y) { public float x = (float)x, y = (float)y; }
[StructLayout(LayoutKind.Sequential)] public struct Rectangle(double x, double y, double w, double h) { public float x = (float)x, y = (float)y, width = (float)w, height = (float)h; }
[StructLayout(LayoutKind.Sequential)] public struct Color(int r, int g, int b, int a = 255) { public byte r = (byte)r, g = (byte)g, b = (byte)b, a = (byte)a; }
[StructLayout(LayoutKind.Sequential)] public struct Camera2D { public Point offset, target; public float rotation, zoom; }
[StructLayout(LayoutKind.Sequential)] public struct Texture { public uint id; public int width, height, mipmaps, format; }
[StructLayout(LayoutKind.Sequential)] public struct RenderTexture { public uint id; public Texture texture, depth; }
[StructLayout(LayoutKind.Sequential)] public struct Image { public nint data; public int width, height, mipmaps, format; }
[StructLayout(LayoutKind.Sequential)] public struct Font { public int baseSize, glyphCount, glyphPadding; public Texture texture; public nint recs, glyphs; }
// Small, explicit binding: only rendering/window functions cross into raylib.
// Farming, navigation, NPCs, saves and UI behavior are managed C# code.
public static class Ray
{
    const string Lib = "raylib";
    public const int KEY_NULL = 0, KEY_SPACE = 32, KEY_ESCAPE = 256, KEY_TAB = 258, KEY_F5 = 294, KEY_F11 = 300, KEY_ONE = 49, KEY_A = 65, KEY_D = 68, KEY_E = 69, KEY_F = 70, KEY_H = 72, KEY_S = 83, KEY_W = 87, KEY_RIGHT = 262, KEY_LEFT = 263, KEY_DOWN = 264, KEY_UP = 265;
    public const int KEY_I = 73, KEY_J = 74, KEY_R = 82, KEY_LEFT_SHIFT = 340, KEY_ENTER = 257, MOUSE_BUTTON_RIGHT = 1;
    public const int MOUSE_BUTTON_LEFT = 0, MOUSE_CURSOR_DEFAULT = 0, MOUSE_CURSOR_POINTING_HAND = 4, FLAG_WINDOW_RESIZABLE = 4, FLAG_VSYNC_HINT = 64, LOG_WARNING = 4, TEXTURE_FILTER_POINT = 0;
    public static readonly Color WHITE = new(255, 255, 255);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void SetConfigFlags(uint flags);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void SetTraceLogLevel(int level);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void InitWindow(int width, int height, [MarshalAs(UnmanagedType.LPUTF8Str)] string title);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)][return: MarshalAs(UnmanagedType.I1)] public static extern bool IsWindowReady();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)][return: MarshalAs(UnmanagedType.I1)] public static extern bool WindowShouldClose();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)][return: MarshalAs(UnmanagedType.I1)] public static extern bool IsWindowFocused();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void SetWindowMinSize(int width, int height);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void SetExitKey(int key);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void SetTargetFPS(int fps);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void ToggleFullscreen();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void CloseWindow();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern int GetScreenWidth();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern int GetScreenHeight();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern float GetFrameTime();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern Point GetMousePosition();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)][return: MarshalAs(UnmanagedType.I1)] public static extern bool IsKeyDown(int key);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)][return: MarshalAs(UnmanagedType.I1)] public static extern bool IsKeyPressed(int key);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)][return: MarshalAs(UnmanagedType.I1)] public static extern bool IsMouseButtonReleased(int key);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void SetMouseCursor(int cursor);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void BeginDrawing();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void EndDrawing();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void ClearBackground(Color color);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern RenderTexture LoadRenderTexture(int width, int height);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)][return: MarshalAs(UnmanagedType.I1)] public static extern bool IsRenderTextureValid(RenderTexture target);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void UnloadRenderTexture(RenderTexture target);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void BeginTextureMode(RenderTexture target);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void EndTextureMode();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void SetTextureFilter(Texture texture, int filter);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void DrawTexturePro(Texture texture, Rectangle source, Rectangle destination, Point origin, float rotation, Color tint);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void BeginMode2D(Camera2D camera);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void EndMode2D();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void BeginScissorMode(int x, int y, int width, int height);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void EndScissorMode();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "DrawRectangle")] static extern void Rect(int x, int y, int w, int h, Color c);
    public static void DrawRectangle(double x, double y, double w, double h, Color c) => Rect((int)x, (int)y, (int)w, (int)h, c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void DrawRectanglePro(Rectangle rect, Point origin, float rotation, Color c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "DrawRectangleLines")] static extern void RectLines(int x, int y, int w, int h, Color c);
    public static void DrawRectangleLines(double x, double y, double w, double h, Color c) => RectLines((int)x, (int)y, (int)w, (int)h, c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void DrawRectangleLinesEx(Rectangle rec, float thick, Color c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void DrawRectangleRounded(Rectangle rec, float roundness, int segments, Color c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void DrawRectangleRoundedLinesEx(Rectangle rec, float roundness, int segments, float thick, Color c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "DrawCircle")] static extern void Circle(int x, int y, float radius, Color c);
    public static void DrawCircle(double x, double y, double radius, Color c) => Circle((int)x, (int)y, (float)radius, c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "DrawEllipse")] static extern void Ellipse(int x, int y, float rx, float ry, Color c);
    public static void DrawEllipse(double x, double y, double rx, double ry, Color c) => Ellipse((int)x, (int)y, (float)rx, (float)ry, c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "DrawEllipseLines")] static extern void EllipseLines(int x, int y, float rx, float ry, Color c);
    public static void DrawEllipseLines(double x, double y, double rx, double ry, Color c) => EllipseLines((int)x, (int)y, (float)rx, (float)ry, c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void DrawTriangle(Point a, Point b, Point c, Color color);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void DrawLineEx(Point a, Point b, float thick, Color color);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "DrawLine")] static extern void Line(int x1, int y1, int x2, int y2, Color color);
    public static void DrawLine(double x1, double y1, double x2, double y2, Color color) => Line((int)x1, (int)y1, (int)x2, (int)y2, color);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "DrawText")] static extern void Text([MarshalAs(UnmanagedType.LPUTF8Str)] string text, int x, int y, int size, Color color);
    public static void DrawText(string text, double x, double y, double size, Color color) { if (SmoothText && uiFont.texture.id != 0) DrawTextEx(uiFont, text, new(x, y), (float)size, 0, color); else Text(text, (int)x, (int)y, (int)size, color); }
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "MeasureText")] static extern int MeasureTextNative([MarshalAs(UnmanagedType.LPUTF8Str)] string text, int size);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern Color ColorAlpha(Color color, float alpha);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)][return: MarshalAs(UnmanagedType.I1)] public static extern bool CheckCollisionPointRec(Point p, Rectangle rec);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern Image GenImageColor(int width, int height, Color color);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void ImageDrawRectangle(ref Image image, int x, int y, int width, int height, Color color);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void SetWindowIcon(Image image);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void UnloadImage(Image image);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void TakeScreenshot([MarshalAs(UnmanagedType.LPUTF8Str)] string path);

    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern Texture LoadTextureFromImage(Image image);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern void UnloadTexture(Texture texture);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern Image LoadImageFromTexture(Texture texture);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)][return: MarshalAs(UnmanagedType.I1)] public static extern bool ExportImage(Image image, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] public static extern Image LoadImageFromMemory([MarshalAs(UnmanagedType.LPUTF8Str)] string type, byte[] bytes, int size);
    static Font uiFont;
    public static bool SmoothText = true;
    public static int MeasureText(string text, int size) => SmoothText && uiFont.texture.id != 0 ? (int)Math.Ceiling(MeasureTextEx(uiFont, text, size, 0).x) : MeasureTextNative(text, size);
    public static void LoadUiFont() { using var stream = typeof(Ray).Assembly.GetManifestResourceStream("ValeDoSol.UiFont.ttf"); if (stream == null) return; using MemoryStream data = new(); stream.CopyTo(data); byte[] bytes = data.ToArray(); int[] glyphs = Enumerable.Range(32, 95).Concat(Enumerable.Range(160, 96)).ToArray(); uiFont = LoadFontFromMemory(".ttf", bytes, bytes.Length, 32, glyphs, glyphs.Length); if (uiFont.texture.id != 0) SetTextureFilter(uiFont.texture, 1); }
    public static void UnloadUiFont() { if (uiFont.texture.id != 0) { UnloadFont(uiFont); uiFont = default; } }
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern Font LoadFontFromMemory([MarshalAs(UnmanagedType.LPUTF8Str)] string type, byte[] data, int dataSize, int size, int[] codepoints, int count);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void UnloadFont(Font font);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void DrawTextEx(Font font, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, Point position, float size, float spacing, Color tint);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern Point MeasureTextEx(Font font, [MarshalAs(UnmanagedType.LPUTF8Str)] string text, float size, float spacing);
}

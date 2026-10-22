using static ValeDoSol.Ray;
using static ValeDoSol.World;
using static ValeDoSol.GameLogic;
namespace ValeDoSol;

public static class Program
{
    public static int Main(string[] args)
    {
        if (args.Contains("--licenses")) { foreach (string resource in new[] { "ValeDoSol.License.txt", "ValeDoSol.RaylibLicense.txt", "ValeDoSol.FontLicense.txt", "ValeDoSol.LpcCredits.txt", "ValeDoSol.LpcLicense.txt" }) { using var stream = typeof(Program).Assembly.GetManifestResourceStream(resource); if (stream != null) { using StreamReader reader = new(stream); Console.WriteLine(reader.ReadToEnd()); } } return 0; }
        if (args.Contains("--version")) { Console.WriteLine("Vale do Sol 6.0.0 | C# + .NET 10 + raylib 6.2 | Linux x86_64"); return 0; }
        if (args.Contains("--self-test")) return SelfTests.Run();
        if (args.Contains("--help")) { Console.WriteLine("Vale do Sol - C#\n--version\n--save-dir DIRETORIO\n--smoke (fecha após 60 quadros)\n--screenshot ARQUIVO.png\n--village (somente com --smoke)"); return 0; }
        bool smoke = false, village = false, home = false, shop = false; string? screenshot = null, dir = null, exportArt = null; Scene? preview = null;
        for (int i = 0; i < args.Length; i++) switch (args[i])
            {
                case "--smoke": smoke = true; break;
                case "--village": village = true; break;
                case "--home": home = true; break;
                case "--shop": shop = true; break;
                case "--scene" when i + 1 < args.Length && Enum.TryParse<Scene>(args[i + 1], true, out var selectedScene) && Enum.IsDefined(selectedScene): preview = selectedScene; i++; break;
                case "--save-dir" when i + 1 < args.Length: dir = args[++i]; break;
                case "--export-art" when i + 1 < args.Length: exportArt = args[++i]; break;
                case "--screenshot" when i + 1 < args.Length: screenshot = args[++i]; break;
                default: Console.Error.WriteLine($"Argumento desconhecido ou incompleto: {args[i]}"); return 2;
            }
        string? xdg = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        dir ??= Path.Combine(string.IsNullOrEmpty(xdg) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".local", "share") : xdg, "vale-do-sol-csharp");
        Game game = new() { ignore_focus = smoke }; game_init(game, dir); if (smoke && village) game_scene(game, Logic.VILLAGE, new(416, 320)); if (smoke && home) game_scene(game, Logic.HOME, new(320, 280)); if (smoke && shop) game_scene(game, Logic.SHOP, new(352, 280));
        if (smoke && preview is Scene selected) game_scene(game, selected, Physics.Nearest(selected, Map.Indoors(selected) ? Map.Arrival(selected) : new(416, 320)));
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY")) && string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WAYLAND_DISPLAY"))) { Console.Error.WriteLine("Abra o jogo em uma sessão gráfica Linux. Para testar sem janela, use --self-test."); return 1; }
        SetTraceLogLevel(LOG_WARNING); SetConfigFlags(FLAG_WINDOW_RESIZABLE | FLAG_VSYNC_HINT); InitWindow(1280, 720, "Vale do Sol"); if (!IsWindowReady()) return 1; SetWindowMinSize(960, 540); SetExitKey(KEY_NULL); SetTargetFPS(60);
        Image icon = GenImageColor(64, 64, color_hex(0x2a4638)); ImageDrawRectangle(ref icon, 28, 28, 7, 29, color_hex(0x9eaf72)); ImageDrawRectangle(ref icon, 16, 37, 17, 6, color_hex(0xc4cd8b)); ImageDrawRectangle(ref icon, 32, 31, 17, 6, color_hex(0xa9bd7c)); ImageDrawRectangle(ref icon, 21, 9, 23, 23, color_hex(0xeed18b)); ImageDrawRectangle(ref icon, 27, 15, 11, 11, color_hex(0xb88a51)); SetWindowIcon(icon); UnloadImage(icon);
        LoadUiFont(); Art.Init(); Scenery.Init(); if (exportArt != null && !Art.Export(exportArt)) { Console.Error.WriteLine("Não foi possível exportar a arte."); Scenery.Unload(); Art.Unload(); UnloadUiFont(); CloseWindow(); return 1; }
        RenderTexture target = LoadRenderTexture(UI_W, UI_H); if (!IsRenderTextureValid(target)) { CloseWindow(); return 1; }
        SetTextureFilter(target.texture, TEXTURE_FILTER_POINT);
        int frames = 0;
        try
        {
            while (!WindowShouldClose() && !game.quit)
            {
                float scale = Math.Min((float)GetScreenWidth() / UI_W, (float)GetScreenHeight() / UI_H), ox = (GetScreenWidth() - UI_W * scale) / 2, oy = (GetScreenHeight() - UI_H * scale) / 2; Point raw = GetMousePosition(), mouse = new((raw.x - ox) / scale, (raw.y - oy) / scale); float dt = Math.Min(GetFrameTime(), .05f);
                game_update(game, dt, mouse); BeginTextureMode(target); Ui.game_draw(game); EndTextureMode();
                BeginDrawing(); ClearBackground(color_hex(0x1e3027)); DrawTexturePro(target.texture, new(0, 0, UI_W, -UI_H), new(ox, oy, UI_W * scale, UI_H * scale), new(0, 0), 0, WHITE); EndDrawing();
                if (smoke && ++frames >= 60) { if (screenshot != null) TakeScreenshot(screenshot); break; }
            }
        }
        finally { game_save(game); UnloadRenderTexture(target); Scenery.Unload(); Art.Unload(); UnloadUiFont(); CloseWindow(); }
        if (smoke) { Console.WriteLine(game.save_ok ? "PASS: janela C#, 60 quadros renderizados e save local." : "FAIL: o aplicativo abriu, mas não conseguiu salvar."); return game.save_ok ? 0 : 1; }
        return 0;
    }
}

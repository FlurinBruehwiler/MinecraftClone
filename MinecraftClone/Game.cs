using System.Diagnostics;
using System.Drawing;
using Flamui;
using Flamui.Drawing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Color = Raylib_cs.Color;
using PixelFormat = Raylib_cs.PixelFormat;
using Rectangle = Raylib_cs.Rectangle;
using Shader = Raylib_cs.Shader;

namespace RayLib3dTest;

public class Game
{
    private Player _player;
    private Model _skyBox;
    public UiTree UiTree;
    private Renderer _renderer;

    public Game(Player player)
    {
        _player = player;
        Initialize();
    }

    public Shader ChunkShader;

    public int ShaderLocSunDirection;

    public unsafe void Initialize()
    {
        { //Initialize Skybox
            var cube = Raylib.GenMeshCube(1, 1, 1);
            _skyBox = Raylib.LoadModelFromMesh(cube);

            var shader = Raylib.LoadShader("Resources/Shaders/skybox.vs", "Resources/Shaders/skybox.fs");

            _skyBox.Materials[0].Shader = shader;

            int[] doGamma = { 0 };
            int[] vflipped = { 0 };
            int[] environmentMap = { (int)MaterialMapIndex.Cubemap };

            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "environmentMap"),  environmentMap , ShaderUniformDataType.Int);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "doGamma"),  doGamma, ShaderUniformDataType.Int);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "vflipped"), vflipped, ShaderUniformDataType.Int);

            var img = Raylib.LoadImage("Resources/skybox.png");
            _skyBox.Materials[0].Maps[(int)MaterialMapIndex.Cubemap].Texture = Raylib.LoadTextureCubemap(img, CubemapLayout.AutoDetect);
            Raylib.UnloadImage(img);
        }

        ChunkShader = Raylib.LoadShader("Resources/Shaders/chunkVertex.vs", "Resources/Shaders/chunkFragment.fs");
        ShaderLocSunDirection = Raylib.GetShaderLocation(ChunkShader, "sunDirection");

        HuskModel = Models.LoadModel("husk");

        var host = new RaylibUiTreeHost();

        _renderer = new Renderer();


        var gl = GL.GetApi(new RaylibGlContext());
        _renderer.Initialize(gl, host);


        UiTree = new UiTree(host, RenderUI);
    }

    public JemFile HuskModel; //should not be here

    public void RenderUI(Ui ui)
    {
        using (ui.Rect().MainAlign(MAlign.End).CrossAlign(XAlign.Center))
        {
            // using (ui.Rect().Color(C.Red4.WithAlpha(100)).Width(500).Height(500))
            // {
            //     ui.Image(new GpuTexture
            //     {
            //         TextureId = CurrentWorld.BlockPreviewAtlas.Id,
            //         Height = CurrentWorld.BlockPreviewAtlas.Height,
            //         Width = CurrentWorld.BlockPreviewAtlas.Width
            //     }).SubImage(new Bounds(0, 0, 1000, 1000)).FlipVertically();
            // }

            //Hotbar
            using (ui.Rect().Height(80).ShrinkWidth().Color(C.Black.WithAlpha(100)).Direction(Dir.Horizontal))
            {
                for (int i = 0; i < 9; i++)
                {
                    using var _ = ui.CreateIdScope(i);

                    using (var border = ui.Rect().Width(80).Height(80).Border(5, C.Gray6))
                    {
                        if (i == _player.SelectedHotbarSlot)
                        {
                            border.Border(6, C.Black);
                        }

                        var slot = _player.Inventory[i];

                        if (slot.Count != 0)
                        {
                            var pos = Textures.GetTexturePosForBlockPreview(slot.BlockId);


                            ui.Image(new GpuTexture
                            {
                                TextureId = CurrentWorld.BlockPreviewAtlas.Id,
                                Height = CurrentWorld.BlockPreviewAtlas.Height,
                                Width = CurrentWorld.BlockPreviewAtlas.Width
                            }).SubImage(new Bounds(pos.X * 100, CurrentWorld.BlockPreviewAtlas.Height - 100 - pos.Y * 100, 100, 100)).FlipVertically();


                            using (ui.Rect().AbsolutePosition().AbsoluteSize(widthOffsetParent: 0, heightOffsetParent:0)
                                       .CrossAlign(XAlign.End).MainAlign(MAlign.End).Padding(5))
                            {
                                ui.Text(slot.Count.ToArenaString()).Size(30).Color(C.White);
                            }
                        }
                    }
                }
            }
        }
    }

    public void GameLoop()
    {
        while (!Raylib.WindowShouldClose())
        {
            Update();

            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.RayWhite);

            Raylib.BeginMode3D(_player.Camera);

                Draw3d();

                Raylib.EndMode3D();

            Draw2d();

            Raylib.EndDrawing();
        }
    }

    private void RunTickStep()
    {
        DevTools.Tick();

        _player.Tick();

        foreach (var bot in CurrentWorld.bots)
        {
            bot.Tick();
        }
    }

    private static long _lastTickTimestamp;
    public const float TickRateMs = 1000f / 20; //50ms, 20tps
    public static float MsSinceLastTick() => (float)Stopwatch.GetElapsedTime(_lastTickTimestamp).TotalMilliseconds;

    private void Update()
    {
        //todo pass MousePosition
        UiTree.Update(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());

        var timeSinceLastTick = Stopwatch.GetElapsedTime(_lastTickTimestamp);

        //if the framerate drops below the tick rate, we do not run multiple ticks per frame. This is done on purpose.
        //The simulation rate of the game will just slow down.
        if(timeSinceLastTick.TotalMilliseconds > TickRateMs){
            DevTools.Print(1000f / (float)timeSinceLastTick.TotalMilliseconds, "TPS");
            _lastTickTimestamp = Stopwatch.GetTimestamp();
            RunTickStep();
        }

        _player.Update();

        if (Raylib.IsKeyPressed(KeyboardKey.F3))
        {
            DevTools.DevToolsEnabled = !DevTools.DevToolsEnabled;
        }

        if (Raylib.IsKeyDown(KeyboardKey.LeftControl) && Raylib.IsKeyPressed(KeyboardKey.S))
        {
            CurrentWorld.SaveToDirectory(SaveLocation);
        }

        if (Raylib.IsKeyReleased(KeyboardKey.M))
        {
            Thread.Sleep(1000);
        }
    }

    public static string SaveLocation = Path.GetFullPath(Path.Combine(Directory.GetParent(typeof(Game).Assembly.FullName).FullName, "../../../../World1"));

    private void Draw2d()
    {
        DevTools.Draw2d();

        int centerX = Raylib.GetScreenWidth() / 2;
        int centerY = Raylib.GetScreenHeight() / 2;

        // Crosshair lines (length = 10 px each side)
        Raylib.DrawLine(centerX - 10, centerY, centerX + 10, centerY, Color.Black); // Horizontal
        Raylib.DrawLine(centerX, centerY - 10, centerX, centerY + 10, Color.Black); // Vertical

        var commands = StaticFunctions.Render(UiTree, Matrix4X4<float>.Identity);

        var texture = StaticFunctions.ExecuteRenderInstructions(commands, _renderer, Raylib.GetScreenWidth(), Raylib.GetScreenHeight(), isExternal: true);

        var raylibTexture = new Texture2D
        {
            Id = texture.textureId,
            Width = texture.width,
            Height = texture.height,
            Format = PixelFormat.UncompressedR8G8B8A8,
            Mipmaps = 1
        };

        Rectangle src = new Rectangle( 0, 0, texture.width, -texture.height );
        Rectangle dst = new Rectangle( 0, 0, texture.width, texture.height );
        Raylib.DrawTexturePro(raylibTexture, src, dst, new Vector2(0, 0), 0, Color.White);


        // Raylib.DrawRectangle(0, 0, CurrentWorld.BlockPreviewAtlas.Width, CurrentWorld.BlockPreviewAtlas.Height, Color.Red);
        // Raylib.DrawTexture(CurrentWorld.BlockPreviewAtlas, 0, 0, Color.White);
    }

    private void Draw3d()
    {
        

        { //Draw Skybox

            Rlgl.DisableBackfaceCulling();

            Rlgl.DisableDepthMask();
            

            Raylib.DrawModel(_skyBox, Vector3.Zero, 1, Color.White);
            

            Rlgl.EnableBackfaceCulling();
            Rlgl.EnableDepthMask();
        }

        DevTools.Draw3d();

        var sunDirection = Vector3.Normalize(new Vector3(-0.2f, 1, -1));
        Raylib.SetShaderValue(ChunkShader, ShaderLocSunDirection, [sunDirection.X, sunDirection.Y, sunDirection.Z], ShaderUniformDataType.Vec3);

        Raylib.BeginBlendMode(BlendMode.Alpha);
        foreach (var (_, chunk) in CurrentWorld.Chunks)
        {
            var pos = new Vector3(chunk.Pos.X * 16, chunk.Pos.Y * 16, chunk.Pos.Z * 16);
            if(chunk.HasMesh)
                Raylib.DrawModel(chunk.Model, pos, 1, Color.White);

            // if(DevTools.DevToolsEnabled)
                // DrawCubeWiresV(pos + new Vector3(8), new Vector3(16), Color.RED);
        }
        Raylib.EndBlendMode();

        _player.Render();
        

        foreach (var bot in CurrentWorld.bots)
        {
            bot.Render();
        }
    }
}

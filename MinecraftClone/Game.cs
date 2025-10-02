using System.Diagnostics;
using System.Drawing;
using Flamui;
using Flamui.Drawing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Color = Raylib_cs.Color;
using PixelFormat = Raylib_cs.PixelFormat;
using Rectangle = Raylib_cs.Rectangle;

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

        HuskModel = Models.LoadModel("husk");


        var host = new RaylibUiTreeHost();

        _renderer = new Renderer();


        var gl = GL.GetApi(new RaylibGlContext());
        _renderer.Initialize(gl, host);


        UiTree = new UiTree(host, (ui) =>
        {
            using (ui.Rect().Padding(10).Border(10, C.Black))
            {
                // using (ui.Rect().Color(C.Blue6))
                {
                    ui.Text("Flamui :)").Size(30);

                    using (ui.Rect().Height(200).Width(200))
                    {
                        ui.Image(new GpuTexture
                        {
                            Width = HuskModel.Texture2D.Width,
                            Height = HuskModel.Texture2D.Height,
                            TextureId = HuskModel.Texture2D.Id
                        });
                    }
                }
            }
        });
    }

    public JemFile HuskModel; //should not be here

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

        TextureAtlas.GenerateBlockPreviews();
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
        

        foreach (var (_, chunk) in CurrentWorld.Chunks)
        {
            var pos = new Vector3(chunk.Pos.X * 16, chunk.Pos.Y * 16, chunk.Pos.Z * 16);
            if(chunk.HasMesh)
                Raylib.DrawModel(chunk.Model, pos, 1, Color.White);

            
            // if(DevTools.DevToolsEnabled)
                // DrawCubeWiresV(pos + new Vector3(8), new Vector3(16), Color.RED);
        }

        _player.Render();
        

        foreach (var bot in CurrentWorld.bots)
        {
            bot.Render();
        }
    }
}

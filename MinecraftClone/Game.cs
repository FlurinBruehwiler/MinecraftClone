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
            var cube = GenMeshCube(1, 1, 1);
            _skyBox = LoadModelFromMesh(cube);

            var shader = LoadShader("Resources/Shaders/skybox.vs", "Resources/Shaders/skybox.fs");

            _skyBox.Materials[0].Shader = shader;

            int[] doGamma = { 0 };
            int[] vflipped = { 0 };
            int[] environmentMap = { (int)MaterialMapIndex.Cubemap };

            SetShaderValue(shader, GetShaderLocation(shader, "environmentMap"),  environmentMap , ShaderUniformDataType.Int);
            SetShaderValue(shader, GetShaderLocation(shader, "doGamma"),  doGamma, ShaderUniformDataType.Int);
            SetShaderValue(shader, GetShaderLocation(shader, "vflipped"), vflipped, ShaderUniformDataType.Int);

            var img = LoadImage("Resources/skybox.png");
            _skyBox.Materials[0].Maps[(int)MaterialMapIndex.Cubemap].Texture = LoadTextureCubemap(img, CubemapLayout.AutoDetect);
            UnloadImage(img);
        }

        HuskModel = Models.LoadModel("husk");


        var host = new RaylibUiTreeHost();

        _renderer = new Renderer();


        var gl = GL.GetApi(new RaylibGlContext());
        _renderer.Initialize(gl, host);


        UiTree = new UiTree(host, (ui) =>
        {
            using (ui.Rect().Width(300).Height(300).Color(C.Red6).Padding(10))
            {
                using (ui.Rect().Color(C.Blue6))
                {
                    ui.Text("Flamui :)").Size(30);
                }
            }
        });
    }

    public JemFile HuskModel; //should not be here

    public void GameLoop()
    {
        while (!WindowShouldClose())
        {
            Update();

            BeginDrawing();

                ClearBackground(Color.RayWhite);

                BeginMode3D(_player.Camera);

                    Draw3d();
    
                EndMode3D();

                Draw2d();
            
            EndDrawing();
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
        UiTree.Update(GetScreenWidth(), GetScreenHeight());

        var timeSinceLastTick = Stopwatch.GetElapsedTime(_lastTickTimestamp);

        //if the framerate drops below the tick rate, we do not run multiple ticks per frame. This is done on purpose.
        //The simulation rate of the game will just slow down.
        if(timeSinceLastTick.TotalMilliseconds > TickRateMs){
            DevTools.Print(1000f / (float)timeSinceLastTick.TotalMilliseconds, "TPS");
            _lastTickTimestamp = Stopwatch.GetTimestamp();
            RunTickStep();
        }

        _player.Update();

        if (IsKeyPressed(KeyboardKey.F3))
        {
            DevTools.DevToolsEnabled = !DevTools.DevToolsEnabled;
        }

        if (IsKeyReleased(KeyboardKey.M))
        {
            Thread.Sleep(1000);
        }
    }

    private void Draw2d()
    {
        DevTools.Draw2d();

        int centerX = GetScreenWidth() / 2;
        int centerY = GetScreenHeight() / 2;

        // Crosshair lines (length = 10 px each side)
        DrawLine(centerX - 10, centerY, centerX + 10, centerY, Color.Black); // Horizontal
        DrawLine(centerX, centerY - 10, centerX, centerY + 10, Color.Black); // Vertical

        var commands = StaticFunctions.Render(UiTree, Matrix4X4<float>.Identity);

        var texture = StaticFunctions.ExecuteRenderInstructions(commands, _renderer, GetScreenWidth(), GetScreenHeight(), isExternal: true);

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
        DrawTexturePro(raylibTexture, src, dst, new Vector2(0, 0), 0, Color.White);
    }

    private void Draw3d()
    {
        

        { //Draw Skybox

            DisableBackfaceCulling();

            DisableDepthMask();
            

            DrawModel(_skyBox, Vector3.Zero, 1, Color.White);
            

            EnableBackfaceCulling();
            EnableDepthMask();
        }

        DevTools.Draw3d();
        

        foreach (var (_, chunk) in CurrentWorld.Chunks)
        {
            var pos = new Vector3(chunk.Pos.X * 16, chunk.Pos.Y * 16, chunk.Pos.Z * 16);
            if(chunk.HasMesh)
                DrawModel(chunk.Model, pos, 1, Color.White);

            
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

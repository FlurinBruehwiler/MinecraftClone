using System.Diagnostics;

namespace RayLib3dTest;

public class Game
{
    private Player _player;
    private Model _skyBox;

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

            _skyBox.materials[0].shader = shader;

            int[] doGamma = { 0 };
            int[] vflipped = { 0 };
            int[] environmentMap = { (int)MaterialMapIndex.MATERIAL_MAP_CUBEMAP };

            SetShaderValue(shader, GetShaderLocation(shader, "environmentMap"),  environmentMap , ShaderUniformDataType.SHADER_UNIFORM_INT);
            SetShaderValue(shader, GetShaderLocation(shader, "doGamma"),  doGamma, ShaderUniformDataType.SHADER_UNIFORM_INT);
            SetShaderValue(shader, GetShaderLocation(shader, "vflipped"), vflipped, ShaderUniformDataType.SHADER_UNIFORM_INT);

            var img = LoadImage("Resources/skybox.png");
            _skyBox.materials[0].maps[(int)MaterialMapIndex.MATERIAL_MAP_CUBEMAP].texture = LoadTextureCubemap(img, CubemapLayout.CUBEMAP_LAYOUT_AUTO_DETECT);
            UnloadImage(img);
        }
    }

    public void GameLoop()
    {
        while (!WindowShouldClose())
        {
            Update();

            BeginDrawing();

                ClearBackground(Color.RAYWHITE);

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
        var timeSinceLastTick = Stopwatch.GetElapsedTime(_lastTickTimestamp);

        //if the framerate drops below the tick rate, we do not run multiple ticks per frame. This is done on purpose.
        //The simulation rate of the game will just slow down.
        if(timeSinceLastTick.TotalMilliseconds > TickRateMs){
            DevTools.Print(1000f / (float)timeSinceLastTick.TotalMilliseconds, "TPS");
            _lastTickTimestamp = Stopwatch.GetTimestamp();
            RunTickStep();
        }

        _player.Update();

        if (IsKeyPressed(KeyboardKey.KEY_F3))
        {
            DevTools.DevToolsEnabled = !DevTools.DevToolsEnabled;
        }

        if (IsKeyReleased(KeyboardKey.KEY_M))
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
        DrawLine(centerX - 10, centerY, centerX + 10, centerY, Color.BLACK); // Horizontal
        DrawLine(centerX, centerY - 10, centerX, centerY + 10, Color.BLACK); // Vertical

    }

    private void Draw3d()
    {
        { //Draw Skybox
            rlDisableBackfaceCulling();
            rlDisableDepthMask();

            DrawModel(_skyBox, Vector3.Zero, 1, Color.WHITE);

            rlEnableBackfaceCulling();
            rlEnableDepthMask();
        }

        DevTools.Draw3d();

        foreach (var (_, chunk) in CurrentWorld.Chunks)
        {
            var pos = new Vector3(chunk.Pos.X * 16, chunk.Pos.Y * 16, chunk.Pos.Z * 16);
            if(chunk.HasMesh)
                DrawModel(chunk.Model, pos, 1, Color.WHITE);

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

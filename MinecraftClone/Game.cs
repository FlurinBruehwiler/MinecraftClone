namespace RayLib3dTest;

public class Game
{
    private CameraManager _cameraManager;
    private Model _skyBox;

    public Game(CameraManager cameraManager)
    {
        _cameraManager = cameraManager;
        Initialize();
    }

    public unsafe void Initialize()
    {
        { //Initialize Skybox
            var cube = GenMeshCube(1, 1, 1);
            _skyBox = LoadModelFromMesh(cube);

            var shader = LoadShader("resources/shaders/skybox.vs", "resources/shaders/skybox.fs");

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
            MakeTheServus();

            BeginDrawing();

                ClearBackground(Color.RAYWHITE);

                BeginMode3D(_cameraManager.Camera);

                    Draw3d();
    
                EndMode3D();

                Draw2d();
            
            EndDrawing();
        }
    }

    private void MakeTheServus()
    {
        _cameraManager.Update();
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
            DrawModel(chunk.Model, new Vector3(chunk.Pos.X * 16, chunk.Pos.Y * 16, chunk.Pos.Z * 16), 1, Color.WHITE);
        }
    }
}

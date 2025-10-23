using System.Diagnostics;
using Flamui;
using Flamui.Components;
using Flamui.Drawing;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Color = Raylib_cs.Color;
using MatrixMode = Raylib_cs.MatrixMode;
using MouseButton = Raylib_cs.MouseButton;
using PixelFormat = Raylib_cs.PixelFormat;
using Rectangle = Raylib_cs.Rectangle;
using Shader = Raylib_cs.Shader;

namespace MinecraftClone;

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

    public static int ShaderLocSunDirection;
    public static int ShaderLocAlphaCutout;
    public static int ShaderLocTime;

    public unsafe void Initialize()
    {
        { //Initialize Skybox
            var cube = Raylib.GenMeshCube(1, 1, 1);
            _skyBox = Raylib.LoadModelFromMesh(cube);

            var shader = Raylib.LoadShader(Resources.Shaders.skyboxVertex.GetResourcesPath(), Resources.Shaders.skyboxFragment.GetResourcesPath());

            _skyBox.Materials[0].Shader = shader;

            int[] doGamma = { 0 };
            int[] vflipped = { 0 };
            int[] environmentMap = { (int)MaterialMapIndex.Cubemap };

            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "environmentMap"),  environmentMap , ShaderUniformDataType.Int);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "doGamma"),  doGamma, ShaderUniformDataType.Int);
            Raylib.SetShaderValue(shader, Raylib.GetShaderLocation(shader, "vflipped"), vflipped, ShaderUniformDataType.Int);

            var img = Raylib.LoadImage(Resources.Textures.skybox.GetResourcesPath());
            _skyBox.Materials[0].Maps[(int)MaterialMapIndex.Cubemap].Texture = Raylib.LoadTextureCubemap(img, CubemapLayout.AutoDetect);
            Raylib.UnloadImage(img);
        }

        ChunkShader = Raylib.LoadShader(Resources.Shaders.chunkVertex.GetResourcesPath(), Resources.Shaders.chunkFragment.GetResourcesPath());
        ShaderLocSunDirection = Raylib.GetShaderLocation(ChunkShader, "sunDirection");
        ShaderLocAlphaCutout = Raylib.GetShaderLocation(ChunkShader, "alphaCutout");
        ShaderLocTime = Raylib.GetShaderLocation(ChunkShader, "time");

        sunRenderTexture = Raylib.LoadRenderTexture(1000, 1000); //todo, what resolution should we pick?

        HuskModel = Models.LoadModel(Resources.husk);

        var host = new RaylibUiTreeHost();

        _renderer = new Renderer();


        Gl = GL.GetApi(new RaylibGlContext());
        _renderer.Initialize(Gl, host);

        UiTree = new UiTree(host, RenderUI);
    }

    public static GL Gl;

    public JemFile HuskModel; //should not be here

    public bool ChatIsActive;
    public static bool UiShouldReceiveInput;

    public void RenderUI(Ui ui)
    {
        using (ui.Rect().MainAlign(MAlign.End).CrossAlign(XAlign.Center))
        {
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
                                       .CrossAlign(XAlign.End).MainAlign(MAlign.End).PaddingHorizontal(2))
                            {
                                ui.Text(slot.Count.ToArenaString()).Size(30).Color(C.White);
                            }
                        }
                    }
                }
            }
        }

        //chat log
        using (ui.Rect().AbsoluteSize(0, 0).AbsolutePosition().MainAlign(MAlign.End))
        {
            ui.CascadingValues.TextColor = C.White;
            ui.CascadingValues.TextSize = 30;

            foreach (var logEntry in chatLog)
            {
                using (ui.Rect().ShrinkHeight().WidthFraction(30).Color(C.Black.WithAlpha(200)).Padding(5))
                {
                    ui.Text(logEntry);
                }
            }

            using (ui.Rect().HeightFraction(20))
            {

            }
        }

        //chat
        if (ChatIsActive)
        {
            ref string input = ref ui.GetString("");

            if (ui.Tree.IsKeyPressed(Key.Enter))
            {
                if (input.StartsWith("/"))
                {
                    ExecuteCommand(input.AsSpan(1));
                }
                else
                {
                    chatLog.Add(input);
                }

                CloseChat();
            }else if (ui.Tree.IsKeyPressed(Key.Escape))
            {
                CloseChat();
            }

            using (ui.Rect().AbsoluteSize(0, 0).AbsolutePosition().MainAlign(MAlign.End))
            {
                ui.CascadingValues.TextColor = C.White;
                ui.CascadingValues.TextSize = 30;

                using (ui.Rect().ShrinkHeight().Color(C.Black.WithAlpha(200)).Margin(5).Padding(5))
                {
                    ui.Input(ref input, true);
                }
            }
        }
    }

    private void ExecuteCommand(ReadOnlySpan<char> input)
    {

    }

    private void CloseChat()
    {
        UiShouldReceiveInput = false;
        ChatIsActive = false;
        Raylib.DisableCursor();
    }

    private Camera3D debugCamera = new(new Vector3(0, 100, 0), new Vector3(0, 0, 1), new Vector3(0, 1, 0), 100,
        CameraProjection.Perspective);

    private bool isDebugCamera = false;
    private bool isDebugControls = false;

    private List<string> chatLog = [];

    public void GameLoop()
    {
        while (!Raylib.WindowShouldClose())
        {
            if (Game.IsKeyPressed(KeyboardKey.Escape))
            {
                break;
            }

            if (Game.IsKeyPressed(KeyboardKey.G))
            {
                isDebugCamera = !isDebugCamera;
                isDebugControls = isDebugCamera;
                if (isDebugCamera)
                {
                    debugCamera.Position = _player.Camera.Position;
                    debugCamera.Target = _player.Camera.Target;
                    debugCamera.Up = _player.Camera.Up;
                    debugCamera.FovY = _player.Camera.FovY;
                }
            }

            if (Game.IsKeyPressed(KeyboardKey.K))
            {
                isDebugControls = !isDebugControls;
            }

            if (isDebugControls)
            {
                Raylib.UpdateCamera(ref debugCamera, CameraMode.Free);
            }
            else
            {
                Update();
            }

            Raylib.BeginDrawing();

            Raylib.ClearBackground(Color.Black);

            var camera = _player.Camera;
            if (isDebugCamera)
            {
                camera = debugCamera;
            }

            //Render sun depth map
            {
                Raylib.BeginTextureMode(sunRenderTexture);


                Raylib.ClearBackground(new Color(0, 0, 0, 0));

                // Rlgl.ColorMask(false, false, false, false);

                var preViewModel = Rlgl.GetMatrixModelview();
                var preProjection = Rlgl.GetMatrixProjection();

                float orthoSize = 100.0f;
                float nearPlane = 0;
                float farPlane  = 10000.0f;
                Matrix4x4 proj = Raymath.MatrixOrtho(-orthoSize, orthoSize, -orthoSize, orthoSize, nearPlane, farPlane);
                Rlgl.SetMatrixProjection(proj);

                Matrix4x4 view = Raymath.MatrixLookAt(
                    sunDirection * 2000.0f,
                    _player.Position,
                    new Vector3( 0.0f, 1.0f, 0.0f )
                );
                Rlgl.SetMatrixModelView(view);

                RenderSolidChunks();

                Rlgl.SetMatrixModelView(preViewModel);
                Rlgl.SetMatrixProjection(preProjection);

                // Rlgl.ColorMask(true, true, true, true);

                Raylib.EndTextureMode();
            }

            Raylib.BeginMode3D(camera);

                Draw3d();

            Raylib.EndMode3D();

            Draw2d();

            Raylib.DrawTexturePro(sunRenderTexture.Texture, new Rectangle(0, 1000, 1000, -1000), new Rectangle(0, 0, 600, 600), new Vector2(0, 0), 0, Color.White);

            Raylib.EndDrawing();
        }
    }

    private RenderTexture2D sunRenderTexture;


    private int TickCounter = 0;

    private void RunTickStep()
    {
        CurrentWorld.AdvanceTextureAnimation();

        TickCounter++;
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
        if (UiShouldReceiveInput)
        {
            string textInput = "";
            while (true)
            {
                var key = Raylib.GetCharPressed();
                if(key <= 0)
                    break;

                textInput += (char)key;
            }

            UiTree.TextInput = textInput;

            while (true)
            {
                var key = (KeyboardKey)Raylib.GetKeyPressed();

                if(key == 0)
                    break;

                UiTree.KeyPressed.Add((Key)key);
                UiTree.KeyDown.Add((Key)key);
            }

            foreach (var key in UiTree.KeyDown.ToArray())
            {
                if (Raylib.IsKeyUp((KeyboardKey)key))
                {
                    UiTree.KeyDown.Remove(key);
                    UiTree.KeyUp.Add(key);
                }
            }
        }



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

        if (Game.IsKeyPressed(KeyboardKey.F3))
        {
            DevTools.DevToolsEnabled = !DevTools.DevToolsEnabled;
        }

        if (Game.IsKeyDown(KeyboardKey.LeftControl) && Game.IsKeyPressed(KeyboardKey.S))
        {
            CurrentWorld.SaveToDirectory(SaveLocation);
        }

        if (Game.IsKeyPressed(KeyboardKey.T))
        {
            ChatIsActive = true;
            UiShouldReceiveInput = true;
            Raylib.EnableCursor();
        }
    }

    public static bool IsMouseButtonPressed(MouseButton mouseButton)
    {
        return !UiShouldReceiveInput && Raylib.IsMouseButtonPressed(mouseButton);
    }

    public static Vector2 GetMouseDelta()
    {
        if (UiShouldReceiveInput)
            return Vector2.Zero;

        return Raylib.GetMouseDelta();
    }

    public static float GetMouseWheelMove()
    {
        if (UiShouldReceiveInput)
            return 0;

        return Raylib.GetMouseWheelMove();
    }

    public static bool IsKeyDown(KeyboardKey key)
    {
        return !UiShouldReceiveInput && Raylib.IsKeyDown(key);
    }

    public static bool IsKeyPressed(KeyboardKey key)
    {
        return !UiShouldReceiveInput && Raylib.IsKeyPressed(key);
    }


    public static bool IsKeyUp(KeyboardKey key)
    {
        return !UiShouldReceiveInput && Raylib.IsKeyUp(key);
    }


    public static bool IsKeyReleased(KeyboardKey key)
    {
        return !UiShouldReceiveInput && Raylib.IsKeyReleased(key);
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
    }

    private Vector3 sunDirection = Vector3.Normalize(new Vector3(-0.2f, 1, -1));

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

        RenderSolidChunks();

        Raylib.SetShaderValue(ChunkShader, ShaderLocAlphaCutout, [-1f], ShaderUniformDataType.Float);

        foreach (var (_, chunk) in CurrentWorld.Chunks.OrderBy(x => x.Value.DistanceToPlayerAtRenderTime))
        {
            var pos = new Vector3(chunk.Pos.X * 16, chunk.Pos.Y * 16, chunk.Pos.Z * 16);

            if (ChunkShouldBeRendered(chunk.Pos.ToVector3NonCenter()))
            {
                if (chunk.HasSemitransparentMesh)
                {
                    Raylib.DrawModel(chunk.ModelSemiTransparent, pos, 1, Color.White);
                }
            }
        }

        if (_player.lookingAtBlock.HasValue)
        {
            var chunk = CurrentWorld.GetChunkContainingBlock(_player.lookingAtBlock.Value);
            if (chunk != null)
            {
                DevTools.Print(chunk.DistanceToPlayerAtRenderTime, "Chunk Distance");
            }
        }

        // Rlgl.EnableColorBlend();
        // Raylib.EndBlendMode();

        _player.Render();
        

        foreach (var bot in CurrentWorld.bots)
        {
            bot.Render();
        }
    }

    public void RenderSolidChunks()
    {
        Gl.Disable(GLEnum.Blend);

        Raylib.SetShaderValue(ChunkShader, ShaderLocSunDirection, [sunDirection.X, sunDirection.Y, sunDirection.Z], ShaderUniformDataType.Vec3);
        Raylib.SetShaderValue(ChunkShader, ShaderLocAlphaCutout, [0.5f], ShaderUniformDataType.Float);
        Raylib.SetShaderValue(ChunkShader, ShaderLocTime, (float)Raylib.GetTime(), ShaderUniformDataType.Float);

        foreach (var (_, chunk) in CurrentWorld.Chunks)
        {
            // chunk.DistanceToPlayerAtRenderTime = GetDistanceToClosestCorner(chunk.Pos.ToVector3NonCenter());

            var pos = new Vector3(chunk.Pos.X * 16, chunk.Pos.Y * 16, chunk.Pos.Z * 16);

            // if (ChunkShouldBeRendered(chunk.Pos.ToVector3NonCenter()))
            {
                if (chunk.HasMesh)
                {
                    Raylib.DrawModel(chunk.Model, pos, 1, Color.White);
                }
            }
        }

        Gl.Enable(GLEnum.Blend);
    }

    public bool ChunkShouldBeRendered(Vector3 chunkPosition)
    {
        foreach (var corner in CubeCorners)
        {
            var cornerPos = (chunkPosition + corner) * 16;
            
            var cornerVector =  cornerPos - _player.Camera.Position;

            var dot = Vector3.Dot(Vector3.Normalize(_player.Direction), Vector3.Normalize(cornerVector));
            var angle = Math.Acos(dot) * Raylib.RAD2DEG;

            if (angle < _player.Camera.FovY / 2 + 30)
                return true;
        }

        return false;
    }

    public float GetDistanceToClosestCorner(Vector3 chunkPosition)
    {
        float minDistance = float.MaxValue;

        foreach (var corner in CubeCorners)
        {
            var cornerPos = (chunkPosition + corner) * 16;

            var cornerVector =  cornerPos - _player.Camera.Position;

            var projectedDistance = Vector3.Dot(Vector3.Normalize(_player.Direction), cornerVector);

            //var distance = Vector3.Distance(cornerPos, _player.Camera.Position);
            if (projectedDistance < minDistance)
                minDistance = projectedDistance;
        }

        return minDistance;
    }
    
    public static Vector3[] CubeCorners = [
        new Vector3(1, 0, 0),
        new Vector3(0, 0, 0),
        new Vector3(1, 1, 0),
        new Vector3(0, 1, 0),
        new Vector3(1, 1, 1),
        new Vector3(0, 1, 1),
        new Vector3(1, 0, 1),
        new Vector3(0, 0, 1),
    ];

    public static IntVector3[] HorizontalPermutationsWithDiagonal = [
        new IntVector3(0, 0, 0),
        new IntVector3(1, 0, 0),
        new IntVector3(-1, 0, 0),
        new IntVector3(0, 0, 1),
        new IntVector3(0, 0, -1),
        new IntVector3(1, 0, 1),
        new IntVector3(-1, 0, -1),
        new IntVector3(1, 0, -1),
        new IntVector3(-1, 0, 1),

    ];
}

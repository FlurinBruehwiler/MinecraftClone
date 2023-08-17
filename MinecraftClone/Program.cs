using System.Numerics;
using Raylib_cs;
using RayLib3dTest;

const int screenWidth = 1920;
const int screenHeight = 1080;


InitWindow(screenWidth, screenHeight, "3dtest");

var camera = new Camera3D(Vector3.Zero, Vector3.One, new Vector3(0, 1, 0), 60, CameraProjection.CAMERA_PERSPECTIVE)
{
    fovy = 100
};

DisableCursor();
SetTargetFPS(120);
SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

var blocks = new Blocks();
var textures = new Textures(blocks);
var merger = new ThinkTexture(textures);
merger.Merge();

var texture = LoadTexture("resources/textureatlas.png");
// var shader = LoadShader("Resources/shader.glsl", "Resources/shader.glsl");

// var grassModel = LoadModel("grass.obj");

// merger.GenerateBlockPreviews(texture);
var globalBoy = new GlobalBoy(texture);

var colcol = new Colcol(globalBoy);
var sirPhysics = new SirPhysics(colcol);

var mrPerlin = new MrPerlin(0);

var chunker = new Chunker(globalBoy, textures, mrPerlin);

float speed = 1f;
const float sens = 60;

bool isFlying = true;

List<Vector3> debugPoints = new();

var selectedBlock = blocks.BlockList.First().Value;

var stefano = new Vector3(50, 16, 50);

while (!WindowShouldClose())
{
    var movDelta = new Vector3(0, isFlying ? 0 : -.1f, 0);
    var rotDelta = GetMouseDelta();

    var playerSpeed = speed;

    var x = GetKeyPressed();

    if (x is >= 48 and <= 57)
    {
        var idx = x - 49;
        if (blocks.BlockList.TryGetValue((ushort)idx, out var bd))
        {
            selectedBlock = bd;
        }
    }
    
    if (IsKeyDown(KeyboardKey.KEY_W))
    {
        movDelta.X += playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_S))
    {
        movDelta.X -= playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_D))
    {
        movDelta.Z += playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_A))
    {
        movDelta.Z -= playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_SPACE))
    {
        movDelta.Y += playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
    {
        movDelta.Y -= playerSpeed;
    }
    
    if (float.IsInfinity(movDelta.Y))
        return;

    if (!isFlying)
    {
        sirPhysics.VerticalCollisions(ref movDelta, camera.position);
        sirPhysics.ForwardCollisions(ref movDelta, camera.position);
        sirPhysics.SidewardCollisions(ref movDelta, camera.position);
    }
 
    if (IsKeyPressed(KeyboardKey.KEY_ENTER))
    {
        isFlying = !isFlying;
    } 
    
    //list of nice names: deepsign

    if (GetMouseWheelMoveV().Y > 0)
    {
        speed *= 1.1f;
    }else if (GetMouseWheelMoveV().Y < 0)
    {
        speed *= 0.9f;
    }
    
    speed = Math.Max(speed, 0);

    if (float.IsInfinity(movDelta.Y))
        return;
    
    UpdateCameraPro(ref camera, new Vector3(movDelta.X, movDelta.Z, movDelta.Y) * sens * GetFrameTime(), new Vector3(rotDelta * 0.5f, 0), 0);

    if (float.IsNaN(camera.position.Z))
        return;
    
    chunker.LoadChunksIfNeccesary(camera.position);
    
    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT))
    {
        debugPoints.Clear();
        
        var col = colcol.Raycast(camera.position, camera.target - camera.position, 10, out _, out _);
        if (col is not null)
        {
            ref var b = ref globalBoy.TryGetBlockAtPos(col.Value, out var wasFound);
            if (wasFound)
            {
                b.BlockId = Blocks.Air.ID;
                
                var chunk = globalBoy.GetChunk(col.Value);
                chunk.GenMesh();
            }
        }
    }

    if (IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT))
    {
        var col = colcol.Raycast(camera.position, camera.target - camera.position, 10, out var previousBlock, out _);
        if (col is not null)
        {
            ref var b = ref globalBoy.TryGetBlockAtPos(previousBlock, out var wasFound);
            if (wasFound)
            {
                b.BlockId = selectedBlock.ID;
                
                var chunk = globalBoy.GetChunk(previousBlock);
                chunk.GenMesh();
            }
        }
    }

    BeginDrawing();
    
    ClearBackground(Color.RAYWHITE);

    BeginMode3D(camera);

    foreach (var (_, chunk) in globalBoy.Chunks)
    {
        DrawModel(chunk.Model, new Vector3(chunk.Pos.X * 16, chunk.Pos.Y, chunk.Pos.Z * 16), 1, Color.WHITE);
    }
    
    DrawCube(stefano, 1, 1, 1, Color.BROWN);
    DrawCube(stefano with { Y = stefano.Y - 1 }, 1, 1, 1, Color.BROWN);
    
    EndMode3D();

    DrawRectangle(90, 90, 200, 100, new Color(0, 0, 0, 100));
    DrawText((1 / GetFrameTime()).ToString(), 100, 100, 20,
        Color.RED);
    DrawText($"{camera.position.X}, {camera.position.Y}, {camera.position.Z}", 100, 120, 20,
        Color.RED);

    var topLeft = screenWidth / 2 - 100 * blocks.BlockList.Count / 2;
    DrawRectangle(topLeft, screenHeight - 100, 100 * blocks.BlockList.Count, 100, new Color(0, 0, 0, 100));
    foreach (var blockDefinition in blocks.BlockList)
    {
        if (blockDefinition.Value == selectedBlock)
        {
            DrawRectangle(topLeft, screenHeight - 100, 100, 100, new Color(0, 0, 0, 200));
        }
        DrawText(blockDefinition.Value.Name, topLeft, screenHeight - 100, 12, Color.WHITE);
        topLeft += 100;
    }
    

    //crosshair
    const int thikness = 5;
    const int length = 10;

    const int centerX = screenWidth / 2;
    const int centerY = screenHeight / 2;
    
    DrawRectangle(centerX - thikness / 2, centerY - length, thikness, length * 2, Color.BLACK);
    DrawRectangle(centerX - length, centerY - thikness / 2, length * 2, thikness, Color.BLACK);

    EndDrawing();
}

CloseWindow();


public record struct Block
{
    public ushort BlockId;

    public bool IsAir()
    {
        return BlockId == Blocks.Air.ID;
    }
}

public enum BlockFace
{
    Left,
    Right,
    Bottom,
    Top,
    Back,
    Front
}

public record struct IntVector3(int X, int Y, int Z)
{
    public static IntVector3 operator +(IntVector3 left, IntVector3 right)
    {
        return new IntVector3(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z
        );
    }

    public static IntVector3 operator *(IntVector3 left, int factor)
    {
        return new IntVector3(
            left.X * factor,
            left.Y * factor,
            left.Z * factor
        );
    }
}

public record struct IntVector2(int X, int Y)
{
    public static IntVector2 operator +(IntVector2 left, IntVector2 right)
    {
        return new IntVector2(
            left.X + right.X,
            left.Y + right.Y
        );
    }

    public static IntVector2 operator *(IntVector2 left, int factor)
    {
        return new IntVector2(
            left.X * factor,
            left.Y * factor
        );
    }
}
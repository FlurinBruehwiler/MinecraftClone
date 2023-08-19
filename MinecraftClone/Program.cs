using System.Numerics;
using Raylib_cs;
using RayLib3dTest;

const int screenWidth = 1800;
const int screenHeight = 1000;


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
List<PrintMessage> printMessages = new();
List<DebugLine> debugLines = new();
List<DebugLine2d> debugLines2d = new();


bool isFlying = true;

List<Vector3> debugPoints = new();

var selectedBlock = blocks.BlockList.First().Value;

var stefano = new Vector3(50, 16, 50);

while (!WindowShouldClose())
{
    var localMoveDelta = new Vector3(0, isFlying ? 0 : -.1f, 0);
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
        localMoveDelta.X += playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_S))
    {
        localMoveDelta.X -= playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_D))
    {
        localMoveDelta.Z += playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_A))
    {
        localMoveDelta.Z -= playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_SPACE))
    {
        localMoveDelta.Y += playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
    {
        localMoveDelta.Y -= playerSpeed;
    }
    
    if (float.IsInfinity(localMoveDelta.Y))
        return;

    Print(localMoveDelta, nameof(localMoveDelta));

    var right = Vector3.Normalize(new Vector3(camera.GetRight().X, 0, camera.GetRight().Z));
    var forward = Vector3.Normalize(new Vector3(-camera.GetRight().Z, 0, camera.GetRight().X));
    
    Print(camera.GetRight(), "realright");
    Print(right, nameof(right));
    Print(forward, nameof(forward));
    
    var xComponent = right * localMoveDelta.X;
    var zComponent = forward * localMoveDelta.Z;
    
    var globalMoveDelta = xComponent + zComponent;
    globalMoveDelta.Y = localMoveDelta.Y;

    if (!isFlying)
    {
        sirPhysics.VerticalCollisions(ref globalMoveDelta, camera.position);
        sirPhysics.ForwardCollisions(ref globalMoveDelta, camera.position);
        sirPhysics.SidewardCollisions(ref globalMoveDelta, camera.position);
    }

    var localX = Vector3.Dot(right, globalMoveDelta);
    var localZ = Vector3.Dot(forward, globalMoveDelta);

    var newlocalMoveDelta = new Vector3(localX, globalMoveDelta.Y, localZ);
    
    Print(newlocalMoveDelta, nameof(newlocalMoveDelta) + "2");
 
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

    if (float.IsInfinity(newlocalMoveDelta.Y))
        return;
    
    UpdateCameraPro(ref camera, new Vector3(newlocalMoveDelta.X, newlocalMoveDelta.Z, newlocalMoveDelta.Y) * sens * GetFrameTime(), new Vector3(rotDelta * 0.5f, 0), 0);

    if (float.IsNaN(camera.position.Z))
        return;
    
    Print(camera.position, "pos");
    
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
    
    foreach (var debugLine in debugLines)
    {
        DrawLine3D(debugLine.start, debugLine.start + debugLine.direction, debugLine.Color);
    }
    debugLines.Clear();
    
    EndMode3D();

    DrawRectangle(10, 10, 600, 600, new Color(0, 0, 0, 100));
    for (var i = 0; i < printMessages.Count; i++)
    {
        var printMessage = printMessages[i];
        DrawText($"{printMessage.name}: {printMessage.value}", 20, i * 30 + 20, 20, Color.WHITE);
    }
    printMessages.Clear();
    
    for (var i = 0; i < debugLines2d.Count; i++)
    {
        var debugLine = debugLines2d[i];
        DrawLine(300, 300, (int)(300 + debugLine.direction.X * 100), (int)(debugLine.direction.Y * 100 + 300), debugLine.Color);
    }
    debugLines2d.Clear();

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


void Print(object value, string name)
{
    printMessages.Add(new PrintMessage(value.ToString(), name));
}

void DrawDebugLine(Vector3 start, Vector3 direction, Color color)
{
    debugLines.Add(new DebugLine(start, direction, color));
}

void DrawDebugLine2d(Vector2 direction, Color color)
{
    debugLines2d.Add(new DebugLine2d(direction, color));
}


record struct PrintMessage(string value, string name);
record struct DebugLine(Vector3 start, Vector3 direction, Color Color);
record struct DebugLine2d(Vector2 direction, Color Color);

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
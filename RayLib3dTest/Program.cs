using System.Numerics;
using Raylib_cs;
using RayLib3dTest;

const int screenWidth = 1000;
const int screenHeight = 700;


InitWindow(screenWidth, screenHeight, "3dtest");

var camera = new Camera3D(Vector3.Zero, Vector3.One, new Vector3(0, 1, 0), 60, CameraProjection.CAMERA_PERSPECTIVE);

DisableCursor();
SetTargetFPS(120);
SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

var blocks = new Blocks();
var textures = new Textures(blocks);
var merger = new TextureManager(textures);
merger.Merge();

var texture = LoadTexture("resources/textureatlas.png");

var globalBoy = new GlobalBoy(textures)
{
    Texture2D = texture
};

var data = MrPerlin.GenerateNoiseMap(400, 400, 1, 5, 5);

foreach (var chunk in globalBoy.Chunks)
{
    for (var x = 0; x < chunk.Blocks.GetLength(0); x++)
    {
        for (var z = 0; z < chunk.Blocks.GetLength(2); z++)
        {
            var height = (int)(Math.Clamp(data[(chunk.Pos.X * 16 + x) * 400 + (chunk.Pos.Z * 16 + z)], 0, 1) * 16);
            for (var y = 0; y < chunk.Blocks.GetLength(1); y++)
            {
                if (y > height)
                {
                    chunk.Blocks[x, y, z].BlockId = Blocks.Air.ID;
                }
                else if (y == height)
                {
                    chunk.Blocks[x, y, z].BlockId = Blocks.Gras.ID;
                }
                else
                {
                    chunk.Blocks[x, y, z].BlockId = Blocks.Dirt.ID;
                }
            }
        }
    }

    chunk.GenMesh();
    chunk.GenModel();
}

float speed = 60;
const float sens = 60;

while (!WindowShouldClose())
{
    var movDelta = new Vector3();
    var rotDelta = GetMouseDelta();

    var playerSpeed = speed * GetFrameTime();

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
        movDelta.Y += playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_A))
    {
        movDelta.Y -= playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_SPACE))
    {
        movDelta.Z += playerSpeed;
    }

    if (IsKeyDown(KeyboardKey.KEY_LEFT_SHIFT))
    {
        movDelta.Z -= playerSpeed;
    }

    speed += GetMouseWheelMoveV().Y * 5;
    speed = Math.Max(speed, 0);

    UpdateCameraPro(ref camera, movDelta * sens * GetFrameTime(), new Vector3(rotDelta * 0.5f, 0), 0);

    BeginDrawing();

    ClearBackground(Color.RAYWHITE);

    BeginMode3D(camera);

    foreach (var chunk in globalBoy.Chunks)
    {
        DrawModel(chunk.Model, new Vector3(chunk.Pos.X * 16, chunk.Pos.Y, chunk.Pos.Z * 16), 1, Color.WHITE);
    }

    DrawCube(new Vector3(0, 0, 0), 1, 1, 1, Color.BLACK);

    EndMode3D();

    DrawRectangle(90, 90, 200, 100, new Color(0, 0, 0, 100));
    DrawText((1 / GetFrameTime()).ToString(), 100, 100, 20, Color.RED);
    DrawText($"{(int)camera.position.X}, {(int)camera.position.Y}, {(int)camera.position.Z}, ", 100, 120, 20,
        Color.RED);

    EndDrawing();
}

CloseWindow();


public record struct Block
{
    public int BlockId;

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
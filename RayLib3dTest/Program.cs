using System.Numerics;
using Raylib_cs;
using RayLib3dTest;

const int screenWidth = 1000;
const int screenHeight = 700;


InitWindow(screenWidth, screenHeight, "3dtest");

var camera = new Camera3D(Vector3.Zero, Vector3.One, new Vector3(0, 1, 0), 60, CameraProjection.CAMERA_PERSPECTIVE);

DisableCursor();
SetTargetFPS(60);
SetConfigFlags(ConfigFlags.FLAG_MSAA_4X_HINT);

var texture = LoadTexture("resources/grass_block_side.png");

var globalBoy = new GlobalBoy()
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
            var height = Math.Clamp(data[(chunk.Pos.X * 16 + x) * 400 + (chunk.Pos.Z * 16 + z)], 0f, 1f) * 16;
            for (var y = 0; y < chunk.Blocks.GetLength(1); y++)
            {
                if (y > height)
                {
                    chunk.Blocks[x, y, z].IsAir = true;
                }
            }
        }
    }

    chunk.GenMesh();
    chunk.GenModel();
}

const float playerSpeed = 1;

while (!WindowShouldClose())
{
    var movDelta = new Vector3();
    var rotDelta = GetMouseDelta();
    
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
    
    UpdateCameraPro(ref camera, movDelta, new Vector3(rotDelta * 0.5f, 0), 0);    

    BeginDrawing();

    ClearBackground(Color.RAYWHITE);

    BeginMode3D(camera);

    foreach (var chunk in globalBoy.Chunks)
    {
        DrawModel(chunk.Model, new Vector3(chunk.Pos.X * 16, 0, chunk.Pos.Z * 16), 1, Color.WHITE);
    }
    
    EndMode3D();

    DrawText((1 / GetFrameTime()).ToString(), 100, 100, 20, Color.RED);
    
    EndDrawing();
}

CloseWindow();


public record struct Block()
{
    public Color Color;
    public bool IsAir;
}

public enum Neighbour
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
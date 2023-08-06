

using System.Numerics;
using Raylib_cs;
using RayLib3dTest;

const int screenWidth = 1000;
const int screenHeight = 700;

InitWindow(screenWidth, screenHeight, "3dtest");

var camera = new Camera3D(Vector3.Zero, Vector3.One, new Vector3(0, 1, 0), 60, CameraProjection.CAMERA_PERSPECTIVE);

DisableCursor();
SetTargetFPS(60);


var chunks = new List<Chunk>();

for (var x = 0; x < 1; x++)
{
    for (var y = 0; y < 1; y++)
    {
        chunks.Add(new Chunk
        {
            Pos = new Vector2(x, y)
        });
    }
}

foreach (var chunk in chunks)
{
    var col1 = new Color(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255), 255);
    var col2 = new Color(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255), 255);

    for (var x = 0; x < chunk.Blocks.GetLength(0); x++)
    {
        for (var y = 0; y < chunk.Blocks.GetLength(1); y++)
        {
            for (var z = 0; z < chunk.Blocks.GetLength(2); z++)
            {
                chunk.Blocks[x, y, z].Color = Random.Shared.Next(2) == 1 ? col1 : col2;
                // chunk.Blocks[x, y, z].IsAir = true;
                //
                // if (x == 0 && y == 0 && z == 0)
                // {
                //     chunk.Blocks[x, y, z].IsAir = false;
                // }
                //
                // if (x == 0 && y == 0 && z == 1)
                // {
                //     chunk.Blocks[x, y, z].IsAir = false;
                // }
            }
        }
    }

    chunk.GenMesh();
    chunk.GenModel();
}

while (!WindowShouldClose())
{
    unsafe
    {
        UpdateCamera(&camera, CameraMode.CAMERA_FREE);
    }

    BeginDrawing();

    ClearBackground(Color.RAYWHITE);

    BeginMode3D(camera);
    
    DrawGrid(20, 0.5f);
    
    foreach (var chunk in chunks)
    {
        DrawModel(chunk.Model, new Vector3(chunk.Pos.X * 16, chunk.Pos.Y * 16, 0), 1, Color.WHITE);
    }
    
    EndMode3D();

    EndDrawing();
}

CloseWindow();


record struct Block
{
    public Color Color;
    public bool IsAir;
}

enum Neighbour
{
    Left,
    Right,
    Bottom,
    Top,
    Back,
    Front
}

record struct IntVector3(int X, int Y, int Z)
{
    public static IntVector3 operator +(IntVector3 left, IntVector3 right)
    {
        return new IntVector3(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z
        );
    }
}
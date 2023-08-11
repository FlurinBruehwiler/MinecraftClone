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

for (var x = 0; x < 10; x++)
{
    for (var z = 0; z < 10; z++)
    {
        chunks.Add(new Chunk
        {
            Pos = new Vector2(x, z)
        });
    }
}

foreach (var chunk in chunks)
{
    for (var x = 0; x < chunk.Blocks.GetLength(0); x++)
    {
        for (var z = 0; z < chunk.Blocks.GetLength(2); z++)
        {
            var blockPosX = chunk.Pos.X * 16 + x; 
            var heightX = Math.Sin(blockPosX*10*Math.PI/180);
            var betterHeightX = (heightX + 1) * 4; 
            
            var blockPosZ = chunk.Pos.Y * 16 + z; 
            var heightZ = Math.Sin(blockPosZ*10*Math.PI/180);
            var betterHeightZ = (heightZ + 1) * 4;

            var finalHeight = betterHeightX + betterHeightZ;
            
            for (var y = 0; y < chunk.Blocks.GetLength(1); y++)
            {
                if (y > finalHeight)
                {
                    chunk.Blocks[x, y, z].IsAir = true;
                }
            }
        }
    }

    chunk.GenMesh();
    chunk.GenModel();
}

while (!WindowShouldClose())
{
    UpdateCamera(ref camera, CameraMode.CAMERA_CUSTOM);

    BeginDrawing();

    ClearBackground(Color.RAYWHITE);

    BeginMode3D(camera);
    
    DrawGrid(20, 0.5f);
    
    foreach (var chunk in chunks)
    {
        DrawModel(chunk.Model, new Vector3(chunk.Pos.X * 16, 0, chunk.Pos.Y * 16), 1, Color.WHITE);
    }
    
    EndMode3D();

    EndDrawing();
}

CloseWindow();


record struct Block()
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
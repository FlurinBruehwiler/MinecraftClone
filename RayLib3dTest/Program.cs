using System.Numerics;
using System.Security.Cryptography.X509Certificates;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Rlgl;

const int screenWidth = 1000;
const int screenHeight = 700;

InitWindow(screenWidth, screenHeight, "3dtest");

var camera = new Camera3D(Vector3.Zero, Vector3.One, new Vector3(0, 1, 0), 60, CameraProjection.CAMERA_PERSPECTIVE);

DisableCursor();
SetTargetFPS(60);

// var cubes = new List<Vector3>();
//
// for (var i = 0; i < 100; i++)
// {
//     cubes.Add(new Vector3(Random.Shared.NextSingle() * 10, Random.Shared.NextSingle() * 10, Random.Shared.NextSingle() * 10));
// }

var chunks = new List<Chunk>();

for (var x = 0; x < 2; x++)
{
    for (var y = 0; y < 2; y++)
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
            }
        }
    }
}

foreach (var chunk in chunks)
{
    for (var x = 0; x < chunk.Blocks.GetLength(0); x++)
    {
        for (var y = 0; y < chunk.Blocks.GetLength(1); y++)
        {
            for (var z = 0; z < chunk.Blocks.GetLength(2); z++)
            {
                var pos = new IntVector3(x, y, z);
                AddQuadFor(chunk, pos, Neighbour.Left);
                AddQuadFor(chunk, pos, Neighbour.Right);
                AddQuadFor(chunk, pos, Neighbour.Top);
                AddQuadFor(chunk, pos, Neighbour.Bottom);
                AddQuadFor(chunk, pos, Neighbour.Back);
                AddQuadFor(chunk, pos, Neighbour.Front);
            }
        }
    }
}

void AddQuadFor(Chunk chunk, IntVector3 block, Neighbour neighbour)
{
    var neighbourBlock = GetBlockAtPos(chunk, block + GetOffset(neighbour));

    if (neighbourBlock is { IsAir: false })
    {
        chunk.Faces.Add(new Face(block, neighbour));
    }
}

IntVector3 GetOffset(Neighbour neighbour)
{
    switch (neighbour)
    {
        case Neighbour.Left:
            return new IntVector3(-1, 0, 0);
        case Neighbour.Right:
            return new IntVector3(1, 0, 0);
        case Neighbour.Bottom:
            return new IntVector3(0, -1, 0);
        case Neighbour.Top:
            return new IntVector3(0, 1, 0);
        case Neighbour.Back:
            return new IntVector3(0, 0, -1);
        case Neighbour.Front:
            return new IntVector3(0, 0, 1);

        default:
            throw new ArgumentOutOfRangeException(nameof(neighbour), neighbour, null);
    }
}

Block? GetBlockAtPos(Chunk chunk, IntVector3 intVector3)
{
    if (intVector3.X > 15)
        return null;

    if (intVector3.Y > 15)
        return null;

    if (intVector3.Z > 15)
        return null;

    return chunk.Blocks[intVector3.X, intVector3.Y, intVector3.Z];
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

    foreach (var chunk in chunks)
    {
        for (var x = 0; x < chunk.Blocks.GetLength(0); x++)
        {
            for (var y = 0; y < chunk.Blocks.GetLength(1); y++)
            {
                for (var z = 0; z < chunk.Blocks.GetLength(2); z++)
                {
                    // DrawCube(new Vector3(x + (chunk.Pos.X * 16), y + (chunk.Pos.Y * 16), z), 1, 1, 1, chunk.Blocks[x, y, z].Color);
                    
                    
                    rlBegin(DrawMode.QUADS);
                    
                    
                }
            }
        }
    }
    
    EndMode3D();
    
    EndDrawing();
}

CloseWindow();

class Chunk
{
    public Chunk()
    {
        Blocks = new Block[16,16,16];
    }

    public Block[,,] Blocks { get; set; }
    public List<Face> Faces { get; set; }
    public required Vector2 Pos { get; init; }
}

record struct Face(IntVector3 BlockPos, Neighbour Neighbour);

record struct Block
{
    public Color Color;
    public bool IsAir;
}

enum Neighbour{
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
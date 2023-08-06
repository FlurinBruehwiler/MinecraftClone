using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;
using static Raylib_cs.Rlgl;

const int screenWidth = 1000;
const int screenHeight = 700;

var bottomLeftFront = new IntVector3(0,0,0);
var bottomRightFront = new IntVector3(1,0,0);
var bottomLeftBack = new IntVector3(0,1,0);
var bottomRightBack = new IntVector3(1,1,0);
var topLeftFront = new IntVector3(0,0,1);
var topRightFront = new IntVector3(1,0,1);
var topLeftBack = new IntVector3(0,1,1);
var topRightBack = new IntVector3(1,1,1);

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
                chunk.Blocks[x, y, z].IsAir = true;
                
                if (x == 0 && y == 0 && z == 0)
                {
                    chunk.Blocks[x, y, z].IsAir = false;
                }
                
                if (x == 0 && y == 0 && z == 1)
                {
                    chunk.Blocks[x, y, z].IsAir = false;
                }
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
                var block = chunk.Blocks[x, y, z];
                if (!block.IsAir)
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
}

void AddQuadFor(Chunk chunk, IntVector3 block, Neighbour neighbour)
{
    var neighbourBlock = GetBlockAtPos(chunk, block + GetOffset(neighbour));
    
    if (neighbourBlock is null || neighbourBlock.Value.IsAir)
    {
        chunk.Faces.Add(new Face(block, neighbour, chunk.Blocks[block.X, block.Y, block.Z].Color));
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
    if (intVector3.X is > 15 or < 0)
        return null;

    if (intVector3.Y is > 15 or < 0)
        return null;

    if (intVector3.Z is > 15 or < 0)
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

    DrawGrid(10, 1.0f);
    
    rlPushMatrix();
    rlTranslatef(0, 0, 0);
        
    rlBegin(DrawMode.QUADS);
        
    rlColor4ub(100, 100, 100, 255);
    
    foreach (var chunk in chunks)
    {
        foreach (var face in chunk.Faces)
        {
            rlColor4ub(face.Color.r, face.Color.g, face.Color.b, face.Color.a);
            // var normal = GetOffset(face.Neighbour);
            // rlNormal3f(normal.X, normal.Y, normal.Z);
            //
            switch (face.Neighbour)
            {
                case Neighbour.Left:
                    DrawVertex(face.BlockPos + topLeftBack);
                    DrawVertex(face.BlockPos + bottomLeftBack);
                    DrawVertex(face.BlockPos + bottomLeftFront);
                    DrawVertex(face.BlockPos + topLeftFront);
                    break;
                case Neighbour.Right:
                    DrawVertex(face.BlockPos + topRightBack);
                    DrawVertex(face.BlockPos + topRightFront);
                    DrawVertex(face.BlockPos + bottomRightFront);
                    DrawVertex(face.BlockPos + bottomRightBack);
                    break;
                case Neighbour.Bottom:
                    DrawVertex(face.BlockPos + bottomRightFront);
                    DrawVertex(face.BlockPos + bottomLeftFront);
                    DrawVertex(face.BlockPos + bottomLeftBack);
                    DrawVertex(face.BlockPos + bottomRightBack);
                    break;
                case Neighbour.Top:
                    DrawVertex(face.BlockPos + topRightBack);
                    DrawVertex(face.BlockPos + topLeftBack);
                    DrawVertex(face.BlockPos + topLeftFront);
                    DrawVertex(face.BlockPos + topRightFront);
                    break;
                case Neighbour.Back:
                    DrawVertex(face.BlockPos + topLeftBack);
                    DrawVertex(face.BlockPos + topRightBack);
                    DrawVertex(face.BlockPos + bottomRightBack);
                    DrawVertex(face.BlockPos + bottomLeftBack);
                    break;
                case Neighbour.Front:
                    DrawVertex(face.BlockPos + topRightFront);
                    DrawVertex(face.BlockPos + topLeftFront);
                    DrawVertex(face.BlockPos + bottomLeftFront);
                    DrawVertex(face.BlockPos + bottomRightFront);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
    
    rlEnd();
        
    rlPopMatrix();
    
    EndMode3D();
    
    EndDrawing();
}

CloseWindow();

void DrawVertex(IntVector3 intVector3)
{
    rlVertex3f(intVector3.X, intVector3.Y, intVector3.Z);
}

class Chunk
{
    public Chunk()
    {
        Blocks = new Block[16,16,16];
    }

    public Block[,,] Blocks { get; set; }
    public List<Face> Faces { get; set; } = new();
    public required Vector2 Pos { get; init; }
}

record struct Face(IntVector3 BlockPos, Neighbour Neighbour, Color Color);

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
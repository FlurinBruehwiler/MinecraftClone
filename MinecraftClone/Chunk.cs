using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace RayLib3dTest;

public class Chunk : IDisposable
{
    private readonly World _world;
    public bool HasMesh;
    public Mesh Mesh;
    public Model Model;
    public Block[] Blocks;
    public required IntVector3 Pos;

    public static readonly IntVector3 _bottomLeftFront = new(0, 0, 0);
    public static readonly IntVector3 _bottomRightFront = new(1, 0, 0);
    public static readonly IntVector3 _bottomLeftBack = new(0, 0, 1);
    public static readonly IntVector3 _bottomRightBack = new(1, 0, 1);
    public static readonly IntVector3 _topLeftFront = new(0, 1, 0);
    public static readonly IntVector3 _topRightFront = new(1, 1, 0);
    public static readonly IntVector3 _topLeftBack = new(0, 1, 1);
    public static readonly IntVector3 _topRightBack = new(1, 1, 1);

    public Chunk(World world)
    {
        _world = world;
        Blocks = new Block[16 * 16 * 16];
    }

    public Chunk(World world, Block[] blocks)
    {
        _world = world;
        Blocks = blocks;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIdx(int x, int y, int z)
    {
        return x + y * 16 + z * 16 * 16;
    }

    public unsafe void GenMesh()
    {
        if (Mesh.Vertices != (void*)IntPtr.Zero)
        {
            Raylib.UnloadModel(Model);
        }

        var mesh = new Mesh();

        var verticesList = new List<Vertex>();

        var startTime = Stopwatch.GetTimestamp();

        for (var x = 0; x < 16; x++)
        {
            for (var y = 0; y < 16; y++)
            {
                for (var z = 0; z < 16; z++)
                {
                    var block = Blocks[GetIdx(x, y, z)];

                    var pos = new IntVector3(x, y, z);

                    if (!block.IsAir())
                    {
                        AddQuadFor(pos, block.BlockId, BlockFace.Left, verticesList);
                        AddQuadFor(pos, block.BlockId, BlockFace.Right, verticesList);
                        AddQuadFor(pos, block.BlockId, BlockFace.Top, verticesList);
                        AddQuadFor(pos, block.BlockId, BlockFace.Bottom, verticesList);
                        AddQuadFor(pos, block.BlockId, BlockFace.Back, verticesList);
                        AddQuadFor(pos, block.BlockId, BlockFace.Front, verticesList);
                    }
                }
            }
        }

        DevTools.Plot(Stopwatch.GetElapsedTime(startTime).Microseconds, new Plotable("mesh gen", 0, 1200));


        Span<float> vertices;
        Span<float> texcoords;
        Span<byte> colors;

        if (verticesList.Count == 0)
        {
            HasMesh = false;
            Mesh = default;
            Model = default;
            return;
        }

        mesh.VertexCount = verticesList.Count;
        mesh.TriangleCount = verticesList.Count / 3;

        mesh.Vertices = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 3, sizeof(float));
        vertices = new Span<float>(mesh.Vertices, verticesList.Count * 3);

        mesh.TexCoords = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 2, sizeof(float));
        texcoords = new Span<float>(mesh.TexCoords, verticesList.Count * 2);

        mesh.Colors = (byte*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 4, sizeof(byte));
        colors = new Span<byte>(mesh.Colors, verticesList.Count * 4);

        for (var i = 0; i < verticesList.Count; i++)
        {
            var vertex = verticesList[i];
            vertices[i * 3] = vertex.Pos.X;
            vertices[i * 3 + 1] = vertex.Pos.Y;
            vertices[i * 3 + 2] = vertex.Pos.Z;

            texcoords[i * 2] = vertex.TextCoord.X;
            texcoords[i * 2 + 1] = vertex.TextCoord.Y;

            colors[i * 4] = vertex.Color.R;
            colors[i * 4 + 1] = vertex.Color.G;
            colors[i * 4 + 2] = vertex.Color.B;
            colors[i * 4 + 3] = vertex.Color.A;
        }

        Mesh = mesh;
        Raylib.UploadMesh(ref Mesh, false);

        Model = Raylib.LoadModelFromMesh(Mesh);
        Model.Materials[0].Maps->Texture = _world.TextureAtlas;
    }

    private IntVector3 GetOffset(BlockFace blockFace)
    {
        return blockFace switch
        {
            BlockFace.Left => new IntVector3(-1, 0, 0),
            BlockFace.Right => new IntVector3(1, 0, 0),
            BlockFace.Bottom => new IntVector3(0, -1, 0),
            BlockFace.Top => new IntVector3(0, 1, 0),
            BlockFace.Back => new IntVector3(0, 0, 1),
            BlockFace.Front => new IntVector3(0, 0, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(blockFace), blockFace, null)
        };
    }

    private void AddBetterVertices(IntVector3 block, IntVector3 p1, IntVector3 p2, IntVector3 p3, IntVector3 p4,
        List<Vertex> vertices,
        ushort blockId, BlockFace blockFace)
    {
        var uvCoordinates = Textures.GetUvCoordinatesForFace(blockId, blockFace);

        AddVertices(block, p1, vertices, uvCoordinates.topLeft, blockFace, Corner2d.TopLeft);
        AddVertices(block, p2, vertices, uvCoordinates.bottomRight, blockFace, Corner2d.BottomRight);
        AddVertices(block, p3, vertices, uvCoordinates.topRight, blockFace, Corner2d.TopRight);

        AddVertices(block, p4, vertices, uvCoordinates.bottomLeft, blockFace, Corner2d.BottomLeft);
        AddVertices(block, p2, vertices, uvCoordinates.bottomRight, blockFace, Corner2d.BottomRight);
        AddVertices(block, p1, vertices, uvCoordinates.topLeft, blockFace, Corner2d.TopLeft);
    }

    enum Corner2d
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    private void AddVertices(IntVector3 blockPos, IntVector3 corner, List<Vertex> vertices, Vector2 texCoord,
        BlockFace blockFace, Corner2d corner2d)
    {
        //ToDo refactor ambient occlusion checks
        var blocksToCheck = blockFace switch
        {
            BlockFace.Left => corner2d switch
            {
                Corner2d.TopRight => (new IntVector3(-1, 0, +1), new IntVector3(-1, +1, 0), new IntVector3()),
                Corner2d.TopLeft => (new IntVector3(-1, 0, -1), new IntVector3(-1, +1, 0), new IntVector3()),
                Corner2d.BottomRight => (new IntVector3(-1, 0, +1), new IntVector3(-1, -1, 0), new IntVector3()),
                Corner2d.BottomLeft => (new IntVector3(-1, 0, -1), new IntVector3(-1, -1, 0), new IntVector3()),
                _ => throw new ArgumentOutOfRangeException(nameof(corner2d), corner2d, null)
            },
            BlockFace.Right => corner2d switch
            {
                Corner2d.TopRight => (new IntVector3(+1, 0, -1), new IntVector3(+1, +1, 0), new IntVector3()),
                Corner2d.TopLeft => (new IntVector3(+1, 0, +1), new IntVector3(+1, +1, 0), new IntVector3()),
                Corner2d.BottomRight => (new IntVector3(+1, 0, -1), new IntVector3(+1, -1, 0), new IntVector3()),
                Corner2d.BottomLeft => (new IntVector3(+1, 0, +1), new IntVector3(+1, -1, 0), new IntVector3()),
                _ => throw new ArgumentOutOfRangeException(nameof(corner2d), corner2d, null)
            },
            BlockFace.Bottom => corner2d switch
            {
                Corner2d.TopLeft => (new IntVector3(0, -1, 0), new IntVector3(0, -1, 0), new IntVector3()),
                Corner2d.TopRight => (new IntVector3(0, -1, 0), new IntVector3(0, -1, 0), new IntVector3()),
                Corner2d.BottomLeft => (new IntVector3(0, -1, 0), new IntVector3(0, -1, 0), new IntVector3()),
                Corner2d.BottomRight => (new IntVector3(0, -1, 0), new IntVector3(0, -1, 0), new IntVector3()),
                _ => throw new ArgumentOutOfRangeException(nameof(corner2d), corner2d, null)
            },
            BlockFace.Top => corner2d switch
            {
                Corner2d.TopLeft => (new IntVector3(0, +1, +1), new IntVector3(+1, +1, 0), new IntVector3(+1,1,+1)),
                Corner2d.TopRight => (new IntVector3(-1, +1, 0), new IntVector3(0, +1, +1), new IntVector3(-1, +1, +1)),
                Corner2d.BottomLeft => (new IntVector3(0, +1, -1), new IntVector3(+1, +1, 0), new IntVector3(+1, +1, -1)),
                Corner2d.BottomRight => (new IntVector3(-1, +1, 0), new IntVector3(0, +1, -1), new IntVector3(-1, +1, -1)),
                _ => throw new ArgumentOutOfRangeException(nameof(corner2d), corner2d, null)
            },
            BlockFace.Back => corner2d switch
            {
                Corner2d.TopLeft => (new IntVector3(-1, 0, +1), new IntVector3(0, +1, +1), new IntVector3()),
                Corner2d.TopRight => (new IntVector3(+1, 0, +1), new IntVector3(0, +1, +1), new IntVector3()),
                Corner2d.BottomLeft => (new IntVector3(-1, 0, +1), new IntVector3(0, -1, +1), new IntVector3()),
                Corner2d.BottomRight => (new IntVector3(+1, 0, +1), new IntVector3(0, -1, +1), new IntVector3()),
                _ => throw new ArgumentOutOfRangeException(nameof(corner2d), corner2d, null)
            },
            BlockFace.Front => corner2d switch
            {
                Corner2d.TopLeft => (new IntVector3(+1, 0, -1), new IntVector3(0, +1, -1), new IntVector3(+1, +1, -1)),
                Corner2d.TopRight => (new IntVector3(-1, 0, -1), new IntVector3(0, +1, -1), new IntVector3(-1, +1, -1)),
                Corner2d.BottomLeft => (new IntVector3(+1, 0, -1), new IntVector3(0, -1, -1), new IntVector3(+1, -1, -1)),
                Corner2d.BottomRight => (new IntVector3(-1, 0, -1), new IntVector3(0, -1, -1), new IntVector3(-1, -1, -1)),
                _ => throw new ArgumentOutOfRangeException(nameof(corner2d), corner2d, null)
            },
            _ => throw new ArgumentOutOfRangeException(nameof(blockFace), blockFace, null)
        };

        var occlusionCount = 0;

        var neighbourBlock1 = TryGetBlockAtPos(blockPos + blocksToCheck.Item1, out var wasFound1);
        var neighbourBlock2 = TryGetBlockAtPos(blockPos + blocksToCheck.Item2, out var wasFound2);
        var neighbourBlock3 = TryGetBlockAtPos(blockPos + blocksToCheck.Item3, out var wasFound3);

        if (wasFound1 && !neighbourBlock1.IsAir())
            occlusionCount++;

        if (wasFound2 && !neighbourBlock2.IsAir())
            occlusionCount++;

        if (occlusionCount == 0 
            && wasFound3 
            && !neighbourBlock3.IsAir() 
            && blocksToCheck.Item3 != new IntVector3(0, 0, 0))
            occlusionCount++;

        Color color;
        if (occlusionCount == 0)
        {
            color = new Color(255, 255, 255, 255);
        }
        else if (occlusionCount == 1)
        {
            color = new Color(200, 200, 200, 255);
        }
        else
        {
            color = new Color(150, 150, 150, 255);
        }

        vertices.Add(new Vertex((blockPos + corner).ToVector3NonCenter(), texCoord, color));
    }

    private Block TryGetBlockAtPos(IntVector3 blockInChunk, out bool wasFound)
    {
        if (blockInChunk.X is < 0 or > 15
            || blockInChunk.Y is < 0 or > 15
            || blockInChunk.Z is < 0 or > 15)
        {
            return _world.TryGetBlockAtPos(Pos * 16 + blockInChunk, out wasFound);
        }

        wasFound = true;

        return Blocks[GetIdx(blockInChunk.X, blockInChunk.Y, blockInChunk.Z)];
    }

    private void AddQuadFor(IntVector3 block, ushort blockId, BlockFace blockFace, List<Vertex> vertices)
    {
        var neighbourBlock = TryGetBlockAtPos(block + GetOffset(blockFace), out var wasFound);

        //is unloaded chunk
        if (!wasFound)
            return;

        //is solid block
        if (!neighbourBlock.IsAir())
            return;

        switch (blockFace)
        {
            case BlockFace.Left:
                AddBetterVertices(block, _topLeftFront, _bottomLeftBack, _topLeftBack, _bottomLeftFront, vertices,
                    blockId, blockFace);
                break;
            case BlockFace.Right:
                AddBetterVertices(block, _topRightBack, _bottomRightFront, _topRightFront, _bottomRightBack, vertices,
                    blockId, blockFace);
                break;
            case BlockFace.Bottom:
                AddBetterVertices(block, _bottomRightFront, _bottomLeftBack, _bottomLeftFront, _bottomRightBack,
                    vertices, blockId, blockFace);
                break;
            case BlockFace.Top:
                AddBetterVertices(block, _topRightBack, _topLeftFront, _topLeftBack, _topRightFront, vertices, blockId,
                    blockFace);
                break;
            case BlockFace.Back:
                AddBetterVertices(block, _topLeftBack, _bottomRightBack, _topRightBack, _bottomLeftBack, vertices,
                    blockId, blockFace);
                break;
            case BlockFace.Front:
                AddBetterVertices(block, _topRightFront, _bottomLeftFront, _topLeftFront, _bottomRightFront, vertices,
                    blockId, blockFace);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void Dispose()
    {
        Raylib.UnloadModel(Model);
    }

    public IntVector3 GetGlobalCoord(int x, int y, int z)
    {
        var globalX = x + Pos.X * 16;
        var globalY = y + Pos.Y * 16;
        var globalZ = z + Pos.Z * 16;

        return new IntVector3(globalX, globalY, globalZ);
    }
}

public record struct Vertex(Vector3 Pos, Vector2 TextCoord, Color Color);

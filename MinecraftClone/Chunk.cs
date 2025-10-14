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

    // public static readonly IntVector3 blockDev.BottomLeftFront() = new(0, 0, 0);
    // public static readonly IntVector3 blockDev.BottomRightFront() = new(1, 0, 0);
    // public static readonly IntVector3 blockDev.BottomLeftBack() = new(0, 0, 1);
    // public static readonly IntVector3 blockDev.BottomRightBack() = new(1, 0, 1);
    // public static readonly IntVector3 blockDev.TopLeftFront() = new(0, 1, 0);
    // public static readonly IntVector3 blockDev.TopRightFront() = new(1, 1, 0);
    // public static readonly IntVector3 blockDev.TopLeftBack() = new(0, 1, 1);
    // public static readonly IntVector3 blockDev.TopRightBack() = new(1, 1, 1);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetIdx(IntVector3 vec)
    {
        return vec.X + vec.Y * 16 + vec.Z * 16 * 16;
    }

    private static IntVector3 GetOffset(JsonBlockFaceDirection blockFace)
    {
        return blockFace switch
        {
            JsonBlockFaceDirection.West => new IntVector3(-1, 0, 0),
            JsonBlockFaceDirection.East => new IntVector3(1, 0, 0),
            JsonBlockFaceDirection.Down => new IntVector3(0, -1, 0),
            JsonBlockFaceDirection.Up => new IntVector3(0, 1, 0),
            JsonBlockFaceDirection.South => new IntVector3(0, 0, 1),
            JsonBlockFaceDirection.North => new IntVector3(0, 0, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(blockFace), blockFace, null)
        };
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

        //todo wow, ok this has very bad performance, can be sped up at lest 10x
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
                        JsonBlockFaceDirection surroundingBlocks = 0;
                        surroundingBlocks |= IsSolidBlock(pos + GetOffset(JsonBlockFaceDirection.North)) ? JsonBlockFaceDirection.North : 0;
                        surroundingBlocks |= IsSolidBlock(pos + GetOffset(JsonBlockFaceDirection.East)) ? JsonBlockFaceDirection.East : 0;
                        surroundingBlocks |= IsSolidBlock(pos + GetOffset(JsonBlockFaceDirection.South)) ? JsonBlockFaceDirection.South : 0;
                        surroundingBlocks |= IsSolidBlock(pos + GetOffset(JsonBlockFaceDirection.West)) ? JsonBlockFaceDirection.West : 0;
                        surroundingBlocks |= IsSolidBlock(pos + GetOffset(JsonBlockFaceDirection.Up)) ? JsonBlockFaceDirection.Up : 0;
                        surroundingBlocks |= IsSolidBlock(pos + GetOffset(JsonBlockFaceDirection.Down)) ? JsonBlockFaceDirection.Down : 0;

                        MeshGen.GenMeshForBlock(block, pos, surroundingBlocks, verticesList);
                    }
                }
            }
        }

        DevTools.Plot(Stopwatch.GetElapsedTime(startTime).Microseconds, new Plotable("mesh gen", 0, 1200));

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
        Span<float> vertices = new Span<float>(mesh.Vertices, verticesList.Count * 3);

        mesh.Normals = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 3, sizeof(float));
        Span<float> normals = new Span<float>(mesh.Normals, verticesList.Count * 3);

        mesh.TexCoords = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 2, sizeof(float));
        Span<float> texcoords = new Span<float>(mesh.TexCoords, verticesList.Count * 2);

        mesh.Colors = (byte*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 4, sizeof(byte));
        Span<byte> colors = new Span<byte>(mesh.Colors, verticesList.Count * 4);

        for (var i = 0; i < verticesList.Count; i++)
        {
            var vertex = verticesList[i];
            vertices[i * 3] = vertex.Pos.X;
            vertices[i * 3 + 1] = vertex.Pos.Y;
            vertices[i * 3 + 2] = vertex.Pos.Z;

            normals[i * 3] = vertex.Normal.X;
            normals[i * 3 + 1] = vertex.Normal.Y;
            normals[i * 3 + 2] = vertex.Normal.Z;

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
        Model.Materials[0].Shader = CurrentWorld.Game.ChunkShader;
    }

    private bool IsSolidBlock(IntVector3 blockInChunk)
    {
        if (blockInChunk.X is < 0 or > 15
            || blockInChunk.Y is < 0 or > 15
            || blockInChunk.Z is < 0 or > 15)
        {
            var block = _world.TryGetBlockAtPos(Pos * 16 + blockInChunk, out var wasFound);

            if (wasFound)
            {
                return !RayLib3dTest.Blocks.BlockList[block.BlockId].IsTransparent;
            }

            return false;
        }

        var b = Blocks[GetIdx(blockInChunk.X, blockInChunk.Y, blockInChunk.Z)];
        return !RayLib3dTest.Blocks.BlockList[b.BlockId].IsTransparent;
    }



    public void Dispose()
    {
        Raylib.UnloadModel(Model);
    }

    public IntVector3 GetLocalCoord(IntVector3 vec)
    {
        var globalX = vec.X - Pos.X * 16;
        var globalY = vec.Y - Pos.Y * 16;
        var globalZ = vec.Z - Pos.Z * 16;

        return new IntVector3(globalX, globalY, globalZ);
    }

    public IntVector3 GetLocalCoord(int x, int y, int z)
    {
        return GetLocalCoord(new IntVector3(x, y, z));
    }

    public IntVector3 GetGlobalCoord(int x, int y, int z)
    {
        var globalX = x + Pos.X * 16;
        var globalY = y + Pos.Y * 16;
        var globalZ = z + Pos.Z * 16;

        return new IntVector3(globalX, globalY, globalZ);
    }

    public bool ContainsGlobalCoord(IntVector3 pos)
    {
        var min = GetGlobalCoord(0, 0, 0);
        var max = GetGlobalCoord(15, 15, 15);
        if (pos.X >= min.X && pos.Y >= min.Y && pos.Z >= min.Z
            && pos.X <= max.X && pos.Y <= max.Y && pos.Z <= max.Z)
        {
            return true;
        }

        return false;
    }
}

public record struct Vertex(Vector3 Pos, Vector2 TextCoord, Color Color, Vector3 Normal);

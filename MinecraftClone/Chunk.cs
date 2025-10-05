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
                        var blockDefinition = RayLib3dTest.Blocks.BlockList[block.BlockId];
                        foreach (var element in blockDefinition.ParsedModel.Elements)
                        {
                            foreach (var (direction, face) in element.Faces)
                            {
                                var t = blockDefinition.Textures[face.Texture];
                                var uvs = Textures.GetUvCoordinatesForTexture(t, face.UvVector);

                                AddQuadFor(pos, uvs, direction, element.BlockDev, verticesList, face.CullfaceDirection != JsonBlockFaceDirection.None);
                            }
                        }
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

    private IntVector3 GetOffset(JsonBlockFaceDirection blockFace)
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

    private void AddBetterVertices(IntVector3 block, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4,
        List<Vertex> vertices,
        UvCoordinates uvCoordinates)
    {
        AddVertices(block, p1, vertices, uvCoordinates.topLeft);
        AddVertices(block, p2, vertices, uvCoordinates.bottomRight);
        AddVertices(block, p3, vertices, uvCoordinates.topRight);

        AddVertices(block, p4, vertices, uvCoordinates.bottomLeft);
        AddVertices(block, p2, vertices, uvCoordinates.bottomRight);
        AddVertices(block, p1, vertices, uvCoordinates.topLeft);
    }

    private void AddVertices(IntVector3 blockPos, Vector3 corner, List<Vertex> vertices, Vector2 texCoord)
    {
        vertices.Add(new Vertex(blockPos.ToVector3NonCenter() + corner, texCoord, new Color(255, 255, 255, 255)));
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

    private void AddQuadFor(IntVector3 block, UvCoordinates uvCoordinates, JsonBlockFaceDirection blockFace, BlockDev blockDev, List<Vertex> vertices, bool cullFace)
    {
        var neighbourBlock = TryGetBlockAtPos(block + GetOffset(blockFace), out var wasFound);

        //is unloaded chunk
        if (!wasFound)
            return;

        //is solid block
        if (cullFace && RayLib3dTest.Blocks.BlockList[neighbourBlock.BlockId].ParsedModel.IsFullBlock)
            return;

        switch (blockFace)
        {
            case JsonBlockFaceDirection.West:
                AddBetterVertices(block, blockDev.TopLeftFront(), blockDev.BottomLeftBack(), blockDev.TopLeftBack(), blockDev.BottomLeftFront(), vertices,
                    uvCoordinates);
                break;
            case JsonBlockFaceDirection.East:
                AddBetterVertices(block, blockDev.TopRightBack(), blockDev.BottomRightFront(), blockDev.TopRightFront(), blockDev.BottomRightBack(), vertices,
                    uvCoordinates);
                break;
            case JsonBlockFaceDirection.Down:
                AddBetterVertices(block, blockDev.BottomRightFront(), blockDev.BottomLeftBack(), blockDev.BottomLeftFront(), blockDev.BottomRightBack(),
                    vertices, uvCoordinates);
                break;
            case JsonBlockFaceDirection.Up:
                AddBetterVertices(block, blockDev.TopRightBack(), blockDev.TopLeftFront(), blockDev.TopLeftBack(), blockDev.TopRightFront(), vertices, uvCoordinates);
                break;
            case JsonBlockFaceDirection.South:
                AddBetterVertices(block, blockDev.TopLeftBack(), blockDev.BottomRightBack(), blockDev.TopRightBack(), blockDev.BottomLeftBack(), vertices,
                    uvCoordinates);
                break;
            case JsonBlockFaceDirection.North:
                AddBetterVertices(block, blockDev.TopRightFront(), blockDev.BottomLeftFront(), blockDev.TopLeftFront(), blockDev.BottomRightFront(), vertices,
                    uvCoordinates);
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

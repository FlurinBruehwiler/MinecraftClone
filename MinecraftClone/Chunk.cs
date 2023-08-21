using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace RayLib3dTest;

public class Chunk : IDisposable
{
    private readonly GlobalBoy _globalBoy;
    private readonly Textures _textures;
    public Mesh Mesh;
    public Model Model { get; set; }
    public Block[,,] Blocks { get; set; }
    public required IntVector3 Pos;
    public bool HasMesh;

    private readonly IntVector3 _bottomLeftFront = new(0, 0, 0);
    private readonly IntVector3 _bottomRightFront = new(1, 0, 0);
    private readonly IntVector3 _bottomLeftBack = new(0, 0, 1);
    private readonly IntVector3 _bottomRightBack = new(1, 0, 1);
    private readonly IntVector3 _topLeftFront = new(0, 1, 0);
    private readonly IntVector3 _topRightFront = new(1, 1, 0);
    private readonly IntVector3 _topLeftBack = new(0, 1, 1);
    private readonly IntVector3 _topRightBack = new(1, 1, 1);

    public Chunk(GlobalBoy globalBoy, Textures textures)
    {
        _globalBoy = globalBoy;
        _textures = textures;
        Blocks = new Block[16, 16, 16];
    }

    private unsafe void GenModel()
    {
        Model = LoadModelFromMesh(Mesh);
        Model.materials[0].maps->texture = _globalBoy.Texture2D;
        // Model.materials[0].shader = _globalBoy.Shader;
    }

    public void GenMesh()
    {
        UnloadMesh(ref Mesh);

        var mesh = new Mesh();

        var verticesList = new List<Vertex>();

        for (var x = 0; x < Blocks.GetLength(0); x++)
        {
            for (var y = 0; y < Blocks.GetLength(1); y++)
            {
                for (var z = 0; z < Blocks.GetLength(2); z++)
                {
                    var block = Blocks[x, y, z];

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

        Span<float> vertices;
        Span<float> texcoords;
        Span<byte> colors;

        mesh.vertexCount = verticesList.Count;
        mesh.triangleCount = verticesList.Count / 3;

        unsafe
        {
            mesh.vertices = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 3, sizeof(float));
            vertices = new Span<float>(mesh.vertices, verticesList.Count * 3);

            mesh.texcoords = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 2, sizeof(float));
            texcoords = new Span<float>(mesh.texcoords, verticesList.Count * 2);

            mesh.colors = (byte*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 4, sizeof(byte));
            colors = new Span<byte>(mesh.colors, verticesList.Count * 4);
        }

        for (var i = 0; i < verticesList.Count; i++)
        {
            var vertex = verticesList[i];
            vertices[i * 3] = vertex.Pos.X;
            vertices[i * 3 + 1] = vertex.Pos.Y;
            vertices[i * 3 + 2] = vertex.Pos.Z;

            texcoords[i * 2] = vertex.TextCoord.X;
            texcoords[i * 2 + 1] = vertex.TextCoord.Y;

            colors[i * 4] = vertex.Color.r;
            colors[i * 4 + 1] = vertex.Color.g;
            colors[i * 4 + 2] = vertex.Color.b;
            colors[i * 4 + 3] = vertex.Color.a;
        }

        Mesh = mesh;
        UploadMesh(ref Mesh, false);
        GenModel();
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
        var texture = _textures.GetTexturePosForFace(blockId, blockFace);
        var topLeft = new Vector2(0.1f * texture.X, 0.1f * texture.Y);
        var topRight = new Vector2(topLeft.X + 0.1f, topLeft.Y);
        var bottomLeft = new Vector2(topLeft.X, topLeft.Y + 0.1f);
        var bottomRight = new Vector2(topRight.X, bottomLeft.Y);

        AddVertices(block, p1, vertices, topLeft, blockFace, Corner2d.TopLeft);
        AddVertices(block, p2, vertices, bottomRight, blockFace, Corner2d.BottomRight);
        AddVertices(block, p3, vertices, topRight, blockFace, Corner2d.TopRight);

        AddVertices(block, p4, vertices, bottomLeft, blockFace, Corner2d.BottomLeft);
        AddVertices(block, p2, vertices, bottomRight, blockFace, Corner2d.BottomRight);
        AddVertices(block, p1, vertices, topLeft, blockFace, Corner2d.TopLeft);
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

        var neighbourBlock1 = _globalBoy.TryGetBlockAtPos(Pos * 16 + blockPos + blocksToCheck.Item1, out var wasFound1);
        var neighbourBlock2 = _globalBoy.TryGetBlockAtPos(Pos * 16 + blockPos + blocksToCheck.Item2, out var wasFound2);
        var neighbourBlock3 = _globalBoy.TryGetBlockAtPos(Pos * 16 + blockPos + blocksToCheck.Item3, out var wasFound3);

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

        vertices.Add(new Vertex((blockPos + corner).ToVector3(), texCoord, color));
    }

    private void AddQuadFor(IntVector3 block, ushort blockId, BlockFace blockFace, List<Vertex> vertices)
    {
        var neighbourBlock = _globalBoy.TryGetBlockAtPos(Pos * 16 + block + GetOffset(blockFace), out var wasFound);

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
        UnloadModel(Model);
    }
}

public record struct Vertex(Vector3 Pos, Vector2 TextCoord, Color Color);
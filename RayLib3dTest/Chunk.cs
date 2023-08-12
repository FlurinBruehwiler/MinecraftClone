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
    
    public unsafe void GenModel()
    {
        UnloadModel(Model);
        
        Model = LoadModelFromMesh(Mesh);
        Model.materials[0].maps->texture = _globalBoy.Texture2D;
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
                        if (Pos * 16 + pos is { X: 63, Y: 9, Z: 51 })
                        {
            
                        }
                        
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

        mesh.vertexCount = verticesList.Count;
        mesh.triangleCount = verticesList.Count * 3;
        
        unsafe
        {
            mesh.vertices = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 3, sizeof(float));
            vertices = new Span<float>(mesh.vertices, verticesList.Count * 3);
            
            mesh.texcoords = (float*)NativeMemory.AllocZeroed((UIntPtr)verticesList.Count * 2, sizeof(float));
            texcoords = new Span<float>(mesh.texcoords, verticesList.Count * 2);
        }
        
        for (var i = 0; i < verticesList.Count; i++)
        {
            vertices[i * 3] = verticesList[i].Pos.X;
            vertices[i * 3 + 1] = verticesList[i].Pos.Y;
            vertices[i * 3 + 2] = verticesList[i].Pos.Z;

            texcoords[i * 2] = verticesList[i].TextCoord.X;
            texcoords[i * 2 + 1] = verticesList[i].TextCoord.Y;
        }

        Mesh = mesh;
        UploadMesh(ref Mesh, false);
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

    private void AddBetterVertices(IntVector3 block, IntVector3 p1, IntVector3 p2, IntVector3 p3, IntVector3 p4, List<Vertex> vertices,
        int blockId, BlockFace blockFace)
    {
        var texture = _textures.GetTexturePosForFace(blockId, blockFace);
        var topLeft = new Vector2(0.1f * texture.X, 0.1f * texture.Y);
        var topRight = new Vector2(topLeft.X + 0.1f, topLeft.Y);
        var bottomLeft = new Vector2(topLeft.X, topLeft.Y + 0.1f);
        var bottomRight = new Vector2(topRight.X, bottomLeft.Y);
        
        AddVertices(block + p1, vertices, topLeft);
        AddVertices(block + p2, vertices, bottomRight);
        AddVertices(block + p3, vertices, topRight);

        AddVertices(block + p4, vertices, bottomLeft);
        AddVertices(block + p2, vertices, bottomRight);
        AddVertices(block + p1, vertices, topLeft);
    }

    private void AddQuadFor(IntVector3 block, int blockId, BlockFace blockFace, List<Vertex> vertices)
    {
        var neighbourBlock = _globalBoy.TryGetBlockAtPos(Pos * 16 + block + GetOffset(blockFace), out var wasFound);

        //is unloaded chunk
        if (!wasFound)
            return;
        
        //is solid block
        if(!neighbourBlock.IsAir())
            return;
        
        switch (blockFace)
        {
            case BlockFace.Left:
                AddBetterVertices(block, _topLeftFront, _bottomLeftBack, _topLeftBack, _bottomLeftFront, vertices, blockId, blockFace);
                break;
            case BlockFace.Right:
                AddBetterVertices(block, _topRightBack, _bottomRightFront, _topRightFront, _bottomRightBack, vertices, blockId, blockFace);
                break;
            case BlockFace.Bottom:
                AddBetterVertices(block, _bottomRightFront, _bottomLeftBack, _bottomLeftFront, _bottomRightBack, vertices, blockId, blockFace);
                break;
            case BlockFace.Top:
                AddBetterVertices(block, _topRightBack, _topLeftFront, _topLeftBack, _topRightFront, vertices, blockId, blockFace);
                break;
            case BlockFace.Back:
                AddBetterVertices(block, _topLeftBack, _bottomRightBack, _topRightBack, _bottomLeftBack, vertices, blockId, blockFace);
                break;
            case BlockFace.Front:
                AddBetterVertices(block, _topRightFront, _bottomLeftFront, _topLeftFront, _bottomRightFront, vertices, blockId, blockFace);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void AddVertices(IntVector3 pos, List<Vertex> vertices, Vector2 texCoord)
    {
        vertices.Add(new Vertex(new Vector3(pos.X, pos.Y, pos.Z), texCoord));
    }

    public void Dispose()
    {
        UnloadModel(Model);
    }
}

public record struct Vertex(Vector3 Pos, Vector2 TextCoord);
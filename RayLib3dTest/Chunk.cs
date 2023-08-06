using System.Numerics;
using System.Runtime.InteropServices;
using Raylib_cs;

namespace RayLib3dTest;

internal class Chunk : IDisposable
{
    public Mesh Mesh;
    public Model Model { get; set; }
    public Block[,,] Blocks { get; set; }
    public required Vector2 Pos { get; init; }

    private readonly IntVector3 _bottomLeftFront = new(0, 0, 0);
    private readonly IntVector3 _bottomRightFront = new(1, 0, 0);
    private readonly IntVector3 _bottomLeftBack = new(0, 0, 1);
    private readonly IntVector3 _bottomRightBack = new(1, 0, 1);
    private readonly IntVector3 _topLeftFront = new(0, 1, 0);
    private readonly IntVector3 _topRightFront = new(1, 1, 0);
    private readonly IntVector3 _topLeftBack = new(0, 1, 1);
    private readonly IntVector3 _topRightBack = new(1, 1, 1);

    private Texture2D _grasTexture;
    
    public Chunk()
    {
        Blocks = new Block[16, 16, 16];
        _grasTexture = LoadTexture("resources/grass_block_side.png");
    }

    public unsafe void GenModel()
    {
        Model = LoadModelFromMesh(Mesh);
        Model.materials[0].maps->texture = _grasTexture;
    }

    public void GenMesh()
    {
        var mesh = new Mesh();
        var verticesList = new List<Vertex>();

        for (var x = 0; x < Blocks.GetLength(0); x++)
        {
            for (var y = 0; y < Blocks.GetLength(1); y++)
            {
                for (var z = 0; z < Blocks.GetLength(2); z++)
                {
                    var block = Blocks[x, y, z];
                    if (!block.IsAir)
                    {
                        var pos = new IntVector3(x, y, z);
                        AddQuadFor(pos, Neighbour.Left, verticesList);
                        AddQuadFor(pos, Neighbour.Right, verticesList);
                        AddQuadFor(pos, Neighbour.Top, verticesList);
                        AddQuadFor(pos, Neighbour.Bottom, verticesList);
                        AddQuadFor(pos, Neighbour.Back, verticesList);
                        AddQuadFor(pos, Neighbour.Front, verticesList);
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

    private IntVector3 GetOffset(Neighbour neighbour)
    {
        return neighbour switch
        {
            Neighbour.Left => new IntVector3(-1, 0, 0),
            Neighbour.Right => new IntVector3(1, 0, 0),
            Neighbour.Bottom => new IntVector3(0, -1, 0),
            Neighbour.Top => new IntVector3(0, 1, 0),
            Neighbour.Back => new IntVector3(0, 0, 1),
            Neighbour.Front => new IntVector3(0, 0, -1),
            _ => throw new ArgumentOutOfRangeException(nameof(neighbour), neighbour, null)
        };
    }

    private void AddQuadFor(IntVector3 block, Neighbour neighbour, List<Vertex> vertices)
    {
        var neighbourBlock = GetBlockAtPos(block + GetOffset(neighbour));

        if (neighbourBlock is not null && !neighbourBlock.Value.IsAir) return;

        switch (neighbour)
        {
            case Neighbour.Left:
                AddVertices(block + _topLeftBack, vertices, new Vector2(0, 1));
                AddVertices(block + _topLeftFront, vertices, new Vector2(1, 1));
                AddVertices(block + _bottomLeftBack, vertices, new Vector2(0, 0));

                AddVertices(block + _topLeftFront, vertices, new Vector2(1, 1));
                AddVertices(block + _bottomLeftFront, vertices, new Vector2(1, 0));
                AddVertices(block + _bottomLeftBack, vertices, new Vector2(0, 0));
                break;
            case Neighbour.Right:
                AddVertices(block + _topRightBack, vertices, new Vector2(1, 1));
                AddVertices(block + _bottomRightBack, vertices, new Vector2(1, 0));
                AddVertices(block + _topRightFront, vertices, new Vector2(0, 1));

                AddVertices(block + _bottomRightFront, vertices, new Vector2(0, 0));
                AddVertices(block + _topRightFront, vertices, new Vector2(1, 1));
                AddVertices(block + _bottomRightBack, vertices, new Vector2(1, 0));
                break;
            case Neighbour.Bottom:
                AddVertices(block + _bottomRightFront, vertices, new Vector2(1, 1));
                AddVertices(block + _bottomLeftBack, vertices, new Vector2(0, 0));
                AddVertices(block + _bottomLeftFront, vertices, new Vector2(0, 1));

                AddVertices(block + _bottomRightBack, vertices, new Vector2(1, 0));
                AddVertices(block + _bottomLeftBack, vertices, new Vector2(0, 0));
                AddVertices(block + _bottomRightFront, vertices, new Vector2(1, 1));
                break;
            case Neighbour.Top:
                AddVertices(block + _topRightBack, vertices, new Vector2(1, 1));
                AddVertices(block + _topLeftFront, vertices, new Vector2(0, 0));
                AddVertices(block + _topLeftBack, vertices, new Vector2(0, 1));
                
                AddVertices(block + _topRightFront, vertices, new Vector2(1, 0));
                AddVertices(block + _topLeftFront, vertices, new Vector2(0, 0));
                AddVertices(block + _topRightBack, vertices, new Vector2(1, 1));
                break;
            case Neighbour.Back:
                AddVertices(block + _topLeftBack, vertices, new Vector2(1, 1));
                AddVertices(block + _bottomRightBack, vertices, new Vector2(0, 0));
                AddVertices(block + _topRightBack, vertices, new Vector2(0, 1));
                
                AddVertices(block + _bottomLeftBack, vertices, new Vector2(1, 0));
                AddVertices(block + _bottomRightBack, vertices, new Vector2(0, 0));
                AddVertices(block + _topLeftBack, vertices, new Vector2(1, 1));
                break;
            case Neighbour.Front:
                AddVertices(block + _topRightFront, vertices, new Vector2(1, 1));
                AddVertices(block + _bottomLeftFront, vertices, new Vector2(0, 0));
                AddVertices(block + _topLeftFront, vertices, new Vector2(0, 1));
                
                AddVertices(block + _bottomRightFront, vertices, new Vector2(1, 0));
                AddVertices(block + _bottomLeftFront, vertices, new Vector2(0, 0));
                AddVertices(block + _topRightFront, vertices, new Vector2(1, 1));
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private Block? GetBlockAtPos(IntVector3 intVector3)
    {
        if (intVector3.X is > 15 or < 0)
            return null;

        if (intVector3.Y is > 15 or < 0)
            return null;

        if (intVector3.Z is > 15 or < 0)
            return null;

        return Blocks[intVector3.X, intVector3.Y, intVector3.Z];
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
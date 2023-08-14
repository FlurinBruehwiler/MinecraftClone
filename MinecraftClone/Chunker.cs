using System.Numerics;

namespace RayLib3dTest;

public class Chunker
{
    private readonly GlobalBoy _globalBoy;
    private readonly Textures _textures;
    private readonly MrPerlin _mrPerlin;

    public Chunker(GlobalBoy globalBoy, Textures textures, MrPerlin mrPerlin)
    {
        _globalBoy = globalBoy;
        _textures = textures;
        _mrPerlin = mrPerlin;
    }
    
    public void LoadChunksIfNeccesary(Vector3 playerPos)
    {
        const int renderDistance = 8;
        var chunkPos = GlobalBoy.GetChunkPos(playerPos);

        var addedChunks = new List<Chunk>();
        
        for (var x = -renderDistance; x < renderDistance; x++)
        {
            for (var z = -renderDistance; z < renderDistance; z++)
            {
                var neededChunk = new IntVector3(chunkPos.X + x, 0, chunkPos.Z + z);
                if (!_globalBoy.Chunks.ContainsKey(neededChunk))
                {
                    var newChunk = GetChunk(neededChunk);
                    addedChunks.Add(newChunk);
                    _globalBoy.Chunks.Add(neededChunk, newChunk);
                }
            }
        }
        
        foreach (var addedChunk in addedChunks)
        {
            addedChunk.GenMesh();
            addedChunk.GenModel();
        }
    }

    private Chunk GetChunk(IntVector3 pos)
    {
        var chunk = new Chunk(_globalBoy, _textures)
        {
            Pos = pos
        };
        
        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                var height = (int)Math.Clamp(_mrPerlin.OctavePerlin((x + chunk.Pos.X * 16) / 10.0f, 0,
                    (z + chunk.Pos.Z * 16) / 10.0f, 1, 2) * 16, 0, 15);
                for (var y = 0; y < 16; y++)
                {
                    if (y > height)
                    {
                        chunk.Blocks[x, y, z].BlockId = Blocks.Air.ID;
                    }
                    else if (y == height)
                    {
                        chunk.Blocks[x, y, z].BlockId = Blocks.Gras.ID;
                    }
                    else
                    {
                        chunk.Blocks[x, y, z].BlockId = Blocks.Dirt.ID;
                    }
                }
            }
        }

        return chunk;
    }
    
}
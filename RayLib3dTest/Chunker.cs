using System.Numerics;

namespace RayLib3dTest;

public class Chunker
{
    private readonly GlobalBoy _globalBoy;
    private readonly Textures _textures;

    public Chunker(GlobalBoy globalBoy, Textures textures)
    {
        _globalBoy = globalBoy;
        _textures = textures;
    }
    
    public void LoadChunksIfNeccesary(Vector3 playerPos)
    {
        const int renderDistance = 1;
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
        var data = MrPerlin.GenerateNoiseMap(pos.X * 16, pos.Z * 16, 16, 16, 1, 2, 2);

        var chunk = new Chunk(_globalBoy, _textures)
        {
            Pos = pos
        };
        
        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                var height = (int)(Math.Clamp(data[x * 16 + z], 0, 1) * 16);
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
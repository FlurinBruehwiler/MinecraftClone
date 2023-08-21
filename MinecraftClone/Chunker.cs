namespace RayLib3dTest;

public class Chunker : I3DDrawable
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
    
    public void Draw3d()
    {
        foreach (var (_, chunk) in _globalBoy.Chunks)
        {
            DrawModel(chunk.Model, new Vector3(chunk.Pos.X * 16, chunk.Pos.Y, chunk.Pos.Z * 16), 1, Color.WHITE);
        }
    }
    
    public void LoadChunksIfNeccesary(Vector3 playerPos)
    {
        const int renderDistance = 8;
        var chunkPos = GlobalBoy.GetChunkPos(playerPos);

        for (var x = -renderDistance; x < renderDistance; x++)
        {
            for (var z = -renderDistance; z < renderDistance; z++)
            {
                var neededChunk = new IntVector3(chunkPos.X + x, 0, chunkPos.Z + z);
                if (!_globalBoy.Chunks.TryGetValue(neededChunk, out var chunk) || !chunk.HasMesh)
                {
                    if (chunk is null)
                    {
                        chunk = GenChunk(neededChunk);
                        _globalBoy.Chunks.Add(neededChunk, chunk);
                    }
                    
                    if (_globalBoy.Chunks.ContainsKey(neededChunk with { Z = neededChunk.Z + 1 })
                        && _globalBoy.Chunks.ContainsKey(neededChunk with { Z = neededChunk.Z - 1 })
                        && _globalBoy.Chunks.ContainsKey(neededChunk with { X = neededChunk.X + 1 })
                        && _globalBoy.Chunks.ContainsKey(neededChunk with { X = neededChunk.X - 1 }))
                    {
                        chunk.GenMesh();
                        chunk.HasMesh = true;
                        return;
                    }
                }
            }
        }
        
    }

    private Chunk GenChunk(IntVector3 pos)
    {
        var chunk = new Chunk(_globalBoy, _textures)
        {
            Pos = pos
        };

        const float scale = .05f;
        
        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                var globalX = x + chunk.Pos.X * 16;
                var globalZ = z + chunk.Pos.Z * 16;
                
                var res = _mrPerlin.OctavePerlin(
                    (globalX + 100_000) * scale,
                    0,
                    (globalZ + 100_000) * scale, 1, 2);
                
                var height = (int)(res * 16);
                
                for (var y = 0; y < 16; y++)
                {
                    if (y > height)
                    {
                        chunk.Blocks[x, y, z].BlockId = Blocks.Air.Id;
                    }
                    else if (y == height)
                    {
                        chunk.Blocks[x, y, z].BlockId = Blocks.Gras.Id;
                    }
                    else
                    {
                        chunk.Blocks[x, y, z].BlockId = Blocks.Dirt.Id;
                    }
                }
            }
        }

        return chunk;
    }
}
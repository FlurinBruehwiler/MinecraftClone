using System.Diagnostics;

namespace RayLib3dTest;

public class Chunker : I3DDrawable
{
    private readonly GlobalBoy _globalBoy;
    private readonly Textures _textures;
    private readonly MrPerlin _mrPerlin;
    private readonly Debuggerus _debuggerus;

    public Chunker(GlobalBoy globalBoy, Textures textures, MrPerlin mrPerlin, Debuggerus debuggerus)
    {
        _globalBoy = globalBoy;
        _textures = textures;
        _mrPerlin = mrPerlin;
        _debuggerus = debuggerus;
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
                        var startTime = Stopwatch.GetTimestamp();

                        chunk = GenChunk(neededChunk);
                        
                        _debuggerus.Plot(Stopwatch.GetElapsedTime(startTime).Microseconds, new Plotable(nameof(GenChunk), 100, 220));
                        _globalBoy.Chunks.Add(neededChunk, chunk);
                    }
                    
                    if (_globalBoy.Chunks.ContainsKey(neededChunk with { Z = neededChunk.Z + 1 })
                        && _globalBoy.Chunks.ContainsKey(neededChunk with { Z = neededChunk.Z - 1 })
                        && _globalBoy.Chunks.ContainsKey(neededChunk with { X = neededChunk.X + 1 })
                        && _globalBoy.Chunks.ContainsKey(neededChunk with { X = neededChunk.X - 1 }))
                    {
                        var startTime = Stopwatch.GetTimestamp();
                        
                        chunk.GenMesh();

                        var res = Stopwatch.GetElapsedTime(startTime).Microseconds;
                        _debuggerus.Plot(res, new Plotable(nameof(chunk.GenMesh), 0, 1200));
                        _debuggerus.Print(res, nameof(chunk.GenMesh));
                        
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
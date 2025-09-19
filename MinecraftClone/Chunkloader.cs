using System.Diagnostics;

namespace RayLib3dTest;

public static class Chunkloader
{
    public static void LoadChunksIfNeccesary(Vector3 playerPos)
    {
        var chunkGenTimer = Stopwatch.GetTimestamp();
        const int renderDistance = 8;
        var chunkPos = GetChunkPos(playerPos);

        for (var x = -renderDistance; x < renderDistance; x++)
        {
            for (var z = -renderDistance; z < renderDistance; z++)
            {
                for (var y = -1; y < 5; y++)
                {
                    var neededChunk = new IntVector3(chunkPos.X + x, y, chunkPos.Z + z);
                    if (!CurrentWorld.Chunks.TryGetValue(neededChunk, out var chunk) || !chunk.HasMesh)
                    {
                        if (chunk is null)
                        {
                            var startTime = Stopwatch.GetTimestamp();

                            chunk = GenChunk(neededChunk);

                            DevTools.Plot(Stopwatch.GetElapsedTime(startTime).Microseconds, new Plotable(nameof(GenChunk), 100, 220));
                            CurrentWorld.Chunks.Add(neededChunk, chunk);
                        }

                        if (CurrentWorld.Chunks.ContainsKey(neededChunk with { Z = neededChunk.Z + 1 })
                            && CurrentWorld.Chunks.ContainsKey(neededChunk with { Z = neededChunk.Z - 1 })
                            && CurrentWorld.Chunks.ContainsKey(neededChunk with { X = neededChunk.X + 1 })
                            && CurrentWorld.Chunks.ContainsKey(neededChunk with { X = neededChunk.X - 1 })
                             && CurrentWorld.Chunks.ContainsKey(neededChunk with { Y = neededChunk.Y + 1 })
                             && CurrentWorld.Chunks.ContainsKey(neededChunk with { Y = neededChunk.Y - 1 }))

                        {
                            chunk.GenMesh();

                            chunk.HasMesh = true;

                            if (Stopwatch.GetElapsedTime(chunkGenTimer).TotalSeconds > Math.Max(GetFrameTime(), 1f / 120))
                                return;
                        }
                    }
                }
            }
        }
        
    }

    private static Chunk GenChunk(IntVector3 pos)
    {
        var chunk = new Chunk(CurrentWorld)
        {
            Pos = pos
        };

        const float scale = .025f;
        
        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                var globalX = x + chunk.Pos.X * 16;
                var globalZ = z + chunk.Pos.Z * 16;

                var res = Perlin.OctavePerlin(
                    (globalX + 100_000) * scale,
                    0,
                    (globalZ + 100_000) * scale, 1, 2);

                var height = (int)(res * 16 * 5);

                for (var y = 0; y < 16; y++)
                {
                    if (chunk.Pos.Y * 16 + y > height)
                    {
                        chunk.Blocks[x, y, z].BlockId = Blocks.Air.Id;
                    }
                    else if (chunk.Pos.Y * 16 + y == height)
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

        // var posX = Random.Shared.Next(0, 15);
        // var posZ = Random.Shared.Next(0, 15);
        //
        // for (int i = 0; i < 16; i++)
        // {
        //     if (chunk.Blocks[posX, i, posZ].IsAir())
        //     {
        //
        //     }
        // }

        return chunk;
    }
}

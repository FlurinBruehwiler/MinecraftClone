using System.Diagnostics;
using System.Security.Cryptography;

namespace RayLib3dTest;

public static class Chunkloader
{
    public static void LoadChunksIfNeccesary(Vector3 playerPos)
    {
        var chunkGenTimer = Stopwatch.GetTimestamp();
        const int renderDistance = 8;
        var chunkPos = GetChunkPos(playerPos);

        for (int i = 1; i < renderDistance; i++) //we do this, so chunks load from the inside to the outside, it is a bit wastefull, but really simple
        {
            for (var x = -i; x < i; x++)
            {
                for (var z = -i; z < i; z++)
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

                                if (Stopwatch.GetElapsedTime(chunkGenTimer).TotalMilliseconds > 0.8f )
                                    return;
                            }
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

        for (var x = 0; x < 16; x++)
        {
            for (var z = 0; z < 16; z++)
            {
                var g = chunk.GetGlobalCoord(x, 0, z);

                var height = GetTerrainHeightAt(g.X, g.Z);

                for (var y = 0; y < 16; y++)
                {
                    var idx = Chunk.GetIdx(x, y, z);

                    if (chunk.Pos.Y * 16 + y > height)
                    {
                        chunk.Blocks[idx].BlockId = Blocks.Air.Id;
                    }
                    else if (chunk.Pos.Y * 16 + y == height)
                    {
                        chunk.Blocks[idx].BlockId = Blocks.Gras.Id;
                    }
                    else
                    {
                        chunk.Blocks[idx].BlockId = Blocks.Dirt.Id;
                    }
                }

            }
        }

        //generate trees
        var posX = GetRandomInt(pos, 606186665, 0, 15);
        var posZ = GetRandomInt(pos, 1602518902, 0, 15);
        var trunkHeight = GetRandomInt(pos, 494945145, 5, 9);
        var global = chunk.GetGlobalCoord(posX, 0, posZ);
        var terrainHeightUnderTree = GetTerrainHeightAt(global.X, global.Z);

        //ok, the above defines the tree trunk, now we need to actually fill in the blocks of the trunk, that belong to _this_ chunk.
        //my idea is to have a BlockRange (Width, Length, Height) of both the trunk and the chunk,
        //and then get the intersection of the two, then i can just fill the resulting BlockRange with one specific block.
        //In the future,


        bool isTrunk = false;
        for (int i = 1; i < 16; i++)
        {
            var idx = Chunk.GetIdx(posX, i, posZ);
            if (chunk.Blocks[idx].IsAir() && !chunk.Blocks[Chunk.GetIdx(posX, i - 1 , posZ)].IsAir())
            {
                isTrunk = true;
            }

            if (isTrunk)
            {
                chunk.Blocks[idx].BlockId = Blocks.OakLog.Id;
            }
        }

        return chunk;
    }

    public static int GetTerrainHeightAt(int globalX, int globalZ)
    {
        const float scale = .025f;

        var res = Perlin.OctavePerlin(
            (globalX + 100_000) * scale,
            0,
            (globalZ + 100_000) * scale, 1, 2);

        var height = (int)(res * 16 * 5);

        return height;
    }

    public static int GetRandomInt(IntVector3 hashPos, int seed, int min, int max)
    {
        if (min > max)
            throw new Exception();

        var hash = HashCode.Combine(hashPos.X, hashPos.Y, hashPos.Z, seed);

        var diff = max - min;

        var o = Math.Abs(NextInt(hash));
        var res = min + (o % diff);

        if (res < min || res > max)
        {
            throw new Exception();
        }

        return res;
    }

    public static int NextInt(int seed)
    {
        Span<byte> bytes = stackalloc byte[4];
        BitConverter.TryWriteBytes(bytes, seed);

        var hash = SHA256.HashData(bytes);
        return BitConverter.ToInt32(hash, 0);
    }
}

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
                                chunk = World.LoadFromDirectory(neededChunk);

                                if (chunk == null) //generate if not already generated
                                {
                                    var startTime = Stopwatch.GetTimestamp();

                                    chunk = GenChunk(neededChunk);

                                    DevTools.Plot(Stopwatch.GetElapsedTime(startTime).Microseconds, new Plotable(nameof(GenChunk), 100, 220));
                                }

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
                g.Y = 0;

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
                        chunk.Blocks[idx].BlockId = Blocks.Grass.Id;
                    }
                    else
                    {
                        chunk.Blocks[idx].BlockId = Blocks.Dirt.Id;
                    }
                }

                var grassRand = GetRandomInt(g, 742389, 0, 20);
                if (grassRand >= 19)
                {
                    SetBlockIfWithinChunk(chunk, g with { Y = height + 1 }, Blocks.TallGrassBottom.Id);
                    SetBlockIfWithinChunk(chunk, g with { Y = height + 2 }, Blocks.TallGrassTop.Id);
                }
                else if (grassRand > 10)
                {
                    SetBlockIfWithinChunk(chunk, g with { Y = height + 1 }, Blocks.ShortGrass.Id);
                }
            }
        }


        //generate trees
        var posX = GetRandomInt(pos with { Y = 0}, 606186665, 0, 15);
        var posZ = GetRandomInt(pos with { Y = 0}, 1602518902, 0, 15);
        var trunkHeight = GetRandomInt(pos, 494945145, 5, 9);
        var global = chunk.GetGlobalCoord(posX, 0, posZ);

        var terrainHeightUnderTree = GetTerrainHeightAt(global.X, global.Z);

        var trunkRegion = new Region { Location = new IntVector3(global.X, terrainHeightUnderTree + 1, global.Z), Dimensions = new IntVector3(1, trunkHeight, 1)};
        FillRegionInChunk(chunk, trunkRegion, Blocks.OakLog);

        uijz 

        return chunk;
    }

    private static void FillRegionInChunk(Chunk chunk, Region region, BlockDefinition blockDefinition)
    {
        var bottomLeft = chunk.GetGlobalCoord(0, 0, 0);
        var chunkRegion = new Region { Location = bottomLeft, Dimensions = new IntVector3(16, 16,  16) };

        var resultingRegion = region.Intersect(chunkRegion);
        for (int x = resultingRegion.Location.X; x < resultingRegion.Location.X + resultingRegion.Dimensions.X; x++)
        {
            for (int y = resultingRegion.Location.Y; y < resultingRegion.Location.Y + resultingRegion.Dimensions.Y; y++)
            {
                for (int z = resultingRegion.Location.Z; z < resultingRegion.Location.Z + resultingRegion.Dimensions.Z; z++)
                {
                    if (x == -9 && y == 61 && z == 1)
                    {

                    }

                    chunk.Blocks[Chunk.GetIdx(chunk.GetLocalCoord(x, y, z))] = new Block
                    {
                        BlockId = blockDefinition.Id
                    };
                }
            }
        }
    }

    public static void SetBlockIfWithinChunk(Chunk chunk, IntVector3 pos, ushort blockId)
    {
        if (chunk.ContainsGlobalCoord(pos))
        {
            chunk.Blocks[Chunk.GetIdx(chunk.GetLocalCoord(pos))].BlockId = blockId;
        }
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

    public static int Combine(int a, int b, int c, int d)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + a;
            hash = hash * 31 + b;
            hash = hash * 31 + c;
            hash = hash * 31 + d;
            return hash;
        }
    }

    public static int GetRandomInt(IntVector3 hashPos, int seed, int min, int max)
    {
        if (min > max)
            throw new Exception();

        var hash = Combine(hashPos.X, hashPos.Y, hashPos.Z, seed);

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

public struct Region
{
    public IntVector3 Location;
    public IntVector3 Dimensions;

    public Region Intersect(Region region)
    {
        var x = Intersect(region.Location.X, region.Dimensions.X, Location.X, Dimensions.X);
        var y = Intersect(region.Location.Y, region.Dimensions.Y, Location.Y, Dimensions.Y);
        var z = Intersect(region.Location.Z, region.Dimensions.Z, Location.Z, Dimensions.Z);

        if (x.length <= 0 || y.length <= 0 || z.length <= 0)
            return new Region();

        return new Region
        {
            Location = new IntVector3(x.from, y.from, z.from),
            Dimensions = new IntVector3(x.length, y.length, z.length)
        };
    }

    private (int from, int length) Intersect(int aFrom, int aLength, int bFrom, int bLength)
    {
        var aTo = aFrom + aLength;
        var bTo = bFrom + bLength;

        if (aFrom >= bTo || bFrom >= aTo)
            return (0, 0);

        int from = Math.Max(aFrom, bFrom);
        int to = Math.Min(aTo, bTo);
        int length = to - from;

        return (from, length);
    }
}

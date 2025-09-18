namespace RayLib3dTest;

public class World
{
    public static World CurrentWorld;

    public Dictionary<IntVector3, Chunk> Chunks = new();
    public Texture2D Texture2D;

    private Block _emptyBlock;

    public World()
    {
        TextureAtlas.Create();

        Texture2D = LoadTexture("Resources/textureatlas.png");
    }

    public ref Block TryGetBlockAtPos(Vector3 pos, out bool wasFound)
    {
        return ref TryGetBlockAtPos(new IntVector3((int)pos.X, (int)pos.Y, (int)pos.Z), out wasFound);
    }

    public static IntVector3 GetChunkPos(Vector3 pos)
    {
        return new IntVector3(GetChunk(pos.X), GetChunk(pos.Y), GetChunk(pos.Z));
    }

    public ref Block TryGetBlockAtPos(IntVector3 pos, out bool wasFound)
    {
        wasFound = true;

        var chunkPosX = GetChunk(pos.X);
        var chunkPosY = GetChunk(pos.Y);
        var chunkPosZ = GetChunk(pos.Z);
        var blockInChunk = WorldToChunkSpace(pos);

        if (!Chunks.ContainsKey(new IntVector3(chunkPosX, chunkPosY, chunkPosZ)))
        {
            wasFound = false;
            return ref _emptyBlock;
        }

        if (blockInChunk.X is > 15 or < 0 || blockInChunk.Y is > 15 or < 0 || blockInChunk.Z is > 15 or < 0)
        {
            wasFound = false;
            return ref _emptyBlock;
        }

        return ref Chunks[new IntVector3(chunkPosX, chunkPosY, chunkPosZ)].Blocks[blockInChunk.X, blockInChunk.Y, blockInChunk.Z];
    }

    private Chunk? TryGetChunk(IntVector3 chunkCoordinate)
    {
        if (Chunks.TryGetValue(chunkCoordinate, out var value))
        {
            return value;
        }

        return null;
    }

    public void InformBlockUpdate(IntVector3 blockPos)
    {
        var chunk = GetChunk(blockPos);
        chunk?.GenMesh();

        var chunkCoord = GetChunkCoordinate(blockPos);

        var localSpace = WorldToChunkSpace(blockPos);

        if (localSpace.X == 0)
        {
            var l = chunkCoord;
            l.X--;
            TryGetChunk(l)?.GenMesh();
        }
        else if (localSpace.X == 15)
        {
            var l = chunkCoord;
            l.X++;
            TryGetChunk(l)?.GenMesh();
        }
        
        if (localSpace.Y == 0)
        {
            var l = chunkCoord;
            l.Y--;
            TryGetChunk(l)?.GenMesh();
        }
        else if (localSpace.Y == 15)
        {
            var l = chunkCoord;
            l.Y++;
            TryGetChunk(l)?.GenMesh();
        }

        if (localSpace.Z == 0)
        {
            var l = chunkCoord;
            l.Z--;
            TryGetChunk(l)?.GenMesh();
        }
        else if (localSpace.Z == 15)
        {
            var l = chunkCoord;
            l.Z++;
            TryGetChunk(l)?.GenMesh();
        }
    }

    public Chunk? GetChunk(IntVector3 pos)
    {
        var coord = GetChunkCoordinate(pos);
        return TryGetChunk(coord);
    }

    public static IntVector3 GetChunkCoordinate(IntVector3 blockCoordinate)
    {
        return new IntVector3(GetChunk(blockCoordinate.X), GetChunk(blockCoordinate.Y), GetChunk(blockCoordinate.Z));
    }

    private static int GetChunk(float x)
    {
        if (x < 0)
            return (int)Math.Floor(-(-x / 16));
        return (int)Math.Floor(x / 16);
    }

    private IntVector3 WorldToChunkSpace(IntVector3 worldPos)
    {
        return new IntVector3(SingleDimension(worldPos.X), SingleDimension(worldPos.Y), SingleDimension(worldPos.Z));

        int SingleDimension(int x)
        {
            if (x < 0)
                return (int)(15 + (x + 1) % 16);
            return (int)(x % 16);
        }
    }

    private IntVector3 ChunkToWorldSpace(IntVector3 blockInChunkPos, IntVector3 chunk)
    {
        const int chunkSize = 16;

        // World coordinate = (chunk coordinate * chunk size) + block index inside chunk
        return new IntVector3(
            chunk.X * chunkSize + blockInChunkPos.X,
            chunk.Y * chunkSize + blockInChunkPos.Y,
            chunk.Z * chunkSize + blockInChunkPos.Z
        );
    }
}
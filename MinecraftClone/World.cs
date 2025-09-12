global using static RayLib3dTest.World;

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

        Texture2D = LoadTexture("resources/textureatlas.png");
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
        var blockPosX = GetBlock(pos.X);
        var blockPosY = GetBlock(pos.Y);
        var blockPosZ = GetBlock(pos.Z);

        if (!Chunks.ContainsKey(new IntVector3(chunkPosX, chunkPosY, chunkPosZ)))
        {
            wasFound = false;
            return ref _emptyBlock;
        }

        if (blockPosX is > 15 or < 0 || blockPosY is > 15 or < 0 || blockPosZ is > 15 or < 0)
        {
            wasFound = false;
            return ref _emptyBlock;
        }

        return ref Chunks[new IntVector3(chunkPosX, chunkPosY, chunkPosZ)].Blocks[blockPosX, blockPosY, blockPosZ];
    }

    public Chunk GetChunk(IntVector3 pos)
    {
        return Chunks[new IntVector3(GetChunk(pos.X),GetChunk(pos.Y),GetChunk(pos.Z))];
    }
    
    private static int GetChunk(float x)
    {
        if(x < 0)
            return (int)Math.Floor(-(-x / 16));
        return (int)Math.Floor(x/16);
    }

    private int GetBlock(float x)
    {
        if(x < 0)
            return (int)(15 + (x + 1) % 16);
        return (int)(x % 16);
    }
}

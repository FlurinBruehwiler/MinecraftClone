using System.Numerics;
using Raylib_cs;

namespace RayLib3dTest;

public class GlobalBoy
{
    public Chunk[,,] Chunks;
    public Texture2D Texture2D;

    private Block _emptyBlock = new Block();
    
    public GlobalBoy(Textures textures)
    {
        Chunks = new Chunk[16,1,16];
        for (var x = 0; x < 16; x++)
        {
            for (var y = 0; y < 1; y++)
            {
                for (var z = 0; z < 16; z++)
                {
                    Chunks[x, y, z] = new Chunk(this, textures)
                    {
                        Pos = new IntVector3(x, y, z)
                    };
                }
            }
        }
    }

    public ref Block TryGetBlockAtPos(Vector3 pos, out bool wasFound)
    {
        return ref TryGetBlockAtPos(new IntVector3((int)pos.X, (int)pos.Y, (int)pos.Z), out wasFound);
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

        if (chunkPosX is > 15 or < 0 || chunkPosY != 0 || chunkPosZ is > 15 or < 0)
        {
            wasFound = false;
            return ref _emptyBlock;
        }

        if (blockPosX is > 15 or < 0 || blockPosY is > 15 or < 0 || blockPosZ is > 15 or < 0)
        {
            wasFound = false;
            return ref _emptyBlock;
        }

        return ref Chunks[chunkPosX, chunkPosY, chunkPosZ].Blocks[blockPosX, blockPosY, blockPosZ];
    }

    public Chunk GetChunk(IntVector3 pos)
    {
        return Chunks[GetChunk(pos.X),GetChunk(pos.Y),GetChunk(pos.Z)];
    }
    
    private int GetChunk(float x)
    {
        if(x < 0)
            return (int)Math.Floor(-(-x / 16));
        return (int)Math.Floor(x/16);
    }

    private int GetBlock(float x)
    {
        if(x < 0)
            return (int)(15 + ((x + 1) % 16));
        return (int)(x % 16);
    }
}
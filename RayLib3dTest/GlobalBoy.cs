using Raylib_cs;

namespace RayLib3dTest;

public class GlobalBoy
{
    public Chunk[,,] Chunks;
    public Texture2D Texture2D;
    
    public GlobalBoy()
    {
        Chunks = new Chunk[16,1,16];
        for (var x = 0; x < 16; x++)
        {
            for (var y = 0; y < 1; y++)
            {
                for (var z = 0; z < 16; z++)
                {
                    Chunks[x, y, z] = new Chunk(this)
                    {
                        Pos = new IntVector3(x, y, z)
                    };
                }
            }
        }
    }

    public Block? GetBlockAtPos(IntVector3 pos)
    { 
        var chunkPosX = pos.X / 16;
        var chunkPosY = pos.Y / 16;
        var chunkPosZ = pos.Z / 16;
        var blockPosX = pos.X % 16;
        var blockPosY = pos.Y % 16;
        var blockPosZ = pos.Z % 16;

        if (chunkPosX is > 15 or < 0 || chunkPosY != 0 || chunkPosZ is > 15 or < 0)
            return null;

        if (blockPosX is > 15 or < 0 || blockPosY is > 15 or < 0 || blockPosZ is > 15 or < 0)
            return null;

        return Chunks[chunkPosX, chunkPosY, chunkPosZ].Blocks[blockPosX, blockPosY, blockPosZ];
    }
}
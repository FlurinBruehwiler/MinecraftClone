using System.Diagnostics;
using System.Runtime.InteropServices;

namespace MinecraftClone;

public class World
{
    public readonly Game Game;
    public static World CurrentWorld;

    public Dictionary<IntVector3, Chunk> Chunks = new();
    public Texture2D TextureAtlas;
    public Texture2D BlockPreviewAtlas;

    private Block _emptyBlock;
    public List<Bot> bots = [];

    public World(Game game)
    {
        Game = game;
        TextureAtlas = MinecraftClone.TextureAtlas.Create();
        BlockPreviewAtlas = MinecraftClone.TextureAtlas.GenerateBlockPreviews(TextureAtlas, game.ChunkShader);

        // LoadFromDirectory(Game.SaveLocation);
    }


    public void AdvanceTextureAnimation()
    {
        foreach (var (_, texture) in Textures.TextureList)
        {
            if (texture.IsAnimated())
            {
                var totalFrameCount = texture.Image.Height / texture.Image.Width;

                texture.CurrentAnimationFrame++;
                texture.CurrentAnimationFrame %= totalFrameCount;

                MinecraftClone.TextureAtlas.ChangeAnimationFrame(TextureAtlas, texture);
                return;
            }
        }
    }

    public ref Block TryGetBlockAtPos(Vector3 pos, out bool wasFound)
    {
        return ref TryGetBlockAtPos(new IntVector3((int)pos.X, (int)pos.Y, (int)pos.Z), out wasFound);
    }

    public static IntVector3 GetChunkPos(Vector3 pos)
    {
        return new IntVector3(GetChunk(pos.X), GetChunk(pos.Y), GetChunk(pos.Z));
    }

    public Chunk? GetChunkContainingBlock(IntVector3 blocKPos)
    {
        var chunkPos = new IntVector3(GetChunk(blocKPos.X),
            GetChunk(blocKPos.Y),
            GetChunk(blocKPos.Z));

        if (Chunks.TryGetValue(chunkPos, out var chunk))
        {
            return chunk;
        }

        return null;
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

        return ref Chunks[new IntVector3(chunkPosX, chunkPosY, chunkPosZ)].Blocks[Chunk.GetIdx(blockInChunk.X, blockInChunk.Y, blockInChunk.Z)];
    }

    public unsafe void SaveToDirectory(string dir)
    {
        if (!Directory.Exists(dir))
        {
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch
            {
                Console.WriteLine("Failed do create directory");
            }

            if (!Directory.Exists(dir))
            {
                Console.WriteLine($"Directory {dir} does not exist");
                return;
            }
        }

        foreach (var (pos, chunk) in Chunks)
        {
            fixed (Block* blocks = chunk.Blocks)
            {
                var blockSpan = new ReadOnlySpan<Block>(blocks, chunk.Blocks.Length);
                var byteSpan = MemoryMarshal.Cast<Block, byte>(blockSpan);
                var path = Path.Combine(dir, $"{pos.X}.{pos.Y}.{pos.Z}.chunk");
                File.WriteAllBytes(path, byteSpan);
            }
        }
    }

    public static unsafe Chunk? LoadFromDirectory(IntVector3 chunkPos)
    {
        if (!Directory.Exists(Game.SaveLocation))
        {
            Console.WriteLine($"Directory {Game.SaveLocation} does not exist");
            return null;
        }

        var chunkFilePath = Path.Combine(Game.SaveLocation, $"{chunkPos.X}.{chunkPos.Y}.{chunkPos.Z}.chunk");

        if (!File.Exists(chunkFilePath))
        {
            return null;
        }
        var content = File.ReadAllBytes(chunkFilePath);

        fixed (byte* blocks = content)
        {
            var byteSpan = new ReadOnlySpan<byte>(blocks, content.Length);
            var blockSpan = MemoryMarshal.Cast<byte, Block>(byteSpan);

            if (blockSpan.Length != 16 * 16 * 16)
                return null;

            return new Chunk(CurrentWorld, blockSpan.ToArray())
            {
                Pos = chunkPos
            };
        }
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

    public bool IsSolid(IntVector3 pos)
    {
        var b = TryGetBlockAtPos(pos, out var found);
        if (found && !b.IsAir())
            return true;
        return false;
    }
}
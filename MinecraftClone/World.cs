using System.Runtime.InteropServices;

namespace RayLib3dTest;

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
        TextureAtlas = RayLib3dTest.TextureAtlas.Create();
        BlockPreviewAtlas = RayLib3dTest.TextureAtlas.GenerateBlockPreviews(TextureAtlas);

        LoadFromDirectory(Game.SaveLocation);
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

    public unsafe void LoadFromDirectory(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Console.WriteLine($"Directory {dir} does not exist");
            return;
        }

        foreach (var file in Directory.GetFiles(dir))
        {
            if (!file.EndsWith(".chunk"))
                continue;

            var coordsAsString = Path.GetFileNameWithoutExtension(file);
            var parts = coordsAsString.Split(".");
            if (parts.Length != 3)
                continue;

            bool isInvalid = false;
            isInvalid |= !int.TryParse(parts[0], out var x);
            isInvalid |= !int.TryParse(parts[1], out var y);
            isInvalid |= !int.TryParse(parts[2], out var z);

            if (isInvalid)
                continue;

            var content = File.ReadAllBytes(file);

            fixed (byte* blocks = content)
            {
                var byteSpan = new ReadOnlySpan<byte>(blocks, content.Length);
                var blockSpan = MemoryMarshal.Cast<byte, Block>(byteSpan);

                if(blockSpan.Length != 16 * 16 * 16)
                    continue;

                var pos = new IntVector3(x, y, z);
                Chunks.Add(pos, new Chunk(this, blockSpan.ToArray())
                {
                    Pos = pos
                });
            }
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
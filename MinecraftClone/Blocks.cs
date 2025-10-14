using SourceGen;

namespace RayLib3dTest;

public class Blocks
{
    public static BlockDefinition Air = new()
    {
        Id = 0,
        Name = "Air",

        IsTransparent = true
    };

    public static BlockDefinition Grass = new()
    {
        Id = 1,
        Name = "Gras",
        Model = Resources.grass_block //textures are referenced within the model
    };

    public static BlockDefinition Dirt = new()
    {
        Id = 2,
        Name = "Dirt",
        Textures = BlockDefinition.ConstructBlockTextures(all: Resources.Block.dirt),
        Model = Resources.blocks
    };

    public static BlockDefinition WoodenPlank = new()
    {
        Id = 3,
        Name = "WoodenPlank",
        Textures = BlockDefinition.ConstructBlockTextures(all: Resources.Block.oak_planks),
        Model = Resources.blocks
    };

    public static BlockDefinition Cobblestone = new()
    {
        Id = 4,
        Name = "Cobblestone",
        Textures = BlockDefinition.ConstructBlockTextures(all: Resources.Block.cobblestone),
        Model = Resources.blocks
    };

    public static BlockDefinition DiamondBlock = new()
    {
        Id = 5,
        Name = "Diamond Block",
        Textures = BlockDefinition.ConstructBlockTextures(all: Resources.Block.diamond_block),
        Model = Resources.blocks
    };

    public static BlockDefinition WoodenStairs = new()
    {
        Id = 6,
        Name = "Wooden Stair",
        Model = Resources.stairs,
        Textures = new()
        {
            { "side", Resources.Block.oak_planks },
            { "top", Resources.Block.oak_planks },
            { "bottom", Resources.Block.oak_planks },
        },
        IsTransparent = true
    };

    public static BlockDefinition OakLog = new()
    {
        Id = 7,
        Name = "Trunk",
        Textures = BlockDefinition.ConstructBlockTextures(bottom: Resources.Block.log_oak_top, sides: Resources.Block.log_oak, top: Resources.Block.log_oak_top),
        Model = Resources.blocks
    };

    public static BlockDefinition LeaveBlock = new()
    {
        Id = 8,
        Name = "Leave Block",
        Textures = BlockDefinition.ConstructBlockTextures(all: Resources.Block.oak_leaves),
        Model = Resources.blocks,
        IsTransparent = true
    };

    public static BlockDefinition Beacon = new()
    {
        Id = 9,
        Name = "Beacon Block",
        Textures = new()
        {
            { "glass", Resources.Block.glass },
            { "obsidian", Resources.Block.obsidian },
            { "beacon", Resources.Block.beacon },
        },
        Model = Resources.beacon,
        IsTransparent = true
    };

    public static BlockDefinition ShortGrass = new()
    {
        Id = 10,
        Name = "Short Grass",
        Textures = new ()
        {
            {"cross", Resources.Block.short_grass}
        },
        Model = Resources.grass_cross,
        IsTransparent = true
    };

    public static BlockDefinition TallGrassBottom = new()
    {
        Id = 11,
        Name = "Short Grass",
        Textures = new ()
        {
            {"cross", Resources.Block.tall_grass_bottom}
        },
        Model = Resources.grass_cross,
        IsTransparent = true
    };

    public static BlockDefinition TallGrassTop = new()
    {
        Id = 12,
        Name = "Short Grass",
        Textures = new ()
        {
            {"cross", Resources.Block.tall_grass_top}
        },
        Model = Resources.grass_cross,
        IsTransparent = true
    };

    public static Dictionary<ushort, BlockDefinition> BlockList;
}
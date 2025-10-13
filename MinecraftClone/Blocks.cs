using System.Reflection;
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
        Model = "grass_block.json" //textures are referenced within the model
    };

    public static BlockDefinition Dirt = new()
    {
        Id = 2,
        Name = "Dirt",
        Textures = BlockDefinition.ConstructBlockTextures(all: Resources.Block.dirt),
        Model = "blocks.json"
    };

    public static BlockDefinition WoodenPlank = new()
    {
        Id = 3,
        Name = "WoodenPlank",
        Textures = BlockDefinition.ConstructBlockTextures(all: Resources.Block.oak_planks),
        Model = "blocks.json"
    };

    public static BlockDefinition Cobblestone = new()
    {
        Id = 4,
        Name = "Cobblestone",
        Textures = BlockDefinition.ConstructBlockTextures(all: Resources.Block.cobblestone),
        Model = "blocks.json"
    };

    public static BlockDefinition DiamondBlock = new()
    {
        Id = 5,
        Name = "Diamond Block",
        Textures = BlockDefinition.ConstructBlockTextures(all: Textures.DiamondBlock),
        Model = "blocks.json"
    };

    public static BlockDefinition WoodenStairs = new()
    {
        Id = 6,
        Name = "Wooden Stair",
        Model = "stairs.json",
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
        Model = "blocks.json"
    };

    public static BlockDefinition LeaveBlock = new()
    {
        Id = 8,
        Name = "Leave Block",
        Textures = BlockDefinition.ConstructBlockTextures(all: Resources.Block.azalea_leaves),
        Model = "blocks.json",
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
        Model = "beacon.json",
        IsTransparent = true
    };

    public static BlockDefinition ShortGrass = new()
    {
        Id = 10,
        Name = "Short Grass",
        Model = "short_grass.json",
        IsTransparent = true
    };

    public static Dictionary<ushort, BlockDefinition> BlockList;
}
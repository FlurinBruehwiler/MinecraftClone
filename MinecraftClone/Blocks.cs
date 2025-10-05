using System.Reflection;

namespace RayLib3dTest;

public class Blocks
{
    public static BlockDefinition Air = new()
    {
        Id = 0,
        Name = "Air"
    };
    
    public static BlockDefinition Gras = new()
    {
        Id = 1,
        Name = "Gras",
        Textures = BlockDefinition.ConstructBlockTextures(bottom: Textures.Dirt, sides: Textures.Grass, top: Textures.GrassTop),
        Model = "blocks.json"
    };
    
    public static BlockDefinition Dirt = new()
    {
        Id = 2,
        Name = "Dirt",
        Textures = BlockDefinition.ConstructBlockTextures(all: Textures.Dirt),
        Model = "blocks.json"
    };

    public static BlockDefinition WoodenPlank = new()
    {
        Id = 3,
        Name = "WoodenPlank",
        Textures = BlockDefinition.ConstructBlockTextures(all: Textures.OakPlank),
        Model = "blocks.json"
    };
    
    public static BlockDefinition Cobblestone = new()
    {
        Id = 4,
        Name = "Cobblestone",
        Textures = BlockDefinition.ConstructBlockTextures(all: Textures.Cobblestone),
        Model = "blocks.json"
    };
    
    public static BlockDefinition DiamondBlock = new()
    {
        Id = 5,
        Name = "Diamon Block",
        Textures = BlockDefinition.ConstructBlockTextures(all: Textures.DiamondBlock),
        Model = "blocks.json"
    };

    public static BlockDefinition WoodenStairs = new()
    {
        Id = 6,
        Name = "Wooden Stair",
        Model = "stairs.json",
        Textures = new() {
            {"#side", Textures.OakPlank},
            {"#top", Textures.OakPlank},
            {"#bottom", Textures.OakPlank},
        },
    };

    public static BlockDefinition OakLog = new()
    {
        Id = 7,
        Name = "Trunk",
        Textures = BlockDefinition.ConstructBlockTextures(bottom: Textures.LogOakTop, sides: Textures.LogOak, top: Textures.LogOakTop),
        Model = "blocks.json"
    };

    public static BlockDefinition LeaveBlock = new()
    {
        Id = 8,
        Name = "Leave Block",
        Textures = BlockDefinition.ConstructBlockTextures(all: Textures.Leave),
        Model = "blocks.json"
    };

    public static Dictionary<ushort, BlockDefinition> BlockList
    {
        get
        {
            if (field == null)
            {
                field = typeof(Blocks)
                    .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                    .Where(x => !x.IsInitOnly && x.FieldType == typeof(BlockDefinition))
                    .Select(x => (BlockDefinition)x.GetValue(null)!)
                    .ToDictionary(x => x.Id, x => x);

                foreach (var (_, block) in field)
                {
                    block.ParsedModel = BlockModels.Get(block.Model);
                }
            }

            return field;
        }
    }

}
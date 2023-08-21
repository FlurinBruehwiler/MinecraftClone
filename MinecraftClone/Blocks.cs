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
        TopTexture = Textures.GrassTop,
        BottomTexture = Textures.Dirt,
        SideTexture = Textures.Grass
    };
    
    public static BlockDefinition Dirt = new()
    {
        Id = 2,
        Name = "Dirt",
        Texture = Textures.Dirt,
    };
    
    public static BlockDefinition WoodenPlank = new()
    {
        Id = 3,
        Name = "WoodenPlank",
        Texture = Textures.OakPlank,
    };
    
    public static BlockDefinition Cobblestone = new()
    {
        Id = 4,
        Name = "Cobblestone",
        Texture = Textures.Cobblestone,
    };
    
    public static BlockDefinition DiamondBlock = new()
    {
        Id = 5,
        Name = "Diamon Block",
        Texture = Textures.DiamonBlock,
    };
    
    public static Dictionary<ushort, BlockDefinition> BlockList { get; }

    static Blocks()
    {
        BlockList = typeof(Blocks)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(x => !x.IsInitOnly && x.FieldType == typeof(BlockDefinition))
            .Select(x => (BlockDefinition)x.GetValue(null)!)
            .ToDictionary(x => x.Id, x => x);
    }
}
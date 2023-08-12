using System.Reflection;

namespace RayLib3dTest;

public class Blocks
{
    public static BlockDefinition Air = new()
    {
        ID = 0,
        Name = "Air"
    };
    
    public static BlockDefinition Gras = new()
    {
        ID = 1,
        Name = "Gras",
        TopTexture = Textures.GrassTop,
        BottomTexture = Textures.Dirt,
        SideTexture = Textures.Grass
    };
    
    public static BlockDefinition Dirt = new()
    {
        ID = 2,
        Name = "Dirt",
        Texture = Textures.Dirt,
    };
    
    public Dictionary<int, BlockDefinition> BlockList { get; }

    public Blocks()
    {
        BlockList = typeof(Blocks)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(x => !x.IsInitOnly && x.FieldType == typeof(BlockDefinition))
            .Select(x => (BlockDefinition)x.GetValue(null)!)
            .ToDictionary(x => x.ID, x => x);
    }
}
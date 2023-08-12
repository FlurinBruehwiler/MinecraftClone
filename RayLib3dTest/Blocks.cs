using System.Reflection;

namespace RayLib3dTest;

public class Blocks
{
    public static BlockDefinition Gras = new()
    {
        ID = 1,
        Name = "Gras",
        TopTexture = Textures.GrassTop,
        BottomTexture = Textures.Dirt,
        SideTexture = Textures.Grass
    };
    
    public Dictionary<int, BlockDefinition> BlockList { get; }

    public Blocks()
    {
        BlockList = typeof(Blocks)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(x => x is { IsInitOnly: false } && x.FieldType == typeof(BlockDefinition))
            .Select(x => (BlockDefinition)x.GetValue(null)!)
            .ToDictionary(x => x.ID, x => x);
    }
}
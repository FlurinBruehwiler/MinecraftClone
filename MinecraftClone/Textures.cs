using System.Reflection;

namespace RayLib3dTest;

public static class Textures
{
    public const string Dirt = "dirt";
    public const string Grass = "grass";
    public const string GrassTop = "grasstop";
    public const string OakPlank = "oak_planks";
    public const string Cobblestone = "cobblestone";
    public const string DiamonBlock = "diamond_block";

    public static Dictionary<string, int> TextureList { get; }

    static Textures()
    {
        var counter = 0;
        TextureList = typeof(Textures)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(x => x is { IsLiteral: true, IsInitOnly: false } && x.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue()!)
            .ToDictionary(x => x, _ => counter++);
    }

    public static IntVector2 GetTexturePosForFace(ushort blockId, BlockFace blockFace)
    {
        var blockDefinition = Blocks.BlockList[blockId];
        var tex = blockFace switch
        {
            BlockFace.Left => blockDefinition.LeftTexture,
            BlockFace.Right => blockDefinition.RightTexture,
            BlockFace.Bottom => blockDefinition.BottomTexture,
            BlockFace.Top => blockDefinition.TopTexture,
            BlockFace.Back => blockDefinition.BackTexture,
            BlockFace.Front => blockDefinition.FrontTexture,
            _ => throw new ArgumentOutOfRangeException(nameof(blockFace), blockFace, null)
        };

        var idx = TextureList[tex];

        return new IntVector2(idx % 10, idx / 10);
    }
}
using System.Reflection;

namespace RayLib3dTest;

public static class Textures
{
    public const string Dirt = "dirt";
    public const string Grass = "grass";
    public const string GrassTop = "grasstop";
    public const string OakPlank = "oak_planks";
    public const string Cobblestone = "cobblestone";
    public const string DiamondBlock = "diamond_block";
    public const string LogOak = "log_oak";
    public const string LogOakTop = "log_oak_top";
    public const string Leave = "azalea_leaves";

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

        if (TextureList.TryGetValue(tex, out var idx))
        {
            return new IntVector2(idx % 10, idx / 10);
        }

        return new IntVector2(0, 0);
    }

    public static UvCoordinates GetUvCoordinatesForFace(ushort blockId, BlockFace blockFace)
    {
        var texture = GetTexturePosForFace(blockId, blockFace);

        UvCoordinates uvCoordinates = default;
        uvCoordinates.topLeft = new Vector2(0.1f * texture.X, 0.1f * texture.Y);
        uvCoordinates.topRight = new Vector2(uvCoordinates.topLeft.X + 0.1f, uvCoordinates.topLeft.Y);
        uvCoordinates.bottomLeft = new Vector2(uvCoordinates.topLeft.X, uvCoordinates.topLeft.Y + 0.1f);
        uvCoordinates.bottomRight = new Vector2(uvCoordinates.topRight.X, uvCoordinates.bottomLeft.Y);

        return uvCoordinates;
    }
}

public struct UvCoordinates
{
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
}
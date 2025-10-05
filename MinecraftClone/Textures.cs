using System.Reflection;

namespace RayLib3dTest;

public struct BlockDev
{
    public Vector3 From;
    public Vector3 To;

    public Vector3 BottomLeftFront()
    {
        return new Vector3(From.X, From.Y, From.Z) / 16;
    }
    public Vector3 BottomRightFront()
    {
        return new Vector3(To.X, From.Y, From.Z) / 16;
    }
    public Vector3 BottomLeftBack()
    {
        return new Vector3(From.X, From.Y, To.Z) / 16;
    }
    public Vector3 BottomRightBack()
    {
        return new Vector3(To.X, From.Y, To.Z) / 16;
    }
    public Vector3 TopLeftFront()
    {
        return new Vector3(From.X, To.Y, From.Z) / 16;
    }
    public Vector3 TopRightFront()
    {
        return new Vector3(To.X, To.Y, From.Z) / 16;
    }
    public Vector3 TopLeftBack()
    {
        return new Vector3(From.X, To.Y, To.Z) / 16;
    }
    public Vector3 TopRightBack()
    {
        return new Vector3(To.X, To.Y, To.Z) / 16;
    }
}

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
    public const string Glass = "glass";
    public const string Obsidian = "obsidian";
    public const string Beacon = "beacon";

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

    public static IntVector2 GetTexturePosForBlockPreview(ushort blockId)
    {
        //this code is superbad

        blockId--;
        return new IntVector2(blockId % 10, blockId / 10);
    }

    // public static IntVector2 GetTexturePosForFace(ushort blockId, BlockFace blockFace)
    // {
    //     var blockDefinition = Blocks.BlockList[blockId];
    //     var tex = blockFace switch
    //     {
    //         BlockFace.Left => blockDefinition.LeftTexture,
    //         BlockFace.Right => blockDefinition.RightTexture,
    //         BlockFace.Bottom => blockDefinition.BottomTexture,
    //         BlockFace.Top => blockDefinition.TopTexture,
    //         BlockFace.Back => blockDefinition.BackTexture,
    //         BlockFace.Front => blockDefinition.FrontTexture,
    //         _ => throw new ArgumentOutOfRangeException(nameof(blockFace), blockFace, null)
    //     };
    //
    //     if (TextureList.TryGetValue(tex, out var idx))
    //     {
    //         return new IntVector2(idx % 10, idx / 10);
    //     }
    //
    //     return new IntVector2(0, 0);
    // }

    // public static UvCoordinates GetUvCoordinatesForFace(ushort blockId, BlockFace blockFace)
    // {
    //     var texture = GetTexturePosForFace(blockId, blockFace);
    //
    //     UvCoordinates uvCoordinates = default;
    //     uvCoordinates.topLeft = new Vector2(0.1f * texture.X, 0.1f * texture.Y);
    //     uvCoordinates.topRight = new Vector2(uvCoordinates.topLeft.X + 0.1f, uvCoordinates.topLeft.Y);
    //     uvCoordinates.bottomLeft = new Vector2(uvCoordinates.topLeft.X, uvCoordinates.topLeft.Y + 0.1f);
    //     uvCoordinates.bottomRight = new Vector2(uvCoordinates.topRight.X, uvCoordinates.bottomLeft.Y);
    //
    //     return uvCoordinates;
    // }

    public static UvCoordinates GetUvCoordinatesForTexture(string textureId, Vector4 subUvCoordinates)
    {
        Vector2 baseCoords = default;

        if (TextureList.TryGetValue(textureId, out var idx))
        {
            var slot = new Vector2(idx % 10, idx / 10);
            baseCoords = new Vector2(0.1f * slot.X, 0.1f * slot.Y);
        }

        var bottomLeftOffset = new Vector2(subUvCoordinates.X, subUvCoordinates.W) / 16 / 10;
        var bottomRightOffset = new Vector2(subUvCoordinates.Z, subUvCoordinates.W) / 16 / 10;
        var topLeftOffset = new Vector2(subUvCoordinates.X, subUvCoordinates.Y) / 16 / 10;
        var topRightOffset = new Vector2(subUvCoordinates.Z, subUvCoordinates.Y) / 16 / 10;

        UvCoordinates uvCoordinates = default;
        uvCoordinates.topLeft = baseCoords + topLeftOffset;
        uvCoordinates.topRight = baseCoords + topRightOffset;
        uvCoordinates.bottomLeft = baseCoords + bottomLeftOffset;
        uvCoordinates.bottomRight = baseCoords + bottomRightOffset;

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
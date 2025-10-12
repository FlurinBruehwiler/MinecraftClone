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

[AttributeUsage(AttributeTargets.Field)]
public class TextureAttribute : Attribute
{
    public bool IsFoliage { get; set; }
}

public struct TextureDefinition
{
    public int Id;
    public bool IsFoliage;
}

public static class Textures
{
    public static Dictionary<string, TextureDefinition> TextureList;

    public static IntVector2 GetTexturePosForBlockPreview(ushort blockId)
    {
        //this code is superbad

        blockId--;
        return new IntVector2(blockId % 10, blockId / 10);
    }

    public static (UvCoordinates, Color color) GetUvCoordinatesForTexture(string textureId, Vector4 subUvCoordinates)
    {
        Vector2 baseCoords = default;
        Color color = Color.White;

        if (TextureList.TryGetValue(textureId, out var t))
        {
            var slot = new Vector2(t.Id % 10, t.Id / 10);
            baseCoords = new Vector2(0.1f * slot.X, 0.1f * slot.Y);

            if (t.IsFoliage)
            {
                color = new Color(146, 193, 98);
            }
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

        return (uvCoordinates, color);
    }
}

public struct UvCoordinates
{
    public Vector2 topLeft;
    public Vector2 topRight;
    public Vector2 bottomLeft;
    public Vector2 bottomRight;
}
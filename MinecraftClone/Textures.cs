using System.Net.Quic;
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

public class TextureDefinition
{
    public int Id;
    public bool IsFoliage;
    public UvCoordinates UvCoordinates;
    public Image Image;
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
        if (TextureList.TryGetValue(textureId, out var t))
        {

            Color color = Color.White;

            if (t.IsFoliage)
            {
                color = new Color(146, 193, 98);
            }

            var topLeftOffset = new Vector2(subUvCoordinates.X, subUvCoordinates.Y) / 16 / 10;
            var bottomRight = new Vector2(subUvCoordinates.Z, subUvCoordinates.W) / 16 / 10;

            var tl = t.UvCoordinates.PosFromTopLeft + topLeftOffset;
            var br = t.UvCoordinates.PosFromTopLeft + bottomRight;

            var baseUvCoordinates = new UvCoordinates(tl, br - tl);

            return (baseUvCoordinates, color);
        }

        return new ValueTuple<UvCoordinates, Color>();
    }
}

public struct UvCoordinates
{
    public Vector2 PosFromTopLeft;
    public Vector2 Size;

    public UvCoordinates(Vector2 pos, Vector2 size) //origin topLeft
    {
        PosFromTopLeft = pos;
        Size = size;
    }

    public Vector2 topLeft() => PosFromTopLeft;
    public Vector2 topRight() => new Vector2(PosFromTopLeft.X + Size.X, PosFromTopLeft.Y);
    public Vector2 bottomLeft() => new Vector2(PosFromTopLeft.X, PosFromTopLeft.Y + Size.Y);
    public Vector2 bottomRight() => PosFromTopLeft + Size;

    public float Width() => topRight().X - topLeft().X;
    public float Height() => bottomLeft().Y - bottomLeft().X;
}
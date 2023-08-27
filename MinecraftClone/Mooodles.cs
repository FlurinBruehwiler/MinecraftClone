namespace RayLib3dTest;

record struct PrintMessage(string Value, string Name);
record struct DebugLine(Vector3 Start, Vector3 Direction, Color Color);
record struct DebugLine2d(Vector2 Direction, Color Color);

public record struct Block
{
    public ushort BlockId;

    public bool IsAir()
    {
        return BlockId == Blocks.Air.Id;
    }
}

public enum BlockFace
{
    Left,
    Right,
    Bottom,
    Top,
    Back,
    Front
}

public record struct IntVector3(int X, int Y, int Z)
{
    public static IntVector3 operator +(IntVector3 left, IntVector3 right)
    {
        return new IntVector3(
            left.X + right.X,
            left.Y + right.Y,
            left.Z + right.Z
        );
    }

    public static IntVector3 operator *(IntVector3 left, int factor)
    {
        return new IntVector3(
            left.X * factor,
            left.Y * factor,
            left.Z * factor
        );
    }

    public override int GetHashCode()
    {
        return X + Y * 1000 + Z * 1000000;
    }
}

public record struct IntVector2(int X, int Y)
{
    public static IntVector2 operator +(IntVector2 left, IntVector2 right)
    {
        return new IntVector2(
            left.X + right.X,
            left.Y + right.Y
        );
    }

    public static IntVector2 operator *(IntVector2 left, int factor)
    {
        return new IntVector2(
            left.X * factor,
            left.Y * factor
        );
    }
}

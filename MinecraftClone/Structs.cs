namespace RayLib3dTest;

record struct DebugLine(Vector3 Start, Vector3 Direction, Color Color);
record struct DebugLine2d(Vector2 Direction, Color Color);

public record struct Block
{
    public ushort BlockId;

    public bool IsAir()
    {
        return BlockId == Blocks.Air.Id;
    }

    public bool IsSolid()
    {
        return BlockId == Blocks.Air.Id || BlockId == Blocks.ShortGrass.Id;
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

public struct IntVector3 : IEquatable<IntVector3 >
{
    public int X;
    public int Y;
    public int Z;

    public IntVector3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public IntVector3(int x)
    {
        X = x;
        Y = x;
        Z = x;
    }

    public static readonly IntVector3 Zero = new IntVector3(0);

    public override string ToString()
    {
        return $"{X}, {Y}, {Z}";
    }

    public static bool operator ==(IntVector3 left, IntVector3 right)
    {
        return left.X == right.X && left.Y == right.Y && left.Z == right.Z;
    }


    public static bool operator !=(IntVector3 left, IntVector3 right)
    {
        return !(left == right);
    }

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

    public bool Equals(IntVector3 other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntVector3 other && Equals(other);
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

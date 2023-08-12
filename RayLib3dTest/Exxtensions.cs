using System.Numerics;

namespace RayLib3dTest;

public static class Exxtensions
{
    public static IntVector3 GetContainingBlock(this Vector3 pos)
    {
        return new IntVector3((int)pos.X, (int)pos.Y, (int)pos.Z);
    }
}
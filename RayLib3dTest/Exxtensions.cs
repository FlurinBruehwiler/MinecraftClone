using System.Numerics;

namespace RayLib3dTest;

public static class Exxtensions
{
    public static IntVector3 ToIntVector3(this Vector3 pos)
    {
        return new IntVector3((int)pos.X, (int)pos.Y, (int)pos.Z);
    }
    
    public static Vector3 ToVector3(this IntVector3 pos)
    {
        return new Vector3(pos.X, pos.Y, pos.Z);
    }
}
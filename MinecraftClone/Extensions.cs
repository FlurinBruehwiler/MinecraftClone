namespace MinecraftClone;

public static class Extensions
{
    public static IntVector3 ToIntVector3(this Vector3 pos)
    {
        return new IntVector3((int)Math.Floor(pos.X),
            (int)Math.Floor(pos.Y),
            (int)Math.Floor(pos.Z));
    }
    
    public static Vector3 ToVector3NonCenter(this IntVector3 pos)
    {
        return new Vector3(pos.X, pos.Y, pos.Z);
    }

    public static Vector3 ToVector3(this IntVector3 pos)
    {
        return new Vector3(pos.X + 0.5f, pos.Y + 0.5f, pos.Z + 0.5f);
    }
}
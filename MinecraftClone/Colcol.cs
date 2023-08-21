namespace RayLib3dTest;

public class Colcol
{
    private readonly GlobalBoy _globalBoy;

    public Colcol(GlobalBoy globalBoy)
    {
        _globalBoy = globalBoy;
    }
    
    public IntVector3? Raycast(Vector3 pos, Vector3 dir, float length, out IntVector3 previousBlock, out float distance)
    {
        dir = Vector3.Normalize(dir);
        distance = 0f;
        var start = pos.ToIntVector3();
        previousBlock = new IntVector3();
        
        var step = new IntVector3(
            dir.X > 0 ? 1 : -1, 
            dir.Y > 0 ? 1 : -1, 
            dir.Z > 0 ? 1 : -1);
        
        var delta = new Vector3(
            Math.Abs(1 / dir.X), 
            Math.Abs(1 / dir.Y), 
            Math.Abs(1 / dir.Z));
        
        var dist = new Vector3(
            step.X > 0 ? start.X + 1 - pos.X : pos.X - start.X,
            step.Y > 0 ? start.Y + 1 - pos.Y : pos.Y - start.Y,
            step.Z > 0 ? start.Z + 1 - pos.Z : pos.Z - start.Z);
        
        var tMax = new Vector3(
            delta.X * dist.X, 
            delta.Y * dist.Y, 
            delta.Z * dist.Z);
        
        while (distance <= length)
        {
            var b = _globalBoy.TryGetBlockAtPos(start, out var wasFound);

            if (wasFound)
            {
                if (!b.IsAir())
                {
                    return start;
                }
            }
            
            previousBlock = start;
            
            if (tMax.X < tMax.Y)
            {
                if (tMax.X < tMax.Z)
                {
                    start.X += step.X;
                    distance = tMax.X;
                    tMax.X += delta.X;
                }
                else
                {
                    start.Z += step.Z;
                    distance = tMax.Z;
                    tMax.Z += delta.Z;
                }
            }
            else
            {
                if (tMax.Y < tMax.Z)
                {
                    start.Y += step.Y;
                    distance = tMax.Y;
                    tMax.Y += delta.Y;
                }
                else
                {
                    start.Z += step.Z;
                    distance = tMax.Z;
                    tMax.Z += delta.Z;
                }
            }
        }

        return null;
    }
}
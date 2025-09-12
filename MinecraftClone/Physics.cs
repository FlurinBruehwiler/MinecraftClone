namespace RayLib3dTest;

public static class Physics
{
    private const float SkinWidth = 0.015f;
    private const float PlayerHeight = 1.8f;
    private const float PlayerWidth = 0.6f;

    private const float ReducedHeight = PlayerHeight - SkinWidth;
    private const float ReducedWidth = PlayerWidth - SkinWidth;
    private const float HalfPlayerWidth = ReducedWidth / 2;
    
    private static List<Vector2> _verticalRayOrigins = new()
    {
        new Vector2(-HalfPlayerWidth, -HalfPlayerWidth),
        new Vector2(-HalfPlayerWidth, +HalfPlayerWidth),
        new Vector2(+HalfPlayerWidth, -HalfPlayerWidth),
        new Vector2(+HalfPlayerWidth, +HalfPlayerWidth),
    };

    private static List<Vector2> _forwardCollisions = new()
    {
        new Vector2(+HalfPlayerWidth, -ReducedHeight),
        new Vector2(-HalfPlayerWidth, -ReducedHeight),
        new Vector2(+HalfPlayerWidth, 0),
        new Vector2(-HalfPlayerWidth, 0),
    };
    
    private static List<Vector2> _sidewardCollisions = new()
    {
        new Vector2(+HalfPlayerWidth, -ReducedHeight),
        new Vector2(-HalfPlayerWidth, -ReducedHeight),
        new Vector2(+HalfPlayerWidth, 0),
        new Vector2(-HalfPlayerWidth, 0),
    };

    public static void DisplayRays(Vector3 velocity, Vector3 playerPos)
    {
        //x
        var directionX = Math.Sign(velocity.X);
        foreach (var verticalRayOrigin in _forwardCollisions)
        {
            var origin = new Vector3(directionX == -1 ? -HalfPlayerWidth : +HalfPlayerWidth, verticalRayOrigin.Y + velocity.Y, verticalRayOrigin.X + velocity.Z);
            origin += playerPos;
            var dir = new Vector3(directionX, 0, 0);

            DrawLine3D(origin, origin + dir, Color.BLUE);
        }
        
        //z
        var directionZ = Math.Sign(velocity.Z);
        foreach (var verticalRayOrigin in _sidewardCollisions)
        {
            var origin = new Vector3(verticalRayOrigin.X + velocity.X, verticalRayOrigin.Y + velocity.Y, directionZ == -1 ? -HalfPlayerWidth : +HalfPlayerWidth);

            origin += playerPos;
            var dir = new Vector3(0, 0, directionZ);
            
            DrawLine3D(origin, origin + dir, Color.RED);
        }
        
        
        //y
        var directionY = Math.Sign(velocity.Y);
        foreach (var verticalRayOrigin in _verticalRayOrigins)
        {
            var origin = new Vector3(verticalRayOrigin.X + velocity.X, directionY == -1 ? -ReducedHeight : 0, verticalRayOrigin.Y + velocity.Z);
            origin += playerPos;
            var dir = new Vector3(0, directionY, 0);
            
            DrawLine3D(origin, origin + dir, Color.YELLOW);
        }
    }
    
    public static void ForwardCollisions(ref Vector3 velocity, Vector3 playerPos)
    {
        var direction = Math.Sign(velocity.X);
        float rayLength = Math.Abs(velocity.X) + SkinWidth;

        foreach (var verticalRayOrigin in _forwardCollisions)
        {
            var origin = new Vector3(direction == -1 ? -HalfPlayerWidth : +HalfPlayerWidth, verticalRayOrigin.Y + velocity.Y, verticalRayOrigin.X + velocity.Z);
            origin += playerPos;
            var hit = Raycast(origin, new Vector3(direction, 0, 0), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.X = (distance - SkinWidth) * direction;
                rayLength = distance;
            }
        }
    }
    
    public static void SidewardCollisions(ref Vector3 velocity, Vector3 playerPos)
    {
        var direction = Math.Sign(velocity.Z);
        float rayLength = Math.Abs(velocity.Z) + SkinWidth;

        foreach (var verticalRayOrigin in _sidewardCollisions)
        {
            var origin = new Vector3(verticalRayOrigin.X + velocity.X, verticalRayOrigin.Y + velocity.Y, direction == -1 ? -HalfPlayerWidth : +HalfPlayerWidth);

            origin += playerPos;
            
            var hit = Raycast(origin, new Vector3(0, 0, direction), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.Z = (distance - SkinWidth) * direction;
                rayLength = distance;
            }
        }
    }

    public static void VerticalCollisions(ref Vector3 velocity, Vector3 playerPos, out bool isHit)
    {
        isHit = false;

        var direction = Math.Sign(velocity.Y);
        float rayLength = Math.Abs(velocity.Y) + SkinWidth;

        foreach (var verticalRayOrigin in _verticalRayOrigins)
        {
            var origin = new Vector3(verticalRayOrigin.X + velocity.X, direction == -1 ? -ReducedHeight : 0, verticalRayOrigin.Y + velocity.Z);
            origin += playerPos;
            var hit = Raycast(origin, new Vector3(0, direction, 0), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.Y = (distance - SkinWidth) * direction;
                rayLength = distance;
                isHit = true;
            }
        }
    }

    public static IntVector3? Raycast(Vector3 pos, Vector3 dir, float length, out IntVector3 previousBlock, out float distance)
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
            var b = CurrentWorld.TryGetBlockAtPos(start, out var wasFound);

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

public struct CollisionInfo
{
    public bool Up, Down, Left, Right, Forward, Backwards;

    public void Reset()
    {
        Up = Down = Left = Right = Forward = Backwards = false;
    }
}

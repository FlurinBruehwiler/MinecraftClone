namespace RayLib3dTest;

public static class Physics
{
    public const float SkinWidth = 0.01f;
    public const float PlayerHeight = 1.8f;
    public const float PlayerWidth = 0.6f;

    private const float ReducedHeight = PlayerHeight - SkinWidth;
    private const float ReducedWidth = PlayerWidth - SkinWidth;
    private const float HalfPlayerWidth = ReducedWidth / 2;
    
    private static List<Vector2> _verticalRayOrigins = new()
    {
        new Vector2(-1, -1),
        new Vector2(-1, +1),
        new Vector2(+1, -1),
        new Vector2(+1, +1),
    };

    private static List<Vector2> _forwardCollisions = new()
    {
        new Vector2(+1, -1),
        new Vector2(-1, -1),
        new Vector2(+1, +1),
        new Vector2(-1, +1),

        new Vector2(+1, 0),
        new Vector2(-1, 0),
    };
    
    private static List<Vector2> _sidewardCollisions = new()
    {
        new Vector2(+1, -1),
        new Vector2(-1, -1),

        new Vector2(+1, 0f),
        new Vector2(-1, 0f),


        new Vector2(+1, +1),
        new Vector2(-1, +1),
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

    private const bool collisionDebug = false;

    public static void ForwardCollisions(ref Vector3 velocity, ref CollisionInfo colInfo, Vector3 playerPos, Hitbox hitbox)
    {
        var direction = Math.Sign(velocity.X);

        if (direction == 0)
            return;

        float rayLength = Math.Abs(velocity.X) + SkinWidth;

        foreach (var verticalRayOrigin in _forwardCollisions)
        {
            var origin = playerPos + hitbox.Shrink(SkinWidth).GetCorner(new Vector3(direction, verticalRayOrigin.Y, verticalRayOrigin.X));
            var hit = Raycast(origin, new Vector3(direction, 0, 0), rayLength, out _, out var distance, collisionDebug);

            if (distance != 0 && !float.IsNormal(distance))
            {
                distance = 0;
            }

            if (hit is not null)
            {
                velocity.X = (distance - SkinWidth) * direction;
                rayLength = distance;

                colInfo.Backwards = direction < 0;
                colInfo.Forward = direction > 0;
            }
        }
    }

    public static void SidewardCollisions(ref Vector3 velocity, ref CollisionInfo colInfo, Vector3 playerPos, Hitbox hitbox)
    {
        var direction = Math.Sign(velocity.Z);

        if (direction == 0)
            return;

        float rayLength = Math.Abs(velocity.Z) + SkinWidth;

        foreach (var verticalRayOrigin in _sidewardCollisions)
        {
            var origin = playerPos + hitbox.Shrink(SkinWidth).GetCorner(new Vector3(verticalRayOrigin.X, verticalRayOrigin.Y, direction));
            var hit = Raycast(origin, new Vector3(0, 0, direction), rayLength, out _, out var distance, collisionDebug);

            if (distance != 0 && !float.IsNormal(distance))
            {
                distance = 0;
            }

            if (hit is not null)
            {
                velocity.Z = (distance - SkinWidth) * direction;
                rayLength = distance;

                colInfo.Left = direction < 0;
                colInfo.Right = direction > 0;
            }
        }
    }

    public static void VerticalCollisions(ref Vector3 velocity, ref CollisionInfo colInfo, Vector3 playerPos, Hitbox hitbox)
    {
        var direction = Math.Sign(velocity.Y);

        if (direction == 0)
            direction = -1;

        float rayLength = Math.Abs(velocity.Y) + SkinWidth;

        foreach (var verticalRayOrigin in _verticalRayOrigins)
        {
            var origin = playerPos + hitbox.Shrink(SkinWidth).GetCorner(new Vector3(verticalRayOrigin.X, direction, verticalRayOrigin.Y));
            var hit = Raycast(origin, new Vector3(0, direction, 0), rayLength, out _, out var distance, collisionDebug);

            if (distance != 0 && !float.IsNormal(distance))
            {
                distance = 0;
            }

            if (hit is not null)
            {
                velocity.Y = (distance - SkinWidth) * direction;
                rayLength = distance;

                colInfo.Down = direction < 0;
                colInfo.Up = direction > 0;
            }
        }
    }

    public static void DebugRayHit(Vector3 pos, Vector3 dir, float hitDistance)
    {
        DevTools.RenderActions.Add(() =>
            {
                DrawLine3D(pos, pos + (Vector3.Normalize(dir) * hitDistance), Color.RED);
                DrawSphere(pos + (Vector3.Normalize(dir) * hitDistance), 0.05f, Color.ORANGE);
            });
    }

    public static IntVector3? Raycast(Vector3 pos, Vector3 dir, float length, out IntVector3 previousBlock, out float distance, bool debug = false)
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
                    if (debug)
                    {
                        DebugRayHit(pos, dir, distance);
                    }
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

        if (debug)
        {
            DebugRayHit(pos, dir, distance);
        }
        return null;
    }

    public static bool AreOverlapping(Hitbox h1, Hitbox h2)
    {
        return (h1.MinVector.X <= h2.MaxVector.X && h1.MaxVector.X >= h2.MinVector.X) &&
               (h1.MinVector.Y <= h2.MaxVector.Y && h1.MaxVector.Y >= h2.MinVector.Y) &&
               (h1.MinVector.Z <= h2.MaxVector.Z && h1.MaxVector.Z >= h2.MinVector.Z);
    }
}

public struct CollisionInfo
{
    public bool Up, Down, Left, Right, Forward, Backwards;

    public void Reset()
    {
        this = default;
    }
}

public struct Hitbox
{
    public readonly Vector3 MinVector; //should have negative xyz
    public readonly Vector3 MaxVector; //should have positive xyz

    public Hitbox(Vector3 min, Vector3 max)
    {
        MinVector = min;
        MaxVector = max;
    }

    public Vector3 GetCenter()
    {
        return GetCorner(new Vector3(0, 0, 0));
    }

    public Vector3 GetSize()
    {
        return MaxVector - MinVector;
    }

    public Vector3 GetCorner(Vector3 corner)
    {
        Vector3 res = default;
        if (corner.X > 0)
            res.X = MaxVector.X;
        else if (corner.X == 0)
            res.X = MinVector.X + (MaxVector.X - MinVector.X) / 2;
        else
            res.X = MinVector.X;

        if (corner.Y > 0)
            res.Y = MaxVector.Y;
        else if(corner.Y == 0)
            res.Y = MinVector.Y + (MaxVector.Y - MinVector.Y) / 2;
        else
            res.Y = MinVector.Y;

        if (corner.Z > 0)
            res.Z = MaxVector.Z;
        else if(corner.Z == 0)
            res.Z = MinVector.Z + (MaxVector.Z - MinVector.Z) / 2;
        else
            res.Z = MinVector.Z;
        return res;
    }

    public Hitbox Shrink(float amount)
    {
        return new Hitbox(new Vector3(MinVector.X + amount, MinVector.Y + amount, MinVector.Z + amount),
            new Vector3(MaxVector.X - amount, MaxVector.Y - amount, MaxVector.Z - amount));
    }
}


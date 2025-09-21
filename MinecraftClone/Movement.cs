namespace RayLib3dTest;

public struct MovementConfig
{
    const float verticalAcceleration = 0.08f;
    const float defaultBlockFriction = 0.546f;
    const float verticalDrag = 0.98f;
    const float jumpVelocity = 0.42f;
    const float horizontalAcceleration = 0.098f;

    public float HorizontalAcceleration;
    public float VerticalAcceleration;
    public float HorizontalFriction;
    public float VerticalFriction;
    public float JumpVelocity;

    public static MovementConfig Default = new()
    {
        HorizontalFriction = defaultBlockFriction,
        JumpVelocity = jumpVelocity,
        HorizontalAcceleration = horizontalAcceleration,
        VerticalAcceleration = verticalAcceleration,
        VerticalFriction = verticalDrag
    };
}

public static class Movement
{
    public static Vector3 Forward(this Vector3 vec)
    {
        Vector3 right = new(-vec.Z, 0, vec.X);

        return Vector3.Normalize(new Vector3(-right.Z, 0, right.X));
    }

    public static Vector3 Right(this Vector3 vec)
    {
        return new(-vec.Z, 0, vec.X);
    }

    public static CollisionInfo Move(ref Vector3 velocity, ref Vector3 entityPosition, Vector3 entityDirection,
        Vector3 movementDir, Hitbox hitbox, bool shouldJump, MovementConfig movementConfig)
    {
        Vector3 Right = new(-entityDirection.Z, 0, entityDirection.X);

        var right = Vector3.Normalize(new Vector3(Right.X, 0, Right.Z));
        var forward = Vector3.Normalize(new Vector3(-Right.Z, 0, Right.X));

        var xComponent = right * movementDir.X;
        var zComponent = forward * movementDir.Z;

        var globalMoveDirection = xComponent + zComponent;
        globalMoveDirection.Y = movementDir.Y;

        if (globalMoveDirection.Length() != 0)
            globalMoveDirection = Vector3.Normalize(globalMoveDirection);

        velocity.X += globalMoveDirection.X * movementConfig.HorizontalAcceleration;
        velocity.Z += globalMoveDirection.Z * movementConfig.HorizontalAcceleration;

        var colInfo = new CollisionInfo();
        Physics.VerticalCollisions(ref velocity, ref colInfo, entityPosition, hitbox);
        Physics.ForwardCollisions(ref velocity, ref colInfo, entityPosition, hitbox);
        Physics.SidewardCollisions(ref velocity, ref colInfo, entityPosition, hitbox);

        entityPosition += velocity;

        if (!colInfo.Down)
            velocity.Y -= movementConfig.VerticalAcceleration;
        else
        {
            if (shouldJump) //cannot check IsKeyPressed because tick doesn't run every frame
            {
                velocity.Y = movementConfig.JumpVelocity;
            }
            else
            {
                velocity.Y = 0;
            }
        }

        velocity.X *= movementConfig.HorizontalFriction;
        velocity.Y *= movementConfig.VerticalFriction;
        velocity.Z *= movementConfig.HorizontalFriction;

        return colInfo;
    }
}
namespace RayLib3dTest;

public struct MovementConfig
{
    public float HorizontalAcceleration;
    public float VerticalAcceleration;
    public float HorizontalFriction;
    public float VerticalFriction;
    public float JumpVelocity;
}

public static class Movement
{
    public static void Move(ref Vector3 velocity, ref Vector3 entityPosition, Vector3 entityDirection,
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

        var posChange = velocity;

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
    }
}
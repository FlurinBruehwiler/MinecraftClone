using System.Numerics;

namespace RayLib3dTest;

public class SirPhysics
{
    private readonly Colcol _colcol;
    private const float skinWidth = 0.015f;
    private const float PlayerHeight = 1.8f;
    private const float PlayerWidth = 0.6f;

    private const float ReducedHeight = PlayerHeight - skinWidth;
    private const float ReducedWidth = PlayerWidth - skinWidth;
    private const float HalfPlayerWidth = ReducedWidth / 2;
    
    private List<Vector2> _verticalRayOrigins = new()
    {
        new Vector2(-HalfPlayerWidth, -HalfPlayerWidth),
        new Vector2(-HalfPlayerWidth, +HalfPlayerWidth),
        new Vector2(+HalfPlayerWidth, -HalfPlayerWidth),
        new Vector2(+HalfPlayerWidth, +HalfPlayerWidth),
    };

    private List<Vector2> _forwardCollisions = new()
    {
        new Vector2(+HalfPlayerWidth, -ReducedHeight),
        new Vector2(-HalfPlayerWidth, -ReducedHeight),
        new Vector2(+HalfPlayerWidth, 0),
        new Vector2(-HalfPlayerWidth, 0),
    };
    
    private List<Vector2> _sidewardCollisions = new()
    {
        new Vector2(+HalfPlayerWidth, -ReducedHeight),
        new Vector2(-HalfPlayerWidth, -ReducedHeight),
        new Vector2(+HalfPlayerWidth, 0),
        new Vector2(-HalfPlayerWidth, 0),
    };

    public SirPhysics(Colcol colcol)
    {
        _colcol = colcol;
    }
    
    public void ForwardCollisions(ref Vector3 velocity, Vector3 playerPos)
    {
        var direction = Math.Sign(velocity.X);
        float rayLength = Math.Abs(velocity.X) + skinWidth;

        foreach (var verticalRayOrigin in _forwardCollisions)
        {
            var origin = new Vector3(direction == -1 ? -HalfPlayerWidth : +HalfPlayerWidth, verticalRayOrigin.Y + velocity.Y, verticalRayOrigin.X + velocity.Z);
            origin += playerPos;
            var hit = _colcol.Raycast(origin, new Vector3(direction, 0, 0), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.X = (distance - skinWidth) * direction;
                rayLength = distance;
            }
        }
    }
    
    public void SidewardCollisions(ref Vector3 velocity, Vector3 playerPos)
    {
        var direction = Math.Sign(velocity.Z);
        float rayLength = Math.Abs(velocity.Z) + skinWidth;

        foreach (var verticalRayOrigin in _sidewardCollisions)
        {
            var origin = new Vector3(verticalRayOrigin.X + velocity.X, verticalRayOrigin.Y + velocity.Y, direction == -1 ? -HalfPlayerWidth : +HalfPlayerWidth);

            origin += playerPos;
            
            var hit = _colcol.Raycast(origin, new Vector3(0, 0, direction), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.Z = (distance - skinWidth) * direction;
                rayLength = distance;
            }
        }
    }

    public void VerticalCollisions(ref Vector3 velocity, Vector3 playerPos)
    {
        var direction = Math.Sign(velocity.Y);
        float rayLength = Math.Abs(velocity.Y) + skinWidth;

        foreach (var verticalRayOrigin in _verticalRayOrigins)
        {
            var origin = new Vector3(verticalRayOrigin.X + velocity.X, direction == -1 ? -ReducedHeight : 0, verticalRayOrigin.Y + velocity.Z);
            origin += playerPos;
            var hit = _colcol.Raycast(origin, new Vector3(0, direction, 0), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.Y = (distance - skinWidth) * direction;
                rayLength = distance;
            }
        }
    }
}
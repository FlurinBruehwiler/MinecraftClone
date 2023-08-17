using System.Numerics;

namespace RayLib3dTest;

public class SirPhysics
{
    private readonly Colcol _colcol;
    private float skinWidth = 0.1f;

    private List<Vector2> _verticalRayOrigins = new()
    {
        new Vector2(-0.3f, -0.3f),
        new Vector2(-0.3f, +0.3f),
        new Vector2(+0.3f, -0.3f),
        new Vector2(+0.3f, +0.4f),
    };

    private List<Vector2> _forwardCollisions = new()
    {
        new Vector2(+0.4f, -1.9f),
        new Vector2(-0.4f, -1.9f),
        new Vector2(+0.4f, 0),
        new Vector2(-0.4f, 0),
    };
    
    private List<Vector2> _sidewardCollisions = new()
    {
        new Vector2(+0.4f, -1.9f),
        new Vector2(-0.4f, -1.9f),
        new Vector2(+0.4f, 0),
        new Vector2(-0.4f, 0),
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
            var origin = new Vector3(direction == -1 ? -0.4f : +0f, verticalRayOrigin.Y, verticalRayOrigin.X);
            origin += playerPos;
            var hit = _colcol.Raycast(origin, new Vector3(direction, 0, 0), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.X = -(distance - skinWidth);
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
            var origin = new Vector3(verticalRayOrigin.X, verticalRayOrigin.Y, direction == -1 ? -0.4f : +0.4f);

            origin += playerPos;
            
            var hit = _colcol.Raycast(origin, new Vector3(0, 0, direction), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.Z = -(distance - skinWidth);
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
            var origin = new Vector3(verticalRayOrigin.X, direction == -1 ? -2 : 0, verticalRayOrigin.Y);
            origin += playerPos;
            var hit = _colcol.Raycast(origin, new Vector3(0, direction, 0), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.Y = -(distance - skinWidth);
                rayLength = distance;
            }
        }
    }
}
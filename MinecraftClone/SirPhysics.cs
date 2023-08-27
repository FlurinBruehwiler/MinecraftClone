namespace RayLib3dTest;

public class SirPhysics
{
    private readonly Colcol _colcol;
    private const float SkinWidth = 0.015f;
    private const float PlayerHeight = 1.8f;
    private const float PlayerWidth = 0.6f;

    private const float ReducedHeight = PlayerHeight - SkinWidth;
    private const float ReducedWidth = PlayerWidth - SkinWidth;
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

    public void DisplayRays(Vector3 velocity, Vector3 playerPos)
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
    
    public void ForwardCollisions(ref Vector3 velocity, Vector3 playerPos)
    {
        var direction = Math.Sign(velocity.X);
        float rayLength = Math.Abs(velocity.X) + SkinWidth;

        foreach (var verticalRayOrigin in _forwardCollisions)
        {
            var origin = new Vector3(direction == -1 ? -HalfPlayerWidth : +HalfPlayerWidth, verticalRayOrigin.Y + velocity.Y, verticalRayOrigin.X + velocity.Z);
            origin += playerPos;
            var hit = _colcol.Raycast(origin, new Vector3(direction, 0, 0), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.X = (distance - SkinWidth) * direction;
                rayLength = distance;
            }
        }
    }
    
    public void SidewardCollisions(ref Vector3 velocity, Vector3 playerPos)
    {
        var direction = Math.Sign(velocity.Z);
        float rayLength = Math.Abs(velocity.Z) + SkinWidth;

        foreach (var verticalRayOrigin in _sidewardCollisions)
        {
            var origin = new Vector3(verticalRayOrigin.X + velocity.X, verticalRayOrigin.Y + velocity.Y, direction == -1 ? -HalfPlayerWidth : +HalfPlayerWidth);

            origin += playerPos;
            
            var hit = _colcol.Raycast(origin, new Vector3(0, 0, direction), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.Z = (distance - SkinWidth) * direction;
                rayLength = distance;
            }
        }
    }

    public void VerticalCollisions(ref Vector3 velocity, Vector3 playerPos, out bool isHit)
    {
        isHit = false;

        var direction = Math.Sign(velocity.Y);
        float rayLength = Math.Abs(velocity.Y) + SkinWidth;

        foreach (var verticalRayOrigin in _verticalRayOrigins)
        {
            var origin = new Vector3(verticalRayOrigin.X + velocity.X, direction == -1 ? -ReducedHeight : 0, verticalRayOrigin.Y + velocity.Z);
            origin += playerPos;
            var hit = _colcol.Raycast(origin, new Vector3(0, direction, 0), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.Y = (distance - SkinWidth) * direction;
                rayLength = distance;
                isHit = true;
            }
        }
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

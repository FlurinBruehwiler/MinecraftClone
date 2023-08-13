using System.Numerics;

namespace RayLib3dTest;

public class SirPhysics
{
    private readonly Colcol _colcol;
    private float skinWidth = 0.1f;

    private List<Vector3> _verticalRayOrigins = new()
    {
        new Vector3(-0.4f, -2, -0.4f),
        new Vector3(-0.4f, -2, +0.4f),
        new Vector3(+0.4f, -2, -0.4f),
        new Vector3(+0.4f, -2, +0.4f),
    };

    public SirPhysics(Colcol colcol)
    {
        _colcol = colcol;
    }
    
    public void VerticalCollisions(ref Vector3 velocity)
    {
        float rayLength = Math.Abs(velocity.Z) + skinWidth;

        foreach (var verticalRayOrigin in _verticalRayOrigins)
        {
            var origin = verticalRayOrigin;
            var hit = _colcol.Raycast(origin, new Vector3(0, -1, 0), rayLength, out _, out var distance);

            if (hit is not null)
            {
                velocity.Z = -(distance - skinWidth);
                rayLength = distance;
            }
        }
    }
}
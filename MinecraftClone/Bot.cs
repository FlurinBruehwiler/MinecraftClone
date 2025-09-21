namespace RayLib3dTest;

public class Bot
{
    public Vector3 Position = new Vector3(0, 100, 0);
    public Vector3 Direction = new(0, 0, 1);
    public Vector3 Velocity;

    public IntVector3 Target;

    // private const float rotationPerTick = 0;

    public void Tick()
    {
        // var path = Pathfinding.PathFind(Position.ToIntVector3(), Target);
        // if (path.Length != 0)
        {
            // var blockTarget = path[0];

            var xVector = Target.ToVector3() - Position;
            var yVector = new Vector3(-xVector.Y, 0, xVector.X);

            Direction = new Vector3(Vector3.Dot(Vector3.Normalize(xVector), new Vector3(1, 0, 0)), 0,
                Vector3.Dot(Vector3.Normalize(yVector), new Vector3(0, 0, 1)));
        }
    }

    public void Render()
    {
        DrawCubeV(Position, Vector3.One, Color.BLUE);
    }
}
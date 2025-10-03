using System.Text.Json;

namespace RayLib3dTest;

public class Bot
{
    public Vector3 Position = new Vector3(0, 100, 0);
    public Vector3 LastPosition;
    public Vector3 Direction = new(1, 0, 0);
    public Vector3 Velocity;

    public IntVector3? Target;
    public JemFile Model;

    // private const float rotationPerTick = 0;

    public void Tick()
    {
        LastPosition = Position;

        if (Target.HasValue)
        {
            Target = FindGround(Target.Value);
        }

        var lowerPos = (Position + new Vector3(0, -1.5f, 0));

        // DevTools.Print(Vector3.Distance(lowerPos, Target.ToVector3()), "Distance");

        var movement = new Vector3(0, 0, 0);
        if (Target.HasValue && Vector3.Distance(lowerPos, Target.Value.ToVector3()) > 0.5f)
        {
            var path = Pathfinding.PathFind( lowerPos.ToIntVector3(), Target.Value);
            Pathfinding.Visualize(path);
            if (path.Length > 0)
            {
                var targetBlock = path[0];

                var xVector = targetBlock.ToVector3() - Position;
                var v = Vector2.Normalize(new Vector2(xVector.X, xVector.Z));
                Direction = new Vector3(v.X, 0, v.Y);
                movement = new Vector3(0, 0, -1);
            }
        }

        if (Direction.LengthSquared() == 0)
            Direction = new Vector3(0, 0, -1);
        var colInfo = Movement.Move(ref Velocity, ref Position, Direction, movement,
            GetHitBox(), shouldJump, MovementConfig.Default);

        DevTools.Print(JsonSerializer.Serialize(colInfo, new JsonSerializerOptions
        {
            IncludeFields = true
        }) , "collision");
        DevTools.Print(Direction, "mob_direction");

        shouldJump = false;
        if (Direction.Z > 0 && colInfo.Right)
            shouldJump = true;
        if (Direction.Z < 0 && colInfo.Left)
            shouldJump = true;
        if (Direction.X > 0 && colInfo.Forward)
            shouldJump = true;
        if (Direction.X < 0 && colInfo.Backwards)
            shouldJump = true;
    }

    private IntVector3 FindGround(IntVector3 pos)
    {
        while (true)
        {
            if (CurrentWorld.IsSolid(pos))
            {
                return pos + new IntVector3(0, 1, 0);
            }

            pos.Y--;
            if (pos.Y < 0)
                return pos;
        }
    }

    private bool shouldJump;

    public Hitbox GetHitBox()
    {
        var halfWidth = Physics.PlayerWidth / 2f;
        return new Hitbox(new Vector3(-halfWidth, -Physics.PlayerHeight, -halfWidth),
            new Vector3(halfWidth, 0, halfWidth));
    }

    public void Render()
    {
        var t = 1 / Game.TickRateMs * Game.MsSinceLastTick();

        var cameraPos = Vector3.Lerp(LastPosition, Position, t);

        // DrawHitBox(cameraPos, GetHitBox());

        Models.RenderModel(Model, cameraPos, Direction, GetHitBox());

        if (DevTools.DevToolsEnabled)
        {
            Raylib.DrawLine3D(cameraPos, cameraPos + -Direction.Forward() * 2, Color.Red);
            if (Target.HasValue)
            {
                Raylib.DrawLine3D(cameraPos, Target.Value.ToVector3(), Color.Blue);
            }
        }
    }

    public void DrawHitBox(Vector3 position, Hitbox hitbox)
    {
        Raylib.DrawCubeV(position + hitbox.GetCenter(), hitbox.GetSize(), Color.Blue);
    }
}
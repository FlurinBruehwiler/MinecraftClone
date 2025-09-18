namespace RayLib3dTest;

public class CameraManager
{
    public Camera3D Camera;
    private float _sensitivity = 1;
    public Player Player;

    public CameraManager(Player player)
    {
        Player = player;
        Camera = new Camera3D(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0), 100,
            CameraProjection.CAMERA_PERSPECTIVE);
    }

    public void Update()
    {
        HandleInput(Player);
        UpdateCamera(Player);
        Chunkloader.LoadChunksIfNeccesary(Player.Position);
    }

    private unsafe void UpdateCamera(Player controlable)
    {
        // var posChangeInWorldSpace = controlable.Position - Camera.position;
        //
        // _debuggerus.Print(Camera.GetLeft(), "camera right");
        // _debuggerus.Print(Camera.GetForward(), "camera forward");
        // _debuggerus.Print(Camera.GetUp(), "camera up");
        //
        // var cameraLeft = Vector3.Normalize(new Vector3(Camera.GetLeft().X, 0, Camera.GetLeft().Z));
        // var cameraForward = Vector3.Normalize(new Vector3(-Camera.GetLeft().Z, 0, Camera.GetLeft().X));
        //
        // var localX = Vector3.Dot(cameraLeft, posChangeInWorldSpace);
        // var localZ = Vector3.Dot(cameraForward, posChangeInWorldSpace);
        //
        // var newlocalMoveDelta = new Vector3(localX, posChangeInWorldSpace.Y, localZ);
        //
        // var finalMoveDelta = newlocalMoveDelta * _sens * GetFrameTime();
        // //
        // _debuggerus.Print(posChangeInWorldSpace, "world movements");
        // _debuggerus.Print(controlable.Position, "player pos");
        // _debuggerus.Print(Camera.position, "camera pos");
        //
        // _debuggerus.Print(controlable.Position, "Player Pos");

        Camera.position = controlable.Position + Player.CameraOffset;
        Camera.target = controlable.Position + Player.CameraOffset + controlable.Direction;

        // fixed (Camera3D* c = &Camera)
        // {
        //     UpdateCameraPro(c, new Vector3(finalMoveDelta.Z, -finalMoveDelta.X, finalMoveDelta.Y),   new Vector3(GetMouseDelta() * 0.2f, 0), 0);
        // }
    }

    private void HandleInput(Player player)
    {
        // HandleSpeedChange();

        var rotationInput = GetMouseDelta() * 0.2f;
        var rotationVector = new Vector3(-rotationInput.X * DEG2RAD, rotationInput.Y * DEG2RAD, 0);

        player.yaw   += rotationInput.X * DEG2RAD;
        player.pitch += -rotationInput.Y * DEG2RAD;
        player.pitch = Math.Clamp(player.pitch, -MathF.PI/2 + 0.001f, MathF.PI/2 - 0.001f);

        var cosPitch = MathF.Cos(player.pitch);
        player.Direction = new Vector3(
            MathF.Cos(player.yaw) * cosPitch,
            MathF.Sin(player.pitch),
            MathF.Sin(player.yaw) * cosPitch
        );

        // player.Direction = Vector3RotateByAxisAngle(player.Direction, -player.Right, rotationVector.Y);
        // player.Direction = Vector3RotateByAxisAngle(player.Direction, new Vector3(0, 1, 0), rotationVector.X);

        // var right = Vector3.Normalize(Vector3.Cross(new Vector3(0,1,0), player.Direction));


        // player.Direction = player.Direction with { Y = Math.Clamp(player.Direction.Y, -0.99f, +0.99f) };


        var moveDelta = GetMovementInput();

        var right = Vector3.Normalize(new Vector3(player.Right.X, 0, player.Right.Z));
        var forward = Vector3.Normalize(new Vector3(-player.Right.Z, 0, player.Right.X));

        var xComponent = right * moveDelta.X;
        var zComponent = forward * moveDelta.Z;

        var globalMoveDelta = xComponent + zComponent;
        globalMoveDelta.Y = moveDelta.Y;

        player.Move(globalMoveDelta);

        DevTools.Print(Player.Position, "Player_Pos");
        DevTools.Print(Player.Direction, "Player_Direction");
        DevTools.Print(GetChunkCoordinate(Player.Position.ToIntVector3()), "Chunk");

        var col = Physics.Raycast(Player.Position + Player.CameraOffset, Player.Direction, 10, out _, out _, true);
        DevTools.Print(col, "Looking at Block");

        DevTools.Print(GetFPS(), "FPS");
    }

    // private void HandleSpeedChange()
    // {
    //     if (GetMouseWheelMoveV().Y > 0)
    //     {
    //         _playerSpeed *= 1.1f;
    //     }
    //     else if (GetMouseWheelMoveV().Y < 0)
    //     {
    //         _playerSpeed *= 0.9f;
    //     }
    //
    //     _playerSpeed = Math.Max(_playerSpeed, 0);
    // }

    private Vector3 GetMovementInput()
    {
        var inputDirection = new Vector3();

        if (IsKeyDown(KeyboardKey.KEY_W))
        {
            inputDirection.Z -= 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_S))
        {
            inputDirection.Z += 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_D))
        {
            inputDirection.X += 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_A))
        {
            inputDirection.X -= 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_SPACE))
        {
            inputDirection.Y += 1;
        }

        if (IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
        {
            inputDirection.Y -= 1;
        }

        return inputDirection;
    }
}

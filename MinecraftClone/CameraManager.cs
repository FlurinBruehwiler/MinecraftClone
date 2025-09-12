namespace RayLib3dTest;

public class CameraManager
{
    public Camera3D Camera;
    private float _sensitivity = 1;
    private Player _player;
    private float _playerSpeed = 0.1f;

    public CameraManager(Player player)
    {
        _player = player;
        Camera = new Camera3D(Vector3.Zero, new Vector3(0, 0, 1), new Vector3(0, 1, 0), 100,
            CameraProjection.CAMERA_PERSPECTIVE);
    }

    public void Update()
    {
        HandleInput(_player);
        UpdateCamera(_player);
        Chunkloader.LoadChunksIfNeccesary(_player.Position);
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

        Camera.position = controlable.Position;
        Camera.target = controlable.Position + controlable.Direction;

        // fixed (Camera3D* c = &Camera)
        // {
        //     UpdateCameraPro(c, new Vector3(finalMoveDelta.Z, -finalMoveDelta.X, finalMoveDelta.Y),   new Vector3(GetMouseDelta() * 0.2f, 0), 0);
        // }
    }

    private void HandleInput(Player controlable)
    {
        HandleSpeedChange();

        var rotationInput = GetMouseDelta() * 0.2f;
        var rotationVector = new Vector3(-rotationInput.X * DEG2RAD, rotationInput.Y * DEG2RAD, 0);

        controlable.Direction = Vector3RotateByAxisAngle(controlable.Direction, -controlable.Right, rotationVector.Y);
        controlable.Direction = Vector3RotateByAxisAngle(controlable.Direction, new Vector3(0, 1, 0), rotationVector.X);


        var moveDelta = GetMovementDelta();

        DevTools.Print(moveDelta, "Input");

        var right = Vector3.Normalize(new Vector3(controlable.Right.X, 0, controlable.Right.Z));
        var forward = Vector3.Normalize(new Vector3(-controlable.Right.Z, 0, controlable.Right.X));

        DevTools.Print(right, "right");
        DevTools.Print(forward, "forward");

        var xComponent = right * moveDelta.X;
        var zComponent = forward * moveDelta.Z;

        var globalMoveDelta = xComponent + zComponent;
        globalMoveDelta.Y = moveDelta.Y;

        controlable.Move(globalMoveDelta);

        DevTools.Print(_player.Position, "Player_Pos");
    }

    private void HandleSpeedChange()
    {
        if (GetMouseWheelMoveV().Y > 0)
        {
            _playerSpeed *= 1.1f;
        }
        else if (GetMouseWheelMoveV().Y < 0)
        {
            _playerSpeed *= 0.9f;
        }

        _playerSpeed = Math.Max(_playerSpeed, 0);
    }

    private Vector3 GetMovementDelta()
    {
        var moveDelta = new Vector3();

        if (IsKeyDown(KeyboardKey.KEY_W))
        {
            moveDelta.Z -= _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_S))
        {
            moveDelta.Z += _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_D))
        {
            moveDelta.X += _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_A))
        {
            moveDelta.X -= _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_SPACE))
        {
            moveDelta.Y += _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_LEFT_CONTROL))
        {
            moveDelta.Y -= _playerSpeed;
        }

        return moveDelta;
    }
}

namespace RayLib3dTest;

public class CameraManager : IServus
{
    public Camera3D Camera;
    private float _sens = 60;
    private Player _player;
    private readonly Chunker _chunker;
    private float _playerSpeed = 1f;

    public CameraManager(Player player, Chunker chunker)
    {
        _player = player;
        _chunker = chunker;
        Camera = new Camera3D(Vector3.Zero, Vector3.One, new Vector3(0, 1, 0), 60, CameraProjection.CAMERA_PERSPECTIVE)
        {
            fovy = 100
        };
    }

    public void Update()
    {
        HandleInput(_player);
        UpdateCamera(_player);
        _chunker.LoadChunksIfNeccesary(_player.Position);
    }

    private void UpdateCamera(IControlable controlable)
    {
        var posChangeInWorldSpace = controlable.Position - Camera.position;

        var cameraRight = Vector3.Normalize(new Vector3(Camera.GetRight().X, 0, Camera.GetRight().Z));
        var cameraForward = Vector3.Normalize(new Vector3(-Camera.GetRight().Z, 0, Camera.GetRight().X));

        var localX = Vector3.Dot(cameraRight, posChangeInWorldSpace);
        var localZ = Vector3.Dot(cameraForward, posChangeInWorldSpace);

        var newlocalMoveDelta = new Vector3(localX, posChangeInWorldSpace.Y, localZ);

        var camera = Camera;
        UpdateCameraPro(ref camera,
            new Vector3(newlocalMoveDelta.X, newlocalMoveDelta.Z, newlocalMoveDelta.Y) * _sens * GetFrameTime(),
            new Vector3(GetMouseDelta() * 0.5f, 0), 0);
    }

    private void HandleInput(IControlable controlable)
    {
        HandleSpeedChange();
        
        var moveDelta = GetMovementDelta();
        
        var right = Vector3.Normalize(new Vector3(controlable.Right.X, 0, controlable.Right.Z));
        var forward = Vector3.Normalize(new Vector3(-controlable.Right.Z, 0, controlable.Right.X));

        var xComponent = right * moveDelta.X;
        var zComponent = forward * moveDelta.Z;
    
        var globalMoveDelta = xComponent + zComponent;
        globalMoveDelta.Y = moveDelta.Y;
        
        controlable.Move(globalMoveDelta);
    }

    private void HandleSpeedChange()
    {
        if (GetMouseWheelMoveV().Y > 0)
        {
            _playerSpeed *= 1.1f;
        }else if (GetMouseWheelMoveV().Y < 0)
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
            moveDelta.X += _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_S))
        {
            moveDelta.X -= _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_D))
        {
            moveDelta.Z += _playerSpeed;
        }

        if (IsKeyDown(KeyboardKey.KEY_A))
        {
            moveDelta.Z -= _playerSpeed;
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